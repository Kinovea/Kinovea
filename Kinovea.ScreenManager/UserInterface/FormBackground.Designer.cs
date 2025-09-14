namespace Kinovea.ScreenManager
{
    partial class FormBackgroundColor
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
      this.nudOpaque = new System.Windows.Forms.NumericUpDown();
      this.lblOpaque = new System.Windows.Forms.Label();
      this.lblColor = new System.Windows.Forms.Label();
      this.grpConfig.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudOpaque)).BeginInit();
      this.SuspendLayout();
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(111, 128);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(99, 24);
      this.btnOK.TabIndex = 33;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(216, 128);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(99, 24);
      this.btnCancel.TabIndex = 34;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // grpConfig
      // 
      this.grpConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpConfig.Controls.Add(this.nudOpaque);
      this.grpConfig.Controls.Add(this.lblOpaque);
      this.grpConfig.Controls.Add(this.lblColor);
      this.grpConfig.Location = new System.Drawing.Point(12, 12);
      this.grpConfig.Name = "grpConfig";
      this.grpConfig.Padding = new System.Windows.Forms.Padding(3, 3, 20, 3);
      this.grpConfig.Size = new System.Drawing.Size(303, 110);
      this.grpConfig.TabIndex = 35;
      this.grpConfig.TabStop = false;
      // 
      // nudOpaque
      // 
      this.nudOpaque.Location = new System.Drawing.Point(227, 67);
      this.nudOpaque.Name = "nudOpaque";
      this.nudOpaque.Size = new System.Drawing.Size(52, 20);
      this.nudOpaque.TabIndex = 26;
      this.nudOpaque.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.nudOpaque.ValueChanged += new System.EventHandler(this.nudOpaque_ValueChanged);
      // 
      // lblOpaque
      // 
      this.lblOpaque.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblOpaque.Location = new System.Drawing.Point(16, 63);
      this.lblOpaque.Name = "lblOpaque";
      this.lblOpaque.Size = new System.Drawing.Size(194, 25);
      this.lblOpaque.TabIndex = 24;
      this.lblOpaque.Text = "Opacity:";
      this.lblOpaque.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblColor
      // 
      this.lblColor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblColor.Location = new System.Drawing.Point(16, 25);
      this.lblColor.Name = "lblColor";
      this.lblColor.Size = new System.Drawing.Size(153, 25);
      this.lblColor.TabIndex = 22;
      this.lblColor.Text = "Color:";
      this.lblColor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // FormBackgroundColor
      // 
      this.AcceptButton = this.btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(327, 164);
      this.Controls.Add(this.grpConfig);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnCancel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormBackgroundColor";
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "FormConfigureVisibility";
      this.grpConfig.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.nudOpaque)).EndInit();
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpConfig;
        private System.Windows.Forms.Label lblColor;
        private System.Windows.Forms.Label lblOpaque;
        private System.Windows.Forms.NumericUpDown nudOpaque;
    }
}