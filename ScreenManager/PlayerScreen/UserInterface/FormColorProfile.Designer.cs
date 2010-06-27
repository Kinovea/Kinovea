namespace Kinovea.ScreenManager
{
    partial class formColorProfile
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
        	this.components = new System.ComponentModel.Container();
        	this.btnApply = new System.Windows.Forms.Button();
        	this.btnCancel = new System.Windows.Forms.Button();
        	this.grpColors = new System.Windows.Forms.GroupBox();
        	this.cmbTextSize = new System.Windows.Forms.ComboBox();
        	this.cmbChronoSize = new System.Windows.Forms.ComboBox();
        	this.btnChronoColor = new System.Windows.Forms.Button();
        	this.btnDrawingToolChrono = new System.Windows.Forms.Button();
        	this.btnLineStyle = new System.Windows.Forms.Button();
        	this.btnPencilStyle = new System.Windows.Forms.Button();
        	this.btnAngleColor = new System.Windows.Forms.Button();
        	this.btnDrawingToolAngle2D = new System.Windows.Forms.Button();
        	this.btnCrossColor = new System.Windows.Forms.Button();
        	this.btnDrawingToolCross2D = new System.Windows.Forms.Button();
        	this.btnLineColor = new System.Windows.Forms.Button();
        	this.btnDrawingToolLine2D = new System.Windows.Forms.Button();
        	this.btnPencilColor = new System.Windows.Forms.Button();
        	this.btnDrawingToolPencil = new System.Windows.Forms.Button();
        	this.btnTextColor = new System.Windows.Forms.Button();
        	this.btnDrawingToolText = new System.Windows.Forms.Button();
        	this.toolTips = new System.Windows.Forms.ToolTip(this.components);
        	this.btnSaveProfile = new System.Windows.Forms.Button();
        	this.btnLoadProfile = new System.Windows.Forms.Button();
        	this.btnDefaultProfile = new System.Windows.Forms.Button();
        	this.grpColors.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// btnApply
        	// 
        	this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnApply.DialogResult = System.Windows.Forms.DialogResult.OK;
        	this.btnApply.Location = new System.Drawing.Point(23, 292);
        	this.btnApply.Name = "btnApply";
        	this.btnApply.Size = new System.Drawing.Size(99, 24);
        	this.btnApply.TabIndex = 70;
        	this.btnApply.Text = "Apply";
        	this.btnApply.UseVisualStyleBackColor = true;
        	this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
        	// 
        	// btnCancel
        	// 
        	this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        	this.btnCancel.Location = new System.Drawing.Point(128, 292);
        	this.btnCancel.Name = "btnCancel";
        	this.btnCancel.Size = new System.Drawing.Size(99, 24);
        	this.btnCancel.TabIndex = 75;
        	this.btnCancel.Text = "Cancel";
        	this.btnCancel.UseVisualStyleBackColor = true;
        	// 
        	// grpColors
        	// 
        	this.grpColors.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        	        	        	| System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.grpColors.Controls.Add(this.cmbTextSize);
        	this.grpColors.Controls.Add(this.cmbChronoSize);
        	this.grpColors.Controls.Add(this.btnChronoColor);
        	this.grpColors.Controls.Add(this.btnDrawingToolChrono);
        	this.grpColors.Controls.Add(this.btnLineStyle);
        	this.grpColors.Controls.Add(this.btnPencilStyle);
        	this.grpColors.Controls.Add(this.btnAngleColor);
        	this.grpColors.Controls.Add(this.btnDrawingToolAngle2D);
        	this.grpColors.Controls.Add(this.btnCrossColor);
        	this.grpColors.Controls.Add(this.btnDrawingToolCross2D);
        	this.grpColors.Controls.Add(this.btnLineColor);
        	this.grpColors.Controls.Add(this.btnDrawingToolLine2D);
        	this.grpColors.Controls.Add(this.btnPencilColor);
        	this.grpColors.Controls.Add(this.btnDrawingToolPencil);
        	this.grpColors.Controls.Add(this.btnTextColor);
        	this.grpColors.Controls.Add(this.btnDrawingToolText);
        	this.grpColors.Location = new System.Drawing.Point(12, 36);
        	this.grpColors.Name = "grpColors";
        	this.grpColors.Size = new System.Drawing.Size(215, 243);
        	this.grpColors.TabIndex = 31;
        	this.grpColors.TabStop = false;
        	// 
        	// cmbTextSize
        	// 
        	this.cmbTextSize.FormattingEnabled = true;
        	this.cmbTextSize.Items.AddRange(new object[] {
        	        	        	"8",
        	        	        	"9",
        	        	        	"10",
        	        	        	"11",
        	        	        	"12",
        	        	        	"14",
        	        	        	"16",
        	        	        	"18",
        	        	        	"20",
        	        	        	"24",
        	        	        	"28",
        	        	        	"32",
        	        	        	"36"});
        	this.cmbTextSize.Location = new System.Drawing.Point(161, 27);
        	this.cmbTextSize.Name = "cmbTextSize";
        	this.cmbTextSize.Size = new System.Drawing.Size(40, 21);
        	this.cmbTextSize.TabIndex = 25;
        	this.cmbTextSize.Text = "12";
        	this.cmbTextSize.SelectedIndexChanged += new System.EventHandler(this.CmbTextSizeSelectedIndexChanged);
        	// 
        	// cmbChronoSize
        	// 
        	this.cmbChronoSize.FormattingEnabled = true;
        	this.cmbChronoSize.Items.AddRange(new object[] {
        	        	        	"8",
        	        	        	"9",
        	        	        	"10",
        	        	        	"11",
        	        	        	"12",
        	        	        	"14",
        	        	        	"16",
        	        	        	"18",
        	        	        	"20",
        	        	        	"24",
        	        	        	"28",
        	        	        	"32",
        	        	        	"36"});
        	this.cmbChronoSize.Location = new System.Drawing.Point(161, 202);
        	this.cmbChronoSize.Name = "cmbChronoSize";
        	this.cmbChronoSize.Size = new System.Drawing.Size(40, 21);
        	this.cmbChronoSize.TabIndex = 65;
        	this.cmbChronoSize.Text = "12";
        	this.cmbChronoSize.SelectedIndexChanged += new System.EventHandler(this.CmbChronoSizeSelectedIndexChanged);
        	// 
        	// btnChronoColor
        	// 
        	this.btnChronoColor.BackColor = System.Drawing.Color.Black;
        	this.btnChronoColor.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnChronoColor.FlatAppearance.BorderColor = System.Drawing.Color.LightGray;
        	this.btnChronoColor.FlatAppearance.BorderSize = 0;
        	this.btnChronoColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnChronoColor.Location = new System.Drawing.Point(59, 200);
        	this.btnChronoColor.Name = "btnChronoColor";
        	this.btnChronoColor.Size = new System.Drawing.Size(85, 25);
        	this.btnChronoColor.TabIndex = 60;
        	this.btnChronoColor.UseVisualStyleBackColor = false;
        	this.btnChronoColor.Click += new System.EventHandler(this.btnChronoColor_Click);
        	// 
        	// btnDrawingToolChrono
        	// 
        	this.btnDrawingToolChrono.BackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawingToolChrono.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.chrono5;
        	this.btnDrawingToolChrono.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        	this.btnDrawingToolChrono.FlatAppearance.BorderSize = 0;
        	this.btnDrawingToolChrono.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolChrono.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawingToolChrono.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnDrawingToolChrono.ForeColor = System.Drawing.Color.Black;
        	this.btnDrawingToolChrono.Location = new System.Drawing.Point(16, 200);
        	this.btnDrawingToolChrono.Name = "btnDrawingToolChrono";
        	this.btnDrawingToolChrono.Size = new System.Drawing.Size(25, 25);
        	this.btnDrawingToolChrono.TabIndex = 44;
        	this.btnDrawingToolChrono.TabStop = false;
        	this.btnDrawingToolChrono.UseVisualStyleBackColor = false;
        	// 
        	// btnLineStyle
        	// 
        	this.btnLineStyle.BackColor = System.Drawing.Color.White;
        	this.btnLineStyle.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnLineStyle.FlatAppearance.BorderColor = System.Drawing.Color.LightGray;
        	this.btnLineStyle.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
        	this.btnLineStyle.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
        	this.btnLineStyle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnLineStyle.Location = new System.Drawing.Point(161, 95);
        	this.btnLineStyle.Name = "btnLineStyle";
        	this.btnLineStyle.Size = new System.Drawing.Size(40, 25);
        	this.btnLineStyle.TabIndex = 45;
        	this.btnLineStyle.UseVisualStyleBackColor = false;
        	this.btnLineStyle.Paint += new System.Windows.Forms.PaintEventHandler(this.btnLineStyle_Paint);
        	this.btnLineStyle.Click += new System.EventHandler(this.btnLineStyle_Click);
        	// 
        	// btnPencilStyle
        	// 
        	this.btnPencilStyle.BackColor = System.Drawing.Color.White;
        	this.btnPencilStyle.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnPencilStyle.FlatAppearance.BorderColor = System.Drawing.Color.LightGray;
        	this.btnPencilStyle.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
        	this.btnPencilStyle.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
        	this.btnPencilStyle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnPencilStyle.Location = new System.Drawing.Point(161, 60);
        	this.btnPencilStyle.Name = "btnPencilStyle";
        	this.btnPencilStyle.Size = new System.Drawing.Size(40, 25);
        	this.btnPencilStyle.TabIndex = 35;
        	this.btnPencilStyle.UseVisualStyleBackColor = false;
        	this.btnPencilStyle.Paint += new System.Windows.Forms.PaintEventHandler(this.btnPencilStyle_Paint);
        	this.btnPencilStyle.Click += new System.EventHandler(this.btnPencilStyle_Click);
        	// 
        	// btnAngleColor
        	// 
        	this.btnAngleColor.BackColor = System.Drawing.Color.Black;
        	this.btnAngleColor.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnAngleColor.FlatAppearance.BorderColor = System.Drawing.Color.LightGray;
        	this.btnAngleColor.FlatAppearance.BorderSize = 0;
        	this.btnAngleColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnAngleColor.Location = new System.Drawing.Point(59, 165);
        	this.btnAngleColor.Name = "btnAngleColor";
        	this.btnAngleColor.Size = new System.Drawing.Size(85, 25);
        	this.btnAngleColor.TabIndex = 55;
        	this.btnAngleColor.UseVisualStyleBackColor = false;
        	this.btnAngleColor.Click += new System.EventHandler(this.btnAngleColor_Click);
        	// 
        	// btnDrawingToolAngle2D
        	// 
        	this.btnDrawingToolAngle2D.BackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawingToolAngle2D.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.angle5;
        	this.btnDrawingToolAngle2D.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        	this.btnDrawingToolAngle2D.FlatAppearance.BorderSize = 0;
        	this.btnDrawingToolAngle2D.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolAngle2D.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawingToolAngle2D.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnDrawingToolAngle2D.ForeColor = System.Drawing.Color.Black;
        	this.btnDrawingToolAngle2D.Location = new System.Drawing.Point(16, 165);
        	this.btnDrawingToolAngle2D.Name = "btnDrawingToolAngle2D";
        	this.btnDrawingToolAngle2D.Size = new System.Drawing.Size(25, 25);
        	this.btnDrawingToolAngle2D.TabIndex = 40;
        	this.btnDrawingToolAngle2D.TabStop = false;
        	this.btnDrawingToolAngle2D.UseVisualStyleBackColor = false;
        	// 
        	// btnCrossColor
        	// 
        	this.btnCrossColor.BackColor = System.Drawing.Color.Black;
        	this.btnCrossColor.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnCrossColor.FlatAppearance.BorderColor = System.Drawing.Color.LightGray;
        	this.btnCrossColor.FlatAppearance.BorderSize = 0;
        	this.btnCrossColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnCrossColor.Location = new System.Drawing.Point(59, 130);
        	this.btnCrossColor.Name = "btnCrossColor";
        	this.btnCrossColor.Size = new System.Drawing.Size(85, 25);
        	this.btnCrossColor.TabIndex = 50;
        	this.btnCrossColor.UseVisualStyleBackColor = false;
        	this.btnCrossColor.Click += new System.EventHandler(this.btnCrossColor_Click);
        	// 
        	// btnDrawingToolCross2D
        	// 
        	this.btnDrawingToolCross2D.BackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawingToolCross2D.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.cross5;
        	this.btnDrawingToolCross2D.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnDrawingToolCross2D.FlatAppearance.BorderSize = 0;
        	this.btnDrawingToolCross2D.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolCross2D.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawingToolCross2D.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnDrawingToolCross2D.ForeColor = System.Drawing.Color.Black;
        	this.btnDrawingToolCross2D.Location = new System.Drawing.Point(16, 130);
        	this.btnDrawingToolCross2D.Name = "btnDrawingToolCross2D";
        	this.btnDrawingToolCross2D.Size = new System.Drawing.Size(25, 25);
        	this.btnDrawingToolCross2D.TabIndex = 38;
        	this.btnDrawingToolCross2D.TabStop = false;
        	this.btnDrawingToolCross2D.UseVisualStyleBackColor = false;
        	// 
        	// btnLineColor
        	// 
        	this.btnLineColor.BackColor = System.Drawing.Color.Black;
        	this.btnLineColor.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnLineColor.FlatAppearance.BorderColor = System.Drawing.Color.LightGray;
        	this.btnLineColor.FlatAppearance.BorderSize = 0;
        	this.btnLineColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnLineColor.Location = new System.Drawing.Point(59, 95);
        	this.btnLineColor.Name = "btnLineColor";
        	this.btnLineColor.Size = new System.Drawing.Size(85, 25);
        	this.btnLineColor.TabIndex = 40;
        	this.btnLineColor.UseVisualStyleBackColor = false;
        	this.btnLineColor.Click += new System.EventHandler(this.btnLineColor_Click);
        	// 
        	// btnDrawingToolLine2D
        	// 
        	this.btnDrawingToolLine2D.BackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawingToolLine2D.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.line6;
        	this.btnDrawingToolLine2D.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnDrawingToolLine2D.FlatAppearance.BorderSize = 0;
        	this.btnDrawingToolLine2D.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolLine2D.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawingToolLine2D.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnDrawingToolLine2D.ForeColor = System.Drawing.Color.Black;
        	this.btnDrawingToolLine2D.Location = new System.Drawing.Point(16, 95);
        	this.btnDrawingToolLine2D.Name = "btnDrawingToolLine2D";
        	this.btnDrawingToolLine2D.Size = new System.Drawing.Size(25, 25);
        	this.btnDrawingToolLine2D.TabIndex = 36;
        	this.btnDrawingToolLine2D.TabStop = false;
        	this.btnDrawingToolLine2D.UseVisualStyleBackColor = false;
        	// 
        	// btnPencilColor
        	// 
        	this.btnPencilColor.BackColor = System.Drawing.Color.Black;
        	this.btnPencilColor.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnPencilColor.FlatAppearance.BorderColor = System.Drawing.Color.LightGray;
        	this.btnPencilColor.FlatAppearance.BorderSize = 0;
        	this.btnPencilColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnPencilColor.Location = new System.Drawing.Point(59, 60);
        	this.btnPencilColor.Name = "btnPencilColor";
        	this.btnPencilColor.Size = new System.Drawing.Size(85, 25);
        	this.btnPencilColor.TabIndex = 30;
        	this.btnPencilColor.UseVisualStyleBackColor = false;
        	this.btnPencilColor.Click += new System.EventHandler(this.btnPencilColor_Click);
        	// 
        	// btnDrawingToolPencil
        	// 
        	this.btnDrawingToolPencil.BackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawingToolPencil.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.activepencil;
        	this.btnDrawingToolPencil.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnDrawingToolPencil.FlatAppearance.BorderSize = 0;
        	this.btnDrawingToolPencil.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolPencil.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawingToolPencil.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnDrawingToolPencil.ForeColor = System.Drawing.Color.Black;
        	this.btnDrawingToolPencil.Location = new System.Drawing.Point(16, 60);
        	this.btnDrawingToolPencil.Name = "btnDrawingToolPencil";
        	this.btnDrawingToolPencil.Size = new System.Drawing.Size(25, 25);
        	this.btnDrawingToolPencil.TabIndex = 34;
        	this.btnDrawingToolPencil.TabStop = false;
        	this.btnDrawingToolPencil.UseVisualStyleBackColor = false;
        	// 
        	// btnTextColor
        	// 
        	this.btnTextColor.BackColor = System.Drawing.Color.Black;
        	this.btnTextColor.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnTextColor.FlatAppearance.BorderColor = System.Drawing.Color.LightGray;
        	this.btnTextColor.FlatAppearance.BorderSize = 0;
        	this.btnTextColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnTextColor.Location = new System.Drawing.Point(59, 25);
        	this.btnTextColor.Name = "btnTextColor";
        	this.btnTextColor.Size = new System.Drawing.Size(85, 25);
        	this.btnTextColor.TabIndex = 20;
        	this.btnTextColor.UseVisualStyleBackColor = false;
        	this.btnTextColor.Click += new System.EventHandler(this.btnTextColor_Click);
        	// 
        	// btnDrawingToolText
        	// 
        	this.btnDrawingToolText.BackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawingToolText.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.TextToolIcon;
        	this.btnDrawingToolText.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnDrawingToolText.FlatAppearance.BorderSize = 0;
        	this.btnDrawingToolText.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnDrawingToolText.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDrawingToolText.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnDrawingToolText.ForeColor = System.Drawing.Color.Black;
        	this.btnDrawingToolText.Location = new System.Drawing.Point(16, 25);
        	this.btnDrawingToolText.Name = "btnDrawingToolText";
        	this.btnDrawingToolText.Size = new System.Drawing.Size(25, 25);
        	this.btnDrawingToolText.TabIndex = 32;
        	this.btnDrawingToolText.TabStop = false;
        	this.btnDrawingToolText.UseVisualStyleBackColor = false;
        	// 
        	// btnSaveProfile
        	// 
        	this.btnSaveProfile.FlatAppearance.BorderSize = 0;
        	this.btnSaveProfile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
        	this.btnSaveProfile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
        	this.btnSaveProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnSaveProfile.Image = global::Kinovea.ScreenManager.Properties.Resources.filesave;
        	this.btnSaveProfile.Location = new System.Drawing.Point(43, 5);
        	this.btnSaveProfile.Name = "btnSaveProfile";
        	this.btnSaveProfile.Size = new System.Drawing.Size(25, 25);
        	this.btnSaveProfile.TabIndex = 10;
        	this.btnSaveProfile.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        	this.btnSaveProfile.UseVisualStyleBackColor = true;
        	this.btnSaveProfile.Click += new System.EventHandler(this.btnSaveProfile_Click);
        	// 
        	// btnLoadProfile
        	// 
        	this.btnLoadProfile.FlatAppearance.BorderSize = 0;
        	this.btnLoadProfile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
        	this.btnLoadProfile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
        	this.btnLoadProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnLoadProfile.Image = global::Kinovea.ScreenManager.Properties.Resources.folder_new;
        	this.btnLoadProfile.Location = new System.Drawing.Point(12, 5);
        	this.btnLoadProfile.Name = "btnLoadProfile";
        	this.btnLoadProfile.Size = new System.Drawing.Size(25, 25);
        	this.btnLoadProfile.TabIndex = 5;
        	this.btnLoadProfile.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        	this.btnLoadProfile.UseVisualStyleBackColor = true;
        	this.btnLoadProfile.Click += new System.EventHandler(this.btnLoadProfile_Click);
        	// 
        	// btnDefaultProfile
        	// 
        	this.btnDefaultProfile.FlatAppearance.BorderSize = 0;
        	this.btnDefaultProfile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
        	this.btnDefaultProfile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
        	this.btnDefaultProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnDefaultProfile.Image = global::Kinovea.ScreenManager.Properties.Resources.bin_empty;
        	this.btnDefaultProfile.Location = new System.Drawing.Point(202, 5);
        	this.btnDefaultProfile.Name = "btnDefaultProfile";
        	this.btnDefaultProfile.Size = new System.Drawing.Size(25, 25);
        	this.btnDefaultProfile.TabIndex = 15;
        	this.btnDefaultProfile.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        	this.btnDefaultProfile.UseVisualStyleBackColor = true;
        	this.btnDefaultProfile.Click += new System.EventHandler(this.btnDefaults_Click);
        	// 
        	// formColorProfile
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackColor = System.Drawing.Color.White;
        	this.ClientSize = new System.Drawing.Size(240, 328);
        	this.Controls.Add(this.btnDefaultProfile);
        	this.Controls.Add(this.grpColors);
        	this.Controls.Add(this.btnSaveProfile);
        	this.Controls.Add(this.btnLoadProfile);
        	this.Controls.Add(this.btnApply);
        	this.Controls.Add(this.btnCancel);
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
        	this.Name = "formColorProfile";
        	this.ShowInTaskbar = false;
        	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        	this.Text = "   Color Profile...";
        	this.grpColors.ResumeLayout(false);
        	this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnLoadProfile;
        private System.Windows.Forms.Button btnSaveProfile;
        private System.Windows.Forms.GroupBox grpColors;
        private System.Windows.Forms.Button btnDrawingToolText;
        private System.Windows.Forms.Button btnTextColor;
        private System.Windows.Forms.Button btnPencilColor;
        private System.Windows.Forms.Button btnDrawingToolPencil;
        private System.Windows.Forms.Button btnLineColor;
        private System.Windows.Forms.Button btnDrawingToolLine2D;
        private System.Windows.Forms.Button btnCrossColor;
        private System.Windows.Forms.Button btnDrawingToolCross2D;
        private System.Windows.Forms.Button btnDrawingToolAngle2D;
        private System.Windows.Forms.Button btnAngleColor;
        private System.Windows.Forms.ToolTip toolTips;
        private System.Windows.Forms.Button btnDefaultProfile;
        private System.Windows.Forms.Button btnPencilStyle;
        private System.Windows.Forms.Button btnLineStyle;
        private System.Windows.Forms.Button btnChronoColor;
        private System.Windows.Forms.Button btnDrawingToolChrono;
        private System.Windows.Forms.ComboBox cmbChronoSize;
        private System.Windows.Forms.ComboBox cmbTextSize;
    }
}