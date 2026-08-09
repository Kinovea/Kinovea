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
      this.lblQualityControl = new System.Windows.Forms.Label();
      this.cbQualityControl = new System.Windows.Forms.ComboBox();
      this.lblEncodingSpeed = new System.Windows.Forms.Label();
      this.cbEncodingSpeed = new System.Windows.Forms.ComboBox();
      this.nudBitrate = new System.Windows.Forms.NumericUpDown();
      this.lblBitrate = new System.Windows.Forms.Label();
      this.nudMinBitrate = new System.Windows.Forms.NumericUpDown();
      this.lblMinBitrate = new System.Windows.Forms.Label();
      this.nudMaxBitrate = new System.Windows.Forms.NumericUpDown();
      this.lblMaxBitrate = new System.Windows.Forms.Label();
      this.nudGOPSize = new System.Windows.Forms.NumericUpDown();
      this.lblGOPSize = new System.Windows.Forms.Label();
      this.lblContainer = new System.Windows.Forms.Label();
      this.cbContainer = new System.Windows.Forms.ComboBox();
      ((System.ComponentModel.ISupportInitialize)(this.nudBitrate)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudMinBitrate)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudMaxBitrate)).BeginInit();
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
      this.lblEncodingQuality.Location = new System.Drawing.Point(16, 158);
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
      this.cbEncodingQuality.Location = new System.Drawing.Point(156, 155);
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
      // lblQualityControl
      // 
      this.lblQualityControl.AutoSize = true;
      this.lblQualityControl.Location = new System.Drawing.Point(16, 119);
      this.lblQualityControl.Name = "lblQualityControl";
      this.lblQualityControl.Size = new System.Drawing.Size(77, 13);
      this.lblQualityControl.TabIndex = 55;
      this.lblQualityControl.Text = "Quality control:";
      // 
      // cbQualityControl
      // 
      this.cbQualityControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.cbQualityControl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbQualityControl.FormattingEnabled = true;
      this.cbQualityControl.Location = new System.Drawing.Point(156, 116);
      this.cbQualityControl.Name = "cbQualityControl";
      this.cbQualityControl.Size = new System.Drawing.Size(132, 21);
      this.cbQualityControl.TabIndex = 56;
      this.cbQualityControl.SelectedIndexChanged += new System.EventHandler(this.any_Changed);
      // 
      // lblEncodingSpeed
      // 
      this.lblEncodingSpeed.AutoSize = true;
      this.lblEncodingSpeed.Location = new System.Drawing.Point(16, 190);
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
      this.cbEncodingSpeed.Location = new System.Drawing.Point(156, 187);
      this.cbEncodingSpeed.Name = "cbEncodingSpeed";
      this.cbEncodingSpeed.Size = new System.Drawing.Size(132, 21);
      this.cbEncodingSpeed.TabIndex = 58;
      this.cbEncodingSpeed.SelectedIndexChanged += new System.EventHandler(this.any_Changed);
      // 
      // nudBitrate
      // 
      this.nudBitrate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.nudBitrate.Location = new System.Drawing.Point(236, 224);
      this.nudBitrate.Maximum = new decimal(new int[] {
            25000,
            0,
            0,
            0});
      this.nudBitrate.Minimum = new decimal(new int[] {
            200,
            0,
            0,
            0});
      this.nudBitrate.Name = "nudBitrate";
      this.nudBitrate.Size = new System.Drawing.Size(52, 20);
      this.nudBitrate.TabIndex = 60;
      this.nudBitrate.Value = new decimal(new int[] {
            6000,
            0,
            0,
            0});
      this.nudBitrate.ValueChanged += new System.EventHandler(this.any_Changed);
      // 
      // lblBitrate
      // 
      this.lblBitrate.AutoSize = true;
      this.lblBitrate.Location = new System.Drawing.Point(16, 226);
      this.lblBitrate.Name = "lblBitrate";
      this.lblBitrate.Size = new System.Drawing.Size(40, 13);
      this.lblBitrate.TabIndex = 59;
      this.lblBitrate.Text = "Bitrate:";
      // 
      // nudMinBitrate
      // 
      this.nudMinBitrate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.nudMinBitrate.Location = new System.Drawing.Point(236, 250);
      this.nudMinBitrate.Maximum = new decimal(new int[] {
            25000,
            0,
            0,
            0});
      this.nudMinBitrate.Name = "nudMinBitrate";
      this.nudMinBitrate.Size = new System.Drawing.Size(52, 20);
      this.nudMinBitrate.TabIndex = 62;
      this.nudMinBitrate.ValueChanged += new System.EventHandler(this.any_Changed);
      // 
      // lblMinBitrate
      // 
      this.lblMinBitrate.AutoSize = true;
      this.lblMinBitrate.Location = new System.Drawing.Point(27, 252);
      this.lblMinBitrate.Name = "lblMinBitrate";
      this.lblMinBitrate.Size = new System.Drawing.Size(59, 13);
      this.lblMinBitrate.TabIndex = 61;
      this.lblMinBitrate.Text = "Min bitrate:";
      // 
      // nudMaxBitrate
      // 
      this.nudMaxBitrate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.nudMaxBitrate.Location = new System.Drawing.Point(236, 276);
      this.nudMaxBitrate.Maximum = new decimal(new int[] {
            25000,
            0,
            0,
            0});
      this.nudMaxBitrate.Name = "nudMaxBitrate";
      this.nudMaxBitrate.Size = new System.Drawing.Size(52, 20);
      this.nudMaxBitrate.TabIndex = 64;
      this.nudMaxBitrate.Value = new decimal(new int[] {
            9000,
            0,
            0,
            0});
      this.nudMaxBitrate.ValueChanged += new System.EventHandler(this.any_Changed);
      // 
      // lblMaxBitrate
      // 
      this.lblMaxBitrate.AutoSize = true;
      this.lblMaxBitrate.Location = new System.Drawing.Point(27, 278);
      this.lblMaxBitrate.Name = "lblMaxBitrate";
      this.lblMaxBitrate.Size = new System.Drawing.Size(62, 13);
      this.lblMaxBitrate.TabIndex = 63;
      this.lblMaxBitrate.Text = "Max bitrate:";
      // 
      // nudGOPSize
      // 
      this.nudGOPSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.nudGOPSize.Location = new System.Drawing.Point(236, 318);
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
      this.lblGOPSize.Location = new System.Drawing.Point(16, 320);
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
      this.Controls.Add(this.nudMaxBitrate);
      this.Controls.Add(this.lblMaxBitrate);
      this.Controls.Add(this.nudMinBitrate);
      this.Controls.Add(this.lblMinBitrate);
      this.Controls.Add(this.nudBitrate);
      this.Controls.Add(this.lblBitrate);
      this.Controls.Add(this.lblEncodingSpeed);
      this.Controls.Add(this.cbEncodingQuality);
      this.Controls.Add(this.lblQualityControl);
      this.Controls.Add(this.cbEncodingSpeed);
      this.Controls.Add(this.cbQualityControl);
      this.Controls.Add(this.lblEncodingQuality);
      this.Controls.Add(this.lblPreset);
      this.Controls.Add(this.cbPreset);
      this.Controls.Add(this.lblCodec);
      this.Controls.Add(this.cbCodec);
      this.Name = "ControlEncodingSettings";
      this.Size = new System.Drawing.Size(305, 358);
      ((System.ComponentModel.ISupportInitialize)(this.nudBitrate)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudMinBitrate)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudMaxBitrate)).EndInit();
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
        private System.Windows.Forms.Label lblQualityControl;
        private System.Windows.Forms.ComboBox cbQualityControl;
        private System.Windows.Forms.Label lblEncodingSpeed;
        private System.Windows.Forms.ComboBox cbEncodingSpeed;
        private System.Windows.Forms.NumericUpDown nudBitrate;
        private System.Windows.Forms.Label lblBitrate;
        private System.Windows.Forms.NumericUpDown nudMinBitrate;
        private System.Windows.Forms.Label lblMinBitrate;
        private System.Windows.Forms.NumericUpDown nudMaxBitrate;
        private System.Windows.Forms.Label lblMaxBitrate;
        private System.Windows.Forms.NumericUpDown nudGOPSize;
        private System.Windows.Forms.Label lblGOPSize;
        private System.Windows.Forms.Label lblContainer;
        private System.Windows.Forms.ComboBox cbContainer;
    }
}
