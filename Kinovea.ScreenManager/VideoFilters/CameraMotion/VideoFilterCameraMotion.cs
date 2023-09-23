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
//using Microsoft.WindowsAPICodePack.Dialogs;
using System.Globalization;

namespace Kinovea.ScreenManager
{
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
        private Random rnd = new Random();

        private CameraTracker tracker = new CameraTracker();

        // Display parameters
        private bool showFeatures = false;      // All the features found.
        private bool showInliers = true;        // Features matched and used to estimate the final motion.
        private bool showOutliers = false;      // Features matched but not used to estimate the final motion. 
        private bool showTransforms = true;     // Frame transforms.

        #region Menu
        private ToolStripMenuItem mnuConfigure = new ToolStripMenuItem();
        
        private ToolStripMenuItem mnuAction = new ToolStripMenuItem();
        private ToolStripMenuItem mnuRun = new ToolStripMenuItem();
        private ToolStripMenuItem mnuImportMask = new ToolStripMenuItem();
        private ToolStripMenuItem mnuImportColmap = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDeleteData = new ToolStripMenuItem();

        private ToolStripMenuItem mnuOptions = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowFeatures = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowOutliers = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowInliers = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowTransforms = new ToolStripMenuItem();

        #endregion

        // Decoration
        private Pen penFeature = new Pen(Color.Yellow, 2.0f);
        private Pen penFeatureOutlier = new Pen(Color.Red, 2.0f);
        private Pen penFeatureInlier = new Pen(Color.Lime, 2.0f);
        private Pen penMatchInlier = new Pen(Color.LimeGreen, 2.0f);
        private Pen penMatchOutlier = new Pen(Color.FromArgb(128, 255, 0, 0), 2.0f);
        private int maxTransformsFrames = 25;
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

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region ctor/dtor
        public VideoFilterCameraMotion(Metadata metadata)
        {
            this.parentMetadata = metadata;

            InitializeMenus();

            //parameters = PreferencesManager.PlayerPreferences.CameraMotion;
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
            mnuRun.Image = Properties.Drawings.trackingplay;
            mnuDeleteData.Image = Properties.Resources.bin_empty;
            mnuRun.Click += MnuRun_Click;
            mnuImportMask.Click += MnuImportMask_Click;
            mnuImportColmap.Click += MnuImportColmap_Click;
            mnuDeleteData.Click += MnuDeleteData_Click;
            mnuAction.DropDownItems.AddRange(new ToolStripItem[] {
                mnuRun,
                new ToolStripSeparator(),
                mnuImportMask,
                mnuImportColmap,
                new ToolStripSeparator(),
                mnuDeleteData,
            });

            mnuOptions.Image = Properties.Resources.equalizer;
            mnuShowFeatures.Image = Properties.Drawings.bullet_orange;
            mnuShowInliers.Image = Properties.Drawings.bullet_green;
            mnuShowOutliers.Image = Properties.Drawings.bullet_red;
            mnuShowFeatures.Click += MnuShowFeatures_Click;
            mnuShowOutliers.Click += MnuShowOutliers_Click;
            mnuShowInliers.Click += MnuShowInliers_Click;
            mnuShowTransforms.Click += MnuShowTransforms_Click;
            mnuOptions.DropDownItems.AddRange(new ToolStripItem[] {
                mnuShowFeatures,
                mnuShowInliers,
                mnuShowOutliers,
                mnuShowTransforms,
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
                mnuConfigure,
                new ToolStripSeparator(),
                mnuAction,
                mnuOptions,
            });

            mnuShowFeatures.Checked = showFeatures;
            mnuShowInliers.Checked = showInliers;
            mnuShowOutliers.Checked = showOutliers;
            mnuShowTransforms.Checked = showTransforms;

            return contextMenu;
        }

        private void ReloadMenusCulture()
        {
            // Just in time localization.
            mnuConfigure.Text = ScreenManagerLang.Generic_ConfigurationElipsis;

            mnuAction.Text = ScreenManagerLang.mnuAction;
            mnuRun.Text = "Run camera motion estimation";
            mnuImportMask.Text = "Import mask";
            mnuImportColmap.Text = "Import COLMAP";
            mnuDeleteData.Text = "Delete tracking data";

            mnuOptions.Text = ScreenManagerLang.Generic_Options;
            mnuShowFeatures.Text = "Show points";
            mnuShowInliers.Text = "Show inliers";
            mnuShowOutliers.Text = "Show outliers";
            mnuShowTransforms.Text = "Show transforms";
        }

