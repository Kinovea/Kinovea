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
        	this.cmbExtraData = new System.Windows.Forms.ComboBox();
        	this.btnLabel = new System.Windows.Forms.Button();
        	this.lblExtra = new System.Windows.Forms.Label();
        	this.btnFocus = new System.Windows.Forms.Button();
        	this.btnComplete = new System.Windows.Forms.Button();
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
        	this.btnOK.Location = new System.Drawing.Point(170, 341);
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
        	this.btnCancel.Location = new System.Drawing.Point(275, 341);
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
        	this.grpAppearance.Location = new System.Drawing.Point(12, 255);
        	this.grpAppearance.Name = "grpAppearance";
        	this.grpAppearance.Size = new System.Drawing.Size(362, 72);
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
        	this.btnLineStyle.Location = new System.Drawing.Point(253, 24);
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
        	this.lblStyle.Location = new System.Drawing.Point(191, 30);
        	this.lblStyle.Name = "lblStyle";
        	this.lblStyle.Size = new System.Drawing.Size(36, 13);
        	this.lblStyle.TabIndex = 37;
        	this.lblStyle.Text = "Style :";
        	// 
        	// lblColor
        	// 
        	this.lblColor.AutoSize = true;
        	this.lblColor.Location = new System.Drawing.Point(21, 30);
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
        	this.tbLabel.Location = new System.Drawing.Point(146, 158);
        	this.tbLabel.Name = "tbLabel";
        	this.tbLabel.Size = new System.Drawing.Size(167, 20);
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
        	this.grpConfig.Controls.Add(this.cmbExtraData);
        	this.grpConfig.Controls.Add(this.btnLabel);
        	this.grpConfig.Controls.Add(this.lblExtra);
        	this.grpConfig.Controls.Add(this.btnFocus);
        	this.grpConfig.Controls.Add(this.btnComplete);
        	this.grpConfig.Controls.Add(this.radioLabel);
        	this.grpConfig.Controls.Add(this.radioFocus);
        	this.grpConfig.Controls.Add(this.radioComplete);
        	this.grpConfig.Controls.Add(this.tbLabel);
        	this.grpConfig.Controls.Add(this.lblLabel);
        	this.grpConfig.Location = new System.Drawing.Point(12, 12);
        	this.grpConfig.Name = "grpConfig";
        	this.grpConfig.Size = new System.Drawing.Size(362, 237);
        	this.grpConfig.TabIndex = 51;
        	this.grpConfig.TabStop = false;
        	this.grpConfig.Text = "Generic_Configuration";
        	// 
        	// cmbExtraData
        	// 
        	this.cmbExtraData.FormattingEnabled = true;
        	this.cmbExtraData.Location = new System.Drawing.Point(146, 195);
        	this.cmbExtraData.Name = "cmbExtraData";
        	this.cmbExtraData.Size = new System.Drawing.Size(167, 21);
        	this.cmbExtraData.TabIndex = 46;
        	this.cmbExtraData.SelectedIndexChanged += new System.EventHandler(this.CmbExtraData_SelectedIndexChanged);
        	// 
        	// btnLabel
        	// 
        	this.btnLabel.BackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnLabel.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.trajconflabel3;
        	this.btnLabel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnLabel.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        	this.btnLabel.FlatAppearance.BorderSize = 0;
        	this.btnLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnLabel.Location = new System.Drawing.Point(21, 105);
        	this.btnLabel.Name = "btnLabel";
        	this.btnLabel.Size = new System.Drawing.Size(48, 32);
        	this.btnLabel.TabIndex = 49;
        	this.btnLabel.UseVisualStyleBackColor = false;
        	this.btnLabel.Click += new System.EventHandler(this.btnLabel_Click);
        	// 
        	// lblExtra
        	// 
        	this.lblExtra.AutoSize = true;
        	this.lblExtra.Location = new System.Drawing.Point(21, 200);
        	this.lblExtra.Name = "lblExtra";
        	this.lblExtra.Size = new System.Drawing.Size(97, 13);
        	this.lblExtra.TabIndex = 45;
        	this.lblExtra.Text = "Distance / Speed :";
        	// 
        	// btnFocus
        	// 
        	this.btnFocus.BackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnFocus.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.trajconffocus3;
        	this.btnFocus.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnFocus.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        	this.btnFocus.FlatAppearance.BorderSize = 0;
        	this.btnFocus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnFocus.Location = new System.Drawing.Point(21, 65);
        	this.btnFocus.Name = "btnFocus";
        	this.btnFocus.Size = new System.Drawing.Size(48, 32);
        	this.btnFocus.TabIndex = 48;
        	this.btnFocus.UseVisualStyleBackColor = false;
        	this.btnFocus.Click += new System.EventHandler(this.btnFocus_Click);
        	// 
        	// btnComplete
        	// 
        	this.btnComplete.BackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnComplete.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.trajconfall3;
        	this.btnComplete.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnComplete.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        	this.btnComplete.FlatAppearance.BorderSize = 0;
        	this.btnComplete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnComplete.Location = new System.Drawing.Point(21, 25);
        	this.btnComplete.Name = "btnComplete";
        	this.btnComplete.Size = new System.Drawing.Size(48, 32);
        	this.btnComplete.TabIndex = 47;
        	this.btnComplete.UseVisualStyleBackColor = false;
        	this.btnComplete.Click += new System.EventHandler(this.btnComplete_Click);
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
        	this.ClientSize = new System.Drawing.Size(386, 377);
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
        	this.Text = "   Configure Trajectory Display";
        	this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formConfigureTrajectoryDisplay_FormClosing);
        	this.grpAppearance.ResumeLayout(false);
        	this.grpAppearance.PerformLayout();
        	this.grpConfig.ResumeLayout(false);
        	this.grpConfig.PerformLayout();
        	this.ResumeLayout(false);
        }
        private System.Windows.Forms.Label lblExtra;
        private System.Windows.Forms.ComboBox cmbExtraData;
        private System.Windows.Forms.Button btnLabel;
        private System.Windows.Forms.Button btnFocus;
        private System.Windows.Forms.Button btnComplete;
        private System.Windows.Forms.GroupBox grpAppearance;
        private System.Windows.Forms.RadioButton radioLabel;
        private System.Windows.Forms.RadioButton radioFocus;
        private System.Windows.Forms.RadioButton radioComplete;
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