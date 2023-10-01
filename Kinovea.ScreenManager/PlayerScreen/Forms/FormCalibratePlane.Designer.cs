#region License
/*
Copyright © Joan Charmant 2012.
jcharmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.
*/
#endregion
namespace Kinovea.ScreenManager
{
    partial class FormCalibratePlane
    {
        /// <summary>
        /// Designer variable used to keep track of non-visual components.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        
        /// <summary>
        /// Disposes resources used by the form.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                if (components != null) {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }
        
        /// <summary>
        /// This method is required for Windows Forms designer support.
        /// Do not change the method contents inside the source code editor. The Forms designer might
        /// not be able to load this method if it was changed manually.
        /// </summary>
        private void InitializeComponent()
        {
      this.components = new System.ComponentModel.Container();
      this.grpConfig = new System.Windows.Forms.GroupBox();
      this.nudA = new System.Windows.Forms.NumericUpDown();
      this.lblPrecision = new System.Windows.Forms.Label();
      this.lblSizeHelp = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.cbUnit = new System.Windows.Forms.ComboBox();
      this.lblRealSize = new System.Windows.Forms.Label();
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.pnlQuadrilateral = new System.Windows.Forms.Panel();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.label3 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.lblOffsetHelp = new System.Windows.Forms.Label();
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this.btnRotate90 = new System.Windows.Forms.Button();
      this.btnFlipY = new System.Windows.Forms.Button();
      this.btnFlipX = new System.Windows.Forms.Button();
      this.nudB = new System.Windows.Forms.NumericUpDown();
      this.nudOffsetX = new System.Windows.Forms.NumericUpDown();
      this.nudOffsetY = new System.Windows.Forms.NumericUpDown();
      this.grpConfig.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudA)).BeginInit();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudB)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudOffsetX)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudOffsetY)).BeginInit();
      this.SuspendLayout();
      // 
      // grpConfig
      // 
      this.grpConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpConfig.Controls.Add(this.nudB);
      this.grpConfig.Controls.Add(this.nudA);
      this.grpConfig.Controls.Add(this.lblPrecision);
      this.grpConfig.Controls.Add(this.lblSizeHelp);
      this.grpConfig.Controls.Add(this.label1);
      this.grpConfig.Controls.Add(this.cbUnit);
      this.grpConfig.Controls.Add(this.lblRealSize);
      this.grpConfig.Location = new System.Drawing.Point(12, 236);
      this.grpConfig.Name = "grpConfig";
      this.grpConfig.Size = new System.Drawing.Size(350, 139);
      this.grpConfig.TabIndex = 32;
      this.grpConfig.TabStop = false;
      this.grpConfig.Text = "Calibration";
      // 
      // nudA
      // 
      this.nudA.DecimalPlaces = 2;
      this.nudA.Location = new System.Drawing.Point(51, 60);
      this.nudA.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
      this.nudA.Name = "nudA";
      this.nudA.Size = new System.Drawing.Size(53, 20);
      this.nudA.TabIndex = 32;
      this.nudA.ValueChanged += new System.EventHandler(this.nud_ValueChanged);
      // 
      // lblPrecision
      // 
      this.lblPrecision.AutoSize = true;
      this.lblPrecision.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblPrecision.Location = new System.Drawing.Point(12, 106);
      this.lblPrecision.Name = "lblPrecision";
      this.lblPrecision.Size = new System.Drawing.Size(70, 13);
      this.lblPrecision.TabIndex = 31;
      this.lblPrecision.Text = "Precision text";
      this.lblPrecision.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
      // 
      // lblSizeHelp
      // 
      this.lblSizeHelp.AutoSize = true;
      this.lblSizeHelp.Location = new System.Drawing.Point(11, 29);
      this.lblSizeHelp.Name = "lblSizeHelp";
      this.lblSizeHelp.Size = new System.Drawing.Size(49, 13);
      this.lblSizeHelp.TabIndex = 30;
      this.lblSizeHelp.Text = "Help text";
      this.lblSizeHelp.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(115, 62);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(25, 13);
      this.label1.TabIndex = 26;
      this.label1.Text = "b =";
      this.label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
      // 
      // cbUnit
      // 
      this.cbUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbUnit.FormattingEnabled = true;
      this.cbUnit.Location = new System.Drawing.Point(213, 58);
      this.cbUnit.Name = "cbUnit";
      this.cbUnit.Size = new System.Drawing.Size(120, 21);
      this.cbUnit.TabIndex = 3;
      this.cbUnit.SelectedIndexChanged += new System.EventHandler(this.cbUnit_SelectedIndexChanged);
      // 
      // lblRealSize
      // 
      this.lblRealSize.AutoSize = true;
      this.lblRealSize.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblRealSize.Location = new System.Drawing.Point(24, 62);
      this.lblRealSize.Name = "lblRealSize";
      this.lblRealSize.Size = new System.Drawing.Size(25, 13);
      this.lblRealSize.TabIndex = 21;
      this.lblRealSize.Text = "a =";
      this.lblRealSize.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(165, 484);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(92, 24);
      this.btnOK.TabIndex = 4;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(270, 484);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(92, 24);
      this.btnCancel.TabIndex = 4;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // pnlQuadrilateral
      // 
      this.pnlQuadrilateral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pnlQuadrilateral.BackColor = System.Drawing.Color.Black;
      this.pnlQuadrilateral.ForeColor = System.Drawing.Color.White;
      this.pnlQuadrilateral.Location = new System.Drawing.Point(12, 12);
      this.pnlQuadrilateral.Name = "pnlQuadrilateral";
      this.pnlQuadrilateral.Size = new System.Drawing.Size(350, 186);
      this.pnlQuadrilateral.TabIndex = 34;
      this.pnlQuadrilateral.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlQuadrilateral_Paint);
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.nudOffsetY);
      this.groupBox1.Controls.Add(this.nudOffsetX);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.lblOffsetHelp);
      this.groupBox1.Location = new System.Drawing.Point(12, 381);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(350, 97);
      this.groupBox1.TabIndex = 33;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Coordinates";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.Location = new System.Drawing.Point(115, 62);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(25, 13);
      this.label3.TabIndex = 32;
      this.label3.Text = "Y =";
      this.label3.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(24, 62);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(25, 13);
      this.label2.TabIndex = 34;
      this.label2.Text = "X =";
      this.label2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
      // 
      // lblOffsetHelp
      // 
      this.lblOffsetHelp.AutoSize = true;
      this.lblOffsetHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblOffsetHelp.Location = new System.Drawing.Point(11, 29);
      this.lblOffsetHelp.Name = "lblOffsetHelp";
      this.lblOffsetHelp.Size = new System.Drawing.Size(145, 13);
      this.lblOffsetHelp.TabIndex = 32;
      this.lblOffsetHelp.Text = "Offset applied to coordinates.";
      this.lblOffsetHelp.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
      // 
      // btnRotate90
      // 
      this.btnRotate90.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnRotate90.FlatAppearance.BorderSize = 0;
      this.btnRotate90.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnRotate90.Image = global::Kinovea.ScreenManager.Properties.Resources.rotate_system;
      this.btnRotate90.Location = new System.Drawing.Point(338, 206);
      this.btnRotate90.Name = "btnRotate90";
      this.btnRotate90.Size = new System.Drawing.Size(24, 24);
      this.btnRotate90.TabIndex = 38;
      this.btnRotate90.UseVisualStyleBackColor = true;
      this.btnRotate90.Click += new System.EventHandler(this.btnRotate90_Click);
      // 
      // btnFlipY
      // 
      this.btnFlipY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnFlipY.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.btnFlipY.FlatAppearance.BorderSize = 0;
      this.btnFlipY.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnFlipY.Image = global::Kinovea.ScreenManager.Properties.Resources.flipy3;
      this.btnFlipY.Location = new System.Drawing.Point(308, 206);
      this.btnFlipY.Name = "btnFlipY";
      this.btnFlipY.Size = new System.Drawing.Size(24, 24);
      this.btnFlipY.TabIndex = 37;
      this.btnFlipY.UseVisualStyleBackColor = true;
      this.btnFlipY.Click += new System.EventHandler(this.btnFlipVert_Click);
      // 
      // btnFlipX
      // 
      this.btnFlipX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnFlipX.FlatAppearance.BorderSize = 0;
      this.btnFlipX.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnFlipX.Image = global::Kinovea.ScreenManager.Properties.Resources.flipx3;
      this.btnFlipX.Location = new System.Drawing.Point(278, 206);
      this.btnFlipX.Name = "btnFlipX";
      this.btnFlipX.Size = new System.Drawing.Size(24, 24);
      this.btnFlipX.TabIndex = 36;
      this.btnFlipX.UseVisualStyleBackColor = true;
      this.btnFlipX.Click += new System.EventHandler(this.btnFlipHorz_Click);
      // 
      // nudB
      // 
      this.nudB.DecimalPlaces = 2;
      this.nudB.Location = new System.Drawing.Point(142, 59);
      this.nudB.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
      this.nudB.Name = "nudB";
      this.nudB.Size = new System.Drawing.Size(53, 20);
      this.nudB.TabIndex = 33;
      this.nudB.ValueChanged += new System.EventHandler(this.nud_ValueChanged);
      // 
      // nudOffsetX
      // 
      this.nudOffsetX.DecimalPlaces = 2;
      this.nudOffsetX.Location = new System.Drawing.Point(51, 59);
      this.nudOffsetX.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
      this.nudOffsetX.Minimum = new decimal(new int[] {
            999,
            0,
            0,
            -2147483648});
      this.nudOffsetX.Name = "nudOffsetX";
      this.nudOffsetX.Size = new System.Drawing.Size(53, 20);
      this.nudOffsetX.TabIndex = 35;
      this.nudOffsetX.ValueChanged += new System.EventHandler(this.nud_ValueChanged);
      // 
      // nudOffsetY
      // 
      this.nudOffsetY.DecimalPlaces = 2;
      this.nudOffsetY.Location = new System.Drawing.Point(142, 59);
      this.nudOffsetY.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
      this.nudOffsetY.Minimum = new decimal(new int[] {
            999,
            0,
            0,
            -2147483648});
      this.nudOffsetY.Name = "nudOffsetY";
      this.nudOffsetY.Size = new System.Drawing.Size(53, 20);
      this.nudOffsetY.TabIndex = 36;
      this.nudOffsetY.ValueChanged += new System.EventHandler(this.nud_ValueChanged);
      // 
      // FormCalibratePlane
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(374, 517);
      this.Controls.Add(this.btnRotate90);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.btnFlipY);
      this.Controls.Add(this.pnlQuadrilateral);
      this.Controls.Add(this.btnFlipX);
      this.Controls.Add(this.grpConfig);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnCancel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormCalibratePlane";
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "FormCalibratePlane";
      this.grpConfig.ResumeLayout(false);
      this.grpConfig.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudA)).EndInit();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudB)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudOffsetX)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudOffsetY)).EndInit();
      this.ResumeLayout(false);

        }
        private System.Windows.Forms.Panel pnlQuadrilateral;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblRealSize;
        private System.Windows.Forms.ComboBox cbUnit;
        private System.Windows.Forms.GroupBox grpConfig;
        private System.Windows.Forms.Label lblSizeHelp;
        private System.Windows.Forms.Label lblPrecision;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblOffsetHelp;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnFlipX;
        private System.Windows.Forms.Button btnRotate90;
        private System.Windows.Forms.Button btnFlipY;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.NumericUpDown nudA;
        private System.Windows.Forms.NumericUpDown nudB;
        private System.Windows.Forms.NumericUpDown nudOffsetY;
        private System.Windows.Forms.NumericUpDown nudOffsetX;
    }
}
