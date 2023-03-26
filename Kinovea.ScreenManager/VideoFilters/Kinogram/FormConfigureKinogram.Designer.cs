
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
      this.nudRows = new System.Windows.Forms.NumericUpDown();
      this.nudCols = new System.Windows.Forms.NumericUpDown();
      this.lblColumns = new System.Windows.Forms.Label();
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnApply = new System.Windows.Forms.Button();
      this.grpConfig.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudCropWidth)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudCropHeight)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudRows)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudCols)).BeginInit();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(279, 65);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(18, 13);
      this.label1.TabIndex = 53;
      this.label1.Text = "px";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(212, 66);
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
      this.grpConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpConfig.Controls.Add(this.lblCropSize);
      this.grpConfig.Controls.Add(this.label4);
      this.grpConfig.Controls.Add(this.label2);
      this.grpConfig.Controls.Add(this.label1);
      this.grpConfig.Controls.Add(this.nudCropWidth);
      this.grpConfig.Controls.Add(this.nudCropHeight);
      this.grpConfig.Controls.Add(this.nudRows);
      this.grpConfig.Controls.Add(this.nudCols);
      this.grpConfig.Controls.Add(this.lblColumns);
      this.grpConfig.Location = new System.Drawing.Point(14, 12);
      this.grpConfig.Name = "grpConfig";
      this.grpConfig.Size = new System.Drawing.Size(314, 164);
      this.grpConfig.TabIndex = 58;
      this.grpConfig.TabStop = false;
      this.grpConfig.Text = "Generic_Configuration";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(204, 30);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(13, 13);
      this.label2.TabIndex = 63;
      this.label2.Text = "×";
      // 
      // lblFrameInterval
      // 
      this.lblFrameInterval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.lblFrameInterval.AutoSize = true;
      this.lblFrameInterval.Location = new System.Drawing.Point(11, 222);
      this.lblFrameInterval.Name = "lblFrameInterval";
      this.lblFrameInterval.Size = new System.Drawing.Size(76, 13);
      this.lblFrameInterval.TabIndex = 62;
      this.lblFrameInterval.Text = "Frame interval:";
      this.lblFrameInterval.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // nudCropWidth
      // 
      this.nudCropWidth.Location = new System.Drawing.Point(163, 62);
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
      this.lblTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.lblTotal.AutoSize = true;
      this.lblTotal.Location = new System.Drawing.Point(12, 191);
      this.lblTotal.Name = "lblTotal";
      this.lblTotal.Size = new System.Drawing.Size(44, 13);
      this.lblTotal.TabIndex = 61;
      this.lblTotal.Text = "Frames:";
      // 
      // nudCropHeight
      // 
      this.nudCropHeight.Location = new System.Drawing.Point(228, 62);
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
      // nudRows
      // 
      this.nudRows.Location = new System.Drawing.Point(223, 26);
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
      this.nudCols.Location = new System.Drawing.Point(163, 26);
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
      this.btnOK.Location = new System.Drawing.Point(126, 264);
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
      this.btnCancel.Location = new System.Drawing.Point(231, 264);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(99, 24);
      this.btnCancel.TabIndex = 57;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // btnApply
      // 
      this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnApply.Location = new System.Drawing.Point(14, 264);
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
      this.ClientSize = new System.Drawing.Size(340, 300);
      this.Controls.Add(this.btnApply);
      this.Controls.Add(this.grpConfig);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.lblFrameInterval);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.lblTotal);
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
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblCropSize;
        private System.Windows.Forms.GroupBox grpConfig;
        private System.Windows.Forms.Label lblColumns;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.NumericUpDown nudCropHeight;
        private System.Windows.Forms.NumericUpDown nudCropWidth;
        private System.Windows.Forms.NumericUpDown nudRows;
        private System.Windows.Forms.NumericUpDown nudCols;
        private System.Windows.Forms.Label lblFrameInterval;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnApply;
    }
}