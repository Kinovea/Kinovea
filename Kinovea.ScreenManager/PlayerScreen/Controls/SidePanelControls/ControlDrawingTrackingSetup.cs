using Kinovea.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Web;
using System.Windows.Forms;


namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This controls holds the name and style configuration editors for the active drawing.
    /// It is used in the side panel.
    /// </summary>
    public partial class ControlDrawingTrackingSetup : UserControl
    {
        #region Events
        public event EventHandler<DrawingEventArgs> DrawingModified;
        #endregion

        #region Properties
        /// <summary>
        /// Returns true if any text editor is being edited.
        /// This must be consulted before triggering a shortcut that would conflict with text input.
        /// </summary>
        public bool Editing
        {
            get { return editing; }
        }
        #endregion

        #region Members
        private AbstractDrawing drawing;
        private Metadata metadata;
        private Guid managerId;
        private bool manualUpdate;
        private bool editing;
        private Pen penBorder = Pens.Silver;
        public static readonly List<TrackingAlgorithm> options = new List<TrackingAlgorithm>() { 
            TrackingAlgorithm.Correlation,
            TrackingAlgorithm.RoundMarker,
            TrackingAlgorithm.QuadrantMarker,
        };

        // Viewport
        private ViewportController viewportController = new ViewportController(false, false);
        private MetadataRenderer metadataRenderer;
        private MetadataManipulator metadataManipulator;
        private ScreenToolManager screenToolManager = new ScreenToolManager();
        private System.Windows.Forms.Timer interactionTimer = new System.Windows.Forms.Timer();
        private IDrawingHostView hostView;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public ControlDrawingTrackingSetup()
        {
            InitializeComponent();
            this.Paint += Control_Paint;

            pnlViewport.Controls.Add(viewportController.View);
            viewportController.View.DoubleClick += pnlViewport_DoubleClick;
            viewportController.View.Dock = DockStyle.Fill;

            NudHelper.FixNudScroll(nudSearchWindowWidth);
            NudHelper.FixNudScroll(nudSearchWindowHeight);
            NudHelper.FixNudScroll(nudObjWindowWidth);
            NudHelper.FixNudScroll(nudObjWindowHeight);
            NudHelper.FixNudScroll(nudTolerance);
            NudHelper.FixNudScroll(nudKeepAlive);
        }
        #endregion

        #region Public methods
        public void SetHostView(IDrawingHostView hostView)
        {
            this.hostView = hostView;
        }

        /// <summary>
        /// Set the drawing this control is managing.
        /// </summary>
        public void SetDrawing(AbstractDrawing drawing, Metadata metadata, Guid managerId, Guid drawingId)
        {
            // Bail out if same drawing.
            if (this.drawing != null && drawing != null && this.drawing.Id == drawing.Id)
            {
                // Updating the image and timestamp causes some issues.
                // Investigate.

                //metadataManipulator.SetFixedTimestamp(hostView.CurrentTimestamp);
                //metadataRenderer.SetSoloMode(true, drawing.Id, true);
                //screenToolManager.SetSoloMode(true, drawing.Id, true);

                //Bitmap newBitmap = BitmapHelper.Copy(hostView.CurrentImage);
                //viewportController.Bitmap = newBitmap;
                //viewportController.Timestamp = hostView.CurrentTimestamp;
                //// Do not recenter in this case, as the select may come from the panel itself.
                ////InitializeDisplayRectangle(newBitmap.Size, hostView.CurrentTimestamp);
                //viewportController.Refresh();
                return;
            }
            
            manualUpdate = true;
            ForgetDrawing();

            this.drawing = drawing;
            this.metadata = metadata;
            this.managerId = managerId;

            if (drawing == null || !(drawing is IDecorable))
            {
                manualUpdate = false;
                return;
            }

            metadataRenderer = new MetadataRenderer(metadata, true);
            metadataRenderer.SetSoloMode(true, drawing.Id, true);
            screenToolManager.SetSoloMode(true, drawing.Id, true);
            
            metadataManipulator = new MetadataManipulator(metadata, screenToolManager);
            metadataManipulator.SetFixedTimestamp(hostView.CurrentTimestamp);
            metadataManipulator.SetFixedKeyframe(-1);

            // The bitmap will soon be disposed by the host view, make our own copy.
            // We could also consider continually updating the image based on the player
            // but for tracking setup it might be interesting to keep it as a reference.
            // Maybe have a button to "refresh" the image and timestamp.
            Bitmap bitmap = BitmapHelper.Copy(hostView.CurrentImage);
            viewportController.Bitmap = bitmap;
            viewportController.Timestamp = hostView.CurrentTimestamp;
            viewportController.MetadataRenderer = metadataRenderer;
            viewportController.MetadataManipulator = metadataManipulator;
            InitializeDisplayRectangle(bitmap.Size, hostView.CurrentTimestamp);
            viewportController.Refresh();

            if (drawing is DrawingTrack)
            {
                DrawingTrack track = (DrawingTrack)drawing;
                track.TrackerParametersChanged += Track_TrackerParametersChanged;
            }

            // Interaction timer for the viewport.
            interactionTimer.Interval = 15;
            interactionTimer.Tick += InteractionTimer_Tick;
            interactionTimer.Start();

            SetupControls();
            
            manualUpdate = false;
        }

        /// <summary>
        /// The timestamp or bitmap or tracking params were updated from the main player.
        /// Update and recenter.
        /// </summary>
        public void UpdateContent()
        {
            metadataManipulator.SetFixedTimestamp(hostView.CurrentTimestamp);
            Bitmap bitmap = BitmapHelper.Copy(hostView.CurrentImage);
            viewportController.Bitmap = bitmap;
            viewportController.Timestamp = hostView.CurrentTimestamp;
            InitializeDisplayRectangle(bitmap.Size, hostView.CurrentTimestamp);
            viewportController.Refresh();
        }
        #endregion

        private void ForgetDrawing()
        {
            drawing = null;
            metadata = null;
            managerId = Guid.Empty;

            interactionTimer.Tick -= InteractionTimer_Tick;
        }

        private void InteractionTimer_Tick(object sender, EventArgs e)
        {
            viewportController.Refresh();
        }

        private void SetupControls()
        {
            // Tracking algorithm combo-box.
            cbTrackingAlgorithm.Items.Clear();
            cbTrackingAlgorithm.ItemHeight = 21;
            int selectedIndex = 0;
            for (int i = 0; i < options.Count; i++)
            {
                cbTrackingAlgorithm.Items.Add(new object());
                //if (cbTrackingAlgorithm[i] == value)
                //    selectedIndex = i;
            }

            cbTrackingAlgorithm.SelectedIndex = selectedIndex;
            cbTrackingAlgorithm.DrawItem += new DrawItemEventHandler(cbTrackingAlgorithm_DrawItem);
            //cbTrackingAlgorithm.SelectedIndexChanged += new EventHandler(editor_SelectedIndexChanged);

            UpdateTrackingParameters();

            this.Height = this.Height - this.ClientRectangle.Height + grpTracking.Bottom + 10;
        }

        private void InitializeDisplayRectangle(Size imgSize, long timestamp)
        {
            // Find an appropriate point to center the mini editor.
            PointF center = imgSize.Center();
            if (drawing is DrawingTrack)
            {
                center = ((DrawingTrack)drawing).GetPosition(timestamp);
            }

            // Scale such that the search window fits in the viewport.
            float scale = 1.0f;
            if (drawing is DrawingTrack)
            {
                Size searchSize = ((DrawingTrack)drawing).TrackerParameters.SearchWindow;
                float scaleX = (float)pnlViewport.Width / searchSize.Width;
                float scaleY = (float)pnlViewport.Height / searchSize.Height;
                scale = Math.Min(scaleX, scaleY) * 0.9f;
            }
    
            PointF normalizedPosition = new PointF(center.X / imgSize.Width, center.Y / imgSize.Height);
            SizeF normalizedHostSize = new SizeF((float)pnlViewport.Width / imgSize.Width, (float)pnlViewport.Height / imgSize.Height);
            PointF normalizedHostCenter = new PointF(normalizedHostSize.Width / 2, normalizedHostSize.Height / 2);
            PointF normalizedDisplayLocation = new PointF(normalizedHostCenter.X - (normalizedPosition.X * scale), normalizedHostCenter.Y - (normalizedPosition.Y * scale));
            PointF topLeft = new PointF(normalizedDisplayLocation.X * imgSize.Width, normalizedDisplayLocation.Y * imgSize.Height);
            Size fullSize = new Size((int)(imgSize.Width * scale), (int)(imgSize.Height * scale));
            Rectangle display = new Rectangle((int)topLeft.X, (int)topLeft.Y, fullSize.Width, fullSize.Height);
            viewportController.InitializeDisplayRectangle(display, imgSize);
        }

        /// <summary>
        /// The parameters were changed via the mini editor.
        /// </summary>
        private void Track_TrackerParametersChanged(object sender, EventArgs e)
        {
            UpdateTrackingParameters();

            DrawingModified?.Invoke(this, new DrawingEventArgs(drawing, managerId));
        }

        /// <summary>
        /// Update the tracking nuds with values from the drawing.
        /// </summary>
        private void UpdateTrackingParameters()
        {
            manualUpdate = true;

            
            if (drawing is DrawingTrack)
            {
                DrawingTrack track = (DrawingTrack)drawing;
                TrackingParameters tp = track.TrackerParameters;
                nudSearchWindowWidth.Value = tp.SearchWindow.Width;
                nudSearchWindowHeight.Value = tp.SearchWindow.Height;
                nudObjWindowWidth.Value = tp.BlockWindow.Width;
                nudObjWindowHeight.Value = tp.BlockWindow.Height;
            }

            manualUpdate = false;
        }

        /// <summary>
        /// Draw one item of the tracking algorithm combo-box.
        /// </summary>
        private void cbTrackingAlgorithm_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= options.Count)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            int top = e.Bounds.Height / 2;

            Brush backgroundBrush = Brushes.White;
            if ((e.State & DrawItemState.Focus) != 0)
                backgroundBrush = Brushes.LightSteelBlue;

            e.Graphics.FillRectangle(backgroundBrush, e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);

            Point topLeft = new Point(e.Bounds.Left + 2, e.Bounds.Top + 2);
            Size size = new Size(16, 16);
            Rectangle rect = new Rectangle(topLeft, size);
            PointF textTopLeft = new PointF(e.Bounds.Left + 20, e.Bounds.Top + 2);
            switch (options[e.Index])
            {
                case TrackingAlgorithm.Correlation:
                    {
                        e.Graphics.DrawImage(Properties.Resources.image_blur, rect);
                        e.Graphics.DrawString("Correlation", e.Font, Brushes.Black, textTopLeft);
                        break;
                    }
                case TrackingAlgorithm.RoundMarker:
                    {
                        e.Graphics.DrawImage(Properties.Resources.circular_marker, rect);
                        e.Graphics.DrawString("Round", e.Font, Brushes.Black, textTopLeft);

                        break;
                    }
                case TrackingAlgorithm.QuadrantMarker:
                    {
                        e.Graphics.DrawImage(Properties.Resources.quadrants_padded, rect);
                        e.Graphics.DrawString("Quadrants", e.Font, Brushes.Black, textTopLeft);
                        break;
                    }
            }
        }

        /// <summary>
        /// Custom outline color.
        /// </summary>
        private void Control_Paint(object sender, PaintEventArgs e)
        {
        }

        private void pnlViewport_Resize(object sender, EventArgs e)
        {
            if (drawing == null)
                return;

            // Refit and recenter.
            Size imgSize = viewportController.Bitmap.Size;
            long timestamp = viewportController.Timestamp;
            InitializeDisplayRectangle(imgSize, timestamp);
        }

        private void pnlViewport_DoubleClick(object sender, EventArgs e)
        {
            if (drawing == null)
                return;

            // Refit and recenter.
            Size imgSize = viewportController.Bitmap.Size;
            long timestamp = viewportController.Timestamp;
            InitializeDisplayRectangle(imgSize, timestamp);
        }

        private void nudSearchWindow_ValueChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            int width = (int)nudSearchWindowWidth.Value;
            int height = (int)nudSearchWindowHeight.Value;
            if (drawing is DrawingTrack)
            {
                DrawingTrack track = (DrawingTrack)drawing;
                track.TrackerParameters.SearchWindow = new Size(width, height);
            }
        }

        private void nudObjWindow_ValueChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            int width = (int)nudObjWindowWidth.Value;
            int height = (int)nudObjWindowHeight.Value;
            if (drawing is DrawingTrack)
            {
                DrawingTrack track = (DrawingTrack)drawing;
                track.TrackerParameters.BlockWindow = new Size(width, height);
            }
        }
    }
}
