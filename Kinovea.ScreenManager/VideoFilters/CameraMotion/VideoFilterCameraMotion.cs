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

namespace Kinovea.ScreenManager
{
    public class VideoFilterCameraMotion : IVideoFilter
    {
        #region Properties
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
        private ToolStripMenuItem mnuRun = new ToolStripMenuItem();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region ctor/dtor
        public VideoFilterCameraMotion(Metadata metadata)
        {
            this.metadata = metadata;

            mnuConfigure.Image = Properties.Drawings.configure;
            //mnuRun.Image
            contextMenu.Add(mnuConfigure);
            contextMenu.Add(mnuRun);
            contextMenu.Add(new ToolStripSeparator());

            mnuConfigure.Click += MnuConfigure_Click;
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

        private void MnuRun_Click(object sender, EventArgs e)
        {
            if (framesContainer == null || framesContainer.Frames == null || framesContainer.Frames.Count < 1)
                return;

            // Find and describe features in all images.
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

            // Loop through frames, detect and describe features.
            int frameIndex = 0;
            foreach (var f in framesContainer.Frames)
            {
                if (frameIndices.ContainsKey(f.Timestamp))
                    continue;

                frameIndices.Add(f.Timestamp, frameIndex);
                
                //OpenCvSharp.KeyPoint[] kp = OpenCvSharp.Cv2.FAST(imgGray, 50, true);
                var cvImage = OpenCvSharp.Extensions.BitmapConverter.ToMat(f.Image);
                var cvImageGray = new OpenCvSharp.Mat();
                OpenCvSharp.Cv2.CvtColor(cvImage, cvImageGray, OpenCvSharp.ColorConversionCodes.BGR2GRAY, 0);
                cvImage.Dispose();

                // Feature detection & description.
                var desc = new OpenCvSharp.Mat();
                orb.DetectAndCompute(cvImageGray, null, out var kp, desc);
                
                keypoints.Add(kp);
                descriptors.Add(desc);

                cvImageGray.Dispose();

                log.DebugFormat("Feature detection - Frame [{0}]: {1} features.", keypoints.Count, kp.Length);
                frameIndex++;
            }

            orb.Dispose();

            // Match features in consecutive frames.
            var matcher = new OpenCvSharp.BFMatcher(OpenCvSharp.NormTypes.Hamming, crossCheck: true);
            for (int i = 0; i < descriptors.Count - 1; i++)
            {
                var mm = matcher.Match(descriptors[i], descriptors[i+1]);
                matches.Add(mm);

                // Find transforms between consecutive frames.
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

                consecTransforms.Add(homography);
            }

            matcher.Dispose();

            // Precompute all the transform matrices towards and back from the common frame of reference.
            // For now we use the first frame as the global reference.
            var identity = OpenCvSharp.Mat.Eye(3, 3, OpenCvSharp.MatType.CV_64FC1);
            for (int i = 0; i < frameIndices.Count; i++)
            {
                // Forward.
                var mat = identity;
                if (i > 0)
                {
                    mat = forwardTransforms[i - 1] * consecTransforms[i - 1];
                }

                forwardTransforms.Add(mat);
            }

            InvalidateFromMenu(sender);

            // Commit transform data.
            metadata.SetCameraMotion(frameIndices, consecTransforms);
        }

        /// <summary>
        /// Concatenate two affine matrices, where 
        /// - a is already a 3x3 matrix of CV_64FC1, 
        /// - b is a 2x3 matrix from OpenCV estimate affine 2D, also of CV_64FC1.
        /// </summary>
        private OpenCvSharp.Mat ConcatAffine(OpenCvSharp.Mat a, OpenCvSharp.Mat b)
        {
            OpenCvSharp.Mat temp = OpenCvSharp.Mat.Eye(3, 3, OpenCvSharp.MatType.CV_64FC1);
            b.Row(0).CopyTo(temp.Row(0));
            b.Row(1).CopyTo(temp.Row(1));

            var result = a * temp;
            temp.Dispose();

            return result;
        }

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

            if (frameIndices[timestamp] >= forwardTransforms.Count)
                return;
            
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
