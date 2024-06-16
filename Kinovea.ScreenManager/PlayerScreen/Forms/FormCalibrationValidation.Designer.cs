namespace Kinovea.ScreenManager
{
    partial class FormCalibrationValidation
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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.olvControlPoints = new BrightIdeasSoftware.ObjectListView();
      this.grpIntrinsics.SuspendLayout();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.olvControlPoints)).BeginInit();
      this.SuspendLayout();
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(224, 350);
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
      this.btnCancel.Location = new System.Drawing.Point(329, 350);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(99, 24);
      this.btnCancel.TabIndex = 32;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // grpIntrinsics
      // 
      this.grpIntrinsics.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpIntrinsics.Controls.Add(this.label1);
      this.grpIntrinsics.Controls.Add(this.lblFocalLength);
      this.grpIntrinsics.Controls.Add(this.lblSensorWidth);
      this.grpIntrinsics.Location = new System.Drawing.Point(16, 12);
      this.grpIntrinsics.Name = "grpIntrinsics";
      this.grpIntrinsics.Size = new System.Drawing.Size(408, 111);
      this.grpIntrinsics.TabIndex = 36;
      this.grpIntrinsics.TabStop = false;
      this.grpIntrinsics.Text = "Camera";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(18, 83);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(110, 13);
      this.label1.TabIndex = 51;
      this.label1.Text = "Camera position in 3D";
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
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.olvControlPoints);
      this.groupBox1.Location = new System.Drawing.Point(20, 129);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(408, 211);
      this.groupBox1.TabIndex = 52;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Control points";
      // 
      // olvControlPoints
      // 
      this.olvControlPoints.AlternateRowBackColor = System.Drawing.Color.Gainsboro;
      this.olvControlPoints.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.olvControlPoints.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.SingleClickAlways;
      this.olvControlPoints.CellEditUseWholeCell = false;
      this.olvControlPoints.Cursor = System.Windows.Forms.Cursors.Default;
      this.olvControlPoints.GridLines = true;
      this.olvControlPoints.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.olvControlPoints.HideSelection = false;
      this.olvControlPoints.Location = new System.Drawing.Point(17, 34);
      this.olvControlPoints.Name = "olvControlPoints";
      this.olvControlPoints.ShowSortIndicators = false;
      this.olvControlPoints.Size = new System.Drawing.Size(374, 156);
      this.olvControlPoints.TabIndex = 26;
      this.olvControlPoints.UseCompatibleStateImageBehavior = false;
      this.olvControlPoints.View = System.Windows.Forms.View.Details;
      this.olvControlPoints.CellEditFinished += new BrightIdeasSoftware.CellEditEventHandler(this.olvSections_CellEditFinished);
      this.olvControlPoints.FormatCell += new System.EventHandler<BrightIdeasSoftware.FormatCellEventArgs>(this.olvControlPoints_FormatCell);
      this.olvControlPoints.FormatRow += new System.EventHandler<BrightIdeasSoftware.FormatRowEventArgs>(this.olvControlPoints_FormatRow);
      // 
      // FormCalibrationValidation
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(436, 386);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.grpIntrinsics);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnCancel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormCalibrationValidation";
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.Text = "FormVDM";
      this.grpIntrinsics.ResumeLayout(false);
      this.grpIntrinsics.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.olvControlPoints)).EndInit();
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpIntrinsics;
        private System.Windows.Forms.Label lblFocalLength;
        private System.Windows.Forms.Label lblSensorWidth;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private BrightIdeasSoftware.ObjectListView olvControlPoints;
    }
}