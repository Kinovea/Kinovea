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
    partial class FormCrashRecovery
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
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.lblInfo = new System.Windows.Forms.Label();
      this.lvRecoverables = new System.Windows.Forms.ListView();
      this.colFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.colDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.SuspendLayout();
      // 
      // btnOk
      // 
      this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOk.Location = new System.Drawing.Point(141, 151);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(99, 24);
      this.btnOk.TabIndex = 16;
      this.btnOk.Text = "Recover";
      this.btnOk.UseVisualStyleBackColor = true;
      this.btnOk.Click += new System.EventHandler(this.BtnOKClick);
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(246, 151);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(99, 24);
      this.btnCancel.TabIndex = 17;
      this.btnCancel.Text = "Annuler";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // lblInfo
      // 
      this.lblInfo.Location = new System.Drawing.Point(13, 9);
      this.lblInfo.Name = "lblInfo";
      this.lblInfo.Size = new System.Drawing.Size(332, 35);
      this.lblInfo.TabIndex = 18;
      this.lblInfo.Text = "info";
      // 
      // lvRecoverables
      // 
      this.lvRecoverables.AutoArrange = false;
      this.lvRecoverables.BackColor = System.Drawing.Color.White;
      this.lvRecoverables.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.lvRecoverables.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colFile,
            this.colDate});
      this.lvRecoverables.GridLines = true;
      this.lvRecoverables.HideSelection = false;
      this.lvRecoverables.Location = new System.Drawing.Point(13, 47);
      this.lvRecoverables.Name = "lvRecoverables";
      this.lvRecoverables.Size = new System.Drawing.Size(333, 94);
      this.lvRecoverables.TabIndex = 0;
      this.lvRecoverables.UseCompatibleStateImageBehavior = false;
      this.lvRecoverables.View = System.Windows.Forms.View.Details;
      // 
      // colFile
      // 
      this.colFile.DisplayIndex = 1;
      this.colFile.Text = "File";
      this.colFile.Width = 196;
      // 
      // colDate
      // 
      this.colDate.DisplayIndex = 0;
      this.colDate.Text = "Date";
      this.colDate.Width = 132;
      // 
      // FormCrashRecovery
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(357, 187);
      this.Controls.Add(this.lblInfo);
      this.Controls.Add(this.btnOk);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.lvRecoverables);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormCrashRecovery";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.Text = "FormCrashRecovery";
      this.ResumeLayout(false);

        }
        private System.Windows.Forms.ListView lvRecoverables;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.ColumnHeader colDate;
        private System.Windows.Forms.ColumnHeader colFile;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Button btnCancel;
    }
}
