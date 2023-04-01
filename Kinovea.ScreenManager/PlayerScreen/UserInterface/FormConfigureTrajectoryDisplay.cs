#region License
/*
Copyright © Joan Charmant 2009.
jcharmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Kinovea.Services;

using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// The dialog lets the user configure a track instance.
    /// Some of the logic is the same as for formConfigureDrawing.
    /// Specifically, we work and update the actual instance in real time. 
    /// If the user finally decide to cancel there's a "fallback to memo" mechanism. 
    /// </summary>
    public partial class formConfigureTrajectoryDisplay : Form
    {
        #region Members
        private bool manualClose = false;
        private Action invalidate;
        private DrawingTrack track;
        private List<AbstractStyleElement> elements = new List<AbstractStyleElement>();
        private ViewportController viewportController = new ViewportController();
        private MetadataRenderer metadataRenderer;
        private MetadataManipulator metadataManipulator;
        private ScreenToolManager screenToolManager = new ScreenToolManager();
        private TrackStatus memoStatus;
        private long timestamp;
        private bool manualUpdate;
        private System.Windows.Forms.Timer interactionTimer = new System.Windows.Forms.Timer();
        #endregion
        
        #region Construction
        public formConfigureTrajectoryDisplay(DrawingTrack track, Metadata metadata, Bitmap image, long timestamp, Action invalidate)
        {
            InitializeComponent();
         
            this.track = track;
            memoStatus = track.Status;
            track.Status = TrackStatus.Configuration;
            track.TrackerParametersChanged += new EventHandler(track_TrackParametersChanged);
            this.invalidate = invalidate;
            this.timestamp = timestamp;

            metadataRenderer = new MetadataRenderer(metadata, true);
            metadataManipulator = new MetadataManipulator(metadata, screenToolManager);
            metadataManipulator.SetFixedTimestamp(timestamp);
            metadataManipulator.SetFixedKeyframe(-1);

            pnlViewport.Controls.Add(viewportController.View);
            viewportController.View.Dock = DockStyle.Fill;
            viewportController.Bitmap = image;
            viewportController.Timestamp = timestamp;
            viewportController.MetadataRenderer = metadataRenderer;
            viewportController.MetadataManipulator = metadataManipulator;
            InitializeDisplayRectangle(image.Size, timestamp);
            viewportController.Refresh();

            track.DrawingStyle.ReadValue();
            
            // Save the current state in case of cancel.
            track.MemorizeState();
            track.DrawingStyle.Memorize();

            InitMarkerCombo();
            InitMeasureLabelTypeCombo();
            InitTrackParameters();
            SetupStyleControls();
            SetCurrentOptions();
            InitCulture();

            interactionTimer.Interval = 15;
            interactionTimer.Tick += InteractionTimer_Tick;
            interactionTimer.Start();
        }
        #endregion
        
        #region Init
        private void InitMeasureLabelTypeCombo()
        {
            // Combo must be filled in the order of the enum.
            cmbMeasureLabelType.Items.Add(track.GetMeasureLabelOptionText(MeasureLabelType.None));
            cmbMeasureLabelType.Items.Add(track.GetMeasureLabelOptionText(MeasureLabelType.Name));
            cmbMeasureLabelType.Items.Add(track.GetMeasureLabelOptionText(MeasureLabelType.Position));
            cmbMeasureLabelType.Items.Add(track.GetMeasureLabelOptionText(MeasureLabelType.TravelDistance));
            cmbMeasureLabelType.Items.Add(track.GetMeasureLabelOptionText(MeasureLabelType.TotalHorizontalDisplacement));
            cmbMeasureLabelType.Items.Add(track.GetMeasureLabelOptionText(MeasureLabelType.TotalVerticalDisplacement));
            cmbMeasureLabelType.Items.Add(track.GetMeasureLabelOptionText(MeasureLabelType.Speed));
            cmbMeasureLabelType.Items.Add(track.GetMeasureLabelOptionText(MeasureLabelType.HorizontalVelocity));
            cmbMeasureLabelType.Items.Add(track.GetMeasureLabelOptionText(MeasureLabelType.VerticalVelocity));
            cmbMeasureLabelType.Items.Add(track.GetMeasureLabelOptionText(MeasureLabelType.Acceleration));
            cmbMeasureLabelType.Items.Add(track.GetMeasureLabelOptionText(MeasureLabelType.HorizontalAcceleration));
            cmbMeasureLabelType.Items.Add(track.GetMeasureLabelOptionText(MeasureLabelType.VerticalAcceleration));
        }
        private void InitMarkerCombo()
        {
            cmbMarker.Items.Add(ScreenManagerLang.dlgConfigureTrajectory_MarkerCross);
            cmbMarker.Items.Add(ScreenManagerLang.dlgConfigureTrajectory_MarkerCircle);
            cmbMarker.Items.Add(ScreenManagerLang.dlgConfigureTrajectory_MarkerTarget);
        }
        private void InitTrackParameters()
        {
            tbBlockWidth.Text = string.Format("{0}", track.TrackerParameters.BlockWindow.Width);
            tbBlockHeight.Text = string.Format("{0}", track.TrackerParameters.BlockWindow.Height);
            tbSearchWidth.Text = string.Format("{0}", track.TrackerParameters.SearchWindow.Width);
            tbSearchHeight.Text = string.Format("{0}", track.TrackerParameters.SearchWindow.Height);
        }
        private void SetupStyleControls()
        {
            // Dynamic loading of track styles but only semi dynamic UI (restricted to 3) for simplicity.
            // Styles should be Color, LineSize and TrackShape.
            foreach(KeyValuePair<string, AbstractStyleElement> pair in track.DrawingStyle.Elements)
            {
                elements.Add(pair.Value);
            }

            if (elements.Count != 3)
                return;
            
            int editorsLeft = 200;
            int lastEditorBottom = 10;
            Size editorSize = new Size(60,20);
                
            foreach(AbstractStyleElement styleElement in elements)
            {
                styleElement.ValueChanged += element_ValueChanged;
                    
                Button btn = new Button();
                btn.Image = styleElement.Icon;
                btn.Size = new Size(20,20);
                btn.Location = new Point(10, lastEditorBottom + 15);
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.BackColor = Color.Transparent;
                
                Label lbl = new Label();
                lbl.Text = styleElement.DisplayName;
                lbl.AutoSize = true;
                lbl.Location = new Point(btn.Right + 10, lastEditorBottom + 20);
                    
                Control miniEditor = styleElement.GetEditor();
                miniEditor.Size = editorSize;
                miniEditor.Location = new Point(editorsLeft, btn.Top);
                    
                lastEditorBottom = miniEditor.Bottom;
                
                grpAppearance.Controls.Add(btn);
                grpAppearance.Controls.Add(lbl);
                grpAppearance.Controls.Add(miniEditor);
            }
        }
        private void SetCurrentOptions()
        {
            tbLabel.Text = track.Name;
            cmbMeasureLabelType.SelectedIndex = (int)track.MeasureLabelType;
            cmbMarker.SelectedIndex = (int)track.Marker;
        }
        private void InitCulture()
        {
            this.Text = "   " + ScreenManagerLang.dlgConfigureTrajectory_Title;

            grpIdentification.Text = ScreenManagerLang.dlgConfigureDrawing_Name;

            grpConfig.Text = ScreenManagerLang.Generic_Configuration;

            lblMarker.Text = ScreenManagerLang.dlgConfigureTrajectory_LabelMarker;
            lblExtra.Text = ScreenManagerLang.dlgConfigureTrajectory_LabelExtraData;
            
            grpAppearance.Text = ScreenManagerLang.Generic_Appearance;

            grpTracking.Text = ScreenManagerLang.dlgConfigureTrajectory_Tracking;
            lblObjectWindow.Text = ScreenManagerLang.dlgConfigureTrajectory_Tracking_ObjectWindow;
            lblSearchWindow.Text = ScreenManagerLang.dlgConfigureTrajectory_Tracking_SearchWindow;

            btnOK.Text = ScreenManagerLang.Generic_Apply;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
        }
        private void InitializeDisplayRectangle(Size imgSize, long timestamp)
        {
            int scale = 2;
            
            // center around current point with zoom.
            PointF position = track.GetPosition(timestamp);
            PointF normalizedPosition = new PointF(position.X / imgSize.Width, position.Y / imgSize.Height);
            SizeF normalizedHostSize = new SizeF((float)pnlViewport.Width / imgSize.Width, (float)pnlViewport.Height / imgSize.Height);
            PointF normalizedHostCenter = new PointF(normalizedHostSize.Width / 2, normalizedHostSize.Height / 2);
            PointF normalizedDisplayLocation = new PointF(normalizedHostCenter.X - (normalizedPosition.X * scale), normalizedHostCenter.Y - (normalizedPosition.Y * scale));
            
            PointF topLeft = new PointF(normalizedDisplayLocation.X * imgSize.Width, normalizedDisplayLocation.Y * imgSize.Height);
            Size fullSize = new Size(imgSize.Width * scale, imgSize.Height * scale);
            
            Rectangle display = new Rectangle((int)topLeft.X, (int)topLeft.Y, fullSize.Width, fullSize.Height);
            viewportController.InitializeDisplayRectangle(display, imgSize);
        }
        #endregion

        private void InteractionTimer_Tick(object sender, EventArgs e)
        {
            viewportController.Refresh();
        }
        private void track_TrackParametersChanged(object sender, EventArgs e)
        {
            manualUpdate = true;
            InitTrackParameters();
            manualUpdate = false;
        }

        #region Event handlers
        private void tbLabel_TextChanged(object sender, EventArgs e)
        {
            track.Name = tbLabel.Text;
            if(invalidate != null) 
                invalidate();
        }
        private void CmbMeasureLabelType_SelectedIndexChanged(object sender, EventArgs e)
        {
            track.MeasureLabelType = (MeasureLabelType)cmbMeasureLabelType.SelectedIndex;
            if(invalidate != null) 
                invalidate();
        }
        private void CmbMarker_SelectedIndexChanged(object sender, EventArgs e)
        {
            track.Marker = (TrackMarker)cmbMarker.SelectedIndex;
            if (invalidate != null) 
                invalidate();
        }

        private void element_ValueChanged(object sender, EventArgs e)
        {
            if(invalidate != null) 
                invalidate();
        }

        private void tbBlockWidth_TextChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            int width;
            bool parsed = ExtractTrackerParameter(tbBlockWidth, out width);
            if (!parsed)
                return;

            Size blockSize = new Size(width, track.TrackerParameters.BlockWindow.Height);
            Size searchSize = track.TrackerParameters.SearchWindow;
            PushTrackerParameters(blockSize, searchSize);
        }
        private void tbBlockHeight_TextChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            int height;
            bool parsed = ExtractTrackerParameter(tbBlockHeight, out height);
            if (!parsed)
                return;

            Size blockSize = new Size(track.TrackerParameters.BlockWindow.Width, height);
            Size searchSize = track.TrackerParameters.SearchWindow;
            PushTrackerParameters(blockSize, searchSize);
        }
        private void tbSearchWidth_TextChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            int width;
            bool parsed = ExtractTrackerParameter(tbSearchWidth, out width);
            if (!parsed)
                return;

            Size blockSize = track.TrackerParameters.BlockWindow;
            Size searchSize = new Size(width, track.TrackerParameters.SearchWindow.Height);
            PushTrackerParameters(blockSize, searchSize);
        }
        private void tbSearchHeight_TextChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            int height;
            bool parsed = ExtractTrackerParameter(tbSearchHeight, out height);
            if (!parsed)
                return;

            Size blockSize = track.TrackerParameters.BlockWindow;
            Size searchSize = new Size(track.TrackerParameters.SearchWindow.Width, height);
            PushTrackerParameters(blockSize, searchSize);
        }
        #endregion
        
        #region OK/Cancel/Closing
        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!manualClose) 
            {
                // Cancel by way of closing the form.
                UnhookEvents();
                Revert();
            }

            track.Status = memoStatus;
        }
        private void UnhookEvents()
        {
            // Unhook style event handlers
            foreach(AbstractStyleElement element in elements)
            {
                element.ValueChanged -= element_ValueChanged;
            }
        }
        private void Revert()
        {
            // Revert to memo and re-update data.
            track.DrawingStyle.Revert();
            track.DrawingStyle.RaiseValueChanged();
            track.RecallState();
            if(invalidate != null) 
                invalidate();
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            UnhookEvents();
            manualClose = true;
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            UnhookEvents();
            Revert();
            manualClose = true;
        }
        #endregion

        private void formConfigureTrajectoryDisplay_Load(object sender, EventArgs e)
        {

        }

        private void pnlViewport_Click(object sender, EventArgs e)
        {
            // Test
            viewportController.Refresh();
        }
        
        private bool ExtractTrackerParameter(TextBox tb, out int value)
        {
            int v;
            bool parsed = int.TryParse(tb.Text, out v);
            tbBlockWidth.ForeColor = parsed ? Color.Black : Color.Red;
            value = parsed ? v : 10;
            return parsed;
        }
        private void PushTrackerParameters(Size block, Size search)
        {
            TrackerParameters old = track.TrackerParameters;
            track.TrackerParameters = new TrackerParameters(old.SimilarityThreshold, old.TemplateUpdateThreshold, old.RefinementNeighborhood, search, block, old.ResetOnMove); ;
            viewportController.Refresh();
        }

    }
}