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
            this.etExplorer = new ExpTreeLib.ExpTree();
            this.lblFolders = new System.Windows.Forms.Label();
            this.lvExplorer = new System.Windows.Forms.ListView();
            this.imgListFiles = new System.Windows.Forms.ImageList(this.components);
            this.lblVideoFiles = new System.Windows.Forms.Label();
            this.tabPageShortcuts = new System.Windows.Forms.TabPage();
            this.splitShortcutsFiles = new System.Windows.Forms.SplitContainer();
            this.etShortcuts = new ExpTreeLib.ExpTree();
            this.btnDeleteShortcut = new System.Windows.Forms.Button();
            this.btnAddShortcut = new System.Windows.Forms.Button();
            this.lblFavFolders = new System.Windows.Forms.Label();
            this.lblFavFiles = new System.Windows.Forms.Label();
            this.lvShortcuts = new System.Windows.Forms.ListView();
            this.tabPageCameras = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.lvCameras = new System.Windows.Forms.ListView();
            this.imgListTabs = new System.Windows.Forms.ImageList(this.components);
            this.ttTabs = new System.Windows.Forms.ToolTip(this.components);
            this.tabControl.SuspendLayout();
            this.tabPageClassic.SuspendLayout();
            this.splitExplorerFiles.Panel1.SuspendLayout();
            this.splitExplorerFiles.Panel2.SuspendLayout();
            this.splitExplorerFiles.SuspendLayout();
            this.tabPageShortcuts.SuspendLayout();
            this.splitShortcutsFiles.Panel1.SuspendLayout();
            this.splitShortcutsFiles.Panel2.SuspendLayout();
            this.splitShortcutsFiles.SuspendLayout();
            this.tabPageCameras.SuspendLayout();
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
            this.tabControl.KeyDown += new System.Windows.Forms.KeyEventHandler(this._tabControl_KeyDown);
            // 
            // tabPageClassic
            // 
            this.tabPageClassic.BackColor = System.Drawing.Color.White;
            this.tabPageClassic.Controls.Add(this.splitExplorerFiles);
            this.tabPageClassic.ImageKey = "tab_video.png";
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
            this.splitExplorerFiles.Panel1.Controls.Add(this.etExplorer);
            this.splitExplorerFiles.Panel1.Controls.Add(this.lblFolders);
            // 
            // splitExplorerFiles.Panel2
            // 
            this.splitExplorerFiles.Panel2.Controls.Add(this.lvExplorer);
            this.splitExplorerFiles.Panel2.Controls.Add(this.lblVideoFiles);
            this.splitExplorerFiles.Size = new System.Drawing.Size(305, 527);
            this.splitExplorerFiles.SplitterDistance = 302;
            this.splitExplorerFiles.TabIndex = 0;
            // 
            // etExplorer
            // 
            this.etExplorer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                                    | System.Windows.Forms.AnchorStyles.Left) 
                                    | System.Windows.Forms.AnchorStyles.Right)));
            this.etExplorer.BackColor = System.Drawing.Color.White;
            this.etExplorer.Cursor = System.Windows.Forms.Cursors.Default;
            this.etExplorer.Location = new System.Drawing.Point(0, 31);
            this.etExplorer.Name = "etExplorer";
            this.etExplorer.RootDisplayName = "Bureau";
            this.etExplorer.ShowRootLines = false;
            this.etExplorer.Size = new System.Drawing.Size(305, 271);
            this.etExplorer.StartUpDirectory = ExpTreeLib.ExpTree.StartDir.Desktop;
            this.etExplorer.TabIndex = 0;
            this.etExplorer.ExpTreeNodeSelected += new ExpTreeLib.ExpTree.ExpTreeNodeSelectedEventHandler(this.etExplorer_ExpTreeNodeSelected);
            this.etExplorer.MouseDown += new System.Windows.Forms.MouseEventHandler(this.etExplorer_MouseDown);
            this.etExplorer.MouseEnter += new System.EventHandler(this.etExplorer_MouseEnter);
            // 
            // lblFolders
            // 
            this.lblFolders.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                                    | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFolders.BackColor = System.Drawing.Color.WhiteSmoke;
            this.lblFolders.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFolders.ForeColor = System.Drawing.Color.SteelBlue;
            this.lblFolders.Location = new System.Drawing.Point(0, 8);
            this.lblFolders.Name = "lblFolders";
            this.lblFolders.Size = new System.Drawing.Size(305, 20);
            this.lblFolders.TabIndex = 2;
            this.lblFolders.Text = "Dossiers :   ";
            this.lblFolders.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lvExplorer
            // 
            this.lvExplorer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                                    | System.Windows.Forms.AnchorStyles.Left) 
                                    | System.Windows.Forms.AnchorStyles.Right)));
            this.lvExplorer.BackColor = System.Drawing.Color.White;
            this.lvExplorer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lvExplorer.GridLines = true;
            this.lvExplorer.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvExplorer.Location = new System.Drawing.Point(0, 23);
            this.lvExplorer.MultiSelect = false;
            this.lvExplorer.Name = "lvExplorer";
            this.lvExplorer.ShowGroups = false;
            this.lvExplorer.Size = new System.Drawing.Size(305, 198);
            this.lvExplorer.SmallImageList = this.imgListFiles;
            this.lvExplorer.TabIndex = 0;
            this.lvExplorer.UseCompatibleStateImageBehavior = false;
            this.lvExplorer.View = System.Windows.Forms.View.List;
            this.lvExplorer.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvExplorer_MouseDoubleClick);
            this.lvExplorer.MouseEnter += new System.EventHandler(this.lvExplorer_MouseEnter);
            // 
            // imgListFiles
            // 
            this.imgListFiles.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgListFiles.ImageStream")));
            this.imgListFiles.TransparentColor = System.Drawing.Color.Transparent;
            this.imgListFiles.Images.SetKeyName(0, "film.png");
            this.imgListFiles.Images.SetKeyName(1, "media-playback-start.png");
            this.imgListFiles.Images.SetKeyName(2, "video-x-generic.png");
            this.imgListFiles.Images.SetKeyName(3, "film.jpg");
            this.imgListFiles.Images.SetKeyName(4, "bullet_yellow.jpg");
            this.imgListFiles.Images.SetKeyName(5, "bullet_blue.jpg");
            this.imgListFiles.Images.SetKeyName(6, "bullet_blue2.jpg");
            // 
            // lblVideoFiles
            // 
            this.lblVideoFiles.BackColor = System.Drawing.Color.WhiteSmoke;
            this.lblVideoFiles.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblVideoFiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVideoFiles.ForeColor = System.Drawing.Color.SteelBlue;
            this.lblVideoFiles.Location = new System.Drawing.Point(0, 0);
            this.lblVideoFiles.Name = "lblVideoFiles";
            this.lblVideoFiles.Size = new System.Drawing.Size(305, 20);
            this.lblVideoFiles.TabIndex = 1;
            this.lblVideoFiles.Text = "Fichiers Vid�o :   ";
            this.lblVideoFiles.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabPageShortcuts
            // 
            this.tabPageShortcuts.BackColor = System.Drawing.Color.White;
            this.tabPageShortcuts.Controls.Add(this.splitShortcutsFiles);
            this.tabPageShortcuts.ImageKey = "tab_shortcuts.png";
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
            this.splitShortcutsFiles.Panel1.Controls.Add(this.etShortcuts);
            this.splitShortcutsFiles.Panel1.Controls.Add(this.btnDeleteShortcut);
            this.splitShortcutsFiles.Panel1.Controls.Add(this.btnAddShortcut);
            this.splitShortcutsFiles.Panel1.Controls.Add(this.lblFavFolders);
            // 
            // splitShortcutsFiles.Panel2
            // 
            this.splitShortcutsFiles.Panel2.Controls.Add(this.lblFavFiles);
            this.splitShortcutsFiles.Panel2.Controls.Add(this.lvShortcuts);
            this.splitShortcutsFiles.Size = new System.Drawing.Size(305, 527);
            this.splitShortcutsFiles.SplitterDistance = 308;
            this.splitShortcutsFiles.TabIndex = 6;
            // 
            // etShortcuts
            // 
            this.etShortcuts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                                    | System.Windows.Forms.AnchorStyles.Left) 
                                    | System.Windows.Forms.AnchorStyles.Right)));
            this.etShortcuts.Cursor = System.Windows.Forms.Cursors.Default;
            this.etShortcuts.Location = new System.Drawing.Point(0, 34);
            this.etShortcuts.Name = "etShortcuts";
            this.etShortcuts.RootDisplayName = "Root";
            this.etShortcuts.ShortcutsMode = true;
            this.etShortcuts.ShowRootLines = false;
            this.etShortcuts.Size = new System.Drawing.Size(305, 273);
            this.etShortcuts.StartUpDirectory = ExpTreeLib.ExpTree.StartDir.Favorites;
            this.etShortcuts.TabIndex = 13;
            this.etShortcuts.ExpTreeNodeSelected += new ExpTreeLib.ExpTree.ExpTreeNodeSelectedEventHandler(this.etShortcuts_ExpTreeNodeSelected);
            this.etShortcuts.MouseDown += new System.Windows.Forms.MouseEventHandler(this.etShortcuts_MouseDown);
            this.etShortcuts.MouseEnter += new System.EventHandler(this.etShortcuts_MouseEnter);
            // 
            // btnDeleteShortcut
            // 
            this.btnDeleteShortcut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeleteShortcut.BackColor = System.Drawing.Color.Transparent;
            this.btnDeleteShortcut.BackgroundImage = global::Kinovea.FileBrowser.Properties.Resources.folder_delete;
            this.btnDeleteShortcut.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnDeleteShortcut.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDeleteShortcut.FlatAppearance.BorderSize = 0;
            this.btnDeleteShortcut.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnDeleteShortcut.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnDeleteShortcut.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteShortcut.Location = new System.Drawing.Point(282, 0);
            this.btnDeleteShortcut.Name = "btnDeleteShortcut";
            this.btnDeleteShortcut.Size = new System.Drawing.Size(20, 20);
            this.btnDeleteShortcut.TabIndex = 9;
            this.btnDeleteShortcut.UseVisualStyleBackColor = false;
            this.btnDeleteShortcut.Click += new System.EventHandler(this.btnDeleteShortcut_Click);
            // 
            // btnAddShortcut
            // 
            this.btnAddShortcut.BackColor = System.Drawing.Color.Transparent;
            this.btnAddShortcut.BackgroundImage = global::Kinovea.FileBrowser.Properties.Resources.folder_add;
            this.btnAddShortcut.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnAddShortcut.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddShortcut.FlatAppearance.BorderSize = 0;
            this.btnAddShortcut.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnAddShortcut.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnAddShortcut.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddShortcut.Location = new System.Drawing.Point(3, 0);
            this.btnAddShortcut.Name = "btnAddShortcut";
            this.btnAddShortcut.Size = new System.Drawing.Size(20, 20);
            this.btnAddShortcut.TabIndex = 8;
            this.btnAddShortcut.UseVisualStyleBackColor = false;
            this.btnAddShortcut.Click += new System.EventHandler(this.btnAddShortcut_Click);
            // 
            // lblFavFolders
            // 
            this.lblFavFolders.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                                    | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFavFolders.BackColor = System.Drawing.Color.WhiteSmoke;
            this.lblFavFolders.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFavFolders.ForeColor = System.Drawing.Color.SteelBlue;
            this.lblFavFolders.Location = new System.Drawing.Point(0, 8);
            this.lblFavFolders.Name = "lblFavFolders";
            this.lblFavFolders.Size = new System.Drawing.Size(305, 20);
            this.lblFavFolders.TabIndex = 6;
            this.lblFavFolders.Text = "Dossiers :   ";
            this.lblFavFolders.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFavFiles
            // 
            this.lblFavFiles.BackColor = System.Drawing.Color.WhiteSmoke;
            this.lblFavFiles.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblFavFiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFavFiles.ForeColor = System.Drawing.Color.SteelBlue;
            this.lblFavFiles.Location = new System.Drawing.Point(0, 0);
            this.lblFavFiles.Name = "lblFavFiles";
            this.lblFavFiles.Size = new System.Drawing.Size(305, 20);
            this.lblFavFiles.TabIndex = 3;
            this.lblFavFiles.Text = "Fichiers Vid�o :   ";
            this.lblFavFiles.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lvShortcuts
            // 
            this.lvShortcuts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                                    | System.Windows.Forms.AnchorStyles.Left) 
                                    | System.Windows.Forms.AnchorStyles.Right)));
            this.lvShortcuts.BackColor = System.Drawing.Color.White;
            this.lvShortcuts.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lvShortcuts.Location = new System.Drawing.Point(0, 23);
            this.lvShortcuts.Name = "lvShortcuts";
            this.lvShortcuts.Size = new System.Drawing.Size(302, 189);
            this.lvShortcuts.SmallImageList = this.imgListFiles;
            this.lvShortcuts.TabIndex = 2;
            this.lvShortcuts.UseCompatibleStateImageBehavior = false;
            this.lvShortcuts.View = System.Windows.Forms.View.List;
            this.lvShortcuts.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvShortcuts_MouseDoubleClick);
            this.lvShortcuts.MouseEnter += new System.EventHandler(this.lvShortcuts_MouseEnter);
            // 
            // tabPageCameras
            // 
            this.tabPageCameras.Controls.Add(this.label1);
            this.tabPageCameras.Controls.Add(this.lvCameras);
            this.tabPageCameras.ImageKey = "tab_camera.png";
            this.tabPageCameras.Location = new System.Drawing.Point(4, 23);
            this.tabPageCameras.Name = "tabPageCameras";
            this.tabPageCameras.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageCameras.Size = new System.Drawing.Size(311, 533);
            this.tabPageCameras.TabIndex = 2;
            this.tabPageCameras.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                                    | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.SteelBlue;
            this.label1.Location = new System.Drawing.Point(0, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(305, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "Cameras :   ";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lvCameras
            // 
            this.lvCameras.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                                    | System.Windows.Forms.AnchorStyles.Left) 
                                    | System.Windows.Forms.AnchorStyles.Right)));
            this.lvCameras.BackColor = System.Drawing.Color.White;
            this.lvCameras.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lvCameras.GridLines = true;
            this.lvCameras.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvCameras.Location = new System.Drawing.Point(2, 31);
            this.lvCameras.MultiSelect = false;
            this.lvCameras.Name = "lvCameras";
            this.lvCameras.ShowGroups = false;
            this.lvCameras.Size = new System.Drawing.Size(305, 198);
            this.lvCameras.SmallImageList = this.imgListFiles;
            this.lvCameras.TabIndex = 1;
            this.lvCameras.UseCompatibleStateImageBehavior = false;
            this.lvCameras.View = System.Windows.Forms.View.List;
            this.lvCameras.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.LvCameras_MouseDoubleClick);
            // 
            // imgListTabs
            // 
            this.imgListTabs.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgListTabs.ImageStream")));
            this.imgListTabs.TransparentColor = System.Drawing.Color.Transparent;
            this.imgListTabs.Images.SetKeyName(0, "tab_video.png");
            this.imgListTabs.Images.SetKeyName(1, "tab_shortcuts.png");
            this.imgListTabs.Images.SetKeyName(2, "tab_camera.png");
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
            this.splitExplorerFiles.ResumeLayout(false);
            this.tabPageShortcuts.ResumeLayout(false);
            this.splitShortcutsFiles.Panel1.ResumeLayout(false);
            this.splitShortcutsFiles.Panel2.ResumeLayout(false);
            this.splitShortcutsFiles.ResumeLayout(false);
            this.tabPageCameras.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        private System.Windows.Forms.ListView lvCameras;
        public System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPageCameras;
        private System.Windows.Forms.ImageList imgListFiles;
        private System.Windows.Forms.SplitContainer splitShortcutsFiles;
        private System.Windows.Forms.SplitContainer splitExplorerFiles;
        private ExpTreeLib.ExpTree etShortcuts;
        private System.Windows.Forms.ToolTip ttTabs;
        private System.Windows.Forms.ImageList imgListTabs;
        private System.Windows.Forms.Button btnDeleteShortcut;
        private System.Windows.Forms.Button btnAddShortcut;
        public System.Windows.Forms.TabPage tabPageShortcuts;
        public System.Windows.Forms.Label lblFavFiles;
        public System.Windows.Forms.Label lblFavFolders;
        private System.Windows.Forms.ListView lvShortcuts;

        #endregion

        private ExpTreeLib.ExpTree etExplorer;
        private System.Windows.Forms.ListView lvExplorer;
        public System.Windows.Forms.Label lblVideoFiles;
        public System.Windows.Forms.Label lblFolders;
        public System.Windows.Forms.TabPage tabPageClassic;
        public System.Windows.Forms.TabControl tabControl;
    }
}
