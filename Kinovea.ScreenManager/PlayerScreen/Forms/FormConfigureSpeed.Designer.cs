namespace Kinovea.ScreenManager
{
    partial class formConfigureSpeed
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
      this.components = new System.ComponentModel.Container();
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.grpHighSpeedCamera = new System.Windows.Forms.GroupBox();
      this.tbCaptureInfo = new System.Windows.Forms.TextBox();
      this.btnResetCapture = new System.Windows.Forms.Button();
      this.tbCapture = new System.Windows.Forms.TextBox();
      this.lblCapture = new System.Windows.Forms.Label();
      this.toolTips = new System.Windows.Forms.ToolTip(this.components);
      this.grpHighSpeedCamera.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(265, 146);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(99, 24);
      this.btnOK.TabIndex = 25;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(370, 146);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(99, 24);
      this.btnCancel.TabIndex = 30;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // grpHighSpeedCamera
      // 
      this.grpHighSpeedCamera.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpHighSpeedCamera.Controls.Add(this.tbCaptureInfo);
      this.grpHighSpeedCamera.Controls.Add(this.btnResetCapture);
      this.grpHighSpeedCamera.Controls.Add(this.tbCapture);
      this.grpHighSpeedCamera.Controls.Add(this.lblCapture);
      this.grpHighSpeedCamera.Location = new System.Drawing.Point(10, 12);
      this.grpHighSpeedCamera.Name = "grpHighSpeedCamera";
      this.grpHighSpeedCamera.Size = new System.Drawing.Size(459, 124);
      this.grpHighSpeedCamera.TabIndex = 29;
      this.grpHighSpeedCamera.TabStop = false;
      this.grpHighSpeedCamera.Text = "High speed camera";
      // 
      // tbCaptureInfo
      // 
      this.tbCaptureInfo.BackColor = System.Drawing.Color.White;
      this.tbCaptureInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.tbCaptureInfo.Location = new System.Drawing.Point(8, 59);
      this.tbCaptureInfo.Multiline = true;
      this.tbCaptureInfo.Name = "tbCaptureInfo";
      this.tbCaptureInfo.ReadOnly = true;
      this.tbCaptureInfo.Size = new System.Drawing.Size(445, 55);
      this.tbCaptureInfo.TabIndex = 26;
      this.tbCaptureInfo.Text = "This option defines the difference between video speed and real time. It impacts " +
    "labels containing times and kinematics measurements. Use it when the video was f" +
    "ilmed in high speed mode. ";
      // 
      // btnResetCapture
      // 
      this.btnResetCapture.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.resettimescale;
      this.btnResetCapture.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
      this.btnResetCapture.Location = new System.Drawing.Point(213, 25);
      this.btnResetCapture.Name = "btnResetCapture";
      this.btnResetCapture.Size = new System.Drawing.Size(20, 20);
      this.btnResetCapture.TabIndex = 25;
      this.btnResetCapture.UseVisualStyleBackColor = true;
      this.btnResetCapture.Click += new System.EventHandler(this.btnResetCapture_Click);
      // 
      // tbCapture
      // 
      this.tbCapture.Location = new System.Drawing.Point(135, 25);
      this.tbCapture.Name = "tbCapture";
      this.tbCapture.Size = new System.Drawing.Size(72, 20);
      this.tbCapture.TabIndex = 24;
      this.tbCapture.Text = "0000";
      this.tbCapture.TextChanged += new System.EventHandler(this.tbCapture_TextChanged);
      this.tbCapture.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbCapture_KeyPress);
      // 
      // lblCapture
      // 
      this.lblCapture.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblCapture.Location = new System.Drawing.Point(10, 22);
      this.lblCapture.Name = "lblCapture";
      this.lblCapture.Size = new System.Drawing.Size(424, 25);
      this.lblCapture.TabIndex = 21;
      this.lblCapture.Text = "Capture framerate:";
      this.lblCapture.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // formConfigureSpeed
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(481, 180);
      this.Controls.Add(this.grpHighSpeedCamera);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnCancel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "formConfigureSpeed";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "   Configure Video time";
      this.grpHighSpeedCamera.ResumeLayout(false);
      this.grpHighSpeedCamera.PerformLayout();
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpHighSpeedCamera;
        private System.Windows.Forms.Label lblCapture;
        private System.Windows.Forms.TextBox tbCapture;
        private System.Windows.Forms.Button btnResetCapture;
        private System.Windows.Forms.ToolTip toolTips;
        private System.Windows.Forms.TextBox tbCaptureInfo;

    }
}