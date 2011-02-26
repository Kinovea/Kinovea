#region License
/*
Copyright © Joan Charmant 2011.
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
	partial class PreferencePanelCapture
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
			this.tabSubPages = new System.Windows.Forms.TabControl();
			this.tabGeneral = new System.Windows.Forms.TabPage();
			this.tabNaming = new System.Windows.Forms.TabPage();
			this.tabSubPages.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabSubPages
			// 
			this.tabSubPages.Controls.Add(this.tabGeneral);
			this.tabSubPages.Controls.Add(this.tabNaming);
			this.tabSubPages.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabSubPages.Location = new System.Drawing.Point(0, 0);
			this.tabSubPages.Name = "tabSubPages";
			this.tabSubPages.SelectedIndex = 0;
			this.tabSubPages.Size = new System.Drawing.Size(432, 236);
			this.tabSubPages.TabIndex = 0;
			// 
			// tabGeneral
			// 
			this.tabGeneral.Location = new System.Drawing.Point(4, 22);
			this.tabGeneral.Name = "tabGeneral";
			this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
			this.tabGeneral.Size = new System.Drawing.Size(424, 210);
			this.tabGeneral.TabIndex = 0;
			this.tabGeneral.Text = "General";
			this.tabGeneral.UseVisualStyleBackColor = true;
			// 
			// tabNaming
			// 
			this.tabNaming.Location = new System.Drawing.Point(4, 22);
			this.tabNaming.Name = "tabNaming";
			this.tabNaming.Padding = new System.Windows.Forms.Padding(3);
			this.tabNaming.Size = new System.Drawing.Size(424, 210);
			this.tabNaming.TabIndex = 1;
			this.tabNaming.Text = "File naming";
			this.tabNaming.UseVisualStyleBackColor = true;
			// 
			// PreferencePanelCapture
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tabSubPages);
			this.Name = "PreferencePanelCapture";
			this.Size = new System.Drawing.Size(432, 236);
			this.tabSubPages.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.TabControl tabSubPages;
		private System.Windows.Forms.TabPage tabGeneral;
		private System.Windows.Forms.TabPage tabNaming;
	}
}
