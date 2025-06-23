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
using System.ComponentModel;
using System.Threading;
using System.Text;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Lens calibration estimation.
    /// 
    /// The goal of this filter is to estimate the intrinsics parameters and 
    /// distortion coefficients of the camera that filmed the current video.
    /// The output is an object with camera intrinsics (focal length and central point)
    /// and distortion coefficients (radial and tangential).
    /// 
    /// This is a "technical" or "helper" filter.
    /// The results are sent to DistortionHelper where they are used to
    /// distort/undistort the position of drawing points, and to the CameraPoser to recompute 
    /// the 3D position of the camera.
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
        public bool HasKVAData 
        {
            // This is a technical filter, the result of its execution is sent to the DistortionHelper.
            // Ultimately the DistortionSerializer is responsible for KVA serialization.
            get { return false; }
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
            // This video mode isn't supposed to receive any annotations
            // but it doesn't hurt to draw them.
            get { return true; }
        }
        public bool DrawDetachedDrawings
        {
            get { return true; }
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
        public bool CanExportData
        {
            get { return true; }
        }
        public LensCalibrationParameters Parameters
        {
            get { return parameters; }
            set { parameters = value; }
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
        private long requestTimestamp;  // Last timestamp requested by the player.
        private long activeTimestamp;   // timestamp of the frame we are actually displaying. 
        private bool calibrated = false;
        private string resultText;
        // frameIndices: reverse index from timestamps to frames indices,
        // for the images actually used in the calibration (found corners).
        private Dictionary<long, int> frameIndices = new Dictionary<long, int>();
        private List<List<OpenCvSharp.Point2f>> imagePoints = new List<List<OpenCvSharp.Point2f>>();
        private List<List<OpenCvSharp.Point2f>> reprojPoints = new List<List<OpenCvSharp.Point2f>>();


        // Configuration
        private LensCalibrationParameters parameters = new LensCalibrationParameters();
        private float eps = 0.001f;

        // Calibration results
        private int usedImages;
        private float reprojError;
        private long duration;
        private DistortionParameters calibration;

        // Display parameters
        private bool showDetectedCorners = true;
        private bool showReprojCorners = true;

        #region Menu
        private ToolStripMenuItem mnuConfigure = new ToolStripMenuItem();
        private ToolStripMenuItem mnuAction = new ToolStripMenuItem();
        private ToolStripMenuItem mnuRun = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDeleteData = new ToolStripMenuItem();
        private ToolStripMenuItem mnuOptions = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowDetectedCorners = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowReprojCorners = new ToolStripMenuItem();
        private ToolStripMenuItem mnuCopy = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSave = new ToolStripMenuItem();
        #endregion

        // Painting
        private static string[] colorCycle = new string[] {
            "FF0000", "FF6A00", "FFD800", "4CFF00", "00FFFF", "0094FF", "B200FF", "FF00DC",
        };
        private SolidBrush brushBack = new SolidBrush(Color.FromArgb(192, Color.Black));
        private SolidBrush brushText = new SolidBrush(Color.White);
        private Font fontText = new Font("Consolas", 14, FontStyle.Bold);

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region ctor/dtor
        public VideoFilterLensCalibration(Metadata metadata)
        {
            this.parentMetadata = metadata;
            InitializeMenus();
            parameters = PreferencesManager.PlayerPreferences.LensCalibration;
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
            mnuConfigure.Image = Properties.Drawings.configure;
            mnuConfigure.Click += MnuConfigure_Click;

            mnuAction.Image = Properties.Resources.action;
            mnuRun.Image = Properties.Resources.checkerboard;
            mnuDeleteData.Image = Properties.Resources.bin_empty;
            mnuRun.Click += MnuRun_Click;
            mnuDeleteData.Click += MnuDeleteData_Click;
            mnuAction.DropDownItems.AddRange(new ToolStripItem[] {
                mnuRun,
                new ToolStripSeparator(),
                mnuDeleteData,
            });

            mnuOptions.Image = Properties.Resources.equalizer;
            mnuShowDetectedCorners.Image = Properties.Drawings.bullet_green;
            mnuShowReprojCorners.Image = Properties.Drawings.bullet_orange;
            mnuShowDetectedCorners.Click += MnuShowCorners_Click;
            mnuShowReprojCorners.Click += MnuShowReprojCorners_Click;
            mnuOptions.DropDownItems.AddRange(new ToolStripItem[] {
                mnuShowDetectedCorners,
                mnuShowReprojCorners,
            });

            mnuCopy.Image = Properties.Resources.clipboard_block;
            mnuCopy.Click += MnuCopy_Click;
            mnuSave.Image = Properties.Resources.save_calibration;
            mnuSave.Click += MnuSave_Click;
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
            requestTimestamp = timestamp;
            bitmap = null;
            List<int> indices = GetIndices(parameters.MaxImages, framesContainer.Frames.Count);
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
            if (calibrated && (showDetectedCorners || showReprojCorners))
            {
                // We do not use the passed timestamp from the player timeline.
                // We are only showing a few selected images, use the timestamp of the active image.
                if (showDetectedCorners)
                    DrawCorners(canvas, imagePoints, transformer, activeTimestamp);

                if (showReprojCorners)
                    DrawCorners(canvas, reprojPoints, transformer, activeTimestamp);
            }

            DrawResults(canvas);
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
            calibrated = false;
            frameIndices.Clear();
            imagePoints.Clear();
        }
        public void WriteData(XmlWriter w)
        {
            // This filter does not save anything to KVA files.
        }

        public void ReadData(XmlReader r)
        {
            // This filter does not load anything from KVA files.
            // Configuration parameters are loaded from the preferences.
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
                mnuConfigure,
                new ToolStripSeparator(),
                mnuAction,
                mnuOptions,
            });

            mnuShowDetectedCorners.Checked = showDetectedCorners;
            mnuShowReprojCorners.Checked = showReprojCorners;
            mnuDeleteData.Enabled = calibrated;
            mnuCopy.Enabled = calibrated;
            mnuSave.Enabled = calibrated;

            return contextMenu;
        }

        public List<ToolStripItem> GetExportDataMenu()
        {
            return new List<ToolStripItem>() { mnuCopy, mnuSave };
        }

        private void ReloadMenusCulture()
        {
            // Just in time localization.
            mnuConfigure.Text = ScreenManagerLang.Generic_ConfigurationElipsis;

            mnuAction.Text = ScreenManagerLang.mnuAction;
            mnuRun.Text = ScreenManagerLang.VideoFilterLensCalibration_RunLensCalibration;
            mnuDeleteData.Text = ScreenManagerLang.VideoFilterLensCalibration_DeleteCalibrationData;

            mnuOptions.Text = ScreenManagerLang.Generic_Options;
            mnuShowDetectedCorners.Text = ScreenManagerLang.VideoFilterLensCalibration_ShowDetectedCorners;
            mnuShowReprojCorners.Text = ScreenManagerLang.VideoFilterLensCalibration_ShowReprojectedCorners;

            mnuCopy.Text = ScreenManagerLang.VideoFilterLensCalibration_CopyCalibrationData;
            mnuSave.Text = ScreenManagerLang.VideoFilterLensCalibration_SaveCalibrationData;
        }

        private void MnuConfigure_Click(object sender, EventArgs e)
        {
            // The dialog is responsible for handling undo/redo.

            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            if (tsmi == null)
                return;

            int oldMaxImages = parameters.MaxImages;
            Size oldSize = parameters.PatternSize;

            FormConfigureLensCalibration fclc = new FormConfigureLensCalibration(this);
            FormsHelper.Locate(fclc);
            fclc.ShowDialog();

            if (fclc.DialogResult == DialogResult.OK)
            {
                SaveAsDefaultParameters();

                // If we changed the frame number or the pattern size we can't 
                // draw the old results correctly anymore so reset the calibration.
                if (oldMaxImages != parameters.MaxImages || oldSize != parameters.PatternSize)
                {
                    ResetData();
                    UpdateTime(requestTimestamp);
                }

            }

            fclc.Dispose();
            InvalidateFromMenu(sender);
        }

        private void MnuRun_Click(object sender, EventArgs e)
        {
            if (framesContainer == null || framesContainer.Frames == null || framesContainer.Frames.Count < 1)
                return;

            calibrated = false;

            formProgressBar2 fpb = new formProgressBar2(true, true, Worker_DoWork);
            fpb.ShowDialog();
            fpb.Dispose();

            if (calibrated)
                parentMetadata.SetLensCalibration(calibration);

            InvalidateFromMenu(sender);
        }

        private void MnuCopy_Click(object sender, EventArgs e)
        {
            if (!calibrated || calibration == null)
                return;

            Clipboard.SetText(resultText);
        }

        private void MnuSave_Click(object sender, EventArgs e)
        {
            if (!calibrated || calibration == null)
                return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = ScreenManagerLang.dlgCameraCalibration_SaveDialogTitle;
            saveFileDialog.Filter = FilesystemHelper.SaveXMLFilter();
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.InitialDirectory = Software.CameraCalibrationDirectory;

            if (saveFileDialog.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(saveFileDialog.FileName))
                return;

            DistortionImporterKinovea.Export(saveFileDialog.FileName, calibration, frameSize);
        }

        private void MnuDeleteData_Click(object sender, EventArgs e)
        {
            ResetData();
            InvalidateFromMenu(sender);
        }

        private void MnuShowCorners_Click(object sender, EventArgs e)
        {
            showDetectedCorners = !mnuShowDetectedCorners.Checked;
            InvalidateFromMenu(sender);
        }

        private void MnuShowReprojCorners_Click(object sender, EventArgs e)
        {
            showReprojCorners = !mnuShowReprojCorners.Checked;
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
        private void DrawCorners(Graphics canvas, List<List<OpenCvSharp.Point2f>> points, IImageToViewportTransformer transformer, long timestamp)
        {
            if (!calibrated || points.Count == 0)
                return;

            if (!frameIndices.ContainsKey(timestamp) || frameIndices[timestamp] >= points.Count)
                return;

            List<PointF> corners = new List<PointF>();
            foreach (var p in points[frameIndices[timestamp]])
            {
                corners.Add(new PointF(p.X, p.Y));
            }

            if (corners.Count == 0)
                return;

            // Paint corners and connector lines.
            int index = 0;
            PointF prevPoint = PointF.Empty;
            for (int j = 0; j < parameters.PatternSize.Height - 1; j++)
            {
                string str = "FF" + colorCycle[j % colorCycle.Length];
                Color c = Color.FromArgb(Convert.ToInt32(str, 16));
                using (Pen pen = new Pen(c, 2.0f))
                {
                    for (int i = 0; i < parameters.PatternSize.Width - 1; i++)
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

        private void DrawResults(Graphics canvas)
        {
            string text = calibrated ? resultText : GetDefaultString();
            
            // We don't care about the original image size, we draw in screen space.
            SizeF textSize = canvas.MeasureString(text, fontText);
            Point bgLocation = new Point(20, 20);
            Size bgSize = new Size((int)textSize.Width, (int)textSize.Height);

            // Background rounded rectangle.
            Rectangle rect = new Rectangle(bgLocation, bgSize);
            int roundingRadius = fontText.Height / 4;
            RoundedRectangle.Draw(canvas, rect, brushBack, roundingRadius, false, false, null);

            // Main text.
            canvas.DrawString(text, fontText, brushText, rect.Location);
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

        /// <summary>
        /// Background worker function that finds corners in the images
        /// and calls the calibration.
        /// </summary>
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // This runs in the background thread.
            Thread.CurrentThread.Name = "LensCalibration";
            BackgroundWorker worker = sender as BackgroundWorker;

            stopwatch.Restart();

            // https://www.opencv.org.cn/opencvdoc/2.3.2/html/modules/calib3d/doc/camera_calibration_and_3d_reconstruction.html
            // OpenCV will only look for the internal corners so subtract one from the number of squares.
            int cbWidth = parameters.PatternSize.Width - 1;
            int cbHeight = parameters.PatternSize.Height - 1;
            OpenCvSharp.Size cbSize = new OpenCvSharp.Size(cbWidth, cbHeight);

            // World space position of the corners.
            // The actual world dimension doesn't matter.
            List<OpenCvSharp.Point3f> points = new List<OpenCvSharp.Point3f>(cbWidth * cbHeight);
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
            reprojPoints.Clear();
            List<List<OpenCvSharp.Point3f>> objectPoints = new List<List<OpenCvSharp.Point3f>>();
            List<int> indices = GetIndices(parameters.MaxImages, framesContainer.Frames.Count);
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

            long findCornersTime = stopwatch.ElapsedMilliseconds;

            log.DebugFormat("Find corners: {0:0.000} s", findCornersTime / 1000.0f);

            if (imagePoints.Count < 2)
            {
                log.Debug("Lens calibration failure. Not enough images to calibrate.");
                return;
            }

            // Compute the calibration.
            stopwatch.Restart();
            Calibrate(objectPoints, imagePoints, parameters.MaxIterations, eps);
            long calibrationTime = stopwatch.ElapsedMilliseconds;
            log.DebugFormat("Calibration: {0:0.000} s", calibrationTime / 1000.0f);
            duration = findCornersTime + calibrationTime;
            resultText = GetResultsString();
        }

        /// <summary>
        /// Call OpenCV calibration with the collected corners.
        /// </summary>
        private void Calibrate(List<List<OpenCvSharp.Point3f>> objectPoints, List<List<OpenCvSharp.Point2f>> imagePoints, int maxIterations, float eps)
        {
            // This still runs in the background thread.
            
            double[,] cameraMatrix = new double[3, 3];
            double[] distCoeffs = new double[5];
            OpenCvSharp.CalibrationFlags flags = OpenCvSharp.CalibrationFlags.None;
            var termCriteriaType = OpenCvSharp.CriteriaTypes.MaxIter | OpenCvSharp.CriteriaTypes.Eps;
            var termCriteria = new OpenCvSharp.TermCriteria(termCriteriaType, maxIterations, eps);

            OpenCvSharp.Vec3d[] rotationVectors;
            OpenCvSharp.Vec3d[] translationVectors;

            double error = OpenCvSharp.Cv2.CalibrateCamera(
                objectPoints,
                imagePoints,
                new OpenCvSharp.Size(frameSize.Width, frameSize.Height),
                cameraMatrix,
                distCoeffs,
                out rotationVectors,
                out translationVectors,
                flags,
                termCriteria
            );

            double k1 = distCoeffs[0];
            double k2 = distCoeffs[1];
            double k3 = distCoeffs[4];
            double p1 = distCoeffs[2];
            double p2 = distCoeffs[3];
            double fx = cameraMatrix[0, 0];
            double fy = cameraMatrix[1, 1];
            double cx = cameraMatrix[0, 2];
            double cy = cameraMatrix[1, 2];

            log.DebugFormat("Reprojection err: {0:0.000}", error);
            log.DebugFormat("Camera intrinsics: fx:{0:0.000}, fy:{1:0.000}, cx:{2:0.000}, cy:{3:0.000}", fx, fy, cx, cy);
            float hfov = (float)(2 * Math.Atan(frameSize.Width / (2 * fx)) * 180 / Math.PI);
            log.DebugFormat("HFOV: {0:0.000}", hfov);
            log.DebugFormat("Coefficients: k1:{0:0.000}, k2:{1:0.000}, k3:{2:0.000}, p1:{3:0.000}, p2:{4:0.000}.", k1, k2, k3, p1, p2);

            usedImages = imagePoints.Count;
            reprojError = (float)error;
            calibration = new DistortionParameters(k1, k2, k3, p1, p2, fx, fy, cx, cy, frameSize);
            calibrated = true;

            // Now compute the reprojection error for each image.
            // Based on https://docs.opencv.org/4.x/d4/d94/tutorial_camera_calibration.html
            for (int i = 0; i < objectPoints.Count; i++)
            {
                // Convert types and project the object points on this image.
                double[] rvec = new double[3] { rotationVectors[i][0], rotationVectors[i][1], rotationVectors[i][2] };
                double[] tvec = new double[3] { translationVectors[i][0], translationVectors[i][1], translationVectors[i][2] };
                OpenCvSharp.Point2f[] imagePoints2;
                double[,] jacobian = new double[3, 3];
                OpenCvSharp.Cv2.ProjectPoints(objectPoints[i], rvec, tvec, cameraMatrix, distCoeffs, out imagePoints2, out jacobian);

                // Store the reprojected points for visualization.
                reprojPoints.Add(new List<OpenCvSharp.Point2f>(imagePoints2));

                // Compute the reprojection error.
                
                OpenCvSharp.Mat ipMat = OpenCvSharp.Mat.FromPixelData(1, imagePoints[i].Count, OpenCvSharp.MatType.CV_32FC2, imagePoints[i].ToArray());
                OpenCvSharp.Mat ip2Mat = OpenCvSharp.Mat.FromPixelData(1, imagePoints2.Length, OpenCvSharp.MatType.CV_32FC2, imagePoints2.ToArray());
                double err = OpenCvSharp.Cv2.Norm(ipMat, ip2Mat, OpenCvSharp.NormTypes.L2SQR);
                err = Math.Sqrt(err / objectPoints[i].Count);

                log.DebugFormat("Reprojection error for frame [{0}]: {1:0.000}", i, err);
                ipMat.Dispose();
                ip2Mat.Dispose();
            }
        }

        /// <summary>
        /// Return calibration results as text.
        /// </summary>
        private string GetResultsString()
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine(string.Format("Lens calibration ({0:0.000} s)", duration / 1000.0f));
            b.AppendLine(string.Format("Pattern: {0}x{1}, Image size:{2}x{3}", parameters.PatternSize.Width, parameters.PatternSize.Height, frameSize.Width, frameSize.Height));
            b.AppendLine(string.Format("Images:{0}/{1}.", usedImages, parameters.MaxImages));
            b.AppendLine(string.Format("Reprojection error: {0:0.000}", reprojError));
            b.AppendLine(string.Format("Intrinsics: fx:{0:0.000}, fy:{1:0.000}, cx:{2:0.000}, cy:{3:0.000}", calibration.Fx, calibration.Fy, calibration.Cx, calibration.Cy));
            b.AppendLine(string.Format("Radial distortion: k1:{0:0.000}, k2:{1:0.000}, k3:{2:0.000}", calibration.K1, calibration.K2, calibration.K3));
            b.AppendLine(string.Format("Tangential distortion: p1:{0:0.000}, p2:{1:0.000}", calibration.P1, calibration.P2));
            return b.ToString();
        }

        /// <summary>
        /// Return the string to show when calibration hasn't been completed.
        /// </summary>
        private string GetDefaultString()
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine(string.Format("Lens calibration"));
            b.AppendLine(string.Format("Pattern: {0}x{1}, Image size:{2}x{3}", parameters.PatternSize.Width, parameters.PatternSize.Height, frameSize.Width, frameSize.Height));
            return b.ToString();
        }

        /// <summary>
        /// Save the configuration as the new preferred configuration.
        /// </summary>
        private void SaveAsDefaultParameters()
        {
            PreferencesManager.PlayerPreferences.LensCalibration = parameters.Clone();
            PreferencesManager.Save();
        }
    }
}
