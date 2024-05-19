using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Kinovea.ScreenManager.Languages;
using Kinovea.Video;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Lens calibration subsystem.
    /// The goal of this filter is to estimate the intrinsics parameters of a camera.
    /// </summary>
    public class VideoFilterLensCalibration : IVideoFilter
    {
        #region Properties
        public VideoFilterType Type
        {
            get { return VideoFilterType.LensCalibration; }
        }
        public string FriendlyNameResource
        {
            get { return "filterName_LensCalibration"; }
        }

        public Bitmap Current
        {
            get { return null; }
        }
        public bool HasContextMenu
        {
            get { return true; }
        }
        public bool RotatedCanvas
        {
            get { return false; }
        }
        public bool DrawAttachedDrawings
        {
            // Don't draw the normal drawings, this is a technical filter, it is not 
            // supposed to be used as a playback mode.
            get { return false; }
        }
        public bool DrawDetachedDrawings
        {
            get { return false; }
        }
        public bool CanExportVideo
        {
            // It could be interesting to export the undistorted video from here.
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
        private Metadata parentMetadata;
        private Stopwatch stopwatch = new Stopwatch();
        // frameIndices: reverse index from timestamps to frames indices.
        private Dictionary<long, int> frameIndices = new Dictionary<long, int>();
        private List<List<OpenCvSharp.Point2f>> imagePoints = new List<List<OpenCvSharp.Point2f>>();

        // Display parameters
        private bool showCorners = false;
        //private bool showInliers = true;        // Features matched and used to estimate the final motion.
        //private bool showOutliers = false;      // Features matched but not used to estimate the final motion. 
        //private bool showTransforms = true;     // Frame transforms.

        #region Menu
        private ToolStripMenuItem mnuAction = new ToolStripMenuItem();
        private ToolStripMenuItem mnuRun = new ToolStripMenuItem();
        //private ToolStripMenuItem mnuImportMask = new ToolStripMenuItem();
        //private ToolStripMenuItem mnuImportColmap = new ToolStripMenuItem();
        //private ToolStripMenuItem mnuDeleteData = new ToolStripMenuItem();

        private ToolStripMenuItem mnuOptions = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowCorners = new ToolStripMenuItem();
        //private ToolStripMenuItem mnuShowOutliers = new ToolStripMenuItem();
        //private ToolStripMenuItem mnuShowInliers = new ToolStripMenuItem();
        //private ToolStripMenuItem mnuShowTransforms = new ToolStripMenuItem();
        #endregion

        // Decoration
        private Pen penCorner = new Pen(Color.Lime, 2.0f);
        //private Pen penFeatureOutlier = new Pen(Color.Red, 2.0f);
        //private Pen penMatchInlier = new Pen(Color.LimeGreen, 2.0f);
        //private Pen penMatchOutlier = new Pen(Color.FromArgb(128, 255, 0, 0), 2.0f);
        //private int maxTransformsFrames = 25;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region ctor/dtor
        public VideoFilterLensCalibration(Metadata metadata)
        {
            this.parentMetadata = metadata;
            InitializeMenus();
        }

        ~VideoFilterLensCalibration()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases all resources used by this video filter.
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
                //tracker.Dispose();
            }
        }

        private void InitializeMenus()
        {
            mnuAction.Image = Properties.Resources.action;
            mnuRun.Image = Properties.Drawings.trackingplay;
            //mnuDeleteData.Image = Properties.Resources.bin_empty;
            mnuRun.Click += MnuRun_Click;
            //mnuDeleteData.Click += MnuDeleteData_Click;
            mnuAction.DropDownItems.AddRange(new ToolStripItem[] {
                mnuRun,
                //new ToolStripSeparator(),
                //mnuDeleteData,
            });

            mnuOptions.Image = Properties.Resources.equalizer;
            mnuShowCorners.Image = Properties.Drawings.bullet_green;
            //mnuShowOutliers.Image = Properties.Drawings.bullet_red;
            mnuShowCorners.Click += MnuShowCorners_Click;
            //mnuShowOutliers.Click += MnuShowOutliers_Click;
            //mnuShowInliers.Click += MnuShowInliers_Click;
            //mnuShowTransforms.Click += MnuShowTransforms_Click;
            mnuOptions.DropDownItems.AddRange(new ToolStripItem[] {
                mnuShowCorners,
                //mnuShowInliers,
                //mnuShowOutliers,
                //mnuShowTransforms,
            });
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

        public void UpdateTimeOrigin(long timestamp)
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

        public void Scroll(int steps, PointF p, Keys modifiers)
        {
        }

        /// <summary>
        /// Draw extra content on top of the produced image.
        /// </summary>
        public void DrawExtra(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer, long timestamp, bool export)
        {
            if (showCorners)
                DrawCorners(canvas, transformer, timestamp);
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
            //tracker.ResetTrackingData();
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

        #region Context menu

        /// <summary>
        /// Get the context menu according to the mouse position, current time and locale.
        /// </summary>
        public List<ToolStripItem> GetContextMenu(PointF pivot, long timestamp)
        {
            List<ToolStripItem> contextMenu = new List<ToolStripItem>();
            ReloadMenusCulture();

            contextMenu.AddRange(new ToolStripItem[] {
                mnuAction,
                mnuOptions,
            });

            mnuShowCorners.Checked = showCorners;

            return contextMenu;
        }

        private void ReloadMenusCulture()
        {
            mnuAction.Text = ScreenManagerLang.mnuAction;
            mnuRun.Text = "Run lens calibration";

            mnuOptions.Text = ScreenManagerLang.Generic_Options;
            mnuShowCorners.Text = "Show corners";
        }

        private void MnuRun_Click(object sender, EventArgs e)
        {
            if (framesContainer == null || framesContainer.Frames == null || framesContainer.Frames.Count < 1)
                return;

            // Perform the actual lens calibration.
            // TODO: use a progress bar.

            stopwatch.Start();

            // http://docs.opencv.org/2.4/modules/calib3d/doc/camera_calibration_and_3d_reconstruction.html#calibratecamera

            int cbWidth = 9;
            int cbHeight = 6;

            // World space position of the corners.
            // The actual world dimension doesn't matter.
            List<OpenCvSharp.Point3f> points = new List<OpenCvSharp.Point3f>(cbHeight * cbWidth);
            for (int i = 0; i < cbHeight; i++)
            {
                for (int j = 0; j < cbWidth; j++)
                {
                    points.Add(new OpenCvSharp.Point3f(j, i, 0));
                }
            }

            OpenCvSharp.Size patternSize = new OpenCvSharp.Size(cbWidth, cbHeight);

            // Find corners in images.
            // FIXME: limit to 15 images equally spaced in the collection.
            frameIndices.Clear();
            imagePoints.Clear();

            List<List<OpenCvSharp.Point3f>> objectPoints = new List<List<OpenCvSharp.Point3f>>();
            foreach (var f in framesContainer.Frames)
            {
                // Convert image to OpenCV and convert to grayscale.
                var cvImage = OpenCvSharp.Extensions.BitmapConverter.ToMat(f.Image);
                var cvImageGray = new OpenCvSharp.Mat();
                OpenCvSharp.Cv2.CvtColor(cvImage, cvImageGray, OpenCvSharp.ColorConversionCodes.BGR2GRAY, 0);
                cvImage.Dispose();

                // Find checkerboard corners in the image.
                var corners = new OpenCvSharp.Mat<OpenCvSharp.Point2f>();
                var flags = OpenCvSharp.ChessboardFlags.AdaptiveThresh | OpenCvSharp.ChessboardFlags.FastCheck | OpenCvSharp.ChessboardFlags.NormalizeImage;
                bool found = OpenCvSharp.Cv2.FindChessboardCorners(cvImageGray, patternSize, corners, flags);
                if (!found)
                {
                    cvImageGray.Dispose();
                    continue;
                }

                // TODO: Refine the corner positions.

                // Collect the point correspondances.
                frameIndices.Add(f.Timestamp, imagePoints.Count);
                objectPoints.Add(points);
                imagePoints.Add(corners.ToArray().ToList());
            }

            DistortionParameters calib = Calibrate(objectPoints, imagePoints);
            InvalidateFromMenu(sender);

            // Commit intrinsics data.
            //parentMetadata.SetLensCalibration(calib);
        }

        private DistortionParameters Calibrate(List<List<OpenCvSharp.Point3f>> objectPoints, List<List<OpenCvSharp.Point2f>> imagePoints)
        {
            // TODO: move this back in CameraCalibrator.
            // Rename CameraCalibrator to LensCalibrator or something.
            
            double[,] mat = new double[3, 3];
            double[] dist = new double[5];
            OpenCvSharp.CalibrationFlags flags = OpenCvSharp.CalibrationFlags.RationalModel;
            var termCriteriaType = OpenCvSharp.CriteriaTypes.MaxIter | OpenCvSharp.CriteriaTypes.Eps;
            int maxIter = 30;
            float eps = 0.001f;
            var termCriteria = new OpenCvSharp.TermCriteria(termCriteriaType, maxIter, eps);

            OpenCvSharp.Cv2.CalibrateCamera(
                objectPoints,
                imagePoints,
                new OpenCvSharp.Size(frameSize.Width, frameSize.Height),
                mat,
                dist,
                out var rotationVectors,
                out var translationVectors,
                flags,
                termCriteria
            );

            double k1 = dist[0];
            double k2 = dist[1];
            double k3 = dist[4];
            double p1 = dist[2];
            double p2 = dist[3];
            double fx = mat[0, 0];
            double fy = mat[1, 1];
            double cx = mat[0, 2];
            double cy = mat[1, 2];

            var parameters = new DistortionParameters(k1, k2, k3, p1, p2, fx, fy, cx, cy, frameSize);
            log.DebugFormat("Distortion coefficients: k1:{0:0.000}, k2:{1:0.000}, k3:{2:0.000}, p1:{3:0.000}, p2:{4:0.000}.", k1, k2, k3, p1, p2);
            log.DebugFormat("Camera intrinsics: fx:{0:0.000}, fy:{1:0.000}, cx:{2:0.000}, cy:{3:0.000}", fx, fy, cx, cy);

            return parameters;
        }

        private void MnuDeleteData_Click(object sender, EventArgs e)
        {
            //CaptureMemento();
            //tracker.ResetTrackingData();
            //Update();
            InvalidateFromMenu(sender);
        }

        private void MnuShowCorners_Click(object sender, EventArgs e)
        {
            //CaptureMemento();

            showCorners = !mnuShowCorners.Checked;

            //Update();
            InvalidateFromMenu(sender);
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
        #endregion

        #region Rendering

        /// <summary>
        /// Draw a dot on each corner.
        /// </summary>
        private void DrawCorners(Graphics canvas, IImageToViewportTransformer transformer, long timestamp)
        {
            if (imagePoints.Count == 0)
                return;

            if (!frameIndices.ContainsKey(timestamp) || frameIndices[timestamp] >= imagePoints.Count)
                return;

            List<PointF> corners = new List<PointF>();
            foreach (var p in imagePoints[frameIndices[timestamp]])
            {
                corners.Add(new PointF(p.X, p.Y));
            }

            if (corners.Count == 0)
                return;

            foreach (var corner in corners)
            {
                PointF p = transformer.Transform(corner);
                canvas.DrawEllipse(penCorner, p.Box(2));
            }
        }

        #endregion

    }
}
