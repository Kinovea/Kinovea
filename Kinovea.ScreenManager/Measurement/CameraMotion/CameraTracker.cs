using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Kinovea.Video;
using Kinovea.Services;
using System.IO;
using System.Globalization;
using System.ComponentModel;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This class performs the steps to compute global camera motion and stores the results.
    /// It calls OpenCV functions and exposes the raw result.
    /// The result should then be sent to `CameraTransformer` which exposes functions 
    /// to transform coordinates from one frame to another.
    /// </summary>
    public class CameraTracker : IDisposable
    {
        #region Properties

        /// <summary>
        /// True if the whole process is completed.
        /// </summary>
        public bool Tracked { get { return tracked; } }

        /// <summary>
        /// The parameters used for this camera motion process.
        /// </summary>
        public CameraMotionParameters Parameters { get {return parameters;} }

        /// <summary>
        /// Reverse mapping going from timestamps to frames indices.
        /// </summary>
        public Dictionary<long, int> FrameIndices { get { return frameIndices; } }

        /// <summary>
        /// Consecutive transforms going from frame i to frame i+1.
        /// </summary>
        public List<OpenCvSharp.Mat> ConsecutiveTransforms { get { return consecTransforms; } }

        /// <summary>
        /// Camera parameters for each frame.
        /// Includes intrinsic parameters and rotation matrix.
        /// </summary>
        public List<OpenCvSharp.Detail.CameraParams> CameraParams { get { return cameraParams; } }
        #endregion

        #region Members

        private Bitmap mask;
        private bool tracked = false;

        // Computed data
        // frameIndices: reverse index from timestamps to frames indices.
        // keypoints: arrays of features found on each image.
        // descriptors: feature descriptors for the keypoints.
        // matches: associations between keypoints in different images.
        // inlier status: whether a match at the corresponding index is an inlier or outlier.
        // inliers: list of inlier matches.
        // consecTransforms: transform matrices going from frame i to i+1.
        // tracks: timelines of features followed over multiple frames.
        private Dictionary<long, int> frameIndices = new Dictionary<long, int>();
        private List<long> timestamps = new List<long>();
        private List<OpenCvSharp.KeyPoint[]> keypoints = new List<OpenCvSharp.KeyPoint[]>();
        private List<OpenCvSharp.Mat> descriptors = new List<OpenCvSharp.Mat>();
        private List<OpenCvSharp.DMatch[]> matches = new List<OpenCvSharp.DMatch[]>();
        private List<List<bool>> inlierStatus = new List<List<bool>>();
        private List<List<PointF>> inliers = new List<List<PointF>>();
        private List<OpenCvSharp.Mat> consecTransforms = new List<OpenCvSharp.Mat>();
        private List<SortedDictionary<long, PointF>> tracks = new List<SortedDictionary<long, PointF>>();
        private List<List<byte>> inliersMasks = new List<List<byte>>();
        private List<OpenCvSharp.Detail.CameraParams> cameraParams = new List<OpenCvSharp.Detail.CameraParams>();

        // The following are used when we import transforms from COLMAP.
        private List<DistortionParameters> intrinsics = new List<DistortionParameters>();
        private List<double[,]> extrinsics = new List<double[,]>();

        // Core parameters
        private CameraMotionParameters parameters;

        private Stopwatch stopwatch = new Stopwatch();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region ctor/dtor
        public CameraTracker(CameraMotionParameters parameters)
        {
            this.parameters = parameters;
        }

        ~CameraTracker()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases all resources used by this camera tracker.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (descriptors != null && descriptors.Count > 0)
                {
                    foreach (var desc in descriptors)
                        desc.Dispose();

                }

                if (mask != null)
                    mask.Dispose();

                ClearCameraParams();
                // TODO: dispose all other native resources.

            }
        }
        #endregion

        #region Camera motion estimation process steps

        /// <summary>
        /// Find features in all frames.
        /// </summary>
        public void FindFeatures(IWorkingZoneFramesContainer framesContainer, BackgroundWorker worker)
        {
            // Find and describe features on each frame.
            // Fills the following global variables:
            // - frameIndices: dictionary of timestamps to indices in the frame list.
            // - timestamps: frame timestamps.
            // - keypoints: list of found features.
            // - descriptors: descriptors for each feature.

            // Parameters:
            // - FeaturesPerFrame: the number of features has an influence on the quality of the result.
            // In "Image Matching across Wide Baselines: From Paper to Practice",  2K features is considered
            // low budget and 8K features is high budget.
            // - Type of features (SIFT, ORB, AKAZE, etc.)

            // TODO: try to run this through a parallel-for if possible.
            // Do we need frameIndices, keypoints and descriptors to be sequential?
            // If so, prepare all the tables without the detection and then run the detection in parallel.

            stopwatch.Restart();
            frameIndices.Clear();
            timestamps.Clear();
            keypoints.Clear();
            descriptors.Clear();

            // Import and convert the mask if needed.
            // This is a single shared mask for all frames, it is used to mask out static overlays like logos.
            OpenCvSharp.Mat cvMaskGray = null;
            bool hasMask = mask != null;
            if (hasMask)
            {
                var cvMask = mask == null ? null : OpenCvSharp.Extensions.BitmapConverter.ToMat(mask);
                cvMaskGray = new OpenCvSharp.Mat();
                OpenCvSharp.Cv2.CvtColor(cvMask, cvMaskGray, OpenCvSharp.ColorConversionCodes.BGR2GRAY, 0);
                cvMask.Dispose();
                log.DebugFormat("Imported mask. {0} ms.", stopwatch.ElapsedMilliseconds);
            }

            OpenCvSharp.ORB orb = null;
            OpenCvSharp.Features2D.SIFT sift = null;

            int featuresPerFrame = Math.Max(100, parameters.FeaturesPerFrame);

            if (parameters.FeatureType == CameraMotionFeatureType.ORB)
                orb = OpenCvSharp.ORB.Create(featuresPerFrame);
            else
                sift = OpenCvSharp.Features2D.SIFT.Create(featuresPerFrame);

            int frameIndex = 0;
            for (int i = 0; i < framesContainer.Frames.Count; i++)
            {
                if (worker.CancellationPending)
                    break;

                var f = framesContainer.Frames[i];
                if (frameIndices.ContainsKey(f.Timestamp))
                    continue;

                frameIndices.Add(f.Timestamp, frameIndex);
                timestamps.Add(f.Timestamp);

                // Convert image to OpenCV and convert to grayscale.
                var cvImage = OpenCvSharp.Extensions.BitmapConverter.ToMat(f.Image);
                var cvImageGray = new OpenCvSharp.Mat();
                OpenCvSharp.Cv2.CvtColor(cvImage, cvImageGray, OpenCvSharp.ColorConversionCodes.BGR2GRAY, 0);
                cvImage.Dispose();

                // Feature detection & description.
                var desc = new OpenCvSharp.Mat();
                OpenCvSharp.KeyPoint[] kp;
                if (parameters.FeatureType == CameraMotionFeatureType.ORB)
                    orb.DetectAndCompute(cvImageGray, cvMaskGray, out kp, desc);
                else
                    sift.DetectAndCompute(cvImageGray, cvMaskGray, out kp, desc);

                keypoints.Add(kp);
                descriptors.Add(desc);

                cvImageGray.Dispose();

                //log.DebugFormat("Feature detection - Frame [{0}]: {1} features.", keypoints.Count, keypoints[keypoints.Count - 1].Length);
                frameIndex++;
                worker.ReportProgress(i + 1, framesContainer.Frames.Count);
            }

            if (parameters.FeatureType == CameraMotionFeatureType.ORB)
                orb.Dispose();
             else
                sift.Dispose();

            if (hasMask)
                cvMaskGray.Dispose();

            log.DebugFormat("Feature detection: {0} ms.", stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Match features from frame to frame.
        /// </summary>
        public void MatchFeatures(IWorkingZoneFramesContainer framesContainer, BackgroundWorker worker)
        {
            // Matching strategy and outlier pre-filtering
            // References:
            // - paragraph 4.2 of "Image Matching across Wide Baselines: From Paper to Practice".
            // - paragraph 7.1 of "Distinctive Image Features from scale invariant key points", Lowe, 2004.
            //
            // Matcher type: Brute force (exact) or FLANN based.
            // -> We always use the BF matcher.
            //
            // There are 3 basic strategies:
            // - unidirectional: match features from i to i+1.
            // - bidirectional + "both": match features from i to i+1 and from i+1 to i and keep the intersection.
            // - bidirectional + "either": same but keep the union of all matches.
            //
            // Using crossCheck = true in the BFMatcher constructor corresponds to strategy "both": it will only
            // return pairs where both features have the other one as their nearest neighbor.
            //
            // Lowe's ratio test.
            // -> Only keep matches where the nearest neighbor is much closer than the second nearest neighbor.
            // Otherwise the match is not very discriminatory and it's likely to be a false positive.
            bool crossCheck = !parameters.UseDistanceRatioTest;
            float r = 0.8f;

            if (descriptors.Count == 0)
                return;
            
            stopwatch.Restart();
            matches.Clear();
            List<OpenCvSharp.DMatch[]> framesMatches = new List<OpenCvSharp.DMatch[]>();

            // Matching distance: SIFT requires L1 norm.
            OpenCvSharp.BFMatcher matcher = null;
            if (parameters.FeatureType == CameraMotionFeatureType.ORB)
                matcher = new OpenCvSharp.BFMatcher(OpenCvSharp.NormTypes.Hamming, crossCheck: crossCheck);
            else
                matcher = new OpenCvSharp.BFMatcher(OpenCvSharp.NormTypes.L1, crossCheck: crossCheck);

            for (int i = 0; i < descriptors.Count - 1; i++)
            {
                if (worker.CancellationPending)
                    break;

                if (parameters.UseDistanceRatioTest)
                {
                    var mm = matcher.KnnMatch(descriptors[i], descriptors[i + 1], 2);

                    // Lowe's ratio test.
                    List<OpenCvSharp.DMatch> keepers = new List<OpenCvSharp.DMatch>();
                    foreach (var matches in mm)
                    {
                        if (matches.Count() < 2)
                            continue;

                        // Accept the match only if the nearest neighbor is much closer than
                        // the second nearest neighbor.
                        if (matches[0].Distance < (1.0f - r) * matches[1].Distance)
                        {
                            keepers.Add(matches[0]);
                        }
                    }

                    framesMatches.Add(keepers.ToArray());
                }
                else
                {
                    OpenCvSharp.DMatch[] mm = matcher.Match(descriptors[i], descriptors[i + 1]);
                    framesMatches.Add(mm);
                }

                worker.ReportProgress(i + 1, descriptors.Count - 1);
            }

            matcher.Dispose();

            if (parameters.UseDistanceThreshold)
            {
                // We know we are tracking a video frame by frame so we can assume the motion vectors to be
                // relatively small. Any match over a distance threshold can be eliminated right away.
                // This will make the job of RANSAC easier.
                Size imageSize = framesContainer.Frames[0].Image.Size;
                float distanceThreshold = imageSize.Width * parameters.DistanceThresholdNormalized;
                for (int i = 0; i < descriptors.Count - 1; i++)
                {
                    var srcPoints = framesMatches[i].Select(m => new OpenCvSharp.Point2d(keypoints[i][m.QueryIdx].Pt.X, keypoints[i][m.QueryIdx].Pt.Y)).ToList();
                    var dstPoints = framesMatches[i].Select(m => new OpenCvSharp.Point2d(keypoints[i + 1][m.TrainIdx].Pt.X, keypoints[i + 1][m.TrainIdx].Pt.Y)).ToList();
                    var keepers = framesMatches[i].Where((match, index) => OpenCvSharp.Point2d.Distance(srcPoints[index], dstPoints[index]) < distanceThreshold);
                    matches.Add(keepers.ToArray());
                }
            }
            else
            {
                matches = framesMatches;
            }

            log.DebugFormat("Feature matching: {0} ms.", stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Find homographies transforming the image plane between consecutive frames.
        /// </summary>
        public void FindHomographies(IWorkingZoneFramesContainer framesContainer, BackgroundWorker worker)
        {
            // Ref: Chapter 5.1 and 5.2 of "Image Matching across Wide Baselines: From Paper to Practice".
            // Parameters:
            // - tau: confidence level in the estimate
            // - eta: the outlier threshold. 
            // - gamma: the maximum number of iterations.
            // The paper reports optimal values of eta=1.25 px, gamma=10K iterations.
            if (matches.Count == 0)
                return;

            double ransacReprojThreshold = 1.25;
            int maxIter = 2000;
            double confidence = 0.995;
            
            stopwatch.Restart();
            inliers.Clear();
            inlierStatus.Clear();
            consecTransforms.Clear();

            for (int i = 0; i < descriptors.Count - 1; i++)
            {
                if (worker.CancellationPending)
                    break;

                var mm = matches[i];
                if (mm.Length < 4)
                {
                    // We can't compute the homography.
                    // TODO: Initialize with identity and continue.
                    log.ErrorFormat("Not enough matches on frame {0}.", i);
                    break;
                }

                var srcPoints = mm.Select(m => new OpenCvSharp.Point2d(keypoints[i][m.QueryIdx].Pt.X, keypoints[i][m.QueryIdx].Pt.Y));
                var dstPoints = mm.Select(m => new OpenCvSharp.Point2d(keypoints[i + 1][m.TrainIdx].Pt.X, keypoints[i + 1][m.TrainIdx].Pt.Y));
                OpenCvSharp.HomographyMethods method = OpenCvSharp.HomographyMethods.USAC_MAGSAC;
                var mask = new List<byte>();
                var cvMask = OpenCvSharp.OutputArray.Create(mask);

                var homography = OpenCvSharp.Cv2.FindHomography(
                    srcPoints,
                    dstPoints,
                    method,
                    ransacReprojThreshold,
                    cvMask,
                    maxIter,
                    confidence);

                inliersMasks.Add(mask);

                // Collect inliers in more usable formats: a list of bools and a list of only inlier points.
                inlierStatus.Add(new List<bool>());
                inliers.Add(new List<PointF>());
                for (int j = 0; j < mask.Count; j++)
                {
                    bool inlier = mask[j] != 0;
                    inlierStatus[i].Add(inlier);

                    if (inlier)
                    {
                        PointF p = new PointF(keypoints[i][mm[j].QueryIdx].Pt.X, keypoints[i][mm[j].QueryIdx].Pt.Y);
                        inliers[i].Add(p);
                    }
                }

                consecTransforms.Add(homography);
                worker.ReportProgress(i + 1, descriptors.Count - 1);
            }

            log.DebugFormat("Transforms computation: {0} ms.", stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Rotation estimation and bundle adjustment.
        /// </summary>
        public void BundleAdjustment(IWorkingZoneFramesContainer framesContainer, BackgroundWorker worker)
        {
            if (matches.Count != inlierStatus.Count)
            {
                // This indicates that there was an issue during FindHomographies
                // and we don't have the required information to do bundle adjustment.
                return;
            }

            // Disable the whole rotation estimation and bundle adjustment for now.
            // The interface with OpenCV is not working as expected.
            tracked = true;
            return;


            //------------------------------------------------------------
            // High level steps:
            // - convert data to be used by OpenCV stitching module low level API.
            // - rough estimate of rotation with a first Homography based estimator.
            // - refine estimate of rotation with bundle adjustment.

            //OpenCvSharp.Size imageSize = new OpenCvSharp.Size(
            //    framesContainer.Frames[0].Image.Size.Width,
            //    framesContainer.Frames[0].Image.Size.Height
            //);

            //// Convert keypoints and descriptors to ImageFeatures.
            //// ImageFeatures: https://docs.opencv.org/4.9.0/d4/db5/structcv_1_1detail_1_1ImageFeatures.html
            //List<OpenCvSharp.Detail.ImageFeatures> features = new List<OpenCvSharp.Detail.ImageFeatures>();
            //for (int i = 0; i < keypoints.Count; i++)
            //{
            //    OpenCvSharp.Detail.ImageFeatures f = new OpenCvSharp.Detail.ImageFeatures(i, imageSize, keypoints[i], descriptors[i]);
            //    features.Add(f);
            //}

            //// Convert matches between consecutive frames and the calculated homographies to MatchesInfo.
            //// MatchesInfo: https://docs.opencv.org/4.9.0/d2/d9a/structcv_1_1detail_1_1MatchesInfo.html
            ////
            //// Array of pairwise matches in OpenCV:
            //// The code in opencv stitching module is made for panoramas and assumes we have matches for 
            //// all possible pairs of images, not just consecutives frames.
            //// It is expecting a 2D array of matches flattened into a 1D array, where missing matches
            //// have their homography set to empty. (See motion_estimator.cpp and autocalib.cpp).
            //int numMatchesInfo = keypoints.Count * keypoints.Count;
            //List<OpenCvSharp.Detail.MatchesInfo> matchesInfo = new List<OpenCvSharp.Detail.MatchesInfo>(numMatchesInfo);
            //for (int i = 0; i < keypoints.Count; i++)
            //{
            //    for (int j = 0; j < keypoints.Count; j++)
            //    {
            //        int index = i * keypoints.Count + j;

            //        int srcImgIdx = i;
            //        int dstImgIdx = j;

            //        if (j == i+1)
            //        {
            //            // Match between i and i+1.
            //            // Use the data from the MatchFeatures and FindHomographies steps.
            //            var mm = matches[i];                            // Matches.
            //            var inliersMask = inliersMasks[i].ToArray();    // Geometrically consistent matches mask.
            //            var numInliers = inliers[i].Count;              // Number of geometrically consistent matches.
            //            OpenCvSharp.Mat H = consecTransforms[i];        // Estimated transformation
            //            double confidence = 1.0;                        // Confidence two images are from the same panorama

            //            OpenCvSharp.Detail.MatchesInfo m = new OpenCvSharp.Detail.MatchesInfo(
            //                srcImgIdx, dstImgIdx, mm, inliersMask, numInliers, H, confidence);

            //            matchesInfo.Add(m);
            //        }
            //        else if (j == i-1)
            //        {
            //            // Match between i and i-1.
            //            // OpenCV will expect this match to exist when it creates the graph of image pairs.
            //            // Use the data we have but use the previous frame as the reference.
            //            var mm = matches[i-1];
            //            var inliersMask = inliersMasks[i-1].ToArray();
            //            var numInliers = inliers[i-1].Count;  
            //            OpenCvSharp.Mat H = consecTransforms[i-1].Inv();
            //            double confidence = 1.0;

            //            OpenCvSharp.Detail.MatchesInfo m = new OpenCvSharp.Detail.MatchesInfo(
            //                srcImgIdx, dstImgIdx, mm, inliersMask, numInliers, H, confidence);

            //            matchesInfo.Add(m);
            //        }
            //        else
            //        {
            //            // Not a match between consecutive frames.
            //            // Create a dummy match info with empty homography and zero confidence.
            //            // Both these members are used in opencv to identify missing data between the two frames.
            //            var mm = new OpenCvSharp.DMatch[0];
            //            var inliersMask = new byte[0];
            //            var numInliers = 0;
            //            OpenCvSharp.Mat H = new OpenCvSharp.Mat();
            //            double confidence = 0.0;

            //            OpenCvSharp.Detail.MatchesInfo m = new OpenCvSharp.Detail.MatchesInfo(
            //                srcImgIdx, dstImgIdx, mm, inliersMask, numInliers, H, confidence);

            //            matchesInfo.Add(m);
            //        }
            //    }
            //}

            ////------------------------------------------------------------
            //// Prepare the list of cameras which will contain the result.
            //// CameraParams: https://docs.opencv.org/4.9.0/d4/d0a/structcv_1_1detail_1_1CameraParams.html
            //// This is always an in/out parameter.
            ////
            //// Use an arbitrary value for focal length, do not use opencv autocalib algorithm.
            //// This is based on the fallback implemented in autocalib.cpp > estimateFocal().
            //// When running through opencv autocalib estimateFocal() it finds wildly different focal lengths
            //// for each frame even if the video doesn't change zoom, and in the end it doesn't work 
            //// because not all homographies are suitable for its algorithm but it requires that there
            //// be at least as many focals calculated as source frames.
            //// So in the end it still computes the focal from the fallback algorithm anyway.
            //// We precompute it here and avoid all the extra work.
            //double focal = imageSize.Width + imageSize.Height;  // <- fallback focal length estimation.
            //double aspect = (double)imageSize.Width / imageSize.Height;
            //double ppx = imageSize.Width / 2.0f;
            //double ppy = imageSize.Height / 2.0f;

            //List<OpenCvSharp.Detail.CameraParams> cameras = new List<OpenCvSharp.Detail.CameraParams>();
            //for (int i = 0; i < keypoints.Count; i++)
            //{
            //    // Initialize the rotation matrix to identity.
            //    // This is important because one of the frames is going to be picked as the reference by 
            //    // opencv (middle frame) and it will use its rotation matrix without setting it itself.
            //    OpenCvSharp.Mat r = OpenCvSharp.Mat.Eye(3, 3, OpenCvSharp.MatType.CV_64FC1);
            //    OpenCvSharp.Mat t = new OpenCvSharp.Mat();
            //    OpenCvSharp.Detail.CameraParams camera = new OpenCvSharp.Detail.CameraParams(focal, aspect, ppx, ppy, r, t);
            //    cameras.Add(camera);
            //}

            //// Perform the first rough estimation pass, based on fake focal length and homographies.
            //bool isFocalsEstimated = true;
            //OpenCvSharp.Detail.HomographyBasedEstimator estimator = new OpenCvSharp.Detail.HomographyBasedEstimator(isFocalsEstimated);
            //bool estimated = estimator.Apply(features, matchesInfo, cameras);
            //if (!estimated)
            //{
            //    log.ErrorFormat("Failure during rotation estimation - Homography based estimator.");
            //    return;
            //}

            //// At this point we have a rotation matrix for each frame going to the reference frame,
            //// but it is poor quality and not as good as the homography matrices.

            //// Convert the rotation matrices from double to float for Bundle adjustment.
            //// See: opencv > Stitcher::estimateCameraParams.
            //for (int i = 0; i < cameras.Count; i++)
            //{
            //    OpenCvSharp.Mat r = new OpenCvSharp.Mat();
            //    cameras[i].R.ConvertTo(r, OpenCvSharp.MatType.CV_32F);
            //    OpenCvSharp.Detail.CameraParams camera = new OpenCvSharp.Detail.CameraParams(
            //        cameras[i].Focal,
            //        aspect,
            //        cameras[i].PpX,
            //        cameras[i].PpY,
            //        r,
            //        new OpenCvSharp.Mat());
            //    cameras[i] = camera;
            //}

            //// Perform the bundle adjustment pass that refines the rotation matrices and focal lengths
            //// of all cameras at once.
            //OpenCvSharp.Detail.BundleAdjusterRay adjuster = new OpenCvSharp.Detail.BundleAdjusterRay();
            //estimated = adjuster.Apply(features, matchesInfo, cameras);
            //if (!estimated)
            //{
            //    log.ErrorFormat("Failure during rotation estimation - Bundle adjustment.");
            //    return;
            //}

            //// Make a deep copy of the calculated camera params.
            //// The underlying native resources get disposed at next GC for some reason.
            //ClearCameraParams();
            //for (int i = 0; i < cameras.Count; i++)
            //{
            //    OpenCvSharp.Mat r = new OpenCvSharp.Mat();
            //    cameras[i].R.CopyTo(r);
            //    //OpenCvSharp.Mat t = ;
            //    OpenCvSharp.Detail.CameraParams camera = new OpenCvSharp.Detail.CameraParams(
            //        cameras[i].Focal,
            //        aspect,
            //        cameras[i].PpX,
            //        cameras[i].PpY,
            //        r,
            //        new OpenCvSharp.Mat());

            //    cameraParams.Add(camera);
            //}


            //GC.Collect(2);


            //tracked = true;
        }

        private void DumpRotationMatrices()
        {
            // Dump the estimated rotation matrices.
            //for (int i = 0; i < cameraParams.Count; i++)
            //{
            //    log.DebugFormat("Camera {0} rotation matrix:", i);
            //    log.DebugFormat("{0:0.000} {1:0.000} {2:0.000}", cameraParams[i].R.Get<double>(0, 0), cameraParams[i].R.Get<double>(0, 1), cameraParams[i].R.Get<double>(0, 2));
            //    log.DebugFormat("{0:0.000} {1:0.000} {2:0.000}", cameraParams[i].R.Get<double>(1, 0), cameraParams[i].R.Get<double>(1, 1), cameraParams[i].R.Get<double>(1, 2));
            //    log.DebugFormat("{0:0.000} {1:0.000} {2:0.000}", cameraParams[i].R.Get<double>(2, 0), cameraParams[i].R.Get<double>(2, 1), cameraParams[i].R.Get<double>(2, 2));
            //}
        }

        private void ClearCameraParams()
        {
            if (cameraParams.Count == 0)
                return;

            //for (int i = cameraParams.Count - 1; i >= 0; i--)
            //    cameraParams[i].Dispose();
            
            cameraParams.Clear();
        }

        /// <summary>
        /// Run all steps of the process in batch.
        /// </summary>
        public void RunAll(IWorkingZoneFramesContainer framesContainer, BackgroundWorker worker)
        {
            // This runs in the background thread.
            ResetTrackingData();

            string cancellationText = "Camera motion estimation cancelled.";
            FindFeatures(framesContainer, worker);
            if (worker.CancellationPending)
            {
                log.DebugFormat(cancellationText);
                return;
            }

            MatchFeatures(framesContainer, worker);
            if (worker.CancellationPending)
            {
                log.DebugFormat(cancellationText);
                return;
            }

            FindHomographies(framesContainer, worker);
            if (worker.CancellationPending)
            {
                log.DebugFormat(cancellationText);
                return;
            }

            BundleAdjustment(framesContainer, worker);
            if (worker.CancellationPending)
            {
                log.DebugFormat(cancellationText);
                return;
            }
        }

        /// <summary>
        /// Build the list of tracks (features tracked over multiple frames).
        /// This is not strictly necessary for the camera motion, it is for 
        /// visual feedback / debugging.
        /// </summary>
        public void BuildTracks(BackgroundWorker worker)
        {
            // Bail out if feature matching or find homography haven't run yet.
            if (matches.Count == 0 || inlierStatus.Count == 0)
                return;

            stopwatch.Restart();
            tracks.Clear();

            // Process all the matches and identify keypoints (features) that are
            // tracked over multiple consecutive frames.
            // Each "match" is a pair of feature from two consecutive frames
            // each feature in the pair points to the index of the feature
            // in the corresponding frame.

            // Keep the index of the last added keypoint to each track to identify
            // which track a given match should be added to.
            List<int> lastIdx = new List<int>();
            int longest = 0;

            // Process all the matches of all the frames.
            for (int frameIndex = 0; frameIndex < timestamps.Count - 1; frameIndex++)
            {
                if (worker.CancellationPending)
                    break;

                if (matches[frameIndex].Length !=inlierStatus[frameIndex].Count)
                    throw new InvalidProgramException("Number of matches and inlier status don't match.");

                for (int matchIndex = 0; matchIndex < matches[frameIndex].Length; matchIndex++)
                {
                    // Do not build tracks with keypoints not part of the global motion.
                    if (!inlierStatus[frameIndex][matchIndex])
                        continue;

                    // Get the matching keypoints and their index in their respective frame.
                    var m = matches[frameIndex][matchIndex];
                    OpenCvSharp.Point2f cvPt1 = keypoints[frameIndex][m.QueryIdx].Pt;
                    OpenCvSharp.Point2f cvPt2 = keypoints[frameIndex + 1][m.TrainIdx].Pt;
                    PointF p1 = new PointF(cvPt1.X, cvPt1.Y);
                    PointF p2 = new PointF(cvPt2.X, cvPt2.Y);

                    // See if the first keypoint of the match is already in a track.
                    int trackIndex = -1;
                    for (int j = 0; j < tracks.Count; j++)
                    {
                        // Make sure we don't match the keypoint to a track that 
                        // we just added in the current loop. Anything we just added
                        // will already have a timestmap corresponding to frameIndex + 1.
                        if (lastIdx[j] == m.QueryIdx && tracks[j].Last().Key == timestamps[frameIndex])
                        {
                            trackIndex = j;
                            break;
                        }
                    }

                    // If we couldn't find an existing track with that keypoint
                    // create a new one starting here.
                    if (trackIndex == -1)
                    {
                        var track = new SortedDictionary<long, PointF>();
                        track.Add(timestamps[frameIndex], p1);

                        tracks.Add(track);
                        lastIdx.Add(-1);

                        trackIndex = tracks.Count - 1;
                    }

                    // Extend the identified track with the match's second keypoint.
                    tracks[trackIndex].Add(timestamps[frameIndex + 1], p2);
                    lastIdx[trackIndex] = m.TrainIdx;
                    longest = Math.Max(longest, tracks[trackIndex].Count);
                }

                worker.ReportProgress(frameIndex + 1, timestamps.Count - 1);
            }

            // Remove short tracks
            tracks.RemoveAll(t => t.Count < parameters.MinTrackLength);

            // Compute the average track length.
            float avg = (float)tracks.Average(t => t.Count);
            
            log.DebugFormat("Build tracks: {0} ms.", stopwatch.ElapsedMilliseconds);
            log.DebugFormat("Average track length: {0} frames", avg);
            log.DebugFormat("Longest track length: {0} frames", longest);
        }
        #endregion

        #region Retrieve internal data

        /// <summary>
        /// Return features found on the frame at this timestamp.
        /// </summary>
        public List<PointF> GetFeatures(long timestamp)
        {
            if (keypoints.Count == 0)
                return null;

            if (!frameIndices.ContainsKey(timestamp) || frameIndices[timestamp] >= keypoints.Count)
                return null;

            List<PointF> features = new List<PointF>();
            foreach (var kp in keypoints[frameIndices[timestamp]])
            {
                features.Add(new PointF(kp.Pt.X, kp.Pt.Y));
            }

            return features;
        }

        /// <summary>
        /// Return matches found for the frame at this timestamp.
        /// These are the features in this frames that were also found in the next frame.
        /// </summary>
        public List<CameraMatch> GetMatches(long timestamp)
        {
            if (matches.Count == 0)
                return null;

            if (!frameIndices.ContainsKey(timestamp) || frameIndices[timestamp] >= matches.Count)
                return null;

            int frameIndex = frameIndices[timestamp];
            var frameMatches = matches[frameIndex];

            List<CameraMatch> result = new List<CameraMatch>();
            for (int i = 0; i < frameMatches.Length; i++)
            {
                var m = frameMatches[i];
                OpenCvSharp.Point2f cvPt1 = keypoints[frameIndex][m.QueryIdx].Pt;
                OpenCvSharp.Point2f cvPt2 = keypoints[frameIndex + 1][m.TrainIdx].Pt;
                PointF p1 = new PointF(cvPt1.X, cvPt1.Y);
                PointF p2 = new PointF(cvPt2.X, cvPt2.Y);

                // Note: inlier status is from find homography which may have failed or not run yet.
                bool hasInliers = inlierStatus.Count > frameIndex && inlierStatus[frameIndex].Count > i;
                bool inlier = hasInliers ? inlierStatus[frameIndex][i] : true;
                result.Add(new CameraMatch(p1, p2, inlier));
            }

            return result;
        }

        /// <summary>
        /// Return features tracked over multiple frames.
        /// </summary>
        public List<SortedDictionary<long, PointF>> GetTracks()
        {
            return tracks;
        }
        #endregion

        #region Other public helpers
        public void ResetTrackingData()
        {
            tracked = false;
            
            frameIndices.Clear();
            timestamps.Clear();
            keypoints.Clear();
            descriptors.Clear();
            matches.Clear();
            inlierStatus.Clear();
            inliers.Clear();
            consecTransforms.Clear();
            tracks.Clear();
        }

        public void SetMask(Bitmap mask)
        {
            if (this.mask != null)
                this.mask.Dispose();

            this.mask = mask;
        }

        /// <summary>
        /// Parse the camera intrinsic and extrinsic parameters calculated by COLMAP (exported as Text).
        /// </summary>
        public void ParseColmap(string folderName)
        {
            string camerasFile = Path.Combine(folderName, "cameras.txt");
            string imagesFile = Path.Combine(folderName, "images.txt");
            if (!File.Exists(camerasFile) || !File.Exists(imagesFile))
                return;

            float parse(string value)
            {
                return float.Parse(value, CultureInfo.InvariantCulture);
            }

            intrinsics.Clear();
            extrinsics.Clear();

            // Parse the camera file (intrinsic parameters).
            // # Camera list with one line of data per camera:
            // #   CAMERA_ID, MODEL, WIDTH, HEIGHT, PARAMS[]
            foreach (var line in File.ReadLines(camerasFile))
            {
                // Examples:
                // # 1 SIMPLE_RADIAL 2048 1536 1580.46 1024 768 0.0045691
                // # 1 RADIAL 1920 1080 1665.1 960 540 0.0672856 -0.0761443
                // # 1 OPENCV 3840 2160 3178.27 3182.09 1920 1080 0.159668 -0.231286 -0.00123982 0.00272224
                if (line.StartsWith("#"))
                    continue;

                string[] split = line.Split(' ');
                int width = (int)parse(split[2]);
                int height = (int)parse(split[3]);
                float fx = parse(split[4]);
                float fy = fx;
                float k1 = 0;
                float k2 = 0;
                float k3 = 0;
                float p1 = 0;
                float p2 = 0;
                float cx = width / 2;
                float cy = height / 2;

                if (split[1] == "SIMPLE_RADIAL")
                {
                    cx = parse(split[5]);
                    cy = parse(split[6]);
                    k1 = parse(split[7]);
                }
                else if (split[1] == "RADIAL")
                {
                    cx = parse(split[5]);
                    cy = parse(split[6]);
                    k1 = parse(split[7]);
                    k2 = parse(split[8]);
                }
                else if (split[1] == "OPENCV")
                {
                    fy = parse(split[5]);
                    cx = parse(split[6]);
                    cy = parse(split[7]);
                    k1 = parse(split[8]);
                    k2 = parse(split[9]);
                    p1 = parse(split[10]);
                    p2 = parse(split[11]);
                }

                // TODO: store to intrinsics list.
                var parameters = new DistortionParameters(k1, k2, k3, p1, p2, fx, fy, cx, cy, new Size(width, height));
                intrinsics.Add(parameters);
            }

            // Parse the image file (extrinsic parameters).
            // # Image list with two lines of data per image:
            // # IMAGE_ID, QW, QX, QY, QZ, TX, TY, TZ, CAMERA_ID, NAME
            // # POINTS2D[] as (X, Y, POINT3D_ID)
            int lineNumber = 0;
            List<Dictionary<int, PointF>> points = new List<Dictionary<int, PointF>>();
            foreach (var line in File.ReadLines(imagesFile))
            {
                if (line.StartsWith("#"))
                    continue;

                if (lineNumber % 2 == 0)
                {
                    // # IMAGE_ID, QW, QX, QY, QZ, TX, TY, TZ, CAMERA_ID, NAME
                    string[] split = line.Split(' ');
                    float qw, qx, qy, qz;
                    qw = parse(split[1]);
                    qx = parse(split[2]);
                    qy = parse(split[3]);
                    qz = parse(split[4]);
                    float tx, ty, tz;
                    tx = parse(split[5]);
                    ty = parse(split[6]);
                    tz = parse(split[7]);

                    // Convert quaternion to rotation matrix.
                    // Construct 4x4 matrix.
                    // Invert.

                    var cameraMatrix = new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
                    extrinsics.Add(cameraMatrix);
                }
                else
                {
                    // # POINTS2D[] as (X, Y, POINT3D_ID)
                    string[] split = line.Split(' ');
                    Dictionary<int, PointF> dict = new Dictionary<int, PointF>();
                    int count = split.Length / 3;
                    for (int j = 0; j < count; j += 3)
                    {
                        int pId = (int)parse(split[j + 2]);
                        if (pId == -1)
                            continue;

                        if (dict.ContainsKey(pId))
                            continue;

                        float x = parse(split[j + 0]);
                        float y = parse(split[j + 1]);
                        dict.Add(pId, new PointF(x, y));
                    }

                    points.Add(dict);
                    log.DebugFormat("Image [{0}], valid points:{1}", lineNumber / 2, dict.Count);
                }

                lineNumber++;
            }

            // Recreate the list of consecutive homographies.
            inlierStatus.Clear();
            inliers.Clear();
            consecTransforms.Clear();
            for (int i = 0; i < points.Count - 1; i++)
            {
                // Find points visible on both frames.
                List<OpenCvSharp.Point2d> srcPoints = new List<OpenCvSharp.Point2d>();
                List<OpenCvSharp.Point2d> dstPoints = new List<OpenCvSharp.Point2d>();
                foreach (var pair in points[i])
                {
                    if (points[i + 1].ContainsKey(pair.Key))
                    {
                        srcPoints.Add(new OpenCvSharp.Point2d(pair.Value.X, pair.Value.Y));
                        dstPoints.Add(new OpenCvSharp.Point2d(points[i + 1][pair.Key].X, points[i + 1][pair.Key].Y));
                    }
                }

                OpenCvSharp.HomographyMethods method = (OpenCvSharp.HomographyMethods)OpenCvSharp.RobustEstimationAlgorithms.USAC_MAGSAC;
                var mask = new List<byte>();
                var homography = OpenCvSharp.Cv2.FindHomography(srcPoints, dstPoints, method, parameters.RansacReprojThreshold, OpenCvSharp.OutputArray.Create(mask));

                // Collect inliers.
                inlierStatus.Add(new List<bool>());
                inliers.Add(new List<PointF>());
                for (int j = 0; j < mask.Count; j++)
                {
                    bool inlier = mask[j] != 0;
                    inlierStatus[i].Add(inlier);

                    if (inlier)
                    {
                        PointF p = new PointF((float)srcPoints[j].X, (float)srcPoints[j].Y);
                        inliers[i].Add(p);
                    }
                }

                LogHomography(i, i + 1, homography);
                consecTransforms.Add(homography);
            }
        }
        #endregion 

        #region Private helpers
        private void LogHomography(int index1, int index2, OpenCvSharp.Mat homography)
        {
            double[] m;
            homography.GetArray<double>(out m);
            string[] m2 = m.Select(v => v.ToString(CultureInfo.InvariantCulture)).ToArray();
            string strHomography = string.Join(" ", m2);
            log.DebugFormat("[{0} -> {1}]: {2}", index1, index2, strHomography);
        }
        #endregion
    }
}
