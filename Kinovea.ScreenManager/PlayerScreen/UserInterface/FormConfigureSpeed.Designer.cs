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
            this.grpConfig = new System.Windows.Forms.GroupBox();
            this.tbCaptureFPS = new System.Windows.Forms.TextBox();
            this.lblSlowFactor = new System.Windows.Forms.Label();
            this.lblVideoFPS = new System.Windows.Forms.Label();
            this.lblCaptureFPS = new System.Windows.Forms.Label();
            this.btnReset = new System.Windows.Forms.Button();
            this.toolTips = new System.Windows.Forms.ToolTip(this.components);
            this.grpConfig.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(110, 190);
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
            this.btnCancel.Location = new System.Drawing.Point(215, 190);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(99, 24);
            this.btnCancel.TabIndex = 30;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // grpConfig
            // 
            this.grpConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpConfig.Controls.Add(this.btnReset);
            this.grpConfig.Controls.Add(this.tbCaptureFPS);
            this.grpConfig.Controls.Add(this.lblSlowFactor);
            this.grpConfig.Controls.Add(this.lblVideoFPS);
            this.grpConfig.Controls.Add(this.lblCaptureFPS);
            this.grpConfig.Location = new System.Drawing.Point(12, 12);
            this.grpConfig.Name = "grpConfig";
            this.grpConfig.Size = new System.Drawing.Size(304, 172);
            this.grpConfig.TabIndex = 29;
            this.grpConfig.TabStop = false;
            this.grpConfig.Text = "Configuration";
            // 
            // tbFPSOriginal
            // 
            this.tbCaptureFPS.Location = new System.Drawing.Point(101, 65);
            this.tbCaptureFPS.Name = "tbFPSOriginal";
            this.tbCaptureFPS.Size = new System.Drawing.Size(51, 20);
            this.tbCaptureFPS.TabIndex = 24;
            this.tbCaptureFPS.Text = "0000";
            this.tbCaptureFPS.TextChanged += new System.EventHandler(this.tbCaptureFPS_TextChanged);
            this.tbCaptureFPS.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbCaptureFPS_KeyPress);
            // 
            // lblSlowFactor
            // 
            this.lblSlowFactor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSlowFactor.Location = new System.Drawing.Point(10, 137);
            this.lblSlowFactor.Name = "lblSlowFactor";
            this.lblSlowFactor.Size = new System.Drawing.Size(288, 21);
            this.lblSlowFactor.TabIndex = 23;
            this.lblSlowFactor.Text = "Video is {0} times slower than original.";
            // 
            // lblFPSDisplayTime
            // 
            this.lblVideoFPS.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVideoFPS.Location = new System.Drawing.Point(10, 111);
            this.lblVideoFPS.Name = "lblFPSDisplayTime";
            this.lblVideoFPS.Size = new System.Drawing.Size(288, 23);
            this.lblVideoFPS.TabIndex = 22;
            this.lblVideoFPS.Text = "Number of frames per seconds at display time : {0} fps.";
            // 
            // lblFPSCaptureTime
            // 
            this.lblCaptureFPS.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCaptureFPS.Location = new System.Drawing.Point(10, 22);
            this.lblCaptureFPS.Name = "lblFPSCaptureTime";
            this.lblCaptureFPS.Size = new System.Drawing.Size(288, 30);
            this.lblCaptureFPS.TabIndex = 21;
            this.lblCaptureFPS.Text = "Number of frames per second at capture time :";
            this.lblCaptureFPS.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // btnReset
            // 
            this.btnReset.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.resettimescale;
            this.btnReset.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnReset.Location = new System.Drawing.Point(75, 65);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(20, 20);
            this.btnReset.TabIndex = 25;
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // formConfigureSpeed
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(326, 224);
            this.Controls.Add(this.grpConfig);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formConfigureSpeed";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "   Configure Original Speed";
            this.grpConfig.ResumeLayout(false);
            this.grpConfig.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpConfig;
        private System.Windows.Forms.Label lblCaptureFPS;
        private System.Windows.Forms.Label lblSlowFactor;
        private System.Windows.Forms.Label lblVideoFPS;
        private System.Windows.Forms.TextBox tbCaptureFPS;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.ToolTip toolTips;

    }
}