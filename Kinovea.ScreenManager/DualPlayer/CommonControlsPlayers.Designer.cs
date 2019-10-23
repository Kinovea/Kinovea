namespace Kinovea.ScreenManager
{
    partial class CommonControlsPlayers
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
      this.lblInfo = new System.Windows.Forms.Label();
      this.toolTips = new System.Windows.Forms.ToolTip(this.components);
      this.trkFrame = new Kinovea.ScreenManager.FrameTracker();
      this.btnMerge = new System.Windows.Forms.Button();
      this.btnSync = new System.Windows.Forms.Button();
      this.btnSwap = new System.Windows.Forms.Button();
      this.buttonGotoFirst = new System.Windows.Forms.Button();
      this.buttonGotoPrevious = new System.Windows.Forms.Button();
      this.buttonGotoNext = new System.Windows.Forms.Button();
      this.buttonGotoLast = new System.Windows.Forms.Button();
      this.buttonPlay = new System.Windows.Forms.Button();
      this.btnSnapshot = new System.Windows.Forms.Button();
      this.btnDualSave = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // lblInfo
      // 
      this.lblInfo.AutoSize = true;
      this.lblInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblInfo.Location = new System.Drawing.Point(14, 15);
      this.lblInfo.Name = "lblInfo";
      this.lblInfo.Size = new System.Drawing.Size(83, 12);
      this.lblInfo.TabIndex = 10;
      this.lblInfo.Text = "Common controls :";
      // 
      // trkFrame
      // 
      this.trkFrame.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.trkFrame.Cursor = System.Windows.Forms.Cursors.Hand;
      this.trkFrame.LeftHairline = ((long)(0));
      this.trkFrame.Location = new System.Drawing.Point(383, 13);
      this.trkFrame.Maximum = ((long)(0));
      this.trkFrame.Minimum = ((long)(0));
      this.trkFrame.MinimumSize = new System.Drawing.Size(50, 20);
      this.trkFrame.Name = "trkFrame";
      this.trkFrame.Position = ((long)(0));
      this.trkFrame.RightHairline = ((long)(0));
      this.trkFrame.Size = new System.Drawing.Size(212, 20);
      this.trkFrame.TabIndex = 14;
      this.trkFrame.PositionChanging += new System.EventHandler<Kinovea.ScreenManager.PositionChangedEventArgs>(this.trkFrame_PositionChanged);
      this.trkFrame.PositionChanged += new System.EventHandler<Kinovea.ScreenManager.PositionChangedEventArgs>(this.trkFrame_PositionChanged);
      // 
      // btnMerge
      // 
      this.btnMerge.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnMerge.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnMerge.FlatAppearance.BorderSize = 0;
      this.btnMerge.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnMerge.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnMerge.Image = global::Kinovea.ScreenManager.Properties.Resources.syncmerge;
      this.btnMerge.Location = new System.Drawing.Point(320, 12);
      this.btnMerge.Margin = new System.Windows.Forms.Padding(0);
      this.btnMerge.MinimumSize = new System.Drawing.Size(25, 25);
      this.btnMerge.Name = "btnMerge";
      this.btnMerge.Size = new System.Drawing.Size(30, 25);
      this.btnMerge.TabIndex = 16;
      this.btnMerge.UseVisualStyleBackColor = false;
      this.btnMerge.Click += new System.EventHandler(this.btnMerge_Click);
      // 
      // btnSync
      // 
      this.btnSync.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnSync.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnSync.FlatAppearance.BorderSize = 0;
      this.btnSync.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnSync.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnSync.Image = global::Kinovea.ScreenManager.Properties.Resources.marker;
      this.btnSync.Location = new System.Drawing.Point(290, 10);
      this.btnSync.Margin = new System.Windows.Forms.Padding(0);
      this.btnSync.MinimumSize = new System.Drawing.Size(25, 25);
      this.btnSync.Name = "btnSync";
      this.btnSync.Size = new System.Drawing.Size(30, 25);
      this.btnSync.TabIndex = 12;
      this.btnSync.UseVisualStyleBackColor = false;
      this.btnSync.Click += new System.EventHandler(this.btnSync_Click);
      // 
      // btnSwap
      // 
      this.btnSwap.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnSwap.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnSwap.FlatAppearance.BorderSize = 0;
      this.btnSwap.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnSwap.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnSwap.Image = global::Kinovea.ScreenManager.Properties.Resources.flatswap3d;
      this.btnSwap.Location = new System.Drawing.Point(350, 12);
      this.btnSwap.Margin = new System.Windows.Forms.Padding(0);
      this.btnSwap.MinimumSize = new System.Drawing.Size(25, 25);
      this.btnSwap.Name = "btnSwap";
      this.btnSwap.Size = new System.Drawing.Size(30, 25);
      this.btnSwap.TabIndex = 11;
      this.btnSwap.UseVisualStyleBackColor = true;
      this.btnSwap.Click += new System.EventHandler(this.btnSwap_Click);
      // 
      // buttonGotoFirst
      // 
      this.buttonGotoFirst.BackColor = System.Drawing.Color.Transparent;
      this.buttonGotoFirst.Cursor = System.Windows.Forms.Cursors.Hand;
      this.buttonGotoFirst.FlatAppearance.BorderSize = 0;
      this.buttonGotoFirst.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.buttonGotoFirst.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.buttonGotoFirst.Image = global::Kinovea.ScreenManager.Properties.Resources.flatstart3;
      this.buttonGotoFirst.Location = new System.Drawing.Point(114, 10);
      this.buttonGotoFirst.MinimumSize = new System.Drawing.Size(25, 25);
      this.buttonGotoFirst.Name = "buttonGotoFirst";
      this.buttonGotoFirst.Size = new System.Drawing.Size(30, 25);
      this.buttonGotoFirst.TabIndex = 9;
      this.buttonGotoFirst.UseVisualStyleBackColor = false;
      this.buttonGotoFirst.Click += new System.EventHandler(this.buttonGotoFirst_Click);
      // 
      // buttonGotoPrevious
      // 
      this.buttonGotoPrevious.BackColor = System.Drawing.Color.Transparent;
      this.buttonGotoPrevious.Cursor = System.Windows.Forms.Cursors.Hand;
      this.buttonGotoPrevious.FlatAppearance.BorderSize = 0;
      this.buttonGotoPrevious.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.buttonGotoPrevious.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.buttonGotoPrevious.Image = global::Kinovea.ScreenManager.Properties.Resources.flatprev3;
      this.buttonGotoPrevious.Location = new System.Drawing.Point(144, 10);
      this.buttonGotoPrevious.MinimumSize = new System.Drawing.Size(25, 25);
      this.buttonGotoPrevious.Name = "buttonGotoPrevious";
      this.buttonGotoPrevious.Size = new System.Drawing.Size(30, 25);
      this.buttonGotoPrevious.TabIndex = 8;
      this.buttonGotoPrevious.UseVisualStyleBackColor = false;
      this.buttonGotoPrevious.Click += new System.EventHandler(this.buttonGotoPrevious_Click);
      // 
      // buttonGotoNext
      // 
      this.buttonGotoNext.BackColor = System.Drawing.Color.Transparent;
      this.buttonGotoNext.Cursor = System.Windows.Forms.Cursors.Hand;
      this.buttonGotoNext.FlatAppearance.BorderSize = 0;
      this.buttonGotoNext.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.buttonGotoNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.buttonGotoNext.Image = global::Kinovea.ScreenManager.Properties.Resources.flatnext3;
      this.buttonGotoNext.Location = new System.Drawing.Point(214, 10);
      this.buttonGotoNext.MinimumSize = new System.Drawing.Size(25, 25);
      this.buttonGotoNext.Name = "buttonGotoNext";
      this.buttonGotoNext.Size = new System.Drawing.Size(30, 25);
      this.buttonGotoNext.TabIndex = 7;
      this.buttonGotoNext.UseVisualStyleBackColor = false;
      this.buttonGotoNext.Click += new System.EventHandler(this.buttonGotoNext_Click);
      // 
      // buttonGotoLast
      // 
      this.buttonGotoLast.BackColor = System.Drawing.Color.Transparent;
      this.buttonGotoLast.Cursor = System.Windows.Forms.Cursors.Hand;
      this.buttonGotoLast.FlatAppearance.BorderSize = 0;
      this.buttonGotoLast.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.buttonGotoLast.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.buttonGotoLast.Image = global::Kinovea.ScreenManager.Properties.Resources.flatend3;
      this.buttonGotoLast.Location = new System.Drawing.Point(244, 10);
      this.buttonGotoLast.MinimumSize = new System.Drawing.Size(25, 25);
      this.buttonGotoLast.Name = "buttonGotoLast";
      this.buttonGotoLast.Size = new System.Drawing.Size(30, 25);
      this.buttonGotoLast.TabIndex = 6;
      this.buttonGotoLast.UseVisualStyleBackColor = false;
      this.buttonGotoLast.Click += new System.EventHandler(this.buttonGotoLast_Click);
      // 
      // buttonPlay
      // 
      this.buttonPlay.BackColor = System.Drawing.Color.Transparent;
      this.buttonPlay.Cursor = System.Windows.Forms.Cursors.Hand;
      this.buttonPlay.FlatAppearance.BorderSize = 0;
      this.buttonPlay.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.buttonPlay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.buttonPlay.Image = global::Kinovea.ScreenManager.Properties.Player.flatplay;
      this.buttonPlay.Location = new System.Drawing.Point(174, 7);
      this.buttonPlay.MinimumSize = new System.Drawing.Size(30, 25);
      this.buttonPlay.Name = "buttonPlay";
      this.buttonPlay.Size = new System.Drawing.Size(40, 30);
      this.buttonPlay.TabIndex = 5;
      this.buttonPlay.UseVisualStyleBackColor = false;
      this.buttonPlay.Click += new System.EventHandler(this.buttonPlay_Click);
      // 
      // btnSnapshot
      // 
      this.btnSnapshot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnSnapshot.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnSnapshot.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnSnapshot.FlatAppearance.BorderSize = 0;
      this.btnSnapshot.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnSnapshot.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnSnapshot.Image = global::Kinovea.ScreenManager.Properties.Resources.camerasingle;
      this.btnSnapshot.Location = new System.Drawing.Point(594, 10);
      this.btnSnapshot.Margin = new System.Windows.Forms.Padding(0);
      this.btnSnapshot.MinimumSize = new System.Drawing.Size(25, 25);
      this.btnSnapshot.Name = "btnSnapshot";
      this.btnSnapshot.Size = new System.Drawing.Size(25, 25);
      this.btnSnapshot.TabIndex = 17;
      this.btnSnapshot.UseVisualStyleBackColor = false;
      this.btnSnapshot.Click += new System.EventHandler(this.btnSnapshot_Click);
      // 
      // btnDualSave
      // 
      this.btnDualSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnDualSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnDualSave.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnDualSave.FlatAppearance.BorderSize = 0;
      this.btnDualSave.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnDualSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnDualSave.Image = global::Kinovea.ScreenManager.Properties.Resources.film_save;
      this.btnDualSave.Location = new System.Drawing.Point(627, 10);
      this.btnDualSave.Margin = new System.Windows.Forms.Padding(0);
      this.btnDualSave.MinimumSize = new System.Drawing.Size(25, 25);
      this.btnDualSave.Name = "btnDualSave";
      this.btnDualSave.Size = new System.Drawing.Size(25, 25);
      this.btnDualSave.TabIndex = 18;
      this.btnDualSave.UseVisualStyleBackColor = false;
      this.btnDualSave.Click += new System.EventHandler(this.btnDualVideo_Click);
      // 
      // CommonControlsPlayers
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.Controls.Add(this.btnDualSave);
      this.Controls.Add(this.btnSnapshot);
      this.Controls.Add(this.btnMerge);
      this.Controls.Add(this.trkFrame);
      this.Controls.Add(this.btnSync);
      this.Controls.Add(this.btnSwap);
      this.Controls.Add(this.lblInfo);
      this.Controls.Add(this.buttonGotoFirst);
      this.Controls.Add(this.buttonGotoPrevious);
      this.Controls.Add(this.buttonGotoNext);
      this.Controls.Add(this.buttonGotoLast);
      this.Controls.Add(this.buttonPlay);
      this.Name = "CommonControlsPlayers";
      this.Size = new System.Drawing.Size(665, 45);
      this.ResumeLayout(false);
      this.PerformLayout();

        }
        private System.Windows.Forms.Button btnMerge;

        #endregion

        private System.Windows.Forms.Button buttonGotoFirst;
        private System.Windows.Forms.Button buttonGotoPrevious;
        private System.Windows.Forms.Button buttonGotoNext;
        private System.Windows.Forms.Button buttonGotoLast;
        private System.Windows.Forms.Button buttonPlay;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Button btnSwap;
        private System.Windows.Forms.ToolTip toolTips;
        private System.Windows.Forms.Button btnSync;
        public Kinovea.ScreenManager.FrameTracker trkFrame;
        private System.Windows.Forms.Button btnSnapshot;
        private System.Windows.Forms.Button btnDualSave;
    }
}
