namespace Kinovea.ScreenManager
{
    partial class formConfigureFading
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
            this.lblValue = new System.Windows.Forms.Label();
            this.trkValue = new System.Windows.Forms.TrackBar();
            this.chkAlwaysVisible = new System.Windows.Forms.CheckBox();
            this.chkDefault = new System.Windows.Forms.CheckBox();
            this.chkEnable = new System.Windows.Forms.CheckBox();
            this.grpConfig.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkValue)).BeginInit();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(14, 229);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(99, 24);
            this.btnOK.TabIndex = 25;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(119, 229);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(99, 24);
            this.btnCancel.TabIndex = 30;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // grpConfig
            // 
            this.grpConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpConfig.Controls.Add(this.lblValue);
            this.grpConfig.Controls.Add(this.trkValue);
            this.grpConfig.Controls.Add(this.chkAlwaysVisible);
            this.grpConfig.Controls.Add(this.chkDefault);
            this.grpConfig.Controls.Add(this.chkEnable);
            this.grpConfig.Location = new System.Drawing.Point(12, 12);
            this.grpConfig.Name = "grpConfig";
            this.grpConfig.Size = new System.Drawing.Size(208, 202);
            this.grpConfig.TabIndex = 29;
            this.grpConfig.TabStop = false;
            this.grpConfig.Text = "Configuration";
            // 
            // lblValue
            // 
            this.lblValue.Location = new System.Drawing.Point(18, 101);
            this.lblValue.Name = "lblValue";
            this.lblValue.Size = new System.Drawing.Size(174, 13);
            this.lblValue.TabIndex = 4;
            this.lblValue.Text = "15 frames.";
            this.lblValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // trkValue
            // 
            this.trkValue.Location = new System.Drawing.Point(6, 117);
            this.trkValue.Maximum = 60;
            this.trkValue.Minimum = 1;
            this.trkValue.Name = "trkValue";
            this.trkValue.Size = new System.Drawing.Size(196, 45);
            this.trkValue.TabIndex = 15;
            this.trkValue.Value = 15;
            this.trkValue.ValueChanged += new System.EventHandler(this.trkValue_ValueChanged);
            // 
            // chkAlwaysVisible
            // 
            this.chkAlwaysVisible.AutoSize = true;
            this.chkAlwaysVisible.Location = new System.Drawing.Point(18, 168);
            this.chkAlwaysVisible.Name = "chkAlwaysVisible";
            this.chkAlwaysVisible.Size = new System.Drawing.Size(91, 17);
            this.chkAlwaysVisible.TabIndex = 20;
            this.chkAlwaysVisible.Text = "Always visible";
            this.chkAlwaysVisible.UseVisualStyleBackColor = true;
            this.chkAlwaysVisible.CheckedChanged += new System.EventHandler(this.chkAlwaysVisible_CheckedChanged);
            // 
            // chkDefault
            // 
            this.chkDefault.AutoSize = true;
            this.chkDefault.Location = new System.Drawing.Point(18, 68);
            this.chkDefault.Name = "chkDefault";
            this.chkDefault.Size = new System.Drawing.Size(109, 17);
            this.chkDefault.TabIndex = 10;
            this.chkDefault.Text = "Use default value";
            this.chkDefault.UseVisualStyleBackColor = true;
            this.chkDefault.CheckedChanged += new System.EventHandler(this.chkDefault_CheckedChanged);
            // 
            // chkEnable
            // 
            this.chkEnable.AutoSize = true;
            this.chkEnable.Location = new System.Drawing.Point(19, 30);
            this.chkEnable.Name = "chkEnable";
            this.chkEnable.Size = new System.Drawing.Size(116, 17);
            this.chkEnable.TabIndex = 5;
            this.chkEnable.Text = "Enable persistence";
            this.chkEnable.UseVisualStyleBackColor = true;
            this.chkEnable.CheckedChanged += new System.EventHandler(this.chkEnable_CheckedChanged);
            // 
            // formConfigureFading
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(230, 263);
            this.Controls.Add(this.grpConfig);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formConfigureFading";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "   Configure Persistence";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formConfigureFading_FormClosing);
            this.grpConfig.ResumeLayout(false);
            this.grpConfig.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkValue)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpConfig;
        private System.Windows.Forms.CheckBox chkAlwaysVisible;
        private System.Windows.Forms.CheckBox chkDefault;
        private System.Windows.Forms.CheckBox chkEnable;
        private System.Windows.Forms.Label lblValue;
        private System.Windows.Forms.TrackBar trkValue;

    }
}