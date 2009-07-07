namespace Kinovea.ScreenManager
{
    partial class formConfigureMosaic
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
        	this.lblInfosTotalFrames = new System.Windows.Forms.Label();
        	this.grpboxConfig = new System.Windows.Forms.GroupBox();
        	this.cbRTL = new System.Windows.Forms.CheckBox();
        	this.trkInterval = new System.Windows.Forms.TrackBar();
        	this.grpboxConfig.SuspendLayout();
        	((System.ComponentModel.ISupportInitialize)(this.trkInterval)).BeginInit();
        	this.SuspendLayout();
        	// 
        	// btnOK
        	// 
        	this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
        	this.btnOK.Location = new System.Drawing.Point(158, 186);
        	this.btnOK.Name = "btnOK";
        	this.btnOK.Size = new System.Drawing.Size(99, 24);
        	this.btnOK.TabIndex = 20;
        	this.btnOK.Text = "Apply";
        	this.btnOK.UseVisualStyleBackColor = true;
        	// 
        	// btnCancel
        	// 
        	this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        	this.btnCancel.Location = new System.Drawing.Point(263, 186);
        	this.btnCancel.Name = "btnCancel";
        	this.btnCancel.Size = new System.Drawing.Size(99, 24);
        	this.btnCancel.TabIndex = 25;
        	this.btnCancel.Text = "Cancel";
        	this.btnCancel.UseVisualStyleBackColor = true;
        	// 
        	// lblInfosTotalFrames
        	// 
        	this.lblInfosTotalFrames.AutoSize = true;
        	this.lblInfosTotalFrames.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.lblInfosTotalFrames.Location = new System.Drawing.Point(12, 91);
        	this.lblInfosTotalFrames.Name = "lblInfosTotalFrames";
        	this.lblInfosTotalFrames.Size = new System.Drawing.Size(196, 13);
        	this.lblInfosTotalFrames.TabIndex = 2;
        	this.lblInfosTotalFrames.Text = "dlgConfigureMosaic_LabelImages";
        	// 
        	// grpboxConfig
        	// 
        	this.grpboxConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.grpboxConfig.BackColor = System.Drawing.Color.White;
        	this.grpboxConfig.Controls.Add(this.cbRTL);
        	this.grpboxConfig.Controls.Add(this.lblInfosTotalFrames);
        	this.grpboxConfig.Controls.Add(this.trkInterval);
        	this.grpboxConfig.Location = new System.Drawing.Point(12, 12);
        	this.grpboxConfig.Name = "grpboxConfig";
        	this.grpboxConfig.Size = new System.Drawing.Size(350, 162);
        	this.grpboxConfig.TabIndex = 28;
        	this.grpboxConfig.TabStop = false;
        	this.grpboxConfig.Text = "Configuration";
        	// 
        	// cbRTL
        	// 
        	this.cbRTL.Location = new System.Drawing.Point(12, 124);
        	this.cbRTL.Name = "cbRTL";
        	this.cbRTL.Size = new System.Drawing.Size(260, 19);
        	this.cbRTL.TabIndex = 18;
        	this.cbRTL.Text = "dlgConfigureMosaic_cbRightToLeft";
        	this.cbRTL.UseVisualStyleBackColor = true;
        	// 
        	// trkInterval
        	// 
        	this.trkInterval.LargeChange = 4;
        	this.trkInterval.Location = new System.Drawing.Point(12, 30);
        	this.trkInterval.Maximum = 100;
        	this.trkInterval.Minimum = 4;
        	this.trkInterval.Name = "trkInterval";
        	this.trkInterval.Size = new System.Drawing.Size(332, 45);
        	this.trkInterval.TabIndex = 5;
        	this.trkInterval.TickFrequency = 4;
        	this.trkInterval.Value = 25;
        	this.trkInterval.ValueChanged += new System.EventHandler(this.trkInterval_ValueChanged);
        	// 
        	// formConfigureMosaic
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackColor = System.Drawing.Color.White;
        	this.ClientSize = new System.Drawing.Size(375, 222);
        	this.Controls.Add(this.grpboxConfig);
        	this.Controls.Add(this.btnOK);
        	this.Controls.Add(this.btnCancel);
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
        	this.MaximizeBox = false;
        	this.MinimizeBox = false;
        	this.Name = "formConfigureMosaic";
        	this.ShowIcon = false;
        	this.ShowInTaskbar = false;
        	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        	this.Text = "dlgConfigureMosaic_Title";
        	this.grpboxConfig.ResumeLayout(false);
        	this.grpboxConfig.PerformLayout();
        	((System.ComponentModel.ISupportInitialize)(this.trkInterval)).EndInit();
        	this.ResumeLayout(false);
        }
        private System.Windows.Forms.CheckBox cbRTL;

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpboxConfig;
        private System.Windows.Forms.Label lblInfosTotalFrames;
        private System.Windows.Forms.TrackBar trkInterval;
    }
}