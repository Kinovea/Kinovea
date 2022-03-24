using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Kinovea.ScreenManager.Languages;
using Kinovea.Video;
using Kinovea.Services;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Globalization;

namespace Kinovea.ScreenManager
{
    public class VideoFilterCameraMotion : IVideoFilter
    {
        #region Properties
        public string FriendlyName
        {
            get { return "Camera motion"; }
        }
        public Bitmap Current
        {
            get { return null; }
        }
        public List<ToolStripItem> ContextMenu
        {
            get
            {
                // Just in time localization.
                mnuConfigure.Text = ScreenManagerLang.Generic_ConfigurationElipsis;
                mnuImportMask.Text = "Import mask";
                mnuImportColmap.Text = "Import COLMAP";
                mnuRun.Text = "Run camera motion estimation";
                return contextMenu;
            }
        }
        public bool CanExportVideo
        {
            get { return false; }
        }

        public bool CanExportImage
        {
            get { return false; }
        }

        public int ContentHash 
        { 
            get { return 0; }
        }
        #endregion

        #region members
        private Size frameSize;
        private IWorkingZoneFramesContainer framesContainer;
        private Metadata metadata;
        private Bitmap mask;

        // Computed data
        private Dictionary<long, int> frameIndices = new Dictionary<long, int>();
        private List<OpenCvSharp.KeyPoint[]> keypoints = new List<OpenCvSharp.KeyPoint[]>();
        private List<OpenCvSharp.Mat> descriptors = new List<OpenCvSharp.Mat>();
        private List<Tuple<int, int>> imagePairs = new List<Tuple<int, int>>();
        private List<OpenCvSharp.DMatch[]> matches = new List<OpenCvSharp.DMatch[]>();
        private List<List<bool>> inlierStatus = new List<List<bool>>();
        private List<List<PointF>> inliers = new List<List<PointF>>();
        private List<OpenCvSharp.Mat> consecTransforms = new List<OpenCvSharp.Mat>();
        private List<OpenCvSharp.Mat> forwardTransforms = new List<OpenCvSharp.Mat>();
        private List<OpenCvSharp.Mat> backwardTransforms = new List<OpenCvSharp.Mat>();

        private List<DistortionParameters> intrinsics = new List<DistortionParameters>();
        private List<double[,]> extrinsics = new List<double[,]>();

        // Core parameters
        private CameraMotionParameters parameters = new CameraMotionParameters();
        private int featuresPerFrame = 500;
        private double ransacReprojThreshold = 1.5;

        // Display parameters
        private bool showFeatures = true;
        private bool showMatches = true;
        private bool showTransforms = true;

        // Menu
        private List<ToolStripItem> contextMenu = new List<ToolStripItem>();
        private ToolStripMenuItem mnuConfigure = new ToolStripMenuItem();
        private ToolStripMenuItem mnuImportMask = new ToolStripMenuItem();
        private ToolStripMenuItem mnuImportColmap = new ToolStripMenuItem();
        private ToolStripMenuItem mnuRun = new ToolStripMenuItem();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region ctor/dtor
        public VideoFilterCameraMotion(Metadata metadata)
        {
            this.metadata = metadata;

            mnuConfigure.Image = Properties.Drawings.configure;
            contextMenu.Add(mnuConfigure);
            contextMenu.Add(mnuImportMask);
            contextMenu.Add(mnuImportColmap);
            contextMenu.Add(mnuRun);

            mnuConfigure.Click += MnuConfigure_Click;
            mnuImportMask.Click += MnuImportMask_Click;
            mnuImportColmap.Click += MnuImportColmap_Click;
            mnuRun.Click += MnuRun_Click;

            //parameters = PreferencesManager.PlayerPreferences.CameraMotion;
        }
        ~VideoFilterCameraMotion()
        {
            Dispose(false);
        }

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

        #region IVideoFilter methods
        public void SetFrames(IWorkingZoneFramesContainer framesContainer)
        {
            this.framesContainer = framesContainer;
            if (framesContainer != null && framesContainer.Frames != null && framesContainer.Frames.Count > 0)
                frameSize = framesContainer.Frames[0].Image.Size;
        }
        public void UpdateSize(Size size)
        {
        }