        private void MnuConfigure_Click(object sender, EventArgs e)
        {
            
        }

        private void MnuRun_Click(object sender, EventArgs e)
        {
            if (framesContainer == null || framesContainer.Frames == null || framesContainer.Frames.Count < 1)
                return;

            tracker.Run(framesContainer);
            
            InvalidateFromMenu(sender);

            // Commit transform data.
            parentMetadata.SetCameraMotion(tracker);
        }

        private void MnuImportMask_Click(object sender, EventArgs e)
        {
            // Open image.
            // Reject if it's not the same size.
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

            tracker.SetMask(filename);
            openFileDialog.Dispose();
        }

        private void MnuImportColmap_Click(object sender, EventArgs e)
        {
            // Import camera intrinsics & extrinsics calculated by COLMAP.
            // Point to folder containing text export.
            //CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            //dialog.IsFolderPicker = true;
            //dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            //string folderName = "";
            //if (dialog.ShowDialog() == CommonFileDialogResult.Ok && !string.IsNullOrEmpty(dialog.FileName))
            //    folderName = dialog.FileName;

            //dialog.Dispose();
            //if (string.IsNullOrEmpty(folderName))
            //    return;

            //ParseColmap(folderName);
            //InvalidateFromMenu(sender);

            //// Commit transform data.
            //int frameIndex = 0;
            //foreach (var f in framesContainer.Frames)
            //{
            //    if (frameIndices.ContainsKey(f.Timestamp))
            //        continue;

            //    frameIndices.Add(f.Timestamp, frameIndex);
            //    frameIndex++;
            //}

            //metadata.SetCameraMotion(frameIndices, consecTransforms);
        }

        private void MnuDeleteData_Click(object sender, EventArgs e)
        {
            //CaptureMemento();
            tracker.ResetTrackingData();
            //Update();
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

        private void MnuShowTransforms_Click(object sender, EventArgs e)
        {
            //CaptureMemento();

            showTransforms = !mnuShowTransforms.Checked;

            //Update();
            InvalidateFromMenu(sender);
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
                    canvas.DrawEllipse(penFeatureInlier, p1.Box(4));
                    canvas.DrawLine(penMatchInlier, p1, p2);
                }
                else if (!m.Inlier && showOutliers)
                {
                    canvas.DrawEllipse(penFeatureOutlier, p1.Box(4));
                    canvas.DrawLine(penMatchOutlier, p1, p2);
                }
            }
        }

        private void DrawTransforms(Graphics canvas, IImageToViewportTransformer transformer, long timestamp)
        {
            if (tracker.ConsecutiveTransforms.Count == 0)
                return;

            if (!tracker.FrameIndices.ContainsKey(timestamp))
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

            
            if (tracker.FrameIndices[timestamp] >= tracker.ConsecutiveTransforms.Count)
                return;

            //---------------------------------
            // Draw the bounds of all the past frames up to this one.
            //---------------------------------
            int start = Math.Max(tracker.FrameIndices[timestamp] - maxTransformsFrames, 0);
            for (int i = start; i < tracker.FrameIndices[timestamp]; i++)
            {
                // `i` is the frame we are representing inside the current one.
                // Apply the consecutive transform starting from it up to the current one.
                // At the end of this we have the rectangle of that frame as seen from the current one.
                var points = bounds;
                for (int j = i; j < tracker.FrameIndices[timestamp]; j++)
                {
                    points = OpenCvSharp.Cv2.PerspectiveTransform(points, tracker.ConsecutiveTransforms[j]);
                }

                // Convert back from OpenCV point to Drawing.PointF
                // and transform to screen space.
                var points3 = points.Select(p => new PointF(p.X, p.Y));
                var points4 = transformer.Transform(points3);

                // Get a random color that will be unique to the represented frame.
                string str = "FF" + colorCycle[i % colorCycle.Length];
                int colorInt = Convert.ToInt32(str, 16);
                Color c = Color.FromArgb(colorInt);
                using (Pen pen = new Pen(c, 2.0f))
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
        
    }
}
