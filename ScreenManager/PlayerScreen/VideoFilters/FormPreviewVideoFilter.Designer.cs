#region License
/*
Copyright © Joan Charmant 2008-2009.
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
namespace Videa.ScreenManager
{
    partial class formPreviewVideoFilter
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
        	this.pnlPreview = new System.Windows.Forms.Panel();
        	this.picPreview = new System.Windows.Forms.PictureBox();
        	this.pnlPreview.SuspendLayout();
        	((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
        	this.SuspendLayout();
        	// 
        	// btnOK
        	// 
        	this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
        	this.btnOK.Location = new System.Drawing.Point(176, 295);
        	this.btnOK.Name = "btnOK";
        	this.btnOK.Size = new System.Drawing.Size(99, 24);
        	this.btnOK.TabIndex = 10;
        	this.btnOK.Text = "OK";
        	this.btnOK.UseVisualStyleBackColor = true;
        	// 
        	// btnCancel
        	// 
        	this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        	this.btnCancel.Location = new System.Drawing.Point(281, 295);
        	this.btnCancel.Name = "btnCancel";
        	this.btnCancel.Size = new System.Drawing.Size(99, 24);
        	this.btnCancel.TabIndex = 15;
        	this.btnCancel.Text = "Cancel";
        	this.btnCancel.UseVisualStyleBackColor = true;
        	// 
        	// pnlPreview
        	// 
        	this.pnlPreview.BackColor = System.Drawing.Color.Black;
        	this.pnlPreview.Controls.Add(this.picPreview);
        	this.pnlPreview.Location = new System.Drawing.Point(15, 15);
        	this.pnlPreview.Name = "pnlPreview";
        	this.pnlPreview.Size = new System.Drawing.Size(360, 270);
        	this.pnlPreview.TabIndex = 16;
        	// 
        	// picPreview
        	// 
        	this.picPreview.Location = new System.Drawing.Point(51, 35);
        	this.picPreview.Name = "picPreview";
        	this.picPreview.Size = new System.Drawing.Size(250, 193);
        	this.picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
        	this.picPreview.TabIndex = 0;
        	this.picPreview.TabStop = false;
        	this.picPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.picPreview_Paint);
        	// 
        	// formPreviewVideoFilter
        	// 
        	this.AcceptButton = this.btnOK;
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.CancelButton = this.btnCancel;
        	this.ClientSize = new System.Drawing.Size(389, 334);
        	this.Controls.Add(this.btnOK);
        	this.Controls.Add(this.btnCancel);
        	this.Controls.Add(this.pnlPreview);
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
        	this.MaximizeBox = false;
        	this.MinimizeBox = false;
        	this.Name = "formPreviewVideoFilter";
        	this.ShowIcon = false;
        	this.ShowInTaskbar = false;
        	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        	this.Text = "   Colors";
        	this.Load += new System.EventHandler(this.formFilterTuner_Load);
        	this.pnlPreview.ResumeLayout(false);
        	((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
        	this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Panel pnlPreview;
        private System.Windows.Forms.PictureBox picPreview;
    }
}