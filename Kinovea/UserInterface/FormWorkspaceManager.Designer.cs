namespace Kinovea.Root
{
    partial class FormWorkspaceManager
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormWorkspaceManager));
      this.grpWorkspaces = new System.Windows.Forms.GroupBox();
      this.btnCreateShortcut2 = new System.Windows.Forms.Button();
      this.btnRename = new System.Windows.Forms.Button();
      this.olvWorkspaces = new BrightIdeasSoftware.ObjectListView();
      this.btnDelete = new System.Windows.Forms.Button();
      this.btnAdd = new System.Windows.Forms.Button();
      this.imgListStatus = new System.Windows.Forms.ImageList(this.components);
      this.btnClose = new System.Windows.Forms.Button();
      this.grpWindowList = new System.Windows.Forms.GroupBox();
      this.olvWindows = new BrightIdeasSoftware.ObjectListView();
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this.btnCreateShortcut = new System.Windows.Forms.Button();
      this.grpWorkspaces.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.olvWorkspaces)).BeginInit();
      this.grpWindowList.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.olvWindows)).BeginInit();
      this.SuspendLayout();
      // 
      // grpWorkspaces
      // 
      this.grpWorkspaces.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpWorkspaces.Controls.Add(this.btnCreateShortcut2);
      this.grpWorkspaces.Controls.Add(this.btnRename);
      this.grpWorkspaces.Controls.Add(this.olvWorkspaces);
      this.grpWorkspaces.Controls.Add(this.btnDelete);
      this.grpWorkspaces.Controls.Add(this.btnAdd);
      this.grpWorkspaces.Location = new System.Drawing.Point(12, 12);
      this.grpWorkspaces.Name = "grpWorkspaces";
      this.grpWorkspaces.Padding = new System.Windows.Forms.Padding(3, 3, 20, 3);
      this.grpWorkspaces.Size = new System.Drawing.Size(379, 160);
      this.grpWorkspaces.TabIndex = 62;
      this.grpWorkspaces.TabStop = false;
      this.grpWorkspaces.Text = "Workspaces";
      // 
      // btnCreateShortcut2
      // 
      this.btnCreateShortcut2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCreateShortcut2.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnCreateShortcut2.FlatAppearance.BorderSize = 0;
      this.btnCreateShortcut2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnCreateShortcut2.Image = global::Kinovea.Root.Properties.Resources.shortcut2;
      this.btnCreateShortcut2.Location = new System.Drawing.Point(338, 81);
      this.btnCreateShortcut2.Name = "btnCreateShortcut2";
      this.btnCreateShortcut2.Size = new System.Drawing.Size(25, 25);
      this.btnCreateShortcut2.TabIndex = 77;
      this.btnCreateShortcut2.UseVisualStyleBackColor = true;
      this.btnCreateShortcut2.Click += new System.EventHandler(this.btnShortcut_Click);
      // 
      // btnRename
      // 
      this.btnRename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnRename.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnRename.FlatAppearance.BorderSize = 0;
      this.btnRename.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnRename.Image = global::Kinovea.Root.Properties.Resources.rename_16;
      this.btnRename.Location = new System.Drawing.Point(338, 50);
      this.btnRename.Name = "btnRename";
      this.btnRename.Size = new System.Drawing.Size(25, 25);
      this.btnRename.TabIndex = 76;
      this.btnRename.UseVisualStyleBackColor = true;
      this.btnRename.Click += new System.EventHandler(this.btnRename_Click);
      // 
      // olvWorkspaces
      // 
      this.olvWorkspaces.AlternateRowBackColor = System.Drawing.Color.Gainsboro;
      this.olvWorkspaces.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.olvWorkspaces.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.SingleClickAlways;
      this.olvWorkspaces.CellEditUseWholeCell = false;
      this.olvWorkspaces.Cursor = System.Windows.Forms.Cursors.Default;
      this.olvWorkspaces.FullRowSelect = true;
      this.olvWorkspaces.GridLines = true;
      this.olvWorkspaces.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.olvWorkspaces.HideSelection = false;
      this.olvWorkspaces.Location = new System.Drawing.Point(13, 19);
      this.olvWorkspaces.Name = "olvWorkspaces";
      this.olvWorkspaces.Size = new System.Drawing.Size(319, 121);
      this.olvWorkspaces.TabIndex = 75;
      this.olvWorkspaces.UseCompatibleStateImageBehavior = false;
      this.olvWorkspaces.View = System.Windows.Forms.View.Details;
      this.olvWorkspaces.SelectedIndexChanged += new System.EventHandler(this.olvWorkspaces_SelectedIndexChanged);
      this.olvWorkspaces.DoubleClick += new System.EventHandler(this.olvWorkspaces_DoubleClick);
      // 
      // btnDelete
      // 
      this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnDelete.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnDelete.FlatAppearance.BorderSize = 0;
      this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnDelete.Image = global::Kinovea.Root.Properties.Resources.bin_empty;
      this.btnDelete.Location = new System.Drawing.Point(338, 115);
      this.btnDelete.Name = "btnDelete";
      this.btnDelete.Size = new System.Drawing.Size(25, 25);
      this.btnDelete.TabIndex = 74;
      this.btnDelete.UseVisualStyleBackColor = true;
      this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
      // 
      // btnAdd
      // 
      this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnAdd.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnAdd.FlatAppearance.BorderSize = 0;
      this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnAdd.Image = global::Kinovea.Root.Properties.Resources.add_16;
      this.btnAdd.Location = new System.Drawing.Point(338, 19);
      this.btnAdd.Name = "btnAdd";
      this.btnAdd.Size = new System.Drawing.Size(25, 25);
      this.btnAdd.TabIndex = 73;
      this.btnAdd.UseVisualStyleBackColor = true;
      this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
      // 
      // imgListStatus
      // 
      this.imgListStatus.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgListStatus.ImageStream")));
      this.imgListStatus.TransparentColor = System.Drawing.Color.Transparent;
      this.imgListStatus.Images.SetKeyName(0, "capture");
      this.imgListStatus.Images.SetKeyName(1, "dualcapture");
      this.imgListStatus.Images.SetKeyName(2, "dualmixed");
      this.imgListStatus.Images.SetKeyName(3, "dualplayback");
      this.imgListStatus.Images.SetKeyName(4, "explorer");
      this.imgListStatus.Images.SetKeyName(5, "playback");
      // 
      // btnClose
      // 
      this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnClose.Location = new System.Drawing.Point(292, 350);
      this.btnClose.Name = "btnClose";
      this.btnClose.Size = new System.Drawing.Size(99, 24);
      this.btnClose.TabIndex = 64;
      this.btnClose.Text = "Close";
      this.btnClose.UseVisualStyleBackColor = true;
      // 
      // grpWindowList
      // 
      this.grpWindowList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpWindowList.Controls.Add(this.olvWindows);
      this.grpWindowList.Location = new System.Drawing.Point(12, 178);
      this.grpWindowList.Name = "grpWindowList";
      this.grpWindowList.Padding = new System.Windows.Forms.Padding(3, 3, 20, 3);
      this.grpWindowList.Size = new System.Drawing.Size(379, 166);
      this.grpWindowList.TabIndex = 65;
      this.grpWindowList.TabStop = false;
      this.grpWindowList.Text = "Windows";
      // 
      // olvWindows
      // 
      this.olvWindows.AlternateRowBackColor = System.Drawing.Color.Gainsboro;
      this.olvWindows.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.olvWindows.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.SingleClickAlways;
      this.olvWindows.CellEditUseWholeCell = false;
      this.olvWindows.Cursor = System.Windows.Forms.Cursors.Default;
      this.olvWindows.FullRowSelect = true;
      this.olvWindows.GridLines = true;
      this.olvWindows.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.olvWindows.HideSelection = false;
      this.olvWindows.Location = new System.Drawing.Point(12, 23);
      this.olvWindows.Name = "olvWindows";
      this.olvWindows.Size = new System.Drawing.Size(320, 128);
      this.olvWindows.SmallImageList = this.imgListStatus;
      this.olvWindows.TabIndex = 76;
      this.olvWindows.UseCompatibleStateImageBehavior = false;
      this.olvWindows.View = System.Windows.Forms.View.Details;
      // 
      // btnCreateShortcut
      // 
      this.btnCreateShortcut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCreateShortcut.Image = global::Kinovea.Root.Properties.Resources.shortcut2;
      this.btnCreateShortcut.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.btnCreateShortcut.Location = new System.Drawing.Point(12, 350);
      this.btnCreateShortcut.Name = "btnCreateShortcut";
      this.btnCreateShortcut.Size = new System.Drawing.Size(188, 24);
      this.btnCreateShortcut.TabIndex = 66;
      this.btnCreateShortcut.Text = "      Create desktop shortcut";
      this.btnCreateShortcut.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.btnCreateShortcut.UseVisualStyleBackColor = true;
      this.btnCreateShortcut.Click += new System.EventHandler(this.btnShortcut_Click);
      // 
      // FormWorkspaceManager
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(403, 386);
      this.Controls.Add(this.btnCreateShortcut);
      this.Controls.Add(this.grpWindowList);
      this.Controls.Add(this.btnClose);
      this.Controls.Add(this.grpWorkspaces);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormWorkspaceManager";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.Text = "Manage workspaces";
      this.grpWorkspaces.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.olvWorkspaces)).EndInit();
      this.grpWindowList.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.olvWindows)).EndInit();
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpWorkspaces;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.GroupBox grpWindowList;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.ImageList imgListStatus;
        private BrightIdeasSoftware.ObjectListView olvWorkspaces;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnCreateShortcut;
        private System.Windows.Forms.Button btnRename;
        private BrightIdeasSoftware.ObjectListView olvWindows;
        private System.Windows.Forms.Button btnCreateShortcut2;
    }
}