        public void UpdateTime(long timestamp)
        {
        }

        public void StartMove(PointF p)
        {
        }

        public void StopMove()
        {
        }

        public void Move(float dx, float dy, Keys modifiers)
        {
        }

        public void DrawExtra(Graphics canvas, IImageToViewportTransformer transformer, long timestamp)
        {
            if (showFeatures)
                DrawFeatures(canvas, transformer, timestamp);

            if (showMatches)
                DrawMatches(canvas, transformer, timestamp);

            //if (showInliers)
              //  DrawInliers(canvas, transformer, timestamp);

            if (showTransforms)
                DrawTransforms(canvas, transformer, timestamp);
        }

        public void ExportVideo(IDrawingHostView host)
        {
            throw new NotImplementedException();
        }

        public void ExportImage(IDrawingHostView host)
        {
            throw new NotImplementedException();
        }

        public void ResetData()
        {
            
        }
        public void WriteData(XmlWriter w)
        {
            
        }

        public void ReadData(XmlReader r)
        {
            bool isEmpty = r.IsEmptyElement;
            r.ReadStartElement();

            if (isEmpty)
                return;

            r.ReadEndElement();
        }

        #endregion

        #region Private methods
        private void MnuConfigure_Click(object sender, EventArgs e)
        {
            
        }

        private void MnuImportMask_Click(object sender, EventArgs e)
        {
            // Open image.
            // Reject image if not of the same size.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Import mask";
            openFileDialog.RestoreDirectory = true;
            //openFileDialog.Filter = "";
            //openFileDialog.FilterIndex = 0;
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            string filename = openFileDialog.FileName;
            if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
                return;

            if (mask != null)
                mask.Dispose();
            
            mask = new Bitmap(filename);

            openFileDialog.Dispose();
        }

        private void MnuImportColmap_Click(object sender, EventArgs e)
        {
            // Import camera intrinsics & extrinsics calculated by COLMAP.
            // Point to folder containing text export.
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            string folderName = "";
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok && !string.IsNullOrEmpty(dialog.FileName))
                folderName = dialog.FileName;

            dialog.Dispose();
            if (string.IsNullOrEmpty(folderName))
                return;

            ParseColmap(folderName);
            InvalidateFromMenu(sender);

            // Commit transform data.
            int frameIndex = 0;
            foreach (var f in framesContainer.Frames)
            {
                if (frameIndices.ContainsKey(f.Timestamp))
                    continue;

                frameIndices.Add(f.Timestamp, frameIndex);
                frameIndex++;
            }

            metadata.SetCameraMotion(frameIndices, consecTransforms);
        }

        private void MnuRun_Click(object sender, EventArgs e)
        {
            if (framesContainer == null || framesContainer.Frames == null || framesContainer.Frames.Count < 1)
                return;

            frameIndices.Clear();
            keypoints.Clear();
            descriptors.Clear();
            imagePairs.Clear();
            matches.Clear();
            inlierStatus.Clear();
            inliers.Clear();
            consecTransforms.Clear();
            forwardTransforms.Clear();
            backwardTransforms.Clear();

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
            }
            
            // Find and describe features on each frame.
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

