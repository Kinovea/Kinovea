namespace Kinovea.ScreenManager
{
    partial class FormVDM
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
      this.grpIntrinsics = new System.Windows.Forms.GroupBox();
      this.label1 = new System.Windows.Forms.Label();
      this.lblFocalLength = new System.Windows.Forms.Label();
      this.lblSensorWidth = new System.Windows.Forms.Label();
      this.grpIntrinsics.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(102, 193);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(99, 24);
      this.btnOK.TabIndex = 31;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(207, 193);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(99, 24);
      this.btnCancel.TabIndex = 32;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // grpIntrinsics
      // 
      this.grpIntrinsics.Controls.Add(this.label1);
      this.grpIntrinsics.Controls.Add(this.lblFocalLength);
      this.grpIntrinsics.Controls.Add(this.lblSensorWidth);
      this.grpIntrinsics.Location = new System.Drawing.Point(16, 12);
      this.grpIntrinsics.Name = "grpIntrinsics";
      this.grpIntrinsics.Size = new System.Drawing.Size(289, 162);
      this.grpIntrinsics.TabIndex = 36;
      this.grpIntrinsics.TabStop = false;
      this.grpIntrinsics.Text = "Camera position";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(18, 83);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(82, 13);
      this.label1.TabIndex = 51;
      this.label1.Text = "Camera position";
      // 
      // lblFocalLength
      // 
      this.lblFocalLength.AutoSize = true;
      this.lblFocalLength.Location = new System.Drawing.Point(18, 56);
      this.lblFocalLength.Name = "lblFocalLength";
      this.lblFocalLength.Size = new System.Drawing.Size(85, 13);
      this.lblFocalLength.TabIndex = 50;
      this.lblFocalLength.Text = "Plane calibration";
      // 
      // lblSensorWidth
      // 
      this.lblSensorWidth.AutoSize = true;
      this.lblSensorWidth.Location = new System.Drawing.Point(18, 30);
      this.lblSensorWidth.Name = "lblSensorWidth";
      this.lblSensorWidth.Size = new System.Drawing.Size(81, 13);
      this.lblSensorWidth.TabIndex = 48;
      this.lblSensorWidth.Text = "Lens calibration";
      // 
      // FormVDM
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(314, 229);
      this.Controls.Add(this.grpIntrinsics);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnCancel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormVDM";
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.Text = "FormVDM";
      this.grpIntrinsics.ResumeLayout(false);
      this.grpIntrinsics.PerformLayout();
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpIntrinsics;
        private System.Windows.Forms.Label lblFocalLength;
        private System.Windows.Forms.Label lblSensorWidth;
        private System.Windows.Forms.Label label1;
    }
}