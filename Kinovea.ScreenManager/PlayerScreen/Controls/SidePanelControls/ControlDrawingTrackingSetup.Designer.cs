
namespace Kinovea.ScreenManager
{
    partial class ControlDrawingTrackingSetup
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
      this.pnlViewport = new System.Windows.Forms.Panel();
      this.grpTracking = new System.Windows.Forms.GroupBox();
      this.btnTrimTrack = new System.Windows.Forms.Button();
      this.btnStartStop = new System.Windows.Forms.Button();
      this.nudUpdateThreshold = new System.Windows.Forms.NumericUpDown();
      this.lblUpdateThreshold = new System.Windows.Forms.Label();
      this.nudMatchTreshold = new System.Windows.Forms.NumericUpDown();
      this.lblMatchThreshold = new System.Windows.Forms.Label();
      this.nudSearchWindowHeight = new System.Windows.Forms.NumericUpDown();
      this.nudObjWindowHeight = new System.Windows.Forms.NumericUpDown();
      this.nudSearchWindowWidth = new System.Windows.Forms.NumericUpDown();
      this.nudObjWindowWidth = new System.Windows.Forms.NumericUpDown();
      this.label3 = new System.Windows.Forms.Label();
      this.cbTrackingAlgorithm = new System.Windows.Forms.ComboBox();
      this.lblSearchWindowPixels = new System.Windows.Forms.Label();
      this.lblObjectWindowPixels = new System.Windows.Forms.Label();
      this.lblSearchWindowX = new System.Windows.Forms.Label();
      this.lblSearchWindow = new System.Windows.Forms.Label();
      this.lblObjectWindowX = new System.Windows.Forms.Label();
      this.lblObjectWindow = new System.Windows.Forms.Label();
      this.grpTracking.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudUpdateThreshold)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudMatchTreshold)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudSearchWindowHeight)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudObjWindowHeight)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudSearchWindowWidth)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudObjWindowWidth)).BeginInit();
      this.SuspendLayout();
      // 
      // pnlViewport
      // 
      this.pnlViewport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pnlViewport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
      this.pnlViewport.Location = new System.Drawing.Point(0, 0);
      this.pnlViewport.Name = "pnlViewport";
      this.pnlViewport.Size = new System.Drawing.Size(362, 270);
      this.pnlViewport.TabIndex = 53;
      this.pnlViewport.Resize += new System.EventHandler(this.pnlViewport_Resize);
      // 
      // grpTracking
      // 
      this.grpTracking.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpTracking.BackColor = System.Drawing.Color.White;
      this.grpTracking.Controls.Add(this.btnTrimTrack);
      this.grpTracking.Controls.Add(this.btnStartStop);
      this.grpTracking.Controls.Add(this.nudUpdateThreshold);
      this.grpTracking.Controls.Add(this.lblUpdateThreshold);
      this.grpTracking.Controls.Add(this.nudMatchTreshold);
      this.grpTracking.Controls.Add(this.lblMatchThreshold);
      this.grpTracking.Controls.Add(this.nudSearchWindowHeight);
      this.grpTracking.Controls.Add(this.nudObjWindowHeight);
      this.grpTracking.Controls.Add(this.nudSearchWindowWidth);
      this.grpTracking.Controls.Add(this.nudObjWindowWidth);
      this.grpTracking.Controls.Add(this.label3);
      this.grpTracking.Controls.Add(this.cbTrackingAlgorithm);
      this.grpTracking.Controls.Add(this.lblSearchWindowPixels);
      this.grpTracking.Controls.Add(this.lblObjectWindowPixels);
      this.grpTracking.Controls.Add(this.lblSearchWindowX);
      this.grpTracking.Controls.Add(this.lblSearchWindow);
      this.grpTracking.Controls.Add(this.lblObjectWindowX);
      this.grpTracking.Controls.Add(this.lblObjectWindow);
      this.grpTracking.Location = new System.Drawing.Point(0, 276);
      this.grpTracking.Name = "grpTracking";
      this.grpTracking.Size = new System.Drawing.Size(362, 251);
      this.grpTracking.TabIndex = 54;
      this.grpTracking.TabStop = false;
      this.grpTracking.Text = "Tracking";
      // 
      // btnTrimTrack
      // 
      this.btnTrimTrack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnTrimTrack.ForeColor = System.Drawing.Color.Black;
      this.btnTrimTrack.Image = global::Kinovea.ScreenManager.Properties.Resources.bin_empty;
      this.btnTrimTrack.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.btnTrimTrack.Location = new System.Drawing.Point(28, 213);
      this.btnTrimTrack.Name = "btnTrimTrack";
      this.btnTrimTrack.Size = new System.Drawing.Size(157, 27);
      this.btnTrimTrack.TabIndex = 68;
      this.btnTrimTrack.Text = "Delete end of track";
      this.btnTrimTrack.UseVisualStyleBackColor = true;
      this.btnTrimTrack.Click += new System.EventHandler(this.btnTrimTrack_Click);
      // 
      // btnStartStop
      // 
      this.btnStartStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnStartStop.AutoSize = true;
      this.btnStartStop.ForeColor = System.Drawing.Color.Black;
      this.btnStartStop.Location = new System.Drawing.Point(28, 180);
      this.btnStartStop.Name = "btnStartStop";
      this.btnStartStop.Size = new System.Drawing.Size(157, 27);
      this.btnStartStop.TabIndex = 67;
      this.btnStartStop.Text = "Start tracking";
      this.btnStartStop.UseVisualStyleBackColor = true;
      this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
      // 
      // nudUpdateThreshold
      // 
      this.nudUpdateThreshold.DecimalPlaces = 2;
      this.nudUpdateThreshold.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
      this.nudUpdateThreshold.Location = new System.Drawing.Point(175, 147);
      this.nudUpdateThreshold.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.nudUpdateThreshold.Name = "nudUpdateThreshold";
      this.nudUpdateThreshold.Size = new System.Drawing.Size(51, 20);
      this.nudUpdateThreshold.TabIndex = 66;
      this.nudUpdateThreshold.ValueChanged += new System.EventHandler(this.nudThresholds_ValueChanged);
      // 
      // lblUpdateThreshold
      // 
      this.lblUpdateThreshold.AutoSize = true;
      this.lblUpdateThreshold.ForeColor = System.Drawing.SystemColors.ControlText;
      this.lblUpdateThreshold.Location = new System.Drawing.Point(25, 149);
      this.lblUpdateThreshold.Name = "lblUpdateThreshold";
      this.lblUpdateThreshold.Size = new System.Drawing.Size(91, 13);
      this.lblUpdateThreshold.TabIndex = 65;
      this.lblUpdateThreshold.Text = "Update threshold:";
      // 
      // nudMatchTreshold
      // 
      this.nudMatchTreshold.DecimalPlaces = 2;
      this.nudMatchTreshold.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
      this.nudMatchTreshold.Location = new System.Drawing.Point(175, 116);
      this.nudMatchTreshold.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.nudMatchTreshold.Name = "nudMatchTreshold";
      this.nudMatchTreshold.Size = new System.Drawing.Size(51, 20);
      this.nudMatchTreshold.TabIndex = 64;
      this.nudMatchTreshold.ValueChanged += new System.EventHandler(this.nudThresholds_ValueChanged);
      // 
      // lblMatchThreshold
      // 
      this.lblMatchThreshold.AutoSize = true;
      this.lblMatchThreshold.ForeColor = System.Drawing.SystemColors.ControlText;
      this.lblMatchThreshold.Location = new System.Drawing.Point(25, 118);
      this.lblMatchThreshold.Name = "lblMatchThreshold";
      this.lblMatchThreshold.Size = new System.Drawing.Size(86, 13);
      this.lblMatchThreshold.TabIndex = 63;
      this.lblMatchThreshold.Text = "Match threshold:";
      // 
      // nudSearchWindowHeight
      // 
      this.nudSearchWindowHeight.Location = new System.Drawing.Point(259, 53);
      this.nudSearchWindowHeight.Maximum = new decimal(new int[] {
            400,
            0,
            0,
            0});
      this.nudSearchWindowHeight.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
      this.nudSearchWindowHeight.Name = "nudSearchWindowHeight";
      this.nudSearchWindowHeight.Size = new System.Drawing.Size(46, 20);
      this.nudSearchWindowHeight.TabIndex = 62;
      this.nudSearchWindowHeight.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
      this.nudSearchWindowHeight.ValueChanged += new System.EventHandler(this.nudSearchWindow_ValueChanged);
      // 
      // nudObjWindowHeight
      // 
      this.nudObjWindowHeight.Location = new System.Drawing.Point(259, 85);
      this.nudObjWindowHeight.Maximum = new decimal(new int[] {
            400,
            0,
            0,
            0});
      this.nudObjWindowHeight.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
      this.nudObjWindowHeight.Name = "nudObjWindowHeight";
      this.nudObjWindowHeight.Size = new System.Drawing.Size(46, 20);
      this.nudObjWindowHeight.TabIndex = 61;
      this.nudObjWindowHeight.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
      this.nudObjWindowHeight.ValueChanged += new System.EventHandler(this.nudObjWindow_ValueChanged);
      // 
      // nudSearchWindowWidth
      // 
      this.nudSearchWindowWidth.Location = new System.Drawing.Point(175, 53);
      this.nudSearchWindowWidth.Maximum = new decimal(new int[] {
            400,
            0,
            0,
            0});
      this.nudSearchWindowWidth.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
      this.nudSearchWindowWidth.Name = "nudSearchWindowWidth";
      this.nudSearchWindowWidth.Size = new System.Drawing.Size(51, 20);
      this.nudSearchWindowWidth.TabIndex = 60;
      this.nudSearchWindowWidth.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
      this.nudSearchWindowWidth.ValueChanged += new System.EventHandler(this.nudSearchWindow_ValueChanged);
      // 
      // nudObjWindowWidth
      // 
      this.nudObjWindowWidth.Location = new System.Drawing.Point(175, 85);
      this.nudObjWindowWidth.Maximum = new decimal(new int[] {
            400,
            0,
            0,
            0});
      this.nudObjWindowWidth.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
      this.nudObjWindowWidth.Name = "nudObjWindowWidth";
      this.nudObjWindowWidth.Size = new System.Drawing.Size(51, 20);
      this.nudObjWindowWidth.TabIndex = 59;
      this.nudObjWindowWidth.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
      this.nudObjWindowWidth.ValueChanged += new System.EventHandler(this.nudObjWindow_ValueChanged);
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.ForeColor = System.Drawing.SystemColors.ControlText;
      this.label3.Location = new System.Drawing.Point(25, 29);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(97, 13);
      this.label3.TabIndex = 55;
      this.label3.Text = "Tracking algorithm:";
      // 
      // cbTrackingAlgorithm
      // 
      this.cbTrackingAlgorithm.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
      this.cbTrackingAlgorithm.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbTrackingAlgorithm.FormattingEnabled = true;
      this.cbTrackingAlgorithm.ItemHeight = 16;
      this.cbTrackingAlgorithm.Location = new System.Drawing.Point(175, 20);
      this.cbTrackingAlgorithm.Name = "cbTrackingAlgorithm";
      this.cbTrackingAlgorithm.Size = new System.Drawing.Size(154, 22);
      this.cbTrackingAlgorithm.TabIndex = 52;
      this.cbTrackingAlgorithm.SelectedIndexChanged += new System.EventHandler(this.cbTrackingAlgorithm_SelectedIndexChanged);
      // 
      // lblSearchWindowPixels
      // 
      this.lblSearchWindowPixels.AutoSize = true;
      this.lblSearchWindowPixels.ForeColor = System.Drawing.SystemColors.ControlText;
      this.lblSearchWindowPixels.Location = new System.Drawing.Point(311, 55);
      this.lblSearchWindowPixels.Name = "lblSearchWindowPixels";
      this.lblSearchWindowPixels.Size = new System.Drawing.Size(18, 13);
      this.lblSearchWindowPixels.TabIndex = 54;
      this.lblSearchWindowPixels.Text = "px";
      // 
      // lblObjectWindowPixels
      // 
      this.lblObjectWindowPixels.AutoSize = true;
      this.lblObjectWindowPixels.ForeColor = System.Drawing.SystemColors.ControlText;
      this.lblObjectWindowPixels.Location = new System.Drawing.Point(311, 88);
      this.lblObjectWindowPixels.Name = "lblObjectWindowPixels";
      this.lblObjectWindowPixels.Size = new System.Drawing.Size(18, 13);
      this.lblObjectWindowPixels.TabIndex = 53;
      this.lblObjectWindowPixels.Text = "px";
      // 
      // lblSearchWindowX
      // 
      this.lblSearchWindowX.AutoSize = true;
      this.lblSearchWindowX.ForeColor = System.Drawing.SystemColors.ControlText;
      this.lblSearchWindowX.Location = new System.Drawing.Point(237, 56);
      this.lblSearchWindowX.Name = "lblSearchWindowX";
      this.lblSearchWindowX.Size = new System.Drawing.Size(13, 13);
      this.lblSearchWindowX.TabIndex = 51;
      this.lblSearchWindowX.Text = "×";
      // 
      // lblSearchWindow
      // 
      this.lblSearchWindow.AutoSize = true;
      this.lblSearchWindow.ForeColor = System.Drawing.SystemColors.ControlText;
      this.lblSearchWindow.Location = new System.Drawing.Point(25, 57);
      this.lblSearchWindow.Name = "lblSearchWindow";
      this.lblSearchWindow.Size = new System.Drawing.Size(83, 13);
      this.lblSearchWindow.TabIndex = 47;
      this.lblSearchWindow.Text = "Search window:";
      // 
      // lblObjectWindowX
      // 
      this.lblObjectWindowX.AutoSize = true;
      this.lblObjectWindowX.ForeColor = System.Drawing.SystemColors.ControlText;
      this.lblObjectWindowX.Location = new System.Drawing.Point(237, 88);
      this.lblObjectWindowX.Name = "lblObjectWindowX";
      this.lblObjectWindowX.Size = new System.Drawing.Size(13, 13);
      this.lblObjectWindowX.TabIndex = 45;
      this.lblObjectWindowX.Text = "×";
      // 
      // lblObjectWindow
      // 
      this.lblObjectWindow.AutoSize = true;
      this.lblObjectWindow.ForeColor = System.Drawing.SystemColors.ControlText;
      this.lblObjectWindow.Location = new System.Drawing.Point(25, 88);
      this.lblObjectWindow.Name = "lblObjectWindow";
      this.lblObjectWindow.Size = new System.Drawing.Size(80, 13);
      this.lblObjectWindow.TabIndex = 43;
      this.lblObjectWindow.Text = "Object window:";
      // 
      // ControlDrawingTrackingSetup
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Gainsboro;
      this.Controls.Add(this.grpTracking);
      this.Controls.Add(this.pnlViewport);
      this.DoubleBuffered = true;
      this.ForeColor = System.Drawing.Color.Gray;
      this.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
      this.Name = "ControlDrawingTrackingSetup";
      this.Size = new System.Drawing.Size(362, 528);
      this.grpTracking.ResumeLayout(false);
      this.grpTracking.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudUpdateThreshold)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudMatchTreshold)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudSearchWindowHeight)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudObjWindowHeight)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudSearchWindowWidth)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudObjWindowWidth)).EndInit();
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlViewport;
        private System.Windows.Forms.GroupBox grpTracking;
        private System.Windows.Forms.Label lblSearchWindowPixels;
        private System.Windows.Forms.Label lblObjectWindowPixels;
        private System.Windows.Forms.Label lblSearchWindowX;
        private System.Windows.Forms.Label lblSearchWindow;
        private System.Windows.Forms.Label lblObjectWindowX;
        private System.Windows.Forms.Label lblObjectWindow;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbTrackingAlgorithm;
        private System.Windows.Forms.NumericUpDown nudObjWindowHeight;
        private System.Windows.Forms.NumericUpDown nudSearchWindowWidth;
        private System.Windows.Forms.NumericUpDown nudObjWindowWidth;
        private System.Windows.Forms.NumericUpDown nudMatchTreshold;
        private System.Windows.Forms.Label lblMatchThreshold;
        private System.Windows.Forms.NumericUpDown nudSearchWindowHeight;
        private System.Windows.Forms.NumericUpDown nudUpdateThreshold;
        private System.Windows.Forms.Label lblUpdateThreshold;
        private System.Windows.Forms.Button btnTrimTrack;
        private System.Windows.Forms.Button btnStartStop;
    }
}
