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
using System.Web;
using SpreadsheetLight.Charts;
using System.ComponentModel;
using System.Threading;

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
            get { return bitmap; }
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
        private Bitmap bitmap;
        private Size frameSize;
        private IWorkingZoneFramesContainer framesContainer;
        private Metadata parentMetadata;
        private Stopwatch stopwatch = new Stopwatch();
        private long activeTimestamp;
        // frameIndices: reverse index from timestamps to frames indices,
        // for the images actually used in the calibration (found corners).
        private Dictionary<long, int> frameIndices = new Dictionary<long, int>();
        private List<List<OpenCvSharp.Point2f>> imagePoints = new List<List<OpenCvSharp.Point2f>>();
        private DistortionParameters calibration;

        // Configuration
        private int maxImages = 12;
        private Size patternSize = new Size(9, 6);
        
        // Display parameters
        private bool showCorners = true;

        #region Menu
        private ToolStripMenuItem mnuAction = new ToolStripMenuItem();
        private ToolStripMenuItem mnuRun = new ToolStripMenuItem();
        private ToolStripMenuItem mnuOptions = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowCorners = new ToolStripMenuItem();
        #endregion

        static string[] colorCycle = new string[] {
            "FF0000", "FF6A00", "FFD800", "4CFF00", "00FFFF", "0094FF", "B200FF", "FF00DC",
        };

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
                if (bitmap != null)
                    bitmap.Dispose();
            }
        }

        private void InitializeMenus()
        {
            mnuAction.Image = Properties.Resources.action;
            mnuRun.Image = Properties.Resources.checkerboard;
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

            // TODO: maybe keep a cache?
            bitmap = framesContainer.Frames[0].Image;
        }
        public void UpdateSize(Size size)
        {
        }

        public void UpdateTime(long timestamp)
        {
            // Bind to the nearest frame used by calibration.
            bitmap = null;
            List<int> indices = GetIndices(maxImages, framesContainer.Frames.Count);
            for (int i = 0; i < indices.Count; i++)
            {
                int index = indices[i];
                if (timestamp >= framesContainer.Frames[index].Timestamp)
                {
                    bitmap = framesContainer.Frames[index].Image;
                    activeTimestamp = framesContainer.Frames[index].Timestamp;
                }
            }
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
            // We do not use the passed timestamp from the player timeline.
            // We are only showing a few selected images, use the timestamp of the active image.
            if (showCorners)
                DrawCorners(canvas, transformer, activeTimestamp);
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

            formProgressBar2 fpb = new formProgressBar2(true, true, Worker_DoWork);
            fpb.ShowDialog();
            fpb.Dispose();

            InvalidateFromMenu(sender);

            // Commit intrinsics data.
            //parentMetadata.SetLensCalibration(calibration);
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // This runs in the background thread.
            Thread.CurrentThread.Name = "LensCalibration";
            BackgroundWorker worker = sender as BackgroundWorker;
            
            stopwatch.Start();

            int cbWidth = patternSize.Width;
            int cbHeight = patternSize.Height;
            OpenCvSharp.Size cbSize = new OpenCvSharp.Size(cbWidth, cbHeight);

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

            // Find corners in images.
            frameIndices.Clear();
            imagePoints.Clear();
            List<List<OpenCvSharp.Point3f>> objectPoints = new List<List<OpenCvSharp.Point3f>>();
            List<int> indices = GetIndices(maxImages, framesContainer.Frames.Count);
            for (int i = 0; i < indices.Count; i++)
            {
                if (worker.CancellationPending)
                    break;

                int index = indices[i];
                var f = framesContainer.Frames[index];

                // Convert image to OpenCV and convert to grayscale.
                var cvImage = OpenCvSharp.Extensions.BitmapConverter.ToMat(f.Image);
                var cvImageGray = new OpenCvSharp.Mat();
                OpenCvSharp.Cv2.CvtColor(cvImage, cvImageGray, OpenCvSharp.ColorConversionCodes.BGR2GRAY, 0);
                cvImage.Dispose();

                // Find checkerboard corners in the image.
                var corners = new OpenCvSharp.Mat<OpenCvSharp.Point2f>();
                var flags = OpenCvSharp.ChessboardFlags.AdaptiveThresh | OpenCvSharp.ChessboardFlags.FastCheck | OpenCvSharp.ChessboardFlags.NormalizeImage;
                bool found = OpenCvSharp.Cv2.FindChessboardCorners(cvImageGray, cbSize, corners, flags);
                if (!found)
                {
                    cvImageGray.Dispose();
                    worker.ReportProgress(index + 1, framesContainer.Frames.Count);
                    continue;
                }
            
                // TODO: Refine the corner positions.

                // Collect the point correspondances.
                frameIndices.Add(f.Timestamp, imagePoints.Count);
                objectPoints.Add(points);
                imagePoints.Add(corners.ToArray().ToList());

                worker.ReportProgress(index + 1, framesContainer.Frames.Count);
            }

            if (worker.CancellationPending)
            {
                log.Debug("Lens calibration cancelled.");
                return;
            }

            log.DebugFormat("Find corners: {0:0.000} s", stopwatch.ElapsedMilliseconds / 1000.0f);

            if (imagePoints.Count < 2)
            {
                log.Debug("Lens calibration failure. Not enough images to calibrate.");
                return;
            }

            // Compute the calibration.
            stopwatch.Restart();
            calibration = Calibrate(objectPoints, imagePoints);
            log.DebugFormat("Calibration: {0:0.000} s", stopwatch.ElapsedMilliseconds / 1000.0f);
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
        /// Draw a circle around each corner and connect them together.
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

            // Paint corners and connector lines.
            int index = 0;
            PointF prevPoint = PointF.Empty;
            for (int j = 0; j < patternSize.Height; j++)
            {
                string str = "FF" + colorCycle[j % colorCycle.Length];
                Color c = Color.FromArgb(Convert.ToInt32(str, 16));
                using (Pen pen = new Pen(c, 3.0f))
                {
                    for (int i = 0; i < patternSize.Width; i++)
                    {
                        PointF p = transformer.Transform(corners[index]);

                        canvas.DrawEllipse(pen, p.Box(5));

                        if (index != 0)
                        {
                            canvas.DrawLine(pen, prevPoint, p);
                        }

                        prevPoint = p;
                        index++;
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Get frame indices for image candidates to use in the calibration.
        /// These are the images we display in this video filter mode
        /// when the user browses the video.
        /// 
        /// The list used for calibration may not contain all of them 
        /// if we can't find the corners in the image.
        /// </summary>
        private List<int> GetIndices(int count, int total)
        {
            List<int> indices = new List<int>();
            if (count >= total)
            {
                for (int i = 0; i < total; i++)
                    indices.Add(i);
            }
            else
            {
                for (int i = 0; i < count; i ++)
                {
                    float u = (float)i / count;
                    indices.Add((int)Math.Round(u * total));
                }
            }

            return indices;
        }
    }
}
