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
      this.lblCameraPosition = new System.Windows.Forms.Label();
      this.lblFocalLength = new System.Windows.Forms.Label();
      this.lblSensorWidth = new System.Windows.Forms.Label();
      this.gpControlPoints = new System.Windows.Forms.GroupBox();
      this.gpValidationMode = new System.Windows.Forms.GroupBox();
      this.rbCompute3D = new System.Windows.Forms.RadioButton();
      this.rbFix3D = new System.Windows.Forms.RadioButton();
      this.rbFix1D = new System.Windows.Forms.RadioButton();
      this.olvControlPoints = new BrightIdeasSoftware.ObjectListView();
      this.btnCSV = new System.Windows.Forms.Button();
      this.lblCameraDistance = new System.Windows.Forms.Label();
      this.grpIntrinsics.SuspendLayout();
      this.gpControlPoints.SuspendLayout();
      this.gpValidationMode.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.olvControlPoints)).BeginInit();
      this.SuspendLayout();
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(293, 526);
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
      this.btnCancel.Location = new System.Drawing.Point(398, 526);
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
      this.grpIntrinsics.Controls.Add(this.lblCameraDistance);
      this.grpIntrinsics.Controls.Add(this.lblCameraPosition);
      this.grpIntrinsics.Controls.Add(this.lblFocalLength);
      this.grpIntrinsics.Controls.Add(this.lblSensorWidth);
      this.grpIntrinsics.Location = new System.Drawing.Point(16, 12);
      this.grpIntrinsics.Name = "grpIntrinsics";
      this.grpIntrinsics.Size = new System.Drawing.Size(477, 153);
      this.grpIntrinsics.TabIndex = 36;
      this.grpIntrinsics.TabStop = false;
      this.grpIntrinsics.Text = "Camera";
      // 
      // label1
      // 
      this.lblCameraPosition.AutoSize = true;
      this.lblCameraPosition.Location = new System.Drawing.Point(18, 83);
      this.lblCameraPosition.Name = "label1";
      this.lblCameraPosition.Size = new System.Drawing.Size(110, 13);
      this.lblCameraPosition.TabIndex = 51;
      this.lblCameraPosition.Text = "Camera position in 3D";
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
      // gpControlPoints
      // 
      this.gpControlPoints.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.gpControlPoints.Controls.Add(this.gpValidationMode);
      this.gpControlPoints.Controls.Add(this.olvControlPoints);
      this.gpControlPoints.Location = new System.Drawing.Point(16, 171);
      this.gpControlPoints.Name = "gpControlPoints";
      this.gpControlPoints.Size = new System.Drawing.Size(477, 349);
      this.gpControlPoints.TabIndex = 52;
      this.gpControlPoints.TabStop = false;
      this.gpControlPoints.Text = "Control points";
      // 
      // gpValidationMode
      // 
      this.gpValidationMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.gpValidationMode.Controls.Add(this.rbCompute3D);
      this.gpValidationMode.Controls.Add(this.rbFix3D);
      this.gpValidationMode.Controls.Add(this.rbFix1D);
      this.gpValidationMode.Location = new System.Drawing.Point(17, 25);
      this.gpValidationMode.Name = "gpValidationMode";
      this.gpValidationMode.Size = new System.Drawing.Size(443, 108);
      this.gpValidationMode.TabIndex = 27;
      this.gpValidationMode.TabStop = false;
      this.gpValidationMode.Text = "Validation mode";
      // 
      // rbCompute3D
      // 
      this.rbCompute3D.AutoSize = true;
      this.rbCompute3D.Location = new System.Drawing.Point(23, 73);
      this.rbCompute3D.Name = "rbCompute3D";
      this.rbCompute3D.Size = new System.Drawing.Size(128, 17);
      this.rbCompute3D.TabIndex = 2;
      this.rbCompute3D.TabStop = true;
      this.rbCompute3D.Text = "Compute 3D positions";
      this.rbCompute3D.UseVisualStyleBackColor = true;
      this.rbCompute3D.CheckedChanged += new System.EventHandler(this.validationMode_Changed);
      // 
      // rbFix3D
      // 
      this.rbFix3D.AutoSize = true;
      this.rbFix3D.Location = new System.Drawing.Point(23, 25);
      this.rbFix3D.Name = "rbFix3D";
      this.rbFix3D.Size = new System.Drawing.Size(290, 17);
      this.rbFix3D.TabIndex = 1;
      this.rbFix3D.TabStop = true;
      this.rbFix3D.Text = "Fix all axes, verify the location of the marker in the image";
      this.rbFix3D.UseVisualStyleBackColor = true;
      this.rbFix3D.CheckedChanged += new System.EventHandler(this.validationMode_Changed);
      // 
      // rbFix1D
      // 
      this.rbFix1D.AutoSize = true;
      this.rbFix1D.Location = new System.Drawing.Point(23, 48);
      this.rbFix1D.Name = "rbFix1D";
      this.rbFix1D.Size = new System.Drawing.Size(176, 17);
      this.rbFix1D.TabIndex = 0;
      this.rbFix1D.TabStop = true;
      this.rbFix1D.Text = "Fix one axis, verify the other two";
      this.rbFix1D.UseVisualStyleBackColor = true;
      this.rbFix1D.CheckedChanged += new System.EventHandler(this.validationMode_Changed);
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
      this.olvControlPoints.Location = new System.Drawing.Point(17, 139);
      this.olvControlPoints.Name = "olvControlPoints";
      this.olvControlPoints.ShowSortIndicators = false;
      this.olvControlPoints.Size = new System.Drawing.Size(443, 192);
      this.olvControlPoints.TabIndex = 26;
      this.olvControlPoints.UseCompatibleStateImageBehavior = false;
      this.olvControlPoints.View = System.Windows.Forms.View.Details;
      this.olvControlPoints.CellEditFinished += new BrightIdeasSoftware.CellEditEventHandler(this.olvSections_CellEditFinished);
      this.olvControlPoints.FormatCell += new System.EventHandler<BrightIdeasSoftware.FormatCellEventArgs>(this.olvControlPoints_FormatCell);
      this.olvControlPoints.FormatRow += new System.EventHandler<BrightIdeasSoftware.FormatRowEventArgs>(this.olvControlPoints_FormatRow);
      // 
      // btnCSV
      // 
      this.btnCSV.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCSV.Location = new System.Drawing.Point(16, 526);
      this.btnCSV.Name = "btnCSV";
      this.btnCSV.Size = new System.Drawing.Size(99, 24);
      this.btnCSV.TabIndex = 53;
      this.btnCSV.Text = "Copy to clipboard";
      this.btnCSV.UseVisualStyleBackColor = true;
      this.btnCSV.Click += new System.EventHandler(this.btnCSV_Click);
      // 
      // lblCameraDistance
      // 
      this.lblCameraDistance.AutoSize = true;
      this.lblCameraDistance.Location = new System.Drawing.Point(18, 113);
      this.lblCameraDistance.Name = "lblCameraDistance";
      this.lblCameraDistance.Size = new System.Drawing.Size(86, 13);
      this.lblCameraDistance.TabIndex = 52;
      this.lblCameraDistance.Text = "Camera distance";
      // 
      // FormCalibrationValidation
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(505, 562);
      this.Controls.Add(this.btnCSV);
      this.Controls.Add(this.gpControlPoints);
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
      this.gpControlPoints.ResumeLayout(false);
      this.gpValidationMode.ResumeLayout(false);
      this.gpValidationMode.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.olvControlPoints)).EndInit();
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpIntrinsics;
        private System.Windows.Forms.Label lblFocalLength;
        private System.Windows.Forms.Label lblSensorWidth;
        private System.Windows.Forms.Label lblCameraPosition;
        private System.Windows.Forms.GroupBox gpControlPoints;
        private BrightIdeasSoftware.ObjectListView olvControlPoints;
        private System.Windows.Forms.GroupBox gpValidationMode;
        private System.Windows.Forms.RadioButton rbFix3D;
        private System.Windows.Forms.RadioButton rbFix1D;
        private System.Windows.Forms.RadioButton rbCompute3D;
        private System.Windows.Forms.Button btnCSV;
        private System.Windows.Forms.Label lblCameraDistance;
    }
}