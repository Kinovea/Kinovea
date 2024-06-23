
namespace Kinovea.ScreenManager
{
    partial class FormConfigureCameraMotion
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
      this.grpConfig = new System.Windows.Forms.GroupBox();
      this.cmbFeatureType = new System.Windows.Forms.ComboBox();
      this.nudFeaturesPerFrame = new System.Windows.Forms.NumericUpDown();
      this.lblFeaturesPerFrame = new System.Windows.Forms.Label();
      this.lblFeatureType = new System.Windows.Forms.Label();
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.grpConfig.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudFeaturesPerFrame)).BeginInit();
      this.SuspendLayout();
      // 
      // grpConfig
      // 
      this.grpConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpConfig.Controls.Add(this.cmbFeatureType);
      this.grpConfig.Controls.Add(this.nudFeaturesPerFrame);
      this.grpConfig.Controls.Add(this.lblFeaturesPerFrame);
      this.grpConfig.Controls.Add(this.lblFeatureType);
      this.grpConfig.Location = new System.Drawing.Point(14, 12);
      this.grpConfig.Name = "grpConfig";
      this.grpConfig.Size = new System.Drawing.Size(239, 100);
      this.grpConfig.TabIndex = 58;
      this.grpConfig.TabStop = false;
      this.grpConfig.Text = "Generic_Configuration";
      // 
      // cmbFeatureType
      // 
      this.cmbFeatureType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbFeatureType.FormattingEnabled = true;
      this.cmbFeatureType.Location = new System.Drawing.Point(163, 30);
      this.cmbFeatureType.Name = "cmbFeatureType";
      this.cmbFeatureType.Size = new System.Drawing.Size(52, 21);
      this.cmbFeatureType.TabIndex = 68;
      this.cmbFeatureType.SelectedIndexChanged += new System.EventHandler(this.cmbFeatureType_SelectedIndexChanged);
      // 
      // nudFeaturesPerFrame
      // 
      this.nudFeaturesPerFrame.Location = new System.Drawing.Point(163, 61);
      this.nudFeaturesPerFrame.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
      this.nudFeaturesPerFrame.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.nudFeaturesPerFrame.Name = "nudFeaturesPerFrame";
      this.nudFeaturesPerFrame.Size = new System.Drawing.Size(52, 20);
      this.nudFeaturesPerFrame.TabIndex = 67;
      this.nudFeaturesPerFrame.Value = new decimal(new int[] {
            2048,
            0,
            0,
            0});
      this.nudFeaturesPerFrame.ValueChanged += new System.EventHandler(this.featuresPerFrame_ValueChanged);
      this.nudFeaturesPerFrame.KeyUp += new System.Windows.Forms.KeyEventHandler(this.featuresPerFrame_KeyUp);
      // 
      // lblFeaturesPerFrame
      // 
      this.lblFeaturesPerFrame.AutoSize = true;
      this.lblFeaturesPerFrame.Location = new System.Drawing.Point(21, 63);
      this.lblFeaturesPerFrame.Name = "lblFeaturesPerFrame";
      this.lblFeaturesPerFrame.Size = new System.Drawing.Size(98, 13);
      this.lblFeaturesPerFrame.TabIndex = 66;
      this.lblFeaturesPerFrame.Text = "Features per frame:";
      // 
      // lblFeatureType
      // 
      this.lblFeatureType.AutoSize = true;
      this.lblFeatureType.Location = new System.Drawing.Point(21, 33);
      this.lblFeatureType.Name = "lblFeatureType";
      this.lblFeatureType.Size = new System.Drawing.Size(69, 13);
      this.lblFeatureType.TabIndex = 52;
      this.lblFeatureType.Text = "Feature type:";
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(51, 127);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(99, 24);
      this.btnOK.TabIndex = 56;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(156, 127);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(99, 24);
      this.btnCancel.TabIndex = 57;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // FormConfigureCameraMotion
      // 
      this.AcceptButton = this.btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(265, 163);
      this.Controls.Add(this.grpConfig);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnCancel);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormConfigureCameraMotion";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "FormConfigureCameraMotion";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_FormClosing);
      this.grpConfig.ResumeLayout(false);
      this.grpConfig.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudFeaturesPerFrame)).EndInit();
      this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox grpConfig;
        private System.Windows.Forms.Label lblFeatureType;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.NumericUpDown nudFeaturesPerFrame;
        private System.Windows.Forms.Label lblFeaturesPerFrame;
        private System.Windows.Forms.ComboBox cmbFeatureType;
    }
}