namespace Kinovea.Root
{
    partial class FormWindowManager
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormWindowManager));
      this.grpIdentifier = new System.Windows.Forms.GroupBox();
      this.olvWindows = new BrightIdeasSoftware.ObjectListView();
      this.imgListStatus = new System.Windows.Forms.ImageList(this.components);
      this.btnDelete = new System.Windows.Forms.Button();
      this.btnStartStop = new System.Windows.Forms.Button();
      this.btnDown = new System.Windows.Forms.Button();
      this.btnUp = new System.Windows.Forms.Button();
      this.btnOK = new System.Windows.Forms.Button();
      this.grpScreenList = new System.Windows.Forms.GroupBox();
      this.pnlScreenList = new System.Windows.Forms.Panel();
      this.lblScreen2 = new System.Windows.Forms.Label();
      this.btnScreen2 = new System.Windows.Forms.Button();
      this.lblScreen1 = new System.Windows.Forms.Label();
      this.btnScreen1 = new System.Windows.Forms.Button();
      this.imgListScreens = new System.Windows.Forms.ImageList(this.components);
      this.grpIdentifier.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.olvWindows)).BeginInit();
      this.grpScreenList.SuspendLayout();
      this.pnlScreenList.SuspendLayout();
      this.SuspendLayout();
      // 
      // grpIdentifier
      // 
      this.grpIdentifier.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpIdentifier.Controls.Add(this.olvWindows);
      this.grpIdentifier.Controls.Add(this.btnDelete);
      this.grpIdentifier.Controls.Add(this.btnStartStop);
      this.grpIdentifier.Controls.Add(this.btnDown);
      this.grpIdentifier.Controls.Add(this.btnUp);
      this.grpIdentifier.Location = new System.Drawing.Point(12, 12);
      this.grpIdentifier.Name = "grpIdentifier";
      this.grpIdentifier.Padding = new System.Windows.Forms.Padding(3, 3, 20, 3);
      this.grpIdentifier.Size = new System.Drawing.Size(379, 243);
      this.grpIdentifier.TabIndex = 62;
      this.grpIdentifier.TabStop = false;
      this.grpIdentifier.Text = "Saved windows";
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
      this.olvWindows.Location = new System.Drawing.Point(13, 19);
      this.olvWindows.Name = "olvWindows";
      this.olvWindows.Size = new System.Drawing.Size(319, 208);
      this.olvWindows.SmallImageList = this.imgListStatus;
      this.olvWindows.TabIndex = 75;
      this.olvWindows.UseCompatibleStateImageBehavior = false;
      this.olvWindows.View = System.Windows.Forms.View.Details;
      this.olvWindows.SelectedIndexChanged += new System.EventHandler(this.olvWindows_SelectedIndexChanged);
      // 
      // imgListStatus
      // 
      this.imgListStatus.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgListStatus.ImageStream")));
      this.imgListStatus.TransparentColor = System.Drawing.Color.Transparent;
      this.imgListStatus.Images.SetKeyName(0, "running");
      this.imgListStatus.Images.SetKeyName(1, "sleeping");
      this.imgListStatus.Images.SetKeyName(2, "myself");
      this.imgListStatus.Images.SetKeyName(3, "capture");
      this.imgListStatus.Images.SetKeyName(4, "dualcapture");
      this.imgListStatus.Images.SetKeyName(5, "dualmixed");
      this.imgListStatus.Images.SetKeyName(6, "dualplayback");
      this.imgListStatus.Images.SetKeyName(7, "explorer");
      this.imgListStatus.Images.SetKeyName(8, "playback");
      // 
      // btnDelete
      // 
      this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnDelete.FlatAppearance.BorderSize = 0;
      this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnDelete.Image = global::Kinovea.Root.Properties.Resources.bin_empty;
      this.btnDelete.Location = new System.Drawing.Point(338, 112);
      this.btnDelete.Name = "btnDelete";
      this.btnDelete.Size = new System.Drawing.Size(25, 25);
      this.btnDelete.TabIndex = 74;
      this.btnDelete.UseVisualStyleBackColor = true;
      // 
      // btnStartStop
      // 
      this.btnStartStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnStartStop.FlatAppearance.BorderSize = 0;
      this.btnStartStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnStartStop.Image = global::Kinovea.Root.Properties.Resources.stop2_16;
      this.btnStartStop.Location = new System.Drawing.Point(338, 81);
      this.btnStartStop.Name = "btnStartStop";
      this.btnStartStop.Size = new System.Drawing.Size(25, 25);
      this.btnStartStop.TabIndex = 73;
      this.btnStartStop.UseVisualStyleBackColor = true;
      // 
      // btnDown
      // 
      this.btnDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnDown.FlatAppearance.BorderSize = 0;
      this.btnDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnDown.Image = global::Kinovea.Root.Properties.Resources.thick_arrow_pointing_down_16;
      this.btnDown.Location = new System.Drawing.Point(338, 50);
      this.btnDown.Name = "btnDown";
      this.btnDown.Size = new System.Drawing.Size(25, 25);
      this.btnDown.TabIndex = 72;
      this.btnDown.UseVisualStyleBackColor = true;
      // 
      // btnUp
      // 
      this.btnUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnUp.FlatAppearance.BorderSize = 0;
      this.btnUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnUp.Image = global::Kinovea.Root.Properties.Resources.thick_arrow_pointing_up_16;
      this.btnUp.Location = new System.Drawing.Point(338, 19);
      this.btnUp.Name = "btnUp";
      this.btnUp.Size = new System.Drawing.Size(25, 25);
      this.btnUp.TabIndex = 71;
      this.btnUp.UseVisualStyleBackColor = true;
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(292, 382);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(99, 24);
      this.btnOK.TabIndex = 64;
      this.btnOK.Text = "Close";
      this.btnOK.UseVisualStyleBackColor = true;
      // 
      // grpScreenList
      // 
      this.grpScreenList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpScreenList.Controls.Add(this.pnlScreenList);
      this.grpScreenList.Location = new System.Drawing.Point(12, 261);
      this.grpScreenList.Name = "grpScreenList";
      this.grpScreenList.Padding = new System.Windows.Forms.Padding(3, 3, 20, 3);
      this.grpScreenList.Size = new System.Drawing.Size(379, 115);
      this.grpScreenList.TabIndex = 65;
      this.grpScreenList.TabStop = false;
      this.grpScreenList.Text = "[name]";
      // 
      // pnlScreenList
      // 
      this.pnlScreenList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pnlScreenList.BackColor = System.Drawing.Color.WhiteSmoke;
      this.pnlScreenList.Controls.Add(this.lblScreen2);
      this.pnlScreenList.Controls.Add(this.btnScreen2);
      this.pnlScreenList.Controls.Add(this.lblScreen1);
      this.pnlScreenList.Controls.Add(this.btnScreen1);
      this.pnlScreenList.Location = new System.Drawing.Point(13, 19);
      this.pnlScreenList.Name = "pnlScreenList";
      this.pnlScreenList.Size = new System.Drawing.Size(350, 79);
      this.pnlScreenList.TabIndex = 66;
      // 
      // lblScreen2
      // 
      this.lblScreen2.AutoSize = true;
      this.lblScreen2.Location = new System.Drawing.Point(47, 52);
      this.lblScreen2.Name = "lblScreen2";
      this.lblScreen2.Size = new System.Drawing.Size(128, 13);
      this.lblScreen2.TabIndex = 70;
      this.lblScreen2.Text = "Playback: video file name";
      // 
      // btnScreen2
      // 
      this.btnScreen2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnScreen2.FlatAppearance.BorderSize = 0;
      this.btnScreen2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnScreen2.Image = global::Kinovea.Root.Properties.Resources.television;
      this.btnScreen2.Location = new System.Drawing.Point(14, 44);
      this.btnScreen2.Name = "btnScreen2";
      this.btnScreen2.Size = new System.Drawing.Size(25, 25);
      this.btnScreen2.TabIndex = 69;
      this.btnScreen2.UseVisualStyleBackColor = true;
      // 
      // lblScreen1
      // 
      this.lblScreen1.AutoSize = true;
      this.lblScreen1.Location = new System.Drawing.Point(47, 19);
      this.lblScreen1.Name = "lblScreen1";
      this.lblScreen1.Size = new System.Drawing.Size(109, 13);
      this.lblScreen1.TabIndex = 68;
      this.lblScreen1.Text = "Capture: camera alias";
      // 
      // btnScreen1
      // 
      this.btnScreen1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnScreen1.FlatAppearance.BorderSize = 0;
      this.btnScreen1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnScreen1.Image = global::Kinovea.Root.Properties.Resources.camera_video;
      this.btnScreen1.Location = new System.Drawing.Point(14, 12);
      this.btnScreen1.Name = "btnScreen1";
      this.btnScreen1.Size = new System.Drawing.Size(25, 25);
      this.btnScreen1.TabIndex = 67;
      this.btnScreen1.UseVisualStyleBackColor = true;
      // 
      // imgListScreens
      // 
      this.imgListScreens.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgListScreens.ImageStream")));
      this.imgListScreens.TransparentColor = System.Drawing.Color.Transparent;
      this.imgListScreens.Images.SetKeyName(0, "capture");
      this.imgListScreens.Images.SetKeyName(1, "dualcapture");
      this.imgListScreens.Images.SetKeyName(2, "dualmixed");
      this.imgListScreens.Images.SetKeyName(3, "dualplayback");
      this.imgListScreens.Images.SetKeyName(4, "explorer");
      this.imgListScreens.Images.SetKeyName(5, "playback");
      // 
      // FormWindowManager
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(403, 418);
      this.Controls.Add(this.grpScreenList);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.grpIdentifier);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormWindowManager";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.Text = "Manage windows";
      this.grpIdentifier.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.olvWindows)).EndInit();
      this.grpScreenList.ResumeLayout(false);
      this.pnlScreenList.ResumeLayout(false);
      this.pnlScreenList.PerformLayout();
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpIdentifier;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.GroupBox grpScreenList;
        private System.Windows.Forms.Panel pnlScreenList;
        private System.Windows.Forms.Label lblScreen2;
        private System.Windows.Forms.Button btnScreen2;
        private System.Windows.Forms.Label lblScreen1;
        private System.Windows.Forms.Button btnScreen1;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.ImageList imgListStatus;
        private BrightIdeasSoftware.ObjectListView olvWindows;
        private System.Windows.Forms.ImageList imgListScreens;
    }
}