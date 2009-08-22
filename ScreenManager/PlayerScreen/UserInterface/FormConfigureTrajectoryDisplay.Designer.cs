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
        	this.btnLineStyle = new System.Windows.Forms.Button();
        	this.lblStyle = new System.Windows.Forms.Label();
        	this.lblColor = new System.Windows.Forms.Label();
        	this.btnTextColor = new System.Windows.Forms.Button();
        	this.tbLabel = new System.Windows.Forms.TextBox();
        	this.lblLabel = new System.Windows.Forms.Label();
        	this.grpConfig = new System.Windows.Forms.GroupBox();
        	this.btnSaveBlended = new System.Windows.Forms.Button();
        	this.btnSaveMuxed = new System.Windows.Forms.Button();
        	this.btnSaveVideo = new System.Windows.Forms.Button();
        	this.radioLabel = new System.Windows.Forms.RadioButton();
        	this.radioFocus = new System.Windows.Forms.RadioButton();
        	this.radioComplete = new System.Windows.Forms.RadioButton();
        	this.grpAppearance.SuspendLayout();
        	this.grpConfig.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// btnOK
        	// 
        	this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
        	this.btnOK.Location = new System.Drawing.Point(170, 337);
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
        	this.btnCancel.Location = new System.Drawing.Point(275, 337);
        	this.btnCancel.Name = "btnCancel";
        	this.btnCancel.Size = new System.Drawing.Size(99, 24);
        	this.btnCancel.TabIndex = 50;
        	this.btnCancel.Text = "Cancel";
        	this.btnCancel.UseVisualStyleBackColor = true;
        	this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
        	// 
        	// grpAppearance
        	// 
        	this.grpAppearance.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.grpAppearance.Controls.Add(this.btnLineStyle);
        	this.grpAppearance.Controls.Add(this.lblStyle);
        	this.grpAppearance.Controls.Add(this.lblColor);
        	this.grpAppearance.Controls.Add(this.btnTextColor);
        	this.grpAppearance.Location = new System.Drawing.Point(12, 210);
        	this.grpAppearance.Name = "grpAppearance";
        	this.grpAppearance.Size = new System.Drawing.Size(362, 110);
        	this.grpAppearance.TabIndex = 29;
        	this.grpAppearance.TabStop = false;
        	this.grpAppearance.Text = "Generic_Appearance";
        	// 
        	// btnLineStyle
        	// 
        	this.btnLineStyle.BackColor = System.Drawing.Color.White;
        	this.btnLineStyle.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnLineStyle.FlatAppearance.BorderColor = System.Drawing.Color.LightGray;
        	this.btnLineStyle.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
        	this.btnLineStyle.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
        	this.btnLineStyle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnLineStyle.Location = new System.Drawing.Point(79, 63);
        	this.btnLineStyle.Name = "btnLineStyle";
        	this.btnLineStyle.Size = new System.Drawing.Size(84, 25);
        	this.btnLineStyle.TabIndex = 44;
        	this.btnLineStyle.UseVisualStyleBackColor = false;
        	this.btnLineStyle.Paint += new System.Windows.Forms.PaintEventHandler(this.btnLineStyle_Paint);
        	this.btnLineStyle.MouseClick += new System.Windows.Forms.MouseEventHandler(this.btnLineStyle_MouseClick);
        	// 
        	// lblStyle
        	// 
        	this.lblStyle.AutoSize = true;
        	this.lblStyle.Location = new System.Drawing.Point(14, 69);
        	this.lblStyle.Name = "lblStyle";
        	this.lblStyle.Size = new System.Drawing.Size(36, 13);
        	this.lblStyle.TabIndex = 37;
        	this.lblStyle.Text = "Style :";
        	// 
        	// lblColor
        	// 
        	this.lblColor.AutoSize = true;
        	this.lblColor.Location = new System.Drawing.Point(14, 30);
        	this.lblColor.Name = "lblColor";
        	this.lblColor.Size = new System.Drawing.Size(37, 13);
        	this.lblColor.TabIndex = 36;
        	this.lblColor.Text = "Color :";
        	// 
        	// btnTextColor
        	// 
        	this.btnTextColor.BackColor = System.Drawing.Color.Black;
        	this.btnTextColor.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnTextColor.FlatAppearance.BorderSize = 0;
        	this.btnTextColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnTextColor.Location = new System.Drawing.Point(80, 24);
        	this.btnTextColor.Name = "btnTextColor";
        	this.btnTextColor.Size = new System.Drawing.Size(83, 25);
        	this.btnTextColor.TabIndex = 10;
        	this.btnTextColor.UseVisualStyleBackColor = false;
        	this.btnTextColor.Click += new System.EventHandler(this.btnTextColor_Click);
        	// 
        	// tbLabel
        	// 
        	this.tbLabel.Location = new System.Drawing.Point(80, 158);
        	this.tbLabel.Name = "tbLabel";
        	this.tbLabel.Size = new System.Drawing.Size(181, 20);
        	this.tbLabel.TabIndex = 30;
        	this.tbLabel.TextChanged += new System.EventHandler(this.tbLabel_TextChanged);
        	// 
        	// lblLabel
        	// 
        	this.lblLabel.AutoSize = true;
        	this.lblLabel.Location = new System.Drawing.Point(21, 161);
        	this.lblLabel.Name = "lblLabel";
        	this.lblLabel.Size = new System.Drawing.Size(39, 13);
        	this.lblLabel.TabIndex = 43;
        	this.lblLabel.Text = "Label :";
        	// 
        	// grpConfig
        	// 
        	this.grpConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.grpConfig.Controls.Add(this.btnSaveBlended);
        	this.grpConfig.Controls.Add(this.btnSaveMuxed);
        	this.grpConfig.Controls.Add(this.btnSaveVideo);
        	this.grpConfig.Controls.Add(this.radioLabel);
        	this.grpConfig.Controls.Add(this.radioFocus);
        	this.grpConfig.Controls.Add(this.radioComplete);
        	this.grpConfig.Controls.Add(this.tbLabel);
        	this.grpConfig.Controls.Add(this.lblLabel);
        	this.grpConfig.Location = new System.Drawing.Point(12, 12);
        	this.grpConfig.Name = "grpConfig";
        	this.grpConfig.Size = new System.Drawing.Size(362, 192);
        	this.grpConfig.TabIndex = 51;
        	this.grpConfig.TabStop = false;
        	this.grpConfig.Text = "Generic_Configuration";
        	// 
        	// btnSaveBlended
        	// 
        	this.btnSaveBlended.BackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnSaveBlended.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.trajconflabel3;
        	this.btnSaveBlended.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnSaveBlended.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        	this.btnSaveBlended.FlatAppearance.BorderSize = 0;
        	this.btnSaveBlended.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnSaveBlended.Location = new System.Drawing.Point(21, 105);
        	this.btnSaveBlended.Name = "btnSaveBlended";
        	this.btnSaveBlended.Size = new System.Drawing.Size(48, 32);
        	this.btnSaveBlended.TabIndex = 49;
        	this.btnSaveBlended.UseVisualStyleBackColor = false;
        	// 
        	// btnSaveMuxed
        	// 
        	this.btnSaveMuxed.BackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnSaveMuxed.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.trajconffocus3;
        	this.btnSaveMuxed.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnSaveMuxed.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        	this.btnSaveMuxed.FlatAppearance.BorderSize = 0;
        	this.btnSaveMuxed.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnSaveMuxed.Location = new System.Drawing.Point(21, 65);
        	this.btnSaveMuxed.Name = "btnSaveMuxed";
        	this.btnSaveMuxed.Size = new System.Drawing.Size(48, 32);
        	this.btnSaveMuxed.TabIndex = 48;
        	this.btnSaveMuxed.UseVisualStyleBackColor = false;
        	// 
        	// btnSaveVideo
        	// 
        	this.btnSaveVideo.BackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnSaveVideo.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.trajconfall3;
        	this.btnSaveVideo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnSaveVideo.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        	this.btnSaveVideo.FlatAppearance.BorderSize = 0;
        	this.btnSaveVideo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnSaveVideo.Location = new System.Drawing.Point(21, 25);
        	this.btnSaveVideo.Name = "btnSaveVideo";
        	this.btnSaveVideo.Size = new System.Drawing.Size(48, 32);
        	this.btnSaveVideo.TabIndex = 47;
        	this.btnSaveVideo.UseVisualStyleBackColor = false;
        	// 
        	// radioLabel
        	// 
        	this.radioLabel.AutoSize = true;
        	this.radioLabel.Location = new System.Drawing.Point(81, 113);
        	this.radioLabel.Name = "radioLabel";
        	this.radioLabel.Size = new System.Drawing.Size(191, 17);
        	this.radioLabel.TabIndex = 46;
        	this.radioLabel.Text = "dlgConfigureTrajectory_RadioLabel";
        	this.radioLabel.UseVisualStyleBackColor = true;
        	this.radioLabel.CheckedChanged += new System.EventHandler(this.RadioViews_CheckedChanged);
        	// 
        	// radioFocus
        	// 
        	this.radioFocus.AutoSize = true;
        	this.radioFocus.Location = new System.Drawing.Point(81, 73);
        	this.radioFocus.Name = "radioFocus";
        	this.radioFocus.Size = new System.Drawing.Size(194, 17);
        	this.radioFocus.TabIndex = 45;
        	this.radioFocus.Text = "dlgConfigureTrajectory_RadioFocus";
        	this.radioFocus.UseVisualStyleBackColor = true;
        	this.radioFocus.CheckedChanged += new System.EventHandler(this.RadioViews_CheckedChanged);
        	// 
        	// radioComplete
        	// 
        	this.radioComplete.AutoSize = true;
        	this.radioComplete.Checked = true;
        	this.radioComplete.Location = new System.Drawing.Point(81, 33);
        	this.radioComplete.Name = "radioComplete";
        	this.radioComplete.Size = new System.Drawing.Size(209, 17);
        	this.radioComplete.TabIndex = 44;
        	this.radioComplete.TabStop = true;
        	this.radioComplete.Text = "dlgConfigureTrajectory_RadioComplete";
        	this.radioComplete.UseVisualStyleBackColor = true;
        	this.radioComplete.CheckedChanged += new System.EventHandler(this.RadioViews_CheckedChanged);
        	// 
        	// formConfigureTrajectoryDisplay
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackColor = System.Drawing.Color.White;
        	this.ClientSize = new System.Drawing.Size(386, 373);
        	this.Controls.Add(this.grpConfig);
        	this.Controls.Add(this.grpAppearance);
        	this.Controls.Add(this.btnOK);
        	this.Controls.Add(this.btnCancel);
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
        	this.MaximizeBox = false;
        	this.MinimizeBox = false;
        	this.Name = "formConfigureTrajectoryDisplay";
        	this.ShowIcon = false;
        	this.ShowInTaskbar = false;
        	this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
        	this.Text = "   Configure Trajectory Display";
        	this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formConfigureTrajectoryDisplay_FormClosing);
        	this.grpAppearance.ResumeLayout(false);
        	this.grpAppearance.PerformLayout();
        	this.grpConfig.ResumeLayout(false);
        	this.grpConfig.PerformLayout();
        	this.ResumeLayout(false);
        }
        private System.Windows.Forms.GroupBox grpAppearance;
        private System.Windows.Forms.RadioButton radioLabel;
        private System.Windows.Forms.RadioButton radioFocus;
        private System.Windows.Forms.RadioButton radioComplete;
        private System.Windows.Forms.Button btnSaveVideo;
        private System.Windows.Forms.Button btnSaveMuxed;
        private System.Windows.Forms.Button btnSaveBlended;
        private System.Windows.Forms.Button btnLineStyle;

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpConfig;
        private System.Windows.Forms.Label lblStyle;
        private System.Windows.Forms.Label lblColor;
        private System.Windows.Forms.Button btnTextColor;
        private System.Windows.Forms.TextBox tbLabel;
        private System.Windows.Forms.Label lblLabel;

    }
}