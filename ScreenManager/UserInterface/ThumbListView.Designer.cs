namespace Kinovea.ScreenManager
{
    partial class ThumbListView
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
        	this.splitResizeBar = new System.Windows.Forms.SplitContainer();
        	this.btnLarge = new System.Windows.Forms.Button();
        	this.btnMedium = new System.Windows.Forms.Button();
        	this.btnSmall = new System.Windows.Forms.Button();
        	this.btnExtraLarge = new System.Windows.Forms.Button();
        	this.btnExtraSmall = new System.Windows.Forms.Button();
        	this.btnHideThumbView = new System.Windows.Forms.Button();
        	this.lblZoomTuner = new System.Windows.Forms.Label();
        	this.btnClose = new System.Windows.Forms.Button();
        	this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
        	this.bgThumbsLoader = new System.ComponentModel.BackgroundWorker();
        	this.splitResizeBar.Panel1.SuspendLayout();
        	this.splitResizeBar.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// splitResizeBar
        	// 
        	this.splitResizeBar.BackColor = System.Drawing.Color.White;
        	this.splitResizeBar.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.splitResizeBar.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
        	this.splitResizeBar.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
        	this.splitResizeBar.IsSplitterFixed = true;
        	this.splitResizeBar.Location = new System.Drawing.Point(0, 0);
        	this.splitResizeBar.Name = "splitResizeBar";
        	this.splitResizeBar.Orientation = System.Windows.Forms.Orientation.Horizontal;
        	// 
        	// splitResizeBar.Panel1
        	// 
        	this.splitResizeBar.Panel1.BackColor = System.Drawing.Color.White;
        	this.splitResizeBar.Panel1.Controls.Add(this.btnLarge);
        	this.splitResizeBar.Panel1.Controls.Add(this.btnMedium);
        	this.splitResizeBar.Panel1.Controls.Add(this.btnSmall);
        	this.splitResizeBar.Panel1.Controls.Add(this.btnExtraLarge);
        	this.splitResizeBar.Panel1.Controls.Add(this.btnExtraSmall);
        	this.splitResizeBar.Panel1.Controls.Add(this.btnHideThumbView);
        	this.splitResizeBar.Panel1.Controls.Add(this.lblZoomTuner);
        	this.splitResizeBar.Panel1.Controls.Add(this.btnClose);
        	// 
        	// splitResizeBar.Panel2
        	// 
        	this.splitResizeBar.Panel2.AutoScroll = true;
        	this.splitResizeBar.Panel2.BackColor = System.Drawing.Color.White;
        	this.splitResizeBar.Panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.Panel2Paint);
        	this.splitResizeBar.Panel2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Panel2MouseDown);
        	this.splitResizeBar.Panel2.Resize += new System.EventHandler(this.splitResizeBar_Panel2_Resize);
        	this.splitResizeBar.Panel2.MouseEnter += new System.EventHandler(this.Panel2MouseEnter);
        	this.splitResizeBar.Size = new System.Drawing.Size(661, 406);
        	this.splitResizeBar.SplitterDistance = 30;
        	this.splitResizeBar.TabIndex = 0;
        	// 
        	// btnLarge
        	// 
        	this.btnLarge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        	this.btnLarge.BackColor = System.Drawing.Color.LightSteelBlue;
        	this.btnLarge.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnLarge.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnLarge.FlatAppearance.BorderSize = 0;
        	this.btnLarge.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightSteelBlue;
        	this.btnLarge.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
        	this.btnLarge.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnLarge.Location = new System.Drawing.Point(87, 10);
        	this.btnLarge.Name = "btnLarge";
        	this.btnLarge.Size = new System.Drawing.Size(20, 15);
        	this.btnLarge.TabIndex = 17;
        	this.btnLarge.UseVisualStyleBackColor = false;
        	this.btnLarge.Click += new System.EventHandler(this.btnLarge_Click);
        	// 
        	// btnMedium
        	// 
        	this.btnMedium.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        	this.btnMedium.BackColor = System.Drawing.Color.SteelBlue;
        	this.btnMedium.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnMedium.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnMedium.FlatAppearance.BorderSize = 0;
        	this.btnMedium.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightSteelBlue;
        	this.btnMedium.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
        	this.btnMedium.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnMedium.Location = new System.Drawing.Point(65, 13);
        	this.btnMedium.Name = "btnMedium";
        	this.btnMedium.Size = new System.Drawing.Size(16, 12);
        	this.btnMedium.TabIndex = 16;
        	this.btnMedium.UseVisualStyleBackColor = false;
        	this.btnMedium.Click += new System.EventHandler(this.btnMedium_Click);
        	// 
        	// btnSmall
        	// 
        	this.btnSmall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        	this.btnSmall.BackColor = System.Drawing.Color.SteelBlue;
        	this.btnSmall.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnSmall.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnSmall.FlatAppearance.BorderSize = 0;
        	this.btnSmall.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightSteelBlue;
        	this.btnSmall.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
        	this.btnSmall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnSmall.Location = new System.Drawing.Point(47, 16);
        	this.btnSmall.Name = "btnSmall";
        	this.btnSmall.Size = new System.Drawing.Size(12, 9);
        	this.btnSmall.TabIndex = 15;
        	this.btnSmall.UseVisualStyleBackColor = false;
        	this.btnSmall.Click += new System.EventHandler(this.btnSmall_Click);
        	// 
        	// btnExtraLarge
        	// 
        	this.btnExtraLarge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        	this.btnExtraLarge.BackColor = System.Drawing.Color.SteelBlue;
        	this.btnExtraLarge.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnExtraLarge.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnExtraLarge.FlatAppearance.BorderSize = 0;
        	this.btnExtraLarge.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightSteelBlue;
        	this.btnExtraLarge.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
        	this.btnExtraLarge.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnExtraLarge.Location = new System.Drawing.Point(113, 7);
        	this.btnExtraLarge.Name = "btnExtraLarge";
        	this.btnExtraLarge.Size = new System.Drawing.Size(24, 18);
        	this.btnExtraLarge.TabIndex = 14;
        	this.btnExtraLarge.UseVisualStyleBackColor = false;
        	this.btnExtraLarge.Click += new System.EventHandler(this.btnExtraLarge_Click);
        	// 
        	// btnExtraSmall
        	// 
        	this.btnExtraSmall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        	this.btnExtraSmall.BackColor = System.Drawing.Color.SteelBlue;
        	this.btnExtraSmall.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnExtraSmall.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnExtraSmall.FlatAppearance.BorderSize = 0;
        	this.btnExtraSmall.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightSteelBlue;
        	this.btnExtraSmall.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
        	this.btnExtraSmall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnExtraSmall.Location = new System.Drawing.Point(33, 19);
        	this.btnExtraSmall.Name = "btnExtraSmall";
        	this.btnExtraSmall.Size = new System.Drawing.Size(8, 6);
        	this.btnExtraSmall.TabIndex = 13;
        	this.btnExtraSmall.UseVisualStyleBackColor = false;
        	this.btnExtraSmall.Click += new System.EventHandler(this.btnExtraSmall_Click);
        	// 
        	// btnHideThumbView
        	// 
        	this.btnHideThumbView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnHideThumbView.AutoSize = true;
        	this.btnHideThumbView.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        	this.btnHideThumbView.BackColor = System.Drawing.Color.Transparent;
        	this.btnHideThumbView.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnHideThumbView.FlatAppearance.BorderSize = 0;
        	this.btnHideThumbView.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnHideThumbView.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnHideThumbView.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.btnHideThumbView.ForeColor = System.Drawing.Color.SteelBlue;
        	this.btnHideThumbView.Location = new System.Drawing.Point(518, 4);
        	this.btnHideThumbView.Name = "btnHideThumbView";
        	this.btnHideThumbView.Size = new System.Drawing.Size(109, 24);
        	this.btnHideThumbView.TabIndex = 12;
        	this.btnHideThumbView.Text = "Hide Thumbnails";
        	this.btnHideThumbView.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        	this.btnHideThumbView.UseVisualStyleBackColor = false;
        	this.btnHideThumbView.Click += new System.EventHandler(this.btnShowThumbView_Click);
        	// 
        	// lblZoomTuner
        	// 
        	this.lblZoomTuner.AutoSize = true;
        	this.lblZoomTuner.BackColor = System.Drawing.Color.White;
        	this.lblZoomTuner.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.lblZoomTuner.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.lblZoomTuner.ForeColor = System.Drawing.Color.Black;
        	this.lblZoomTuner.Location = new System.Drawing.Point(219, 14);
        	this.lblZoomTuner.Margin = new System.Windows.Forms.Padding(0);
        	this.lblZoomTuner.Name = "lblZoomTuner";
        	this.lblZoomTuner.Size = new System.Drawing.Size(78, 14);
        	this.lblZoomTuner.TabIndex = 11;
        	this.lblZoomTuner.Text = "Zoom : 100 %";
        	this.lblZoomTuner.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        	this.lblZoomTuner.Visible = false;
        	// 
        	// btnClose
        	// 
        	this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Right;
        	this.btnClose.BackColor = System.Drawing.Color.Transparent;
        	this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnClose.FlatAppearance.BorderSize = 0;
        	this.btnClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
        	this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnClose.Image = global::Kinovea.ScreenManager.Properties.Resources.closegrey;
        	this.btnClose.Location = new System.Drawing.Point(630, 3);
        	this.btnClose.Name = "btnClose";
        	this.btnClose.Size = new System.Drawing.Size(20, 20);
        	this.btnClose.TabIndex = 4;
        	this.btnClose.UseVisualStyleBackColor = false;
        	this.btnClose.Visible = false;
        	this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
        	// 
        	// bgThumbsLoader
        	// 
        	this.bgThumbsLoader.WorkerReportsProgress = true;
        	this.bgThumbsLoader.WorkerSupportsCancellation = true;
        	// 
        	// ThumbListView
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
        	this.Controls.Add(this.splitResizeBar);
        	this.Name = "ThumbListView";
        	this.Size = new System.Drawing.Size(661, 406);
        	this.splitResizeBar.Panel1.ResumeLayout(false);
        	this.splitResizeBar.Panel1.PerformLayout();
        	this.splitResizeBar.ResumeLayout(false);
        	this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitResizeBar;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblZoomTuner;
        private System.Windows.Forms.Button btnHideThumbView;
        private System.Windows.Forms.Button btnExtraSmall;
        private System.Windows.Forms.Button btnLarge;
        private System.Windows.Forms.Button btnMedium;
        private System.Windows.Forms.Button btnSmall;
        private System.Windows.Forms.Button btnExtraLarge;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.ComponentModel.BackgroundWorker bgThumbsLoader;
    }
}
