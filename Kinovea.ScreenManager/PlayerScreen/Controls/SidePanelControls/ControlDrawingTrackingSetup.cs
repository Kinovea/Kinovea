using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Kinovea.Services;


namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This controls exposes Tracking UI for the active track.
    /// It is used in the side panel.
    /// 
    /// This control contains a mini viewport with a track in "solo mode".
    /// It must handle the following events gracefully and sync with the main viewport.
    /// - drawing selected
    /// - drawing deleted
    /// - tracking status changed
    /// - tracking parameters changed, incl. search/template boxes and thresholds.
    /// - search box is moving
    /// - current point was moved.
    /// Some of these events come from the outside, some can be triggered in the 
    /// control itself, either in the mini viewport, nuds or buttons.
    /// 
    /// Some of these events are raised by the drawing itself, others by a container (metadata, viewport).
    /// We have two viewports handling the same drawing at the same time.
    /// 
    /// Must handle undo/redo gracefully.
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
        private DrawingTrack track;
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
        private ViewportController viewportController = new ViewportController(false, false, false);
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
            viewportController.View.Dock = DockStyle.Fill;
            viewportController.View.DoubleClick += pnlViewport_DoubleClick;
            viewportController.View.MouseEnter += miniViewport_MouseEnter;
            viewportController.View.MouseLeave += miniViewport_MouseLeave;

            NudHelper.FixNudScroll(nudSearchWindowWidth);
            NudHelper.FixNudScroll(nudSearchWindowHeight);
            NudHelper.FixNudScroll(nudObjWindowWidth);
            NudHelper.FixNudScroll(nudObjWindowHeight);
            NudHelper.FixNudScroll(nudMatchTreshold);
            NudHelper.FixNudScroll(nudUpdateThreshold);

            btnStartStop.Image = Properties.Drawings.trackingplay;
            btnStartStop.ImageAlign = ContentAlignment.MiddleLeft;
        }
        #endregion

        #region Public methods
        public void SetHostView(IDrawingHostView hostView)
        {
            this.hostView = hostView;
        }

        public void SetMetadata(Metadata metadata)
        {
            ForgetMetadata();
            this.metadata = metadata;

            metadataRenderer = new MetadataRenderer(metadata, true);

            metadataManipulator = new MetadataManipulator(metadata, screenToolManager);
            metadataManipulator.SetFixedTimestamp(hostView.CurrentTimestamp);
            metadataManipulator.SetFixedKeyframe(-1);
            metadataManipulator.DrawingModified += MetadataManipulator_DrawingModified;

            viewportController.MetadataRenderer = metadataRenderer;
            viewportController.MetadataManipulator = metadataManipulator;
        }

        /// <summary>
        /// Set the drawing this control is managing.
        /// This is called when a drawing is selected or "nothing" is selected.
        /// </summary>
        public void SetDrawing(AbstractDrawing drawing, Metadata metadata, Guid managerId, Guid drawingId)
        {
            if (metadata != null && metadata != this.metadata)
            {
                SetMetadata(metadata);
            }
            
            // Bail out if deselected.
            if (drawing == null)
            {
                manualUpdate = true;
                ForgetDrawing();
                manualUpdate = false;
                return;
            }

            // Bail out if it's the same drawing we are already managing.
            if (this.drawing != null && drawing != null && this.drawing.Id == drawing.Id)
            {
                return;
            }

            manualUpdate = true;
            ForgetDrawing();

            this.drawing = drawing;
            this.track = drawing as DrawingTrack;
            this.managerId = managerId;

            if (drawing == null || !(drawing is IDecorable))
            {
                manualUpdate = false;
                return;
            }

            metadataRenderer.SetSoloMode(true, drawing.Id, true);
            screenToolManager.SetSoloMode(true, drawing.Id, true);

            UpdateContent();

            if (track != null)
            {
                track.TrackingStatusChanged += Track_TrackingStatusChanged;
            }
            
            // Interaction timer for the mini viewport.
            // Right now this is constantly turned on. 
            // Maybe we can enable this only when mouse is over the mini viewport.
            // but we also want to update when the object is moved from the main viewport.
            interactionTimer.Interval = 40; // 25 fps.
            interactionTimer.Tick += InteractionTimer_Tick;
            interactionTimer.Start();

            SetupControls();

            manualUpdate = false;
        }

        /// <summary>
        /// The timestamp, bitmap or tracking parameters were updated from the main viewport.
        /// Update video image and recenter.
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

        private void ForgetMetadata()
        {
            if (metadata == null)
                return;

            metadataManipulator.DrawingModified -= MetadataManipulator_DrawingModified;
            metadata = null;
        }

        private void ForgetDrawing()
        {

            if (track != null)
            {
                track.TrackingStatusChanged -= Track_TrackingStatusChanged;
            }

            drawing = null;
            track = null;
            managerId = Guid.Empty;

            interactionTimer.Tick -= InteractionTimer_Tick;
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
            UpdateStartStopButton();

            this.Height = this.Height - this.ClientRectangle.Height + grpTracking.Bottom + 10;
        }

        private void FitSearchBox()
        {
            // Refit and recenter.
            Size imgSize = viewportController.Bitmap.Size;
            long timestamp = viewportController.Timestamp;
            InitializeDisplayRectangle(imgSize, timestamp);
        }

        private void InitializeDisplayRectangle(Size imgSize, long timestamp)
        {
            // Find an appropriate point to center the mini editor.
            PointF center = imgSize.Center();
            if (track != null)
            {
                center = track.GetPosition(timestamp);
                if (float.IsNaN(center.X) || float.IsNaN(center.Y) || center.IsEmpty)
                    center = imgSize.Center();
            }

            // Scale such that the search window fits in the viewport.
            float scale = 1.0f;
            if (track != null)
            {
                Size searchSize = track.TrackingParameters.SearchWindow;
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
        /// Update the tracking nuds with values from the drawing.
        /// </summary>
        private void UpdateTrackingParameters()
        {
            manualUpdate = true;

            if (track != null)
            {
                TrackingParameters tp = track.TrackingParameters;
                nudSearchWindowWidth.Value = tp.SearchWindow.Width;
                nudSearchWindowHeight.Value = tp.SearchWindow.Height;
                nudObjWindowWidth.Value = tp.BlockWindow.Width;
                nudObjWindowHeight.Value = tp.BlockWindow.Height;
                nudMatchTreshold.Value = (decimal)tp.SimilarityThreshold;
                nudUpdateThreshold.Value = (decimal)tp.TemplateUpdateThreshold;
            }

            manualUpdate = false;
        }

        /// <summary>
        /// Update the start/stop button to reflect the current tracking status.
        /// </summary>
        private void UpdateStartStopButton()
        {
            if (track != null)
            {
                btnStartStop.Image = track.Status == TrackStatus.Interactive ? Properties.Drawings.trackingplay : Properties.Drawings.trackstop;
                btnStartStop.Text = track.Status == TrackStatus.Interactive ? "Start tracking" : "Stop tracking";
            }
        }

        /// <summary>
        /// Force turn tracking ON if it's not the case already.
        /// This should be called for any change done via the panel (controls or mini viewport).
        /// This makes things coherent and improves perfs as the main 
        /// viewport will only draw a subset of the track in this case.
        /// </summary>
        private void EnsureTracking()
        {
            if (track == null)
                return;

            if (track.Status == TrackStatus.Interactive)
                track.StartTracking();
        }

        private void RaiseDrawingModified(DrawingAction action)
        {
            if (drawing != null)
            {
                DrawingModified?.Invoke(this, new DrawingEventArgs(drawing, managerId, action));
            }
        }

        #region Data events

        /// <summary>
        /// The tracking status (active vs inactive) was changed.
        /// This may originate from our own button or from the context menu on the drawing.
        /// Does not raise the DrawingModified event. Should it though?
        /// </summary>
        private void Track_TrackingStatusChanged(object sender, EventArgs e)
        {
            // Update local UI.
            UpdateStartStopButton();
        }

        private void Metadata_DrawingModified(object sender, DrawingEventArgs e)
        {
            log.DebugFormat("Non track drawing modified from the outside.");
            // A non-track drawing was modified from the outside.
            //if (drawing != null && drawing.Id == e.Drawing.Id)
            //    UpdateContent();
        }
        #endregion

        #region UI events to modify the data

        private void nudSearchWindow_ValueChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            int width = (int)nudSearchWindowWidth.Value;
            int height = (int)nudSearchWindowHeight.Value;
            if (track != null)
            {
                // Update the data.
                track.TrackingParameters.SearchWindow = new Size(width, height);
                EnsureTracking();

                // Update local UI.
                FitSearchBox();
                viewportController.Refresh();

                // Update other controllers.
                RaiseDrawingModified(DrawingAction.Resized);
            }
        }

        private void nudObjWindow_ValueChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            int width = (int)nudObjWindowWidth.Value;
            int height = (int)nudObjWindowHeight.Value;
            if (track != null)
            {
                // Update the data.
                track.TrackingParameters.BlockWindow = new Size(width, height);
                EnsureTracking();

                // Update local UI.
                viewportController.Refresh();

                // Update other controllers.
                RaiseDrawingModified(DrawingAction.Resized);
            }
        }

        private void nudThresholds_ValueChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            double matchThreshold = (double)nudMatchTreshold.Value;
            double updateThreshold = (double)nudUpdateThreshold.Value;
            if (track != null)
            {
                // Update the data.
                track.TrackingParameters.SimilarityThreshold = matchThreshold;
                track.TrackingParameters.TemplateUpdateThreshold = updateThreshold;
                EnsureTracking();

                // Update local UI.
                viewportController.Refresh();

                // Update other controllers.
                RaiseDrawingModified(DrawingAction.StateChanged);
            }
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            if (track != null)
            {
                // Update the data.
                track.ToggleTracking();
                
                // Update local UI.
                // Already handled since we listen to tracking status change events.

                // Update other controllers.
                RaiseDrawingModified(DrawingAction.TrackingStatusChanged);
            }
        }

        private void btnTrimTrack_Click(object sender, EventArgs e)
        {
            if (track != null)
            {
                // Update the data.
                track.Trim(viewportController.Timestamp);
                
                // Do not force open tracking. One scenario of trimming is 
                // when the tracking is closed and we go back to the last 
                // good point and trim the rest.

                // Update local UI.
                // Nothing to do.
                // TODO: have a control with the number of tracked frames.

                // Update other controllers.
                RaiseDrawingModified(DrawingAction.StateChanged);
            }
        }
        #endregion

        #region Misc UI events
        private void InteractionTimer_Tick(object sender, EventArgs e)
        {
            viewportController.Refresh();
        }

        private void pnlViewport_Resize(object sender, EventArgs e)
        {
            if (drawing == null)
                return;

            FitSearchBox();
        }

        private void pnlViewport_DoubleClick(object sender, EventArgs e)
        {
            if (drawing == null)
                return;

            FitSearchBox();
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
                        e.Graphics.DrawString("Template matching", e.Font, Brushes.Black, textTopLeft);
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
        private void miniViewport_MouseEnter(object sender, EventArgs e)
        {
            //interactionTimer.Start();
        }

        private void miniViewport_MouseLeave(object sender, EventArgs e)
        {
            //interactionTimer.Stop();
        }

        /// <summary>
        /// The track is being moved from the mini viewport.
        /// </summary>
        private void miniViewport_Moving(object sender, EventArgs e)
        {
            EnsureTracking();

            // Signal to the main viewport for invalidation.
            RaiseDrawingModified(DrawingAction.Moving);
        }

        private void MetadataManipulator_DrawingModified(object sender, DrawingEventArgs e)
        {
            if (track == null || track.Id != e.Drawing.Id)
                return;

            if (e.DrawingAction == DrawingAction.Resizing)
            {
                // Keep the nuds up to date but don't trigger invalidation of the main viewport
                // as this is raised for every mouse move while dragging the corners.
                UpdateTrackingParameters();
            }
            else if (e.DrawingAction == DrawingAction.Resized || e.DrawingAction == DrawingAction.Moved)
            {
                FitSearchBox();
                EnsureTracking();

                // Update other controllers.
                RaiseDrawingModified(DrawingAction.Resized);
            }
        }
        #endregion
    }
}
