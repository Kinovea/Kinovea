namespace Kinovea.ScreenManager
{
    partial class formConfigureTrajectoryDisplay
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.grpAppearance = new System.Windows.Forms.GroupBox();
            this.tbLabel = new System.Windows.Forms.TextBox();
            this.grpConfig = new System.Windows.Forms.GroupBox();
            this.chkBestFitCircle = new System.Windows.Forms.CheckBox();
            this.cmbView = new System.Windows.Forms.ComboBox();
            this.lblView = new System.Windows.Forms.Label();
            this.cmbMarker = new System.Windows.Forms.ComboBox();
            this.lblMarker = new System.Windows.Forms.Label();
            this.cmbExtraData = new System.Windows.Forms.ComboBox();
            this.lblExtra = new System.Windows.Forms.Label();
            this.grpIdentification = new System.Windows.Forms.GroupBox();
            this.grpTracking = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tbSearchHeight = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbSearchWidth = new System.Windows.Forms.TextBox();
            this.tbBlockHeight = new System.Windows.Forms.TextBox();
            this.lblSearchWindow = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tbBlockWidth = new System.Windows.Forms.TextBox();
            this.lblObjectWindow = new System.Windows.Forms.Label();
            this.pnlViewport = new System.Windows.Forms.Panel();
            this.grpConfig.SuspendLayout();
            this.grpIdentification.SuspendLayout();
            this.grpTracking.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(640, 470);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(99, 24);
            this.btnOK.TabIndex = 45;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(745, 470);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(99, 24);
            this.btnCancel.TabIndex = 50;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // grpAppearance
            // 
            this.grpAppearance.Location = new System.Drawing.Point(12, 232);
            this.grpAppearance.Name = "grpAppearance";
            this.grpAppearance.Size = new System.Drawing.Size(297, 128);
            this.grpAppearance.TabIndex = 29;
            this.grpAppearance.TabStop = false;
            this.grpAppearance.Text = "Generic_Appearance";
            // 
            // tbLabel
            // 
            this.tbLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLabel.Location = new System.Drawing.Point(24, 19);
            this.tbLabel.Name = "tbLabel";
            this.tbLabel.Size = new System.Drawing.Size(260, 20);
            this.tbLabel.TabIndex = 30;
            this.tbLabel.TextChanged += new System.EventHandler(this.tbLabel_TextChanged);
            // 
            // grpConfig
            // 
            this.grpConfig.Controls.Add(this.chkBestFitCircle);
            this.grpConfig.Controls.Add(this.cmbView);
            this.grpConfig.Controls.Add(this.lblView);
            this.grpConfig.Controls.Add(this.cmbMarker);
            this.grpConfig.Controls.Add(this.lblMarker);
            this.grpConfig.Controls.Add(this.cmbExtraData);
            this.grpConfig.Controls.Add(this.lblExtra);
            this.grpConfig.Location = new System.Drawing.Point(12, 74);
            this.grpConfig.Name = "grpConfig";
            this.grpConfig.Size = new System.Drawing.Size(297, 152);
            this.grpConfig.TabIndex = 51;
            this.grpConfig.TabStop = false;
            this.grpConfig.Text = "Generic_Configuration";
            // 
            // chkBestFitCircle
            // 
            this.chkBestFitCircle.AutoSize = true;
            this.chkBestFitCircle.Location = new System.Drawing.Point(24, 115);
            this.chkBestFitCircle.Name = "chkBestFitCircle";
            this.chkBestFitCircle.Size = new System.Drawing.Size(122, 17);
            this.chkBestFitCircle.TabIndex = 54;
            this.chkBestFitCircle.Text = "Display best fit circle";
            this.chkBestFitCircle.UseVisualStyleBackColor = true;
            this.chkBestFitCircle.CheckedChanged += new System.EventHandler(this.chkBestFitCircle_CheckedChanged);
            // 
            // cmbView
            // 
            this.cmbView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbView.FormattingEnabled = true;
            this.cmbView.Location = new System.Drawing.Point(146, 19);
            this.cmbView.Name = "cmbView";
            this.cmbView.Size = new System.Drawing.Size(138, 21);
            this.cmbView.TabIndex = 53;
            this.cmbView.SelectedIndexChanged += new System.EventHandler(this.CmbView_SelectedIndexChanged);
            // 
            // lblView
            // 
            this.lblView.AutoSize = true;
            this.lblView.Location = new System.Drawing.Point(21, 24);
            this.lblView.Name = "lblView";
            this.lblView.Size = new System.Drawing.Size(49, 13);
            this.lblView.TabIndex = 52;
            this.lblView.Text = "Visibility :";
            // 
            // cmbMarker
            // 
            this.cmbMarker.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbMarker.FormattingEnabled = true;
            this.cmbMarker.Location = new System.Drawing.Point(146, 48);
            this.cmbMarker.Name = "cmbMarker";
            this.cmbMarker.Size = new System.Drawing.Size(138, 21);
            this.cmbMarker.TabIndex = 51;
            this.cmbMarker.SelectedIndexChanged += new System.EventHandler(this.CmbMarker_SelectedIndexChanged);
            // 
            // lblMarker
            // 
            this.lblMarker.AutoSize = true;
            this.lblMarker.Location = new System.Drawing.Point(21, 53);
            this.lblMarker.Name = "lblMarker";
            this.lblMarker.Size = new System.Drawing.Size(46, 13);
            this.lblMarker.TabIndex = 50;
            this.lblMarker.Text = "Marker :";
            // 
            // cmbExtraData
            // 
            this.cmbExtraData.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbExtraData.FormattingEnabled = true;
            this.cmbExtraData.Location = new System.Drawing.Point(146, 75);
            this.cmbExtraData.Name = "cmbExtraData";
            this.cmbExtraData.Size = new System.Drawing.Size(138, 21);
            this.cmbExtraData.TabIndex = 46;
            this.cmbExtraData.SelectedIndexChanged += new System.EventHandler(this.CmbExtraData_SelectedIndexChanged);
            // 
            // lblExtra
            // 
            this.lblExtra.AutoSize = true;
            this.lblExtra.Location = new System.Drawing.Point(21, 80);
            this.lblExtra.Name = "lblExtra";
            this.lblExtra.Size = new System.Drawing.Size(77, 13);
            this.lblExtra.TabIndex = 45;
            this.lblExtra.Text = "Measurement :";
            // 
            // grpIdentification
            // 
            this.grpIdentification.Controls.Add(this.tbLabel);
            this.grpIdentification.Location = new System.Drawing.Point(12, 12);
            this.grpIdentification.Name = "grpIdentification";
            this.grpIdentification.Size = new System.Drawing.Size(297, 56);
            this.grpIdentification.TabIndex = 30;
            this.grpIdentification.TabStop = false;
            this.grpIdentification.Text = "Name";
            // 
            // grpTracking
            // 
            this.grpTracking.Controls.Add(this.label2);
            this.grpTracking.Controls.Add(this.label1);
            this.grpTracking.Controls.Add(this.tbSearchHeight);
            this.grpTracking.Controls.Add(this.label5);
            this.grpTracking.Controls.Add(this.tbSearchWidth);
            this.grpTracking.Controls.Add(this.tbBlockHeight);
            this.grpTracking.Controls.Add(this.lblSearchWindow);
            this.grpTracking.Controls.Add(this.label4);
            this.grpTracking.Controls.Add(this.tbBlockWidth);
            this.grpTracking.Controls.Add(this.lblObjectWindow);
            this.grpTracking.Location = new System.Drawing.Point(9, 366);
            this.grpTracking.Name = "grpTracking";
            this.grpTracking.Size = new System.Drawing.Size(300, 93);
            this.grpTracking.TabIndex = 44;
            this.grpTracking.TabStop = false;
            this.grpTracking.Text = "Tracking";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(248, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(18, 13);
            this.label2.TabIndex = 54;
            this.label2.Text = "px";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(248, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(18, 13);
            this.label1.TabIndex = 53;
            this.label1.Text = "px";
            // 
            // tbSearchHeight
            // 
            this.tbSearchHeight.Location = new System.Drawing.Point(210, 53);
            this.tbSearchHeight.Name = "tbSearchHeight";
            this.tbSearchHeight.Size = new System.Drawing.Size(30, 20);
            this.tbSearchHeight.TabIndex = 52;
            this.tbSearchHeight.TextChanged += new System.EventHandler(this.tbSearchHeight_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(191, 56);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(13, 13);
            this.label5.TabIndex = 51;
            this.label5.Text = "×";
            // 
            // tbSearchWidth
            // 
            this.tbSearchWidth.Location = new System.Drawing.Point(151, 53);
            this.tbSearchWidth.Name = "tbSearchWidth";
            this.tbSearchWidth.Size = new System.Drawing.Size(30, 20);
            this.tbSearchWidth.TabIndex = 50;
            this.tbSearchWidth.TextChanged += new System.EventHandler(this.tbSearchWidth_TextChanged);
            // 
            // tbBlockHeight
            // 
            this.tbBlockHeight.Location = new System.Drawing.Point(210, 27);
            this.tbBlockHeight.Name = "tbBlockHeight";
            this.tbBlockHeight.Size = new System.Drawing.Size(30, 20);
            this.tbBlockHeight.TabIndex = 48;
            this.tbBlockHeight.TextChanged += new System.EventHandler(this.tbBlockHeight_TextChanged);
            // 
            // lblSearchWindow
            // 
            this.lblSearchWindow.AutoSize = true;
            this.lblSearchWindow.Location = new System.Drawing.Point(21, 57);
            this.lblSearchWindow.Name = "lblSearchWindow";
            this.lblSearchWindow.Size = new System.Drawing.Size(86, 13);
            this.lblSearchWindow.TabIndex = 47;
            this.lblSearchWindow.Text = "Search window :";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(191, 30);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(13, 13);
            this.label4.TabIndex = 45;
            this.label4.Text = "×";
            // 
            // tbBlockWidth
            // 
            this.tbBlockWidth.Location = new System.Drawing.Point(151, 27);
            this.tbBlockWidth.Name = "tbBlockWidth";
            this.tbBlockWidth.Size = new System.Drawing.Size(30, 20);
            this.tbBlockWidth.TabIndex = 30;
            this.tbBlockWidth.TextChanged += new System.EventHandler(this.tbBlockWidth_TextChanged);
            // 
            // lblObjectWindow
            // 
            this.lblObjectWindow.AutoSize = true;
            this.lblObjectWindow.Location = new System.Drawing.Point(21, 30);
            this.lblObjectWindow.Name = "lblObjectWindow";
            this.lblObjectWindow.Size = new System.Drawing.Size(83, 13);
            this.lblObjectWindow.TabIndex = 43;
            this.lblObjectWindow.Text = "Object window :";
            // 
            // pnlViewport
            // 
            this.pnlViewport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlViewport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.pnlViewport.Location = new System.Drawing.Point(315, 12);
            this.pnlViewport.Name = "pnlViewport";
            this.pnlViewport.Size = new System.Drawing.Size(529, 452);
            this.pnlViewport.TabIndex = 52;
            this.pnlViewport.Click += new System.EventHandler(this.pnlViewport_Click);
            // 
            // formConfigureTrajectoryDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(856, 506);
            this.Controls.Add(this.pnlViewport);
            this.Controls.Add(this.grpTracking);
            this.Controls.Add(this.grpIdentification);
            this.Controls.Add(this.grpConfig);
            this.Controls.Add(this.grpAppearance);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formConfigureTrajectoryDisplay";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "   Configure trajectory tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_FormClosing);
            this.Load += new System.EventHandler(this.formConfigureTrajectoryDisplay_Load);
            this.grpConfig.ResumeLayout(false);
            this.grpConfig.PerformLayout();
            this.grpIdentification.ResumeLayout(false);
            this.grpIdentification.PerformLayout();
            this.grpTracking.ResumeLayout(false);
            this.grpTracking.PerformLayout();
            this.ResumeLayout(false);

        }
        private System.Windows.Forms.Label lblExtra;
        private System.Windows.Forms.ComboBox cmbExtraData;
        private System.Windows.Forms.GroupBox grpAppearance;

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpConfig;
        private System.Windows.Forms.TextBox tbLabel;
        private System.Windows.Forms.ComboBox cmbMarker;
        private System.Windows.Forms.Label lblMarker;
        private System.Windows.Forms.ComboBox cmbView;
        private System.Windows.Forms.Label lblView;
        private System.Windows.Forms.GroupBox grpIdentification;
        private System.Windows.Forms.GroupBox grpTracking;
        private System.Windows.Forms.TextBox tbSearchHeight;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbSearchWidth;
        private System.Windows.Forms.TextBox tbBlockHeight;
        private System.Windows.Forms.Label lblSearchWindow;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbBlockWidth;
        private System.Windows.Forms.Label lblObjectWindow;
        private System.Windows.Forms.Panel pnlViewport;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkBestFitCircle;

    }
}