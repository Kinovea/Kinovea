#region License
/*
Copyright © Joan Charmant 2013.
joan.charmant@gmail.com 
 
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
namespace Kinovea.Camera.Basler
{
    partial class FormConfiguration
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
            this.btnApply = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.gpTriggerOptions = new System.Windows.Forms.GroupBox();
            this.lblRecordingFramerate = new System.Windows.Forms.Label();
            this.tbRecordingFramerate = new System.Windows.Forms.TextBox();
            this.btnSoftwareTrigger = new System.Windows.Forms.Button();
            this.lblTriggerSource = new System.Windows.Forms.Label();
            this.cbTriggerSource = new System.Windows.Forms.ComboBox();
            this.chkTriggerMode = new System.Windows.Forms.CheckBox();
            this.lblResultingFrameRate = new System.Windows.Forms.Label();
            this.tbFramerate = new System.Windows.Forms.TextBox();
            this.lblAcquisitionFramerate = new System.Windows.Forms.Label();
            this.trkGainRaw = new System.Windows.Forms.TrackBar();
            this.tbGainRaw = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbExposureUnit = new System.Windows.Forms.ComboBox();
            this.tbExposureTimeAbs = new System.Windows.Forms.TextBox();
            this.trkExposure = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tbAlias = new System.Windows.Forms.TextBox();
            this.lblSystemName = new System.Windows.Forms.Label();
            this.btnIcon = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.gpTriggerOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkGainRaw)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkExposure)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnApply.Location = new System.Drawing.Point(264, 435);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(99, 24);
            this.btnApply.TabIndex = 78;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(368, 435);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(99, 24);
            this.btnCancel.TabIndex = 79;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancelClick);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.gpTriggerOptions);
            this.groupBox1.Controls.Add(this.chkTriggerMode);
            this.groupBox1.Controls.Add(this.lblResultingFrameRate);
            this.groupBox1.Controls.Add(this.tbFramerate);
            this.groupBox1.Controls.Add(this.lblAcquisitionFramerate);
            this.groupBox1.Controls.Add(this.trkGainRaw);
            this.groupBox1.Controls.Add(this.tbGainRaw);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.cbExposureUnit);
            this.groupBox1.Controls.Add(this.tbExposureTimeAbs);
            this.groupBox1.Controls.Add(this.trkExposure);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 94);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(456, 326);
            this.groupBox1.TabIndex = 84;
            this.groupBox1.TabStop = false;
            // 
            // gpTriggerOptions
            // 
            this.gpTriggerOptions.Controls.Add(this.lblRecordingFramerate);
            this.gpTriggerOptions.Controls.Add(this.tbRecordingFramerate);
            this.gpTriggerOptions.Controls.Add(this.btnSoftwareTrigger);
            this.gpTriggerOptions.Controls.Add(this.lblTriggerSource);
            this.gpTriggerOptions.Controls.Add(this.cbTriggerSource);
            this.gpTriggerOptions.Location = new System.Drawing.Point(16, 200);
            this.gpTriggerOptions.Name = "gpTriggerOptions";
            this.gpTriggerOptions.Size = new System.Drawing.Size(424, 104);
            this.gpTriggerOptions.TabIndex = 95;
            this.gpTriggerOptions.TabStop = false;
            // 
            // lblRecordingFramerate
            // 
            this.lblRecordingFramerate.Location = new System.Drawing.Point(8, 72);
            this.lblRecordingFramerate.Name = "lblRecordingFramerate";
            this.lblRecordingFramerate.Size = new System.Drawing.Size(123, 19);
            this.lblRecordingFramerate.TabIndex = 93;
            this.lblRecordingFramerate.Text = "Recording frame rate :";
            // 
            // tbRecordingFramerate
            // 
            this.tbRecordingFramerate.Enabled = false;
            this.tbRecordingFramerate.Location = new System.Drawing.Point(137, 69);
            this.tbRecordingFramerate.Name = "tbRecordingFramerate";
            this.tbRecordingFramerate.Size = new System.Drawing.Size(50, 20);
            this.tbRecordingFramerate.TabIndex = 94;
            // 
            // btnSoftwareTrigger
            // 
            this.btnSoftwareTrigger.Location = new System.Drawing.Point(240, 24);
            this.btnSoftwareTrigger.Name = "btnSoftwareTrigger";
            this.btnSoftwareTrigger.Size = new System.Drawing.Size(160, 23);
            this.btnSoftwareTrigger.TabIndex = 89;
            this.btnSoftwareTrigger.Text = "Generate a software trigger";
            this.btnSoftwareTrigger.UseVisualStyleBackColor = true;
            this.btnSoftwareTrigger.Click += new System.EventHandler(this.BtnSoftwareTrigger_Click);
            // 
            // lblTriggerSource
            // 
            this.lblTriggerSource.Location = new System.Drawing.Point(10, 26);
            this.lblTriggerSource.Name = "lblTriggerSource";
            this.lblTriggerSource.Size = new System.Drawing.Size(123, 19);
            this.lblTriggerSource.TabIndex = 90;
            this.lblTriggerSource.Text = "Trigger source :";
            // 
            // cbTriggerSource
            // 
            this.cbTriggerSource.FormattingEnabled = true;
            this.cbTriggerSource.Items.AddRange(new object[] {
            "Software",
            "Hardware"});
            this.cbTriggerSource.Location = new System.Drawing.Point(139, 26);
            this.cbTriggerSource.Name = "cbTriggerSource";
            this.cbTriggerSource.Size = new System.Drawing.Size(79, 21);
            this.cbTriggerSource.TabIndex = 91;
            this.cbTriggerSource.Text = "Software";
            this.cbTriggerSource.SelectedIndexChanged += new System.EventHandler(this.CbTriggerSourceSelectedIndexChanged);
            // 
            // chkTriggerMode
            // 
            this.chkTriggerMode.Location = new System.Drawing.Point(24, 168);
            this.chkTriggerMode.Name = "chkTriggerMode";
            this.chkTriggerMode.Size = new System.Drawing.Size(152, 24);
            this.chkTriggerMode.TabIndex = 92;
            this.chkTriggerMode.Text = "Use trigger mode";
            this.chkTriggerMode.UseVisualStyleBackColor = true;
            this.chkTriggerMode.CheckedChanged += new System.EventHandler(this.ChkTriggerModeCheckedChanged);
            // 
            // lblResultingFrameRate
            // 
            this.lblResultingFrameRate.Location = new System.Drawing.Point(275, 120);
            this.lblResultingFrameRate.Name = "lblResultingFrameRate";
            this.lblResultingFrameRate.Size = new System.Drawing.Size(157, 19);
            this.lblResultingFrameRate.TabIndex = 9;
            this.lblResultingFrameRate.Text = "Forced to : 50,226 fps";
            this.lblResultingFrameRate.Visible = false;
            // 
            // tbFramerate
            // 
            this.tbFramerate.Location = new System.Drawing.Point(153, 117);
            this.tbFramerate.Name = "tbFramerate";
            this.tbFramerate.Size = new System.Drawing.Size(50, 20);
            this.tbFramerate.TabIndex = 8;
            this.tbFramerate.TextChanged += new System.EventHandler(this.tbFramerate_TextChanged);
            // 
            // lblAcquisitionFramerate
            // 
            this.lblAcquisitionFramerate.Location = new System.Drawing.Point(24, 120);
            this.lblAcquisitionFramerate.Name = "lblAcquisitionFramerate";
            this.lblAcquisitionFramerate.Size = new System.Drawing.Size(123, 19);
            this.lblAcquisitionFramerate.TabIndex = 7;
            this.lblAcquisitionFramerate.Text = "Acquisition frame rate :";
            // 
            // trkGainRaw
            // 
            this.trkGainRaw.Location = new System.Drawing.Point(278, 75);
            this.trkGainRaw.Maximum = 512;
            this.trkGainRaw.Minimum = 36;
            this.trkGainRaw.Name = "trkGainRaw";
            this.trkGainRaw.Size = new System.Drawing.Size(157, 45);
            this.trkGainRaw.TabIndex = 6;
            this.trkGainRaw.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trkGainRaw.Value = 36;
            this.trkGainRaw.ValueChanged += new System.EventHandler(this.TrkGainRaw_ValueChanged);
            // 
            // tbGainRaw
            // 
            this.tbGainRaw.Location = new System.Drawing.Point(153, 75);
            this.tbGainRaw.Name = "tbGainRaw";
            this.tbGainRaw.Size = new System.Drawing.Size(50, 20);
            this.tbGainRaw.TabIndex = 5;
            this.tbGainRaw.Text = "36";
            this.tbGainRaw.TextChanged += new System.EventHandler(this.TbGainRawTextChanged);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(24, 78);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "Gain :";
            // 
            // cbExposureUnit
            // 
            this.cbExposureUnit.FormattingEnabled = true;
            this.cbExposureUnit.Items.AddRange(new object[] {
            "ms",
            "µs"});
            this.cbExposureUnit.Location = new System.Drawing.Point(189, 30);
            this.cbExposureUnit.Name = "cbExposureUnit";
            this.cbExposureUnit.Size = new System.Drawing.Size(43, 21);
            this.cbExposureUnit.TabIndex = 3;
            this.cbExposureUnit.Text = "ms";
            this.cbExposureUnit.SelectedIndexChanged += new System.EventHandler(this.CbExposureUnitSelectedIndexChanged);
            // 
            // tbExposureTimeAbs
            // 
            this.tbExposureTimeAbs.Location = new System.Drawing.Point(153, 31);
            this.tbExposureTimeAbs.Name = "tbExposureTimeAbs";
            this.tbExposureTimeAbs.Size = new System.Drawing.Size(30, 20);
            this.tbExposureTimeAbs.TabIndex = 2;
            this.tbExposureTimeAbs.Text = "10";
            this.tbExposureTimeAbs.TextChanged += new System.EventHandler(this.TbExposureTimeAbs_TextChanged);
            // 
            // trkExposure
            // 
            this.trkExposure.Location = new System.Drawing.Point(278, 31);
            this.trkExposure.Maximum = 1000;
            this.trkExposure.Minimum = 1;
            this.trkExposure.Name = "trkExposure";
            this.trkExposure.Size = new System.Drawing.Size(157, 45);
            this.trkExposure.TabIndex = 1;
            this.trkExposure.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trkExposure.Value = 1;
            this.trkExposure.ValueChanged += new System.EventHandler(this.TrkExposureTimeAbs_ValueChanged);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(24, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Exposure Time :";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.tbAlias);
            this.groupBox2.Controls.Add(this.lblSystemName);
            this.groupBox2.Controls.Add(this.btnIcon);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(457, 76);
            this.groupBox2.TabIndex = 88;
            this.groupBox2.TabStop = false;
            // 
            // tbAlias
            // 
            this.tbAlias.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbAlias.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbAlias.ForeColor = System.Drawing.Color.CornflowerBlue;
            this.tbAlias.Location = new System.Drawing.Point(73, 22);
            this.tbAlias.Name = "tbAlias";
            this.tbAlias.Size = new System.Drawing.Size(223, 15);
            this.tbAlias.TabIndex = 86;
            this.tbAlias.Text = "Alias";
            // 
            // lblSystemName
            // 
            this.lblSystemName.AutoSize = true;
            this.lblSystemName.BackColor = System.Drawing.Color.Transparent;
            this.lblSystemName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSystemName.ForeColor = System.Drawing.Color.Black;
            this.lblSystemName.Location = new System.Drawing.Point(68, 45);
            this.lblSystemName.Name = "lblSystemName";
            this.lblSystemName.Size = new System.Drawing.Size(70, 13);
            this.lblSystemName.TabIndex = 85;
            this.lblSystemName.Text = "System name";
            this.lblSystemName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnIcon
            // 
            this.btnIcon.BackColor = System.Drawing.Color.Transparent;
            this.btnIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnIcon.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnIcon.FlatAppearance.BorderSize = 0;
            this.btnIcon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIcon.Location = new System.Drawing.Point(24, 26);
            this.btnIcon.Name = "btnIcon";
            this.btnIcon.Size = new System.Drawing.Size(16, 16);
            this.btnIcon.TabIndex = 83;
            this.btnIcon.UseVisualStyleBackColor = false;
            this.btnIcon.Click += new System.EventHandler(this.BtnIconClick);
            // 
            // FormConfiguration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(480, 471);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormConfiguration";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FormConfiguration";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.gpTriggerOptions.ResumeLayout(false);
            this.gpTriggerOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkGainRaw)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkExposure)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }
        private System.Windows.Forms.GroupBox gpTriggerOptions;
        private System.Windows.Forms.Label lblRecordingFramerate;
        private System.Windows.Forms.TextBox tbRecordingFramerate;
        private System.Windows.Forms.CheckBox chkTriggerMode;
        private System.Windows.Forms.Button btnSoftwareTrigger;
        private System.Windows.Forms.Label lblTriggerSource;
        private System.Windows.Forms.ComboBox cbTriggerSource;
        private System.Windows.Forms.Label lblResultingFrameRate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar trkExposure;
        private System.Windows.Forms.TextBox tbExposureTimeAbs;
        private System.Windows.Forms.ComboBox cbExposureUnit;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbGainRaw;
        private System.Windows.Forms.TrackBar trkGainRaw;
        private System.Windows.Forms.Label lblAcquisitionFramerate;
        private System.Windows.Forms.TextBox tbFramerate;
        private System.Windows.Forms.Button btnIcon;
        private System.Windows.Forms.Label lblSystemName;
        private System.Windows.Forms.TextBox tbAlias;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnApply;
    }
}
