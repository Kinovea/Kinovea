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
    partial class DropDownMenuContainer
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
        	this.panel1 = new System.Windows.Forms.Panel();
        	this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
        	this.SuspendLayout();
        	// 
        	// panel1
        	// 
        	this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        	this.panel1.BackColor = System.Drawing.Color.Transparent;
        	this.panel1.Location = new System.Drawing.Point(12, 12);
        	this.panel1.Name = "panel1";
        	this.panel1.Size = new System.Drawing.Size(33, 22);
        	this.panel1.TabIndex = 1;
        	// 
        	// contextMenuStrip
        	// 
        	this.contextMenuStrip.DropShadowEnabled = false;
        	this.contextMenuStrip.Name = "contextMenuStrip1";
        	this.contextMenuStrip.Size = new System.Drawing.Size(61, 4);
        	// 
        	// DropDownForm
        	// 
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        	this.BackColor = System.Drawing.Color.DarkGray;
        	this.ClientSize = new System.Drawing.Size(57, 46);
        	this.ControlBox = false;
        	this.Controls.Add(this.panel1);
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        	this.Name = "DropDownForm";
        	this.ShowIcon = false;
        	this.ShowInTaskbar = false;
        	this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
        	this.Text = "DropDownForm";
        	this.Deactivate += new System.EventHandler(this.DropDownFormDeactivate);
        	this.Leave += new System.EventHandler(this.DropDownFormLeave);
        	this.ResumeLayout(false);
        }
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
    }
}
