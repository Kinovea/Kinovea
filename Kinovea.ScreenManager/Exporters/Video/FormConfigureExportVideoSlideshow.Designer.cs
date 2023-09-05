
namespace Kinovea.ScreenManager
{
    partial class FormConfigureExportVideoSlideshow
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
      this.grpboxConfig = new System.Windows.Forms.GroupBox();
      this.lblInfosFrequency = new System.Windows.Forms.Label();
      this.trkInterval = new System.Windows.Forms.TrackBar();
      this.grpboxConfig.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trkInterval)).BeginInit();
      this.SuspendLayout();
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(218, 161);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(99, 24);
      this.btnOK.TabIndex = 34;
      this.btnOK.Text = "Save";
      this.btnOK.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(323, 161);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(99, 24);
      this.btnCancel.TabIndex = 35;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // grpboxConfig
      // 
      this.grpboxConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpboxConfig.BackColor = System.Drawing.Color.White;
      this.grpboxConfig.Controls.Add(this.lblInfosFrequency);
      this.grpboxConfig.Controls.Add(this.trkInterval);
      this.grpboxConfig.Location = new System.Drawing.Point(14, 20);
      this.grpboxConfig.Name = "grpboxConfig";
      this.grpboxConfig.Size = new System.Drawing.Size(408, 129);
      this.grpboxConfig.TabIndex = 36;
      this.grpboxConfig.TabStop = false;
      this.grpboxConfig.Text = "generic_config";
      // 
      // lblInfosFrequency
      // 
      this.lblInfosFrequency.AutoSize = true;
      this.lblInfosFrequency.Location = new System.Drawing.Point(24, 32);
      this.lblInfosFrequency.Name = "lblInfosFrequency";
      this.lblInfosFrequency.Size = new System.Drawing.Size(182, 13);
      this.lblInfosFrequency.TabIndex = 1;
      this.lblInfosFrequency.Text = "Duration of each slide: 40 hundredth.";
      // 
      // trkInterval
      // 
      this.trkInterval.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.trkInterval.Location = new System.Drawing.Point(27, 59);
      this.trkInterval.Name = "trkInterval";
      this.trkInterval.Size = new System.Drawing.Size(356, 45);
      this.trkInterval.TabIndex = 5;
      this.trkInterval.TickStyle = System.Windows.Forms.TickStyle.None;
      this.trkInterval.ValueChanged += new System.EventHandler(this.trkInterval_ValueChanged);
      // 
      // FormConfigureExportVideoSlideshow
      // 
      this.AcceptButton = this.btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(434, 197);
      this.Controls.Add(this.grpboxConfig);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnCancel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "FormConfigureExportVideoSlideshow";
      this.Text = "FormExportImageSequence";
      this.grpboxConfig.ResumeLayout(false);
      this.grpboxConfig.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trkInterval)).EndInit();
      this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpboxConfig;
        private System.Windows.Forms.Label lblInfosFrequency;
        private System.Windows.Forms.TrackBar trkInterval;
    }
}