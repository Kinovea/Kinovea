
namespace Kinovea.ScreenManager
{
    partial class FormConfigureKinogram
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
      this.label1 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.lblCropSize = new System.Windows.Forms.Label();
      this.grpConfig = new System.Windows.Forms.GroupBox();
      this.label2 = new System.Windows.Forms.Label();
      this.lblFrameInterval = new System.Windows.Forms.Label();
      this.nudCropWidth = new System.Windows.Forms.NumericUpDown();
      this.lblTotal = new System.Windows.Forms.Label();
      this.nudCropHeight = new System.Windows.Forms.NumericUpDown();
      this.cbRTL = new System.Windows.Forms.CheckBox();
      this.nudRows = new System.Windows.Forms.NumericUpDown();
      this.nudCols = new System.Windows.Forms.NumericUpDown();
      this.lblColumns = new System.Windows.Forms.Label();
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.grpAppearance = new System.Windows.Forms.GroupBox();
      this.cbBorderVisible = new System.Windows.Forms.CheckBox();
      this.btnApply = new System.Windows.Forms.Button();
      this.grpConfig.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudCropWidth)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudCropHeight)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudRows)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudCols)).BeginInit();
      this.grpAppearance.SuspendLayout();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(267, 68);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(18, 13);
      this.label1.TabIndex = 53;
      this.label1.Text = "px";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(200, 69);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(13, 13);
      this.label4.TabIndex = 45;
      this.label4.Text = "×";
      // 
      // lblCropSize
      // 
      this.lblCropSize.AutoSize = true;
      this.lblCropSize.Location = new System.Drawing.Point(21, 67);
      this.lblCropSize.Name = "lblCropSize";
      this.lblCropSize.Size = new System.Drawing.Size(56, 13);
      this.lblCropSize.TabIndex = 43;
      this.lblCropSize.Text = "Crop size :";
      // 
      // grpConfig
      // 
      this.grpConfig.Controls.Add(this.lblCropSize);
      this.grpConfig.Controls.Add(this.label4);
      this.grpConfig.Controls.Add(this.label2);
      this.grpConfig.Controls.Add(this.label1);
      this.grpConfig.Controls.Add(this.lblFrameInterval);
      this.grpConfig.Controls.Add(this.nudCropWidth);
      this.grpConfig.Controls.Add(this.lblTotal);
      this.grpConfig.Controls.Add(this.nudCropHeight);
      this.grpConfig.Controls.Add(this.cbRTL);
      this.grpConfig.Controls.Add(this.nudRows);
      this.grpConfig.Controls.Add(this.nudCols);
      this.grpConfig.Controls.Add(this.lblColumns);
      this.grpConfig.Location = new System.Drawing.Point(14, 12);
      this.grpConfig.Name = "grpConfig";
      this.grpConfig.Size = new System.Drawing.Size(338, 235);
      this.grpConfig.TabIndex = 58;
      this.grpConfig.TabStop = false;
      this.grpConfig.Text = "Generic_Configuration";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(192, 33);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(13, 13);
      this.label2.TabIndex = 63;
      this.label2.Text = "×";
      // 
      // lblFrameInterval
      // 
      this.lblFrameInterval.AutoSize = true;
      this.lblFrameInterval.Location = new System.Drawing.Point(21, 204);
      this.lblFrameInterval.Name = "lblFrameInterval";
      this.lblFrameInterval.Size = new System.Drawing.Size(79, 13);
      this.lblFrameInterval.TabIndex = 62;
      this.lblFrameInterval.Text = "Frame interval :";
      // 
      // nudCropWidth
      // 
      this.nudCropWidth.Location = new System.Drawing.Point(151, 65);
      this.nudCropWidth.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
      this.nudCropWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.nudCropWidth.Name = "nudCropWidth";
      this.nudCropWidth.Size = new System.Drawing.Size(45, 20);
      this.nudCropWidth.TabIndex = 58;
      this.nudCropWidth.Value = new decimal(new int[] {
            400,
            0,
            0,
            0});
      this.nudCropWidth.ValueChanged += new System.EventHandler(this.cropSize_ValueChanged);
      this.nudCropWidth.KeyUp += new System.Windows.Forms.KeyEventHandler(this.cropSize_KeyUp);
      // 
      // lblTotal
      // 
      this.lblTotal.AutoSize = true;
      this.lblTotal.Location = new System.Drawing.Point(21, 173);
      this.lblTotal.Name = "lblTotal";
      this.lblTotal.Size = new System.Drawing.Size(37, 13);
      this.lblTotal.TabIndex = 61;
      this.lblTotal.Text = "Total :";
      // 
      // nudCropHeight
      // 
      this.nudCropHeight.Location = new System.Drawing.Point(216, 65);
      this.nudCropHeight.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
      this.nudCropHeight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.nudCropHeight.Name = "nudCropHeight";
      this.nudCropHeight.Size = new System.Drawing.Size(45, 20);
      this.nudCropHeight.TabIndex = 59;
      this.nudCropHeight.Value = new decimal(new int[] {
            600,
            0,
            0,
            0});
      this.nudCropHeight.ValueChanged += new System.EventHandler(this.cropSize_ValueChanged);
      this.nudCropHeight.KeyUp += new System.Windows.Forms.KeyEventHandler(this.cropSize_KeyUp);
      // 
      // cbRTL
      // 
      this.cbRTL.AutoSize = true;
      this.cbRTL.Location = new System.Drawing.Point(24, 103);
      this.cbRTL.Name = "cbRTL";
      this.cbRTL.Size = new System.Drawing.Size(80, 17);
      this.cbRTL.TabIndex = 60;
      this.cbRTL.Text = "Right to left";
      this.cbRTL.UseVisualStyleBackColor = true;
      this.cbRTL.CheckedChanged += new System.EventHandler(this.cbRTL_CheckedChanged);
      // 
      // nudRows
      // 
      this.nudRows.Location = new System.Drawing.Point(211, 29);
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
      this.nudRows.ValueChanged += new System.EventHandler(this.grid_ValueChanged);
      this.nudRows.KeyUp += new System.Windows.Forms.KeyEventHandler(this.grid_KeyUp);
      // 
      // nudCols
      // 
      this.nudCols.Location = new System.Drawing.Point(151, 29);
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
      this.nudCols.ValueChanged += new System.EventHandler(this.grid_ValueChanged);
      this.nudCols.KeyUp += new System.Windows.Forms.KeyEventHandler(this.grid_KeyUp);
      // 
      // lblColumns
      // 
      this.lblColumns.AutoSize = true;
      this.lblColumns.Location = new System.Drawing.Point(21, 33);
      this.lblColumns.Name = "lblColumns";
      this.lblColumns.Size = new System.Drawing.Size(37, 13);
      this.lblColumns.TabIndex = 52;
      this.lblColumns.Text = "Table:";
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(150, 387);
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
      this.btnCancel.Location = new System.Drawing.Point(255, 387);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(99, 24);
      this.btnCancel.TabIndex = 57;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // grpAppearance
      // 
      this.grpAppearance.Controls.Add(this.cbBorderVisible);
      this.grpAppearance.Location = new System.Drawing.Point(14, 253);
      this.grpAppearance.Name = "grpAppearance";
      this.grpAppearance.Size = new System.Drawing.Size(338, 128);
      this.grpAppearance.TabIndex = 53;
      this.grpAppearance.TabStop = false;
      this.grpAppearance.Text = "Generic_Appearance";
      // 
      // cbBorderVisible
      // 
      this.cbBorderVisible.AutoSize = true;
      this.cbBorderVisible.Location = new System.Drawing.Point(24, 35);
      this.cbBorderVisible.Name = "cbBorderVisible";
      this.cbBorderVisible.Size = new System.Drawing.Size(86, 17);
      this.cbBorderVisible.TabIndex = 61;
      this.cbBorderVisible.Text = "Show border";
      this.cbBorderVisible.UseVisualStyleBackColor = true;
      this.cbBorderVisible.CheckedChanged += new System.EventHandler(this.cbBorderVisible_CheckedChanged);
      // 
      // btnApply
      // 
      this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnApply.Location = new System.Drawing.Point(14, 387);
      this.btnApply.Name = "btnApply";
      this.btnApply.Size = new System.Drawing.Size(100, 24);
      this.btnApply.TabIndex = 59;
      this.btnApply.Text = "Apply";
      this.btnApply.UseVisualStyleBackColor = true;
      this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
      // 
      // FormConfigureKinogram
      // 
      this.AcceptButton = this.btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(364, 423);
      this.Controls.Add(this.btnApply);
      this.Controls.Add(this.grpConfig);
      this.Controls.Add(this.grpAppearance);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnCancel);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormConfigureKinogram";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "FormConfigureKinogram";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_FormClosing);
      this.grpConfig.ResumeLayout(false);
      this.grpConfig.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudCropWidth)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudCropHeight)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudRows)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudCols)).EndInit();
      this.grpAppearance.ResumeLayout(false);
      this.grpAppearance.PerformLayout();
      this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblCropSize;
        private System.Windows.Forms.GroupBox grpConfig;
        private System.Windows.Forms.Label lblColumns;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpAppearance;
        private System.Windows.Forms.CheckBox cbRTL;
        private System.Windows.Forms.NumericUpDown nudCropHeight;
        private System.Windows.Forms.NumericUpDown nudCropWidth;
        private System.Windows.Forms.NumericUpDown nudRows;
        private System.Windows.Forms.NumericUpDown nudCols;
        private System.Windows.Forms.CheckBox cbBorderVisible;
        private System.Windows.Forms.Label lblFrameInterval;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnApply;
    }
}