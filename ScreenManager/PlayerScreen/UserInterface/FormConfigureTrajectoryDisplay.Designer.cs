namespace Videa.ScreenManager
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
        	this.grpConfig = new System.Windows.Forms.GroupBox();
        	this.btnLineStyle = new System.Windows.Forms.Button();
        	this.chkShowTrajectory = new System.Windows.Forms.CheckBox();
        	this.tbLabel = new System.Windows.Forms.TextBox();
        	this.lblLabel = new System.Windows.Forms.Label();
        	this.chkShowTarget = new System.Windows.Forms.CheckBox();
        	this.chkShowTitles = new System.Windows.Forms.CheckBox();
        	this.lblStyle = new System.Windows.Forms.Label();
        	this.lblColor = new System.Windows.Forms.Label();
        	this.btnTextColor = new System.Windows.Forms.Button();
        	this.cmbTrackView = new System.Windows.Forms.ComboBox();
        	this.lblMode = new System.Windows.Forms.Label();
        	this.grpConfig.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// btnOK
        	// 
        	this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
        	this.btnOK.Location = new System.Drawing.Point(64, 315);
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
        	this.btnCancel.Location = new System.Drawing.Point(169, 315);
        	this.btnCancel.Name = "btnCancel";
        	this.btnCancel.Size = new System.Drawing.Size(99, 24);
        	this.btnCancel.TabIndex = 50;
        	this.btnCancel.Text = "Cancel";
        	this.btnCancel.UseVisualStyleBackColor = true;
        	this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
        	// 
        	// grpConfig
        	// 
        	this.grpConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.grpConfig.Controls.Add(this.btnLineStyle);
        	this.grpConfig.Controls.Add(this.chkShowTrajectory);
        	this.grpConfig.Controls.Add(this.tbLabel);
        	this.grpConfig.Controls.Add(this.lblLabel);
        	this.grpConfig.Controls.Add(this.chkShowTarget);
        	this.grpConfig.Controls.Add(this.chkShowTitles);
        	this.grpConfig.Controls.Add(this.lblStyle);
        	this.grpConfig.Controls.Add(this.lblColor);
        	this.grpConfig.Controls.Add(this.btnTextColor);
        	this.grpConfig.Location = new System.Drawing.Point(19, 56);
        	this.grpConfig.Name = "grpConfig";
        	this.grpConfig.Size = new System.Drawing.Size(249, 247);
        	this.grpConfig.TabIndex = 29;
        	this.grpConfig.TabStop = false;
        	this.grpConfig.Text = "Configuration";
        	// 
        	// btnLineStyle
        	// 
        	this.btnLineStyle.BackColor = System.Drawing.Color.White;
        	this.btnLineStyle.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnLineStyle.FlatAppearance.BorderColor = System.Drawing.Color.LightGray;
        	this.btnLineStyle.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
        	this.btnLineStyle.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
        	this.btnLineStyle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnLineStyle.Location = new System.Drawing.Point(78, 82);
        	this.btnLineStyle.Name = "btnLineStyle";
        	this.btnLineStyle.Size = new System.Drawing.Size(140, 25);
        	this.btnLineStyle.TabIndex = 44;
        	this.btnLineStyle.UseVisualStyleBackColor = false;
        	this.btnLineStyle.Paint += new System.Windows.Forms.PaintEventHandler(this.btnLineStyle_Paint);
        	this.btnLineStyle.MouseClick += new System.Windows.Forms.MouseEventHandler(this.btnLineStyle_MouseClick);
        	// 
        	// chkShowTrajectory
        	// 
        	this.chkShowTrajectory.AutoSize = true;
        	this.chkShowTrajectory.Checked = true;
        	this.chkShowTrajectory.CheckState = System.Windows.Forms.CheckState.Checked;
        	this.chkShowTrajectory.Location = new System.Drawing.Point(31, 185);
        	this.chkShowTrajectory.Name = "chkShowTrajectory";
        	this.chkShowTrajectory.Size = new System.Drawing.Size(103, 17);
        	this.chkShowTrajectory.TabIndex = 36;
        	this.chkShowTrajectory.Text = "Show Trajectory";
        	this.chkShowTrajectory.UseVisualStyleBackColor = true;
        	this.chkShowTrajectory.Visible = false;
        	this.chkShowTrajectory.CheckedChanged += new System.EventHandler(this.chkShowTrajectory_CheckedChanged);
        	// 
        	// tbLabel
        	// 
        	this.tbLabel.Location = new System.Drawing.Point(107, 129);
        	this.tbLabel.Name = "tbLabel";
        	this.tbLabel.Size = new System.Drawing.Size(111, 20);
        	this.tbLabel.TabIndex = 30;
        	this.tbLabel.TextChanged += new System.EventHandler(this.tbLabel_TextChanged);
        	// 
        	// lblLabel
        	// 
        	this.lblLabel.AutoSize = true;
        	this.lblLabel.Location = new System.Drawing.Point(28, 133);
        	this.lblLabel.Name = "lblLabel";
        	this.lblLabel.Size = new System.Drawing.Size(39, 13);
        	this.lblLabel.TabIndex = 43;
        	this.lblLabel.Text = "Label :";
        	// 
        	// chkShowTarget
        	// 
        	this.chkShowTarget.AutoSize = true;
        	this.chkShowTarget.Checked = true;
        	this.chkShowTarget.CheckState = System.Windows.Forms.CheckState.Checked;
        	this.chkShowTarget.Location = new System.Drawing.Point(31, 205);
        	this.chkShowTarget.Name = "chkShowTarget";
        	this.chkShowTarget.Size = new System.Drawing.Size(141, 17);
        	this.chkShowTarget.TabIndex = 40;
        	this.chkShowTarget.Text = "Show the Target Marker";
        	this.chkShowTarget.UseVisualStyleBackColor = true;
        	this.chkShowTarget.CheckedChanged += new System.EventHandler(this.chkShowTarget_CheckedChanged);
        	// 
        	// chkShowTitles
        	// 
        	this.chkShowTitles.AutoSize = true;
        	this.chkShowTitles.Checked = true;
        	this.chkShowTitles.CheckState = System.Windows.Forms.CheckState.Checked;
        	this.chkShowTitles.Location = new System.Drawing.Point(31, 166);
        	this.chkShowTitles.Name = "chkShowTitles";
        	this.chkShowTitles.Size = new System.Drawing.Size(139, 17);
        	this.chkShowTitles.TabIndex = 35;
        	this.chkShowTitles.Text = "Show Key Images Titles";
        	this.chkShowTitles.UseVisualStyleBackColor = true;
        	this.chkShowTitles.CheckedChanged += new System.EventHandler(this.chkShowTitles_CheckedChanged);
        	// 
        	// lblStyle
        	// 
        	this.lblStyle.AutoSize = true;
        	this.lblStyle.Location = new System.Drawing.Point(27, 88);
        	this.lblStyle.Name = "lblStyle";
        	this.lblStyle.Size = new System.Drawing.Size(36, 13);
        	this.lblStyle.TabIndex = 37;
        	this.lblStyle.Text = "Style :";
        	// 
        	// lblColor
        	// 
        	this.lblColor.AutoSize = true;
        	this.lblColor.Location = new System.Drawing.Point(27, 42);
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
        	this.btnTextColor.Location = new System.Drawing.Point(78, 36);
        	this.btnTextColor.Name = "btnTextColor";
        	this.btnTextColor.Size = new System.Drawing.Size(140, 25);
        	this.btnTextColor.TabIndex = 10;
        	this.btnTextColor.UseVisualStyleBackColor = false;
        	this.btnTextColor.Click += new System.EventHandler(this.btnTextColor_Click);
        	// 
        	// cmbTrackView
        	// 
        	this.cmbTrackView.FormattingEnabled = true;
        	this.cmbTrackView.Items.AddRange(new object[] {
        	        	        	"Trajectory",
        	        	        	"Label Follows",
        	        	        	"Arrow Follows"});
        	this.cmbTrackView.Location = new System.Drawing.Point(97, 19);
        	this.cmbTrackView.Name = "cmbTrackView";
        	this.cmbTrackView.Size = new System.Drawing.Size(159, 21);
        	this.cmbTrackView.TabIndex = 5;
        	this.cmbTrackView.Text = "Trajectory";
        	this.cmbTrackView.SelectedIndexChanged += new System.EventHandler(this.cmbTrackView_SelectedIndexChanged);
        	// 
        	// lblMode
        	// 
        	this.lblMode.AutoSize = true;
        	this.lblMode.Location = new System.Drawing.Point(26, 23);
        	this.lblMode.Name = "lblMode";
        	this.lblMode.Size = new System.Drawing.Size(40, 13);
        	this.lblMode.TabIndex = 38;
        	this.lblMode.Text = "Mode :";
        	// 
        	// formConfigureTrajectoryDisplay
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackColor = System.Drawing.Color.White;
        	this.ClientSize = new System.Drawing.Size(292, 351);
        	this.Controls.Add(this.lblMode);
        	this.Controls.Add(this.cmbTrackView);
        	this.Controls.Add(this.grpConfig);
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
        	this.grpConfig.ResumeLayout(false);
        	this.grpConfig.PerformLayout();
        	this.ResumeLayout(false);
        	this.PerformLayout();
        }
        private System.Windows.Forms.Button btnLineStyle;

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpConfig;
        private System.Windows.Forms.Label lblStyle;
        private System.Windows.Forms.Label lblColor;
        private System.Windows.Forms.Button btnTextColor;
        private System.Windows.Forms.CheckBox chkShowTarget;
        private System.Windows.Forms.CheckBox chkShowTitles;
        private System.Windows.Forms.TextBox tbLabel;
        private System.Windows.Forms.Label lblLabel;
        private System.Windows.Forms.ComboBox cmbTrackView;
        private System.Windows.Forms.Label lblMode;
        private System.Windows.Forms.CheckBox chkShowTrajectory;

    }
}