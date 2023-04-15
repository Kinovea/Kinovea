
namespace Kinovea.ScreenManager
{
    partial class FormConfigureExportImageSideBySide
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
      this.rbVertical = new System.Windows.Forms.RadioButton();
      this.rbHorizontal = new System.Windows.Forms.RadioButton();
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
      this.grpboxConfig.Controls.Add(this.rbVertical);
      this.grpboxConfig.Controls.Add(this.rbHorizontal);
      this.grpboxConfig.Location = new System.Drawing.Point(12, 12);
      this.grpboxConfig.Name = "grpboxConfig";
      this.grpboxConfig.Size = new System.Drawing.Size(275, 126);
      this.grpboxConfig.TabIndex = 33;
      this.grpboxConfig.TabStop = false;
      this.grpboxConfig.Text = "Configuration";
      // 
      // rbVertical
      // 
      this.rbVertical.AutoSize = true;
      this.rbVertical.Location = new System.Drawing.Point(32, 75);
      this.rbVertical.Name = "rbVertical";
      this.rbVertical.Size = new System.Drawing.Size(60, 17);
      this.rbVertical.TabIndex = 1;
      this.rbVertical.TabStop = true;
      this.rbVertical.Text = "Vertical";
      this.rbVertical.UseVisualStyleBackColor = true;
      // 
      // rbHorizontal
      // 
      this.rbHorizontal.AutoSize = true;
      this.rbHorizontal.Checked = true;
      this.rbHorizontal.Location = new System.Drawing.Point(32, 39);
      this.rbHorizontal.Name = "rbHorizontal";
      this.rbHorizontal.Size = new System.Drawing.Size(72, 17);
      this.rbHorizontal.TabIndex = 0;
      this.rbHorizontal.TabStop = true;
      this.rbHorizontal.Text = "Horizontal";
      this.rbHorizontal.UseVisualStyleBackColor = true;
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(84, 161);
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
      this.btnCancel.Location = new System.Drawing.Point(189, 161);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(99, 24);
      this.btnCancel.TabIndex = 35;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // FormConfigureExportImageSideBySide
      // 
      this.AcceptButton = this.btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(300, 197);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.grpboxConfig);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "FormConfigureExportImageSideBySide";
      this.Text = "FormExportImageSequence";
      this.grpboxConfig.ResumeLayout(false);
      this.grpboxConfig.PerformLayout();
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpboxConfig;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.RadioButton rbVertical;
        private System.Windows.Forms.RadioButton rbHorizontal;
    }
}