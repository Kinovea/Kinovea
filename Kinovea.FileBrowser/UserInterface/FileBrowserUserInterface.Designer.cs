namespace Kinovea.FileBrowser
{
    partial class FileBrowserUserInterface
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileBrowserUserInterface));
      this.tabControl = new System.Windows.Forms.TabControl();
      this.tabPageClassic = new System.Windows.Forms.TabPage();
      this.splitExplorerFiles = new System.Windows.Forms.SplitContainer();
      this.tvExplorer = new Kinovea.FileBrowser.BufferedTreeView();
      this.lvExplorer = new System.Windows.Forms.ListView();
      this.imgListFiles = new System.Windows.Forms.ImageList(this.components);
      this.tabPageShortcuts = new System.Windows.Forms.TabPage();
      this.splitShortcutsFiles = new System.Windows.Forms.SplitContainer();
      this.tvShortcuts = new Kinovea.FileBrowser.BufferedTreeView();
      this.lvShortcuts = new System.Windows.Forms.ListView();
      this.tabPageCameras = new System.Windows.Forms.TabPage();
      this.btnCameraRefresh = new System.Windows.Forms.Button();
      this.olvCameras = new BrightIdeasSoftware.ObjectListView();
      this.lvCaptured = new System.Windows.Forms.ListView();
      this.lblCaptureHistory = new System.Windows.Forms.Label();
      this.btnManual = new System.Windows.Forms.Button();
      this.imgListTabs = new System.Windows.Forms.ImageList(this.components);
      this.ttTabs = new System.Windows.Forms.ToolTip(this.components);
      this.btnAddShortcut = new System.Windows.Forms.Button();
      this.tabControl.SuspendLayout();
      this.tabPageClassic.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitExplorerFiles)).BeginInit();
      this.splitExplorerFiles.Panel1.SuspendLayout();
      this.splitExplorerFiles.Panel2.SuspendLayout();
      this.splitExplorerFiles.SuspendLayout();
      this.tabPageShortcuts.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitShortcutsFiles)).BeginInit();
      this.splitShortcutsFiles.Panel1.SuspendLayout();
      this.splitShortcutsFiles.Panel2.SuspendLayout();
      this.splitShortcutsFiles.SuspendLayout();
      this.tabPageCameras.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.olvCameras)).BeginInit();
      this.SuspendLayout();
      // 
      // tabControl
      // 
      this.tabControl.Controls.Add(this.tabPageClassic);
      this.tabControl.Controls.Add(this.tabPageShortcuts);
      this.tabControl.Controls.Add(this.tabPageCameras);
      this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabControl.ImageList = this.imgListTabs;
      this.tabControl.Location = new System.Drawing.Point(0, 0);
      this.tabControl.Name = "tabControl";
      this.tabControl.SelectedIndex = 0;
      this.tabControl.Size = new System.Drawing.Size(319, 560);
      this.tabControl.TabIndex = 0;
      this.tabControl.SelectedIndexChanged += new System.EventHandler(this.TabControlSelected_IndexChanged);
      // 
      // tabPageClassic
      // 
      this.tabPageClassic.BackColor = System.Drawing.Color.White;
      this.tabPageClassic.Controls.Add(this.splitExplorerFiles);
      this.tabPageClassic.ImageKey = "tree.png";
      this.tabPageClassic.Location = new System.Drawing.Point(4, 23);
      this.tabPageClassic.Name = "tabPageClassic";
      this.tabPageClassic.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageClassic.Size = new System.Drawing.Size(311, 533);
      this.tabPageClassic.TabIndex = 0;
      // 
      // splitExplorerFiles
      // 
      this.splitExplorerFiles.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitExplorerFiles.Location = new System.Drawing.Point(3, 3);
      this.splitExplorerFiles.Name = "splitExplorerFiles";
      this.splitExplorerFiles.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitExplorerFiles.Panel1
      // 
      this.splitExplorerFiles.Panel1.Controls.Add(this.btnAddShortcut);
      this.splitExplorerFiles.Panel1.Controls.Add(this.tvExplorer);
      // 
      // splitExplorerFiles.Panel2
      // 
      this.splitExplorerFiles.Panel2.BackColor = System.Drawing.Color.White;
      this.splitExplorerFiles.Panel2.Controls.Add(this.lvExplorer);
      this.splitExplorerFiles.Size = new System.Drawing.Size(305, 527);
      this.splitExplorerFiles.SplitterDistance = 301;
      this.splitExplorerFiles.TabIndex = 0;
      // 
      // tvExplorer
      // 
      this.tvExplorer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tvExplorer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.tvExplorer.Location = new System.Drawing.Point(3, 28);
      this.tvExplorer.Margin = new System.Windows.Forms.Padding(5, 10, 5, 5);
      this.tvExplorer.Name = "tvExplorer";
      this.tvExplorer.Size = new System.Drawing.Size(299, 270);
      this.tvExplorer.TabIndex = 4;
      // 
      // lvExplorer
      // 
      this.lvExplorer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lvExplorer.BackColor = System.Drawing.Color.White;
      this.lvExplorer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.lvExplorer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lvExplorer.ForeColor = System.Drawing.Color.Black;
      this.lvExplorer.FullRowSelect = true;
      this.lvExplorer.GridLines = true;
      this.lvExplorer.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
      this.lvExplorer.HideSelection = false;
      this.lvExplorer.Location = new System.Drawing.Point(3, 3);
      this.lvExplorer.MultiSelect = false;
      this.lvExplorer.Name = "lvExplorer";
      this.lvExplorer.ShowGroups = false;
      this.lvExplorer.Size = new System.Drawing.Size(299, 216);
      this.lvExplorer.SmallImageList = this.imgListFiles;
      this.lvExplorer.TabIndex = 0;
      this.lvExplorer.UseCompatibleStateImageBehavior = false;
      this.lvExplorer.View = System.Windows.Forms.View.Details;
      this.lvExplorer.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
      this.lvExplorer.SizeChanged += new System.EventHandler(this.listView_SizeChanged);
      this.lvExplorer.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView_MouseDoubleClick);
      this.lvExplorer.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listView_MouseDown);
      // 
      // imgListFiles
      // 
      this.imgListFiles.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgListFiles.ImageStream")));
      this.imgListFiles.TransparentColor = System.Drawing.Color.Transparent;
      this.imgListFiles.Images.SetKeyName(0, "film_small.png");
      // 
      // tabPageShortcuts
      // 
      this.tabPageShortcuts.BackColor = System.Drawing.Color.White;
      this.tabPageShortcuts.Controls.Add(this.splitShortcutsFiles);
      this.tabPageShortcuts.ImageKey = "shortcuts";
      this.tabPageShortcuts.Location = new System.Drawing.Point(4, 23);
      this.tabPageShortcuts.Name = "tabPageShortcuts";
      this.tabPageShortcuts.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageShortcuts.Size = new System.Drawing.Size(311, 533);
      this.tabPageShortcuts.TabIndex = 1;
      // 
      // splitShortcutsFiles
      // 
      this.splitShortcutsFiles.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitShortcutsFiles.Location = new System.Drawing.Point(3, 3);
      this.splitShortcutsFiles.Name = "splitShortcutsFiles";
      this.splitShortcutsFiles.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitShortcutsFiles.Panel1
      // 
      this.splitShortcutsFiles.Panel1.Controls.Add(this.tvShortcuts);
      // 
      // splitShortcutsFiles.Panel2
      // 
      this.splitShortcutsFiles.Panel2.Controls.Add(this.lvShortcuts);
      this.splitShortcutsFiles.Size = new System.Drawing.Size(305, 527);
      this.splitShortcutsFiles.SplitterDistance = 307;
      this.splitShortcutsFiles.TabIndex = 6;
      // 
      // tvShortcuts
      // 
      this.tvShortcuts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tvShortcuts.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.tvShortcuts.Location = new System.Drawing.Point(3, 31);
      this.tvShortcuts.Margin = new System.Windows.Forms.Padding(5, 10, 5, 5);
      this.tvShortcuts.Name = "tvShortcuts";
      this.tvShortcuts.Size = new System.Drawing.Size(299, 273);
      this.tvShortcuts.TabIndex = 15;
      // 
      // lvShortcuts
      // 
      this.lvShortcuts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lvShortcuts.BackColor = System.Drawing.Color.White;
      this.lvShortcuts.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.lvShortcuts.FullRowSelect = true;
      this.lvShortcuts.GridLines = true;
      this.lvShortcuts.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
      this.lvShortcuts.HideSelection = false;
      this.lvShortcuts.Location = new System.Drawing.Point(3, 3);
      this.lvShortcuts.Name = "lvShortcuts";
      this.lvShortcuts.Size = new System.Drawing.Size(299, 210);
      this.lvShortcuts.SmallImageList = this.imgListFiles;
      this.lvShortcuts.TabIndex = 2;
      this.lvShortcuts.UseCompatibleStateImageBehavior = false;
      this.lvShortcuts.View = System.Windows.Forms.View.Details;
      this.lvShortcuts.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
      this.lvShortcuts.SizeChanged += new System.EventHandler(this.listView_SizeChanged);
      this.lvShortcuts.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listView_MouseDown);
      // 
      // tabPageCameras
      // 
      this.tabPageCameras.Controls.Add(this.btnCameraRefresh);
      this.tabPageCameras.Controls.Add(this.olvCameras);
      this.tabPageCameras.Controls.Add(this.lvCaptured);
      this.tabPageCameras.Controls.Add(this.lblCaptureHistory);
      this.tabPageCameras.Controls.Add(this.btnManual);
      this.tabPageCameras.ImageKey = "camera";
      this.tabPageCameras.Location = new System.Drawing.Point(4, 23);
      this.tabPageCameras.Name = "tabPageCameras";
      this.tabPageCameras.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageCameras.Size = new System.Drawing.Size(311, 533);
      this.tabPageCameras.TabIndex = 2;
      this.tabPageCameras.UseVisualStyleBackColor = true;
      // 
      // btnCameraRefresh
      // 
      this.btnCameraRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCameraRefresh.BackColor = System.Drawing.Color.Transparent;
      this.btnCameraRefresh.BackgroundImage = global::Kinovea.FileBrowser.Properties.Resources.arrow_refresh;
      this.btnCameraRefresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.btnCameraRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnCameraRefresh.FlatAppearance.BorderSize = 0;
      this.btnCameraRefresh.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.btnCameraRefresh.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.btnCameraRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnCameraRefresh.Location = new System.Drawing.Point(285, 8);
      this.btnCameraRefresh.Name = "btnCameraRefresh";
      this.btnCameraRefresh.Size = new System.Drawing.Size(20, 20);
      this.btnCameraRefresh.TabIndex = 12;
      this.btnCameraRefresh.UseVisualStyleBackColor = false;
      this.btnCameraRefresh.Click += new System.EventHandler(this.btnCameraRefresh_Click);
      // 
      // olvCameras
      // 
      this.olvCameras.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.olvCameras.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.olvCameras.CellEditUseWholeCell = false;
      this.olvCameras.Cursor = System.Windows.Forms.Cursors.Default;
      this.olvCameras.HideSelection = false;
      this.olvCameras.Location = new System.Drawing.Point(6, 31);
      this.olvCameras.Name = "olvCameras";
      this.olvCameras.Size = new System.Drawing.Size(299, 279);
      this.olvCameras.TabIndex = 11;
      this.olvCameras.UseCompatibleStateImageBehavior = false;
      this.olvCameras.View = System.Windows.Forms.View.Details;
      this.olvCameras.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.olvCameras_ItemDrag);
      this.olvCameras.DoubleClick += new System.EventHandler(this.olvCameras_DoubleClick);
      // 
      // lvCaptured
      // 
      this.lvCaptured.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lvCaptured.BackColor = System.Drawing.Color.White;
      this.lvCaptured.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.lvCaptured.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lvCaptured.ForeColor = System.Drawing.Color.Black;
      this.lvCaptured.FullRowSelect = true;
      this.lvCaptured.GridLines = true;
      this.lvCaptured.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
      this.lvCaptured.HideSelection = false;
      this.lvCaptured.Location = new System.Drawing.Point(4, 387);
      this.lvCaptured.MultiSelect = false;
      this.lvCaptured.Name = "lvCaptured";
      this.lvCaptured.ShowGroups = false;
      this.lvCaptured.Size = new System.Drawing.Size(302, 141);
      this.lvCaptured.SmallImageList = this.imgListFiles;
      this.lvCaptured.TabIndex = 10;
      this.lvCaptured.UseCompatibleStateImageBehavior = false;
      this.lvCaptured.View = System.Windows.Forms.View.Details;
      this.lvCaptured.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
      this.lvCaptured.SizeChanged += new System.EventHandler(this.listView_SizeChanged);
      this.lvCaptured.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView_MouseDoubleClick);
      this.lvCaptured.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listView_MouseDown);
      // 
      // lblCaptureHistory
      // 
      this.lblCaptureHistory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblCaptureHistory.BackColor = System.Drawing.Color.White;
      this.lblCaptureHistory.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblCaptureHistory.ForeColor = System.Drawing.Color.Gray;
      this.lblCaptureHistory.Location = new System.Drawing.Point(3, 364);
      this.lblCaptureHistory.Name = "lblCaptureHistory";
      this.lblCaptureHistory.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
      this.lblCaptureHistory.Size = new System.Drawing.Size(305, 20);
      this.lblCaptureHistory.TabIndex = 6;
      this.lblCaptureHistory.Text = "Capture history  ";
      this.lblCaptureHistory.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btnManual
      // 
      this.btnManual.Location = new System.Drawing.Point(6, 316);
      this.btnManual.Name = "btnManual";
      this.btnManual.Size = new System.Drawing.Size(132, 23);
      this.btnManual.TabIndex = 4;
      this.btnManual.Text = "Manual connection";
      this.btnManual.UseVisualStyleBackColor = true;
      this.btnManual.Click += new System.EventHandler(this.BtnManualClick);
      // 
      // imgListTabs
      // 
      this.imgListTabs.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgListTabs.ImageStream")));
      this.imgListTabs.TransparentColor = System.Drawing.Color.Transparent;
      this.imgListTabs.Images.SetKeyName(0, "tree.png");
      this.imgListTabs.Images.SetKeyName(1, "shortcuts");
      this.imgListTabs.Images.SetKeyName(2, "tab_camera.png");
      this.imgListTabs.Images.SetKeyName(3, "camera");
      // 
      // btnAddShortcut
      // 
      this.btnAddShortcut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnAddShortcut.BackColor = System.Drawing.Color.Transparent;
      this.btnAddShortcut.BackgroundImage = global::Kinovea.FileBrowser.Properties.Resources.folder_add;
      this.btnAddShortcut.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.btnAddShortcut.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnAddShortcut.FlatAppearance.BorderSize = 0;
      this.btnAddShortcut.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.btnAddShortcut.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.btnAddShortcut.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnAddShortcut.Location = new System.Drawing.Point(279, 5);
      this.btnAddShortcut.Name = "btnAddShortcut";
      this.btnAddShortcut.Size = new System.Drawing.Size(20, 20);
      this.btnAddShortcut.TabIndex = 10;
      this.btnAddShortcut.UseVisualStyleBackColor = false;
      this.btnAddShortcut.Click += new System.EventHandler(this.btnAddShortcut_Click);
      // 
      // FileBrowserUserInterface
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.Controls.Add(this.tabControl);
      this.Name = "FileBrowserUserInterface";
      this.Size = new System.Drawing.Size(319, 560);
      this.tabControl.ResumeLayout(false);
      this.tabPageClassic.ResumeLayout(false);
      this.splitExplorerFiles.Panel1.ResumeLayout(false);
      this.splitExplorerFiles.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitExplorerFiles)).EndInit();
      this.splitExplorerFiles.ResumeLayout(false);
      this.tabPageShortcuts.ResumeLayout(false);
      this.splitShortcutsFiles.Panel1.ResumeLayout(false);
      this.splitShortcutsFiles.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitShortcutsFiles)).EndInit();
      this.splitShortcutsFiles.ResumeLayout(false);
      this.tabPageCameras.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.olvCameras)).EndInit();
      this.ResumeLayout(false);

        }
        
        private System.Windows.Forms.Button btnManual;
        private System.Windows.Forms.TabPage tabPageCameras;
        private System.Windows.Forms.ImageList imgListFiles;
        private System.Windows.Forms.SplitContainer splitShortcutsFiles;
        private System.Windows.Forms.SplitContainer splitExplorerFiles;
        private System.Windows.Forms.ToolTip ttTabs;
        private System.Windows.Forms.ImageList imgListTabs;
        public System.Windows.Forms.TabPage tabPageShortcuts;
        public System.Windows.Forms.TabPage tabPageClassic;
        public System.Windows.Forms.TabControl tabControl;

        #endregion
        private System.Windows.Forms.ListView lvExplorer;
        private System.Windows.Forms.ListView lvShortcuts;
        public System.Windows.Forms.Label lblCaptureHistory;
        private System.Windows.Forms.ListView lvCaptured;
        private BrightIdeasSoftware.ObjectListView olvCameras;
        private System.Windows.Forms.Button btnCameraRefresh;
        private BufferedTreeView tvShortcuts;
        private BufferedTreeView tvExplorer;
        private System.Windows.Forms.Button btnAddShortcut;
    }
}
