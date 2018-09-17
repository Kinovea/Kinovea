#region License
/*
Copyright © Joan Charmant 2010.
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
namespace Kinovea.Updater
{
	partial class UpdateDialog2
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
            this.btnSoftware = new System.Windows.Forms.Button();
            this.labelInfos = new System.Windows.Forms.Label();
            this.lblNewVersion = new System.Windows.Forms.Label();
            this.lblNewVersionFileSize = new System.Windows.Forms.Label();
            this.lblChangeLog = new System.Windows.Forms.Label();
            this.rtbxChangeLog = new System.Windows.Forms.RichTextBox();
            this.lnkKinovea = new System.Windows.Forms.LinkLabel();
            this.btnDownload = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.bgwkrDownloader = new System.ComponentModel.BackgroundWorker();
            this.progressDownload = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // btnSoftware
            // 
            this.btnSoftware.FlatAppearance.BorderSize = 0;
            this.btnSoftware.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnSoftware.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btnSoftware.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSoftware.Image = global::Kinovea.Updater.Properties.Resources.Install;
            this.btnSoftware.Location = new System.Drawing.Point(12, 12);
            this.btnSoftware.Name = "btnSoftware";
            this.btnSoftware.Size = new System.Drawing.Size(80, 74);
            this.btnSoftware.TabIndex = 14;
            this.btnSoftware.UseVisualStyleBackColor = true;
            // 
            // labelInfos
            // 
            this.labelInfos.AutoSize = true;
            this.labelInfos.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelInfos.Location = new System.Drawing.Point(109, 41);
            this.labelInfos.Name = "labelInfos";
            this.labelInfos.Size = new System.Drawing.Size(197, 16);
            this.labelInfos.TabIndex = 15;
            this.labelInfos.Text = "A new version is available !";
            // 
            // lblNewVersion
            // 
            this.lblNewVersion.AutoSize = true;
            this.lblNewVersion.Location = new System.Drawing.Point(12, 99);
            this.lblNewVersion.Name = "lblNewVersion";
            this.lblNewVersion.Size = new System.Drawing.Size(188, 13);
            this.lblNewVersion.TabIndex = 16;
            this.lblNewVersion.Text = "New Version : 0.6.4 - ( Current : 0.6.2 )";
            // 
            // lblNewVersionFileSize
            // 
            this.lblNewVersionFileSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNewVersionFileSize.Location = new System.Drawing.Point(298, 99);
            this.lblNewVersionFileSize.Name = "lblNewVersionFileSize";
            this.lblNewVersionFileSize.Size = new System.Drawing.Size(178, 13);
            this.lblNewVersionFileSize.TabIndex = 17;
            this.lblNewVersionFileSize.Text = "File Size : 5.4 MB";
            this.lblNewVersionFileSize.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblChangeLog
            // 
            this.lblChangeLog.AutoSize = true;
            this.lblChangeLog.Location = new System.Drawing.Point(12, 126);
            this.lblChangeLog.Name = "lblChangeLog";
            this.lblChangeLog.Size = new System.Drawing.Size(71, 13);
            this.lblChangeLog.TabIndex = 18;
            this.lblChangeLog.Text = "Change Log :";
            // 
            // rtbxChangeLog
            // 
            this.rtbxChangeLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbxChangeLog.BackColor = System.Drawing.Color.Gainsboro;
            this.rtbxChangeLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbxChangeLog.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbxChangeLog.Location = new System.Drawing.Point(12, 154);
            this.rtbxChangeLog.Name = "rtbxChangeLog";
            this.rtbxChangeLog.Size = new System.Drawing.Size(464, 208);
            this.rtbxChangeLog.TabIndex = 19;
            this.rtbxChangeLog.Text = "The quick brown fox jumps over the lazy dog";
            // 
            // lnkKinovea
            // 
            this.lnkKinovea.ActiveLinkColor = System.Drawing.Color.GreenYellow;
            this.lnkKinovea.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lnkKinovea.AutoSize = true;
            this.lnkKinovea.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lnkKinovea.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkKinovea.LinkColor = System.Drawing.Color.DarkGreen;
            this.lnkKinovea.Location = new System.Drawing.Point(12, 375);
            this.lnkKinovea.Name = "lnkKinovea";
            this.lnkKinovea.Size = new System.Drawing.Size(81, 12);
            this.lnkKinovea.TabIndex = 20;
            this.lnkKinovea.TabStop = true;
            this.lnkKinovea.Text = "www.kinovea.org";
            this.lnkKinovea.VisitedLinkColor = System.Drawing.Color.DarkGreen;
            // 
            // btnDownload
            // 
            this.btnDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDownload.Location = new System.Drawing.Point(271, 368);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(99, 24);
            this.btnDownload.TabIndex = 41;
            this.btnDownload.Text = "Download";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(377, 368);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(99, 24);
            this.btnCancel.TabIndex = 42;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // progressDownload
            // 
            this.progressDownload.Location = new System.Drawing.Point(312, 41);
            this.progressDownload.Name = "progressDownload";
            this.progressDownload.Size = new System.Drawing.Size(164, 26);
            this.progressDownload.TabIndex = 43;
            this.progressDownload.Visible = false;
            // 
            // UpdateDialog2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(488, 404);
            this.Controls.Add(this.progressDownload);
            this.Controls.Add(this.btnDownload);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lnkKinovea);
            this.Controls.Add(this.rtbxChangeLog);
            this.Controls.Add(this.lblChangeLog);
            this.Controls.Add(this.lblNewVersionFileSize);
            this.Controls.Add(this.lblNewVersion);
            this.Controls.Add(this.labelInfos);
            this.Controls.Add(this.btnSoftware);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdateDialog2";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "UpdateDialog2";
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		private System.Windows.Forms.ProgressBar progressDownload;
		private System.ComponentModel.BackgroundWorker bgwkrDownloader;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnDownload;
		private System.Windows.Forms.LinkLabel lnkKinovea;
		private System.Windows.Forms.RichTextBox rtbxChangeLog;
		private System.Windows.Forms.Label lblChangeLog;
		private System.Windows.Forms.Label lblNewVersionFileSize;
		private System.Windows.Forms.Label lblNewVersion;
		private System.Windows.Forms.Label labelInfos;
		private System.Windows.Forms.Button btnSoftware;
	}
}
