namespace Kinovea.ScreenManager
{
    partial class PlayerScreenUserInterface
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
        	this.panel1 = new System.Windows.Forms.Panel();
        	this.btnSnapShot = new System.Windows.Forms.Button();
        	this.btnPausedVideo = new System.Windows.Forms.Button();
        	this.btnSaveVideo = new System.Windows.Forms.Button();
        	this.btnDiaporama = new System.Windows.Forms.Button();
        	this.btnRafale = new System.Windows.Forms.Button();
        	this.btnHandlersReset = new System.Windows.Forms.Button();
        	this.btnSetHandlerRight = new System.Windows.Forms.Button();
        	this.btnSetHandlerLeft = new System.Windows.Forms.Button();
        	this.lblWorkingZone = new System.Windows.Forms.Label();
        	this.trkSelection = new Kinovea.ScreenManager.SelectionTracker();
        	this.trkFrame = new Kinovea.ScreenManager.FrameTracker();
        	this.btn_HandlersLock = new System.Windows.Forms.Button();
        	this.lblSelStartSelection = new System.Windows.Forms.Label();
        	this.lblTimeCode = new System.Windows.Forms.Label();
        	this.buttonGotoFirst = new System.Windows.Forms.Button();
        	this.buttonGotoPrevious = new System.Windows.Forms.Button();
        	this.buttonGotoNext = new System.Windows.Forms.Button();
        	this.buttonPlay = new System.Windows.Forms.Button();
        	this.sldrSpeed = new Kinovea.ScreenManager.SpeedSlider();
        	this.lblSpeedTuner = new System.Windows.Forms.Label();
        	this.buttonGotoLast = new System.Windows.Forms.Button();
        	this.buttonPlayingMode = new System.Windows.Forms.Button();
        	this.btnPdf = new System.Windows.Forms.Button();
        	this.groupBoxSpeedTuner = new System.Windows.Forms.GroupBox();
        	this.markerSpeedTuner = new System.Windows.Forms.Button();
        	this.PrimarySelection = new System.Windows.Forms.Button();
        	this.panelCenter = new System.Windows.Forms.Panel();
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
        	this.pbSurfaceScreen = new System.Windows.Forms.PictureBox();
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
        	this.panelTop.SuspendLayout();
        	this.panelVideoControls.SuspendLayout();
        	this.panel1.SuspendLayout();
        	this.groupBoxSpeedTuner.SuspendLayout();
        	this.panelCenter.SuspendLayout();
        	this.panelDebug.SuspendLayout();
        	((System.ComponentModel.ISupportInitialize)(this.pbSurfaceScreen)).BeginInit();
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
        	this.panelTop.Size = new System.Drawing.Size(420, 19);
        	this.panelTop.TabIndex = 0;
        	// 
        	// lblFileName
        	// 
        	this.lblFileName.AutoSize = true;
        	this.lblFileName.Location = new System.Drawing.Point(3, 4);
        	this.lblFileName.Name = "lblFileName";
        	this.lblFileName.Size = new System.Drawing.Size(0, 13);
        	this.lblFileName.TabIndex = 3;
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
        	this.btnClose.Location = new System.Drawing.Point(398, -1);
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
        	this.lblSelDuration.Location = new System.Drawing.Point(220, 24);
        	this.lblSelDuration.Name = "lblSelDuration";
        	this.lblSelDuration.Size = new System.Drawing.Size(91, 15);
        	this.lblSelDuration.TabIndex = 4;
        	this.lblSelDuration.Text = "Durée : 0:00:00:00";
        	// 
        	// panelVideoControls
        	// 
        	this.panelVideoControls.BackColor = System.Drawing.Color.White;
        	this.panelVideoControls.Controls.Add(this.panel1);
        	this.panelVideoControls.Controls.Add(this.btnHandlersReset);
        	this.panelVideoControls.Controls.Add(this.btnSetHandlerRight);
        	this.panelVideoControls.Controls.Add(this.btnSetHandlerLeft);
        	this.panelVideoControls.Controls.Add(this.lblWorkingZone);
        	this.panelVideoControls.Controls.Add(this.trkSelection);
        	this.panelVideoControls.Controls.Add(this.trkFrame);
        	this.panelVideoControls.Controls.Add(this.btn_HandlersLock);
        	this.panelVideoControls.Controls.Add(this.lblSelStartSelection);
        	this.panelVideoControls.Controls.Add(this.lblTimeCode);
        	this.panelVideoControls.Controls.Add(this.buttonGotoFirst);
        	this.panelVideoControls.Controls.Add(this.buttonGotoPrevious);
        	this.panelVideoControls.Controls.Add(this.buttonGotoNext);
        	this.panelVideoControls.Controls.Add(this.lblSelDuration);
        	this.panelVideoControls.Controls.Add(this.buttonPlay);
        	this.panelVideoControls.Controls.Add(this.sldrSpeed);
        	this.panelVideoControls.Controls.Add(this.lblSpeedTuner);
        	this.panelVideoControls.Controls.Add(this.buttonGotoLast);
        	this.panelVideoControls.Controls.Add(this.buttonPlayingMode);
        	this.panelVideoControls.Dock = System.Windows.Forms.DockStyle.Bottom;
        	this.panelVideoControls.Location = new System.Drawing.Point(0, 386);
        	this.panelVideoControls.MinimumSize = new System.Drawing.Size(175, 100);
        	this.panelVideoControls.Name = "panelVideoControls";
        	this.panelVideoControls.Size = new System.Drawing.Size(420, 124);
        	this.panelVideoControls.TabIndex = 2;
        	this.panelVideoControls.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.Common_MouseWheel);
        	this.panelVideoControls.MouseEnter += new System.EventHandler(this.PanelVideoControls_MouseEnter);
        	// 
        	// panel1
        	// 
        	this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        	this.panel1.BackColor = System.Drawing.Color.White;
        	this.panel1.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.ExportDock5;
        	this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
        	this.panel1.Controls.Add(this.btnSnapShot);
        	this.panel1.Controls.Add(this.btnPausedVideo);
        	this.panel1.Controls.Add(this.btnSaveVideo);
        	this.panel1.Controls.Add(this.btnDiaporama);
        	this.panel1.Controls.Add(this.btnRafale);
        	this.panel1.Location = new System.Drawing.Point(240, 78);
        	this.panel1.Name = "panel1";
        	this.panel1.Size = new System.Drawing.Size(185, 46);
        	this.panel1.TabIndex = 26;
        	// 
        	// btnSnapShot
        	// 
        	this.btnSnapShot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnSnapShot.BackColor = System.Drawing.Color.Transparent;
        	this.btnSnapShot.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        	this.btnSnapShot.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnSnapShot.FlatAppearance.BorderSize = 0;
        	this.btnSnapShot.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
        	this.btnSnapShot.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnSnapShot.Image = global::Kinovea.ScreenManager.Properties.Resources.snapsingle_1;
        	this.btnSnapShot.Location = new System.Drawing.Point(28, 14);
        	this.btnSnapShot.MinimumSize = new System.Drawing.Size(25, 25);
        	this.btnSnapShot.Name = "btnSnapShot";
        	this.btnSnapShot.Size = new System.Drawing.Size(30, 25);
        	this.btnSnapShot.TabIndex = 18;
        	this.btnSnapShot.Tag = "";
        	this.btnSnapShot.UseVisualStyleBackColor = false;
        	this.btnSnapShot.Click += new System.EventHandler(this.btnSnapShot_Click);
        	// 
        	// btnPausedVideo
        	// 
        	this.btnPausedVideo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnPausedVideo.BackColor = System.Drawing.Color.Transparent;
        	this.btnPausedVideo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        	this.btnPausedVideo.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnPausedVideo.FlatAppearance.BorderSize = 0;
        	this.btnPausedVideo.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
        	this.btnPausedVideo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnPausedVideo.Image = global::Kinovea.ScreenManager.Properties.Resources.save_paused_video;
        	this.btnPausedVideo.Location = new System.Drawing.Point(148, 14);
        	this.btnPausedVideo.MinimumSize = new System.Drawing.Size(25, 25);
        	this.btnPausedVideo.Name = "btnPausedVideo";
        	this.btnPausedVideo.Size = new System.Drawing.Size(30, 25);
        	this.btnPausedVideo.TabIndex = 25;
        	this.btnPausedVideo.Tag = "";
        	this.btnPausedVideo.UseVisualStyleBackColor = false;
        	this.btnPausedVideo.Click += new System.EventHandler(this.btnDiaporama_Click);
        	// 
        	// btnSaveVideo
        	// 
        	this.btnSaveVideo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnSaveVideo.BackColor = System.Drawing.Color.Transparent;
        	this.btnSaveVideo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        	this.btnSaveVideo.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnSaveVideo.FlatAppearance.BorderSize = 0;
        	this.btnSaveVideo.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
        	this.btnSaveVideo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnSaveVideo.Image = global::Kinovea.ScreenManager.Properties.Resources.savevideo;
        	this.btnSaveVideo.Location = new System.Drawing.Point(88, 14);
        	this.btnSaveVideo.MinimumSize = new System.Drawing.Size(25, 25);
        	this.btnSaveVideo.Name = "btnSaveVideo";
        	this.btnSaveVideo.Size = new System.Drawing.Size(30, 25);
        	this.btnSaveVideo.TabIndex = 25;
        	this.btnSaveVideo.Tag = "";
        	this.btnSaveVideo.UseVisualStyleBackColor = false;
        	this.btnSaveVideo.Click += new System.EventHandler(this.btnVideo_Click);
        	// 
        	// btnDiaporama
        	// 
        	this.btnDiaporama.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnDiaporama.BackColor = System.Drawing.Color.Transparent;
        	this.btnDiaporama.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        	this.btnDiaporama.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnDiaporama.FlatAppearance.BorderSize = 0;
        	this.btnDiaporama.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
        	this.btnDiaporama.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnDiaporama.Image = global::Kinovea.ScreenManager.Properties.Resources.saveStaticDiaporama;
        	this.btnDiaporama.Location = new System.Drawing.Point(118, 14);
        	this.btnDiaporama.MinimumSize = new System.Drawing.Size(25, 25);
        	this.btnDiaporama.Name = "btnDiaporama";
        	this.btnDiaporama.Size = new System.Drawing.Size(30, 25);
        	this.btnDiaporama.TabIndex = 25;
        	this.btnDiaporama.Tag = "";
        	this.btnDiaporama.UseVisualStyleBackColor = false;
        	this.btnDiaporama.Click += new System.EventHandler(this.btnDiaporama_Click);
        	// 
        	// btnRafale
        	// 
        	this.btnRafale.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnRafale.BackColor = System.Drawing.Color.Transparent;
        	this.btnRafale.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        	this.btnRafale.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnRafale.FlatAppearance.BorderSize = 0;
        	this.btnRafale.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
        	this.btnRafale.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnRafale.Image = global::Kinovea.ScreenManager.Properties.Resources.snapmulti_1;
        	this.btnRafale.Location = new System.Drawing.Point(58, 14);
        	this.btnRafale.MinimumSize = new System.Drawing.Size(25, 25);
        	this.btnRafale.Name = "btnRafale";
        	this.btnRafale.Size = new System.Drawing.Size(30, 25);
        	this.btnRafale.TabIndex = 23;
        	this.btnRafale.Tag = "";
        	this.btnRafale.UseVisualStyleBackColor = false;
        	this.btnRafale.Click += new System.EventHandler(this.btnRafale_Click);
        	// 
        	// btnHandlersReset
        	// 
        	this.btnHandlersReset.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        	this.btnHandlersReset.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnHandlersReset.FlatAppearance.BorderSize = 0;
        	this.btnHandlersReset.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnHandlersReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnHandlersReset.Image = global::Kinovea.ScreenManager.Properties.Resources.outward4;
        	this.btnHandlersReset.Location = new System.Drawing.Point(65, 5);
        	this.btnHandlersReset.Name = "btnHandlersReset";
        	this.btnHandlersReset.Size = new System.Drawing.Size(20, 20);
        	this.btnHandlersReset.TabIndex = 24;
        	this.btnHandlersReset.UseVisualStyleBackColor = true;
        	this.btnHandlersReset.Click += new System.EventHandler(this.btnHandlersReset_Click);
        	// 
        	// btnSetHandlerRight
        	// 
        	this.btnSetHandlerRight.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        	this.btnSetHandlerRight.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnSetHandlerRight.FlatAppearance.BorderSize = 0;
        	this.btnSetHandlerRight.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnSetHandlerRight.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnSetHandlerRight.Image = global::Kinovea.ScreenManager.Properties.Resources.handlersetright;
        	this.btnSetHandlerRight.Location = new System.Drawing.Point(45, 5);
        	this.btnSetHandlerRight.Name = "btnSetHandlerRight";
        	this.btnSetHandlerRight.Size = new System.Drawing.Size(20, 20);
        	this.btnSetHandlerRight.TabIndex = 22;
        	this.btnSetHandlerRight.UseVisualStyleBackColor = true;
        	this.btnSetHandlerRight.Click += new System.EventHandler(this.btnSetHandlerRight_Click);
        	// 
        	// btnSetHandlerLeft
        	// 
        	this.btnSetHandlerLeft.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        	this.btnSetHandlerLeft.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnSetHandlerLeft.FlatAppearance.BorderSize = 0;
        	this.btnSetHandlerLeft.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnSetHandlerLeft.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnSetHandlerLeft.Image = global::Kinovea.ScreenManager.Properties.Resources.handlersetleft;
        	this.btnSetHandlerLeft.Location = new System.Drawing.Point(25, 5);
        	this.btnSetHandlerLeft.Name = "btnSetHandlerLeft";
        	this.btnSetHandlerLeft.Size = new System.Drawing.Size(20, 20);
        	this.btnSetHandlerLeft.TabIndex = 21;
        	this.btnSetHandlerLeft.UseVisualStyleBackColor = true;
        	this.btnSetHandlerLeft.Click += new System.EventHandler(this.btnSetHandlerLeft_Click);
        	// 
        	// lblWorkingZone
        	// 
        	this.lblWorkingZone.AutoSize = true;
        	this.lblWorkingZone.BackColor = System.Drawing.Color.White;
        	this.lblWorkingZone.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.lblWorkingZone.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.lblWorkingZone.ForeColor = System.Drawing.Color.Black;
        	this.lblWorkingZone.Location = new System.Drawing.Point(14, 24);
        	this.lblWorkingZone.Margin = new System.Windows.Forms.Padding(0);
        	this.lblWorkingZone.Name = "lblWorkingZone";
        	this.lblWorkingZone.Size = new System.Drawing.Size(75, 12);
        	this.lblWorkingZone.TabIndex = 19;
        	this.lblWorkingZone.Text = "Zone de Travail : ";
        	// 
        	// trkSelection
        	// 
        	this.trkSelection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.trkSelection.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.liqbackdock;
        	this.trkSelection.Location = new System.Drawing.Point(91, 5);
        	this.trkSelection.Maximum = ((long)(100));
        	this.trkSelection.Minimum = ((long)(0));
        	this.trkSelection.Name = "trkSelection";
        	this.trkSelection.SelEnd = ((long)(100));
        	this.trkSelection.SelLocked = false;
        	this.trkSelection.SelPos = ((long)(0));
        	this.trkSelection.SelStart = ((long)(0));
        	this.trkSelection.Size = new System.Drawing.Size(329, 20);
        	this.trkSelection.TabIndex = 17;
        	this.trkSelection.ToolTip = "";
        	this.trkSelection.SelectionChanged += new Kinovea.ScreenManager.SelectionTracker.SelectionChangedHandler(this.trkSelection_SelectionChanged);
        	this.trkSelection.SelectionChanging += new Kinovea.ScreenManager.SelectionTracker.SelectionChangingHandler(this.trkSelection_SelectionChanging);
        	this.trkSelection.TargetAcquired += new Kinovea.ScreenManager.SelectionTracker.TargetAcquiredHandler(this.trkSelection_TargetAcquired);
        	// 
        	// trkFrame
        	// 
        	this.trkFrame.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.trkFrame.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.trkFrame.Location = new System.Drawing.Point(5, 45);
        	this.trkFrame.Maximum = ((long)(100));
        	this.trkFrame.Minimum = ((long)(0));
        	this.trkFrame.MinimumSize = new System.Drawing.Size(50, 20);
        	this.trkFrame.Name = "trkFrame";
        	this.trkFrame.Position = ((long)(0));
        	this.trkFrame.ReportOnMouseMove = false;
        	this.trkFrame.Size = new System.Drawing.Size(410, 20);
        	this.trkFrame.TabIndex = 16;
        	this.trkFrame.PositionChanging += new Kinovea.ScreenManager.FrameTracker.PositionChangingHandler(this.trkFrame_PositionChanging);
        	this.trkFrame.PositionChanged += new Kinovea.ScreenManager.FrameTracker.PositionChangedHandler(this.trkFrame_PositionChanged);
        	// 
        	// btn_HandlersLock
        	// 
        	this.btn_HandlersLock.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        	this.btn_HandlersLock.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btn_HandlersLock.FlatAppearance.BorderSize = 0;
        	this.btn_HandlersLock.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btn_HandlersLock.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btn_HandlersLock.Image = global::Kinovea.ScreenManager.Properties.Resources.primselec_unlocked3;
        	this.btn_HandlersLock.Location = new System.Drawing.Point(5, 5);
        	this.btn_HandlersLock.Name = "btn_HandlersLock";
        	this.btn_HandlersLock.Size = new System.Drawing.Size(20, 20);
        	this.btn_HandlersLock.TabIndex = 8;
        	this.btn_HandlersLock.UseVisualStyleBackColor = true;
        	this.btn_HandlersLock.Click += new System.EventHandler(this.btn_HandlersLock_Click);
        	// 
        	// lblSelStartSelection
        	// 
        	this.lblSelStartSelection.BackColor = System.Drawing.Color.White;
        	this.lblSelStartSelection.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.lblSelStartSelection.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.lblSelStartSelection.ForeColor = System.Drawing.Color.Black;
        	this.lblSelStartSelection.Location = new System.Drawing.Point(108, 24);
        	this.lblSelStartSelection.Margin = new System.Windows.Forms.Padding(0);
        	this.lblSelStartSelection.Name = "lblSelStartSelection";
        	this.lblSelStartSelection.Size = new System.Drawing.Size(88, 15);
        	this.lblSelStartSelection.TabIndex = 3;
        	this.lblSelStartSelection.Text = "Début : 0:00:00:00";
        	// 
        	// lblTimeCode
        	// 
        	this.lblTimeCode.AutoSize = true;
        	this.lblTimeCode.BackColor = System.Drawing.Color.Transparent;
        	this.lblTimeCode.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.lblTimeCode.Location = new System.Drawing.Point(14, 63);
        	this.lblTimeCode.Name = "lblTimeCode";
        	this.lblTimeCode.Size = new System.Drawing.Size(89, 12);
        	this.lblTimeCode.TabIndex = 2;
        	this.lblTimeCode.Text = "Position : 0:00:00:00";
        	this.lblTimeCode.DoubleClick += new System.EventHandler(this.lblTimeCode_DoubleClick);
        	// 
        	// buttonGotoFirst
        	// 
        	this.buttonGotoFirst.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.buttonGotoFirst.FlatAppearance.BorderColor = System.Drawing.Color.White;
        	this.buttonGotoFirst.FlatAppearance.BorderSize = 0;
        	this.buttonGotoFirst.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
        	this.buttonGotoFirst.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
        	this.buttonGotoFirst.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.buttonGotoFirst.Image = global::Kinovea.ScreenManager.Properties.Resources.liqfirst7;
        	this.buttonGotoFirst.Location = new System.Drawing.Point(25, 87);
        	this.buttonGotoFirst.MinimumSize = new System.Drawing.Size(25, 25);
        	this.buttonGotoFirst.Name = "buttonGotoFirst";
        	this.buttonGotoFirst.Size = new System.Drawing.Size(30, 25);
        	this.buttonGotoFirst.TabIndex = 4;
        	this.buttonGotoFirst.UseVisualStyleBackColor = true;
        	this.buttonGotoFirst.Click += new System.EventHandler(this.buttonGotoFirst_Click);
        	// 
        	// buttonGotoPrevious
        	// 
        	this.buttonGotoPrevious.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.buttonGotoPrevious.FlatAppearance.BorderSize = 0;
        	this.buttonGotoPrevious.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
        	this.buttonGotoPrevious.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
        	this.buttonGotoPrevious.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.buttonGotoPrevious.Image = global::Kinovea.ScreenManager.Properties.Resources.liqprev5;
        	this.buttonGotoPrevious.Location = new System.Drawing.Point(60, 87);
        	this.buttonGotoPrevious.MinimumSize = new System.Drawing.Size(25, 25);
        	this.buttonGotoPrevious.Name = "buttonGotoPrevious";
        	this.buttonGotoPrevious.Size = new System.Drawing.Size(30, 25);
        	this.buttonGotoPrevious.TabIndex = 3;
        	this.buttonGotoPrevious.UseVisualStyleBackColor = true;
        	this.buttonGotoPrevious.Click += new System.EventHandler(this.buttonGotoPrevious_Click);
        	// 
        	// buttonGotoNext
        	// 
        	this.buttonGotoNext.BackColor = System.Drawing.Color.Transparent;
        	this.buttonGotoNext.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.buttonGotoNext.FlatAppearance.BorderSize = 0;
        	this.buttonGotoNext.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
        	this.buttonGotoNext.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
        	this.buttonGotoNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.buttonGotoNext.Image = global::Kinovea.ScreenManager.Properties.Resources.liqnext6;
        	this.buttonGotoNext.Location = new System.Drawing.Point(140, 87);
        	this.buttonGotoNext.MinimumSize = new System.Drawing.Size(25, 25);
        	this.buttonGotoNext.Name = "buttonGotoNext";
        	this.buttonGotoNext.Size = new System.Drawing.Size(30, 25);
        	this.buttonGotoNext.TabIndex = 2;
        	this.buttonGotoNext.UseVisualStyleBackColor = false;
        	this.buttonGotoNext.Click += new System.EventHandler(this.buttonGotoNext_Click);
        	// 
        	// buttonPlay
        	// 
        	this.buttonPlay.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.buttonPlay.FlatAppearance.BorderSize = 0;
        	this.buttonPlay.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
        	this.buttonPlay.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
        	this.buttonPlay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.buttonPlay.Image = global::Kinovea.ScreenManager.Properties.Resources.liqplay17;
        	this.buttonPlay.Location = new System.Drawing.Point(95, 84);
        	this.buttonPlay.MinimumSize = new System.Drawing.Size(30, 25);
        	this.buttonPlay.Name = "buttonPlay";
        	this.buttonPlay.Size = new System.Drawing.Size(40, 30);
        	this.buttonPlay.TabIndex = 0;
        	this.buttonPlay.UseVisualStyleBackColor = true;
        	this.buttonPlay.Click += new System.EventHandler(this.buttonPlay_Click);
        	// 
        	// sldrSpeed
        	// 
        	this.sldrSpeed.BackColor = System.Drawing.Color.White;
        	this.sldrSpeed.Enabled = false;
        	this.sldrSpeed.LargeChange = 5;
        	this.sldrSpeed.Location = new System.Drawing.Point(195, 65);
        	this.sldrSpeed.Maximum = 200;
        	this.sldrSpeed.Minimum = 0;
        	this.sldrSpeed.MinimumSize = new System.Drawing.Size(20, 10);
        	this.sldrSpeed.Name = "sldrSpeed";
        	this.sldrSpeed.Size = new System.Drawing.Size(150, 10);
        	this.sldrSpeed.SmallChange = 1;
        	this.sldrSpeed.StickyValue = 100;
        	this.sldrSpeed.TabIndex = 15;
        	this.sldrSpeed.Value = 100;
        	this.sldrSpeed.ValueChanged += new Kinovea.ScreenManager.SpeedSlider.ValueChangedHandler(this.sldrSpeed_ValueChanged);
        	this.sldrSpeed.KeyDown += new System.Windows.Forms.KeyEventHandler(this.sldrSpeed_KeyDown);
        	// 
        	// lblSpeedTuner
        	// 
        	this.lblSpeedTuner.AutoSize = true;
        	this.lblSpeedTuner.BackColor = System.Drawing.Color.White;
        	this.lblSpeedTuner.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.lblSpeedTuner.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.lblSpeedTuner.ForeColor = System.Drawing.Color.Black;
        	this.lblSpeedTuner.Location = new System.Drawing.Point(120, 63);
        	this.lblSpeedTuner.Margin = new System.Windows.Forms.Padding(0);
        	this.lblSpeedTuner.Name = "lblSpeedTuner";
        	this.lblSpeedTuner.Size = new System.Drawing.Size(67, 12);
        	this.lblSpeedTuner.TabIndex = 10;
        	this.lblSpeedTuner.Text = "Vitesse : 100%";
        	this.lblSpeedTuner.DoubleClick += new System.EventHandler(this.lblSpeedTuner_DoubleClick);
        	// 
        	// buttonGotoLast
        	// 
        	this.buttonGotoLast.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.buttonGotoLast.FlatAppearance.BorderSize = 0;
        	this.buttonGotoLast.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
        	this.buttonGotoLast.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
        	this.buttonGotoLast.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.buttonGotoLast.Image = global::Kinovea.ScreenManager.Properties.Resources.liqlast5;
        	this.buttonGotoLast.Location = new System.Drawing.Point(175, 87);
        	this.buttonGotoLast.MinimumSize = new System.Drawing.Size(25, 25);
        	this.buttonGotoLast.Name = "buttonGotoLast";
        	this.buttonGotoLast.Size = new System.Drawing.Size(30, 25);
        	this.buttonGotoLast.TabIndex = 1;
        	this.buttonGotoLast.UseVisualStyleBackColor = true;
        	this.buttonGotoLast.Click += new System.EventHandler(this.buttonGotoLast_Click);
        	// 
        	// buttonPlayingMode
        	// 
        	this.buttonPlayingMode.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        	this.buttonPlayingMode.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.buttonPlayingMode.FlatAppearance.BorderSize = 0;
        	this.buttonPlayingMode.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.buttonPlayingMode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.buttonPlayingMode.Image = global::Kinovea.ScreenManager.Properties.Resources.playmodeloop;
        	this.buttonPlayingMode.Location = new System.Drawing.Point(210, 87);
        	this.buttonPlayingMode.MinimumSize = new System.Drawing.Size(25, 25);
        	this.buttonPlayingMode.Name = "buttonPlayingMode";
        	this.buttonPlayingMode.Size = new System.Drawing.Size(30, 25);
        	this.buttonPlayingMode.TabIndex = 5;
        	this.buttonPlayingMode.Tag = "";
        	this.buttonPlayingMode.UseVisualStyleBackColor = true;
        	this.buttonPlayingMode.Click += new System.EventHandler(this.buttonPlayingMode_Click);
        	// 
        	// btnPdf
        	// 
        	this.btnPdf.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnPdf.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        	this.btnPdf.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnPdf.FlatAppearance.BorderSize = 0;
        	this.btnPdf.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnPdf.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnPdf.Image = global::Kinovea.ScreenManager.Properties.Resources.pdfexport;
        	this.btnPdf.Location = new System.Drawing.Point(356, 27);
        	this.btnPdf.MinimumSize = new System.Drawing.Size(25, 25);
        	this.btnPdf.Name = "btnPdf";
        	this.btnPdf.Size = new System.Drawing.Size(30, 25);
        	this.btnPdf.TabIndex = 26;
        	this.btnPdf.Tag = "";
        	this.btnPdf.UseVisualStyleBackColor = true;
        	this.btnPdf.Visible = false;
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
        	// panelCenter
        	// 
        	this.panelCenter.BackColor = System.Drawing.Color.Black;
        	this.panelCenter.Controls.Add(this.ImageResizerNE);
        	this.panelCenter.Controls.Add(this.ImageResizerNW);
        	this.panelCenter.Controls.Add(this.ImageResizerSW);
        	this.panelCenter.Controls.Add(this.ImageResizerSE);
        	this.panelCenter.Controls.Add(this.panelDebug);
        	this.panelCenter.Controls.Add(this.pbSurfaceScreen);
        	this.panelCenter.Controls.Add(this.ActiveScreenIndicator);
        	this.panelCenter.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.panelCenter.Location = new System.Drawing.Point(0, 0);
        	this.panelCenter.MinimumSize = new System.Drawing.Size(350, 25);
        	this.panelCenter.Name = "panelCenter";
        	this.panelCenter.Size = new System.Drawing.Size(420, 235);
        	this.panelCenter.TabIndex = 2;
        	this.panelCenter.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PanelCenter_MouseClick);
        	this.panelCenter.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PanelCenter_MouseDown);
        	this.panelCenter.Resize += new System.EventHandler(this.PanelCenter_Resize);
        	this.panelCenter.MouseEnter += new System.EventHandler(this.PanelCenter_MouseEnter);
        	// 
        	// ImageResizerNE
        	// 
        	this.ImageResizerNE.Anchor = System.Windows.Forms.AnchorStyles.None;
        	this.ImageResizerNE.BackColor = System.Drawing.Color.DimGray;
        	this.ImageResizerNE.Cursor = System.Windows.Forms.Cursors.SizeNESW;
        	this.ImageResizerNE.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.ImageResizerNE.Image = global::Kinovea.ScreenManager.Properties.Resources.resizer4;
        	this.ImageResizerNE.Location = new System.Drawing.Point(92, 70);
        	this.ImageResizerNE.Name = "ImageResizerNE";
        	this.ImageResizerNE.Size = new System.Drawing.Size(6, 6);
        	this.ImageResizerNE.TabIndex = 9;
        	this.ImageResizerNE.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ImageResizerNE_MouseMove);
        	this.ImageResizerNE.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.Resizers_MouseDoubleClick);
        	this.ImageResizerNE.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Resizers_MouseUp);
        	// 
        	// ImageResizerNW
        	// 
        	this.ImageResizerNW.Anchor = System.Windows.Forms.AnchorStyles.None;
        	this.ImageResizerNW.BackColor = System.Drawing.Color.DimGray;
        	this.ImageResizerNW.Cursor = System.Windows.Forms.Cursors.SizeNWSE;
        	this.ImageResizerNW.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.ImageResizerNW.Image = global::Kinovea.ScreenManager.Properties.Resources.resizer4;
        	this.ImageResizerNW.Location = new System.Drawing.Point(57, 70);
        	this.ImageResizerNW.Name = "ImageResizerNW";
        	this.ImageResizerNW.Size = new System.Drawing.Size(6, 6);
        	this.ImageResizerNW.TabIndex = 8;
        	this.ImageResizerNW.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ImageResizerNW_MouseMove);
        	this.ImageResizerNW.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.Resizers_MouseDoubleClick);
        	this.ImageResizerNW.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Resizers_MouseUp);
        	// 
        	// ImageResizerSW
        	// 
        	this.ImageResizerSW.Anchor = System.Windows.Forms.AnchorStyles.None;
        	this.ImageResizerSW.BackColor = System.Drawing.Color.DimGray;
        	this.ImageResizerSW.Cursor = System.Windows.Forms.Cursors.SizeNESW;
        	this.ImageResizerSW.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.ImageResizerSW.Image = global::Kinovea.ScreenManager.Properties.Resources.resizer4;
        	this.ImageResizerSW.Location = new System.Drawing.Point(57, 95);
        	this.ImageResizerSW.Name = "ImageResizerSW";
        	this.ImageResizerSW.Size = new System.Drawing.Size(6, 6);
        	this.ImageResizerSW.TabIndex = 7;
        	this.ImageResizerSW.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ImageResizerSW_MouseMove);
        	this.ImageResizerSW.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.Resizers_MouseDoubleClick);
        	this.ImageResizerSW.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Resizers_MouseUp);
        	// 
        	// ImageResizerSE
        	// 
        	this.ImageResizerSE.Anchor = System.Windows.Forms.AnchorStyles.None;
        	this.ImageResizerSE.BackColor = System.Drawing.Color.DimGray;
        	this.ImageResizerSE.Cursor = System.Windows.Forms.Cursors.SizeNWSE;
        	this.ImageResizerSE.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.ImageResizerSE.ForeColor = System.Drawing.Color.Transparent;
        	this.ImageResizerSE.Image = global::Kinovea.ScreenManager.Properties.Resources.resizer4;
        	this.ImageResizerSE.Location = new System.Drawing.Point(92, 95);
        	this.ImageResizerSE.Name = "ImageResizerSE";
        	this.ImageResizerSE.Size = new System.Drawing.Size(6, 6);
        	this.ImageResizerSE.TabIndex = 6;
        	this.ImageResizerSE.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ImageResizerSE_MouseMove);
        	this.ImageResizerSE.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.Resizers_MouseDoubleClick);
        	this.ImageResizerSE.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Resizers_MouseUp);
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
        	// pbSurfaceScreen
        	// 
        	this.pbSurfaceScreen.Cursor = System.Windows.Forms.Cursors.Arrow;
        	this.pbSurfaceScreen.Location = new System.Drawing.Point(43, 29);
        	this.pbSurfaceScreen.Name = "pbSurfaceScreen";
        	this.pbSurfaceScreen.Size = new System.Drawing.Size(101, 73);
        	this.pbSurfaceScreen.TabIndex = 2;
        	this.pbSurfaceScreen.TabStop = false;
        	this.pbSurfaceScreen.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.Common_MouseWheel);
        	this.pbSurfaceScreen.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SurfaceScreen_MouseMove);
        	this.pbSurfaceScreen.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.SurfaceScreen_MouseDoubleClick);
        	this.pbSurfaceScreen.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SurfaceScreen_MouseDown);
        	this.pbSurfaceScreen.Paint += new System.Windows.Forms.PaintEventHandler(this.SurfaceScreen_Paint);
        	this.pbSurfaceScreen.MouseUp += new System.Windows.Forms.MouseEventHandler(this.SurfaceScreen_MouseUp);
        	this.pbSurfaceScreen.MouseEnter += new System.EventHandler(this.SurfaceScreen_MouseEnter);
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
        	this.splitKeyframes.Panel1.Controls.Add(this.panelCenter);
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
        	this.splitKeyframes.Size = new System.Drawing.Size(420, 367);
        	this.splitKeyframes.SplitterDistance = 235;
        	this.splitKeyframes.SplitterWidth = 2;
        	this.splitKeyframes.TabIndex = 10;
        	this.splitKeyframes.Resize += new System.EventHandler(this.splitKeyframes_Resize);
        	// 
        	// btn3dplane
        	// 
        	this.btn3dplane.BackColor = System.Drawing.Color.Transparent;
        	this.btn3dplane.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btn3dplane.FlatAppearance.BorderSize = 0;
        	this.btn3dplane.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btn3dplane.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btn3dplane.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btn3dplane.ForeColor = System.Drawing.Color.Black;
        	this.btn3dplane.Image = global::Kinovea.ScreenManager.Properties.Resources.plane4;
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
        	this.btnMagnifier.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnMagnifier.FlatAppearance.BorderSize = 0;
        	this.btnMagnifier.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnMagnifier.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnMagnifier.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnMagnifier.ForeColor = System.Drawing.Color.Black;
        	this.btnMagnifier.Image = global::Kinovea.ScreenManager.Properties.Resources.magnifier2;
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
        	this.btnDrawingToolChrono.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnDrawingToolChrono.FlatAppearance.BorderSize = 0;
        	this.btnDrawingToolChrono.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolChrono.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawingToolChrono.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnDrawingToolChrono.ForeColor = System.Drawing.Color.Black;
        	this.btnDrawingToolChrono.Image = global::Kinovea.ScreenManager.Properties.Resources.chrono5;
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
        	this.btnDockBottom.Location = new System.Drawing.Point(398, 4);
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
        	this.btnDrawingToolCross2D.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnDrawingToolCross2D.FlatAppearance.BorderSize = 0;
        	this.btnDrawingToolCross2D.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolCross2D.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawingToolCross2D.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnDrawingToolCross2D.ForeColor = System.Drawing.Color.Black;
        	this.btnDrawingToolCross2D.Image = global::Kinovea.ScreenManager.Properties.Resources.cross5;
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
        	this.btnShowComments.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnShowComments.FlatAppearance.BorderSize = 0;
        	this.btnShowComments.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnShowComments.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnShowComments.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnShowComments.ForeColor = System.Drawing.Color.Black;
        	this.btnShowComments.Image = global::Kinovea.ScreenManager.Properties.Resources.comments2;
        	this.btnShowComments.Location = new System.Drawing.Point(70, 2);
        	this.btnShowComments.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
        	this.btnShowComments.Name = "btnShowComments";
        	this.btnShowComments.Size = new System.Drawing.Size(25, 25);
        	this.btnShowComments.TabIndex = 14;
        	this.btnShowComments.UseVisualStyleBackColor = false;
        	this.btnShowComments.Click += new System.EventHandler(this.btnShowComments_Click);
        	// 
        	// btnDrawingToolLine2D
        	// 
        	this.btnDrawingToolLine2D.BackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolLine2D.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnDrawingToolLine2D.FlatAppearance.BorderSize = 0;
        	this.btnDrawingToolLine2D.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolLine2D.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawingToolLine2D.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnDrawingToolLine2D.ForeColor = System.Drawing.Color.Black;
        	this.btnDrawingToolLine2D.Image = global::Kinovea.ScreenManager.Properties.Resources.line6;
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
        	this.btnColorProfile.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnColorProfile.FlatAppearance.BorderSize = 0;
        	this.btnColorProfile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnColorProfile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnColorProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnColorProfile.ForeColor = System.Drawing.Color.Black;
        	this.btnColorProfile.Image = global::Kinovea.ScreenManager.Properties.Resources.SwatchIcon3;
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
        	this.btnDrawingToolText.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnDrawingToolText.FlatAppearance.BorderSize = 0;
        	this.btnDrawingToolText.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolText.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawingToolText.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnDrawingToolText.ForeColor = System.Drawing.Color.Black;
        	this.btnDrawingToolText.Image = global::Kinovea.ScreenManager.Properties.Resources.TextToolIcon;
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
        	this.btnDrawingToolPencil.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnDrawingToolPencil.FlatAppearance.BorderSize = 0;
        	this.btnDrawingToolPencil.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolPencil.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawingToolPencil.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnDrawingToolPencil.ForeColor = System.Drawing.Color.Black;
        	this.btnDrawingToolPencil.Image = global::Kinovea.ScreenManager.Properties.Resources.activepencil;
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
        	this.btnDrawingToolAngle2D.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        	this.btnDrawingToolAngle2D.FlatAppearance.BorderSize = 0;
        	this.btnDrawingToolAngle2D.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolAngle2D.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawingToolAngle2D.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnDrawingToolAngle2D.ForeColor = System.Drawing.Color.Black;
        	this.btnDrawingToolAngle2D.Image = global::Kinovea.ScreenManager.Properties.Resources.angle5;
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
        	this.btnDrawingToolPointer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnDrawingToolPointer.FlatAppearance.BorderSize = 0;
        	this.btnDrawingToolPointer.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolPointer.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawingToolPointer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnDrawingToolPointer.ForeColor = System.Drawing.Color.White;
        	this.btnDrawingToolPointer.Image = global::Kinovea.ScreenManager.Properties.Resources.handtool;
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
        	this.pnlThumbnails.Controls.Add(this.btnPdf);
        	this.pnlThumbnails.Controls.Add(this.pictureBox1);
        	this.pnlThumbnails.Location = new System.Drawing.Point(0, 27);
        	this.pnlThumbnails.Name = "pnlThumbnails";
        	this.pnlThumbnails.Size = new System.Drawing.Size(420, 102);
        	this.pnlThumbnails.TabIndex = 3;
        	this.pnlThumbnails.DoubleClick += new System.EventHandler(this.pnlThumbnails_DoubleClick);
        	this.pnlThumbnails.MouseEnter += new System.EventHandler(this.pnlThumbnails_MouseEnter);
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
        	this.btnAddKeyframe.FlatAppearance.BorderSize = 0;
        	this.btnAddKeyframe.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnAddKeyframe.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnAddKeyframe.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnAddKeyframe.ForeColor = System.Drawing.Color.White;
        	this.btnAddKeyframe.Image = global::Kinovea.ScreenManager.Properties.Resources.MenuFileNewIcon;
        	this.btnAddKeyframe.Location = new System.Drawing.Point(5, 2);
        	this.btnAddKeyframe.Name = "btnAddKeyframe";
        	this.btnAddKeyframe.Size = new System.Drawing.Size(25, 25);
        	this.btnAddKeyframe.TabIndex = 0;
        	this.btnAddKeyframe.UseVisualStyleBackColor = false;
        	this.btnAddKeyframe.Click += new System.EventHandler(this.btnAddKeyframe_Click);
        	// 
        	// PlayerScreenUserInterface
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackColor = System.Drawing.Color.Gainsboro;
        	this.Controls.Add(this.splitKeyframes);
        	this.Controls.Add(this.panelVideoControls);
        	this.Controls.Add(this.panelTop);
        	this.MinimumSize = new System.Drawing.Size(350, 310);
        	this.Name = "PlayerScreenUserInterface";
        	this.Size = new System.Drawing.Size(420, 510);
        	this.panelTop.ResumeLayout(false);
        	this.panelTop.PerformLayout();
        	this.panelVideoControls.ResumeLayout(false);
        	this.panelVideoControls.PerformLayout();
        	this.panel1.ResumeLayout(false);
        	this.groupBoxSpeedTuner.ResumeLayout(false);
        	this.panelCenter.ResumeLayout(false);
        	this.panelDebug.ResumeLayout(false);
        	this.panelDebug.PerformLayout();
        	((System.ComponentModel.ISupportInitialize)(this.pbSurfaceScreen)).EndInit();
        	this.splitKeyframes.Panel1.ResumeLayout(false);
        	this.splitKeyframes.Panel2.ResumeLayout(false);
        	this.splitKeyframes.ResumeLayout(false);
        	this.pnlThumbnails.ResumeLayout(false);
        	((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
        	this.ResumeLayout(false);
        }
        private System.Windows.Forms.Button btnPausedVideo;
        private System.Windows.Forms.Button btnSaveVideo;
        private System.Windows.Forms.Panel panel1;

        #endregion

        private System.Windows.Forms.Panel panelTop;
        public System.Windows.Forms.Panel panelCenter;
        private System.Windows.Forms.Panel panelVideoControls;
        private System.Windows.Forms.Button buttonPlay;
        private System.Windows.Forms.Button buttonGotoLast;
        private System.Windows.Forms.Button buttonGotoFirst;
        private System.Windows.Forms.Button buttonGotoPrevious;
        private System.Windows.Forms.Button buttonGotoNext;
        private System.Windows.Forms.Panel panelDebug;
        public System.Windows.Forms.Label dbgDrops;
        private System.Windows.Forms.Button buttonPlayingMode;
        private System.Windows.Forms.Button PrimarySelection;
        private System.Windows.Forms.Label lblSelDuration;
        private System.Windows.Forms.GroupBox groupBoxSpeedTuner;
        private System.Windows.Forms.Button markerSpeedTuner;
        private System.Windows.Forms.Label lblSelStartSelection;
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
        private System.Windows.Forms.Button btn_HandlersLock;
        private System.Windows.Forms.Label lblSpeedTuner;
        private FrameTracker trkFrame;
        private SelectionTracker trkSelection;
        private System.Windows.Forms.Button btnSnapShot;
        private System.Windows.Forms.Label ImageResizerSE;
        private System.Windows.Forms.Label ImageResizerSW;
        private System.Windows.Forms.Label ImageResizerNE;
        private System.Windows.Forms.Label ImageResizerNW;
        private System.Windows.Forms.Label lblWorkingZone;
        private System.Windows.Forms.Label dbgDurationFrames;
        private System.Windows.Forms.Label dbgCurrentFrame;
        public System.Windows.Forms.PictureBox pbSurfaceScreen;
        private System.Windows.Forms.Button btnSetHandlerRight;
        private System.Windows.Forms.Button btnSetHandlerLeft;
        private System.Windows.Forms.Label dbgAvailableRam;
        private System.Windows.Forms.SplitContainer splitKeyframes;
        private System.Windows.Forms.Button btnAddKeyframe;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel pnlThumbnails;
        private System.Windows.Forms.Button btnDrawingToolPointer;
        private SpeedSlider sldrSpeed;
        private System.Windows.Forms.Button btnDrawingToolCross2D;
        private System.Windows.Forms.Button btnDrawingToolAngle2D;
        private System.Windows.Forms.Button btnDrawingToolPencil;
        private System.Windows.Forms.Button btnDrawingToolText;
        private System.Windows.Forms.Button btnShowComments;
        private System.Windows.Forms.Button btnColorProfile;
        private System.Windows.Forms.Button btnDockBottom;
        private System.Windows.Forms.Button btnDrawingToolLine2D;
        private System.Windows.Forms.Button btnRafale;
        private System.Windows.Forms.Button btnHandlersReset;
        private System.Windows.Forms.Button btnDiaporama;
        private System.Windows.Forms.Button btnPdf;
        private System.Windows.Forms.Button btnDrawingToolChrono;
        private System.Windows.Forms.Button btnMagnifier;
        private System.Windows.Forms.Button btn3dplane;
        
    }
}
