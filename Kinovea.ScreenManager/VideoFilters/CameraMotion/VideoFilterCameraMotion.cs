using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.ComponentModel;
using System.Threading;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Camera motion estimation.
    /// 
    /// The goal of this filter is to estimate the global motion of the camera 
    /// over the sequence of frames in the working zone.
    /// The output is a series of transforms that can be used to calculate 
    /// the position of points in any frame based on their position in a reference frame.
    /// The low level work is done by the CameraTracker class.
    /// 
    /// This is a "technical" or "helper" filter.
    /// The results are sent to CameraTransformer where they are used
    /// to transform the position of drawings points.
    /// </summary>
    public class VideoFilterCameraMotion : IVideoFilter
    {
        #region Properties
        public VideoFilterType Type
        {
            get { return VideoFilterType.CameraMotion; }
        }
        public string FriendlyNameResource
        {
            get { return "filterName_CameraMotion"; }
        }
        public Bitmap Current
        {
            get { return null; }
        }
        public bool HasKVAData
        {
            // This is a technical filter, the result of its execution
            // are sent to the CameraTransformer which is responsible 
            // for KVA serialization.
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
            // Allow drawings in this mode.
            // This lets us immediately validate the quality of the tracking.
            get { return true; }
        }
        public bool DrawDetachedDrawings
        {
            get { return false; }
        }
        public bool CanExportVideo
        {
            get { return false; }
        }
        public bool CanExportImage
        {
            get { return false; }
        }
        public bool CanExportData
        {
            get { return false; }
        }
        public CameraMotionParameters Parameters
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
        private Size frameSize;
        private IWorkingZoneFramesContainer framesContainer;
        private Metadata parentMetadata;
        private Stopwatch stopwatch = new Stopwatch();
        private Random rnd = new Random();
        private CameraTracker tracker;
        private CameraMotionStep step = CameraMotionStep.All;

        // Configuration
        private CameraMotionParameters parameters;

        // Display parameters
        private bool showFeatures = false;      // All the features found.
        private bool showInliers = true;        // Features matched and used to estimate the final motion.
        private bool showOutliers = false;      // Features matched but not used to estimate the final motion. 
        private bool showMotionField = false;   // Field of motion vectors.
        private bool showTransforms = false;    // Frame transforms.
        private bool showTracks = false;        // Features tracked over multiple frames.

        #region Menu
        private ToolStripMenuItem mnuConfigure = new ToolStripMenuItem();

        private ToolStripMenuItem mnuAction = new ToolStripMenuItem();
        private ToolStripMenuItem mnuRunAll = new ToolStripMenuItem();
        private ToolStripMenuItem mnuFindFeatures = new ToolStripMenuItem();
        private ToolStripMenuItem mnuMatchFeatures = new ToolStripMenuItem();
        private ToolStripMenuItem mnuFindHomographies = new ToolStripMenuItem();
        private ToolStripMenuItem mnuBundleAdjustment = new ToolStripMenuItem();
        private ToolStripMenuItem mnuBuildTracks = new ToolStripMenuItem();

        private ToolStripMenuItem mnuDeleteData = new ToolStripMenuItem();

        private ToolStripMenuItem mnuOptions = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowFeatures = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowOutliers = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowInliers = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowMotionField = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowTransforms = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowTracks = new ToolStripMenuItem();

        #endregion

        // Decoration
        private Pen penFeature = new Pen(Color.Yellow, 2.0f);
        private Pen penFeatureOutlier = new Pen(Color.Red, 2.0f);
        private Pen penFeatureInlier = new Pen(Color.Lime, 2.0f);
        private Pen penMatchInlier = new Pen(Color.LimeGreen, 2.0f);
        private Pen penMatchOutlier = new Pen(Color.FromArgb(128, 255, 0, 0), 2.0f);
        private Pen penMotionField = new Pen(Color.CornflowerBlue, 3.0f);
        private Pen penTracks = new Pen(Color.Fuchsia, 2.0f);
        private int maxAgeTransformFrames = 25;         // Age of the oldest frame to show in the transforms visualization.
        private bool showSingleTransformFrame = false;  // If true we only show the transform of the oldest.
        private float penWidthTransformFrames = 1.0f;
        private int motionFieldPoints = 25;             // Number of points in each dimension for the motion field visualization.

        // Precomputed list of unique colors to draw frame references.
        // https://stackoverflow.com/questions/309149/generate-distinctly-different-rgb-colors-in-graphs
        static string[] colorCycle = new string[] {
            "00FF00", "0000FF", "FF0000", "01FFFE", "FFA6FE", "FFDB66", "006401", "010067", "95003A", "007DB5", "FF00F6",
            "FFEEE8", "774D00", "90FB92", "0076FF", "D5FF00", "FF937E", "6A826C", "FF029D", "FE8900", "7A4782", "7E2DD2",
            "85A900", "FF0056", "A42400", "00AE7E", "683D3B", "BDC6FF", "263400", "BDD393", "00B917", "9E008E", "001544",
            "C28C9F", "FF74A3", "01D0FF", "004754", "E56FFE", "788231", "0E4CA1", "91D0CB", "BE9970", "968AE8", "BB8800",
            "43002C", "DEFF74", "00FFC6", "FFE502", "620E00", "008F9C", "98FF52", "7544B1", "B500FF", "00FF78", "FF6E41",
            "005F39", "6B6882", "5FAD4E", "A75740", "A5FFD2", "FFB167", "009BFF", "E85EBE",
        };
        private SolidBrush brushBack = new SolidBrush(Color.FromArgb(192, Color.Black));
        private SolidBrush brushText = new SolidBrush(Color.White);
        private Font fontText = new Font("Consolas", 14, FontStyle.Bold);

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region ctor/dtor
        public VideoFilterCameraMotion(Metadata metadata)
        {
            this.parentMetadata = metadata;
            parameters = PreferencesManager.PlayerPreferences.CameraMotionParameters;
            tracker = new CameraTracker(parameters);
            InitializeMenus();
        }

        ~VideoFilterCameraMotion()
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
                tracker.Dispose();
            }
        }

        private void InitializeMenus()
        {
            mnuConfigure.Image = Properties.Drawings.configure;
            mnuConfigure.Click += MnuConfigure_Click;

            mnuAction.Image = Properties.Resources.action;
            mnuRunAll.Image = Properties.Resources.motion_detector;
            mnuFindFeatures.Image = Properties.Drawings.bullet_orange;
            mnuMatchFeatures.Image = Properties.Drawings.bullet_green;
            mnuFindHomographies.Image = Properties.Resources.frame_transforms2;
            mnuBuildTracks.Image = Properties.Resources.bullet_pink;
            mnuDeleteData.Image = Properties.Resources.bin_empty;
            mnuRunAll.Click += MnuRunAll_Click;
            mnuFindFeatures.Click += MnuFindFeatures_Click;
            mnuMatchFeatures.Click += MnuMatchFeatures_Click;
            mnuFindHomographies.Click += MnuFindHomographies_Click;
            mnuBundleAdjustment.Click += MnuBundleAdjustment_Click;
            mnuBuildTracks.Click += MnuBuildTracks_Click;
            
            mnuDeleteData.Click += MnuDeleteData_Click;
            
            mnuOptions.Image = Properties.Resources.equalizer;
            mnuShowFeatures.Image = Properties.Drawings.bullet_orange;
            mnuShowInliers.Image = Properties.Drawings.bullet_green;
            mnuShowOutliers.Image = Properties.Drawings.bullet_red;
            mnuShowTracks.Image = Properties.Resources.bullet_pink;
            mnuShowTransforms.Image = Properties.Resources.frame_transforms2;
            mnuShowMotionField.Image = Properties.Resources.motion_vectors;
            mnuShowFeatures.Click += MnuShowFeatures_Click;
            mnuShowOutliers.Click += MnuShowOutliers_Click;
            mnuShowInliers.Click += MnuShowInliers_Click;
            mnuShowMotionField.Click += MnuShowMotionField_Click;
            mnuShowTransforms.Click += MnuShowTransforms_Click;
            mnuShowTracks.Click += MnuShowTracks_Click;
            mnuOptions.DropDownItems.AddRange(new ToolStripItem[] {
                mnuShowFeatures,
                mnuShowInliers,
                mnuShowOutliers,
                mnuShowMotionField,
                mnuShowTransforms,
                mnuShowTracks,
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
            if (showFeatures)
                DrawFeatures(canvas, transformer, timestamp);

            if (showOutliers || showInliers)
                DrawMatches(canvas, transformer, timestamp);

            if (showTransforms)
                DrawTransforms(canvas, transformer, timestamp);

            if (showMotionField)
                DrawMotionField(canvas, transformer, timestamp);

            if (showTracks)
                DrawTracks(canvas, transformer, timestamp);

            DrawResults(canvas, timestamp);
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
            tracker.ResetTrackingData();
        }
        public void WriteData(XmlWriter w)
        {
            // This filter does not save anything to KVA files.
        }

        public void ReadData(XmlReader r)
        {
            // This filter does not load anything from KVA files.
            // Configuration parameters are loaded from the preferences.
            // Camera transforms are loaded as part of CameraTransformer.
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

            // The content of the action menu depends on whether we are 
            // running each step individually or all at once.
            // step-by-step is useful for troubleshooting.
            mnuAction.DropDownItems.Clear();

            mnuAction.DropDownItems.Add(mnuRunAll);

            if (tracker.Parameters.StepByStep)
            {
                mnuAction.DropDownItems.Add(new ToolStripSeparator());
                mnuAction.DropDownItems.Add(mnuFindFeatures);
                mnuAction.DropDownItems.Add(mnuMatchFeatures);
                mnuAction.DropDownItems.Add(mnuFindHomographies);
                mnuAction.DropDownItems.Add(mnuBundleAdjustment);
                mnuAction.DropDownItems.Add(mnuBuildTracks);
            }
            
            mnuAction.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripSeparator(),
                mnuDeleteData,
            });

            contextMenu.AddRange(new ToolStripItem[] {
                mnuConfigure,
                new ToolStripSeparator(),
                mnuAction,
                mnuOptions,
            });

            mnuShowFeatures.Checked = showFeatures;
            mnuShowInliers.Checked = showInliers;
            mnuShowOutliers.Checked = showOutliers;
            mnuShowMotionField.Checked = showMotionField;
            mnuShowTransforms.Checked = showTransforms;
            mnuShowTracks.Checked = showTracks;

            return contextMenu;
        }

        public List<ToolStripItem> GetExportDataMenu()
        {
            throw new NotImplementedException();
        }

        private void ReloadMenusCulture()
        {
            mnuConfigure.Text = ScreenManagerLang.Generic_ConfigurationElipsis;

            mnuAction.Text = ScreenManagerLang.mnuAction;
            mnuRunAll.Text = ScreenManagerLang.VideoFilterCameraMotion_RunCameraMotionEstimation;
            mnuFindFeatures.Text = ScreenManagerLang.VideoFilterCameraMotion_FindFeatures;
            mnuMatchFeatures.Text = ScreenManagerLang.VideoFilterCameraMotion_MatchFeatures;
            mnuFindHomographies.Text = ScreenManagerLang.VideoFilterCameraMotion_FindHomographies;
            mnuBundleAdjustment.Text = ScreenManagerLang.VideoFilterCameraMotion_BundleAdjustment;
            mnuBuildTracks.Text = ScreenManagerLang.VideoFilterCameraMotion_BuildTracks;
            mnuDeleteData.Text = ScreenManagerLang.VideoFilterCameraMotion_DeleteTrackingData;

            mnuOptions.Text = ScreenManagerLang.Generic_Options;
            mnuShowFeatures.Text = ScreenManagerLang.VideoFilterCameraMotion_ShowPoints;
            mnuShowInliers.Text = ScreenManagerLang.VideoFilterCameraMotion_ShowInliers;
            mnuShowOutliers.Text = ScreenManagerLang.VideoFilterCameraMotion_ShowOutliers;
            mnuShowMotionField.Text = ScreenManagerLang.VideoFilterCameraMotion_ShowMotionField;
            mnuShowTransforms.Text = ScreenManagerLang.VideoFilterCameraMotion_ShowFrameTransforms;
            mnuShowTracks.Text = ScreenManagerLang.VideoFilterCameraMotion_ShowTracks;
        }

        private void MnuConfigure_Click(object sender, EventArgs e)
        {
            // The dialog is responsible for handling undo/redo.

            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            if (tsmi == null)
                return;

            FormConfigureCameraMotion fccm = new FormConfigureCameraMotion(this);
            FormsHelper.Locate(fccm);
            fccm.ShowDialog();

            if (fccm.DialogResult == DialogResult.OK)
            {
                SaveAsDefaultParameters();
                ResetData();
            }

            fccm.Dispose();
            InvalidateFromMenu(sender);
        }

        private void MnuRunAll_Click(object sender, EventArgs e)
        {
            if (framesContainer == null || framesContainer.Frames == null || framesContainer.Frames.Count < 1)
                return;

            step = CameraMotionStep.All;
            StartProcess(sender);
            InvalidateFromMenu(sender);
        }

        private void MnuFindFeatures_Click(object sender, EventArgs e)
        {
            if (framesContainer == null || framesContainer.Frames == null || framesContainer.Frames.Count < 1)
                return;

            step = CameraMotionStep.FindFeatures;
            StartProcess(sender);

            // Force visualization options.
            showFeatures = true;
            showInliers = false;
            showOutliers = false;
            showMotionField = false;
            showTransforms = false;
            showTracks = false;
            InvalidateFromMenu(sender);
        }

        private void MnuMatchFeatures_Click(object sender, EventArgs e)
        {
            if (framesContainer == null || framesContainer.Frames == null || framesContainer.Frames.Count < 1)
                return;

            step = CameraMotionStep.MatchFeatures;
            StartProcess(sender);

            // Force visualization options
            showFeatures = false;
            showInliers = true;
            showOutliers = false;
            showMotionField = false;
            showTransforms = false;
            showTracks = false;
            InvalidateFromMenu(sender);
        }

        private void MnuFindHomographies_Click(object sender, EventArgs e)
        {
            if (framesContainer == null || framesContainer.Frames == null || framesContainer.Frames.Count < 1)
                return;

            step = CameraMotionStep.FindHomographies;
            StartProcess(sender);

            // Force visualization options
            showFeatures = false;
            showInliers = false;
            showOutliers = false;
            showMotionField = true;
            showTransforms = true;
            showTracks = false;
            InvalidateFromMenu(sender);
        }

        private void MnuBundleAdjustment_Click(object sender, EventArgs e)
        {
            if (framesContainer == null || framesContainer.Frames == null || framesContainer.Frames.Count < 1)
                return;

            step = CameraMotionStep.BundleAdjustment;
            StartProcess(sender);

            // Force visualization options
            showFeatures = false;
            showInliers = false;
            showOutliers = false;
            showMotionField = true;
            showTransforms = true;
            showTracks = false;
            InvalidateFromMenu(sender);
        }

        private void MnuBuildTracks_Click(object sender, EventArgs e)
        {
            if (framesContainer == null || framesContainer.Frames == null || framesContainer.Frames.Count < 1)
                return;

            step = CameraMotionStep.BuildTracks;
            StartProcess(sender);

            // Force visualization options
            showFeatures = false;
            showInliers = true;
            showOutliers = false;
            showMotionField = false;
            showTransforms = false;
            showTracks = true;
            InvalidateFromMenu(sender);
        }

        private void StartProcess(object sender)
        {
            formProgressBar2 fpb = new formProgressBar2(true, true, Worker_DoWork);
            fpb.ShowDialog();
            fpb.Dispose();

            if (tracker.Tracked)
                parentMetadata.SetCameraMotion(tracker);
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // This runs in the background thread.
            Thread.CurrentThread.Name = "CameraMotionEstimation";
            BackgroundWorker worker = sender as BackgroundWorker;

            switch (step)
            {
                case CameraMotionStep.FindFeatures:
                    MakeMask();
                    tracker.FindFeatures(framesContainer, worker);
                    break;
                case CameraMotionStep.MatchFeatures:
                    tracker.MatchFeatures(framesContainer, worker);
                    break;
                case CameraMotionStep.FindHomographies:
                    tracker.FindHomographies(framesContainer, worker);
                    break;
                case CameraMotionStep.BundleAdjustment:
                    tracker.BundleAdjustment(framesContainer, worker);
                    break;
                case CameraMotionStep.BuildTracks:
                    tracker.BuildTracks(worker);
                    break;
                case CameraMotionStep.All:
                default:
                    MakeMask();
                    tracker.RunAll(framesContainer, worker);
                    break;
            }
        }

        private void MnuDeleteData_Click(object sender, EventArgs e)
        {
            //CaptureMemento();
            tracker.ResetTrackingData();

            parentMetadata.SetCameraMotion(tracker);

            InvalidateFromMenu(sender);
        }

        private void MnuShowFeatures_Click(object sender, EventArgs e)
        {
            //CaptureMemento();

            showFeatures = !mnuShowFeatures.Checked;

            //Update();
            InvalidateFromMenu(sender);
        }

        private void MnuShowInliers_Click(object sender, EventArgs e)
        {
            //CaptureMemento();

            showInliers = !mnuShowInliers.Checked;

            //Update();
            InvalidateFromMenu(sender);
        }

        private void MnuShowOutliers_Click(object sender, EventArgs e)
        {
            //CaptureMemento();

            showOutliers = !mnuShowOutliers.Checked;

            //Update();
            InvalidateFromMenu(sender);
        }

        private void MnuShowMotionField_Click(object sender, EventArgs e)
        {
            //CaptureMemento();

            showMotionField = !mnuShowMotionField.Checked;

            //Update();
            InvalidateFromMenu(sender);
        }

        private void MnuShowTransforms_Click(object sender, EventArgs e)
        {
            //CaptureMemento();

            showTransforms = !mnuShowTransforms.Checked;

            //Update();
            InvalidateFromMenu(sender);
        }

        private void MnuShowTracks_Click(object sender, EventArgs e)
        {
            //CaptureMemento();

            showTracks = !mnuShowTracks.Checked;

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
        /// Draw a dot on each found feature.
        /// These are all the features found, they may or may not end up being used in the motion estimation. 
        /// </summary>
        private void DrawFeatures(Graphics canvas, IImageToViewportTransformer transformer, long timestamp)
        {
            List<PointF> features = tracker.GetFeatures(timestamp);
            if (features == null || features.Count == 0)
                return;

            foreach (var feature in features)
            {
                PointF p = transformer.Transform(feature);
                canvas.DrawEllipse(penFeature, p.Box(2));
            }
        }

        /// <summary>
        /// Draw feature matches, outliers and/or inliers.
        /// Matches are drawn as a line connecting the feature in this frame with its supposed location
        /// in the next frame.
        /// The connector is drawn green for inliers and red for outliers.
        /// </summary>
        private void DrawMatches(Graphics canvas, IImageToViewportTransformer transformer, long timestamp)
        {
            List<CameraMatch> matches = tracker.GetMatches(timestamp);
            if (matches == null || matches.Count == 0)
                return;

            foreach (var m in matches)
            {
                PointF p1 = transformer.Transform(m.P1);
                PointF p2 = transformer.Transform(m.P2);

                if (m.Inlier && showInliers)
                {
                    canvas.DrawEllipse(penFeatureInlier, p1.Box(2));
                    canvas.DrawLine(penMatchInlier, p1, p2);
                }
                else if (!m.Inlier && showOutliers)
                {
                    canvas.DrawEllipse(penFeatureOutlier, p1.Box(2));
                    canvas.DrawLine(penMatchOutlier, p1, p2);
                }
            }
        }

        /// <summary>
        /// Draw a field of motion vectors based on the transform to the next frame.
        /// This shows the computed global motion.
        /// </summary>
        private void DrawMotionField(Graphics canvas, IImageToViewportTransformer transformer, long timestamp)
        {
            if (tracker.ConsecutiveTransforms.Count == 0)
                return;

            if (!tracker.FrameIndices.ContainsKey(timestamp))
                return;

            if (tracker.FrameIndices[timestamp] >= tracker.ConsecutiveTransforms.Count)
                return;

            // Generate a field of points.
            List<PointF> sources = new List<PointF>();
            int cols = motionFieldPoints;
            int rows = motionFieldPoints;
            for (int i = 0; i < cols; i++)
            {
                float left = ((i + 0.5f) / cols) * frameSize.Width;

                for (int j = 0; j < rows; j++)
                {
                    float top = ((j + 0.5f) / rows) * frameSize.Height;
                    sources.Add(new PointF(left, top));
                }
            }
            

            // Convert the points to OpenCV.
            var cvTargets = sources.Select(p => new OpenCvSharp.Point2f(p.X, p.Y));

            // Apply the perspective transform to all the points.
            int index = tracker.FrameIndices[timestamp];
            cvTargets = OpenCvSharp.Cv2.PerspectiveTransform(cvTargets, tracker.ConsecutiveTransforms[index]);
            
            // Convert back to Drawing.PointF
            var targets = cvTargets.Select(p => new PointF(p.X, p.Y));

            // Transform to screen space.
            var sourcesTransformed = transformer.Transform(sources);
            var targetsTransformed = transformer.Transform(targets);

            // Draw arrows pointing to the position of the points in the next frame.
            penMotionField.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
            for (int i = 0; i < sourcesTransformed.Count; i++)
            {
                PointF p1 = sourcesTransformed[i];
                PointF p2 = targetsTransformed[i];
                if (GeometryHelper.GetDistance(p1, p2) < 2.0f)
                {
                    canvas.DrawEllipse(penMotionField, p1.Box(4));
                }
                else
                {
                    canvas.DrawLine(penMotionField, p1, p2);
                }
            }
        }

        /// <summary>
        /// Draw rectangles of the previous frames transformed into this frame space.
        /// </summary>
        private void DrawTransforms(Graphics canvas, IImageToViewportTransformer transformer, long timestamp)
        {
            if (tracker.ConsecutiveTransforms.Count == 0)
                return;

            if (!tracker.FrameIndices.ContainsKey(timestamp))
                return;

            if (tracker.FrameIndices[timestamp] >= tracker.ConsecutiveTransforms.Count)
                return;

            // Transform an image space rectangle to show how the image is modified from one frame to the next.
            float left = frameSize.Width * 0.1f;
            float top = frameSize.Height * 0.1f;
            float right = left + frameSize.Width * 0.8f;
            float bottom = top + frameSize.Height * 0.8f;
            var bounds = new[]
            {
                new OpenCvSharp.Point2f(left, top),
                new OpenCvSharp.Point2f(right, top),
                new OpenCvSharp.Point2f(right, bottom),
                new OpenCvSharp.Point2f(left, bottom),
            };

            DrawFramesBounds(canvas, transformer, timestamp, bounds);
            //DrawFramesBounds2(canvas, transformer, timestamp, bounds);
            //DrawFramesBounds3(canvas, transformer, timestamp, bounds);
        }

        /// <summary>
        /// Draw the bounds of past frames into this one.
        /// </summary>
        private void DrawFramesBounds(Graphics canvas, IImageToViewportTransformer transformer, long timestamp, OpenCvSharp.Point2f[] points)
        {
            // This version iteratively transform the points using each frame-to-frame homography
            // from the source image up to the current image.

            // first and last represent the range of frames we are representing into the current one.
            int current = tracker.FrameIndices[timestamp];
            int first = Math.Max(tracker.FrameIndices[timestamp] - maxAgeTransformFrames, 0);
            int last = showSingleTransformFrame ? first + 1 : current;
            for (int i = first; i < last; i++)
            {
                var points2 = points;
                // `i` is the frame we are representing inside the current one.
                // Iteratively apply the frame-to-frame consecutive transforms, up to the current one.
                // At the end of this we have the rectangle of frame i, seen from the current frame.
                for (int j = i; j < current; j++)
                {
                    points2 = OpenCvSharp.Cv2.PerspectiveTransform(points2, tracker.ConsecutiveTransforms[j]);
                }

                DrawSingleFrameBounds(canvas, transformer, points2, i);
            }
        }

        /// <summary>
        /// Draw the bounds of past frames into this one.
        /// </summary>
        private void DrawFramesBounds2(Graphics canvas, IImageToViewportTransformer transformer, long timestamp, OpenCvSharp.Point2f[] points)
        {
            // This version pre-computes the homography going from the source frame to the current frame.
            // This is mainly to see how it diverges from the iterative method which is the ground truth.
            // Result: we can start to see differences with the iterative method when using age=25 frames.

            // first and last represent the range of frames we are representing into the current one.
            int current = tracker.FrameIndices[timestamp];
            int first = Math.Max(tracker.FrameIndices[timestamp] - maxAgeTransformFrames, 0);
            int last = showSingleTransformFrame ? first + 1 : current;

            for (int i = first; i < last; i++)
            {
                var points2 = points;

                // Pre-compute the homography going from first to current.
                var homography = OpenCvSharp.Mat.Eye(3, 3, OpenCvSharp.MatType.CV_64FC1);
                for (int j = i; j < current; j++)
                {
                    homography = homography * tracker.ConsecutiveTransforms[j];
                }

                // Apply the combined homography.
                points2 = OpenCvSharp.Cv2.PerspectiveTransform(points2, homography);
                homography.Dispose();

                DrawSingleFrameBounds(canvas, transformer, points2, i);
            }
        }

        /// <summary>
        /// Draw the bounds of past frames into this one.
        /// </summary>
        private void DrawFramesBounds3(Graphics canvas, IImageToViewportTransformer transformer, long timestamp, OpenCvSharp.Point2f[] points)
        {
            // This version uses the global rotation matrices.
            //int refFrame = 62;
            //int current = tracker.FrameIndices[timestamp];
            //int first = Math.Max(tracker.FrameIndices[timestamp] - maxAgeTransformFrames, 0);
            //int last = showSingleTransformFrame ? first + 1 : current;
            //for (int i = first; i < last; i++)
            //{
            //    var points2 = Unproject(points, i);
            //    var points3 = RotateToRef(points2, i, refFrame);
            //    var points4 = RotateFromRef(points3, current, refFrame);
            //    var points5 = Project(points4, refFrame);

            //    DrawSingleFrameBounds(canvas, transformer, points5, i);
            //}
        }

        #region Rotation estimation (WIP)
        /// <summary>
        /// Go from image space to camera space of the specified frame.
        /// </summary>
        //private OpenCvSharp.Point3f[] Unproject(OpenCvSharp.Point2f[] pp, int frameId)
        //{
        //    // Apply the inverse of the projection matrix.
        //    var pp2 = new List<OpenCvSharp.Point3f>();
        //    for (int i = 0; i < pp.Length; i++)
        //    {
        //        double x = (pp[i].X - tracker.CameraParams[frameId].PpX) / tracker.CameraParams[frameId].Focal;
        //        double y = (pp[i].Y - tracker.CameraParams[frameId].PpY) / (tracker.CameraParams[frameId].Focal * tracker.CameraParams[frameId].Aspect);
        //        pp2.Add(new OpenCvSharp.Point3f((float)x, (float)y, 1.0f));
        //    }

        //    return pp2.ToArray();
        //}

        ///// <summary>
        ///// Go from camera space to image space of the specified frame.
        ///// </summary>
        //private OpenCvSharp.Point2f[] Project(OpenCvSharp.Point3f[] pp, int frameId)
        //{
        //    // Apply the projection matrix.
        //    var pp2 = new List<OpenCvSharp.Point2f>();
        //    for (int i = 0; i < pp.Length; i++)
        //    {
        //        double x = (pp[i].X * tracker.CameraParams[frameId].Focal) + tracker.CameraParams[frameId].PpX;
        //        double y = (pp[i].Y * (tracker.CameraParams[frameId].Focal * tracker.CameraParams[frameId].Aspect)) + tracker.CameraParams[frameId].PpY;
        //        pp2.Add(new OpenCvSharp.Point2f((float)x, (float)y));
        //    }

        //    return pp2.ToArray();
        //}

        ///// <summary>
        ///// Rotate points from camera space of the specified frame into the camera space of the reference frame.
        ///// </summary>
        //private OpenCvSharp.Point3f[] RotateToRef(OpenCvSharp.Point3f[] pp, int frameId, int refFrame)
        //{
        //    if (frameId == refFrame)
        //        return pp;

        //    if (tracker.CameraParams[frameId].R.Empty())
        //    {
        //        log.Error("Empty rotation matrix.");
        //        throw new InvalidDataException();
        //        return pp;
        //    }

        //    // Apply the rotation matrix.
        //    // Note: these rotation matrices are already built from the *inverses* of
        //    // the consecutive homographies going from the reference frame to the considered frame.
        //    // So they are already transforming points from the source frame to the
        //    // reference frame, we don't need to invert it.
        //    // see: motion_estimators.cpp > CalcRotation > operator().
        //    // (the breadth-first graph walk starts at the reference frame and goes outwards).
        //    // Mat R = K_from.inv() * pairwise_matches[pair_idx].H.inv() * K_to;
        //    var srcMat = OpenCvSharp.Mat.FromArray(pp);
        //    var dstMat = new OpenCvSharp.Mat<OpenCvSharp.Point3f>(pp.Length, 1);
        //    OpenCvSharp.Cv2.Transform(srcMat, dstMat, tracker.CameraParams[frameId].R);
            
        //    var result = dstMat.ToArray();
        //    dstMat.Dispose();
        //    srcMat.Dispose();

        //    return result;
        //}

        ///// <summary>
        ///// Rotate points from camera space of the reference frame into the camera space of the specified frame.
        ///// </summary>
        //private OpenCvSharp.Point3f[] RotateFromRef(OpenCvSharp.Point3f[] pp, int frameId, int refFrame)
        //{
        //    if (frameId == refFrame)
        //        return pp;

        //    if (tracker.CameraParams[frameId].R.Empty())
        //    {
        //        log.Error("Empty rotation matrix.");
        //        return pp;
        //    }

        //    // Apply the inverse of the rotation matrix.
        //    var invRot = tracker.CameraParams[frameId].R.Inv();
        //    var srcMat = OpenCvSharp.Mat.FromArray(pp);
        //    var dstMat = new OpenCvSharp.Mat<OpenCvSharp.Point3f>(pp.Length, 1);
        //    OpenCvSharp.Cv2.Transform(srcMat, dstMat, invRot);

        //    var result = dstMat.ToArray();
        //    dstMat.Dispose();
        //    srcMat.Dispose();
        //    //invRot.Dispose();

        //    return result;
        //}
        #endregion

        /// <summary>
        /// Draw the bounds of a single previous frame using a random color.
        /// The frame id is only used to pick the random color.
        /// </summary>
        private void DrawSingleFrameBounds(Graphics canvas, IImageToViewportTransformer transformer, OpenCvSharp.Point2f[] points, int id)
        {
            // Convert back from OpenCV point to Drawing.PointF and transform to screen space.
            var points3 = points.Select(p => new PointF(p.X, p.Y));
            var points4 = transformer.Transform(points3);

            // Get a random color that will be unique to the represented frame.
            string str = "FF" + colorCycle[id % colorCycle.Length];
            Color c = Color.FromArgb(Convert.ToInt32(str, 16));
            using (Pen pen = new Pen(c, penWidthTransformFrames))
                canvas.DrawPolygon(pen, points4.ToArray());
        }

        private void DrawTracks(Graphics canvas, IImageToViewportTransformer transformer, long timestamp)
        {
            if (tracker.ConsecutiveTransforms.Count == 0)
                return;

            if (!tracker.FrameIndices.ContainsKey(timestamp))
                return;

            if (tracker.FrameIndices[timestamp] >= tracker.ConsecutiveTransforms.Count)
                return;

            // Follow the feature over multiple frames.
            var tracks = tracker.GetTracks();

            for (int i = 0; i < tracks.Count; i++)
            {
                var track = tracks[i];
                long start = track.First().Key;
                long end = track.Last().Key;
                if (timestamp < start || timestamp > end)
                    continue;

                // Collect the points of the track
                // and draw their position.
                List<PointF> points = new List<PointF>();
                foreach (var entry in track)
                {
                    long t = entry.Key;
                    PointF p = transformer.Transform(entry.Value);
                    points.Add(p);

                    if (entry.Key == timestamp)
                        canvas.DrawRectangle(penTracks, p.Box(4).ToRectangle());
                    else
                        canvas.DrawEllipse(penTracks, p.Box(2));
                }

                // Draw the track.
                canvas.DrawLines(penTracks, points.ToArray());
            }
        }

        /// <summary>
        /// Draw various textual statistics.
        /// </summary>
        private void DrawResults(Graphics canvas, long timestamp)
        {
            string text = GetResultsString(timestamp);

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
        /// Return camera motion results as text.
        /// </summary>
        private string GetResultsString(long timestamp)
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine(string.Format("Camera motion"));

            if (tracker.FrameIndices.Count > 0)
            {
                int index = tracker.FrameIndices[timestamp];
                b.AppendLine(string.Format("Frame: {0}/{1}", index + 1, framesContainer.Frames.Count));
            }

            var features = tracker.GetFeatures(timestamp);
            if (features != null && features.Count > 0)
            {
                // It typically finds the requested number of features but they might be very close to each other.
                b.AppendLine(string.Format("Features: {0}/{1}", features.Count, tracker.Parameters.FeaturesPerFrame));
            }

            var matches = tracker.GetMatches(timestamp);
            if (matches != null && matches.Count > 0)
            {
                b.AppendLine(string.Format("Matches:{0}/{1}", matches.Count, features.Count));
                int inliers = matches.Count(m => m.Inlier);
                b.AppendLine(string.Format("Inliers:{0}/{1}", inliers, matches.Count));
            }

            var tracks = tracker.GetTracks();
            if (tracks != null && tracks.Count > 0)
            {
                b.AppendLine(string.Format("Tracks:{0}", tracks.Count));
            }

            return b.ToString();
        }

        /// <summary>
        /// Save the configuration as the new preferred configuration.
        /// </summary>
        private void SaveAsDefaultParameters()
        {
            PreferencesManager.PlayerPreferences.CameraMotionParameters = parameters.Clone();
        }

        /// <summary>
        /// Create a binary mask from rectangles in the image to ignore static areas.
        /// </summary>
        private void MakeMask()
        {
            // Collect rectangle objects in the video.
            List<RectangleF> rr = new List<RectangleF>();
            foreach (var d in parentMetadata.AttachedDrawings())
            {
                if (d is DrawingRectangle)
                {
                    DrawingRectangle dr = d as DrawingRectangle;
                    rr.Add(dr.QuadImage.GetBoundingBox());
                }
            }

            if (rr.Count == 0)
                return;

            // Create the mask and send it to the tracker.
            Bitmap mask = new Bitmap(frameSize.Width, frameSize.Height);
            using (Graphics g = Graphics.FromImage(mask))
            {
                g.Clear(Color.White);
                foreach (var r in rr)
                {
                    g.FillRectangle(Brushes.Black, r);
                }
            }

            tracker.SetMask(mask);
        }
    }
}
