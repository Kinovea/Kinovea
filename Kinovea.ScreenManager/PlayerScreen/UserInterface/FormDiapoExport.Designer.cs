namespace Kinovea.ScreenManager
{
    partial class formDiapoExport
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
        	this.grpboxConfig = new System.Windows.Forms.GroupBox();
        	this.lblInfosFrequency = new System.Windows.Forms.Label();
        	this.trkInterval = new System.Windows.Forms.TrackBar();
        	this.groupSaveMethod = new System.Windows.Forms.GroupBox();
        	this.btnSaveMuxed = new System.Windows.Forms.Button();
        	this.btnSaveVideo = new System.Windows.Forms.Button();
        	this.radioSavePausedVideo = new System.Windows.Forms.RadioButton();
        	this.radioSaveSlideshow = new System.Windows.Forms.RadioButton();
        	this.grpboxConfig.SuspendLayout();
        	((System.ComponentModel.ISupportInitialize)(this.trkInterval)).BeginInit();
        	this.groupSaveMethod.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// btnOK
        	// 
        	this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnOK.Location = new System.Drawing.Point(307, 252);
        	this.btnOK.Name = "btnOK";
        	this.btnOK.Size = new System.Drawing.Size(99, 24);
        	this.btnOK.TabIndex = 10;
        	this.btnOK.Text = "Enregistrer";
        	this.btnOK.UseVisualStyleBackColor = true;
        	this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
        	// 
        	// btnCancel
        	// 
        	this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        	this.btnCancel.Location = new System.Drawing.Point(412, 252);
        	this.btnCancel.Name = "btnCancel";
        	this.btnCancel.Size = new System.Drawing.Size(99, 24);
        	this.btnCancel.TabIndex = 15;
        	this.btnCancel.Text = "Annuler";
        	this.btnCancel.UseVisualStyleBackColor = true;
        	// 
        	// grpboxConfig
        	// 
        	this.grpboxConfig.Anchor = System.Windows.Forms.AnchorStyles.Top;
        	this.grpboxConfig.BackColor = System.Drawing.Color.White;
        	this.grpboxConfig.Controls.Add(this.lblInfosFrequency);
        	this.grpboxConfig.Controls.Add(this.trkInterval);
        	this.grpboxConfig.Location = new System.Drawing.Point(12, 132);
        	this.grpboxConfig.Name = "grpboxConfig";
        	this.grpboxConfig.Size = new System.Drawing.Size(499, 110);
        	this.grpboxConfig.TabIndex = 29;
        	this.grpboxConfig.TabStop = false;
        	this.grpboxConfig.Text = "generic_config";
        	// 
        	// lblInfosFrequency
        	// 
        	this.lblInfosFrequency.Anchor = System.Windows.Forms.AnchorStyles.Top;
        	this.lblInfosFrequency.AutoSize = true;
        	this.lblInfosFrequency.Location = new System.Drawing.Point(21, 31);
        	this.lblInfosFrequency.Name = "lblInfosFrequency";
        	this.lblInfosFrequency.Size = new System.Drawing.Size(282, 13);
        	this.lblInfosFrequency.TabIndex = 1;
        	this.lblInfosFrequency.Text = "Duration of each slide: 40 hundredth.";
        	// 
        	// trkInterval
        	// 
        	this.trkInterval.Anchor = System.Windows.Forms.AnchorStyles.Top;
        	this.trkInterval.Location = new System.Drawing.Point(21, 59);
        	this.trkInterval.Name = "trkInterval";
        	this.trkInterval.Size = new System.Drawing.Size(282, 45);
        	this.trkInterval.TabIndex = 5;
        	this.trkInterval.TickStyle = System.Windows.Forms.TickStyle.None;
        	this.trkInterval.ValueChanged += new System.EventHandler(this.trkInterval_ValueChanged);
        	// 
        	// groupSaveMethod
        	// 
        	this.groupSaveMethod.Controls.Add(this.btnSaveMuxed);
        	this.groupSaveMethod.Controls.Add(this.btnSaveVideo);
        	this.groupSaveMethod.Controls.Add(this.radioSavePausedVideo);
        	this.groupSaveMethod.Controls.Add(this.radioSaveSlideshow);
        	this.groupSaveMethod.Location = new System.Drawing.Point(12, 12);
        	this.groupSaveMethod.Name = "groupSaveMethod";
        	this.groupSaveMethod.Size = new System.Drawing.Size(498, 114);
        	this.groupSaveMethod.TabIndex = 30;
        	this.groupSaveMethod.TabStop = false;
        	this.groupSaveMethod.Text = "dlgDiapoExport_GroupDiapoType";
        	// 
        	// btnSaveMuxed
        	// 
        	this.btnSaveMuxed.BackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnSaveMuxed.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.save_paused_video;
        	this.btnSaveMuxed.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnSaveMuxed.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        	this.btnSaveMuxed.FlatAppearance.BorderSize = 0;
        	this.btnSaveMuxed.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnSaveMuxed.Location = new System.Drawing.Point(21, 65);
        	this.btnSaveMuxed.Name = "btnSaveMuxed";
        	this.btnSaveMuxed.Size = new System.Drawing.Size(48, 32);
        	this.btnSaveMuxed.TabIndex = 23;
        	this.btnSaveMuxed.UseVisualStyleBackColor = false;
        	// 
        	// btnSaveVideo
        	// 
        	this.btnSaveVideo.BackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnSaveVideo.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.saveStaticDiaporama;
        	this.btnSaveVideo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnSaveVideo.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        	this.btnSaveVideo.FlatAppearance.BorderSize = 0;
        	this.btnSaveVideo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnSaveVideo.Location = new System.Drawing.Point(21, 25);
        	this.btnSaveVideo.Name = "btnSaveVideo";
        	this.btnSaveVideo.Size = new System.Drawing.Size(48, 32);
        	this.btnSaveVideo.TabIndex = 21;
        	this.btnSaveVideo.UseVisualStyleBackColor = false;
        	// 
        	// radioSavePausedVideo
        	// 
        	this.radioSavePausedVideo.AutoSize = true;
        	this.radioSavePausedVideo.Location = new System.Drawing.Point(81, 73);
        	this.radioSavePausedVideo.Name = "radioSavePausedVideo";
        	this.radioSavePausedVideo.Size = new System.Drawing.Size(194, 17);
        	this.radioSavePausedVideo.TabIndex = 15;
        	this.radioSavePausedVideo.Text = "dlgDiapoExport_RadioPausedVideo";
        	this.radioSavePausedVideo.UseVisualStyleBackColor = true;
        	// 
        	// radioSaveSlideshow
        	// 
        	this.radioSaveSlideshow.AutoSize = true;
        	this.radioSaveSlideshow.Location = new System.Drawing.Point(81, 33);
        	this.radioSaveSlideshow.Name = "radioSaveSlideshow";
        	this.radioSaveSlideshow.Size = new System.Drawing.Size(179, 17);
        	this.radioSaveSlideshow.TabIndex = 5;
        	this.radioSaveSlideshow.Text = "dlgDiapoExport_RadioSlideshow";
        	this.radioSaveSlideshow.UseVisualStyleBackColor = true;
        	// 
        	// formDiapoExport
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackColor = System.Drawing.Color.White;
        	this.ClientSize = new System.Drawing.Size(523, 288);
        	this.Controls.Add(this.grpboxConfig);
        	this.Controls.Add(this.groupSaveMethod);
        	this.Controls.Add(this.btnOK);
        	this.Controls.Add(this.btnCancel);
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        	this.MaximizeBox = false;
        	this.MinimizeBox = false;
        	this.Name = "formDiapoExport";
        	this.ShowIcon = false;
        	this.ShowInTaskbar = false;
        	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        	this.Text = "dlgDiapoExport_Title";
        	this.grpboxConfig.ResumeLayout(false);
        	this.grpboxConfig.PerformLayout();
        	((System.ComponentModel.ISupportInitialize)(this.trkInterval)).EndInit();
        	this.groupSaveMethod.ResumeLayout(false);
        	this.groupSaveMethod.PerformLayout();
        	this.ResumeLayout(false);
        }
        private System.Windows.Forms.RadioButton radioSavePausedVideo;
        private System.Windows.Forms.RadioButton radioSaveSlideshow;
        private System.Windows.Forms.Button btnSaveVideo;
        private System.Windows.Forms.Button btnSaveMuxed;
        private System.Windows.Forms.GroupBox groupSaveMethod;

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpboxConfig;
        //private FrequencyViewer freqViewer;
        private System.Windows.Forms.Label lblInfosFrequency;
        private System.Windows.Forms.TrackBar trkInterval;
    }
}