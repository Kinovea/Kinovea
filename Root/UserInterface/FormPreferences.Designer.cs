namespace Kinovea.Root
{
    partial class formPreferences
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
        	this.panel1 = new System.Windows.Forms.Panel();
        	this.btnDrawings = new System.Windows.Forms.Button();
        	this.btnPlayAnalyze = new System.Windows.Forms.Button();
        	this.btnGeneral = new System.Windows.Forms.Button();
        	this.pageGeneral = new System.Windows.Forms.Panel();
        	this.grpGeneral = new System.Windows.Forms.GroupBox();
        	this.cmbSpeedUnit = new System.Windows.Forms.ComboBox();
        	this.lblSpeedUnit = new System.Windows.Forms.Label();
        	this.chkDeinterlace = new System.Windows.Forms.CheckBox();
        	this.cmbImageFormats = new System.Windows.Forms.ComboBox();
        	this.lblImageFormat = new System.Windows.Forms.Label();
        	this.cmbTimeCodeFormat = new System.Windows.Forms.ComboBox();
        	this.lblTimeMarkersFormat = new System.Windows.Forms.Label();
        	this.cmbHistoryCount = new System.Windows.Forms.ComboBox();
        	this.lblLanguage = new System.Windows.Forms.Label();
        	this.lblHistoryCount = new System.Windows.Forms.Label();
        	this.cmbLanguage = new System.Windows.Forms.ComboBox();
        	this.pagePlayAnalyze = new System.Windows.Forms.Panel();
        	this.grpColors = new System.Windows.Forms.GroupBox();
        	this.lblPlane3D = new System.Windows.Forms.Label();
        	this.lblGrid = new System.Windows.Forms.Label();
        	this.btn3DPlaneColor = new System.Windows.Forms.Button();
        	this.btnGridColor = new System.Windows.Forms.Button();
        	this.grpSwitchToAnalysis = new System.Windows.Forms.GroupBox();
        	this.lblWorkingZoneLogic = new System.Windows.Forms.Label();
        	this.trkWorkingZoneSeconds = new System.Windows.Forms.TrackBar();
        	this.trkWorkingZoneMemory = new System.Windows.Forms.TrackBar();
        	this.lblWorkingZoneMemory = new System.Windows.Forms.Label();
        	this.lblWorkingZoneSeconds = new System.Windows.Forms.Label();
        	this.btnSave = new System.Windows.Forms.Button();
        	this.btnCancel = new System.Windows.Forms.Button();
        	this.pageDrawings = new System.Windows.Forms.Panel();
        	this.chkDrawOnPlay = new System.Windows.Forms.CheckBox();
        	this.grpDrawingsFading = new System.Windows.Forms.GroupBox();
        	this.chkAlwaysVisible = new System.Windows.Forms.CheckBox();
        	this.lblFading = new System.Windows.Forms.Label();
        	this.trkFading = new System.Windows.Forms.TrackBar();
        	this.chkEnablePersistence = new System.Windows.Forms.CheckBox();
        	this.panel1.SuspendLayout();
        	this.pageGeneral.SuspendLayout();
        	this.grpGeneral.SuspendLayout();
        	this.pagePlayAnalyze.SuspendLayout();
        	this.grpColors.SuspendLayout();
        	this.grpSwitchToAnalysis.SuspendLayout();
        	((System.ComponentModel.ISupportInitialize)(this.trkWorkingZoneSeconds)).BeginInit();
        	((System.ComponentModel.ISupportInitialize)(this.trkWorkingZoneMemory)).BeginInit();
        	this.pageDrawings.SuspendLayout();
        	this.grpDrawingsFading.SuspendLayout();
        	((System.ComponentModel.ISupportInitialize)(this.trkFading)).BeginInit();
        	this.SuspendLayout();
        	// 
        	// panel1
        	// 
        	this.panel1.BackColor = System.Drawing.Color.White;
        	this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.panel1.Controls.Add(this.btnDrawings);
        	this.panel1.Controls.Add(this.btnPlayAnalyze);
        	this.panel1.Controls.Add(this.btnGeneral);
        	this.panel1.ForeColor = System.Drawing.Color.Black;
        	this.panel1.Location = new System.Drawing.Point(8, 9);
        	this.panel1.Margin = new System.Windows.Forms.Padding(4);
        	this.panel1.Name = "panel1";
        	this.panel1.Size = new System.Drawing.Size(189, 358);
        	this.panel1.TabIndex = 0;
        	// 
        	// btnDrawings
        	// 
        	this.btnDrawings.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnDrawings.FlatAppearance.BorderSize = 0;
        	this.btnDrawings.FlatAppearance.MouseDownBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawings.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnDrawings.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.btnDrawings.Image = global::Kinovea.Root.Properties.Resources.drawings;
        	this.btnDrawings.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
        	this.btnDrawings.Location = new System.Drawing.Point(-1, 154);
        	this.btnDrawings.Margin = new System.Windows.Forms.Padding(4);
        	this.btnDrawings.Name = "btnDrawings";
        	this.btnDrawings.Size = new System.Drawing.Size(189, 68);
        	this.btnDrawings.TabIndex = 2;
        	this.btnDrawings.Text = "Drawings";
        	this.btnDrawings.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
        	this.btnDrawings.UseVisualStyleBackColor = true;
        	this.btnDrawings.Click += new System.EventHandler(this.btnDrawings_Click);
        	// 
        	// btnPlayAnalyze
        	// 
        	this.btnPlayAnalyze.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnPlayAnalyze.FlatAppearance.BorderSize = 0;
        	this.btnPlayAnalyze.FlatAppearance.MouseDownBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnPlayAnalyze.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnPlayAnalyze.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnPlayAnalyze.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.btnPlayAnalyze.Image = global::Kinovea.Root.Properties.Resources.video;
        	this.btnPlayAnalyze.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
        	this.btnPlayAnalyze.Location = new System.Drawing.Point(-1, 80);
        	this.btnPlayAnalyze.Margin = new System.Windows.Forms.Padding(4);
        	this.btnPlayAnalyze.Name = "btnPlayAnalyze";
        	this.btnPlayAnalyze.Size = new System.Drawing.Size(189, 68);
        	this.btnPlayAnalyze.TabIndex = 1;
        	this.btnPlayAnalyze.Text = "Play / Analyze";
        	this.btnPlayAnalyze.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
        	this.btnPlayAnalyze.UseVisualStyleBackColor = true;
        	this.btnPlayAnalyze.Click += new System.EventHandler(this.btnPlayAnalyze_Click);
        	// 
        	// btnGeneral
        	// 
        	this.btnGeneral.BackColor = System.Drawing.Color.White;
        	this.btnGeneral.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnGeneral.FlatAppearance.BorderSize = 0;
        	this.btnGeneral.FlatAppearance.MouseDownBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnGeneral.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnGeneral.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnGeneral.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.btnGeneral.Image = global::Kinovea.Root.Properties.Resources.configure;
        	this.btnGeneral.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
        	this.btnGeneral.Location = new System.Drawing.Point(5, 6);
        	this.btnGeneral.Margin = new System.Windows.Forms.Padding(4);
        	this.btnGeneral.Name = "btnGeneral";
        	this.btnGeneral.Size = new System.Drawing.Size(176, 68);
        	this.btnGeneral.TabIndex = 0;
        	this.btnGeneral.Text = "General";
        	this.btnGeneral.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
        	this.btnGeneral.UseVisualStyleBackColor = false;
        	this.btnGeneral.Click += new System.EventHandler(this.btnGeneral_Click);
        	// 
        	// pageGeneral
        	// 
        	this.pageGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.pageGeneral.BackColor = System.Drawing.Color.White;
        	this.pageGeneral.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.pageGeneral.Controls.Add(this.grpGeneral);
        	this.pageGeneral.Location = new System.Drawing.Point(205, 10);
        	this.pageGeneral.Margin = new System.Windows.Forms.Padding(4);
        	this.pageGeneral.Name = "pageGeneral";
        	this.pageGeneral.Size = new System.Drawing.Size(578, 357);
        	this.pageGeneral.TabIndex = 1;
        	// 
        	// grpGeneral
        	// 
        	this.grpGeneral.Controls.Add(this.cmbSpeedUnit);
        	this.grpGeneral.Controls.Add(this.lblSpeedUnit);
        	this.grpGeneral.Controls.Add(this.chkDeinterlace);
        	this.grpGeneral.Controls.Add(this.cmbImageFormats);
        	this.grpGeneral.Controls.Add(this.lblImageFormat);
        	this.grpGeneral.Controls.Add(this.cmbTimeCodeFormat);
        	this.grpGeneral.Controls.Add(this.lblTimeMarkersFormat);
        	this.grpGeneral.Controls.Add(this.cmbHistoryCount);
        	this.grpGeneral.Controls.Add(this.lblLanguage);
        	this.grpGeneral.Controls.Add(this.lblHistoryCount);
        	this.grpGeneral.Controls.Add(this.cmbLanguage);
        	this.grpGeneral.Location = new System.Drawing.Point(19, 12);
        	this.grpGeneral.Margin = new System.Windows.Forms.Padding(4);
        	this.grpGeneral.Name = "grpGeneral";
        	this.grpGeneral.Padding = new System.Windows.Forms.Padding(4);
        	this.grpGeneral.Size = new System.Drawing.Size(540, 322);
        	this.grpGeneral.TabIndex = 21;
        	this.grpGeneral.TabStop = false;
        	this.grpGeneral.Text = "General";
        	// 
        	// cmbSpeedUnit
        	// 
        	this.cmbSpeedUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        	this.cmbSpeedUnit.Location = new System.Drawing.Point(265, 183);
        	this.cmbSpeedUnit.Margin = new System.Windows.Forms.Padding(4);
        	this.cmbSpeedUnit.Name = "cmbSpeedUnit";
        	this.cmbSpeedUnit.Size = new System.Drawing.Size(244, 24);
        	this.cmbSpeedUnit.TabIndex = 20;
        	this.cmbSpeedUnit.SelectedIndexChanged += new System.EventHandler(this.cmbSpeedUnit_SelectedIndexChanged);
        	// 
        	// lblSpeedUnit
        	// 
        	this.lblSpeedUnit.AutoSize = true;
        	this.lblSpeedUnit.Location = new System.Drawing.Point(8, 187);
        	this.lblSpeedUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        	this.lblSpeedUnit.Name = "lblSpeedUnit";
        	this.lblSpeedUnit.Size = new System.Drawing.Size(167, 17);
        	this.lblSpeedUnit.TabIndex = 19;
        	this.lblSpeedUnit.Text = "Preferred unit for speed :";
        	// 
        	// chkDeinterlace
        	// 
        	this.chkDeinterlace.Location = new System.Drawing.Point(17, 274);
        	this.chkDeinterlace.Margin = new System.Windows.Forms.Padding(4);
        	this.chkDeinterlace.Name = "chkDeinterlace";
        	this.chkDeinterlace.Size = new System.Drawing.Size(492, 25);
        	this.chkDeinterlace.TabIndex = 18;
        	this.chkDeinterlace.Text = "dlgPreferences_DeinterlaceByDefault";
        	this.chkDeinterlace.UseVisualStyleBackColor = true;
        	this.chkDeinterlace.CheckedChanged += new System.EventHandler(this.ChkDeinterlaceCheckedChanged);
        	// 
        	// cmbImageFormats
        	// 
        	this.cmbImageFormats.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        	this.cmbImageFormats.Location = new System.Drawing.Point(265, 230);
        	this.cmbImageFormats.Margin = new System.Windows.Forms.Padding(4);
        	this.cmbImageFormats.Name = "cmbImageFormats";
        	this.cmbImageFormats.Size = new System.Drawing.Size(243, 24);
        	this.cmbImageFormats.TabIndex = 17;
        	this.cmbImageFormats.SelectedIndexChanged += new System.EventHandler(this.cmbImageAspectRatio_SelectedIndexChanged);
        	// 
        	// lblImageFormat
        	// 
        	this.lblImageFormat.AutoSize = true;
        	this.lblImageFormat.Location = new System.Drawing.Point(8, 234);
        	this.lblImageFormat.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        	this.lblImageFormat.Name = "lblImageFormat";
        	this.lblImageFormat.Size = new System.Drawing.Size(147, 17);
        	this.lblImageFormat.TabIndex = 16;
        	this.lblImageFormat.Text = "Default image format :";
        	// 
        	// cmbTimeCodeFormat
        	// 
        	this.cmbTimeCodeFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        	this.cmbTimeCodeFormat.Location = new System.Drawing.Point(265, 140);
        	this.cmbTimeCodeFormat.Margin = new System.Windows.Forms.Padding(4);
        	this.cmbTimeCodeFormat.Name = "cmbTimeCodeFormat";
        	this.cmbTimeCodeFormat.Size = new System.Drawing.Size(244, 24);
        	this.cmbTimeCodeFormat.TabIndex = 15;
        	this.cmbTimeCodeFormat.SelectedIndexChanged += new System.EventHandler(this.cmbTimeCodeFormat_SelectedIndexChanged);
        	// 
        	// lblTimeMarkersFormat
        	// 
        	this.lblTimeMarkersFormat.AutoSize = true;
        	this.lblTimeMarkersFormat.Location = new System.Drawing.Point(8, 144);
        	this.lblTimeMarkersFormat.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        	this.lblTimeMarkersFormat.Name = "lblTimeMarkersFormat";
        	this.lblTimeMarkersFormat.Size = new System.Drawing.Size(146, 17);
        	this.lblTimeMarkersFormat.TabIndex = 12;
        	this.lblTimeMarkersFormat.Text = "Time markers format :";
        	// 
        	// cmbHistoryCount
        	// 
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
        	this.cmbHistoryCount.Location = new System.Drawing.Point(463, 90);
        	this.cmbHistoryCount.Margin = new System.Windows.Forms.Padding(4);
        	this.cmbHistoryCount.Name = "cmbHistoryCount";
        	this.cmbHistoryCount.Size = new System.Drawing.Size(47, 24);
        	this.cmbHistoryCount.TabIndex = 10;
        	this.cmbHistoryCount.Text = "5";
        	this.cmbHistoryCount.SelectedIndexChanged += new System.EventHandler(this.cmbHistoryCount_SelectedIndexChanged);
        	// 
        	// lblLanguage
        	// 
        	this.lblLanguage.AutoSize = true;
        	this.lblLanguage.Location = new System.Drawing.Point(8, 46);
        	this.lblLanguage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        	this.lblLanguage.Name = "lblLanguage";
        	this.lblLanguage.Size = new System.Drawing.Size(80, 17);
        	this.lblLanguage.TabIndex = 9;
        	this.lblLanguage.Text = "Language :";
        	// 
        	// lblHistoryCount
        	// 
        	this.lblHistoryCount.AutoSize = true;
        	this.lblHistoryCount.Location = new System.Drawing.Point(8, 94);
        	this.lblHistoryCount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        	this.lblHistoryCount.Name = "lblHistoryCount";
        	this.lblHistoryCount.Size = new System.Drawing.Size(216, 17);
        	this.lblHistoryCount.TabIndex = 10;
        	this.lblHistoryCount.Text = "Number of files in recent history :";
        	// 
        	// cmbLanguage
        	// 
        	this.cmbLanguage.FormattingEnabled = true;
        	this.cmbLanguage.Items.AddRange(new object[] {
        	        	        	"English",
        	        	        	"Français"});
        	this.cmbLanguage.Location = new System.Drawing.Point(372, 46);
        	this.cmbLanguage.Margin = new System.Windows.Forms.Padding(4);
        	this.cmbLanguage.Name = "cmbLanguage";
        	this.cmbLanguage.Size = new System.Drawing.Size(137, 24);
        	this.cmbLanguage.TabIndex = 5;
        	this.cmbLanguage.Text = "Français";
        	this.cmbLanguage.SelectedIndexChanged += new System.EventHandler(this.cmbLanguage_SelectedIndexChanged);
        	// 
        	// pagePlayAnalyze
        	// 
        	this.pagePlayAnalyze.BackColor = System.Drawing.Color.White;
        	this.pagePlayAnalyze.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.pagePlayAnalyze.Controls.Add(this.grpColors);
        	this.pagePlayAnalyze.Controls.Add(this.grpSwitchToAnalysis);
        	this.pagePlayAnalyze.Location = new System.Drawing.Point(207, 409);
        	this.pagePlayAnalyze.Margin = new System.Windows.Forms.Padding(4);
        	this.pagePlayAnalyze.Name = "pagePlayAnalyze";
        	this.pagePlayAnalyze.Size = new System.Drawing.Size(578, 356);
        	this.pagePlayAnalyze.TabIndex = 2;
        	// 
        	// grpColors
        	// 
        	this.grpColors.Controls.Add(this.lblPlane3D);
        	this.grpColors.Controls.Add(this.lblGrid);
        	this.grpColors.Controls.Add(this.btn3DPlaneColor);
        	this.grpColors.Controls.Add(this.btnGridColor);
        	this.grpColors.Location = new System.Drawing.Point(17, 11);
        	this.grpColors.Margin = new System.Windows.Forms.Padding(4);
        	this.grpColors.Name = "grpColors";
        	this.grpColors.Padding = new System.Windows.Forms.Padding(4);
        	this.grpColors.Size = new System.Drawing.Size(540, 91);
        	this.grpColors.TabIndex = 20;
        	this.grpColors.TabStop = false;
        	this.grpColors.Text = "Colors";
        	// 
        	// lblPlane3D
        	// 
        	this.lblPlane3D.AutoSize = true;
        	this.lblPlane3D.Location = new System.Drawing.Point(15, 63);
        	this.lblPlane3D.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        	this.lblPlane3D.Name = "lblPlane3D";
        	this.lblPlane3D.Size = new System.Drawing.Size(74, 17);
        	this.lblPlane3D.TabIndex = 13;
        	this.lblPlane3D.Text = "3D Plane :";
        	// 
        	// lblGrid
        	// 
        	this.lblGrid.AutoSize = true;
        	this.lblGrid.Location = new System.Drawing.Point(15, 28);
        	this.lblGrid.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        	this.lblGrid.Name = "lblGrid";
        	this.lblGrid.Size = new System.Drawing.Size(43, 17);
        	this.lblGrid.TabIndex = 11;
        	this.lblGrid.Text = "Grid :";
        	// 
        	// btn3DPlaneColor
        	// 
        	this.btn3DPlaneColor.Anchor = System.Windows.Forms.AnchorStyles.Right;
        	this.btn3DPlaneColor.BackColor = System.Drawing.Color.Black;
        	this.btn3DPlaneColor.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btn3DPlaneColor.FlatAppearance.BorderColor = System.Drawing.Color.LightGray;
        	this.btn3DPlaneColor.FlatAppearance.BorderSize = 0;
        	this.btn3DPlaneColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btn3DPlaneColor.Location = new System.Drawing.Point(431, 54);
        	this.btn3DPlaneColor.Margin = new System.Windows.Forms.Padding(4);
        	this.btn3DPlaneColor.Name = "btn3DPlaneColor";
        	this.btn3DPlaneColor.Size = new System.Drawing.Size(80, 25);
        	this.btn3DPlaneColor.TabIndex = 25;
        	this.btn3DPlaneColor.UseVisualStyleBackColor = false;
        	this.btn3DPlaneColor.Click += new System.EventHandler(this.btn3DPlaneColor_Click);
        	// 
        	// btnGridColor
        	// 
        	this.btnGridColor.Anchor = System.Windows.Forms.AnchorStyles.Right;
        	this.btnGridColor.BackColor = System.Drawing.Color.Black;
        	this.btnGridColor.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnGridColor.FlatAppearance.BorderColor = System.Drawing.Color.LightGray;
        	this.btnGridColor.FlatAppearance.BorderSize = 0;
        	this.btnGridColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnGridColor.Location = new System.Drawing.Point(431, 20);
        	this.btnGridColor.Margin = new System.Windows.Forms.Padding(4);
        	this.btnGridColor.Name = "btnGridColor";
        	this.btnGridColor.Size = new System.Drawing.Size(80, 25);
        	this.btnGridColor.TabIndex = 20;
        	this.btnGridColor.UseVisualStyleBackColor = false;
        	this.btnGridColor.Click += new System.EventHandler(this.btnGridColor_Click);
        	// 
        	// grpSwitchToAnalysis
        	// 
        	this.grpSwitchToAnalysis.Controls.Add(this.lblWorkingZoneLogic);
        	this.grpSwitchToAnalysis.Controls.Add(this.trkWorkingZoneSeconds);
        	this.grpSwitchToAnalysis.Controls.Add(this.trkWorkingZoneMemory);
        	this.grpSwitchToAnalysis.Controls.Add(this.lblWorkingZoneMemory);
        	this.grpSwitchToAnalysis.Controls.Add(this.lblWorkingZoneSeconds);
        	this.grpSwitchToAnalysis.Location = new System.Drawing.Point(17, 110);
        	this.grpSwitchToAnalysis.Margin = new System.Windows.Forms.Padding(4);
        	this.grpSwitchToAnalysis.Name = "grpSwitchToAnalysis";
        	this.grpSwitchToAnalysis.Padding = new System.Windows.Forms.Padding(4);
        	this.grpSwitchToAnalysis.Size = new System.Drawing.Size(540, 225);
        	this.grpSwitchToAnalysis.TabIndex = 19;
        	this.grpSwitchToAnalysis.TabStop = false;
        	this.grpSwitchToAnalysis.Text = "Switch to Analysis Mode";
        	// 
        	// lblWorkingZoneLogic
        	// 
        	this.lblWorkingZoneLogic.AutoSize = true;
        	this.lblWorkingZoneLogic.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.lblWorkingZoneLogic.Location = new System.Drawing.Point(13, 97);
        	this.lblWorkingZoneLogic.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        	this.lblWorkingZoneLogic.Name = "lblWorkingZoneLogic";
        	this.lblWorkingZoneLogic.Size = new System.Drawing.Size(36, 17);
        	this.lblWorkingZoneLogic.TabIndex = 19;
        	this.lblWorkingZoneLogic.Text = "And";
        	// 
        	// trkWorkingZoneSeconds
        	// 
        	this.trkWorkingZoneSeconds.Location = new System.Drawing.Point(17, 53);
        	this.trkWorkingZoneSeconds.Margin = new System.Windows.Forms.Padding(4);
        	this.trkWorkingZoneSeconds.Maximum = 30;
        	this.trkWorkingZoneSeconds.Minimum = 1;
        	this.trkWorkingZoneSeconds.Name = "trkWorkingZoneSeconds";
        	this.trkWorkingZoneSeconds.Size = new System.Drawing.Size(515, 56);
        	this.trkWorkingZoneSeconds.TabIndex = 30;
        	this.trkWorkingZoneSeconds.Value = 12;
        	this.trkWorkingZoneSeconds.ValueChanged += new System.EventHandler(this.trkWorkingZoneSeconds_ValueChanged);
        	// 
        	// trkWorkingZoneMemory
        	// 
        	this.trkWorkingZoneMemory.Location = new System.Drawing.Point(12, 145);
        	this.trkWorkingZoneMemory.Margin = new System.Windows.Forms.Padding(4);
        	this.trkWorkingZoneMemory.Maximum = 1024;
        	this.trkWorkingZoneMemory.Minimum = 16;
        	this.trkWorkingZoneMemory.Name = "trkWorkingZoneMemory";
        	this.trkWorkingZoneMemory.Size = new System.Drawing.Size(520, 56);
        	this.trkWorkingZoneMemory.TabIndex = 35;
        	this.trkWorkingZoneMemory.TickFrequency = 50;
        	this.trkWorkingZoneMemory.Value = 512;
        	this.trkWorkingZoneMemory.ValueChanged += new System.EventHandler(this.trkWorkingZoneMemory_ValueChanged);
        	// 
        	// lblWorkingZoneMemory
        	// 
        	this.lblWorkingZoneMemory.AutoSize = true;
        	this.lblWorkingZoneMemory.Location = new System.Drawing.Point(13, 126);
        	this.lblWorkingZoneMemory.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        	this.lblWorkingZoneMemory.Name = "lblWorkingZoneMemory";
        	this.lblWorkingZoneMemory.Size = new System.Drawing.Size(339, 17);
        	this.lblWorkingZoneMemory.TabIndex = 17;
        	this.lblWorkingZoneMemory.Text = "Working Zone will take less than 512 Mib of Memory.";
        	// 
        	// lblWorkingZoneSeconds
        	// 
        	this.lblWorkingZoneSeconds.AutoSize = true;
        	this.lblWorkingZoneSeconds.Location = new System.Drawing.Point(13, 32);
        	this.lblWorkingZoneSeconds.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        	this.lblWorkingZoneSeconds.Name = "lblWorkingZoneSeconds";
        	this.lblWorkingZoneSeconds.Size = new System.Drawing.Size(253, 17);
        	this.lblWorkingZoneSeconds.TabIndex = 15;
        	this.lblWorkingZoneSeconds.Text = "Working Zone is less than 12 seconds.";
        	// 
        	// btnSave
        	// 
        	this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnSave.Location = new System.Drawing.Point(549, 374);
        	this.btnSave.Margin = new System.Windows.Forms.Padding(4);
        	this.btnSave.Name = "btnSave";
        	this.btnSave.Size = new System.Drawing.Size(113, 27);
        	this.btnSave.TabIndex = 55;
        	this.btnSave.Text = "Save";
        	this.btnSave.UseVisualStyleBackColor = true;
        	this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
        	// 
        	// btnCancel
        	// 
        	this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnCancel.Location = new System.Drawing.Point(672, 374);
        	this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
        	this.btnCancel.Name = "btnCancel";
        	this.btnCancel.Size = new System.Drawing.Size(113, 27);
        	this.btnCancel.TabIndex = 60;
        	this.btnCancel.Text = "Cancel";
        	this.btnCancel.UseVisualStyleBackColor = true;
        	this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
        	// 
        	// pageDrawings
        	// 
        	this.pageDrawings.BackColor = System.Drawing.Color.White;
        	this.pageDrawings.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.pageDrawings.Controls.Add(this.chkDrawOnPlay);
        	this.pageDrawings.Controls.Add(this.grpDrawingsFading);
        	this.pageDrawings.Location = new System.Drawing.Point(207, 773);
        	this.pageDrawings.Margin = new System.Windows.Forms.Padding(4);
        	this.pageDrawings.Name = "pageDrawings";
        	this.pageDrawings.Size = new System.Drawing.Size(578, 356);
        	this.pageDrawings.TabIndex = 11;
        	// 
        	// chkDrawOnPlay
        	// 
        	this.chkDrawOnPlay.AutoSize = true;
        	this.chkDrawOnPlay.Location = new System.Drawing.Point(35, 305);
        	this.chkDrawOnPlay.Margin = new System.Windows.Forms.Padding(4);
        	this.chkDrawOnPlay.Name = "chkDrawOnPlay";
        	this.chkDrawOnPlay.Size = new System.Drawing.Size(262, 21);
        	this.chkDrawOnPlay.TabIndex = 50;
        	this.chkDrawOnPlay.Text = "Show drawings when video is playing";
        	this.chkDrawOnPlay.UseVisualStyleBackColor = true;
        	this.chkDrawOnPlay.CheckedChanged += new System.EventHandler(this.chkDrawOnPlay_CheckedChanged);
        	// 
        	// grpDrawingsFading
        	// 
        	this.grpDrawingsFading.Controls.Add(this.chkAlwaysVisible);
        	this.grpDrawingsFading.Controls.Add(this.lblFading);
        	this.grpDrawingsFading.Controls.Add(this.trkFading);
        	this.grpDrawingsFading.Controls.Add(this.chkEnablePersistence);
        	this.grpDrawingsFading.Location = new System.Drawing.Point(17, 22);
        	this.grpDrawingsFading.Margin = new System.Windows.Forms.Padding(4);
        	this.grpDrawingsFading.Name = "grpDrawingsFading";
        	this.grpDrawingsFading.Padding = new System.Windows.Forms.Padding(4);
        	this.grpDrawingsFading.Size = new System.Drawing.Size(540, 251);
        	this.grpDrawingsFading.TabIndex = 19;
        	this.grpDrawingsFading.TabStop = false;
        	this.grpDrawingsFading.Text = "Persistence";
        	// 
        	// chkAlwaysVisible
        	// 
        	this.chkAlwaysVisible.AutoSize = true;
        	this.chkAlwaysVisible.Location = new System.Drawing.Point(19, 206);
        	this.chkAlwaysVisible.Margin = new System.Windows.Forms.Padding(4);
        	this.chkAlwaysVisible.Name = "chkAlwaysVisible";
        	this.chkAlwaysVisible.Size = new System.Drawing.Size(116, 21);
        	this.chkAlwaysVisible.TabIndex = 51;
        	this.chkAlwaysVisible.Text = "Always visible";
        	this.chkAlwaysVisible.UseVisualStyleBackColor = true;
        	this.chkAlwaysVisible.CheckedChanged += new System.EventHandler(this.chkAlwaysVisible_CheckedChanged);
        	// 
        	// lblFading
        	// 
        	this.lblFading.Location = new System.Drawing.Point(13, 85);
        	this.lblFading.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        	this.lblFading.Name = "lblFading";
        	this.lblFading.Size = new System.Drawing.Size(483, 39);
        	this.lblFading.TabIndex = 20;
        	this.lblFading.Text = "By default, drawings will stay visible for 12 images around the Key Image. kdjfns" +
        	"kdjbnsdkjbksdjvbdvkbj";
        	this.lblFading.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
        	// 
        	// trkFading
        	// 
        	this.trkFading.Location = new System.Drawing.Point(19, 132);
        	this.trkFading.Margin = new System.Windows.Forms.Padding(4);
        	this.trkFading.Maximum = 60;
        	this.trkFading.Minimum = 1;
        	this.trkFading.Name = "trkFading";
        	this.trkFading.Size = new System.Drawing.Size(479, 56);
        	this.trkFading.TabIndex = 45;
        	this.trkFading.Value = 5;
        	this.trkFading.ValueChanged += new System.EventHandler(this.trkFading_ValueChanged);
        	// 
        	// chkEnablePersistence
        	// 
        	this.chkEnablePersistence.AutoSize = true;
        	this.chkEnablePersistence.Location = new System.Drawing.Point(17, 42);
        	this.chkEnablePersistence.Margin = new System.Windows.Forms.Padding(4);
        	this.chkEnablePersistence.Name = "chkEnablePersistence";
        	this.chkEnablePersistence.Size = new System.Drawing.Size(151, 21);
        	this.chkEnablePersistence.TabIndex = 40;
        	this.chkEnablePersistence.Text = "Enable persistence";
        	this.chkEnablePersistence.UseVisualStyleBackColor = true;
        	this.chkEnablePersistence.CheckedChanged += new System.EventHandler(this.chkFading_CheckedChanged);
        	// 
        	// formPreferences
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackColor = System.Drawing.Color.White;
        	this.ClientSize = new System.Drawing.Size(803, 407);
        	this.Controls.Add(this.pageDrawings);
        	this.Controls.Add(this.btnSave);
        	this.Controls.Add(this.btnCancel);
        	this.Controls.Add(this.pagePlayAnalyze);
        	this.Controls.Add(this.pageGeneral);
        	this.Controls.Add(this.panel1);
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
        	this.Margin = new System.Windows.Forms.Padding(4);
        	this.MaximizeBox = false;
        	this.MinimizeBox = false;
        	this.Name = "formPreferences";
        	this.ShowInTaskbar = false;
        	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        	this.Text = "   Preferences...";
        	this.panel1.ResumeLayout(false);
        	this.pageGeneral.ResumeLayout(false);
        	this.grpGeneral.ResumeLayout(false);
        	this.grpGeneral.PerformLayout();
        	this.pagePlayAnalyze.ResumeLayout(false);
        	this.grpColors.ResumeLayout(false);
        	this.grpColors.PerformLayout();
        	this.grpSwitchToAnalysis.ResumeLayout(false);
        	this.grpSwitchToAnalysis.PerformLayout();
        	((System.ComponentModel.ISupportInitialize)(this.trkWorkingZoneSeconds)).EndInit();
        	((System.ComponentModel.ISupportInitialize)(this.trkWorkingZoneMemory)).EndInit();
        	this.pageDrawings.ResumeLayout(false);
        	this.pageDrawings.PerformLayout();
        	this.grpDrawingsFading.ResumeLayout(false);
        	this.grpDrawingsFading.PerformLayout();
        	((System.ComponentModel.ISupportInitialize)(this.trkFading)).EndInit();
        	this.ResumeLayout(false);
        }
        private System.Windows.Forms.CheckBox chkAlwaysVisible;
        private System.Windows.Forms.Label lblSpeedUnit;
        private System.Windows.Forms.ComboBox cmbSpeedUnit;
        private System.Windows.Forms.CheckBox chkDeinterlace;
        private System.Windows.Forms.Label lblImageFormat;
        private System.Windows.Forms.ComboBox cmbImageFormats;

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnGeneral;
        private System.Windows.Forms.Button btnPlayAnalyze;
        private System.Windows.Forms.Panel pageGeneral;
        private System.Windows.Forms.Label lblLanguage;
        private System.Windows.Forms.ComboBox cmbLanguage;
        private System.Windows.Forms.ComboBox cmbHistoryCount;
        private System.Windows.Forms.Label lblHistoryCount;
        private System.Windows.Forms.Panel pagePlayAnalyze;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnGridColor;
        private System.Windows.Forms.Label lblPlane3D;
        private System.Windows.Forms.Button btn3DPlaneColor;
        private System.Windows.Forms.Label lblGrid;
        private System.Windows.Forms.TrackBar trkWorkingZoneMemory;
        private System.Windows.Forms.Label lblWorkingZoneMemory;
        private System.Windows.Forms.TrackBar trkWorkingZoneSeconds;
        private System.Windows.Forms.Label lblWorkingZoneSeconds;
        private System.Windows.Forms.GroupBox grpColors;
        private System.Windows.Forms.GroupBox grpSwitchToAnalysis;
        private System.Windows.Forms.GroupBox grpGeneral;
        
        private System.Windows.Forms.Label lblTimeMarkersFormat;
        private System.Windows.Forms.Label lblWorkingZoneLogic;
        private System.Windows.Forms.ComboBox cmbTimeCodeFormat;
        private System.Windows.Forms.Button btnDrawings;
        private System.Windows.Forms.Panel pageDrawings;
        private System.Windows.Forms.GroupBox grpDrawingsFading;
        private System.Windows.Forms.CheckBox chkDrawOnPlay;
        private System.Windows.Forms.TrackBar trkFading;
        private System.Windows.Forms.CheckBox chkEnablePersistence;
        private System.Windows.Forms.Label lblFading;
    }
}