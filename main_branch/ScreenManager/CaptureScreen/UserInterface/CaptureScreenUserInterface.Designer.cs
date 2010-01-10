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
        	this.buttonRecord = new System.Windows.Forms.Button();
        	this.btnRafale = new System.Windows.Forms.Button();
        	this.lblWorkingZone = new System.Windows.Forms.Label();
        	this.btnSnapShot = new System.Windows.Forms.Button();
        	this.lblTimeCode = new System.Windows.Forms.Label();
        	this.buttonPlay = new System.Windows.Forms.Button();
        	this.lblSpeedTuner = new System.Windows.Forms.Label();
        	this.panelCenter = new System.Windows.Forms.Panel();
        	this.ImageResizerNE = new System.Windows.Forms.Label();
        	this.ImageResizerNW = new System.Windows.Forms.Label();
        	this.ImageResizerSW = new System.Windows.Forms.Label();
        	this.ImageResizerSE = new System.Windows.Forms.Label();
        	this.pbSurfaceScreen = new System.Windows.Forms.PictureBox();
        	this.ActiveScreenIndicator = new System.Windows.Forms.Label();
        	this.toolTips = new System.Windows.Forms.ToolTip(this.components);
        	this.splitKeyframes = new System.Windows.Forms.SplitContainer();
        	this.btn3dplane = new System.Windows.Forms.Button();
        	this.btnMagnifier = new System.Windows.Forms.Button();
        	this.btnDockBottom = new System.Windows.Forms.Button();
        	this.btnDrawingToolCross2D = new System.Windows.Forms.Button();
        	this.btnDrawingToolLine2D = new System.Windows.Forms.Button();
        	this.btnColorProfile = new System.Windows.Forms.Button();
        	this.btnDrawingToolText = new System.Windows.Forms.Button();
        	this.btnDrawingToolPencil = new System.Windows.Forms.Button();
        	this.btnDrawingToolAngle2D = new System.Windows.Forms.Button();
        	this.btnDrawingToolPointer = new System.Windows.Forms.Button();
        	this.pnlThumbnails = new System.Windows.Forms.Panel();
        	this.pictureBox1 = new System.Windows.Forms.PictureBox();
        	this.tmrCaptureDeviceDetector = new System.Windows.Forms.Timer(this.components);
        	this.panelTop.SuspendLayout();
        	this.panelVideoControls.SuspendLayout();
        	this.panelCenter.SuspendLayout();
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
        	this.panelTop.Size = new System.Drawing.Size(350, 19);
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
        	this.panelVideoControls.Controls.Add(this.lblSelDuration);
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
        	// buttonRecord
        	// 
        	this.buttonRecord.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.record;
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
        	// 
        	// panelCenter
        	// 
        	this.panelCenter.BackColor = System.Drawing.Color.Black;
        	this.panelCenter.Controls.Add(this.ImageResizerNE);
        	this.panelCenter.Controls.Add(this.ImageResizerNW);
        	this.panelCenter.Controls.Add(this.ImageResizerSW);
        	this.panelCenter.Controls.Add(this.ImageResizerSE);
        	this.panelCenter.Controls.Add(this.pbSurfaceScreen);
        	this.panelCenter.Controls.Add(this.ActiveScreenIndicator);
        	this.panelCenter.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.panelCenter.Location = new System.Drawing.Point(0, 0);
        	this.panelCenter.MinimumSize = new System.Drawing.Size(350, 25);
        	this.panelCenter.Name = "panelCenter";
        	this.panelCenter.Size = new System.Drawing.Size(350, 250);
        	this.panelCenter.TabIndex = 2;
        	this.panelCenter.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.Common_MouseWheel);
        	this.panelCenter.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PanelCenter_MouseClick);
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
        	this.splitKeyframes.Panel2.Controls.Add(this.btnDockBottom);
        	this.splitKeyframes.Panel2.Controls.Add(this.btnDrawingToolCross2D);
        	this.splitKeyframes.Panel2.Controls.Add(this.btnDrawingToolLine2D);
        	this.splitKeyframes.Panel2.Controls.Add(this.btnColorProfile);
        	this.splitKeyframes.Panel2.Controls.Add(this.btnDrawingToolText);
        	this.splitKeyframes.Panel2.Controls.Add(this.btnDrawingToolPencil);
        	this.splitKeyframes.Panel2.Controls.Add(this.btnDrawingToolAngle2D);
        	this.splitKeyframes.Panel2.Controls.Add(this.btnDrawingToolPointer);
        	this.splitKeyframes.Panel2.Controls.Add(this.pnlThumbnails);
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
        	this.btn3dplane.Location = new System.Drawing.Point(170, 2);
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
        	this.btnMagnifier.Location = new System.Drawing.Point(195, 2);
        	this.btnMagnifier.Name = "btnMagnifier";
        	this.btnMagnifier.Size = new System.Drawing.Size(25, 25);
        	this.btnMagnifier.TabIndex = 18;
        	this.btnMagnifier.UseVisualStyleBackColor = false;
        	this.btnMagnifier.Click += new System.EventHandler(this.btnMagnifier_Click);
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
        	this.btnDrawingToolCross2D.Location = new System.Drawing.Point(120, 2);
        	this.btnDrawingToolCross2D.Name = "btnDrawingToolCross2D";
        	this.btnDrawingToolCross2D.Size = new System.Drawing.Size(25, 25);
        	this.btnDrawingToolCross2D.TabIndex = 7;
        	this.btnDrawingToolCross2D.UseVisualStyleBackColor = false;
        	this.btnDrawingToolCross2D.Click += new System.EventHandler(this.btnDrawingToolCross2D_Click);
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
        	this.btnDrawingToolLine2D.Location = new System.Drawing.Point(95, 2);
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
        	this.btnColorProfile.Location = new System.Drawing.Point(220, 2);
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
        	this.btnDrawingToolText.Location = new System.Drawing.Point(45, 2);
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
        	this.btnDrawingToolPencil.Location = new System.Drawing.Point(70, 2);
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
        	this.btnDrawingToolAngle2D.Location = new System.Drawing.Point(145, 2);
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
        	this.btnDrawingToolPointer.Location = new System.Drawing.Point(5, 2);
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
        	// tmrCaptureDeviceDetector
        	// 
        	this.tmrCaptureDeviceDetector.Interval = 1000;
        	this.tmrCaptureDeviceDetector.Tick += new System.EventHandler(this.tmrCaptureDeviceDetector_Tick);
        	// 
        	// CaptureScreenUserInterface
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackColor = System.Drawing.Color.Gainsboro;
        	this.Controls.Add(this.splitKeyframes);
        	this.Controls.Add(this.panelVideoControls);
        	this.Controls.Add(this.panelTop);
        	this.MinimumSize = new System.Drawing.Size(350, 310);
        	this.Name = "CaptureScreenUserInterface";
        	this.Size = new System.Drawing.Size(350, 510);
        	this.panelTop.ResumeLayout(false);
        	this.panelTop.PerformLayout();
        	this.panelVideoControls.ResumeLayout(false);
        	this.panelVideoControls.PerformLayout();
        	this.panelCenter.ResumeLayout(false);
        	((System.ComponentModel.ISupportInitialize)(this.pbSurfaceScreen)).EndInit();
        	this.splitKeyframes.Panel1.ResumeLayout(false);
        	this.splitKeyframes.Panel2.ResumeLayout(false);
        	this.splitKeyframes.ResumeLayout(false);
        	this.pnlThumbnails.ResumeLayout(false);
        	((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
        	this.ResumeLayout(false);
        }
        private System.Windows.Forms.Timer tmrCaptureDeviceDetector;
        private System.Windows.Forms.Button buttonRecord;

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Panel panelCenter;
        private System.Windows.Forms.Panel panelVideoControls;
        private System.Windows.Forms.Button buttonPlay;
        private System.Windows.Forms.Label lblSelDuration;
        private System.Windows.Forms.Label ActiveScreenIndicator;
        private System.Windows.Forms.ToolTip toolTips;
        private System.Windows.Forms.Label lblTimeCode;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.Label lblSpeedTuner;
        
        private System.Windows.Forms.Button btnSnapShot;
        private System.Windows.Forms.Label ImageResizerSE;
        private System.Windows.Forms.Label ImageResizerSW;
        private System.Windows.Forms.Label ImageResizerNE;
        private System.Windows.Forms.Label ImageResizerNW;
        private System.Windows.Forms.Label lblWorkingZone;
        public System.Windows.Forms.PictureBox pbSurfaceScreen;
        private System.Windows.Forms.SplitContainer splitKeyframes;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel pnlThumbnails;
        private System.Windows.Forms.Button btnDrawingToolPointer;
        
        private System.Windows.Forms.Button btnDrawingToolCross2D;
        private System.Windows.Forms.Button btnDrawingToolAngle2D;
        private System.Windows.Forms.Button btnDrawingToolPencil;
        private System.Windows.Forms.Button btnDrawingToolText;
        private System.Windows.Forms.Button btnColorProfile;
        private System.Windows.Forms.Button btnDockBottom;
        private System.Windows.Forms.Button btnDrawingToolLine2D;
        private System.Windows.Forms.Button btnRafale;
        private System.Windows.Forms.Button btnMagnifier;
        private System.Windows.Forms.Button btn3dplane;
        
    }
}
