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
            this.button1 = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblP2 = new System.Windows.Forms.Label();
            this.lblP1 = new System.Windows.Forms.Label();
            this.lblK3 = new System.Windows.Forms.Label();
            this.lblK2 = new System.Windows.Forms.Label();
            this.lblK1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lblCy = new System.Windows.Forms.Label();
            this.lblCx = new System.Windows.Forms.Label();
            this.lblFy = new System.Windows.Forms.Label();
            this.lblFx = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(363, 235);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(99, 24);
            this.btnOK.TabIndex = 31;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(468, 235);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(99, 24);
            this.btnCancel.TabIndex = 32;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Location = new System.Drawing.Point(12, 236);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(115, 23);
            this.button1.TabIndex = 33;
            this.button1.Text = "Calibrate camera";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.panel1.Location = new System.Drawing.Point(265, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(301, 210);
            this.panel1.TabIndex = 34;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblP2);
            this.groupBox1.Controls.Add(this.lblP1);
            this.groupBox1.Controls.Add(this.lblK3);
            this.groupBox1.Controls.Add(this.lblK2);
            this.groupBox1.Controls.Add(this.lblK1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(237, 108);
            this.groupBox1.TabIndex = 35;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Distortion coefficients";
            // 
            // lblP2
            // 
            this.lblP2.AutoSize = true;
            this.lblP2.Location = new System.Drawing.Point(131, 51);
            this.lblP2.Name = "lblP2";
            this.lblP2.Size = new System.Drawing.Size(25, 13);
            this.lblP2.TabIndex = 4;
            this.lblP2.Text = "p2 :";
            // 
            // lblP1
            // 
            this.lblP1.AutoSize = true;
            this.lblP1.Location = new System.Drawing.Point(131, 28);
            this.lblP1.Name = "lblP1";
            this.lblP1.Size = new System.Drawing.Size(25, 13);
            this.lblP1.TabIndex = 3;
            this.lblP1.Text = "p1 :";
            // 
            // lblK3
            // 
            this.lblK3.AutoSize = true;
            this.lblK3.Location = new System.Drawing.Point(18, 78);
            this.lblK3.Name = "lblK3";
            this.lblK3.Size = new System.Drawing.Size(25, 13);
            this.lblK3.TabIndex = 2;
            this.lblK3.Text = "k3 :";
            // 
            // lblK2
            // 
            this.lblK2.AutoSize = true;
            this.lblK2.Location = new System.Drawing.Point(18, 51);
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
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lblCy);
            this.groupBox2.Controls.Add(this.lblCx);
            this.groupBox2.Controls.Add(this.lblFy);
            this.groupBox2.Controls.Add(this.lblFx);
            this.groupBox2.Location = new System.Drawing.Point(12, 126);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(237, 96);
            this.groupBox2.TabIndex = 36;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Camera intrinsics";
            // 
            // lblCy
            // 
            this.lblCy.AutoSize = true;
            this.lblCy.Location = new System.Drawing.Point(131, 49);
            this.lblCy.Name = "lblCy";
            this.lblCy.Size = new System.Drawing.Size(24, 13);
            this.lblCy.TabIndex = 8;
            this.lblCy.Text = "cy :";
            // 
            // lblCx
            // 
            this.lblCx.AutoSize = true;
            this.lblCx.Location = new System.Drawing.Point(131, 26);
            this.lblCx.Name = "lblCx";
            this.lblCx.Size = new System.Drawing.Size(24, 13);
            this.lblCx.TabIndex = 7;
            this.lblCx.Text = "cx :";
            // 
            // lblFy
            // 
            this.lblFy.AutoSize = true;
            this.lblFy.Location = new System.Drawing.Point(18, 49);
            this.lblFy.Name = "lblFy";
            this.lblFy.Size = new System.Drawing.Size(21, 13);
            this.lblFy.TabIndex = 6;
            this.lblFy.Text = "fy :";
            // 
            // lblFx
            // 
            this.lblFx.AutoSize = true;
            this.lblFx.Location = new System.Drawing.Point(18, 26);
            this.lblFx.Name = "lblFx";
            this.lblFx.Size = new System.Drawing.Size(21, 13);
            this.lblFx.TabIndex = 5;
            this.lblFx.Text = "fx :";
            // 
            // FormCalibrateDistortion
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(579, 271);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormCalibrateDistortion";
            this.Text = "FormCalibrateDistortion";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblP2;
        private System.Windows.Forms.Label lblP1;
        private System.Windows.Forms.Label lblK3;
        private System.Windows.Forms.Label lblK2;
        private System.Windows.Forms.Label lblK1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label lblCy;
        private System.Windows.Forms.Label lblCx;
        private System.Windows.Forms.Label lblFy;
        private System.Windows.Forms.Label lblFx;
    }
}