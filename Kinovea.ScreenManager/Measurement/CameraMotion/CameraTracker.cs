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

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This class performs the camera tracking for camera motion estimation.
    /// It calls OpenCV functions and exposes the raw result.
    /// The result is then sent over to `CameraTransformer` which exposes functions 
    /// to transform coordinates from one frame to another.
    /// </summary>
    public class CameraTracker
    {
        #region Properties
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

        // Computed data
        // frameIndices: reverse index from timestamps to frames indices.
        // keypoints: features found on the images.
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
        //private List<Tuple<int, int>> imagePairs = new List<Tuple<int, int>>();
        //private List<OpenCvSharp.Mat> forwardTransforms = new List<OpenCvSharp.Mat>();
        //private List<OpenCvSharp.Mat> backwardTransforms = new List<OpenCvSharp.Mat>();

        // The following are used when we import transforms from COLMAP.
        private List<DistortionParameters> intrinsics = new List<DistortionParameters>();
        private List<double[,]> extrinsics = new List<double[,]>();

        // Core parameters
        private CameraMotionParameters parameters = new CameraMotionParameters();
        private int featuresPerFrame = 500;
        private double ransacReprojThreshold = 1.5;

        private Stopwatch stopwatch = new Stopwatch();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public CameraTracker()
        {

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

        public void Run(IWorkingZoneFramesContainer framesContainer)
        {
            ResetTrackingData();

            stopwatch.Start();

            var orb = OpenCvSharp.ORB.Create(featuresPerFrame);

            // Import and convert mask if needed.
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


            // Find and describe features on each frame.
            // TODO: try to run this through a parallel for if possible.
            // Do we need frameIndices, keypoints and descriptors to be sequential?
            // If so, prepare all the tables without the detection and then run the detection in parallel.
            int frameIndex = 0;
            foreach (var f in framesContainer.Frames)
            {
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
                orb.DetectAndCompute(cvImageGray, cvMaskGray, out var kp, desc);

                keypoints.Add(kp);
                descriptors.Add(desc);

                cvImageGray.Dispose();

                //log.DebugFormat("Feature detection - Frame [{0}]: {1} features.", keypoints.Count, keypoints[keypoints.Count - 1].Length);
                frameIndex++;
            }

            if (hasMask)
                cvMaskGray.Dispose();

            orb.Dispose();

            log.DebugFormat("Feature detection: {0} ms.", stopwatch.ElapsedMilliseconds);

            // Match features in consecutive frames.
            // TODO: match each frame with the n next frames where n depends on framerate.
            var matcher = new OpenCvSharp.BFMatcher(OpenCvSharp.NormTypes.Hamming, crossCheck: true);
            for (int i = 0; i < descriptors.Count - 1; i++)
            {
                var mm = matcher.Match(descriptors[i], descriptors[i + 1]);
                matches.Add(mm);
            }

            matcher.Dispose();
            log.DebugFormat("Feature matching: {0} ms.", stopwatch.ElapsedMilliseconds);

            // Compute transforms between consecutive frames.
            // TODO: bundle adjustment.
            for (int i = 0; i < descriptors.Count - 1; i++)
            {
                var mm = matches[i];
                var srcPoints = mm.Select(m => new OpenCvSharp.Point2d(keypoints[i][m.QueryIdx].Pt.X, keypoints[i][m.QueryIdx].Pt.Y));
                var dstPoints = mm.Select(m => new OpenCvSharp.Point2d(keypoints[i + 1][m.TrainIdx].Pt.X, keypoints[i + 1][m.TrainIdx].Pt.Y));

                OpenCvSharp.HomographyMethods method = (OpenCvSharp.HomographyMethods)OpenCvSharp.RobustEstimationAlgorithms.USAC_MAGSAC;
                var mask = new List<byte>();
                var homography = OpenCvSharp.Cv2.FindHomography(srcPoints, dstPoints, method, ransacReprojThreshold, OpenCvSharp.OutputArray.Create(mask));

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

                //LogHomography(i, i + 1, homography);
                consecTransforms.Add(homography);
            }

            log.DebugFormat("Transforms computation: {0} ms.", stopwatch.ElapsedMilliseconds);

            // Precompute all the transform matrices towards and back from the common frame of reference.
            // For now we use the first frame as the global reference.
            //var identity = OpenCvSharp.Mat.Eye(3, 3, OpenCvSharp.MatType.CV_64FC1);
            //for (int i = 0; i < frameIndices.Count; i++)
            //{
            //    // Forward.
            //    var mat = identity;
            //    if (i > 0)
            //    {
            //        mat = forwardTransforms[i - 1] * consecTransforms[i - 1];
            //    }

            //    forwardTransforms.Add(mat);
            //}

        }

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
                bool inlier = inlierStatus[frameIndex][i];
                
                result.Add(new CameraMatch(p1, p2, inlier));
            }

            return result;
        }

        public void ResetTrackingData()
        {
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
                var homography = OpenCvSharp.Cv2.FindHomography(srcPoints, dstPoints, method, ransacReprojThreshold, OpenCvSharp.OutputArray.Create(mask));

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

        private void LogHomography(int index1, int index2, OpenCvSharp.Mat homography)
        {
            double[] m;
            homography.GetArray<double>(out m);
            string[] m2 = m.Select(v => v.ToString(CultureInfo.InvariantCulture)).ToArray();
            string strHomography = string.Join(" ", m2);
            log.DebugFormat("[{0} -> {1}]: {2}", index1, index2, strHomography);
        }

        

    }
}
