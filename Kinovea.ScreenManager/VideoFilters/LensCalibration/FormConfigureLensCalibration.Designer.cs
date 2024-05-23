
namespace Kinovea.ScreenManager
{
    partial class FormConfigureLensCalibration
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
      this.lblPatternSize = new System.Windows.Forms.Label();
      this.grpConfig = new System.Windows.Forms.GroupBox();
      this.lblMaxIterations = new System.Windows.Forms.Label();
      this.nudMaxIterations = new System.Windows.Forms.NumericUpDown();
      this.label2 = new System.Windows.Forms.Label();
      this.nudMaxImages = new System.Windows.Forms.NumericUpDown();
      this.nudRows = new System.Windows.Forms.NumericUpDown();
      this.nudCols = new System.Windows.Forms.NumericUpDown();
      this.lblMaxImages = new System.Windows.Forms.Label();
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.grpConfig.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudMaxIterations)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudMaxImages)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudRows)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudCols)).BeginInit();
      this.SuspendLayout();
      // 
      // lblPatternSize
      // 
      this.lblPatternSize.AutoSize = true;
      this.lblPatternSize.Location = new System.Drawing.Point(21, 67);
      this.lblPatternSize.Name = "lblPatternSize";
      this.lblPatternSize.Size = new System.Drawing.Size(65, 13);
      this.lblPatternSize.TabIndex = 43;
      this.lblPatternSize.Text = "Pattern size:";
      // 
      // grpConfig
      // 
      this.grpConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpConfig.Controls.Add(this.lblMaxIterations);
      this.grpConfig.Controls.Add(this.nudMaxIterations);
      this.grpConfig.Controls.Add(this.lblPatternSize);
      this.grpConfig.Controls.Add(this.label2);
      this.grpConfig.Controls.Add(this.nudMaxImages);
      this.grpConfig.Controls.Add(this.nudRows);
      this.grpConfig.Controls.Add(this.nudCols);
      this.grpConfig.Controls.Add(this.lblMaxImages);
      this.grpConfig.Location = new System.Drawing.Point(14, 12);
      this.grpConfig.Name = "grpConfig";
      this.grpConfig.Size = new System.Drawing.Size(314, 140);
      this.grpConfig.TabIndex = 58;
      this.grpConfig.TabStop = false;
      this.grpConfig.Text = "Generic_Configuration";
      // 
      // lblMaxIterations
      // 
      this.lblMaxIterations.AutoSize = true;
      this.lblMaxIterations.Location = new System.Drawing.Point(22, 101);
      this.lblMaxIterations.Name = "lblMaxIterations";
      this.lblMaxIterations.Size = new System.Drawing.Size(75, 13);
      this.lblMaxIterations.TabIndex = 65;
      this.lblMaxIterations.Text = "Max iterations:";
      // 
      // nudMaxIterations
      // 
      this.nudMaxIterations.Location = new System.Drawing.Point(163, 99);
      this.nudMaxIterations.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
      this.nudMaxIterations.Name = "nudMaxIterations";
      this.nudMaxIterations.Size = new System.Drawing.Size(45, 20);
      this.nudMaxIterations.TabIndex = 64;
      this.nudMaxIterations.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
      this.nudMaxIterations.ValueChanged += new System.EventHandler(this.maxIterations_ValueChanged);
      this.nudMaxIterations.KeyUp += new System.Windows.Forms.KeyEventHandler(this.maxIterations_KeyUp);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(204, 69);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(13, 13);
      this.label2.TabIndex = 63;
      this.label2.Text = "×";
      // 
      // nudMaxImages
      // 
      this.nudMaxImages.Location = new System.Drawing.Point(163, 31);
      this.nudMaxImages.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
      this.nudMaxImages.Name = "nudMaxImages";
      this.nudMaxImages.Size = new System.Drawing.Size(45, 20);
      this.nudMaxImages.TabIndex = 58;
      this.nudMaxImages.Value = new decimal(new int[] {
            12,
            0,
            0,
            0});
      this.nudMaxImages.ValueChanged += new System.EventHandler(this.maxImages_ValueChanged);
      this.nudMaxImages.KeyUp += new System.Windows.Forms.KeyEventHandler(this.maxImages_KeyUp);
      // 
      // nudRows
      // 
      this.nudRows.Location = new System.Drawing.Point(223, 65);
      this.nudRows.Maximum = new decimal(new int[] {
            25,
            0,
            0,
            0});
      this.nudRows.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.nudRows.Name = "nudRows";
      this.nudRows.Size = new System.Drawing.Size(35, 20);
      this.nudRows.TabIndex = 57;
      this.nudRows.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
      this.nudRows.ValueChanged += new System.EventHandler(this.patternSize_ValueChanged);
      this.nudRows.KeyUp += new System.Windows.Forms.KeyEventHandler(this.patternSize_KeyUp);
      // 
      // nudCols
      // 
      this.nudCols.Location = new System.Drawing.Point(163, 65);
      this.nudCols.Maximum = new decimal(new int[] {
            25,
            0,
            0,
            0});
      this.nudCols.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.nudCols.Name = "nudCols";
      this.nudCols.Size = new System.Drawing.Size(35, 20);
      this.nudCols.TabIndex = 56;
      this.nudCols.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
      this.nudCols.ValueChanged += new System.EventHandler(this.patternSize_ValueChanged);
      this.nudCols.KeyUp += new System.Windows.Forms.KeyEventHandler(this.patternSize_KeyUp);
      // 
      // lblMaxImages
      // 
      this.lblMaxImages.AutoSize = true;
      this.lblMaxImages.Location = new System.Drawing.Point(21, 33);
      this.lblMaxImages.Name = "lblMaxImages";
      this.lblMaxImages.Size = new System.Drawing.Size(66, 13);
      this.lblMaxImages.TabIndex = 52;
      this.lblMaxImages.Text = "Max images:";
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(126, 167);
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
      this.btnCancel.Location = new System.Drawing.Point(231, 167);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(99, 24);
      this.btnCancel.TabIndex = 57;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // FormConfigureLensCalibration
      // 
      this.AcceptButton = this.btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(340, 203);
      this.Controls.Add(this.grpConfig);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnCancel);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormConfigureLensCalibration";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "FormConfigureLensCalibration";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_FormClosing);
      this.grpConfig.ResumeLayout(false);
      this.grpConfig.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudMaxIterations)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudMaxImages)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudRows)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudCols)).EndInit();
      this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label lblPatternSize;
        private System.Windows.Forms.GroupBox grpConfig;
        private System.Windows.Forms.Label lblMaxImages;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.NumericUpDown nudRows;
        private System.Windows.Forms.NumericUpDown nudCols;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblMaxIterations;
        private System.Windows.Forms.NumericUpDown nudMaxIterations;
        private System.Windows.Forms.NumericUpDown nudMaxImages;
    }
}