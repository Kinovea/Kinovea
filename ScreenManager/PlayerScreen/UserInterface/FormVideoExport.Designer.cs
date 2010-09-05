namespace Kinovea.ScreenManager
{
    partial class formVideoExport
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
        	this.groupSaveMethod = new System.Windows.Forms.GroupBox();
        	this.btnSaveBlended = new System.Windows.Forms.Button();
        	this.btnSaveMuxed = new System.Windows.Forms.Button();
        	this.btnSaveAnalysis = new System.Windows.Forms.Button();
        	this.btnSaveVideo = new System.Windows.Forms.Button();
        	this.radioSaveBlended = new System.Windows.Forms.RadioButton();
        	this.radioSaveMuxed = new System.Windows.Forms.RadioButton();
        	this.radioSaveAnalysis = new System.Windows.Forms.RadioButton();
        	this.radioSaveVideo = new System.Windows.Forms.RadioButton();
        	this.groupOptions = new System.Windows.Forms.GroupBox();
        	this.checkSlowMotion = new System.Windows.Forms.CheckBox();
        	this.groupSaveMethod.SuspendLayout();
        	this.groupOptions.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// btnOK
        	// 
        	this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnOK.Location = new System.Drawing.Point(306, 310);
        	this.btnOK.Name = "btnOK";
        	this.btnOK.Size = new System.Drawing.Size(99, 24);
        	this.btnOK.TabIndex = 35;
        	this.btnOK.Text = "Generic_Save";
        	this.btnOK.UseVisualStyleBackColor = true;
        	this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
        	// 
        	// btnCancel
        	// 
        	this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        	this.btnCancel.Location = new System.Drawing.Point(411, 310);
        	this.btnCancel.Name = "btnCancel";
        	this.btnCancel.Size = new System.Drawing.Size(99, 24);
        	this.btnCancel.TabIndex = 40;
        	this.btnCancel.Text = "Generic_Cancel";
        	this.btnCancel.UseVisualStyleBackColor = true;
        	// 
        	// groupSaveMethod
        	// 
        	this.groupSaveMethod.Controls.Add(this.btnSaveBlended);
        	this.groupSaveMethod.Controls.Add(this.btnSaveMuxed);
        	this.groupSaveMethod.Controls.Add(this.btnSaveAnalysis);
        	this.groupSaveMethod.Controls.Add(this.btnSaveVideo);
        	this.groupSaveMethod.Controls.Add(this.radioSaveBlended);
        	this.groupSaveMethod.Controls.Add(this.radioSaveMuxed);
        	this.groupSaveMethod.Controls.Add(this.radioSaveAnalysis);
        	this.groupSaveMethod.Controls.Add(this.radioSaveVideo);
        	this.groupSaveMethod.Location = new System.Drawing.Point(12, 12);
        	this.groupSaveMethod.Name = "groupSaveMethod";
        	this.groupSaveMethod.Size = new System.Drawing.Size(498, 196);
        	this.groupSaveMethod.TabIndex = 25;
        	this.groupSaveMethod.TabStop = false;
        	this.groupSaveMethod.Text = "dlgSaveAnalysisOrVideo_GroupSaveMethod";
        	// 
        	// btnSaveBlended
        	// 
        	this.btnSaveBlended.BackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnSaveBlended.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.saveblended;
        	this.btnSaveBlended.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnSaveBlended.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        	this.btnSaveBlended.FlatAppearance.BorderSize = 0;
        	this.btnSaveBlended.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnSaveBlended.Location = new System.Drawing.Point(21, 105);
        	this.btnSaveBlended.Name = "btnSaveBlended";
        	this.btnSaveBlended.Size = new System.Drawing.Size(48, 32);
        	this.btnSaveBlended.TabIndex = 24;
        	this.btnSaveBlended.UseVisualStyleBackColor = false;
        	this.btnSaveBlended.Click += new System.EventHandler(this.BtnSaveBothClick);
        	// 
        	// btnSaveMuxed
        	// 
        	this.btnSaveMuxed.BackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnSaveMuxed.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.savemuxed;
        	this.btnSaveMuxed.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnSaveMuxed.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        	this.btnSaveMuxed.FlatAppearance.BorderSize = 0;
        	this.btnSaveMuxed.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnSaveMuxed.Location = new System.Drawing.Point(21, 65);
        	this.btnSaveMuxed.Name = "btnSaveMuxed";
        	this.btnSaveMuxed.Size = new System.Drawing.Size(48, 32);
        	this.btnSaveMuxed.TabIndex = 23;
        	this.btnSaveMuxed.UseVisualStyleBackColor = false;
        	this.btnSaveMuxed.Click += new System.EventHandler(this.BtnSaveMuxedClick);
        	// 
        	// btnSaveAnalysis
        	// 
        	this.btnSaveAnalysis.BackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnSaveAnalysis.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.savedata;
        	this.btnSaveAnalysis.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnSaveAnalysis.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        	this.btnSaveAnalysis.FlatAppearance.BorderSize = 0;
        	this.btnSaveAnalysis.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnSaveAnalysis.Location = new System.Drawing.Point(21, 145);
        	this.btnSaveAnalysis.Name = "btnSaveAnalysis";
        	this.btnSaveAnalysis.Size = new System.Drawing.Size(48, 32);
        	this.btnSaveAnalysis.TabIndex = 22;
        	this.btnSaveAnalysis.UseVisualStyleBackColor = false;
        	this.btnSaveAnalysis.Click += new System.EventHandler(this.BtnSaveAnalysisClick);
        	// 
        	// btnSaveVideo
        	// 
        	this.btnSaveVideo.BackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnSaveVideo.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.savevideo;
        	this.btnSaveVideo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnSaveVideo.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        	this.btnSaveVideo.FlatAppearance.BorderSize = 0;
        	this.btnSaveVideo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnSaveVideo.Location = new System.Drawing.Point(21, 25);
        	this.btnSaveVideo.Name = "btnSaveVideo";
        	this.btnSaveVideo.Size = new System.Drawing.Size(48, 32);
        	this.btnSaveVideo.TabIndex = 21;
        	this.btnSaveVideo.UseVisualStyleBackColor = false;
        	this.btnSaveVideo.Click += new System.EventHandler(this.BtnSaveVideoClick);
        	// 
        	// radioSaveBlended
        	// 
        	this.radioSaveBlended.AutoSize = true;
        	this.radioSaveBlended.Location = new System.Drawing.Point(81, 113);
        	this.radioSaveBlended.Name = "radioSaveBlended";
        	this.radioSaveBlended.Size = new System.Drawing.Size(213, 17);
        	this.radioSaveBlended.TabIndex = 20;
        	this.radioSaveBlended.Text = "dlgSaveAnalysisOrVideo_RadioBlended";
        	this.radioSaveBlended.UseVisualStyleBackColor = true;
        	this.radioSaveBlended.CheckedChanged += new System.EventHandler(this.radio_CheckedChanged);
        	// 
        	// radioSaveMuxed
        	// 
        	this.radioSaveMuxed.AutoSize = true;
        	this.radioSaveMuxed.Location = new System.Drawing.Point(81, 73);
        	this.radioSaveMuxed.Name = "radioSaveMuxed";
        	this.radioSaveMuxed.Size = new System.Drawing.Size(206, 17);
        	this.radioSaveMuxed.TabIndex = 15;
        	this.radioSaveMuxed.Text = "dlgSaveAnalysisOrVideo_RadioMuxed";
        	this.radioSaveMuxed.UseVisualStyleBackColor = true;
        	this.radioSaveMuxed.CheckedChanged += new System.EventHandler(this.radio_CheckedChanged);
        	// 
        	// radioSaveAnalysis
        	// 
        	this.radioSaveAnalysis.AutoSize = true;
        	this.radioSaveAnalysis.Location = new System.Drawing.Point(78, 153);
        	this.radioSaveAnalysis.Name = "radioSaveAnalysis";
        	this.radioSaveAnalysis.Size = new System.Drawing.Size(212, 17);
        	this.radioSaveAnalysis.TabIndex = 10;
        	this.radioSaveAnalysis.Text = "dlgSaveAnalysisOrVideo_RadioAnalysis";
        	this.radioSaveAnalysis.UseVisualStyleBackColor = true;
        	this.radioSaveAnalysis.CheckedChanged += new System.EventHandler(this.radio_CheckedChanged);
        	// 
        	// radioSaveVideo
        	// 
        	this.radioSaveVideo.AutoSize = true;
        	this.radioSaveVideo.Location = new System.Drawing.Point(81, 33);
        	this.radioSaveVideo.Name = "radioSaveVideo";
        	this.radioSaveVideo.Size = new System.Drawing.Size(201, 17);
        	this.radioSaveVideo.TabIndex = 5;
        	this.radioSaveVideo.Text = "dlgSaveAnalysisOrVideo_RadioVideo";
        	this.radioSaveVideo.UseVisualStyleBackColor = true;
        	this.radioSaveVideo.CheckedChanged += new System.EventHandler(this.radio_CheckedChanged);
        	// 
        	// groupOptions
        	// 
        	this.groupOptions.Controls.Add(this.checkSlowMotion);
        	this.groupOptions.Location = new System.Drawing.Point(12, 223);
        	this.groupOptions.Name = "groupOptions";
        	this.groupOptions.Size = new System.Drawing.Size(495, 73);
        	this.groupOptions.TabIndex = 26;
        	this.groupOptions.TabStop = false;
        	this.groupOptions.Text = "dlgSaveAnalysisOrVideo_GroupOptions";
        	// 
        	// checkSlowMotion
        	// 
        	this.checkSlowMotion.AutoSize = true;
        	this.checkSlowMotion.Location = new System.Drawing.Point(29, 37);
        	this.checkSlowMotion.Name = "checkSlowMotion";
        	this.checkSlowMotion.Size = new System.Drawing.Size(201, 17);
        	this.checkSlowMotion.TabIndex = 25;
        	this.checkSlowMotion.Text = "dlgSaveAnalysisOrVideo_CheckSlow";
        	this.checkSlowMotion.UseVisualStyleBackColor = true;
        	// 
        	// formVideoExport
        	// 
        	this.AcceptButton = this.btnOK;
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackColor = System.Drawing.Color.White;
        	this.CancelButton = this.btnCancel;
        	this.ClientSize = new System.Drawing.Size(522, 346);
        	this.Controls.Add(this.groupOptions);
        	this.Controls.Add(this.groupSaveMethod);
        	this.Controls.Add(this.btnOK);
        	this.Controls.Add(this.btnCancel);
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        	this.MaximizeBox = false;
        	this.MinimizeBox = false;
        	this.Name = "formVideoExport";
        	this.ShowIcon = false;
        	this.ShowInTaskbar = false;
        	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        	this.Text = "dlgSaveAnalysisOrVideo_Title";
        	this.groupSaveMethod.ResumeLayout(false);
        	this.groupSaveMethod.PerformLayout();
        	this.groupOptions.ResumeLayout(false);
        	this.groupOptions.PerformLayout();
        	this.ResumeLayout(false);
        }
        private System.Windows.Forms.RadioButton radioSaveBlended;
        private System.Windows.Forms.Button btnSaveBlended;
        private System.Windows.Forms.Button btnSaveVideo;
        private System.Windows.Forms.Button btnSaveAnalysis;
        private System.Windows.Forms.Button btnSaveMuxed;

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupSaveMethod;
        private System.Windows.Forms.RadioButton radioSaveMuxed;
        private System.Windows.Forms.RadioButton radioSaveAnalysis;
        private System.Windows.Forms.RadioButton radioSaveVideo;
        private System.Windows.Forms.GroupBox groupOptions;
        private System.Windows.Forms.CheckBox checkSlowMotion;
    }
}