namespace Kinovea.ScreenManager
{
    partial class FormConfigureVisibility
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
      this.nudFading = new System.Windows.Forms.NumericUpDown();
      this.nudOpaque = new System.Windows.Forms.NumericUpDown();
      this.nudMax = new System.Windows.Forms.NumericUpDown();
      this.lblOpaque = new System.Windows.Forms.Label();
      this.lblFading = new System.Windows.Forms.Label();
      this.lblMax = new System.Windows.Forms.Label();
      this.grpConfig.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudFading)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudOpaque)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudMax)).BeginInit();
      this.SuspendLayout();
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(111, 171);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(99, 24);
      this.btnOK.TabIndex = 33;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(216, 171);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(99, 24);
      this.btnCancel.TabIndex = 34;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // grpConfig
      // 
      this.grpConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpConfig.Controls.Add(this.nudFading);
      this.grpConfig.Controls.Add(this.nudOpaque);
      this.grpConfig.Controls.Add(this.nudMax);
      this.grpConfig.Controls.Add(this.lblOpaque);
      this.grpConfig.Controls.Add(this.lblFading);
      this.grpConfig.Controls.Add(this.lblMax);
      this.grpConfig.Location = new System.Drawing.Point(12, 12);
      this.grpConfig.Name = "grpConfig";
      this.grpConfig.Padding = new System.Windows.Forms.Padding(3, 3, 20, 3);
      this.grpConfig.Size = new System.Drawing.Size(303, 153);
      this.grpConfig.TabIndex = 35;
      this.grpConfig.TabStop = false;
      this.grpConfig.Text = "Configuration";
      // 
      // nudFading
      // 
      this.nudFading.Location = new System.Drawing.Point(228, 105);
      this.nudFading.Maximum = new decimal(new int[] {
            250,
            0,
            0,
            0});
      this.nudFading.Name = "nudFading";
      this.nudFading.Size = new System.Drawing.Size(52, 20);
      this.nudFading.TabIndex = 27;
      this.nudFading.ValueChanged += new System.EventHandler(this.nudFading_ValueChanged);
      // 
      // nudOpaque
      // 
      this.nudOpaque.Location = new System.Drawing.Point(228, 70);
      this.nudOpaque.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
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
      // nudMax
      // 
      this.nudMax.Location = new System.Drawing.Point(228, 36);
      this.nudMax.Name = "nudMax";
      this.nudMax.Size = new System.Drawing.Size(52, 20);
      this.nudMax.TabIndex = 25;
      this.nudMax.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.nudMax.ValueChanged += new System.EventHandler(this.nudMax_ValueChanged);
      // 
      // lblOpaque
      // 
      this.lblOpaque.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblOpaque.Location = new System.Drawing.Point(17, 66);
      this.lblOpaque.Name = "lblOpaque";
      this.lblOpaque.Size = new System.Drawing.Size(194, 25);
      this.lblOpaque.TabIndex = 24;
      this.lblOpaque.Text = "Opaque duration (frames):";
      this.lblOpaque.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblFading
      // 
      this.lblFading.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblFading.Location = new System.Drawing.Point(17, 101);
      this.lblFading.Name = "lblFading";
      this.lblFading.Size = new System.Drawing.Size(153, 25);
      this.lblFading.TabIndex = 23;
      this.lblFading.Text = "Fading duration (frames):";
      this.lblFading.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblMax
      // 
      this.lblMax.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblMax.Location = new System.Drawing.Point(17, 32);
      this.lblMax.Name = "lblMax";
      this.lblMax.Size = new System.Drawing.Size(153, 25);
      this.lblMax.TabIndex = 22;
      this.lblMax.Text = "Maximum opacity (%):";
      this.lblMax.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // FormConfigureVisibility
      // 
      this.AcceptButton = this.btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(327, 207);
      this.Controls.Add(this.grpConfig);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnCancel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormConfigureVisibility";
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "FormConfigureVisibility";
      this.grpConfig.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.nudFading)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudOpaque)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudMax)).EndInit();
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpConfig;
        private System.Windows.Forms.Label lblMax;
        private System.Windows.Forms.Label lblOpaque;
        private System.Windows.Forms.Label lblFading;
        private System.Windows.Forms.NumericUpDown nudFading;
        private System.Windows.Forms.NumericUpDown nudOpaque;
        private System.Windows.Forms.NumericUpDown nudMax;
    }
}