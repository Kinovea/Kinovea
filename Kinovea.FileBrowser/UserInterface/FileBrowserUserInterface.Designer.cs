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
      this.lvCaptured = new System.Windows.Forms.ListView();
      this.lblCaptureHistory = new System.Windows.Forms.Label();
      this.btnManual = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.lvCameras = new System.Windows.Forms.ListView();
      this.imgListTabs = new System.Windows.Forms.ImageList(this.components);
      this.ttTabs = new System.Windows.Forms.ToolTip(this.components);
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
      this.splitExplorerFiles.Panel1.Controls.Add(this.etExplorer);
      this.splitExplorerFiles.Panel1.Controls.Add(this.lblFolders);
      // 
      // splitExplorerFiles.Panel2
      // 
      this.splitExplorerFiles.Panel2.BackColor = System.Drawing.Color.White;
      this.splitExplorerFiles.Panel2.Controls.Add(this.lvExplorer);
      this.splitExplorerFiles.Panel2.Controls.Add(this.lblVideoFiles);
      this.splitExplorerFiles.Size = new System.Drawing.Size(305, 527);
      this.splitExplorerFiles.SplitterDistance = 301;
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
      this.etExplorer.Size = new System.Drawing.Size(305, 270);
      this.etExplorer.TabIndex = 0;
      this.etExplorer.ExpTreeNodeSelected += new ExpTreeLib.ExpTree.ExpTreeNodeSelectedEventHandler(this.etExplorer_ExpTreeNodeSelected);
      this.etExplorer.MouseDown += new System.Windows.Forms.MouseEventHandler(this.etExplorer_MouseDown);
      this.etExplorer.MouseEnter += new System.EventHandler(this.etExplorer_MouseEnter);
      // 
      // lblFolders
      // 
      this.lblFolders.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblFolders.BackColor = System.Drawing.Color.White;
      this.lblFolders.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblFolders.ForeColor = System.Drawing.Color.Gray;
      this.lblFolders.Location = new System.Drawing.Point(0, 8);
      this.lblFolders.Margin = new System.Windows.Forms.Padding(10, 0, 3, 0);
      this.lblFolders.Name = "lblFolders";
      this.lblFolders.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
      this.lblFolders.Size = new System.Drawing.Size(305, 20);
      this.lblFolders.TabIndex = 2;
      this.lblFolders.Text = "Folders";
      this.lblFolders.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
      this.lvExplorer.Location = new System.Drawing.Point(0, 43);
      this.lvExplorer.MultiSelect = false;
      this.lvExplorer.Name = "lvExplorer";
      this.lvExplorer.ShowGroups = false;
      this.lvExplorer.Size = new System.Drawing.Size(305, 179);
      this.lvExplorer.SmallImageList = this.imgListFiles;
      this.lvExplorer.TabIndex = 0;
      this.lvExplorer.UseCompatibleStateImageBehavior = false;
      this.lvExplorer.View = System.Windows.Forms.View.Details;
      this.lvExplorer.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
      this.lvExplorer.SizeChanged += new System.EventHandler(this.listView_SizeChanged);
      this.lvExplorer.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvExplorer_MouseDoubleClick);
      this.lvExplorer.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listView_MouseDown);
      // 
      // imgListFiles
      // 
      this.imgListFiles.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgListFiles.ImageStream")));
      this.imgListFiles.TransparentColor = System.Drawing.Color.Transparent;
      this.imgListFiles.Images.SetKeyName(0, "film_small.png");
      // 
      // lblVideoFiles
      // 
      this.lblVideoFiles.BackColor = System.Drawing.Color.White;
      this.lblVideoFiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblVideoFiles.ForeColor = System.Drawing.Color.Gray;
      this.lblVideoFiles.Location = new System.Drawing.Point(-3, 20);
      this.lblVideoFiles.Name = "lblVideoFiles";
      this.lblVideoFiles.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
      this.lblVideoFiles.Size = new System.Drawing.Size(305, 20);
      this.lblVideoFiles.TabIndex = 1;
      this.lblVideoFiles.Text = "Video files   ";
      this.lblVideoFiles.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
      this.splitShortcutsFiles.SplitterDistance = 307;
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
      this.etShortcuts.Size = new System.Drawing.Size(305, 272);
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
      this.btnDeleteShortcut.Location = new System.Drawing.Point(276, 8);
      this.btnDeleteShortcut.Name = "btnDeleteShortcut";
      this.btnDeleteShortcut.Size = new System.Drawing.Size(20, 20);
      this.btnDeleteShortcut.TabIndex = 9;
      this.btnDeleteShortcut.UseVisualStyleBackColor = false;
      this.btnDeleteShortcut.Click += new System.EventHandler(this.btnDeleteShortcut_Click);
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
      this.btnAddShortcut.Location = new System.Drawing.Point(250, 8);
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
      this.lblFavFolders.BackColor = System.Drawing.Color.White;
      this.lblFavFolders.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblFavFolders.ForeColor = System.Drawing.Color.Gray;
      this.lblFavFolders.Location = new System.Drawing.Point(0, 8);
      this.lblFavFolders.Name = "lblFavFolders";
      this.lblFavFolders.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
      this.lblFavFolders.Size = new System.Drawing.Size(244, 20);
      this.lblFavFolders.TabIndex = 6;
      this.lblFavFolders.Text = "Folders";
      this.lblFavFolders.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblFavFiles
      // 
      this.lblFavFiles.BackColor = System.Drawing.Color.White;
      this.lblFavFiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblFavFiles.ForeColor = System.Drawing.Color.Gray;
      this.lblFavFiles.Location = new System.Drawing.Point(-3, 16);
      this.lblFavFiles.Name = "lblFavFiles";
      this.lblFavFiles.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
      this.lblFavFiles.Size = new System.Drawing.Size(305, 20);
      this.lblFavFiles.TabIndex = 3;
      this.lblFavFiles.Text = "Video files  ";
      this.lblFavFiles.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
      this.lvShortcuts.Location = new System.Drawing.Point(0, 39);
      this.lvShortcuts.Name = "lvShortcuts";
      this.lvShortcuts.Size = new System.Drawing.Size(302, 174);
      this.lvShortcuts.SmallImageList = this.imgListFiles;
      this.lvShortcuts.TabIndex = 2;
      this.lvShortcuts.UseCompatibleStateImageBehavior = false;
      this.lvShortcuts.View = System.Windows.Forms.View.Details;
      this.lvShortcuts.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
      this.lvShortcuts.SizeChanged += new System.EventHandler(this.listView_SizeChanged);
      this.lvShortcuts.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvShortcuts_MouseDoubleClick);
      this.lvShortcuts.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listView_MouseDown);
      // 
      // tabPageCameras
      // 
      this.tabPageCameras.Controls.Add(this.lvCaptured);
      this.tabPageCameras.Controls.Add(this.lblCaptureHistory);
      this.tabPageCameras.Controls.Add(this.btnManual);
      this.tabPageCameras.Controls.Add(this.label1);
      this.tabPageCameras.Controls.Add(this.lvCameras);
      this.tabPageCameras.ImageKey = "camera";
      this.tabPageCameras.Location = new System.Drawing.Point(4, 23);
      this.tabPageCameras.Name = "tabPageCameras";
      this.tabPageCameras.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageCameras.Size = new System.Drawing.Size(311, 533);
      this.tabPageCameras.TabIndex = 2;
      this.tabPageCameras.UseVisualStyleBackColor = true;
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
      this.lvCaptured.Location = new System.Drawing.Point(4, 311);
      this.lvCaptured.MultiSelect = false;
      this.lvCaptured.Name = "lvCaptured";
      this.lvCaptured.ShowGroups = false;
      this.lvCaptured.Size = new System.Drawing.Size(302, 217);
      this.lvCaptured.SmallImageList = this.imgListFiles;
      this.lvCaptured.TabIndex = 10;
      this.lvCaptured.UseCompatibleStateImageBehavior = false;
      this.lvCaptured.View = System.Windows.Forms.View.Details;
      this.lvCaptured.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
      this.lvCaptured.SizeChanged += new System.EventHandler(this.listView_SizeChanged);
      this.lvCaptured.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.LvCaptured_MouseDoubleClick);
      this.lvCaptured.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listView_MouseDown);
      // 
      // lblCaptureHistory
      // 
      this.lblCaptureHistory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblCaptureHistory.BackColor = System.Drawing.Color.White;
      this.lblCaptureHistory.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblCaptureHistory.ForeColor = System.Drawing.Color.Gray;
      this.lblCaptureHistory.Location = new System.Drawing.Point(3, 284);
      this.lblCaptureHistory.Name = "lblCaptureHistory";
      this.lblCaptureHistory.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
      this.lblCaptureHistory.Size = new System.Drawing.Size(305, 20);
      this.lblCaptureHistory.TabIndex = 6;
      this.lblCaptureHistory.Text = "Capture history  ";
      this.lblCaptureHistory.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btnManual
      // 
      this.btnManual.Location = new System.Drawing.Point(1, 235);
      this.btnManual.Name = "btnManual";
      this.btnManual.Size = new System.Drawing.Size(132, 23);
      this.btnManual.TabIndex = 4;
      this.btnManual.Text = "Manual connection";
      this.btnManual.UseVisualStyleBackColor = true;
      this.btnManual.Click += new System.EventHandler(this.BtnManualClick);
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.BackColor = System.Drawing.Color.White;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.ForeColor = System.Drawing.Color.Gray;
      this.label1.Location = new System.Drawing.Point(0, 8);
      this.label1.Name = "label1";
      this.label1.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
      this.label1.Size = new System.Drawing.Size(305, 20);
      this.label1.TabIndex = 3;
      this.label1.Text = "Cameras";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lvCameras
      // 
      this.lvCameras.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lvCameras.BackColor = System.Drawing.Color.White;
      this.lvCameras.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.lvCameras.GridLines = true;
      this.lvCameras.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
      this.lvCameras.HideSelection = false;
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
      this.imgListTabs.Images.SetKeyName(0, "tree.png");
      this.imgListTabs.Images.SetKeyName(1, "shortcuts");
      this.imgListTabs.Images.SetKeyName(2, "tab_camera.png");
      this.imgListTabs.Images.SetKeyName(3, "camera");
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
      this.ResumeLayout(false);

        }

        private System.Windows.Forms.ListView lvCameras;
        
        private System.Windows.Forms.Button btnManual;
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
        private ExpTreeLib.ExpTree etExplorer;
        public System.Windows.Forms.TabPage tabPageShortcuts;
        public System.Windows.Forms.Label lblFavFiles;
        public System.Windows.Forms.Label lblFavFolders;
        public System.Windows.Forms.Label lblVideoFiles;
        public System.Windows.Forms.TabPage tabPageClassic;
        public System.Windows.Forms.TabControl tabControl;

        #endregion
        private System.Windows.Forms.ListView lvExplorer;
        private System.Windows.Forms.ListView lvShortcuts;
        public System.Windows.Forms.Label lblCaptureHistory;
        public System.Windows.Forms.Label lblFolders;
        private System.Windows.Forms.ListView lvCaptured;
    }
}
