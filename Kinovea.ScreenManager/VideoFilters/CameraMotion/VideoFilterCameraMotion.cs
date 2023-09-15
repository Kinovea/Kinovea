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
        private Bitmap mask;
        private Stopwatch stopwatch = new Stopwatch();
        private Random rnd = new Random();

        // Computed data
        // frameIndices: reverse index from timestamps to frames indices.
        private Dictionary<long, int> frameIndices = new Dictionary<long, int>();
        private List<OpenCvSharp.KeyPoint[]> keypoints = new List<OpenCvSharp.KeyPoint[]>();
        private List<OpenCvSharp.Mat> descriptors = new List<OpenCvSharp.Mat>();
        private List<Tuple<int, int>> imagePairs = new List<Tuple<int, int>>();
        private List<OpenCvSharp.DMatch[]> matches = new List<OpenCvSharp.DMatch[]>();
        private List<List<bool>> inlierStatus = new List<List<bool>>();
        private List<List<PointF>> inliers = new List<List<PointF>>();
        private List<OpenCvSharp.Mat> consecTransforms = new List<OpenCvSharp.Mat>();
        //private List<OpenCvSharp.Mat> forwardTransforms = new List<OpenCvSharp.Mat>();
        //private List<OpenCvSharp.Mat> backwardTransforms = new List<OpenCvSharp.Mat>();

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

        #region Menu
        private ToolStripMenuItem mnuConfigure = new ToolStripMenuItem();
        
        private ToolStripMenuItem mnuAction = new ToolStripMenuItem();
        private ToolStripMenuItem mnuRun = new ToolStripMenuItem();
        private ToolStripMenuItem mnuImportMask = new ToolStripMenuItem();
        private ToolStripMenuItem mnuImportColmap = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDeleteData = new ToolStripMenuItem();

        private ToolStripMenuItem mnuOptions = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowFeatures = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowMatches = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowTransforms = new ToolStripMenuItem();

        #endregion

        // Decoration
        private Pen penFeature = new Pen(Color.Yellow, 2.0f);
        private Pen penFeatureInlier = new Pen(Color.Lime, 2.0f);
        private Pen penMatchInlier = new Pen(Color.LimeGreen, 2.0f);
        private Pen penMatchOutlier = new Pen(Color.FromArgb(128, 255, 0, 0), 2.0f);
        // Precomputed list of unique colors to draw frames references.
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
            //mnuShowFeatures.Image
            //mnuShowMatches.Image
            //mnuShowTransforms.Image
            mnuShowFeatures.Click += MnuShowFeatures_Click;
            mnuShowMatches.Click += MnuShowMatches_Click;
            mnuShowTransforms.Click += MnuShowTransforms_Click;
            mnuOptions.DropDownItems.AddRange(new ToolStripItem[] {
                mnuShowFeatures,
                mnuShowMatches,
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

        public void StartMove(PointF p)
        {
        }

        public void StopMove()
        {
        }

        public void Move(float dx, float dy, Keys modifiers)
        {
        }

        /// <summary>
        /// Draw extra content on top of the produced image.
        /// </summary>
        public void DrawExtra(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer, long timestamp, bool export)
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
            ResetTrackingData();
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
            mnuShowMatches.Checked = showMatches;
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
            mnuShowFeatures.Text = "Show trackers";
            mnuShowMatches.Text = "Show matches";
            mnuShowTransforms.Text = "Show transforms";
        }

        private void MnuConfigure_Click(object sender, EventArgs e)
        {
            
        }

        private void MnuRun_Click(object sender, EventArgs e)
        {
            if (framesContainer == null || framesContainer.Frames == null || framesContainer.Frames.Count < 1)
                return;

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

            InvalidateFromMenu(sender);

            // Commit transform data.
            parentMetadata.SetCameraMotion(frameIndices, consecTransforms);
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

            if (mask != null)
                mask.Dispose();
            
            mask = new Bitmap(filename);

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
            ResetTrackingData();
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

        private void MnuShowMatches_Click(object sender, EventArgs e)
        {
            //CaptureMemento();

            showMatches = !mnuShowMatches.Checked;

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


        private void LogHomography(int index1, int index2, OpenCvSharp.Mat homography)
        {
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
        #endregion

        #region Private utilities
        private void ResetTrackingData()
        {
            frameIndices.Clear();
            keypoints.Clear();
            descriptors.Clear();
            imagePairs.Clear();
            matches.Clear();
            inlierStatus.Clear();
            inliers.Clear();
            consecTransforms.Clear();
            //forwardTransforms.Clear();
            //backwardTransforms.Clear();
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
        #endregion

        #region Rendering

        /// <summary>
        /// Draw a dot on each found feature.
        /// These are all the features found, they may or may not end up being used in the motion estimation. 
        /// </summary>
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
                canvas.DrawEllipse(penFeature, p.Box(2));
            }
        }

        /// <summary>
        /// Draw matches and inliers.
        /// Matches are drawn as a line connecting the feature in this frame with its supposed location
        /// in the next frame.
        /// The connector is drawn green for inliers and red for outliers.
        /// </summary>
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
                Pen pen = inlier ? penMatchInlier : penMatchOutlier;
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
                canvas.DrawEllipse(penFeatureInlier, p2.Box(4));
            }
        }

        private void DrawTransforms(Graphics canvas, IImageToViewportTransformer transformer, long timestamp)
        {
            if (consecTransforms.Count == 0)
                return;

            if (!frameIndices.ContainsKey(timestamp))
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

            
            if (frameIndices[timestamp] >= consecTransforms.Count)
                return;

            //---------------------------------
            // Draw the bounds of all the past frames up to this one.
            //---------------------------------

            for (int i = 0; i < frameIndices[timestamp]; i++)
            {
                // `i` is the frame we are representing inside the current one.
                // Apply the consecutive transform starting from it up to the current one.
                // At the end of this we have the rectangle of that frame as seen from the current one.
                var points = bounds;
                for (int j = i; j < frameIndices[timestamp]; j++)
                {
                    points = OpenCvSharp.Cv2.PerspectiveTransform(points, consecTransforms[j]);
                }

                // Convert back from OpenCV point to Drawing.PointF
                // and transform to screen space.
                var points3 = points.Select(p => new PointF(p.X, p.Y));
                var points4 = transformer.Transform(points3);

                // Get a random color that will be unique to the represented frame.
                string str = "FF" + colorCycle[i % colorCycle.Length];
                int colorInt = Convert.ToInt32(str, 16);
                Color c = Color.FromArgb(colorInt);
                using (Pen pen = new Pen(c, 1.0f))
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
