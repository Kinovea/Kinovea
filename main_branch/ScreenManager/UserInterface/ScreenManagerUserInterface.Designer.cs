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
            this.btnClose = new System.Windows.Forms.Button();
            this.pbLogo = new System.Windows.Forms.PictureBox();
            this.btnShowThumbView = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            
            this.ComCtrls = new Kinovea.ScreenManager.CommonControls();
            
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
            this.splitScreensPanel.Panel2.Controls.Add(this.ComCtrls);
            this.splitScreensPanel.Panel2.Controls.Add(this.btnClose);
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
            this.splitScreens.Panel1.DragOver += new System.Windows.Forms.DragEventHandler(this.splitScreens_Panel1_DragOver);
            this.splitScreens.Panel1.DragDrop += new System.Windows.Forms.DragEventHandler(this.splitScreens_Panel1_DragDrop);
            // 
            // splitScreens.Panel2
            // 
            this.splitScreens.Panel2.BackColor = System.Drawing.Color.White;
            this.splitScreens.Panel2.DragOver += new System.Windows.Forms.DragEventHandler(this.splitScreens_Panel2_DragOver);
            this.splitScreens.Panel2.DragDrop += new System.Windows.Forms.DragEventHandler(this.splitScreens_Panel2_DragDrop);
            this.splitScreens.Size = new System.Drawing.Size(574, 315);
            this.splitScreens.SplitterDistance = 287;
            this.splitScreens.TabIndex = 0;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.BackColor = System.Drawing.Color.Transparent;
            this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Image = global::Kinovea.ScreenManager.Properties.Resources.closegrey;
            this.btnClose.Location = new System.Drawing.Point(551, 2);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(20, 20);
            this.btnClose.TabIndex = 3;
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
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
            // 
            // btnShowThumbView
            // 
            this.btnShowThumbView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnShowThumbView.AutoSize = true;
            this.btnShowThumbView.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnShowThumbView.BackColor = System.Drawing.Color.Transparent;
            this.btnShowThumbView.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnShowThumbView.FlatAppearance.BorderSize = 0;
            this.btnShowThumbView.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
            this.btnShowThumbView.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnShowThumbView.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnShowThumbView.ForeColor = System.Drawing.Color.SteelBlue;
            this.btnShowThumbView.Location = new System.Drawing.Point(570, 4);
            this.btnShowThumbView.Name = "btnShowThumbView";
            this.btnShowThumbView.Size = new System.Drawing.Size(116, 24);
            this.btnShowThumbView.TabIndex = 3;
            this.btnShowThumbView.Text = "Show Thumbnails";
            this.btnShowThumbView.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnShowThumbView.UseVisualStyleBackColor = false;
            this.btnShowThumbView.Click += new System.EventHandler(this.btnShowThumbView_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.AutoSize = true;
            this.button1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button1.BackColor = System.Drawing.Color.Transparent;
            this.button1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.ForeColor = System.Drawing.Color.SteelBlue;
            this.button1.Location = new System.Drawing.Point(606, 34);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(83, 24);
            this.button1.TabIndex = 5;
            this.button1.Text = "Help Videos";
            this.button1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Visible = false;
            // 
            // ComCtrls
            // 
            this.ComCtrls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ComCtrls.BackColor = System.Drawing.Color.White;
            this.ComCtrls.Location = new System.Drawing.Point(0, 0);
            this.ComCtrls.Name = "ComCtrls";
            this.ComCtrls.Playing = false;
            this.ComCtrls.Size = new System.Drawing.Size(545, 45);
            this.ComCtrls.TabIndex = 4;
            // 
            // ScreenManagerUserInterface
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.Controls.Add(this.pnlScreens);
            this.Controls.Add(this.pbLogo);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnShowThumbView);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "ScreenManagerUserInterface";
            this.Size = new System.Drawing.Size(720, 560);
            this.DragOver += new System.Windows.Forms.DragEventHandler(this.ScreenManagerUserInterface_DragOver);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.ScreenManagerUserInterface_DragDrop);
            this.DoubleClick += new System.EventHandler(this.ScreenManagerUserInterface_DoubleClick);
            this.pnlScreens.ResumeLayout(false);
            this.splitScreensPanel.Panel1.ResumeLayout(false);
            this.splitScreensPanel.Panel2.ResumeLayout(false);
            this.splitScreensPanel.ResumeLayout(false);
            this.splitScreens.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pbLogo;
        public System.Windows.Forms.SplitContainer splitScreensPanel;
        public System.Windows.Forms.SplitContainer splitScreens;
        public System.Windows.Forms.Panel pnlScreens;
        private System.Windows.Forms.Button btnClose;
        public Kinovea.ScreenManager.CommonControls ComCtrls;
        private System.Windows.Forms.Button btnShowThumbView;
        private System.Windows.Forms.Button button1;

    }
}
