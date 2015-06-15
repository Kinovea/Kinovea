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
	partial class PreferencePanelDrawings
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
            this.chkDrawOnPlay = new System.Windows.Forms.CheckBox();
            this.tabPersistence = new System.Windows.Forms.TabPage();
            this.chkAlwaysVisible = new System.Windows.Forms.CheckBox();
            this.lblFading = new System.Windows.Forms.Label();
            this.trkFading = new System.Windows.Forms.TrackBar();
            this.chkEnablePersistence = new System.Windows.Forms.CheckBox();
            this.tabTracking = new System.Windows.Forms.TabPage();
            this.tbSearchHeight = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbSearchWidth = new System.Windows.Forms.TextBox();
            this.tbBlockHeight = new System.Windows.Forms.TextBox();
            this.lblSearchWindow = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tbBlockWidth = new System.Windows.Forms.TextBox();
            this.lblObjectWindow = new System.Windows.Forms.Label();
            this.cmbBlockWindowUnit = new System.Windows.Forms.ComboBox();
            this.cmbSearchWindowUnit = new System.Windows.Forms.ComboBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.tabSubPages.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.tabPersistence.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkFading)).BeginInit();
            this.tabTracking.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabSubPages
            // 
            this.tabSubPages.Controls.Add(this.tabGeneral);
            this.tabSubPages.Controls.Add(this.tabPersistence);
            this.tabSubPages.Controls.Add(this.tabTracking);
            this.tabSubPages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabSubPages.Location = new System.Drawing.Point(0, 0);
            this.tabSubPages.Name = "tabSubPages";
            this.tabSubPages.SelectedIndex = 0;
            this.tabSubPages.Size = new System.Drawing.Size(432, 236);
            this.tabSubPages.TabIndex = 0;
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.chkDrawOnPlay);
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneral.Size = new System.Drawing.Size(424, 210);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // chkDrawOnPlay
            // 
            this.chkDrawOnPlay.AutoSize = true;
            this.chkDrawOnPlay.Location = new System.Drawing.Point(17, 27);
            this.chkDrawOnPlay.Name = "chkDrawOnPlay";
            this.chkDrawOnPlay.Size = new System.Drawing.Size(202, 17);
            this.chkDrawOnPlay.TabIndex = 52;
            this.chkDrawOnPlay.Text = "Show drawings when video is playing";
            this.chkDrawOnPlay.UseVisualStyleBackColor = true;
            this.chkDrawOnPlay.CheckedChanged += new System.EventHandler(this.chkDrawOnPlay_CheckedChanged);
            // 
            // tabPersistence
            // 
            this.tabPersistence.Controls.Add(this.chkAlwaysVisible);
            this.tabPersistence.Controls.Add(this.lblFading);
            this.tabPersistence.Controls.Add(this.trkFading);
            this.tabPersistence.Controls.Add(this.chkEnablePersistence);
            this.tabPersistence.Location = new System.Drawing.Point(4, 22);
            this.tabPersistence.Name = "tabPersistence";
            this.tabPersistence.Padding = new System.Windows.Forms.Padding(3);
            this.tabPersistence.Size = new System.Drawing.Size(424, 210);
            this.tabPersistence.TabIndex = 1;
            this.tabPersistence.Text = "Persistence";
            this.tabPersistence.UseVisualStyleBackColor = true;
            // 
            // chkAlwaysVisible
            // 
            this.chkAlwaysVisible.AutoSize = true;
            this.chkAlwaysVisible.Location = new System.Drawing.Point(19, 137);
            this.chkAlwaysVisible.Name = "chkAlwaysVisible";
            this.chkAlwaysVisible.Size = new System.Drawing.Size(91, 17);
            this.chkAlwaysVisible.TabIndex = 55;
            this.chkAlwaysVisible.Text = "Always visible";
            this.chkAlwaysVisible.UseVisualStyleBackColor = true;
            this.chkAlwaysVisible.CheckedChanged += new System.EventHandler(this.chkAlwaysVisible_CheckedChanged);
            // 
            // lblFading
            // 
            this.lblFading.Location = new System.Drawing.Point(15, 39);
            this.lblFading.Name = "lblFading";
            this.lblFading.Size = new System.Drawing.Size(362, 32);
            this.lblFading.TabIndex = 52;
            this.lblFading.Text = "By default, drawings will stay visible for 12 images around the Key Image. kdjfns" +
                "kdjbnsdkjbksdjvbdvkbj";
            this.lblFading.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // trkFading
            // 
            this.trkFading.BackColor = System.Drawing.Color.White;
            this.trkFading.Location = new System.Drawing.Point(19, 77);
            this.trkFading.Maximum = 200;
            this.trkFading.Minimum = 1;
            this.trkFading.Name = "trkFading";
            this.trkFading.Size = new System.Drawing.Size(359, 45);
            this.trkFading.TabIndex = 54;
            this.trkFading.TickFrequency = 5;
            this.trkFading.Value = 5;
            this.trkFading.ValueChanged += new System.EventHandler(this.trkFading_ValueChanged);
            // 
            // chkEnablePersistence
            // 
            this.chkEnablePersistence.AutoSize = true;
            this.chkEnablePersistence.Location = new System.Drawing.Point(15, 15);
            this.chkEnablePersistence.Name = "chkEnablePersistence";
            this.chkEnablePersistence.Size = new System.Drawing.Size(116, 17);
            this.chkEnablePersistence.TabIndex = 53;
            this.chkEnablePersistence.Text = "Enable persistence";
            this.chkEnablePersistence.UseVisualStyleBackColor = true;
            this.chkEnablePersistence.CheckedChanged += new System.EventHandler(this.chkFading_CheckedChanged);
            // 
            // tabTracking
            // 
            this.tabTracking.Controls.Add(this.lblDescription);
            this.tabTracking.Controls.Add(this.cmbSearchWindowUnit);
            this.tabTracking.Controls.Add(this.cmbBlockWindowUnit);
            this.tabTracking.Controls.Add(this.tbSearchHeight);
            this.tabTracking.Controls.Add(this.label5);
            this.tabTracking.Controls.Add(this.tbSearchWidth);
            this.tabTracking.Controls.Add(this.tbBlockHeight);
            this.tabTracking.Controls.Add(this.lblSearchWindow);
            this.tabTracking.Controls.Add(this.label4);
            this.tabTracking.Controls.Add(this.tbBlockWidth);
            this.tabTracking.Controls.Add(this.lblObjectWindow);
            this.tabTracking.Location = new System.Drawing.Point(4, 22);
            this.tabTracking.Name = "tabTracking";
            this.tabTracking.Padding = new System.Windows.Forms.Padding(3);
            this.tabTracking.Size = new System.Drawing.Size(424, 210);
            this.tabTracking.TabIndex = 2;
            this.tabTracking.Text = "Tracking";
            this.tabTracking.UseVisualStyleBackColor = true;
            // 
            // tbSearchHeight
            // 
            this.tbSearchHeight.Location = new System.Drawing.Point(241, 72);
            this.tbSearchHeight.Name = "tbSearchHeight";
            this.tbSearchHeight.Size = new System.Drawing.Size(30, 20);
            this.tbSearchHeight.TabIndex = 62;
            this.tbSearchHeight.TextChanged += new System.EventHandler(this.tbSearchHeight_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(222, 75);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(13, 13);
            this.label5.TabIndex = 61;
            this.label5.Text = "×";
            // 
            // tbSearchWidth
            // 
            this.tbSearchWidth.Location = new System.Drawing.Point(182, 72);
            this.tbSearchWidth.Name = "tbSearchWidth";
            this.tbSearchWidth.Size = new System.Drawing.Size(30, 20);
            this.tbSearchWidth.TabIndex = 60;
            this.tbSearchWidth.TextChanged += new System.EventHandler(this.tbSearchWidth_TextChanged);
            // 
            // tbBlockHeight
            // 
            this.tbBlockHeight.Location = new System.Drawing.Point(241, 46);
            this.tbBlockHeight.Name = "tbBlockHeight";
            this.tbBlockHeight.Size = new System.Drawing.Size(30, 20);
            this.tbBlockHeight.TabIndex = 59;
            this.tbBlockHeight.TextChanged += new System.EventHandler(this.tbBlockHeight_TextChanged);
            // 
            // lblSearchWindow
            // 
            this.lblSearchWindow.AutoSize = true;
            this.lblSearchWindow.Location = new System.Drawing.Point(52, 76);
            this.lblSearchWindow.Name = "lblSearchWindow";
            this.lblSearchWindow.Size = new System.Drawing.Size(86, 13);
            this.lblSearchWindow.TabIndex = 58;
            this.lblSearchWindow.Text = "Search window :";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(222, 49);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(13, 13);
            this.label4.TabIndex = 57;
            this.label4.Text = "×";
            // 
            // tbBlockWidth
            // 
            this.tbBlockWidth.Location = new System.Drawing.Point(182, 46);
            this.tbBlockWidth.Name = "tbBlockWidth";
            this.tbBlockWidth.Size = new System.Drawing.Size(30, 20);
            this.tbBlockWidth.TabIndex = 55;
            this.tbBlockWidth.TextChanged += new System.EventHandler(this.tbBlockWidth_TextChanged);
            // 
            // lblObjectWindow
            // 
            this.lblObjectWindow.AutoSize = true;
            this.lblObjectWindow.Location = new System.Drawing.Point(52, 49);
            this.lblObjectWindow.Name = "lblObjectWindow";
            this.lblObjectWindow.Size = new System.Drawing.Size(83, 13);
            this.lblObjectWindow.TabIndex = 56;
            this.lblObjectWindow.Text = "Object window :";
            // 
            // cmbBlockWindowUnit
            // 
            this.cmbBlockWindowUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBlockWindowUnit.Location = new System.Drawing.Point(288, 45);
            this.cmbBlockWindowUnit.Name = "cmbBlockWindowUnit";
            this.cmbBlockWindowUnit.Size = new System.Drawing.Size(82, 21);
            this.cmbBlockWindowUnit.TabIndex = 65;
            this.cmbBlockWindowUnit.SelectedIndexChanged += new System.EventHandler(this.cmbBlockWindowUnit_SelectedIndexChanged);
            // 
            // cmbSearchWindowUnit
            // 
            this.cmbSearchWindowUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSearchWindowUnit.Location = new System.Drawing.Point(288, 72);
            this.cmbSearchWindowUnit.Name = "cmbSearchWindowUnit";
            this.cmbSearchWindowUnit.Size = new System.Drawing.Size(82, 21);
            this.cmbSearchWindowUnit.TabIndex = 66;
            this.cmbSearchWindowUnit.SelectedIndexChanged += new System.EventHandler(this.cmbSearchWindowUnit_SelectedIndexChanged);
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(16, 13);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(137, 13);
            this.lblDescription.TabIndex = 67;
            this.lblDescription.Text = "Default tracking parameters";
            // 
            // PreferencePanelDrawings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabSubPages);
            this.Name = "PreferencePanelDrawings";
            this.Size = new System.Drawing.Size(432, 236);
            this.tabSubPages.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tabGeneral.PerformLayout();
            this.tabPersistence.ResumeLayout(false);
            this.tabPersistence.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkFading)).EndInit();
            this.tabTracking.ResumeLayout(false);
            this.tabTracking.PerformLayout();
            this.ResumeLayout(false);

		}
		private System.Windows.Forms.TabControl tabSubPages;
		private System.Windows.Forms.TabPage tabGeneral;
		private System.Windows.Forms.TabPage tabPersistence;
		private System.Windows.Forms.CheckBox chkEnablePersistence;
		private System.Windows.Forms.TrackBar trkFading;
		private System.Windows.Forms.Label lblFading;
		private System.Windows.Forms.CheckBox chkAlwaysVisible;
		private System.Windows.Forms.CheckBox chkDrawOnPlay;
        private System.Windows.Forms.TabPage tabTracking;
        private System.Windows.Forms.TextBox tbSearchHeight;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbSearchWidth;
        private System.Windows.Forms.TextBox tbBlockHeight;
        private System.Windows.Forms.Label lblSearchWindow;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbBlockWidth;
        private System.Windows.Forms.Label lblObjectWindow;
        private System.Windows.Forms.ComboBox cmbSearchWindowUnit;
        private System.Windows.Forms.ComboBox cmbBlockWindowUnit;
        private System.Windows.Forms.Label lblDescription;
	}
}
