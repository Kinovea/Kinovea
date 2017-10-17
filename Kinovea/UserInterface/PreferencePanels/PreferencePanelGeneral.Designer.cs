#region License
/*
Copyright � Joan Charmant 2011.
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
namespace Kinovea.Root
{
	partial class PreferencePanelGeneral
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the control.
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
            this.cmbHistoryCount = new System.Windows.Forms.ComboBox();
            this.lblLanguage = new System.Windows.Forms.Label();
            this.lblHistoryCount = new System.Windows.Forms.Label();
            this.cmbLanguage = new System.Windows.Forms.ComboBox();
            this.chkAllowMultipleInstances = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // cmbHistoryCount
            // 
            this.cmbHistoryCount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbHistoryCount.FormattingEnabled = true;
            this.cmbHistoryCount.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.cmbHistoryCount.Location = new System.Drawing.Point(370, 83);
            this.cmbHistoryCount.Name = "cmbHistoryCount";
            this.cmbHistoryCount.Size = new System.Drawing.Size(36, 21);
            this.cmbHistoryCount.TabIndex = 13;
            this.cmbHistoryCount.SelectedIndexChanged += new System.EventHandler(this.cmbHistoryCount_SelectedIndexChanged);
            // 
            // lblLanguage
            // 
            this.lblLanguage.AutoSize = true;
            this.lblLanguage.Location = new System.Drawing.Point(29, 47);
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Size = new System.Drawing.Size(61, 13);
            this.lblLanguage.TabIndex = 12;
            this.lblLanguage.Text = "Language :";
            // 
            // lblHistoryCount
            // 
            this.lblHistoryCount.AutoSize = true;
            this.lblHistoryCount.Location = new System.Drawing.Point(29, 86);
            this.lblHistoryCount.Name = "lblHistoryCount";
            this.lblHistoryCount.Size = new System.Drawing.Size(160, 13);
            this.lblHistoryCount.TabIndex = 14;
            this.lblHistoryCount.Text = "Number of files in recent history :";
            // 
            // cmbLanguage
            // 
            this.cmbLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLanguage.FormattingEnabled = true;
            this.cmbLanguage.Items.AddRange(new object[] {
            "English",
            "Fran�ais"});
            this.cmbLanguage.Location = new System.Drawing.Point(302, 47);
            this.cmbLanguage.Name = "cmbLanguage";
            this.cmbLanguage.Size = new System.Drawing.Size(104, 21);
            this.cmbLanguage.TabIndex = 11;
            this.cmbLanguage.SelectedIndexChanged += new System.EventHandler(this.cmbLanguage_SelectedIndexChanged);
            // 
            // chkAllowMultipleInstances
            // 
            this.chkAllowMultipleInstances.AutoSize = true;
            this.chkAllowMultipleInstances.Location = new System.Drawing.Point(32, 126);
            this.chkAllowMultipleInstances.Name = "chkAllowMultipleInstances";
            this.chkAllowMultipleInstances.Size = new System.Drawing.Size(191, 17);
            this.chkAllowMultipleInstances.TabIndex = 53;
            this.chkAllowMultipleInstances.Text = "Allow multiple instances of Kinovea";
            this.chkAllowMultipleInstances.UseVisualStyleBackColor = true;
            this.chkAllowMultipleInstances.CheckedChanged += new System.EventHandler(this.chkAllowMultipleInstances_CheckedChanged);
            // 
            // PreferencePanelGeneral
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gainsboro;
            this.Controls.Add(this.chkAllowMultipleInstances);
            this.Controls.Add(this.cmbHistoryCount);
            this.Controls.Add(this.lblLanguage);
            this.Controls.Add(this.lblHistoryCount);
            this.Controls.Add(this.cmbLanguage);
            this.Name = "PreferencePanelGeneral";
            this.Size = new System.Drawing.Size(432, 236);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		private System.Windows.Forms.ComboBox cmbLanguage;
		private System.Windows.Forms.Label lblHistoryCount;
		private System.Windows.Forms.Label lblLanguage;
		private System.Windows.Forms.ComboBox cmbHistoryCount;
        private System.Windows.Forms.CheckBox chkAllowMultipleInstances;
    }
}
