#region License
/*
Copyright © Joan Charmant 2013.
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
    partial class SizeSelector
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
            this.btnLarge = new System.Windows.Forms.Button();
            this.btnMedium = new System.Windows.Forms.Button();
            this.btnSmall = new System.Windows.Forms.Button();
            this.btnExtraLarge = new System.Windows.Forms.Button();
            this.btnExtraSmall = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnLarge
            // 
            this.btnLarge.BackColor = System.Drawing.Color.LightSteelBlue;
            this.btnLarge.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnLarge.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLarge.FlatAppearance.BorderSize = 0;
            this.btnLarge.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightSteelBlue;
            this.btnLarge.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
            this.btnLarge.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLarge.Location = new System.Drawing.Point(59, 8);
            this.btnLarge.Name = "btnLarge";
            this.btnLarge.Size = new System.Drawing.Size(20, 15);
            this.btnLarge.TabIndex = 22;
            this.btnLarge.UseVisualStyleBackColor = false;
            // 
            // btnMedium
            // 
            this.btnMedium.BackColor = System.Drawing.Color.SteelBlue;
            this.btnMedium.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnMedium.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMedium.FlatAppearance.BorderSize = 0;
            this.btnMedium.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightSteelBlue;
            this.btnMedium.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
            this.btnMedium.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMedium.Location = new System.Drawing.Point(37, 11);
            this.btnMedium.Name = "btnMedium";
            this.btnMedium.Size = new System.Drawing.Size(16, 12);
            this.btnMedium.TabIndex = 21;
            this.btnMedium.UseVisualStyleBackColor = false;
            // 
            // btnSmall
            // 
            this.btnSmall.BackColor = System.Drawing.Color.SteelBlue;
            this.btnSmall.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnSmall.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSmall.FlatAppearance.BorderSize = 0;
            this.btnSmall.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightSteelBlue;
            this.btnSmall.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
            this.btnSmall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSmall.Location = new System.Drawing.Point(19, 14);
            this.btnSmall.Name = "btnSmall";
            this.btnSmall.Size = new System.Drawing.Size(12, 9);
            this.btnSmall.TabIndex = 20;
            this.btnSmall.UseVisualStyleBackColor = false;
            // 
            // btnExtraLarge
            // 
            this.btnExtraLarge.BackColor = System.Drawing.Color.SteelBlue;
            this.btnExtraLarge.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnExtraLarge.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExtraLarge.FlatAppearance.BorderSize = 0;
            this.btnExtraLarge.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightSteelBlue;
            this.btnExtraLarge.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
            this.btnExtraLarge.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExtraLarge.Location = new System.Drawing.Point(85, 5);
            this.btnExtraLarge.Name = "btnExtraLarge";
            this.btnExtraLarge.Size = new System.Drawing.Size(24, 18);
            this.btnExtraLarge.TabIndex = 19;
            this.btnExtraLarge.UseVisualStyleBackColor = false;
            // 
            // btnExtraSmall
            // 
            this.btnExtraSmall.BackColor = System.Drawing.Color.SteelBlue;
            this.btnExtraSmall.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnExtraSmall.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExtraSmall.FlatAppearance.BorderSize = 0;
            this.btnExtraSmall.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightSteelBlue;
            this.btnExtraSmall.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
            this.btnExtraSmall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExtraSmall.Location = new System.Drawing.Point(5, 17);
            this.btnExtraSmall.Name = "btnExtraSmall";
            this.btnExtraSmall.Size = new System.Drawing.Size(8, 6);
            this.btnExtraSmall.TabIndex = 18;
            this.btnExtraSmall.UseVisualStyleBackColor = false;
            // 
            // SizeSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.btnLarge);
            this.Controls.Add(this.btnMedium);
            this.Controls.Add(this.btnSmall);
            this.Controls.Add(this.btnExtraLarge);
            this.Controls.Add(this.btnExtraSmall);
            this.Name = "SizeSelector";
            this.Size = new System.Drawing.Size(115, 29);
            this.ResumeLayout(false);
        }
        private System.Windows.Forms.Button btnExtraSmall;
        private System.Windows.Forms.Button btnExtraLarge;
        private System.Windows.Forms.Button btnSmall;
        private System.Windows.Forms.Button btnMedium;
        private System.Windows.Forms.Button btnLarge;
    }
}
