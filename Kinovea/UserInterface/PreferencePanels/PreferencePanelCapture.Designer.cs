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
            this.btnBrowseVideo = new System.Windows.Forms.Button();
            this.btnBrowseImage = new System.Windows.Forms.Button();
            this.tbVideoDirectory = new System.Windows.Forms.TextBox();
            this.tbImageDirectory = new System.Windows.Forms.TextBox();
            this.cmbImageFormat = new System.Windows.Forms.ComboBox();
            this.lblImageFormat = new System.Windows.Forms.Label();
            this.lblVideoDirectory = new System.Windows.Forms.Label();
            this.lblImageDirectory = new System.Windows.Forms.Label();
            this.tabNaming = new System.Windows.Forms.TabPage();
            this.btnResetCounter = new System.Windows.Forms.Button();
            this.lblCounter = new System.Windows.Forms.Label();
            this.btnIncrement = new System.Windows.Forms.Button();
            this.lblSecond = new System.Windows.Forms.Label();
            this.lblMinute = new System.Windows.Forms.Label();
            this.lblHour = new System.Windows.Forms.Label();
            this.btnHour = new System.Windows.Forms.Button();
            this.btnSecond = new System.Windows.Forms.Button();
            this.btnMinute = new System.Windows.Forms.Button();
            this.lblDay = new System.Windows.Forms.Label();
            this.lblMonth = new System.Windows.Forms.Label();
            this.lblYear = new System.Windows.Forms.Label();
            this.btnYear = new System.Windows.Forms.Button();
            this.btnDay = new System.Windows.Forms.Button();
            this.btnMonth = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.lblSample = new System.Windows.Forms.Label();
            this.tbPattern = new System.Windows.Forms.TextBox();
            this.rbPattern = new System.Windows.Forms.RadioButton();
            this.rbFreeText = new System.Windows.Forms.RadioButton();
            this.tabMemory = new System.Windows.Forms.TabPage();
            this.lblMemoryBuffer = new System.Windows.Forms.Label();
            this.trkMemoryBuffer = new System.Windows.Forms.TrackBar();
            this.tabSubPages.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.tabNaming.SuspendLayout();
            this.tabMemory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkMemoryBuffer)).BeginInit();
            this.SuspendLayout();
            // 
            // tabSubPages
            // 
            this.tabSubPages.Controls.Add(this.tabGeneral);
            this.tabSubPages.Controls.Add(this.tabNaming);
            this.tabSubPages.Controls.Add(this.tabMemory);
            this.tabSubPages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabSubPages.Location = new System.Drawing.Point(0, 0);
            this.tabSubPages.Name = "tabSubPages";
            this.tabSubPages.SelectedIndex = 0;
            this.tabSubPages.Size = new System.Drawing.Size(432, 236);
            this.tabSubPages.TabIndex = 0;
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.btnBrowseVideo);
            this.tabGeneral.Controls.Add(this.btnBrowseImage);
            this.tabGeneral.Controls.Add(this.tbVideoDirectory);
            this.tabGeneral.Controls.Add(this.tbImageDirectory);
            this.tabGeneral.Controls.Add(this.cmbImageFormat);
            this.tabGeneral.Controls.Add(this.lblImageFormat);
            this.tabGeneral.Controls.Add(this.lblVideoDirectory);
            this.tabGeneral.Controls.Add(this.lblImageDirectory);
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneral.Size = new System.Drawing.Size(424, 210);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // btnBrowseVideo
            // 
            this.btnBrowseVideo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseVideo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnBrowseVideo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBrowseVideo.FlatAppearance.BorderSize = 0;
            this.btnBrowseVideo.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
            this.btnBrowseVideo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowseVideo.Image = global::Kinovea.Root.Properties.Resources.folder;
            this.btnBrowseVideo.Location = new System.Drawing.Point(375, 54);
            this.btnBrowseVideo.MinimumSize = new System.Drawing.Size(25, 25);
            this.btnBrowseVideo.Name = "btnBrowseVideo";
            this.btnBrowseVideo.Size = new System.Drawing.Size(30, 25);
            this.btnBrowseVideo.TabIndex = 37;
            this.btnBrowseVideo.Tag = "";
            this.btnBrowseVideo.UseVisualStyleBackColor = true;
            this.btnBrowseVideo.Click += new System.EventHandler(this.btnBrowseVideoLocation_Click);
            // 
            // btnBrowseImage
            // 
            this.btnBrowseImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseImage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnBrowseImage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBrowseImage.FlatAppearance.BorderSize = 0;
            this.btnBrowseImage.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
            this.btnBrowseImage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowseImage.Image = global::Kinovea.Root.Properties.Resources.folder;
            this.btnBrowseImage.Location = new System.Drawing.Point(375, 25);
            this.btnBrowseImage.MinimumSize = new System.Drawing.Size(25, 25);
            this.btnBrowseImage.Name = "btnBrowseImage";
            this.btnBrowseImage.Size = new System.Drawing.Size(30, 25);
            this.btnBrowseImage.TabIndex = 36;
            this.btnBrowseImage.Tag = "";
            this.btnBrowseImage.UseVisualStyleBackColor = true;
            this.btnBrowseImage.Click += new System.EventHandler(this.btnBrowseImageLocation_Click);
            // 
            // tbVideoDirectory
            // 
            this.tbVideoDirectory.Location = new System.Drawing.Point(171, 59);
            this.tbVideoDirectory.Name = "tbVideoDirectory";
            this.tbVideoDirectory.Size = new System.Drawing.Size(198, 20);
            this.tbVideoDirectory.TabIndex = 7;
            this.tbVideoDirectory.TextChanged += new System.EventHandler(this.tbVideoDirectory_TextChanged);
            // 
            // tbImageDirectory
            // 
            this.tbImageDirectory.Location = new System.Drawing.Point(171, 30);
            this.tbImageDirectory.Name = "tbImageDirectory";
            this.tbImageDirectory.Size = new System.Drawing.Size(198, 20);
            this.tbImageDirectory.TabIndex = 6;
            this.tbImageDirectory.TextChanged += new System.EventHandler(this.tbImageDirectory_TextChanged);
            // 
            // cmbImageFormat
            // 
            this.cmbImageFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbImageFormat.FormattingEnabled = true;
            this.cmbImageFormat.Location = new System.Drawing.Point(171, 119);
            this.cmbImageFormat.Name = "cmbImageFormat";
            this.cmbImageFormat.Size = new System.Drawing.Size(52, 21);
            this.cmbImageFormat.TabIndex = 5;
            this.cmbImageFormat.SelectedIndexChanged += new System.EventHandler(this.cmbImageFormat_SelectedIndexChanged);
            // 
            // lblImageFormat
            // 
            this.lblImageFormat.Location = new System.Drawing.Point(16, 122);
            this.lblImageFormat.Name = "lblImageFormat";
            this.lblImageFormat.Size = new System.Drawing.Size(149, 18);
            this.lblImageFormat.TabIndex = 2;
            this.lblImageFormat.Text = "Image format :";
            // 
            // lblVideoDirectory
            // 
            this.lblVideoDirectory.Location = new System.Drawing.Point(16, 62);
            this.lblVideoDirectory.Name = "lblVideoDirectory";
            this.lblVideoDirectory.Size = new System.Drawing.Size(149, 17);
            this.lblVideoDirectory.TabIndex = 1;
            this.lblVideoDirectory.Text = "Video directory :";
            // 
            // lblImageDirectory
            // 
            this.lblImageDirectory.Location = new System.Drawing.Point(16, 33);
            this.lblImageDirectory.Name = "lblImageDirectory";
            this.lblImageDirectory.Size = new System.Drawing.Size(149, 17);
            this.lblImageDirectory.TabIndex = 0;
            this.lblImageDirectory.Text = "Image directory :";
            // 
            // tabNaming
            // 
            this.tabNaming.Controls.Add(this.btnResetCounter);
            this.tabNaming.Controls.Add(this.lblCounter);
            this.tabNaming.Controls.Add(this.btnIncrement);
            this.tabNaming.Controls.Add(this.lblSecond);
            this.tabNaming.Controls.Add(this.lblMinute);
            this.tabNaming.Controls.Add(this.lblHour);
            this.tabNaming.Controls.Add(this.btnHour);
            this.tabNaming.Controls.Add(this.btnSecond);
            this.tabNaming.Controls.Add(this.btnMinute);
            this.tabNaming.Controls.Add(this.lblDay);
            this.tabNaming.Controls.Add(this.lblMonth);
            this.tabNaming.Controls.Add(this.lblYear);
            this.tabNaming.Controls.Add(this.btnYear);
            this.tabNaming.Controls.Add(this.btnDay);
            this.tabNaming.Controls.Add(this.btnMonth);
            this.tabNaming.Controls.Add(this.button1);
            this.tabNaming.Controls.Add(this.lblSample);
            this.tabNaming.Controls.Add(this.tbPattern);
            this.tabNaming.Controls.Add(this.rbPattern);
            this.tabNaming.Controls.Add(this.rbFreeText);
            this.tabNaming.Location = new System.Drawing.Point(4, 22);
            this.tabNaming.Name = "tabNaming";
            this.tabNaming.Padding = new System.Windows.Forms.Padding(3);
            this.tabNaming.Size = new System.Drawing.Size(424, 210);
            this.tabNaming.TabIndex = 1;
            this.tabNaming.Text = "File naming";
            this.tabNaming.UseVisualStyleBackColor = true;
            // 
            // btnResetCounter
            // 
            this.btnResetCounter.BackColor = System.Drawing.Color.Transparent;
            this.btnResetCounter.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnResetCounter.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnResetCounter.Location = new System.Drawing.Point(266, 168);
            this.btnResetCounter.Name = "btnResetCounter";
            this.btnResetCounter.Size = new System.Drawing.Size(142, 25);
            this.btnResetCounter.TabIndex = 21;
            this.btnResetCounter.Text = "Reset counters";
            this.btnResetCounter.UseVisualStyleBackColor = false;
            this.btnResetCounter.Click += new System.EventHandler(this.btnResetCounter_Click);
            // 
            // lblCounter
            // 
            this.lblCounter.AutoSize = true;
            this.lblCounter.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblCounter.Location = new System.Drawing.Point(312, 119);
            this.lblCounter.Name = "lblCounter";
            this.lblCounter.Size = new System.Drawing.Size(44, 13);
            this.lblCounter.TabIndex = 20;
            this.lblCounter.Text = "Counter";
            this.lblCounter.Click += new System.EventHandler(this.lblMarker_Click);
            // 
            // btnIncrement
            // 
            this.btnIncrement.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btnIncrement.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnIncrement.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnIncrement.Location = new System.Drawing.Point(266, 113);
            this.btnIncrement.Name = "btnIncrement";
            this.btnIncrement.Size = new System.Drawing.Size(40, 25);
            this.btnIncrement.TabIndex = 19;
            this.btnIncrement.Text = "%i";
            this.btnIncrement.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnIncrement.UseVisualStyleBackColor = false;
            this.btnIncrement.Click += new System.EventHandler(this.btnMarker_Click);
            // 
            // lblSecond
            // 
            this.lblSecond.AutoSize = true;
            this.lblSecond.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblSecond.Location = new System.Drawing.Point(198, 174);
            this.lblSecond.Name = "lblSecond";
            this.lblSecond.Size = new System.Drawing.Size(44, 13);
            this.lblSecond.TabIndex = 18;
            this.lblSecond.Text = "Second";
            this.lblSecond.Click += new System.EventHandler(this.lblMarker_Click);
            // 
            // lblMinute
            // 
            this.lblMinute.AutoSize = true;
            this.lblMinute.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblMinute.Location = new System.Drawing.Point(198, 146);
            this.lblMinute.Name = "lblMinute";
            this.lblMinute.Size = new System.Drawing.Size(39, 13);
            this.lblMinute.TabIndex = 17;
            this.lblMinute.Text = "Minute";
            this.lblMinute.Click += new System.EventHandler(this.lblMarker_Click);
            // 
            // lblHour
            // 
            this.lblHour.AutoSize = true;
            this.lblHour.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblHour.Location = new System.Drawing.Point(198, 119);
            this.lblHour.Name = "lblHour";
            this.lblHour.Size = new System.Drawing.Size(30, 13);
            this.lblHour.TabIndex = 16;
            this.lblHour.Text = "Hour";
            this.lblHour.Click += new System.EventHandler(this.lblMarker_Click);
            // 
            // btnHour
            // 
            this.btnHour.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnHour.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnHour.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnHour.Location = new System.Drawing.Point(152, 113);
            this.btnHour.Name = "btnHour";
            this.btnHour.Size = new System.Drawing.Size(40, 25);
            this.btnHour.TabIndex = 15;
            this.btnHour.Text = "%h";
            this.btnHour.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnHour.UseVisualStyleBackColor = false;
            this.btnHour.Click += new System.EventHandler(this.btnMarker_Click);
            // 
            // btnSecond
            // 
            this.btnSecond.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btnSecond.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSecond.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSecond.Location = new System.Drawing.Point(152, 168);
            this.btnSecond.Name = "btnSecond";
            this.btnSecond.Size = new System.Drawing.Size(40, 25);
            this.btnSecond.TabIndex = 14;
            this.btnSecond.Text = "%s";
            this.btnSecond.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnSecond.UseVisualStyleBackColor = false;
            this.btnSecond.Click += new System.EventHandler(this.btnMarker_Click);
            // 
            // btnMinute
            // 
            this.btnMinute.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnMinute.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMinute.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMinute.Location = new System.Drawing.Point(152, 140);
            this.btnMinute.Name = "btnMinute";
            this.btnMinute.Size = new System.Drawing.Size(40, 25);
            this.btnMinute.TabIndex = 13;
            this.btnMinute.Text = "%mi";
            this.btnMinute.UseVisualStyleBackColor = false;
            this.btnMinute.Click += new System.EventHandler(this.btnMarker_Click);
            // 
            // lblDay
            // 
            this.lblDay.AutoSize = true;
            this.lblDay.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblDay.Location = new System.Drawing.Point(83, 174);
            this.lblDay.Name = "lblDay";
            this.lblDay.Size = new System.Drawing.Size(26, 13);
            this.lblDay.TabIndex = 12;
            this.lblDay.Text = "Day";
            this.lblDay.Click += new System.EventHandler(this.lblMarker_Click);
            // 
            // lblMonth
            // 
            this.lblMonth.AutoSize = true;
            this.lblMonth.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblMonth.Location = new System.Drawing.Point(83, 146);
            this.lblMonth.Name = "lblMonth";
            this.lblMonth.Size = new System.Drawing.Size(37, 13);
            this.lblMonth.TabIndex = 11;
            this.lblMonth.Text = "Month";
            this.lblMonth.Click += new System.EventHandler(this.lblMarker_Click);
            // 
            // lblYear
            // 
            this.lblYear.AutoSize = true;
            this.lblYear.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblYear.Location = new System.Drawing.Point(83, 119);
            this.lblYear.Name = "lblYear";
            this.lblYear.Size = new System.Drawing.Size(29, 13);
            this.lblYear.TabIndex = 10;
            this.lblYear.Text = "Year";
            this.lblYear.Click += new System.EventHandler(this.lblMarker_Click);
            // 
            // btnYear
            // 
            this.btnYear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnYear.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnYear.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnYear.Location = new System.Drawing.Point(37, 113);
            this.btnYear.Name = "btnYear";
            this.btnYear.Size = new System.Drawing.Size(40, 25);
            this.btnYear.TabIndex = 9;
            this.btnYear.Text = "%y";
            this.btnYear.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnYear.UseVisualStyleBackColor = false;
            this.btnYear.Click += new System.EventHandler(this.btnMarker_Click);
            // 
            // btnDay
            // 
            this.btnDay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnDay.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDay.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDay.Location = new System.Drawing.Point(37, 168);
            this.btnDay.Name = "btnDay";
            this.btnDay.Size = new System.Drawing.Size(40, 25);
            this.btnDay.TabIndex = 8;
            this.btnDay.Text = "%d";
            this.btnDay.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnDay.UseVisualStyleBackColor = false;
            this.btnDay.Click += new System.EventHandler(this.btnMarker_Click);
            // 
            // btnMonth
            // 
            this.btnMonth.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnMonth.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMonth.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMonth.Location = new System.Drawing.Point(37, 140);
            this.btnMonth.Name = "btnMonth";
            this.btnMonth.Size = new System.Drawing.Size(40, 25);
            this.btnMonth.TabIndex = 6;
            this.btnMonth.Text = "%mo";
            this.btnMonth.UseVisualStyleBackColor = false;
            this.btnMonth.Click += new System.EventHandler(this.btnMarker_Click);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.Transparent;
            this.button1.BackgroundImage = global::Kinovea.Root.Properties.Resources.bullet_go;
            this.button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(210, 71);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(20, 20);
            this.button1.TabIndex = 5;
            this.button1.UseVisualStyleBackColor = false;
            // 
            // lblSample
            // 
            this.lblSample.BackColor = System.Drawing.Color.WhiteSmoke;
            this.lblSample.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSample.Location = new System.Drawing.Point(236, 71);
            this.lblSample.Name = "lblSample";
            this.lblSample.Size = new System.Drawing.Size(172, 21);
            this.lblSample.TabIndex = 4;
            this.lblSample.Text = "[computed value]";
            this.lblSample.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbPattern
            // 
            this.tbPattern.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbPattern.Location = new System.Drawing.Point(51, 71);
            this.tbPattern.Name = "tbPattern";
            this.tbPattern.Size = new System.Drawing.Size(153, 20);
            this.tbPattern.TabIndex = 2;
            this.tbPattern.Text = "Cap-%y-%mo-%d - %i";
            this.tbPattern.TextChanged += new System.EventHandler(this.tbPattern_TextChanged);
            // 
            // rbPattern
            // 
            this.rbPattern.Location = new System.Drawing.Point(20, 43);
            this.rbPattern.Name = "rbPattern";
            this.rbPattern.Size = new System.Drawing.Size(286, 22);
            this.rbPattern.TabIndex = 1;
            this.rbPattern.TabStop = true;
            this.rbPattern.Text = "Naming pattern";
            this.rbPattern.UseVisualStyleBackColor = true;
            this.rbPattern.CheckedChanged += new System.EventHandler(this.radio_CheckedChanged);
            // 
            // rbFreeText
            // 
            this.rbFreeText.Location = new System.Drawing.Point(20, 14);
            this.rbFreeText.Name = "rbFreeText";
            this.rbFreeText.Size = new System.Drawing.Size(286, 20);
            this.rbFreeText.TabIndex = 0;
            this.rbFreeText.TabStop = true;
            this.rbFreeText.Text = "Free text with automatic counter";
            this.rbFreeText.UseVisualStyleBackColor = true;
            this.rbFreeText.CheckedChanged += new System.EventHandler(this.radio_CheckedChanged);
            // 
            // tabMemory
            // 
            this.tabMemory.Controls.Add(this.lblMemoryBuffer);
            this.tabMemory.Controls.Add(this.trkMemoryBuffer);
            this.tabMemory.Location = new System.Drawing.Point(4, 22);
            this.tabMemory.Name = "tabMemory";
            this.tabMemory.Size = new System.Drawing.Size(424, 210);
            this.tabMemory.TabIndex = 2;
            this.tabMemory.Text = "Memory";
            this.tabMemory.UseVisualStyleBackColor = true;
            // 
            // lblMemoryBuffer
            // 
            this.lblMemoryBuffer.AutoSize = true;
            this.lblMemoryBuffer.Location = new System.Drawing.Point(15, 30);
            this.lblMemoryBuffer.Name = "lblMemoryBuffer";
            this.lblMemoryBuffer.Size = new System.Drawing.Size(221, 13);
            this.lblMemoryBuffer.TabIndex = 36;
            this.lblMemoryBuffer.Text = "Memory allocated for capture buffers : {0} MB";
            // 
            // trkMemoryBuffer
            // 
            this.trkMemoryBuffer.BackColor = System.Drawing.Color.White;
            this.trkMemoryBuffer.Location = new System.Drawing.Point(15, 55);
            this.trkMemoryBuffer.Maximum = 1024;
            this.trkMemoryBuffer.Minimum = 16;
            this.trkMemoryBuffer.Name = "trkMemoryBuffer";
            this.trkMemoryBuffer.Size = new System.Drawing.Size(386, 45);
            this.trkMemoryBuffer.TabIndex = 38;
            this.trkMemoryBuffer.TickFrequency = 50;
            this.trkMemoryBuffer.Value = 16;
            this.trkMemoryBuffer.ValueChanged += new System.EventHandler(this.trkMemoryBuffer_ValueChanged);
            // 
            // PreferencePanelCapture
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabSubPages);
            this.Name = "PreferencePanelCapture";
            this.Size = new System.Drawing.Size(432, 236);
            this.tabSubPages.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tabGeneral.PerformLayout();
            this.tabNaming.ResumeLayout(false);
            this.tabNaming.PerformLayout();
            this.tabMemory.ResumeLayout(false);
            this.tabMemory.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkMemoryBuffer)).EndInit();
            this.ResumeLayout(false);

		}
		private System.Windows.Forms.Label lblMemoryBuffer;
		private System.Windows.Forms.TrackBar trkMemoryBuffer;
        private System.Windows.Forms.TabPage tabMemory;
		private System.Windows.Forms.Label lblImageFormat;
		private System.Windows.Forms.Label lblVideoDirectory;
		private System.Windows.Forms.Label lblImageDirectory;
		private System.Windows.Forms.Button btnBrowseImage;
		private System.Windows.Forms.Button btnBrowseVideo;
		private System.Windows.Forms.TextBox tbImageDirectory;
		private System.Windows.Forms.TextBox tbVideoDirectory;
        private System.Windows.Forms.ComboBox cmbImageFormat;
		private System.Windows.Forms.Label lblCounter;
		private System.Windows.Forms.Label lblSecond;
		private System.Windows.Forms.Label lblMinute;
		private System.Windows.Forms.Label lblHour;
		private System.Windows.Forms.Label lblDay;
		private System.Windows.Forms.Label lblMonth;
		private System.Windows.Forms.Label lblYear;
		private System.Windows.Forms.RadioButton rbPattern;
		private System.Windows.Forms.RadioButton rbFreeText;
		private System.Windows.Forms.Button btnResetCounter;
		private System.Windows.Forms.Button btnIncrement;
		private System.Windows.Forms.Button btnHour;
		private System.Windows.Forms.Button btnSecond;
		private System.Windows.Forms.Button btnMinute;
		private System.Windows.Forms.Button btnDay;
		private System.Windows.Forms.Button btnMonth;
		private System.Windows.Forms.Button btnYear;
		private System.Windows.Forms.Label lblSample;
		private System.Windows.Forms.TextBox tbPattern;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.TabControl tabSubPages;
		private System.Windows.Forms.TabPage tabGeneral;
		private System.Windows.Forms.TabPage tabNaming;
	}
}
