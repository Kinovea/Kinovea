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
namespace Kinovea.Camera.DirectShow
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
            this.btnDeviceProperties = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblConfig = new System.Windows.Forms.Label();
            this.cmbCapabilities = new System.Windows.Forms.ComboBox();
            this.btnIcon = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tbAlias = new System.Windows.Forms.TextBox();
            this.lblSystemName = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnDeviceProperties
            // 
            this.btnDeviceProperties.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
                                    | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeviceProperties.Location = new System.Drawing.Point(123, 132);
            this.btnDeviceProperties.Name = "btnDeviceProperties";
            this.btnDeviceProperties.Size = new System.Drawing.Size(173, 24);
            this.btnDeviceProperties.TabIndex = 11;
            this.btnDeviceProperties.Text = "Device Properties";
            this.btnDeviceProperties.UseVisualStyleBackColor = true;
            this.btnDeviceProperties.Click += new System.EventHandler(this.BtnDevicePropertiesClick);
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnApply.Location = new System.Drawing.Point(119, 207);
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
            this.btnCancel.Location = new System.Drawing.Point(224, 207);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(99, 24);
            this.btnCancel.TabIndex = 79;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // lblConfig
            // 
            this.lblConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
                                    | System.Windows.Forms.AnchorStyles.Right)));
            this.lblConfig.BackColor = System.Drawing.Color.Transparent;
            this.lblConfig.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblConfig.ForeColor = System.Drawing.Color.Black;
            this.lblConfig.Location = new System.Drawing.Point(24, 87);
            this.lblConfig.Name = "lblConfig";
            this.lblConfig.Size = new System.Drawing.Size(93, 23);
            this.lblConfig.TabIndex = 80;
            this.lblConfig.Text = "Configuration :";
            this.lblConfig.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmbCapabilities
            // 
            this.cmbCapabilities.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
                                    | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbCapabilities.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCapabilities.FormattingEnabled = true;
            this.cmbCapabilities.Location = new System.Drawing.Point(123, 89);
            this.cmbCapabilities.Name = "cmbCapabilities";
            this.cmbCapabilities.Size = new System.Drawing.Size(173, 21);
            this.cmbCapabilities.TabIndex = 81;
            this.cmbCapabilities.SelectedIndexChanged += new System.EventHandler(this.CmbCapabilitiesSelectedIndexChanged);
            // 
            // btnIcon
            // 
            this.btnIcon.BackColor = System.Drawing.Color.Transparent;
            this.btnIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnIcon.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnIcon.FlatAppearance.BorderSize = 0;
            this.btnIcon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIcon.Location = new System.Drawing.Point(49, 29);
            this.btnIcon.Name = "btnIcon";
            this.btnIcon.Size = new System.Drawing.Size(16, 16);
            this.btnIcon.TabIndex = 83;
            this.btnIcon.UseVisualStyleBackColor = false;
            this.btnIcon.Click += new System.EventHandler(this.BtnIconClick);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                                    | System.Windows.Forms.AnchorStyles.Left) 
                                    | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.tbAlias);
            this.groupBox1.Controls.Add(this.lblSystemName);
            this.groupBox1.Controls.Add(this.btnIcon);
            this.groupBox1.Controls.Add(this.cmbCapabilities);
            this.groupBox1.Controls.Add(this.lblConfig);
            this.groupBox1.Controls.Add(this.btnDeviceProperties);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(311, 180);
            this.groupBox1.TabIndex = 84;
            this.groupBox1.TabStop = false;
            // 
            // tbAlias
            // 
            this.tbAlias.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbAlias.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbAlias.ForeColor = System.Drawing.Color.CornflowerBlue;
            this.tbAlias.Location = new System.Drawing.Point(127, 26);
            this.tbAlias.Name = "tbAlias";
            this.tbAlias.Size = new System.Drawing.Size(173, 15);
            this.tbAlias.TabIndex = 86;
            this.tbAlias.Text = "Alias";
            // 
            // lblSystemName
            // 
            this.lblSystemName.AutoSize = true;
            this.lblSystemName.BackColor = System.Drawing.Color.Transparent;
            this.lblSystemName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSystemName.ForeColor = System.Drawing.Color.Black;
            this.lblSystemName.Location = new System.Drawing.Point(122, 45);
            this.lblSystemName.Name = "lblSystemName";
            this.lblSystemName.Size = new System.Drawing.Size(70, 13);
            this.lblSystemName.TabIndex = 85;
            this.lblSystemName.Text = "System name";
            this.lblSystemName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FormConfiguration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(335, 243);
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
            this.ResumeLayout(false);
        }
        private System.Windows.Forms.TextBox tbAlias;
        private System.Windows.Forms.Label lblSystemName;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnIcon;
        private System.Windows.Forms.ComboBox cmbCapabilities;
        private System.Windows.Forms.Label lblConfig;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnDeviceProperties;
    }
}