                log.DebugFormat("Feature detection - Frame [{0}]: {1} features.", keypoints.Count, keypoints[keypoints.Count - 1].Length);
                frameIndex++;
            }

            if (hasMask)
                cvMaskGray.Dispose();
            
            orb.Dispose();

            // Match features in consecutive frames.
            // TODO: match each frame with the n next frames where n depends on framerate.
            var matcher = new OpenCvSharp.BFMatcher(OpenCvSharp.NormTypes.Hamming, crossCheck: true);
            for (int i = 0; i < descriptors.Count - 1; i++)
            {
                var mm = matcher.Match(descriptors[i], descriptors[i + 1]);
                matches.Add(mm);
            }

            matcher.Dispose();

            // Find transforms between frames.
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

                LogHomography(i, i + 1, homography);
                consecTransforms.Add(homography);
            }

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

            InvalidateFromMenu(sender);

            // Commit transform data.
            metadata.SetCameraMotion(frameIndices, consecTransforms);
        }

        private void LogHomography(int index1, int index2, OpenCvSharp.Mat homography)
        {
            //double m00 = homography.At<double>(0, 0);
            //double m01 = homography.At<double>(0, 1);
            //double m02 = homography.At<double>(0, 2);

            double[] m;
            homography.GetArray<double>(out m);
            string[] m2 = m.Select(v => v.ToString(CultureInfo.InvariantCulture)).ToArray();
            string strHomography = string.Join(" ", m2);
            log.DebugFormat("[{0} -> {1}]: {2}", index1, index2, strHomography);
        }

        /// <summary>
        /// Concatenate two affine matrices, where 
        /// - a is already a 3x3 matrix of CV_64FC1, 
        /// - b is a 2x3 matrix from OpenCV estimate affine 2D, also of CV_64FC1.
        /// </summary>
        //private OpenCvSharp.Mat ConcatAffine(OpenCvSharp.Mat a, OpenCvSharp.Mat b)
        //{
        //    OpenCvSharp.Mat temp = OpenCvSharp.Mat.Eye(3, 3, OpenCvSharp.MatType.CV_64FC1);
        //    b.Row(0).CopyTo(temp.Row(0));
        //    b.Row(1).CopyTo(temp.Row(1));

        //    var result = a * temp;
        //    temp.Dispose();

        //    return result;
        //}

        private void InvalidateFromMenu(object sender)
        {
            // Update the main viewport.
            // The screen hook was injected inside the menu.
            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            if (tsmi == null)
                return;

            IDrawingHostView host = tsmi.Tag as IDrawingHostView;
            if (host == null)
                return;

            host.InvalidateFromMenu();
        }

        /// <summary>
        /// Parse the camera intrinsic and extrinsic parameters calculated by COLMAP (exported as Text).
        /// </summary>
        private void ParseColmap(string folderName)
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
                    for (int j = 0; j < count; j+=3)
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


        #region Drawing
        private void DrawFeatures(Graphics canvas, IImageToViewportTransformer transformer, long timestamp)
        {
            if (keypoints.Count == 0) 
                return;

            if (!frameIndices.ContainsKey(timestamp) || frameIndices[timestamp] >= keypoints.Count)
                return;

            foreach (var kp in keypoints[frameIndices[timestamp]])
            {
                PointF p = new PointF(kp.Pt.X, kp.Pt.Y);
                p = transformer.Transform(p);

                using (Pen pen = new Pen(Color.CornflowerBlue, 2.0f))
                    canvas.DrawEllipse(pen, p.Box(3));
            }
        }

        private void DrawMatches(Graphics canvas, IImageToViewportTransformer transformer, long timestamp)
        {
            if (matches.Count == 0)
                return;

            if (!frameIndices.ContainsKey(timestamp) || frameIndices[timestamp] >= matches.Count)
                return;

            int frameIndex = frameIndices[timestamp];
            var frameMatches = matches[frameIndex];
            
            for (int i = 0; i < frameMatches.Length; i++)
            {
                var m = frameMatches[i];
                PointF p1 = new PointF(keypoints[frameIndex][m.QueryIdx].Pt.X, keypoints[frameIndex][m.QueryIdx].Pt.Y);
                PointF p2 = new PointF(keypoints[frameIndex + 1][m.TrainIdx].Pt.X, keypoints[frameIndex + 1][m.TrainIdx].Pt.Y);
                p1 = transformer.Transform(p1);
                p2 = transformer.Transform(p2);

                var inlier = inlierStatus[frameIndex][i];
                Color c = inlier ? Color.LimeGreen : Color.Red;

                using (Pen pen = new Pen(c, 2.0f))
                    canvas.DrawLine(pen, p1, p2);
            }

            DrawInliers(canvas, transformer, timestamp);
        }

        private void DrawInliers(Graphics canvas, IImageToViewportTransformer transformer, long timestamp)
        {
            if (inliers.Count == 0)
                return;

            if (!frameIndices.ContainsKey(timestamp) || frameIndices[timestamp] >= inliers.Count)
                return;

            int frameIndex = frameIndices[timestamp];
            foreach (var p in inliers[frameIndex])
            {
                var p2 = transformer.Transform(p);
                using (Pen pen = new Pen(Color.DarkGreen, 3.0f))
                    canvas.DrawEllipse(pen, p2.Box(5));
            }
        }

        private void DrawTransforms(Graphics canvas, IImageToViewportTransformer transformer, long timestamp)
        {
            if (consecTransforms.Count == 0)
                return;

            if (!frameIndices.ContainsKey(timestamp))
                return;

            // Transform a rectangle to show how the image is modified from one frame to the next.
            Rectangle rect = new Rectangle(Point.Empty, frameSize);
            var bounds = new[]
            {
                new OpenCvSharp.Point2f(rect.Left, rect.Top),
                new OpenCvSharp.Point2f(rect.Right, rect.Top),
                new OpenCvSharp.Point2f(rect.Right, rect.Bottom),
                new OpenCvSharp.Point2f(rect.Left, rect.Bottom),
            };

            //if (frameIndices[timestamp] >= forwardTransforms.Count)
            //  return;

            // Cumulative transforms.
            //var transform = forwardTransforms[frameIndices[timestamp]];
            //DrawTransformRectangle(canvas, transformer, transform, bounds, Color.Yellow);

            // Draw the bounds of one reference frame in all subsequent frames.
            //var points = bounds;
            //for (int i = 30; i < frameIndices[timestamp]; i++)
            //{
            //    points = OpenCvSharp.Cv2.PerspectiveTransform(points, consecTransforms[i]);
            //}

            //var points3 = points.Select(p => new PointF((float)p.X, (float)p.Y));

            //var points4 = transformer.Transform(points3);
            //using (Pen pen = new Pen(Color.Yellow, 4.0f))
            //    canvas.DrawPolygon(pen, points4.ToArray());

            //---------------------------------
            // Draw the bounds of all the frames in all subsequent frames.
            if (frameIndices[timestamp] >= consecTransforms.Count)
                return;
            
            for (int i = 0; i < frameIndices[timestamp]; i++)
            {
                var points = bounds;
                for (int j = i; j < frameIndices[timestamp]; j++)
                {
                    points = OpenCvSharp.Cv2.PerspectiveTransform(points, consecTransforms[j]);
                }

                var points3 = points.Select(p => new PointF(p.X, p.Y));

                var points4 = transformer.Transform(points3);
                using (Pen pen = new Pen(Color.Yellow, 1.0f))
                    canvas.DrawPolygon(pen, points4.ToArray());
            }
        }

        private void DrawTransformRectangle(Graphics canvas, IImageToViewportTransformer transformer, OpenCvSharp.Mat transform, OpenCvSharp.Point2f[] points, Color color)
        {
            // Homography
            var points2 = OpenCvSharp.Cv2.PerspectiveTransform(points, transform);
            var points3 = points2.Select(p => new PointF((float)p.X, (float)p.Y));
            var points4 = transformer.Transform(points3);
            using (Pen pen = new Pen(color, 4.0f))
                canvas.DrawPolygon(pen, points4.ToArray());


            // Affine
            //var src = new OpenCvSharp.Mat(points.Length, 1, OpenCvSharp.MatType.CV_32FC2, points);
            //var dst = new OpenCvSharp.Mat();
            //OpenCvSharp.Cv2.Transform(src, dst, transform);

            //var points2 = new List<PointF>();
            //for (int i = 0; i < points.Length; i++)
            //{
            //    OpenCvSharp.Vec2f p = dst.Get<OpenCvSharp.Vec2f>(i);
            //    points2.Add(new PointF(p[0], p[1]));
            //}

            //var points3 = transformer.Transform(points2);
            //using (Pen pen = new Pen(color, 4.0f))
            //    canvas.DrawPolygon(pen, points3.ToArray());

            //src.Dispose();
            //dst.Dispose();
        }
        #endregion
        #endregion
    }
}
