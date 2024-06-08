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
    public class CameraTracker
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
        private Dictionary<long, int> frameIndices = new Dictionary<long, int>();
        private List<OpenCvSharp.KeyPoint[]> keypoints = new List<OpenCvSharp.KeyPoint[]>();
        private List<OpenCvSharp.Mat> descriptors = new List<OpenCvSharp.Mat>();
        private List<OpenCvSharp.DMatch[]> matches = new List<OpenCvSharp.DMatch[]>();
        private List<List<bool>> inlierStatus = new List<List<bool>>();
        private List<List<PointF>> inliers = new List<List<PointF>>();
        private List<OpenCvSharp.Mat> consecTransforms = new List<OpenCvSharp.Mat>();
        
        // The following are used when we import transforms from COLMAP.
        private List<DistortionParameters> intrinsics = new List<DistortionParameters>();
        private List<double[,]> extrinsics = new List<double[,]>();

        // Core parameters
        private CameraMotionParameters parameters;
        private float distanceThresholdNormalized = 0.1f; // Matches spanning more than this fraction of the image width are filtered out.
        private bool prefilterSpuriousMatches = true;
        private CameraMotionFeatureType featureType = CameraMotionFeatureType.ORB;
        private bool distanceRatioTest = true;

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
                    foreach (var desc in descriptors)
                        desc.Dispose();

                if (mask != null)
                    mask.Dispose();
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

            if (featureType == CameraMotionFeatureType.ORB)
                orb = OpenCvSharp.ORB.Create(parameters.FeaturesPerFrame);
            else
                sift = OpenCvSharp.Features2D.SIFT.Create(parameters.FeaturesPerFrame);

            int frameIndex = 0;
            for (int i = 0; i < framesContainer.Frames.Count; i++)
            {
                if (worker.CancellationPending)
                    break;

                var f = framesContainer.Frames[i];
                if (frameIndices.ContainsKey(f.Timestamp))
                    continue;

                frameIndices.Add(f.Timestamp, frameIndex);

                // Convert image to OpenCV and convert to grayscale.
                var cvImage = OpenCvSharp.Extensions.BitmapConverter.ToMat(f.Image);
                var cvImageGray = new OpenCvSharp.Mat();
                OpenCvSharp.Cv2.CvtColor(cvImage, cvImageGray, OpenCvSharp.ColorConversionCodes.BGR2GRAY, 0);
                cvImage.Dispose();

                // Feature detection & description.
                var desc = new OpenCvSharp.Mat();
                OpenCvSharp.KeyPoint[] kp;
                if (featureType == CameraMotionFeatureType.ORB)
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

            if (featureType == CameraMotionFeatureType.ORB)
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
            // Matcher type: Brute force (exact) or FLANN based. We use the BF matcher.
            //
            // There are 3 basic strategies:
            // - unidirectional: match features from i to i+1.
            // - bidirectional + "both": match features from i to i+1 and from i+1 to i and keep the intersection.
            // - bidirectional + "either": same but keep the union of all matches.
            //
            // Using crossCheck = true in the BFMatcher constructor corresponds to strategy "both": it will only
            // return pairs (i,j) such that for i-th query descriptor the j-th descriptor in the matcher's
            // collection is the nearest and vice versa.
            //
            // Lowe's ratio test.
            // -> Only keep matches where the nearest neighbor is much closer than the second nearest neighbor.
            // Otherwise the match is not very discriminatory and it's likely to be a false positive.
            bool crossCheck = !distanceRatioTest;
            float r = 0.8f;

            if (descriptors.Count == 0)
                return;
            
            stopwatch.Restart();
            matches.Clear();
            List<OpenCvSharp.DMatch[]> framesMatches = new List<OpenCvSharp.DMatch[]>();

            // Matching distance: SIFT requires L1 norm.
            OpenCvSharp.BFMatcher matcher = null;
            if (featureType == CameraMotionFeatureType.ORB)
                matcher = new OpenCvSharp.BFMatcher(OpenCvSharp.NormTypes.Hamming, crossCheck: crossCheck);
            else
                matcher = new OpenCvSharp.BFMatcher(OpenCvSharp.NormTypes.L1, crossCheck: crossCheck);

            for (int i = 0; i < descriptors.Count - 1; i++)
            {
                if (worker.CancellationPending)
                    break;

                if (distanceRatioTest)
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

            if (prefilterSpuriousMatches)
            {
                // We know we are tracking a video frame by frame so we can assume the motion vectors to be
                // relatively small. Any match over a distance threshold can be eliminated right away.
                // This will make the job of RANSAC easier.
                Size imageSize = framesContainer.Frames[0].Image.Size;
                float distanceThreshold = imageSize.Width * distanceThresholdNormalized;
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
            int maxIter = 10000;
            double confidence = 0.995;

            
            stopwatch.Restart();
            consecTransforms.Clear();
            for (int i = 0; i < descriptors.Count - 1; i++)
            {
                if (worker.CancellationPending)
                    break;

                var mm = matches[i];
                var srcPoints = mm.Select(m => new OpenCvSharp.Point2d(keypoints[i][m.QueryIdx].Pt.X, keypoints[i][m.QueryIdx].Pt.Y));
                var dstPoints = mm.Select(m => new OpenCvSharp.Point2d(keypoints[i + 1][m.TrainIdx].Pt.X, keypoints[i + 1][m.TrainIdx].Pt.Y));

                OpenCvSharp.HomographyMethods method = (OpenCvSharp.HomographyMethods)OpenCvSharp.RobustEstimationAlgorithms.USAC_MAGSAC;
                var mask = new List<byte>();
                var cvMask = OpenCvSharp.OutputArray.Create(mask);
                var homography = OpenCvSharp.Cv2.FindHomography(srcPoints, dstPoints, method, ransacReprojThreshold, cvMask);

                // Collect inliers.
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

        public void BundleAdjustment(IWorkingZoneFramesContainer framesContainer, BackgroundWorker worker)
        {
            tracked = true;
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
                PointF p1 = new PointF(keypoints[frameIndex][m.QueryIdx].Pt.X, keypoints[frameIndex][m.QueryIdx].Pt.Y);
                PointF p2 = new PointF(keypoints[frameIndex + 1][m.TrainIdx].Pt.X, keypoints[frameIndex + 1][m.TrainIdx].Pt.Y);

                bool inlier = inlierStatus.Count > 0 ? inlierStatus[frameIndex][i] : true;
                result.Add(new CameraMatch(p1, p2, inlier));
            }

            return result;
        }
        #endregion

        #region Other public helpers
        public void ResetTrackingData()
        {
            tracked = false;
            
            frameIndices.Clear();
            keypoints.Clear();
            descriptors.Clear();
            //imagePairs.Clear();
            matches.Clear();
            inlierStatus.Clear();
            inliers.Clear();
            consecTransforms.Clear();
            //forwardTransforms.Clear();
            //backwardTransforms.Clear();
        }

        public void SetMask(string filename)
        {
            if (mask != null)
                mask.Dispose();

            mask = new Bitmap(filename);
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
