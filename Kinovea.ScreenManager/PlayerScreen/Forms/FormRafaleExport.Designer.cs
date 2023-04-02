namespace Kinovea.ScreenManager
{
    partial class FormRafaleExport
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
            this.grpboxConfig = new System.Windows.Forms.GroupBox();
            this.lblTimeDecimation = new System.Windows.Forms.Label();
            this.lblFrameDecimation = new System.Windows.Forms.Label();
            this.trkDecimate = new System.Windows.Forms.TrackBar();
            this.chkKeyframesOnly = new System.Windows.Forms.CheckBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblTotalFrames = new System.Windows.Forms.Label();
            this.grpboxConfig.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkDecimate)).BeginInit();
            this.SuspendLayout();
            // 
            // grpboxConfig
            // 
            this.grpboxConfig.BackColor = System.Drawing.Color.White;
            this.grpboxConfig.Controls.Add(this.lblTotalFrames);
            this.grpboxConfig.Controls.Add(this.lblTimeDecimation);
            this.grpboxConfig.Controls.Add(this.lblFrameDecimation);
            this.grpboxConfig.Controls.Add(this.trkDecimate);
            this.grpboxConfig.Location = new System.Drawing.Point(7, 46);
            this.grpboxConfig.Name = "grpboxConfig";
            this.grpboxConfig.Size = new System.Drawing.Size(280, 152);
            this.grpboxConfig.TabIndex = 32;
            this.grpboxConfig.TabStop = false;
            this.grpboxConfig.Text = "Configuration";
            // 
            // lblTimeDecimation
            // 
            this.lblTimeDecimation.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblTimeDecimation.AutoSize = true;
            this.lblTimeDecimation.Location = new System.Drawing.Point(20, 96);
            this.lblTimeDecimation.Name = "lblTimeDecimation";
            this.lblTimeDecimation.Size = new System.Drawing.Size(135, 13);
            this.lblTimeDecimation.TabIndex = 16;
            this.lblTimeDecimation.Text = "Export one image every 2s.";
            // 
            // lblFrameDecimation
            // 
            this.lblFrameDecimation.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblFrameDecimation.AutoSize = true;
            this.lblFrameDecimation.Location = new System.Drawing.Point(20, 74);
            this.lblFrameDecimation.Name = "lblFrameDecimation";
            this.lblFrameDecimation.Size = new System.Drawing.Size(166, 13);
            this.lblFrameDecimation.TabIndex = 1;
            this.lblFrameDecimation.Text = "Export one image every 5 images.";
            // 
            // trkDecimate
            // 
            this.trkDecimate.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.trkDecimate.Location = new System.Drawing.Point(6, 24);
            this.trkDecimate.Name = "trkDecimate";
            this.trkDecimate.Size = new System.Drawing.Size(268, 45);
            this.trkDecimate.TabIndex = 5;
            this.trkDecimate.ValueChanged += new System.EventHandler(this.trkDecimate_ValueChanged);
            // 
            // chkKeyframesOnly
            // 
            this.chkKeyframesOnly.AutoSize = true;
            this.chkKeyframesOnly.Location = new System.Drawing.Point(12, 12);
            this.chkKeyframesOnly.Name = "chkKeyframesOnly";
            this.chkKeyframesOnly.Size = new System.Drawing.Size(137, 17);
            this.chkKeyframesOnly.TabIndex = 15;
            this.chkKeyframesOnly.Text = "Export key images only.";
            this.chkKeyframesOnly.UseVisualStyleBackColor = true;
            this.chkKeyframesOnly.CheckedChanged += new System.EventHandler(this.chkKeyframesOnly_CheckedChanged);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(91, 211);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(99, 24);
            this.btnOK.TabIndex = 29;
            this.btnOK.Text = "Save";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(196, 211);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(99, 24);
            this.btnCancel.TabIndex = 30;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // lblTotalFrames
            // 
            this.lblTotalFrames.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblTotalFrames.AutoSize = true;
            this.lblTotalFrames.Location = new System.Drawing.Point(20, 121);
            this.lblTotalFrames.Name = "lblTotalFrames";
            this.lblTotalFrames.Size = new System.Drawing.Size(99, 13);
            this.lblTotalFrames.TabIndex = 17;
            this.lblTotalFrames.Text = "Total exported : 83.";
            // 
            // FormRafaleExport2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(303, 242);
            this.Controls.Add(this.grpboxConfig);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.chkKeyframesOnly);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormRafaleExport2";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Rafale export";
            this.grpboxConfig.ResumeLayout(false);
            this.grpboxConfig.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkDecimate)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox grpboxConfig;
        private System.Windows.Forms.Label lblTimeDecimation;
        private System.Windows.Forms.CheckBox chkKeyframesOnly;
        private System.Windows.Forms.Label lblFrameDecimation;
        private System.Windows.Forms.TrackBar trkDecimate;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblTotalFrames;
    }
}