
namespace Kinovea.ScreenManager
{
    partial class FormConfigureExportVideo
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
      this.encodingSettings = new Kinovea.ScreenManager.Exporters.Video.ControlEncodingSettings();
      this.checkSlowMotion = new System.Windows.Forms.CheckBox();
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.grpboxConfig.SuspendLayout();
      this.SuspendLayout();
      // 
      // grpboxConfig
      // 
      this.grpboxConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpboxConfig.BackColor = System.Drawing.Color.White;
      this.grpboxConfig.Controls.Add(this.encodingSettings);
      this.grpboxConfig.Controls.Add(this.checkSlowMotion);
      this.grpboxConfig.Location = new System.Drawing.Point(12, 12);
      this.grpboxConfig.Name = "grpboxConfig";
      this.grpboxConfig.Size = new System.Drawing.Size(376, 312);
      this.grpboxConfig.TabIndex = 33;
      this.grpboxConfig.TabStop = false;
      this.grpboxConfig.Text = "Configuration";
      // 
      // encodingSettings
      // 
      this.encodingSettings.Location = new System.Drawing.Point(6, 68);
      this.encodingSettings.Name = "encodingSettings";
      this.encodingSettings.Size = new System.Drawing.Size(354, 232);
      this.encodingSettings.TabIndex = 27;
      // 
      // checkSlowMotion
      // 
      this.checkSlowMotion.AutoSize = true;
      this.checkSlowMotion.Location = new System.Drawing.Point(24, 33);
      this.checkSlowMotion.Name = "checkSlowMotion";
      this.checkSlowMotion.Size = new System.Drawing.Size(201, 17);
      this.checkSlowMotion.TabIndex = 26;
      this.checkSlowMotion.Text = "dlgSaveAnalysisOrVideo_CheckSlow";
      this.checkSlowMotion.UseVisualStyleBackColor = true;
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(184, 330);
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
      this.btnCancel.Location = new System.Drawing.Point(289, 330);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(99, 24);
      this.btnCancel.TabIndex = 35;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // FormConfigureExportVideo
      // 
      this.AcceptButton = this.btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(400, 366);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.grpboxConfig);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "FormConfigureExportVideo";
      this.Text = "FormExportImageSequence";
      this.grpboxConfig.ResumeLayout(false);
      this.grpboxConfig.PerformLayout();
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpboxConfig;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox checkSlowMotion;
        private Exporters.Video.ControlEncodingSettings encodingSettings;
    }
}