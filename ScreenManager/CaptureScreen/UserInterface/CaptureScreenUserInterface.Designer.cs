namespace Kinovea.ScreenManager
{
    partial class CaptureScreenUserInterface
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        	this.components = new System.ComponentModel.Container();
        	this.panelTop = new System.Windows.Forms.Panel();
        	this.lblFileName = new System.Windows.Forms.Label();
        	this.btnClose = new System.Windows.Forms.Button();
        	this.lblSelDuration = new System.Windows.Forms.Label();
        	this.panelVideoControls = new System.Windows.Forms.Panel();
        	this.btnRafale = new System.Windows.Forms.Button();
        	this.lblWorkingZone = new System.Windows.Forms.Label();
        	this.btnSnapShot = new System.Windows.Forms.Button();
        	this.lblTimeCode = new System.Windows.Forms.Label();
        	this.buttonGotoFirst = new System.Windows.Forms.Button();
        	this.buttonGotoPrevious = new System.Windows.Forms.Button();
        	this.buttonGotoNext = new System.Windows.Forms.Button();
        	this.buttonGotoLast = new System.Windows.Forms.Button();
        	this.buttonPlay = new System.Windows.Forms.Button();
        	this.lblSpeedTuner = new System.Windows.Forms.Label();
        	this.groupBoxSpeedTuner = new System.Windows.Forms.GroupBox();
        	this.markerSpeedTuner = new System.Windows.Forms.Button();
        	this.PrimarySelection = new System.Windows.Forms.Button();
        	this._panelCenter = new System.Windows.Forms.Panel();
        	this.ImageResizerNE = new System.Windows.Forms.Label();
        	this.ImageResizerNW = new System.Windows.Forms.Label();
        	this.ImageResizerSW = new System.Windows.Forms.Label();
        	this.ImageResizerSE = new System.Windows.Forms.Label();
        	this.panelDebug = new System.Windows.Forms.Panel();
        	this.dbgAvailableRam = new System.Windows.Forms.Label();
        	this.dbgDurationFrames = new System.Windows.Forms.Label();
        	this.dbgCurrentFrame = new System.Windows.Forms.Label();
        	this.dbgCurrentPositionRel = new System.Windows.Forms.Label();
        	this.dbgStartOffset = new System.Windows.Forms.Label();
        	this.dbgCurrentPositionAbs = new System.Windows.Forms.Label();
        	this.dbgDrops = new System.Windows.Forms.Label();
        	this.dbgSelectionDuration = new System.Windows.Forms.Label();
        	this.dbgSelectionEnd = new System.Windows.Forms.Label();
        	this.dbgSelectionStart = new System.Windows.Forms.Label();
        	this.dbgFFps = new System.Windows.Forms.Label();
        	this.dbgDurationTimeStamps = new System.Windows.Forms.Label();
        	this._surfaceScreen = new System.Windows.Forms.PictureBox();
        	this.ActiveScreenIndicator = new System.Windows.Forms.Label();
        	this.toolTips = new System.Windows.Forms.ToolTip(this.components);
        	this.splitKeyframes = new System.Windows.Forms.SplitContainer();
        	this.btn3dplane = new System.Windows.Forms.Button();
        	this.btnMagnifier = new System.Windows.Forms.Button();
        	this.btnDrawingToolChrono = new System.Windows.Forms.Button();
        	this.btnDockBottom = new System.Windows.Forms.Button();
        	this.btnDrawingToolCross2D = new System.Windows.Forms.Button();
        	this.btnShowComments = new System.Windows.Forms.Button();
        	this.btnDrawingToolLine2D = new System.Windows.Forms.Button();
        	this.btnColorProfile = new System.Windows.Forms.Button();
        	this.btnDrawingToolText = new System.Windows.Forms.Button();
        	this.btnDrawingToolPencil = new System.Windows.Forms.Button();
        	this.btnDrawingToolAngle2D = new System.Windows.Forms.Button();
        	this.btnDrawingToolPointer = new System.Windows.Forms.Button();
        	this.pnlThumbnails = new System.Windows.Forms.Panel();
        	this.pictureBox1 = new System.Windows.Forms.PictureBox();
        	this.btnAddKeyframe = new System.Windows.Forms.Button();
        	this.buttonRecord = new System.Windows.Forms.Button();
        	this.panelTop.SuspendLayout();
        	this.panelVideoControls.SuspendLayout();
        	this.groupBoxSpeedTuner.SuspendLayout();
        	this._panelCenter.SuspendLayout();
        	this.panelDebug.SuspendLayout();
        	((System.ComponentModel.ISupportInitialize)(this._surfaceScreen)).BeginInit();
        	this.splitKeyframes.Panel1.SuspendLayout();
        	this.splitKeyframes.Panel2.SuspendLayout();
        	this.splitKeyframes.SuspendLayout();
        	this.pnlThumbnails.SuspendLayout();
        	((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
        	this.SuspendLayout();
        	// 
        	// panelTop
        	// 
        	this.panelTop.BackColor = System.Drawing.Color.White;
        	this.panelTop.Controls.Add(this.lblFileName);
        	this.panelTop.Controls.Add(this.btnClose);
        	this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
        	this.panelTop.Location = new System.Drawing.Point(0, 0);
        	this.panelTop.Name = "panelTop";
        	this.panelTop.Size = new System.Drawing.Size(350, 19);
        	this.panelTop.TabIndex = 0;
        	// 
        	// lblFileName
        	// 
        	this.lblFileName.AutoSize = true;
        	this.lblFileName.Location = new System.Drawing.Point(3, 4);
        	this.lblFileName.Name = "lblFileName";
        	this.lblFileName.Size = new System.Drawing.Size(67, 13);
        	this.lblFileName.TabIndex = 3;
        	this.lblFileName.Text = "";
        	// 
        	// btnClose
        	// 
        	this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnClose.BackColor = System.Drawing.Color.Transparent;
        	this.btnClose.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.closegrey;
        	this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnClose.Cursor = System.Windows.Forms.Cursors.Default;
        	this.btnClose.FlatAppearance.BorderSize = 0;
        	this.btnClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
        	this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnClose.Location = new System.Drawing.Point(328, -1);
        	this.btnClose.Name = "btnClose";
        	this.btnClose.Size = new System.Drawing.Size(20, 20);
        	this.btnClose.TabIndex = 2;
        	this.btnClose.UseVisualStyleBackColor = false;
        	this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
        	// 
        	// lblSelDuration
        	// 
        	this.lblSelDuration.BackColor = System.Drawing.Color.Transparent;
        	this.lblSelDuration.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.lblSelDuration.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.lblSelDuration.Location = new System.Drawing.Point(104, 7);
        	this.lblSelDuration.Name = "lblSelDuration";
        	this.lblSelDuration.Size = new System.Drawing.Size(91, 15);
        	this.lblSelDuration.TabIndex = 4;
        	this.lblSelDuration.Text = "Durée : 0:00:00:00";
        	// 
        	// panelVideoControls
        	// 
        	this.panelVideoControls.BackColor = System.Drawing.Color.White;
        	this.panelVideoControls.Controls.Add(this.buttonRecord);
        	this.panelVideoControls.Controls.Add(this.btnRafale);
        	this.panelVideoControls.Controls.Add(this.lblWorkingZone);
        	this.panelVideoControls.Controls.Add(this.btnSnapShot);
        	this.panelVideoControls.Controls.Add(this.lblTimeCode);
        	this.panelVideoControls.Controls.Add(this.buttonGotoFirst);
        	this.panelVideoControls.Controls.Add(this.buttonGotoPrevious);
        	this.panelVideoControls.Controls.Add(this.buttonGotoNext);
        	this.panelVideoControls.Controls.Add(this.lblSelDuration);
        	this.panelVideoControls.Controls.Add(this.buttonGotoLast);
        	this.panelVideoControls.Controls.Add(this.buttonPlay);
        	this.panelVideoControls.Controls.Add(this.lblSpeedTuner);
        	this.panelVideoControls.Dock = System.Windows.Forms.DockStyle.Bottom;
        	this.panelVideoControls.Location = new System.Drawing.Point(0, 410);
        	this.panelVideoControls.MinimumSize = new System.Drawing.Size(175, 100);
        	this.panelVideoControls.Name = "panelVideoControls";
        	this.panelVideoControls.Size = new System.Drawing.Size(350, 100);
        	this.panelVideoControls.TabIndex = 2;
        	this.panelVideoControls.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.Common_MouseWheel);
        	this.panelVideoControls.MouseEnter += new System.EventHandler(this.PanelVideoControls_MouseEnter);
        	// 
        	// btnRafale
        	// 
        	this.btnRafale.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnRafale.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        	this.btnRafale.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnRafale.FlatAppearance.BorderSize = 0;
        	this.btnRafale.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnRafale.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnRafale.Image = global::Kinovea.ScreenManager.Properties.Resources.snaprafale;
        	this.btnRafale.Location = new System.Drawing.Point(310, 68);
        	this.btnRafale.MinimumSize = new System.Drawing.Size(25, 25);
        	this.btnRafale.Name = "btnRafale";
        	this.btnRafale.Size = new System.Drawing.Size(30, 25);
        	this.btnRafale.TabIndex = 23;
        	this.btnRafale.Tag = "";
        	this.btnRafale.UseVisualStyleBackColor = true;
        	this.btnRafale.Click += new System.EventHandler(this.btnRafale_Click);
        	// 
        	// lblWorkingZone
        	// 
        	this.lblWorkingZone.AutoSize = true;
        	this.lblWorkingZone.BackColor = System.Drawing.Color.White;
        	this.lblWorkingZone.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.lblWorkingZone.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.lblWorkingZone.ForeColor = System.Drawing.Color.Black;
        	this.lblWorkingZone.Location = new System.Drawing.Point(14, 7);
        	this.lblWorkingZone.Margin = new System.Windows.Forms.Padding(0);
        	this.lblWorkingZone.Name = "lblWorkingZone";
        	this.lblWorkingZone.Size = new System.Drawing.Size(75, 12);
        	this.lblWorkingZone.TabIndex = 19;
        	this.lblWorkingZone.Text = "Zone de Travail : ";
        	// 
        	// btnSnapShot
        	// 
        	this.btnSnapShot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnSnapShot.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        	this.btnSnapShot.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnSnapShot.FlatAppearance.BorderSize = 0;
        	this.btnSnapShot.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnSnapShot.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnSnapShot.Image = global::Kinovea.ScreenManager.Properties.Resources.screencap5;
        	this.btnSnapShot.Location = new System.Drawing.Point(280, 68);
        	this.btnSnapShot.MinimumSize = new System.Drawing.Size(25, 25);
        	this.btnSnapShot.Name = "btnSnapShot";
        	this.btnSnapShot.Size = new System.Drawing.Size(30, 25);
        	this.btnSnapShot.TabIndex = 18;
        	this.btnSnapShot.Tag = "";
        	this.btnSnapShot.UseVisualStyleBackColor = true;
        	this.btnSnapShot.Click += new System.EventHandler(this.btnSnapShot_Click);
        	// 
        	// lblTimeCode
        	// 
        	this.lblTimeCode.AutoSize = true;
        	this.lblTimeCode.BackColor = System.Drawing.Color.Transparent;
        	this.lblTimeCode.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.lblTimeCode.Location = new System.Drawing.Point(14, 44);
        	this.lblTimeCode.Name = "lblTimeCode";
        	this.lblTimeCode.Size = new System.Drawing.Size(89, 12);
        	this.lblTimeCode.TabIndex = 2;
        	this.lblTimeCode.Text = "Position : 0:00:00:00";
        	this.lblTimeCode.DoubleClick += new System.EventHandler(this.lblTimeCode_DoubleClick);
        	// 
        	// buttonGotoFirst
        	// 
        	this.buttonGotoFirst.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.liqfirst7;
        	this.buttonGotoFirst.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
        	this.buttonGotoFirst.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.buttonGotoFirst.FlatAppearance.BorderColor = System.Drawing.Color.White;
        	this.buttonGotoFirst.FlatAppearance.BorderSize = 0;
        	this.buttonGotoFirst.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
        	this.buttonGotoFirst.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
        	this.buttonGotoFirst.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.buttonGotoFirst.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
        	this.buttonGotoFirst.Location = new System.Drawing.Point(310, 5);
        	this.buttonGotoFirst.MinimumSize = new System.Drawing.Size(25, 25);
        	this.buttonGotoFirst.Name = "buttonGotoFirst";
        	this.buttonGotoFirst.Size = new System.Drawing.Size(30, 25);
        	this.buttonGotoFirst.TabIndex = 4;
        	this.buttonGotoFirst.UseVisualStyleBackColor = true;
        	this.buttonGotoFirst.Visible = false;
        	this.buttonGotoFirst.Click += new System.EventHandler(this.buttonGotoFirst_Click);
        	// 
        	// buttonGotoPrevious
        	// 
        	this.buttonGotoPrevious.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.liqprev5;
        	this.buttonGotoPrevious.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
        	this.buttonGotoPrevious.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.buttonGotoPrevious.FlatAppearance.BorderSize = 0;
        	this.buttonGotoPrevious.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
        	this.buttonGotoPrevious.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
        	this.buttonGotoPrevious.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.buttonGotoPrevious.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
        	this.buttonGotoPrevious.Location = new System.Drawing.Point(292, 7);
        	this.buttonGotoPrevious.MinimumSize = new System.Drawing.Size(25, 25);
        	this.buttonGotoPrevious.Name = "buttonGotoPrevious";
        	this.buttonGotoPrevious.Size = new System.Drawing.Size(30, 25);
        	this.buttonGotoPrevious.TabIndex = 3;
        	this.buttonGotoPrevious.UseVisualStyleBackColor = true;
        	this.buttonGotoPrevious.Visible = false;
        	this.buttonGotoPrevious.Click += new System.EventHandler(this.buttonGotoPrevious_Click);
        	// 
        	// buttonGotoNext
        	// 
        	this.buttonGotoNext.BackColor = System.Drawing.Color.Transparent;
        	this.buttonGotoNext.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.liqnext6;
        	this.buttonGotoNext.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
        	this.buttonGotoNext.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.buttonGotoNext.FlatAppearance.BorderSize = 0;
        	this.buttonGotoNext.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
        	this.buttonGotoNext.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
        	this.buttonGotoNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.buttonGotoNext.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
        	this.buttonGotoNext.Location = new System.Drawing.Point(275, 7);
        	this.buttonGotoNext.MinimumSize = new System.Drawing.Size(25, 25);
        	this.buttonGotoNext.Name = "buttonGotoNext";
        	this.buttonGotoNext.Size = new System.Drawing.Size(30, 25);
        	this.buttonGotoNext.TabIndex = 2;
        	this.buttonGotoNext.UseVisualStyleBackColor = false;
        	this.buttonGotoNext.Visible = false;
        	this.buttonGotoNext.Click += new System.EventHandler(this.buttonGotoNext_Click);
        	// 
        	// buttonGotoLast
        	// 
        	this.buttonGotoLast.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.liqlast5;
        	this.buttonGotoLast.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
        	this.buttonGotoLast.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.buttonGotoLast.FlatAppearance.BorderSize = 0;
        	this.buttonGotoLast.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
        	this.buttonGotoLast.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
        	this.buttonGotoLast.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.buttonGotoLast.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
        	this.buttonGotoLast.Location = new System.Drawing.Point(264, 7);
        	this.buttonGotoLast.MinimumSize = new System.Drawing.Size(25, 25);
        	this.buttonGotoLast.Name = "buttonGotoLast";
        	this.buttonGotoLast.Size = new System.Drawing.Size(30, 25);
        	this.buttonGotoLast.TabIndex = 1;
        	this.buttonGotoLast.UseVisualStyleBackColor = true;
        	this.buttonGotoLast.Visible = false;
        	this.buttonGotoLast.Click += new System.EventHandler(this.buttonGotoLast_Click);
        	// 
        	// buttonPlay
        	// 
        	this.buttonPlay.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.liqplay17;
        	this.buttonPlay.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
        	this.buttonPlay.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.buttonPlay.FlatAppearance.BorderSize = 0;
        	this.buttonPlay.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
        	this.buttonPlay.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
        	this.buttonPlay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.buttonPlay.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
        	this.buttonPlay.Location = new System.Drawing.Point(15, 65);
        	this.buttonPlay.MinimumSize = new System.Drawing.Size(30, 25);
        	this.buttonPlay.Name = "buttonPlay";
        	this.buttonPlay.Size = new System.Drawing.Size(40, 30);
        	this.buttonPlay.TabIndex = 0;
        	this.buttonPlay.UseVisualStyleBackColor = true;
        	this.buttonPlay.Click += new System.EventHandler(this.buttonPlay_Click);
        	// 
        	// lblSpeedTuner
        	// 
        	this.lblSpeedTuner.AutoSize = true;
        	this.lblSpeedTuner.BackColor = System.Drawing.Color.White;
        	this.lblSpeedTuner.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.lblSpeedTuner.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.lblSpeedTuner.ForeColor = System.Drawing.Color.Black;
        	this.lblSpeedTuner.Location = new System.Drawing.Point(120, 44);
        	this.lblSpeedTuner.Margin = new System.Windows.Forms.Padding(0);
        	this.lblSpeedTuner.Name = "lblSpeedTuner";
        	this.lblSpeedTuner.Size = new System.Drawing.Size(67, 12);
        	this.lblSpeedTuner.TabIndex = 10;
        	this.lblSpeedTuner.Text = "Vitesse : 100%";
        	this.lblSpeedTuner.DoubleClick += new System.EventHandler(this.lblSpeedTuner_DoubleClick);
        	// 
        	// groupBoxSpeedTuner
        	// 
        	this.groupBoxSpeedTuner.BackColor = System.Drawing.Color.White;
        	this.groupBoxSpeedTuner.Controls.Add(this.markerSpeedTuner);
        	this.groupBoxSpeedTuner.Controls.Add(this.PrimarySelection);
        	this.groupBoxSpeedTuner.Location = new System.Drawing.Point(25, 375);
        	this.groupBoxSpeedTuner.MaximumSize = new System.Drawing.Size(300, 0);
        	this.groupBoxSpeedTuner.Name = "groupBoxSpeedTuner";
        	this.groupBoxSpeedTuner.Size = new System.Drawing.Size(172, 0);
        	this.groupBoxSpeedTuner.TabIndex = 0;
        	this.groupBoxSpeedTuner.TabStop = false;
        	this.groupBoxSpeedTuner.Text = "Playback Speed : 100%";
        	// 
        	// markerSpeedTuner
        	// 
        	this.markerSpeedTuner.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
        	this.markerSpeedTuner.FlatAppearance.BorderSize = 0;
        	this.markerSpeedTuner.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
        	this.markerSpeedTuner.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
        	this.markerSpeedTuner.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.markerSpeedTuner.Location = new System.Drawing.Point(110, 51);
        	this.markerSpeedTuner.Name = "markerSpeedTuner";
        	this.markerSpeedTuner.Size = new System.Drawing.Size(2, 3);
        	this.markerSpeedTuner.TabIndex = 1;
        	this.markerSpeedTuner.UseVisualStyleBackColor = false;
        	// 
        	// PrimarySelection
        	// 
        	this.PrimarySelection.BackColor = System.Drawing.Color.Maroon;
        	this.PrimarySelection.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
        	this.PrimarySelection.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
        	this.PrimarySelection.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.PrimarySelection.Location = new System.Drawing.Point(1, -7);
        	this.PrimarySelection.Name = "PrimarySelection";
        	this.PrimarySelection.Size = new System.Drawing.Size(150, 14);
        	this.PrimarySelection.TabIndex = 0;
        	this.PrimarySelection.UseVisualStyleBackColor = false;
        	// 
        	// _panelCenter
        	// 
        	this._panelCenter.BackColor = System.Drawing.Color.Black;
        	this._panelCenter.Controls.Add(this.ImageResizerNE);
        	this._panelCenter.Controls.Add(this.ImageResizerNW);
        	this._panelCenter.Controls.Add(this.ImageResizerSW);
        	this._panelCenter.Controls.Add(this.ImageResizerSE);
        	this._panelCenter.Controls.Add(this.panelDebug);
        	this._panelCenter.Controls.Add(this._surfaceScreen);
        	this._panelCenter.Controls.Add(this.ActiveScreenIndicator);
        	this._panelCenter.Dock = System.Windows.Forms.DockStyle.Fill;
        	this._panelCenter.Location = new System.Drawing.Point(0, 0);
        	this._panelCenter.MinimumSize = new System.Drawing.Size(350, 25);
        	this._panelCenter.Name = "_panelCenter";
        	this._panelCenter.Size = new System.Drawing.Size(350, 250);
        	this._panelCenter.TabIndex = 2;
        	this._panelCenter.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.Common_MouseWheel);
        	this._panelCenter.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PanelCenter_MouseClick);
        	this._panelCenter.Resize += new System.EventHandler(this.PanelCenter_Resize);
        	this._panelCenter.MouseEnter += new System.EventHandler(this.PanelCenter_MouseEnter);
        	// 
        	// ImageResizerNE
        	// 
        	this.ImageResizerNE.Anchor = System.Windows.Forms.AnchorStyles.None;
        	this.ImageResizerNE.BackColor = System.Drawing.Color.DimGray;
        	this.ImageResizerNE.Cursor = System.Windows.Forms.Cursors.SizeNESW;
        	this.ImageResizerNE.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.ImageResizerNE.Image = global::Kinovea.ScreenManager.Properties.Resources.resizer4;
        	this.ImageResizerNE.Location = new System.Drawing.Point(92, 78);
        	this.ImageResizerNE.Name = "ImageResizerNE";
        	this.ImageResizerNE.Size = new System.Drawing.Size(6, 6);
        	this.ImageResizerNE.TabIndex = 9;
        	this.ImageResizerNE.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ImageResizerNE_MouseMove);
        	this.ImageResizerNE.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.Resizers_MouseDoubleClick);
        	// 
        	// ImageResizerNW
        	// 
        	this.ImageResizerNW.Anchor = System.Windows.Forms.AnchorStyles.None;
        	this.ImageResizerNW.BackColor = System.Drawing.Color.DimGray;
        	this.ImageResizerNW.Cursor = System.Windows.Forms.Cursors.SizeNWSE;
        	this.ImageResizerNW.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.ImageResizerNW.Image = global::Kinovea.ScreenManager.Properties.Resources.resizer4;
        	this.ImageResizerNW.Location = new System.Drawing.Point(57, 78);
        	this.ImageResizerNW.Name = "ImageResizerNW";
        	this.ImageResizerNW.Size = new System.Drawing.Size(6, 6);
        	this.ImageResizerNW.TabIndex = 8;
        	this.ImageResizerNW.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ImageResizerNW_MouseMove);
        	this.ImageResizerNW.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.Resizers_MouseDoubleClick);
        	// 
        	// ImageResizerSW
        	// 
        	this.ImageResizerSW.Anchor = System.Windows.Forms.AnchorStyles.None;
        	this.ImageResizerSW.BackColor = System.Drawing.Color.DimGray;
        	this.ImageResizerSW.Cursor = System.Windows.Forms.Cursors.SizeNESW;
        	this.ImageResizerSW.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.ImageResizerSW.Image = global::Kinovea.ScreenManager.Properties.Resources.resizer4;
        	this.ImageResizerSW.Location = new System.Drawing.Point(57, 103);
        	this.ImageResizerSW.Name = "ImageResizerSW";
        	this.ImageResizerSW.Size = new System.Drawing.Size(6, 6);
        	this.ImageResizerSW.TabIndex = 7;
        	this.ImageResizerSW.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ImageResizerSW_MouseMove);
        	this.ImageResizerSW.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.Resizers_MouseDoubleClick);
        	// 
        	// ImageResizerSE
        	// 
        	this.ImageResizerSE.Anchor = System.Windows.Forms.AnchorStyles.None;
        	this.ImageResizerSE.BackColor = System.Drawing.Color.DimGray;
        	this.ImageResizerSE.Cursor = System.Windows.Forms.Cursors.SizeNWSE;
        	this.ImageResizerSE.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.ImageResizerSE.ForeColor = System.Drawing.Color.Transparent;
        	this.ImageResizerSE.Image = global::Kinovea.ScreenManager.Properties.Resources.resizer4;
        	this.ImageResizerSE.Location = new System.Drawing.Point(92, 103);
        	this.ImageResizerSE.Name = "ImageResizerSE";
        	this.ImageResizerSE.Size = new System.Drawing.Size(6, 6);
        	this.ImageResizerSE.TabIndex = 6;
        	this.ImageResizerSE.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ImageResizerSE_MouseMove);
        	this.ImageResizerSE.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.Resizers_MouseDoubleClick);
        	// 
        	// panelDebug
        	// 
        	this.panelDebug.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
        	this.panelDebug.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.panelDebug.Controls.Add(this.dbgAvailableRam);
        	this.panelDebug.Controls.Add(this.dbgDurationFrames);
        	this.panelDebug.Controls.Add(this.dbgCurrentFrame);
        	this.panelDebug.Controls.Add(this.dbgCurrentPositionRel);
        	this.panelDebug.Controls.Add(this.dbgStartOffset);
        	this.panelDebug.Controls.Add(this.dbgCurrentPositionAbs);
        	this.panelDebug.Controls.Add(this.dbgDrops);
        	this.panelDebug.Controls.Add(this.dbgSelectionDuration);
        	this.panelDebug.Controls.Add(this.dbgSelectionEnd);
        	this.panelDebug.Controls.Add(this.dbgSelectionStart);
        	this.panelDebug.Controls.Add(this.dbgFFps);
        	this.panelDebug.Controls.Add(this.groupBoxSpeedTuner);
        	this.panelDebug.Controls.Add(this.dbgDurationTimeStamps);
        	this.panelDebug.Location = new System.Drawing.Point(240, 15);
        	this.panelDebug.Name = "panelDebug";
        	this.panelDebug.Size = new System.Drawing.Size(100, 174);
        	this.panelDebug.TabIndex = 1;
        	this.panelDebug.Visible = false;
        	this.panelDebug.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.panelDebug_MouseDoubleClick);
        	// 
        	// dbgAvailableRam
        	// 
        	this.dbgAvailableRam.AutoSize = true;
        	this.dbgAvailableRam.Font = new System.Drawing.Font("Vrinda", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.dbgAvailableRam.ForeColor = System.Drawing.Color.White;
        	this.dbgAvailableRam.Location = new System.Drawing.Point(3, 148);
        	this.dbgAvailableRam.Name = "dbgAvailableRam";
        	this.dbgAvailableRam.Size = new System.Drawing.Size(64, 17);
        	this.dbgAvailableRam.TabIndex = 24;
        	this.dbgAvailableRam.Text = "iAvailableRam";
        	// 
        	// dbgDurationFrames
        	// 
        	this.dbgDurationFrames.AutoSize = true;
        	this.dbgDurationFrames.Font = new System.Drawing.Font("Vrinda", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.dbgDurationFrames.ForeColor = System.Drawing.Color.White;
        	this.dbgDurationFrames.Location = new System.Drawing.Point(3, 134);
        	this.dbgDurationFrames.Name = "dbgDurationFrames";
        	this.dbgDurationFrames.Size = new System.Drawing.Size(72, 17);
        	this.dbgDurationFrames.TabIndex = 23;
        	this.dbgDurationFrames.Text = "iDurationFrames";
        	// 
        	// dbgCurrentFrame
        	// 
        	this.dbgCurrentFrame.AutoSize = true;
        	this.dbgCurrentFrame.Font = new System.Drawing.Font("Vrinda", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.dbgCurrentFrame.ForeColor = System.Drawing.Color.White;
        	this.dbgCurrentFrame.Location = new System.Drawing.Point(3, 120);
        	this.dbgCurrentFrame.Name = "dbgCurrentFrame";
        	this.dbgCurrentFrame.Size = new System.Drawing.Size(64, 17);
        	this.dbgCurrentFrame.TabIndex = 22;
        	this.dbgCurrentFrame.Text = "iCurrentFrame";
        	// 
        	// dbgCurrentPositionRel
        	// 
        	this.dbgCurrentPositionRel.AutoSize = true;
        	this.dbgCurrentPositionRel.Font = new System.Drawing.Font("Vrinda", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.dbgCurrentPositionRel.ForeColor = System.Drawing.Color.White;
        	this.dbgCurrentPositionRel.Location = new System.Drawing.Point(3, 106);
        	this.dbgCurrentPositionRel.Name = "dbgCurrentPositionRel";
        	this.dbgCurrentPositionRel.Size = new System.Drawing.Size(78, 17);
        	this.dbgCurrentPositionRel.TabIndex = 18;
        	this.dbgCurrentPositionRel.Text = "iCurrentPos(rel)";
        	// 
        	// dbgStartOffset
        	// 
        	this.dbgStartOffset.AutoSize = true;
        	this.dbgStartOffset.Font = new System.Drawing.Font("Vrinda", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.dbgStartOffset.ForeColor = System.Drawing.Color.White;
        	this.dbgStartOffset.Location = new System.Drawing.Point(3, 41);
        	this.dbgStartOffset.Name = "dbgStartOffset";
        	this.dbgStartOffset.Size = new System.Drawing.Size(50, 17);
        	this.dbgStartOffset.TabIndex = 17;
        	this.dbgStartOffset.Text = "iStartOffset";
        	// 
        	// dbgCurrentPositionAbs
        	// 
        	this.dbgCurrentPositionAbs.AutoSize = true;
        	this.dbgCurrentPositionAbs.Font = new System.Drawing.Font("Vrinda", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.dbgCurrentPositionAbs.ForeColor = System.Drawing.Color.White;
        	this.dbgCurrentPositionAbs.Location = new System.Drawing.Point(3, 93);
        	this.dbgCurrentPositionAbs.Name = "dbgCurrentPositionAbs";
        	this.dbgCurrentPositionAbs.Size = new System.Drawing.Size(82, 17);
        	this.dbgCurrentPositionAbs.TabIndex = 16;
        	this.dbgCurrentPositionAbs.Text = "iCurrentPos(abs)";
        	// 
        	// dbgDrops
        	// 
        	this.dbgDrops.AutoSize = true;
        	this.dbgDrops.Font = new System.Drawing.Font("Vrinda", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.dbgDrops.ForeColor = System.Drawing.Color.White;
        	this.dbgDrops.Location = new System.Drawing.Point(3, 13);
        	this.dbgDrops.Name = "dbgDrops";
        	this.dbgDrops.Size = new System.Drawing.Size(37, 17);
        	this.dbgDrops.TabIndex = 7;
        	this.dbgDrops.Text = "Drops ";
        	// 
        	// dbgSelectionDuration
        	// 
        	this.dbgSelectionDuration.AutoSize = true;
        	this.dbgSelectionDuration.Font = new System.Drawing.Font("Vrinda", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.dbgSelectionDuration.ForeColor = System.Drawing.Color.White;
        	this.dbgSelectionDuration.Location = new System.Drawing.Point(3, 79);
        	this.dbgSelectionDuration.Name = "dbgSelectionDuration";
        	this.dbgSelectionDuration.Size = new System.Drawing.Size(56, 17);
        	this.dbgSelectionDuration.TabIndex = 15;
        	this.dbgSelectionDuration.Text = "iSelDuration";
        	// 
        	// dbgSelectionEnd
        	// 
        	this.dbgSelectionEnd.AutoSize = true;
        	this.dbgSelectionEnd.Font = new System.Drawing.Font("Vrinda", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.dbgSelectionEnd.ForeColor = System.Drawing.Color.White;
        	this.dbgSelectionEnd.Location = new System.Drawing.Point(3, 66);
        	this.dbgSelectionEnd.Name = "dbgSelectionEnd";
        	this.dbgSelectionEnd.Size = new System.Drawing.Size(39, 17);
        	this.dbgSelectionEnd.TabIndex = 14;
        	this.dbgSelectionEnd.Text = "iSelEnd";
        	// 
        	// dbgSelectionStart
        	// 
        	this.dbgSelectionStart.AutoSize = true;
        	this.dbgSelectionStart.Font = new System.Drawing.Font("Vrinda", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.dbgSelectionStart.ForeColor = System.Drawing.Color.White;
        	this.dbgSelectionStart.Location = new System.Drawing.Point(3, 54);
        	this.dbgSelectionStart.Name = "dbgSelectionStart";
        	this.dbgSelectionStart.Size = new System.Drawing.Size(41, 17);
        	this.dbgSelectionStart.TabIndex = 13;
        	this.dbgSelectionStart.Text = "iSelStart";
        	// 
        	// dbgFFps
        	// 
        	this.dbgFFps.AutoSize = true;
        	this.dbgFFps.Font = new System.Drawing.Font("Vrinda", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.dbgFFps.ForeColor = System.Drawing.Color.White;
        	this.dbgFFps.Location = new System.Drawing.Point(3, -1);
        	this.dbgFFps.Name = "dbgFFps";
        	this.dbgFFps.Size = new System.Drawing.Size(24, 17);
        	this.dbgFFps.TabIndex = 12;
        	this.dbgFFps.Text = "fFps";
        	// 
        	// dbgDurationTimeStamps
        	// 
        	this.dbgDurationTimeStamps.AutoSize = true;
        	this.dbgDurationTimeStamps.Font = new System.Drawing.Font("Vrinda", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.dbgDurationTimeStamps.ForeColor = System.Drawing.Color.White;
        	this.dbgDurationTimeStamps.Location = new System.Drawing.Point(3, 28);
        	this.dbgDurationTimeStamps.Name = "dbgDurationTimeStamps";
        	this.dbgDurationTimeStamps.Size = new System.Drawing.Size(91, 17);
        	this.dbgDurationTimeStamps.TabIndex = 11;
        	this.dbgDurationTimeStamps.Text = "iDurationTimeStamps";
        	// 
        	// _surfaceScreen
        	// 
        	this._surfaceScreen.Cursor = System.Windows.Forms.Cursors.Arrow;
        	this._surfaceScreen.Location = new System.Drawing.Point(43, 29);
        	this._surfaceScreen.Name = "_surfaceScreen";
        	this._surfaceScreen.Size = new System.Drawing.Size(101, 73);
        	this._surfaceScreen.TabIndex = 2;
        	this._surfaceScreen.TabStop = false;
        	this._surfaceScreen.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.Common_MouseWheel);
        	this._surfaceScreen.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SurfaceScreen_MouseMove);
        	this._surfaceScreen.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.SurfaceScreen_MouseDoubleClick);
        	this._surfaceScreen.MouseDown += new System.Windows.Forms.MouseEventHandler(this._surfaceScreen_MouseDown);
        	this._surfaceScreen.Paint += new System.Windows.Forms.PaintEventHandler(this.SurfaceScreen_Paint);
        	this._surfaceScreen.MouseUp += new System.Windows.Forms.MouseEventHandler(this.SurfaceScreen_MouseUp);
        	this._surfaceScreen.MouseEnter += new System.EventHandler(this.SurfaceScreen_MouseEnter);
        	// 
        	// ActiveScreenIndicator
        	// 
        	this.ActiveScreenIndicator.BackColor = System.Drawing.Color.Transparent;
        	this.ActiveScreenIndicator.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.ActiveScreenIndicator.Image = global::Kinovea.ScreenManager.Properties.Resources.activepencil;
        	this.ActiveScreenIndicator.Location = new System.Drawing.Point(1, 3);
        	this.ActiveScreenIndicator.Margin = new System.Windows.Forms.Padding(0);
        	this.ActiveScreenIndicator.Name = "ActiveScreenIndicator";
        	this.ActiveScreenIndicator.Size = new System.Drawing.Size(16, 16);
        	this.ActiveScreenIndicator.TabIndex = 3;
        	this.ActiveScreenIndicator.Visible = false;
        	// 
        	// splitKeyframes
        	// 
        	this.splitKeyframes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
        	this.splitKeyframes.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.splitKeyframes.IsSplitterFixed = true;
        	this.splitKeyframes.Location = new System.Drawing.Point(0, 19);
        	this.splitKeyframes.Margin = new System.Windows.Forms.Padding(0);
        	this.splitKeyframes.Name = "splitKeyframes";
        	this.splitKeyframes.Orientation = System.Windows.Forms.Orientation.Horizontal;
        	// 
        	// splitKeyframes.Panel1
        	// 
        	this.splitKeyframes.Panel1.Controls.Add(this._panelCenter);
        	// 
        	// splitKeyframes.Panel2
        	// 
        	this.splitKeyframes.Panel2.BackColor = System.Drawing.Color.White;
        	this.splitKeyframes.Panel2.Controls.Add(this.btn3dplane);
        	this.splitKeyframes.Panel2.Controls.Add(this.btnMagnifier);
        	this.splitKeyframes.Panel2.Controls.Add(this.btnDrawingToolChrono);
        	this.splitKeyframes.Panel2.Controls.Add(this.btnDockBottom);
        	this.splitKeyframes.Panel2.Controls.Add(this.btnDrawingToolCross2D);
        	this.splitKeyframes.Panel2.Controls.Add(this.btnShowComments);
        	this.splitKeyframes.Panel2.Controls.Add(this.btnDrawingToolLine2D);
        	this.splitKeyframes.Panel2.Controls.Add(this.btnColorProfile);
        	this.splitKeyframes.Panel2.Controls.Add(this.btnDrawingToolText);
        	this.splitKeyframes.Panel2.Controls.Add(this.btnDrawingToolPencil);
        	this.splitKeyframes.Panel2.Controls.Add(this.btnDrawingToolAngle2D);
        	this.splitKeyframes.Panel2.Controls.Add(this.btnDrawingToolPointer);
        	this.splitKeyframes.Panel2.Controls.Add(this.pnlThumbnails);
        	this.splitKeyframes.Panel2.Controls.Add(this.btnAddKeyframe);
        	this.splitKeyframes.Panel2.DoubleClick += new System.EventHandler(this.splitKeyframes_Panel2_DoubleClick);
        	this.splitKeyframes.Panel2MinSize = 30;
        	this.splitKeyframes.Size = new System.Drawing.Size(350, 391);
        	this.splitKeyframes.SplitterDistance = 250;
        	this.splitKeyframes.SplitterWidth = 2;
        	this.splitKeyframes.TabIndex = 10;
        	this.splitKeyframes.Resize += new System.EventHandler(this.splitKeyframes_Resize);
        	// 
        	// btn3dplane
        	// 
        	this.btn3dplane.BackColor = System.Drawing.Color.Transparent;
        	this.btn3dplane.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.plane4;
        	this.btn3dplane.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btn3dplane.FlatAppearance.BorderSize = 0;
        	this.btn3dplane.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btn3dplane.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btn3dplane.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btn3dplane.ForeColor = System.Drawing.Color.Black;
        	this.btn3dplane.Location = new System.Drawing.Point(250, 2);
        	this.btn3dplane.Name = "btn3dplane";
        	this.btn3dplane.Size = new System.Drawing.Size(25, 25);
        	this.btn3dplane.TabIndex = 19;
        	this.btn3dplane.UseVisualStyleBackColor = false;
        	this.btn3dplane.Click += new System.EventHandler(this.btn3dplane_Click);
        	// 
        	// btnMagnifier
        	// 
        	this.btnMagnifier.BackColor = System.Drawing.Color.Transparent;
        	this.btnMagnifier.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.magnifier2;
        	this.btnMagnifier.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnMagnifier.FlatAppearance.BorderSize = 0;
        	this.btnMagnifier.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnMagnifier.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnMagnifier.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnMagnifier.ForeColor = System.Drawing.Color.Black;
        	this.btnMagnifier.Location = new System.Drawing.Point(275, 2);
        	this.btnMagnifier.Name = "btnMagnifier";
        	this.btnMagnifier.Size = new System.Drawing.Size(25, 25);
        	this.btnMagnifier.TabIndex = 18;
        	this.btnMagnifier.UseVisualStyleBackColor = false;
        	this.btnMagnifier.Click += new System.EventHandler(this.btnMagnifier_Click);
        	// 
        	// btnDrawingToolChrono
        	// 
        	this.btnDrawingToolChrono.BackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolChrono.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.chrono5;
        	this.btnDrawingToolChrono.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnDrawingToolChrono.FlatAppearance.BorderSize = 0;
        	this.btnDrawingToolChrono.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolChrono.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawingToolChrono.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnDrawingToolChrono.ForeColor = System.Drawing.Color.Black;
        	this.btnDrawingToolChrono.Location = new System.Drawing.Point(220, 2);
        	this.btnDrawingToolChrono.Name = "btnDrawingToolChrono";
        	this.btnDrawingToolChrono.Size = new System.Drawing.Size(25, 25);
        	this.btnDrawingToolChrono.TabIndex = 17;
        	this.btnDrawingToolChrono.UseVisualStyleBackColor = false;
        	this.btnDrawingToolChrono.Click += new System.EventHandler(this.btnDrawingToolChrono_Click);
        	// 
        	// btnDockBottom
        	// 
        	this.btnDockBottom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnDockBottom.BackColor = System.Drawing.Color.Transparent;
        	this.btnDockBottom.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.dock16x16;
        	this.btnDockBottom.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnDockBottom.Cursor = System.Windows.Forms.Cursors.Default;
        	this.btnDockBottom.FlatAppearance.BorderSize = 0;
        	this.btnDockBottom.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnDockBottom.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
        	this.btnDockBottom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnDockBottom.Location = new System.Drawing.Point(328, 4);
        	this.btnDockBottom.Name = "btnDockBottom";
        	this.btnDockBottom.Size = new System.Drawing.Size(20, 20);
        	this.btnDockBottom.TabIndex = 16;
        	this.btnDockBottom.UseVisualStyleBackColor = false;
        	this.btnDockBottom.Visible = false;
        	this.btnDockBottom.Click += new System.EventHandler(this.btnDockBottom_Click);
        	// 
        	// btnDrawingToolCross2D
        	// 
        	this.btnDrawingToolCross2D.BackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolCross2D.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.cross5;
        	this.btnDrawingToolCross2D.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnDrawingToolCross2D.FlatAppearance.BorderSize = 0;
        	this.btnDrawingToolCross2D.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolCross2D.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawingToolCross2D.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnDrawingToolCross2D.ForeColor = System.Drawing.Color.Black;
        	this.btnDrawingToolCross2D.Location = new System.Drawing.Point(170, 2);
        	this.btnDrawingToolCross2D.Name = "btnDrawingToolCross2D";
        	this.btnDrawingToolCross2D.Size = new System.Drawing.Size(25, 25);
        	this.btnDrawingToolCross2D.TabIndex = 7;
        	this.btnDrawingToolCross2D.UseVisualStyleBackColor = false;
        	this.btnDrawingToolCross2D.Click += new System.EventHandler(this.btnDrawingToolCross2D_Click);
        	// 
        	// btnShowComments
        	// 
        	this.btnShowComments.BackColor = System.Drawing.Color.Transparent;
        	this.btnShowComments.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.comments2;
        	this.btnShowComments.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnShowComments.FlatAppearance.BorderSize = 0;
        	this.btnShowComments.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnShowComments.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnShowComments.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnShowComments.ForeColor = System.Drawing.Color.Black;
        	this.btnShowComments.Location = new System.Drawing.Point(70, 2);
        	this.btnShowComments.Name = "btnShowComments";
        	this.btnShowComments.Size = new System.Drawing.Size(25, 25);
        	this.btnShowComments.TabIndex = 14;
        	this.btnShowComments.UseVisualStyleBackColor = false;
        	this.btnShowComments.Click += new System.EventHandler(this.btnShowComments_Click);
        	// 
        	// btnDrawingToolLine2D
        	// 
        	this.btnDrawingToolLine2D.BackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolLine2D.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.line6;
        	this.btnDrawingToolLine2D.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnDrawingToolLine2D.FlatAppearance.BorderSize = 0;
        	this.btnDrawingToolLine2D.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolLine2D.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawingToolLine2D.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnDrawingToolLine2D.ForeColor = System.Drawing.Color.Black;
        	this.btnDrawingToolLine2D.Location = new System.Drawing.Point(145, 2);
        	this.btnDrawingToolLine2D.Name = "btnDrawingToolLine2D";
        	this.btnDrawingToolLine2D.Size = new System.Drawing.Size(25, 25);
        	this.btnDrawingToolLine2D.TabIndex = 4;
        	this.btnDrawingToolLine2D.UseVisualStyleBackColor = false;
        	this.btnDrawingToolLine2D.Click += new System.EventHandler(this.btnDrawingToolLine2D_Click);
        	// 
        	// btnColorProfile
        	// 
        	this.btnColorProfile.BackColor = System.Drawing.Color.Transparent;
        	this.btnColorProfile.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.SwatchIcon3;
        	this.btnColorProfile.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnColorProfile.FlatAppearance.BorderSize = 0;
        	this.btnColorProfile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnColorProfile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnColorProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnColorProfile.ForeColor = System.Drawing.Color.Black;
        	this.btnColorProfile.Location = new System.Drawing.Point(300, 2);
        	this.btnColorProfile.Name = "btnColorProfile";
        	this.btnColorProfile.Size = new System.Drawing.Size(25, 25);
        	this.btnColorProfile.TabIndex = 15;
        	this.btnColorProfile.UseVisualStyleBackColor = false;
        	this.btnColorProfile.Click += new System.EventHandler(this.btnColorProfile_Click);
        	// 
        	// btnDrawingToolText
        	// 
        	this.btnDrawingToolText.BackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolText.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.TextToolIcon;
        	this.btnDrawingToolText.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnDrawingToolText.FlatAppearance.BorderSize = 0;
        	this.btnDrawingToolText.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolText.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawingToolText.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnDrawingToolText.ForeColor = System.Drawing.Color.Black;
        	this.btnDrawingToolText.Location = new System.Drawing.Point(95, 2);
        	this.btnDrawingToolText.Name = "btnDrawingToolText";
        	this.btnDrawingToolText.Size = new System.Drawing.Size(25, 25);
        	this.btnDrawingToolText.TabIndex = 10;
        	this.btnDrawingToolText.UseVisualStyleBackColor = false;
        	this.btnDrawingToolText.Click += new System.EventHandler(this.btnDrawingToolText_Click);
        	// 
        	// btnDrawingToolPencil
        	// 
        	this.btnDrawingToolPencil.BackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolPencil.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.activepencil;
        	this.btnDrawingToolPencil.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnDrawingToolPencil.FlatAppearance.BorderSize = 0;
        	this.btnDrawingToolPencil.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolPencil.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawingToolPencil.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnDrawingToolPencil.ForeColor = System.Drawing.Color.Black;
        	this.btnDrawingToolPencil.Location = new System.Drawing.Point(120, 2);
        	this.btnDrawingToolPencil.Name = "btnDrawingToolPencil";
        	this.btnDrawingToolPencil.Size = new System.Drawing.Size(25, 25);
        	this.btnDrawingToolPencil.TabIndex = 9;
        	this.btnDrawingToolPencil.UseVisualStyleBackColor = false;
        	this.btnDrawingToolPencil.Click += new System.EventHandler(this.btnDrawingToolPencil_Click);
        	// 
        	// btnDrawingToolAngle2D
        	// 
        	this.btnDrawingToolAngle2D.BackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolAngle2D.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.angle5;
        	this.btnDrawingToolAngle2D.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        	this.btnDrawingToolAngle2D.FlatAppearance.BorderSize = 0;
        	this.btnDrawingToolAngle2D.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolAngle2D.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawingToolAngle2D.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnDrawingToolAngle2D.ForeColor = System.Drawing.Color.Black;
        	this.btnDrawingToolAngle2D.Location = new System.Drawing.Point(195, 2);
        	this.btnDrawingToolAngle2D.Name = "btnDrawingToolAngle2D";
        	this.btnDrawingToolAngle2D.Size = new System.Drawing.Size(25, 25);
        	this.btnDrawingToolAngle2D.TabIndex = 8;
        	this.btnDrawingToolAngle2D.UseVisualStyleBackColor = false;
        	this.btnDrawingToolAngle2D.Click += new System.EventHandler(this.btnDrawingToolAngle2D_Click);
        	// 
        	// btnDrawingToolPointer
        	// 
        	this.btnDrawingToolPointer.BackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolPointer.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.move;
        	this.btnDrawingToolPointer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnDrawingToolPointer.FlatAppearance.BorderSize = 0;
        	this.btnDrawingToolPointer.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolPointer.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawingToolPointer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnDrawingToolPointer.ForeColor = System.Drawing.Color.White;
        	this.btnDrawingToolPointer.Location = new System.Drawing.Point(30, 2);
        	this.btnDrawingToolPointer.Name = "btnDrawingToolPointer";
        	this.btnDrawingToolPointer.Size = new System.Drawing.Size(25, 25);
        	this.btnDrawingToolPointer.TabIndex = 5;
        	this.btnDrawingToolPointer.UseVisualStyleBackColor = false;
        	this.btnDrawingToolPointer.Click += new System.EventHandler(this.btnDrawingToolPointer_Click);
        	// 
        	// pnlThumbnails
        	// 
        	this.pnlThumbnails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        	        	        	| System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.pnlThumbnails.AutoScroll = true;
        	this.pnlThumbnails.BackColor = System.Drawing.Color.Black;
        	this.pnlThumbnails.Controls.Add(this.pictureBox1);
        	this.pnlThumbnails.Location = new System.Drawing.Point(0, 27);
        	this.pnlThumbnails.Name = "pnlThumbnails";
        	this.pnlThumbnails.Size = new System.Drawing.Size(350, 111);
        	this.pnlThumbnails.TabIndex = 3;
        	this.pnlThumbnails.DoubleClick += new System.EventHandler(this.pnlThumbnails_DoubleClick);
        	// 
        	// pictureBox1
        	// 
        	this.pictureBox1.BackColor = System.Drawing.Color.DimGray;
        	this.pictureBox1.Location = new System.Drawing.Point(5, 8);
        	this.pictureBox1.Name = "pictureBox1";
        	this.pictureBox1.Size = new System.Drawing.Size(100, 75);
        	this.pictureBox1.TabIndex = 2;
        	this.pictureBox1.TabStop = false;
        	this.pictureBox1.Visible = false;
        	// 
        	// btnAddKeyframe
        	// 
        	this.btnAddKeyframe.BackColor = System.Drawing.Color.Transparent;
        	this.btnAddKeyframe.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.MenuFileNewIcon;
        	this.btnAddKeyframe.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnAddKeyframe.FlatAppearance.BorderSize = 0;
        	this.btnAddKeyframe.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnAddKeyframe.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnAddKeyframe.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnAddKeyframe.ForeColor = System.Drawing.Color.White;
        	this.btnAddKeyframe.Location = new System.Drawing.Point(5, 2);
        	this.btnAddKeyframe.Name = "btnAddKeyframe";
        	this.btnAddKeyframe.Size = new System.Drawing.Size(25, 25);
        	this.btnAddKeyframe.TabIndex = 0;
        	this.btnAddKeyframe.UseVisualStyleBackColor = false;
        	this.btnAddKeyframe.Click += new System.EventHandler(this.btnAddKeyframe_Click);
        	// 
        	// buttonRecord
        	// 
        	this.buttonRecord.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.liqplay17;
        	this.buttonRecord.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
        	this.buttonRecord.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.buttonRecord.FlatAppearance.BorderSize = 0;
        	this.buttonRecord.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
        	this.buttonRecord.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
        	this.buttonRecord.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.buttonRecord.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
        	this.buttonRecord.Location = new System.Drawing.Point(58, 65);
        	this.buttonRecord.MinimumSize = new System.Drawing.Size(30, 25);
        	this.buttonRecord.Name = "buttonRecord";
        	this.buttonRecord.Size = new System.Drawing.Size(40, 30);
        	this.buttonRecord.TabIndex = 24;
        	this.buttonRecord.UseVisualStyleBackColor = true;
        	this.buttonRecord.Click += new System.EventHandler(this.buttonRecord_Click);
        	// 
        	// CaptureScreenUserInterface
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackColor = System.Drawing.Color.Gainsboro;
        	this.Controls.Add(this.splitKeyframes);
        	this.Controls.Add(this.panelVideoControls);
        	this.Controls.Add(this.panelTop);
        	this.MinimumSize = new System.Drawing.Size(350, 510);
        	this.Name = "CaptureScreenUserInterface";
        	this.Size = new System.Drawing.Size(350, 510);
        	this.panelTop.ResumeLayout(false);
        	this.panelTop.PerformLayout();
        	this.panelVideoControls.ResumeLayout(false);
        	this.panelVideoControls.PerformLayout();
        	this.groupBoxSpeedTuner.ResumeLayout(false);
        	this._panelCenter.ResumeLayout(false);
        	this.panelDebug.ResumeLayout(false);
        	this.panelDebug.PerformLayout();
        	((System.ComponentModel.ISupportInitialize)(this._surfaceScreen)).EndInit();
        	this.splitKeyframes.Panel1.ResumeLayout(false);
        	this.splitKeyframes.Panel2.ResumeLayout(false);
        	this.splitKeyframes.ResumeLayout(false);
        	this.pnlThumbnails.ResumeLayout(false);
        	((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
        	this.ResumeLayout(false);
        }
        private System.Windows.Forms.Button buttonRecord;

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Panel _panelCenter;
        private System.Windows.Forms.Panel panelVideoControls;
        private System.Windows.Forms.Button buttonPlay;
        private System.Windows.Forms.Button buttonGotoLast;
        private System.Windows.Forms.Button buttonGotoFirst;
        private System.Windows.Forms.Button buttonGotoPrevious;
        private System.Windows.Forms.Button buttonGotoNext;
        private System.Windows.Forms.Panel panelDebug;
        public System.Windows.Forms.Label dbgDrops;
        private System.Windows.Forms.Button PrimarySelection;
        private System.Windows.Forms.Label lblSelDuration;
        private System.Windows.Forms.GroupBox groupBoxSpeedTuner;
        private System.Windows.Forms.Button markerSpeedTuner;
        private System.Windows.Forms.Label ActiveScreenIndicator;
        private System.Windows.Forms.ToolTip toolTips;
        private System.Windows.Forms.Label lblTimeCode;
        private System.Windows.Forms.Label dbgDurationTimeStamps;
        private System.Windows.Forms.Label dbgFFps;
        private System.Windows.Forms.Label dbgCurrentPositionAbs;
        private System.Windows.Forms.Label dbgSelectionDuration;
        private System.Windows.Forms.Label dbgSelectionEnd;
        private System.Windows.Forms.Label dbgSelectionStart;
        private System.Windows.Forms.Label dbgStartOffset;
        private System.Windows.Forms.Label dbgCurrentPositionRel;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.Label lblSpeedTuner;
        
        private System.Windows.Forms.Button btnSnapShot;
        private System.Windows.Forms.Label ImageResizerSE;
        private System.Windows.Forms.Label ImageResizerSW;
        private System.Windows.Forms.Label ImageResizerNE;
        private System.Windows.Forms.Label ImageResizerNW;
        private System.Windows.Forms.Label lblWorkingZone;
        private System.Windows.Forms.Label dbgDurationFrames;
        private System.Windows.Forms.Label dbgCurrentFrame;
        public System.Windows.Forms.PictureBox _surfaceScreen;
        private System.Windows.Forms.Label dbgAvailableRam;
        private System.Windows.Forms.SplitContainer splitKeyframes;
        private System.Windows.Forms.Button btnAddKeyframe;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel pnlThumbnails;
        private System.Windows.Forms.Button btnDrawingToolPointer;
        
        private System.Windows.Forms.Button btnDrawingToolCross2D;
        private System.Windows.Forms.Button btnDrawingToolAngle2D;
        private System.Windows.Forms.Button btnDrawingToolPencil;
        private System.Windows.Forms.Button btnDrawingToolText;
        private System.Windows.Forms.Button btnShowComments;
        private System.Windows.Forms.Button btnColorProfile;
        private System.Windows.Forms.Button btnDockBottom;
        private System.Windows.Forms.Button btnDrawingToolLine2D;
        private System.Windows.Forms.Button btnRafale;
        private System.Windows.Forms.Button btnDrawingToolChrono;
        private System.Windows.Forms.Button btnMagnifier;
        private System.Windows.Forms.Button btn3dplane;
        
    }
}
