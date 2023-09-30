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
      this.grpConfig = new System.Windows.Forms.GroupBox();
      this.lblPrecision = new System.Windows.Forms.Label();
      this.lblSizeHelp = new System.Windows.Forms.Label();
      this.tbB = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.cbUnit = new System.Windows.Forms.ComboBox();
      this.tbA = new System.Windows.Forms.TextBox();
      this.lblRealSize = new System.Windows.Forms.Label();
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.pnlQuadrilateral = new System.Windows.Forms.Panel();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.label3 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.lblOffsetHelp = new System.Windows.Forms.Label();
      this.tbOffsetY = new System.Windows.Forms.TextBox();
      this.tbOffsetX = new System.Windows.Forms.TextBox();
      this.grpConfig.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // grpConfig
      // 
      this.grpConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpConfig.Controls.Add(this.lblPrecision);
      this.grpConfig.Controls.Add(this.lblSizeHelp);
      this.grpConfig.Controls.Add(this.tbB);
      this.grpConfig.Controls.Add(this.label1);
      this.grpConfig.Controls.Add(this.cbUnit);
      this.grpConfig.Controls.Add(this.tbA);
      this.grpConfig.Controls.Add(this.lblRealSize);
      this.grpConfig.Location = new System.Drawing.Point(12, 204);
      this.grpConfig.Name = "grpConfig";
      this.grpConfig.Size = new System.Drawing.Size(337, 139);
      this.grpConfig.TabIndex = 32;
      this.grpConfig.TabStop = false;
      this.grpConfig.Text = "Calibration";
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
      // tbB
      // 
      this.tbB.AcceptsReturn = true;
      this.tbB.Location = new System.Drawing.Point(137, 59);
      this.tbB.MaxLength = 10;
      this.tbB.Name = "tbB";
      this.tbB.Size = new System.Drawing.Size(40, 20);
      this.tbB.TabIndex = 2;
      this.tbB.TextChanged += new System.EventHandler(this.textBox_TextChanged);
      this.tbB.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_KeyPress);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(110, 62);
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
      this.cbUnit.Location = new System.Drawing.Point(200, 58);
      this.cbUnit.Name = "cbUnit";
      this.cbUnit.Size = new System.Drawing.Size(120, 21);
      this.cbUnit.TabIndex = 3;
      this.cbUnit.SelectedIndexChanged += new System.EventHandler(this.cbUnit_SelectedIndexChanged);
      // 
      // tbA
      // 
      this.tbA.AcceptsReturn = true;
      this.tbA.Location = new System.Drawing.Point(51, 59);
      this.tbA.MaxLength = 10;
      this.tbA.Name = "tbA";
      this.tbA.Size = new System.Drawing.Size(40, 20);
      this.tbA.TabIndex = 1;
      this.tbA.TextChanged += new System.EventHandler(this.textBox_TextChanged);
      this.tbA.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_KeyPress);
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
      this.btnOK.Location = new System.Drawing.Point(152, 465);
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
      this.btnCancel.Location = new System.Drawing.Point(257, 465);
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
      this.pnlQuadrilateral.Size = new System.Drawing.Size(337, 186);
      this.pnlQuadrilateral.TabIndex = 34;
      this.pnlQuadrilateral.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlQuadrilateral_Paint);
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.lblOffsetHelp);
      this.groupBox1.Controls.Add(this.tbOffsetY);
      this.groupBox1.Controls.Add(this.tbOffsetX);
      this.groupBox1.Location = new System.Drawing.Point(12, 349);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(337, 100);
      this.groupBox1.TabIndex = 33;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Coordinates";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.Location = new System.Drawing.Point(106, 62);
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
      // tbOffsetY
      // 
      this.tbOffsetY.AcceptsReturn = true;
      this.tbOffsetY.Location = new System.Drawing.Point(137, 58);
      this.tbOffsetY.MaxLength = 10;
      this.tbOffsetY.Name = "tbOffsetY";
      this.tbOffsetY.Size = new System.Drawing.Size(40, 20);
      this.tbOffsetY.TabIndex = 2;
      this.tbOffsetY.TextChanged += new System.EventHandler(this.textBox_TextChanged);
      this.tbOffsetY.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_KeyPress);
      // 
      // tbOffsetX
      // 
      this.tbOffsetX.AcceptsReturn = true;
      this.tbOffsetX.Location = new System.Drawing.Point(51, 58);
      this.tbOffsetX.MaxLength = 10;
      this.tbOffsetX.Name = "tbOffsetX";
      this.tbOffsetX.Size = new System.Drawing.Size(40, 20);
      this.tbOffsetX.TabIndex = 1;
      this.tbOffsetX.TextChanged += new System.EventHandler(this.textBox_TextChanged);
      this.tbOffsetX.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_KeyPress);
      // 
      // FormCalibratePlane
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(361, 498);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.pnlQuadrilateral);
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
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

        }
        private System.Windows.Forms.Panel pnlQuadrilateral;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbB;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblRealSize;
        private System.Windows.Forms.TextBox tbA;
        private System.Windows.Forms.ComboBox cbUnit;
        private System.Windows.Forms.GroupBox grpConfig;
        private System.Windows.Forms.Label lblSizeHelp;
        private System.Windows.Forms.Label lblPrecision;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblOffsetHelp;
        private System.Windows.Forms.TextBox tbOffsetY;
        private System.Windows.Forms.TextBox tbOffsetX;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
    }
}
