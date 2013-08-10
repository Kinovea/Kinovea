namespace Kinovea.ScreenManager
{
    partial class ScreenManagerUserInterface
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScreenManagerUserInterface));
            this.pnlScreens = new System.Windows.Forms.Panel();
            this.splitScreensPanel = new System.Windows.Forms.SplitContainer();
            this.splitScreens = new System.Windows.Forms.SplitContainer();
            this.commonControls = new Kinovea.ScreenManager.CommonControls();
            this.pbLogo = new System.Windows.Forms.PictureBox();
            this.pnlScreens.SuspendLayout();
            this.splitScreensPanel.Panel1.SuspendLayout();
            this.splitScreensPanel.Panel2.SuspendLayout();
            this.splitScreensPanel.SuspendLayout();
            this.splitScreens.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlScreens
            // 
            this.pnlScreens.Controls.Add(this.splitScreensPanel);
            this.pnlScreens.Location = new System.Drawing.Point(14, 34);
            this.pnlScreens.Margin = new System.Windows.Forms.Padding(1);
            this.pnlScreens.Name = "pnlScreens";
            this.pnlScreens.Size = new System.Drawing.Size(574, 367);
            this.pnlScreens.TabIndex = 2;
            this.pnlScreens.Visible = false;
            this.pnlScreens.Resize += new System.EventHandler(this.pnlScreens_Resize);
            // 
            // splitScreensPanel
            // 
            this.splitScreensPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitScreensPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitScreensPanel.IsSplitterFixed = true;
            this.splitScreensPanel.Location = new System.Drawing.Point(0, 0);
            this.splitScreensPanel.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.splitScreensPanel.Name = "splitScreensPanel";
            this.splitScreensPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitScreensPanel.Panel1
            // 
            this.splitScreensPanel.Panel1.Controls.Add(this.splitScreens);
            // 
            // splitScreensPanel.Panel2
            // 
            this.splitScreensPanel.Panel2.BackColor = System.Drawing.Color.White;
            this.splitScreensPanel.Panel2.Controls.Add(this.commonControls);
            this.splitScreensPanel.Size = new System.Drawing.Size(574, 367);
            this.splitScreensPanel.SplitterDistance = 315;
            this.splitScreensPanel.TabIndex = 0;
            // 
            // splitScreens
            // 
            this.splitScreens.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitScreens.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitScreens.Location = new System.Drawing.Point(0, 0);
            this.splitScreens.Name = "splitScreens";
            // 
            // splitScreens.Panel1
            // 
            this.splitScreens.Panel1.BackColor = System.Drawing.Color.White;
            this.splitScreens.Panel1.DragDrop += new System.Windows.Forms.DragEventHandler(this.splitScreens_Panel1_DragDrop);
            this.splitScreens.Panel1.DragOver += new System.Windows.Forms.DragEventHandler(this.DroppableArea_DragOver);
            // 
            // splitScreens.Panel2
            // 
            this.splitScreens.Panel2.BackColor = System.Drawing.Color.White;
            this.splitScreens.Panel2.DragDrop += new System.Windows.Forms.DragEventHandler(this.splitScreens_Panel2_DragDrop);
            this.splitScreens.Panel2.DragOver += new System.Windows.Forms.DragEventHandler(this.DroppableArea_DragOver);
            this.splitScreens.Size = new System.Drawing.Size(574, 315);
            this.splitScreens.SplitterDistance = 287;
            this.splitScreens.TabIndex = 0;
            // 
            // commonControls
            // 
            this.commonControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.commonControls.BackColor = System.Drawing.Color.White;
            this.commonControls.Location = new System.Drawing.Point(0, 0);
            this.commonControls.Name = "commonControls";
            this.commonControls.Playing = false;
            this.commonControls.Size = new System.Drawing.Size(545, 45);
            this.commonControls.SyncMerging = false;
            this.commonControls.TabIndex = 4;
            // 
            // pbLogo
            // 
            this.pbLogo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pbLogo.BackColor = System.Drawing.Color.Transparent;
            this.pbLogo.Image = ((System.Drawing.Image)(resources.GetObject("pbLogo.Image")));
            this.pbLogo.InitialImage = null;
            this.pbLogo.Location = new System.Drawing.Point(327, 422);
            this.pbLogo.Name = "pbLogo";
            this.pbLogo.Size = new System.Drawing.Size(362, 126);
            this.pbLogo.TabIndex = 1;
            this.pbLogo.TabStop = false;
            this.pbLogo.Visible = false;
            // 
            // ScreenManagerUserInterface
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.Controls.Add(this.pnlScreens);
            this.Controls.Add(this.pbLogo);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "ScreenManagerUserInterface";
            this.Size = new System.Drawing.Size(720, 560);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.ScreenManagerUserInterface_DragDrop);
            this.DragOver += new System.Windows.Forms.DragEventHandler(this.DroppableArea_DragOver);
            this.DoubleClick += new System.EventHandler(this.ScreenManagerUserInterface_DoubleClick);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ScreenManagerUserInterface_KeyDown);
            this.pnlScreens.ResumeLayout(false);
            this.splitScreensPanel.Panel1.ResumeLayout(false);
            this.splitScreensPanel.Panel2.ResumeLayout(false);
            this.splitScreensPanel.ResumeLayout(false);
            this.splitScreens.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbLogo;
        private System.Windows.Forms.SplitContainer splitScreensPanel;
        private System.Windows.Forms.SplitContainer splitScreens;
        private System.Windows.Forms.Panel pnlScreens;
        private Kinovea.ScreenManager.CommonControls commonControls;

    }
}
