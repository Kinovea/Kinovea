namespace Kinovea.ScreenManager
{
    partial class FormCalibrateDistortion
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
      this.grpDistortionCoefficients = new System.Windows.Forms.GroupBox();
      this.nudP2 = new System.Windows.Forms.NumericUpDown();
      this.nudP1 = new System.Windows.Forms.NumericUpDown();
      this.nudK3 = new System.Windows.Forms.NumericUpDown();
      this.nudK2 = new System.Windows.Forms.NumericUpDown();
      this.lblP2 = new System.Windows.Forms.Label();
      this.lblP1 = new System.Windows.Forms.Label();
      this.lblK3 = new System.Windows.Forms.Label();
      this.lblK2 = new System.Windows.Forms.Label();
      this.lblK1 = new System.Windows.Forms.Label();
      this.nudK1 = new System.Windows.Forms.NumericUpDown();
      this.grpIntrinsics = new System.Windows.Forms.GroupBox();
      this.nudFocalLength = new System.Windows.Forms.NumericUpDown();
      this.lblFocalLength = new System.Windows.Forms.Label();
      this.nudSensorWidth = new System.Windows.Forms.NumericUpDown();
      this.lblSensorWidth = new System.Windows.Forms.Label();
      this.nudCy = new System.Windows.Forms.NumericUpDown();
      this.nudCx = new System.Windows.Forms.NumericUpDown();
      this.nudFy = new System.Windows.Forms.NumericUpDown();
      this.nudFx = new System.Windows.Forms.NumericUpDown();
      this.lblCy = new System.Windows.Forms.Label();
      this.lblCx = new System.Windows.Forms.Label();
      this.lblFy = new System.Windows.Forms.Label();
      this.lblFx = new System.Windows.Forms.Label();
      this.menuStrip1 = new System.Windows.Forms.MenuStrip();
      this.mnuFile = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuOpen = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuSave = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuDefault = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuQuit = new System.Windows.Forms.ToolStripMenuItem();
      this.grpAppearance = new System.Windows.Forms.GroupBox();
      this.pnlPreview = new Kinovea.ScreenManager.PanelDoubleBuffer();
      this.grpDistortionCoefficients.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudP2)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudP1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudK3)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudK2)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudK1)).BeginInit();
      this.grpIntrinsics.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudFocalLength)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudSensorWidth)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudCy)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudCx)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudFy)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudFx)).BeginInit();
      this.menuStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(796, 610);
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
      this.btnCancel.Location = new System.Drawing.Point(901, 610);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(99, 24);
      this.btnCancel.TabIndex = 32;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // grpDistortionCoefficients
      // 
      this.grpDistortionCoefficients.Controls.Add(this.nudP2);
      this.grpDistortionCoefficients.Controls.Add(this.nudP1);
      this.grpDistortionCoefficients.Controls.Add(this.nudK3);
      this.grpDistortionCoefficients.Controls.Add(this.nudK2);
      this.grpDistortionCoefficients.Controls.Add(this.lblP2);
      this.grpDistortionCoefficients.Controls.Add(this.lblP1);
      this.grpDistortionCoefficients.Controls.Add(this.lblK3);
      this.grpDistortionCoefficients.Controls.Add(this.lblK2);
      this.grpDistortionCoefficients.Controls.Add(this.lblK1);
      this.grpDistortionCoefficients.Controls.Add(this.nudK1);
      this.grpDistortionCoefficients.Location = new System.Drawing.Point(16, 255);
      this.grpDistortionCoefficients.Name = "grpDistortionCoefficients";
      this.grpDistortionCoefficients.Size = new System.Drawing.Size(219, 165);
      this.grpDistortionCoefficients.TabIndex = 35;
      this.grpDistortionCoefficients.TabStop = false;
      this.grpDistortionCoefficients.Text = "Distortion coefficients";
      // 
      // nudP2
      // 
      this.nudP2.DecimalPlaces = 3;
      this.nudP2.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
      this.nudP2.Location = new System.Drawing.Point(134, 130);
      this.nudP2.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
      this.nudP2.Name = "nudP2";
      this.nudP2.Size = new System.Drawing.Size(70, 20);
      this.nudP2.TabIndex = 46;
      this.nudP2.ValueChanged += new System.EventHandler(this.parameters_ValueChanged);
      // 
      // nudP1
      // 
      this.nudP1.DecimalPlaces = 3;
      this.nudP1.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
      this.nudP1.Location = new System.Drawing.Point(134, 104);
      this.nudP1.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
      this.nudP1.Name = "nudP1";
      this.nudP1.Size = new System.Drawing.Size(70, 20);
      this.nudP1.TabIndex = 45;
      this.nudP1.ValueChanged += new System.EventHandler(this.parameters_ValueChanged);
      // 
      // nudK3
      // 
      this.nudK3.DecimalPlaces = 3;
      this.nudK3.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
      this.nudK3.Location = new System.Drawing.Point(134, 78);
      this.nudK3.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
      this.nudK3.Name = "nudK3";
      this.nudK3.Size = new System.Drawing.Size(70, 20);
      this.nudK3.TabIndex = 44;
      this.nudK3.ValueChanged += new System.EventHandler(this.parameters_ValueChanged);
      // 
      // nudK2
      // 
      this.nudK2.DecimalPlaces = 3;
      this.nudK2.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
      this.nudK2.Location = new System.Drawing.Point(134, 52);
      this.nudK2.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
      this.nudK2.Name = "nudK2";
      this.nudK2.Size = new System.Drawing.Size(70, 20);
      this.nudK2.TabIndex = 43;
      this.nudK2.ValueChanged += new System.EventHandler(this.parameters_ValueChanged);
      // 
      // lblP2
      // 
      this.lblP2.AutoSize = true;
      this.lblP2.Location = new System.Drawing.Point(18, 132);
      this.lblP2.Name = "lblP2";
      this.lblP2.Size = new System.Drawing.Size(25, 13);
      this.lblP2.TabIndex = 4;
      this.lblP2.Text = "p2 :";
      // 
      // lblP1
      // 
      this.lblP1.AutoSize = true;
      this.lblP1.Location = new System.Drawing.Point(18, 106);
      this.lblP1.Name = "lblP1";
      this.lblP1.Size = new System.Drawing.Size(25, 13);
      this.lblP1.TabIndex = 3;
      this.lblP1.Text = "p1 :";
      // 
      // lblK3
      // 
      this.lblK3.AutoSize = true;
      this.lblK3.Location = new System.Drawing.Point(18, 80);
      this.lblK3.Name = "lblK3";
      this.lblK3.Size = new System.Drawing.Size(25, 13);
      this.lblK3.TabIndex = 2;
      this.lblK3.Text = "k3 :";
      // 
      // lblK2
      // 
      this.lblK2.AutoSize = true;
      this.lblK2.Location = new System.Drawing.Point(18, 54);
      this.lblK2.Name = "lblK2";
      this.lblK2.Size = new System.Drawing.Size(25, 13);
      this.lblK2.TabIndex = 1;
      this.lblK2.Text = "k2 :";
      // 
      // lblK1
      // 
      this.lblK1.AutoSize = true;
      this.lblK1.Location = new System.Drawing.Point(18, 28);
      this.lblK1.Name = "lblK1";
      this.lblK1.Size = new System.Drawing.Size(25, 13);
      this.lblK1.TabIndex = 0;
      this.lblK1.Text = "k1 :";
      // 
      // nudK1
      // 
      this.nudK1.DecimalPlaces = 3;
      this.nudK1.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
      this.nudK1.Location = new System.Drawing.Point(134, 26);
      this.nudK1.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
      this.nudK1.Name = "nudK1";
      this.nudK1.Size = new System.Drawing.Size(70, 20);
      this.nudK1.TabIndex = 42;
      this.nudK1.ValueChanged += new System.EventHandler(this.parameters_ValueChanged);
      // 
      // grpIntrinsics
      // 
      this.grpIntrinsics.Controls.Add(this.nudFocalLength);
      this.grpIntrinsics.Controls.Add(this.lblFocalLength);
      this.grpIntrinsics.Controls.Add(this.nudSensorWidth);
      this.grpIntrinsics.Controls.Add(this.lblSensorWidth);
      this.grpIntrinsics.Controls.Add(this.nudCy);
      this.grpIntrinsics.Controls.Add(this.nudCx);
      this.grpIntrinsics.Controls.Add(this.nudFy);
      this.grpIntrinsics.Controls.Add(this.nudFx);
      this.grpIntrinsics.Controls.Add(this.lblCy);
      this.grpIntrinsics.Controls.Add(this.lblCx);
      this.grpIntrinsics.Controls.Add(this.lblFy);
      this.grpIntrinsics.Controls.Add(this.lblFx);
      this.grpIntrinsics.Location = new System.Drawing.Point(16, 36);
      this.grpIntrinsics.Name = "grpIntrinsics";
      this.grpIntrinsics.Size = new System.Drawing.Size(219, 213);
      this.grpIntrinsics.TabIndex = 36;
      this.grpIntrinsics.TabStop = false;
      this.grpIntrinsics.Text = "Camera intrinsics";
      // 
      // nudFocalLength
      // 
      this.nudFocalLength.DecimalPlaces = 3;
      this.nudFocalLength.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
      this.nudFocalLength.Location = new System.Drawing.Point(134, 54);
      this.nudFocalLength.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this.nudFocalLength.Name = "nudFocalLength";
      this.nudFocalLength.Size = new System.Drawing.Size(70, 20);
      this.nudFocalLength.TabIndex = 51;
      this.nudFocalLength.ValueChanged += new System.EventHandler(this.physicalParameters_ValueChanged);
      // 
      // lblFocalLength
      // 
      this.lblFocalLength.AutoSize = true;
      this.lblFocalLength.Location = new System.Drawing.Point(18, 56);
      this.lblFocalLength.Name = "lblFocalLength";
      this.lblFocalLength.Size = new System.Drawing.Size(96, 13);
      this.lblFocalLength.TabIndex = 50;
      this.lblFocalLength.Text = "Focal length (mm) :";
      // 
      // nudSensorWidth
      // 
      this.nudSensorWidth.DecimalPlaces = 3;
      this.nudSensorWidth.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
      this.nudSensorWidth.Location = new System.Drawing.Point(134, 28);
      this.nudSensorWidth.Name = "nudSensorWidth";
      this.nudSensorWidth.Size = new System.Drawing.Size(70, 20);
      this.nudSensorWidth.TabIndex = 49;
      this.nudSensorWidth.ValueChanged += new System.EventHandler(this.physicalParameters_ValueChanged);
      // 
      // lblSensorWidth
      // 
      this.lblSensorWidth.AutoSize = true;
      this.lblSensorWidth.Location = new System.Drawing.Point(18, 30);
      this.lblSensorWidth.Name = "lblSensorWidth";
      this.lblSensorWidth.Size = new System.Drawing.Size(99, 13);
      this.lblSensorWidth.TabIndex = 48;
      this.lblSensorWidth.Text = "Sensor width (mm) :";
      // 
      // nudCy
      // 
      this.nudCy.DecimalPlaces = 3;
      this.nudCy.Location = new System.Drawing.Point(134, 179);
      this.nudCy.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
      this.nudCy.Minimum = new decimal(new int[] {
            10000,
            0,
            0,
            -2147483648});
      this.nudCy.Name = "nudCy";
      this.nudCy.Size = new System.Drawing.Size(70, 20);
      this.nudCy.TabIndex = 46;
      this.nudCy.ValueChanged += new System.EventHandler(this.parameters_ValueChanged);
      // 
      // nudCx
      // 
      this.nudCx.DecimalPlaces = 3;
      this.nudCx.Location = new System.Drawing.Point(134, 153);
      this.nudCx.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
      this.nudCx.Minimum = new decimal(new int[] {
            10000,
            0,
            0,
            -2147483648});
      this.nudCx.Name = "nudCx";
      this.nudCx.Size = new System.Drawing.Size(70, 20);
      this.nudCx.TabIndex = 45;
      this.nudCx.ValueChanged += new System.EventHandler(this.parameters_ValueChanged);
      // 
      // nudFy
      // 
      this.nudFy.DecimalPlaces = 3;
      this.nudFy.Location = new System.Drawing.Point(134, 125);
      this.nudFy.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
      this.nudFy.Name = "nudFy";
      this.nudFy.Size = new System.Drawing.Size(70, 20);
      this.nudFy.TabIndex = 44;
      this.nudFy.ValueChanged += new System.EventHandler(this.parameters_ValueChanged);
      // 
      // nudFx
      // 
      this.nudFx.DecimalPlaces = 3;
      this.nudFx.Location = new System.Drawing.Point(134, 99);
      this.nudFx.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
      this.nudFx.Name = "nudFx";
      this.nudFx.Size = new System.Drawing.Size(70, 20);
      this.nudFx.TabIndex = 43;
      this.nudFx.ValueChanged += new System.EventHandler(this.nudFx_ValueChanged);
      // 
      // lblCy
      // 
      this.lblCy.AutoSize = true;
      this.lblCy.Location = new System.Drawing.Point(18, 181);
      this.lblCy.Name = "lblCy";
      this.lblCy.Size = new System.Drawing.Size(24, 13);
      this.lblCy.TabIndex = 8;
      this.lblCy.Text = "cy :";
      // 
      // lblCx
      // 
      this.lblCx.AutoSize = true;
      this.lblCx.Location = new System.Drawing.Point(18, 155);
      this.lblCx.Name = "lblCx";
      this.lblCx.Size = new System.Drawing.Size(24, 13);
      this.lblCx.TabIndex = 7;
      this.lblCx.Text = "cx :";
      // 
      // lblFy
      // 
      this.lblFy.AutoSize = true;
      this.lblFy.Location = new System.Drawing.Point(18, 127);
      this.lblFy.Name = "lblFy";
      this.lblFy.Size = new System.Drawing.Size(21, 13);
      this.lblFy.TabIndex = 6;
      this.lblFy.Text = "fy :";
      // 
      // lblFx
      // 
      this.lblFx.AutoSize = true;
      this.lblFx.Location = new System.Drawing.Point(18, 101);
      this.lblFx.Name = "lblFx";
      this.lblFx.Size = new System.Drawing.Size(21, 13);
      this.lblFx.TabIndex = 5;
      this.lblFx.Text = "fx :";
      // 
      // menuStrip1
      // 
      this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFile});
      this.menuStrip1.Location = new System.Drawing.Point(0, 0);
      this.menuStrip1.Name = "menuStrip1";
      this.menuStrip1.Size = new System.Drawing.Size(1008, 24);
      this.menuStrip1.TabIndex = 41;
      this.menuStrip1.Text = "menuStrip1";
      // 
      // mnuFile
      // 
      this.mnuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuOpen,
            this.mnuSave,
            this.mnuDefault,
            this.mnuQuit});
      this.mnuFile.Name = "mnuFile";
      this.mnuFile.Size = new System.Drawing.Size(37, 20);
      this.mnuFile.Text = "File";
      // 
      // mnuOpen
      // 
      this.mnuOpen.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.mnuOpen.Image = global::Kinovea.ScreenManager.Properties.Resources.folder;
      this.mnuOpen.Name = "mnuOpen";
      this.mnuOpen.Size = new System.Drawing.Size(153, 22);
      this.mnuOpen.Text = "Open";
      // 
      // mnuSave
      // 
      this.mnuSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.mnuSave.Image = global::Kinovea.ScreenManager.Properties.Resources.disk;
      this.mnuSave.Name = "mnuSave";
      this.mnuSave.Size = new System.Drawing.Size(153, 22);
      this.mnuSave.Text = "Save";
      // 
      // mnuDefault
      // 
      this.mnuDefault.Image = global::Kinovea.ScreenManager.Properties.Resources.null_symbol_16;
      this.mnuDefault.Name = "mnuDefault";
      this.mnuDefault.Size = new System.Drawing.Size(153, 22);
      this.mnuDefault.Text = "Restore default";
      // 
      // mnuQuit
      // 
      this.mnuQuit.Image = global::Kinovea.ScreenManager.Properties.Resources.quit2;
      this.mnuQuit.Name = "mnuQuit";
      this.mnuQuit.Size = new System.Drawing.Size(153, 22);
      this.mnuQuit.Text = "Quit";
      // 
      // grpAppearance
      // 
      this.grpAppearance.Location = new System.Drawing.Point(16, 426);
      this.grpAppearance.Name = "grpAppearance";
      this.grpAppearance.Size = new System.Drawing.Size(219, 178);
      this.grpAppearance.TabIndex = 47;
      this.grpAppearance.TabStop = false;
      this.grpAppearance.Text = "Appearance";
      // 
      // pnlPreview
      // 
      this.pnlPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pnlPreview.BackColor = System.Drawing.Color.DimGray;
      this.pnlPreview.Location = new System.Drawing.Point(241, 43);
      this.pnlPreview.Name = "pnlPreview";
      this.pnlPreview.Size = new System.Drawing.Size(759, 561);
      this.pnlPreview.TabIndex = 48;
      this.pnlPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlPreview_Paint);
      this.pnlPreview.Resize += new System.EventHandler(this.pnlPreview_Resize);
      // 
      // FormCalibrateDistortion
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(1008, 646);
      this.Controls.Add(this.pnlPreview);
      this.Controls.Add(this.grpAppearance);
      this.Controls.Add(this.grpIntrinsics);
      this.Controls.Add(this.grpDistortionCoefficients);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.menuStrip1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MainMenuStrip = this.menuStrip1;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormCalibrateDistortion";
      this.Text = "FormCalibrateDistortion";
      this.grpDistortionCoefficients.ResumeLayout(false);
      this.grpDistortionCoefficients.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudP2)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudP1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudK3)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudK2)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudK1)).EndInit();
      this.grpIntrinsics.ResumeLayout(false);
      this.grpIntrinsics.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudFocalLength)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudSensorWidth)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudCy)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudCx)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudFy)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudFx)).EndInit();
      this.menuStrip1.ResumeLayout(false);
      this.menuStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpDistortionCoefficients;
        private System.Windows.Forms.Label lblP2;
        private System.Windows.Forms.Label lblP1;
        private System.Windows.Forms.Label lblK3;
        private System.Windows.Forms.Label lblK2;
        private System.Windows.Forms.Label lblK1;
        private System.Windows.Forms.GroupBox grpIntrinsics;
        private System.Windows.Forms.Label lblCy;
        private System.Windows.Forms.Label lblCx;
        private System.Windows.Forms.Label lblFy;
        private System.Windows.Forms.Label lblFx;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuFile;
        private System.Windows.Forms.ToolStripMenuItem mnuOpen;
        private System.Windows.Forms.ToolStripMenuItem mnuSave;
        private System.Windows.Forms.ToolStripMenuItem mnuDefault;
        private System.Windows.Forms.ToolStripMenuItem mnuQuit;
        private System.Windows.Forms.NumericUpDown nudK1;
        private System.Windows.Forms.NumericUpDown nudK3;
        private System.Windows.Forms.NumericUpDown nudK2;
        private System.Windows.Forms.NumericUpDown nudP2;
        private System.Windows.Forms.NumericUpDown nudP1;
        private System.Windows.Forms.NumericUpDown nudCy;
        private System.Windows.Forms.NumericUpDown nudCx;
        private System.Windows.Forms.NumericUpDown nudFy;
        private System.Windows.Forms.NumericUpDown nudFx;
        private System.Windows.Forms.GroupBox grpAppearance;
        private PanelDoubleBuffer pnlPreview;
        private System.Windows.Forms.NumericUpDown nudFocalLength;
        private System.Windows.Forms.Label lblFocalLength;
        private System.Windows.Forms.NumericUpDown nudSensorWidth;
        private System.Windows.Forms.Label lblSensorWidth;
    }
}