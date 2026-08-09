namespace Kinovea.ScreenManager.Exporters.Video
{
    partial class ControlEncodingSettings
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
      this.lblCodec = new System.Windows.Forms.Label();
      this.cbCodec = new System.Windows.Forms.ComboBox();
      this.lblEncodingQuality = new System.Windows.Forms.Label();
      this.cbEncodingQuality = new System.Windows.Forms.ComboBox();
      this.lblPreset = new System.Windows.Forms.Label();
      this.cbPreset = new System.Windows.Forms.ComboBox();
      this.lblEncodingSpeed = new System.Windows.Forms.Label();
      this.cbEncodingSpeed = new System.Windows.Forms.ComboBox();
      this.nudGOPSize = new System.Windows.Forms.NumericUpDown();
      this.lblGOPSize = new System.Windows.Forms.Label();
      this.lblContainer = new System.Windows.Forms.Label();
      this.cbContainer = new System.Windows.Forms.ComboBox();
      ((System.ComponentModel.ISupportInitialize)(this.nudGOPSize)).BeginInit();
      this.SuspendLayout();
      // 
      // lblCodec
      // 
      this.lblCodec.AutoSize = true;
      this.lblCodec.Location = new System.Drawing.Point(16, 89);
      this.lblCodec.Name = "lblCodec";
      this.lblCodec.Size = new System.Drawing.Size(70, 13);
      this.lblCodec.TabIndex = 48;
      this.lblCodec.Text = "Video codec:";
      // 
      // cbCodec
      // 
      this.cbCodec.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.cbCodec.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbCodec.FormattingEnabled = true;
      this.cbCodec.Location = new System.Drawing.Point(224, 86);
      this.cbCodec.Name = "cbCodec";
      this.cbCodec.Size = new System.Drawing.Size(64, 21);
      this.cbCodec.TabIndex = 49;
      this.cbCodec.SelectedIndexChanged += new System.EventHandler(this.any_Changed);
      // 
      // lblEncodingQuality
      // 
      this.lblEncodingQuality.AutoSize = true;
      this.lblEncodingQuality.Location = new System.Drawing.Point(16, 134);
      this.lblEncodingQuality.Name = "lblEncodingQuality";
      this.lblEncodingQuality.Size = new System.Drawing.Size(88, 13);
      this.lblEncodingQuality.TabIndex = 50;
      this.lblEncodingQuality.Text = "Encoding quality:";
      // 
      // cbEncodingQuality
      // 
      this.cbEncodingQuality.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.cbEncodingQuality.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbEncodingQuality.FormattingEnabled = true;
      this.cbEncodingQuality.Location = new System.Drawing.Point(156, 131);
      this.cbEncodingQuality.Name = "cbEncodingQuality";
      this.cbEncodingQuality.Size = new System.Drawing.Size(132, 21);
      this.cbEncodingQuality.TabIndex = 51;
      this.cbEncodingQuality.SelectedIndexChanged += new System.EventHandler(this.any_Changed);
      // 
      // lblPreset
      // 
      this.lblPreset.AutoSize = true;
      this.lblPreset.Location = new System.Drawing.Point(16, 14);
      this.lblPreset.Name = "lblPreset";
      this.lblPreset.Size = new System.Drawing.Size(40, 13);
      this.lblPreset.TabIndex = 53;
      this.lblPreset.Text = "Preset:";
      // 
      // cbPreset
      // 
      this.cbPreset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.cbPreset.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbPreset.FormattingEnabled = true;
      this.cbPreset.Location = new System.Drawing.Point(156, 11);
      this.cbPreset.Name = "cbPreset";
      this.cbPreset.Size = new System.Drawing.Size(132, 21);
      this.cbPreset.TabIndex = 54;
      this.cbPreset.SelectedIndexChanged += new System.EventHandler(this.cbPreset_SelectedIndexChanged);
      // 
      // lblEncodingSpeed
      // 
      this.lblEncodingSpeed.AutoSize = true;
      this.lblEncodingSpeed.Location = new System.Drawing.Point(16, 166);
      this.lblEncodingSpeed.Name = "lblEncodingSpeed";
      this.lblEncodingSpeed.Size = new System.Drawing.Size(87, 13);
      this.lblEncodingSpeed.TabIndex = 57;
      this.lblEncodingSpeed.Text = "Encoding speed:";
      // 
      // cbEncodingSpeed
      // 
      this.cbEncodingSpeed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.cbEncodingSpeed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbEncodingSpeed.FormattingEnabled = true;
      this.cbEncodingSpeed.Location = new System.Drawing.Point(156, 163);
      this.cbEncodingSpeed.Name = "cbEncodingSpeed";
      this.cbEncodingSpeed.Size = new System.Drawing.Size(132, 21);
      this.cbEncodingSpeed.TabIndex = 58;
      this.cbEncodingSpeed.SelectedIndexChanged += new System.EventHandler(this.any_Changed);
      // 
      // nudGOPSize
      // 
      this.nudGOPSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.nudGOPSize.Location = new System.Drawing.Point(236, 200);
      this.nudGOPSize.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
      this.nudGOPSize.Name = "nudGOPSize";
      this.nudGOPSize.Size = new System.Drawing.Size(52, 20);
      this.nudGOPSize.TabIndex = 66;
      this.nudGOPSize.Value = new decimal(new int[] {
            12,
            0,
            0,
            0});
      this.nudGOPSize.ValueChanged += new System.EventHandler(this.any_Changed);
      // 
      // lblGOPSize
      // 
      this.lblGOPSize.AutoSize = true;
      this.lblGOPSize.Location = new System.Drawing.Point(16, 202);
      this.lblGOPSize.Name = "lblGOPSize";
      this.lblGOPSize.Size = new System.Drawing.Size(54, 13);
      this.lblGOPSize.TabIndex = 65;
      this.lblGOPSize.Text = "GOP size:";
      // 
      // lblContainer
      // 
      this.lblContainer.AutoSize = true;
      this.lblContainer.Location = new System.Drawing.Point(16, 59);
      this.lblContainer.Name = "lblContainer";
      this.lblContainer.Size = new System.Drawing.Size(84, 13);
      this.lblContainer.TabIndex = 67;
      this.lblContainer.Text = "Video container:";
      // 
      // cbContainer
      // 
      this.cbContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.cbContainer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbContainer.FormattingEnabled = true;
      this.cbContainer.Location = new System.Drawing.Point(224, 56);
      this.cbContainer.Name = "cbContainer";
      this.cbContainer.Size = new System.Drawing.Size(64, 21);
      this.cbContainer.TabIndex = 68;
      this.cbContainer.SelectedIndexChanged += new System.EventHandler(this.any_Changed);
      // 
      // ControlEncodingSettings
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.lblContainer);
      this.Controls.Add(this.cbContainer);
      this.Controls.Add(this.nudGOPSize);
      this.Controls.Add(this.lblGOPSize);
      this.Controls.Add(this.lblEncodingSpeed);
      this.Controls.Add(this.cbEncodingQuality);
      this.Controls.Add(this.cbEncodingSpeed);
      this.Controls.Add(this.lblEncodingQuality);
      this.Controls.Add(this.lblPreset);
      this.Controls.Add(this.cbPreset);
      this.Controls.Add(this.lblCodec);
      this.Controls.Add(this.cbCodec);
      this.Name = "ControlEncodingSettings";
      this.Size = new System.Drawing.Size(305, 244);
      ((System.ComponentModel.ISupportInitialize)(this.nudGOPSize)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblCodec;
        private System.Windows.Forms.ComboBox cbCodec;
        private System.Windows.Forms.Label lblEncodingQuality;
        private System.Windows.Forms.ComboBox cbEncodingQuality;
        private System.Windows.Forms.Label lblPreset;
        private System.Windows.Forms.ComboBox cbPreset;
        private System.Windows.Forms.Label lblEncodingSpeed;
        private System.Windows.Forms.ComboBox cbEncodingSpeed;
        private System.Windows.Forms.NumericUpDown nudGOPSize;
        private System.Windows.Forms.Label lblGOPSize;
        private System.Windows.Forms.Label lblContainer;
        private System.Windows.Forms.ComboBox cbContainer;
    }
}
