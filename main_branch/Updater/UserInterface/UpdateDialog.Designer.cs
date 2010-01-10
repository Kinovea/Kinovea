namespace Kinovea.Updater
{
    partial class UpdateDialog
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.labelInfos = new System.Windows.Forms.Label();
            this.progressDownload = new System.Windows.Forms.ProgressBar();
            this.btnDownload = new System.Windows.Forms.Button();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.bgwkrDownloader = new System.ComponentModel.BackgroundWorker();
            this.lnkKinovea = new System.Windows.Forms.LinkLabel();
            this.lblNewVersion = new System.Windows.Forms.Label();
            this.pageSoftware = new System.Windows.Forms.Panel();
            this.lblChangeLog = new System.Windows.Forms.Label();
            this.rtbxChangeLog = new System.Windows.Forms.RichTextBox();
            this.lblNewVersionFileSize = new System.Windows.Forms.Label();
            this.lblSoftware = new System.Windows.Forms.Label();
            this.btnSoftware = new System.Windows.Forms.Button();
            this.pnlButtonSoftware = new System.Windows.Forms.Panel();
            this.pnlButtonManuals = new System.Windows.Forms.Panel();
            this.btnManuals = new System.Windows.Forms.Button();
            this.lblManuals = new System.Windows.Forms.Label();
            this.pnlButtonVideos = new System.Windows.Forms.Panel();
            this.lblVideos = new System.Windows.Forms.Label();
            this.btnVideos = new System.Windows.Forms.Button();
            this.pageManuals = new System.Windows.Forms.Panel();
            this.lblAllManualsUpToDate = new System.Windows.Forms.Label();
            this.lblTotalSelectedManuals = new System.Windows.Forms.Label();
            this.lblSelectManual = new System.Windows.Forms.Label();
            this.chklstManuals = new System.Windows.Forms.CheckedListBox();
            this.pageVideos = new System.Windows.Forms.Panel();
            this.lblAllVideosUpToDate = new System.Windows.Forms.Label();
            this.lblFilterByLanguage = new System.Windows.Forms.Label();
            this.cmbLanguageFilter = new System.Windows.Forms.ComboBox();
            this.lblTotalSelectedVideos = new System.Windows.Forms.Label();
            this.lblSelectVideos = new System.Windows.Forms.Label();
            this.chklstVideos = new System.Windows.Forms.CheckedListBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.lblInstruction = new System.Windows.Forms.Label();
            this.pageSoftware.SuspendLayout();
            this.pnlButtonSoftware.SuspendLayout();
            this.pnlButtonManuals.SuspendLayout();
            this.pnlButtonVideos.SuspendLayout();
            this.pageManuals.SuspendLayout();
            this.pageVideos.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(397, 395);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(85, 22);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // labelInfos
            // 
            this.labelInfos.AutoSize = true;
            this.labelInfos.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelInfos.Location = new System.Drawing.Point(10, 15);
            this.labelInfos.Name = "labelInfos";
            this.labelInfos.Size = new System.Drawing.Size(163, 13);
            this.labelInfos.TabIndex = 5;
            this.labelInfos.Text = "A new version is available !";
            // 
            // progressDownload
            // 
            this.progressDownload.Location = new System.Drawing.Point(15, 367);
            this.progressDownload.Name = "progressDownload";
            this.progressDownload.Size = new System.Drawing.Size(467, 18);
            this.progressDownload.TabIndex = 6;
            this.progressDownload.Visible = false;
            // 
            // btnDownload
            // 
            this.btnDownload.Location = new System.Drawing.Point(306, 395);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(85, 22);
            this.btnDownload.TabIndex = 7;
            this.btnDownload.Text = "Download";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // lnkKinovea
            // 
            this.lnkKinovea.AutoSize = true;
            this.lnkKinovea.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lnkKinovea.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkKinovea.LinkColor = System.Drawing.Color.Blue;
            this.lnkKinovea.Location = new System.Drawing.Point(19, 401);
            this.lnkKinovea.Name = "lnkKinovea";
            this.lnkKinovea.Size = new System.Drawing.Size(81, 12);
            this.lnkKinovea.TabIndex = 8;
            this.lnkKinovea.TabStop = true;
            this.lnkKinovea.Text = "www.kinovea.org";
            this.lnkKinovea.VisitedLinkColor = System.Drawing.Color.Blue;
            this.lnkKinovea.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkKinovea_LinkClicked);
            // 
            // lblNewVersion
            // 
            this.lblNewVersion.AutoSize = true;
            this.lblNewVersion.Location = new System.Drawing.Point(8, 40);
            this.lblNewVersion.Name = "lblNewVersion";
            this.lblNewVersion.Size = new System.Drawing.Size(188, 13);
            this.lblNewVersion.TabIndex = 6;
            this.lblNewVersion.Text = "New Version : 0.6.4 - ( Current : 0.6.2 )";
            // 
            // pageSoftware
            // 
            this.pageSoftware.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pageSoftware.Controls.Add(this.lblChangeLog);
            this.pageSoftware.Controls.Add(this.rtbxChangeLog);
            this.pageSoftware.Controls.Add(this.lblNewVersionFileSize);
            this.pageSoftware.Controls.Add(this.lblNewVersion);
            this.pageSoftware.Controls.Add(this.labelInfos);
            this.pageSoftware.Location = new System.Drawing.Point(15, 121);
            this.pageSoftware.Name = "pageSoftware";
            this.pageSoftware.Size = new System.Drawing.Size(468, 230);
            this.pageSoftware.TabIndex = 12;
            // 
            // lblChangeLog
            // 
            this.lblChangeLog.AutoSize = true;
            this.lblChangeLog.Location = new System.Drawing.Point(10, 66);
            this.lblChangeLog.Name = "lblChangeLog";
            this.lblChangeLog.Size = new System.Drawing.Size(71, 13);
            this.lblChangeLog.TabIndex = 10;
            this.lblChangeLog.Text = "Change Log :";
            this.lblChangeLog.Visible = false;
            // 
            // rtbxChangeLog
            // 
            this.rtbxChangeLog.BackColor = System.Drawing.Color.GhostWhite;
            this.rtbxChangeLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbxChangeLog.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbxChangeLog.Location = new System.Drawing.Point(10, 91);
            this.rtbxChangeLog.Name = "rtbxChangeLog";
            this.rtbxChangeLog.Size = new System.Drawing.Size(441, 119);
            this.rtbxChangeLog.TabIndex = 9;
            this.rtbxChangeLog.Text = "The quick brown fox jumps over the lazy dog";
            this.rtbxChangeLog.Visible = false;
            // 
            // lblNewVersionFileSize
            // 
            this.lblNewVersionFileSize.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblNewVersionFileSize.AutoSize = true;
            this.lblNewVersionFileSize.Location = new System.Drawing.Point(360, 40);
            this.lblNewVersionFileSize.Name = "lblNewVersionFileSize";
            this.lblNewVersionFileSize.Size = new System.Drawing.Size(89, 13);
            this.lblNewVersionFileSize.TabIndex = 8;
            this.lblNewVersionFileSize.Text = "File Size : 5.4 MB";
            // 
            // lblSoftware
            // 
            this.lblSoftware.BackColor = System.Drawing.Color.White;
            this.lblSoftware.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSoftware.Location = new System.Drawing.Point(0, 74);
            this.lblSoftware.Name = "lblSoftware";
            this.lblSoftware.Size = new System.Drawing.Size(80, 30);
            this.lblSoftware.TabIndex = 14;
            this.lblSoftware.Text = "Software";
            this.lblSoftware.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblSoftware.Click += new System.EventHandler(this.lblSoftware_Click);
            // 
            // btnSoftware
            // 
            this.btnSoftware.FlatAppearance.BorderSize = 0;
            this.btnSoftware.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnSoftware.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btnSoftware.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSoftware.Image = global::Kinovea.Updater.Properties.Resources.Install;
            this.btnSoftware.Location = new System.Drawing.Point(0, 0);
            this.btnSoftware.Name = "btnSoftware";
            this.btnSoftware.Size = new System.Drawing.Size(80, 74);
            this.btnSoftware.TabIndex = 13;
            this.btnSoftware.UseVisualStyleBackColor = true;
            this.btnSoftware.Click += new System.EventHandler(this.btnSoftware_Click);
            // 
            // pnlButtonSoftware
            // 
            this.pnlButtonSoftware.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlButtonSoftware.Controls.Add(this.btnSoftware);
            this.pnlButtonSoftware.Controls.Add(this.lblSoftware);
            this.pnlButtonSoftware.Location = new System.Drawing.Point(15, 16);
            this.pnlButtonSoftware.Name = "pnlButtonSoftware";
            this.pnlButtonSoftware.Size = new System.Drawing.Size(80, 104);
            this.pnlButtonSoftware.TabIndex = 19;
            // 
            // pnlButtonManuals
            // 
            this.pnlButtonManuals.Controls.Add(this.btnManuals);
            this.pnlButtonManuals.Controls.Add(this.lblManuals);
            this.pnlButtonManuals.Location = new System.Drawing.Point(100, 16);
            this.pnlButtonManuals.Name = "pnlButtonManuals";
            this.pnlButtonManuals.Size = new System.Drawing.Size(80, 104);
            this.pnlButtonManuals.TabIndex = 20;
            // 
            // btnManuals
            // 
            this.btnManuals.FlatAppearance.BorderSize = 0;
            this.btnManuals.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnManuals.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btnManuals.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnManuals.Image = global::Kinovea.Updater.Properties.Resources.manual3;
            this.btnManuals.Location = new System.Drawing.Point(0, 0);
            this.btnManuals.Name = "btnManuals";
            this.btnManuals.Size = new System.Drawing.Size(80, 74);
            this.btnManuals.TabIndex = 13;
            this.btnManuals.UseVisualStyleBackColor = true;
            this.btnManuals.Click += new System.EventHandler(this.btnManuals_Click);
            // 
            // lblManuals
            // 
            this.lblManuals.BackColor = System.Drawing.Color.White;
            this.lblManuals.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblManuals.Location = new System.Drawing.Point(0, 74);
            this.lblManuals.Name = "lblManuals";
            this.lblManuals.Size = new System.Drawing.Size(80, 30);
            this.lblManuals.TabIndex = 14;
            this.lblManuals.Text = "Manual";
            this.lblManuals.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblManuals.Click += new System.EventHandler(this.lblManuals_Click);
            // 
            // pnlButtonVideos
            // 
            this.pnlButtonVideos.Controls.Add(this.lblVideos);
            this.pnlButtonVideos.Controls.Add(this.btnVideos);
            this.pnlButtonVideos.Location = new System.Drawing.Point(185, 16);
            this.pnlButtonVideos.Name = "pnlButtonVideos";
            this.pnlButtonVideos.Size = new System.Drawing.Size(80, 104);
            this.pnlButtonVideos.TabIndex = 21;
            // 
            // lblVideos
            // 
            this.lblVideos.BackColor = System.Drawing.Color.White;
            this.lblVideos.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVideos.Location = new System.Drawing.Point(0, 74);
            this.lblVideos.Name = "lblVideos";
            this.lblVideos.Size = new System.Drawing.Size(80, 30);
            this.lblVideos.TabIndex = 15;
            this.lblVideos.Text = "Videos";
            this.lblVideos.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnVideos
            // 
            this.btnVideos.FlatAppearance.BorderSize = 0;
            this.btnVideos.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnVideos.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btnVideos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVideos.Image = global::Kinovea.Updater.Properties.Resources.videos;
            this.btnVideos.Location = new System.Drawing.Point(0, 0);
            this.btnVideos.Name = "btnVideos";
            this.btnVideos.Size = new System.Drawing.Size(80, 74);
            this.btnVideos.TabIndex = 13;
            this.btnVideos.UseVisualStyleBackColor = true;
            this.btnVideos.Click += new System.EventHandler(this.btnVideos_Click);
            // 
            // pageManuals
            // 
            this.pageManuals.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pageManuals.Controls.Add(this.lblAllManualsUpToDate);
            this.pageManuals.Controls.Add(this.lblTotalSelectedManuals);
            this.pageManuals.Controls.Add(this.lblSelectManual);
            this.pageManuals.Controls.Add(this.chklstManuals);
            this.pageManuals.Location = new System.Drawing.Point(489, 8);
            this.pageManuals.Name = "pageManuals";
            this.pageManuals.Size = new System.Drawing.Size(468, 230);
            this.pageManuals.TabIndex = 22;
            this.pageManuals.Visible = false;
            // 
            // lblAllManualsUpToDate
            // 
            this.lblAllManualsUpToDate.AutoSize = true;
            this.lblAllManualsUpToDate.Location = new System.Drawing.Point(139, 204);
            this.lblAllManualsUpToDate.Name = "lblAllManualsUpToDate";
            this.lblAllManualsUpToDate.Size = new System.Drawing.Size(170, 13);
            this.lblAllManualsUpToDate.TabIndex = 3;
            this.lblAllManualsUpToDate.Text = "All files in this catory are up to date";
            this.lblAllManualsUpToDate.Visible = false;
            // 
            // lblTotalSelectedManuals
            // 
            this.lblTotalSelectedManuals.AutoSize = true;
            this.lblTotalSelectedManuals.Location = new System.Drawing.Point(16, 204);
            this.lblTotalSelectedManuals.Name = "lblTotalSelectedManuals";
            this.lblTotalSelectedManuals.Size = new System.Drawing.Size(117, 13);
            this.lblTotalSelectedManuals.TabIndex = 2;
            this.lblTotalSelectedManuals.Text = "Total selected : 11 MB.";
            // 
            // lblSelectManual
            // 
            this.lblSelectManual.AutoSize = true;
            this.lblSelectManual.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSelectManual.Location = new System.Drawing.Point(10, 15);
            this.lblSelectManual.Name = "lblSelectManual";
            this.lblSelectManual.Size = new System.Drawing.Size(304, 13);
            this.lblSelectManual.TabIndex = 1;
            this.lblSelectManual.Text = "Select which manual(s) you would like to download :";
            // 
            // chklstManuals
            // 
            this.chklstManuals.CheckOnClick = true;
            this.chklstManuals.FormattingEnabled = true;
            this.chklstManuals.Items.AddRange(new object[] {
            "English manual ( Mise à jour - 5.4 Mo )",
            "Manuel en français ( Nouveau - 5.6 Mo )",
            "fcdvb",
            "dfbh",
            "df",
            "dfbn",
            "fg",
            "fgn",
            "fgn",
            "fgn",
            "fgn",
            "fgn",
            "fgn",
            "fgn"});
            this.chklstManuals.Location = new System.Drawing.Point(13, 43);
            this.chklstManuals.Name = "chklstManuals";
            this.chklstManuals.Size = new System.Drawing.Size(436, 139);
            this.chklstManuals.TabIndex = 0;
            this.chklstManuals.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.chklstManuals_ItemCheck);
            // 
            // pageVideos
            // 
            this.pageVideos.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pageVideos.Controls.Add(this.lblAllVideosUpToDate);
            this.pageVideos.Controls.Add(this.lblFilterByLanguage);
            this.pageVideos.Controls.Add(this.cmbLanguageFilter);
            this.pageVideos.Controls.Add(this.lblTotalSelectedVideos);
            this.pageVideos.Controls.Add(this.lblSelectVideos);
            this.pageVideos.Controls.Add(this.chklstVideos);
            this.pageVideos.Location = new System.Drawing.Point(489, 244);
            this.pageVideos.Name = "pageVideos";
            this.pageVideos.Size = new System.Drawing.Size(468, 230);
            this.pageVideos.TabIndex = 23;
            this.pageVideos.Visible = false;
            // 
            // lblAllVideosUpToDate
            // 
            this.lblAllVideosUpToDate.AutoSize = true;
            this.lblAllVideosUpToDate.Location = new System.Drawing.Point(144, 203);
            this.lblAllVideosUpToDate.Name = "lblAllVideosUpToDate";
            this.lblAllVideosUpToDate.Size = new System.Drawing.Size(170, 13);
            this.lblAllVideosUpToDate.TabIndex = 8;
            this.lblAllVideosUpToDate.Text = "All files in this catory are up to date";
            this.lblAllVideosUpToDate.Visible = false;
            // 
            // lblFilterByLanguage
            // 
            this.lblFilterByLanguage.AutoSize = true;
            this.lblFilterByLanguage.Location = new System.Drawing.Point(16, 38);
            this.lblFilterByLanguage.Name = "lblFilterByLanguage";
            this.lblFilterByLanguage.Size = new System.Drawing.Size(96, 13);
            this.lblFilterByLanguage.TabIndex = 7;
            this.lblFilterByLanguage.Text = "Filter by language :";
            // 
            // cmbLanguageFilter
            // 
            this.cmbLanguageFilter.FormattingEnabled = true;
            this.cmbLanguageFilter.Items.AddRange(new object[] {
            "All",
            "English",
            "Français"});
            this.cmbLanguageFilter.Location = new System.Drawing.Point(265, 34);
            this.cmbLanguageFilter.Name = "cmbLanguageFilter";
            this.cmbLanguageFilter.Size = new System.Drawing.Size(184, 21);
            this.cmbLanguageFilter.TabIndex = 6;
            this.cmbLanguageFilter.Text = "All";
            this.cmbLanguageFilter.SelectedIndexChanged += new System.EventHandler(this.cmbLanguageFilter_SelectedIndexChanged);
            // 
            // lblTotalSelectedVideos
            // 
            this.lblTotalSelectedVideos.AutoSize = true;
            this.lblTotalSelectedVideos.Location = new System.Drawing.Point(17, 203);
            this.lblTotalSelectedVideos.Name = "lblTotalSelectedVideos";
            this.lblTotalSelectedVideos.Size = new System.Drawing.Size(117, 13);
            this.lblTotalSelectedVideos.TabIndex = 5;
            this.lblTotalSelectedVideos.Text = "Total selected : 83 MB.";
            // 
            // lblSelectVideos
            // 
            this.lblSelectVideos.AutoSize = true;
            this.lblSelectVideos.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSelectVideos.Location = new System.Drawing.Point(10, 15);
            this.lblSelectVideos.Name = "lblSelectVideos";
            this.lblSelectVideos.Size = new System.Drawing.Size(295, 13);
            this.lblSelectVideos.TabIndex = 4;
            this.lblSelectVideos.Text = "Select which video(s) you would like to download :";
            // 
            // chklstVideos
            // 
            this.chklstVideos.CheckOnClick = true;
            this.chklstVideos.FormattingEnabled = true;
            this.chklstVideos.Items.AddRange(new object[] {
            "Visualisation - découverte ( Mise à jour - 5.4 M",
            "Visualisation - Ajustement de l\'image ( Nouveau - 5.6 Mo )",
            "fcdvb",
            "dfbh",
            "df",
            "dfbn",
            "fg",
            "fgn",
            "fgn",
            "fgn",
            "fgn"});
            this.chklstVideos.Location = new System.Drawing.Point(13, 63);
            this.chklstVideos.Name = "chklstVideos";
            this.chklstVideos.Size = new System.Drawing.Size(436, 124);
            this.chklstVideos.TabIndex = 3;
            this.chklstVideos.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.chklstVideos_ItemCheck);
            // 
            // lblInstruction
            // 
            this.lblInstruction.AutoSize = true;
            this.lblInstruction.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstruction.Location = new System.Drawing.Point(15, 367);
            this.lblInstruction.Name = "lblInstruction";
            this.lblInstruction.Size = new System.Drawing.Size(292, 13);
            this.lblInstruction.TabIndex = 24;
            this.lblInstruction.Text = "Click the \'Download\' button to begin downloading.";
            // 
            // UpdateDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(994, 487);
            this.Controls.Add(this.lblInstruction);
            this.Controls.Add(this.pageVideos);
            this.Controls.Add(this.pageManuals);
            this.Controls.Add(this.pnlButtonVideos);
            this.Controls.Add(this.pnlButtonManuals);
            this.Controls.Add(this.pageSoftware);
            this.Controls.Add(this.lnkKinovea);
            this.Controls.Add(this.btnDownload);
            this.Controls.Add(this.progressDownload);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.pnlButtonSoftware);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdateDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "   Kinovea - UpdateDialog";
            this.pageSoftware.ResumeLayout(false);
            this.pageSoftware.PerformLayout();
            this.pnlButtonSoftware.ResumeLayout(false);
            this.pnlButtonManuals.ResumeLayout(false);
            this.pnlButtonVideos.ResumeLayout(false);
            this.pageManuals.ResumeLayout(false);
            this.pageManuals.PerformLayout();
            this.pageVideos.ResumeLayout(false);
            this.pageVideos.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label labelInfos;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.ProgressBar progressDownload;
        private System.ComponentModel.BackgroundWorker bgwkrDownloader;
        private System.Windows.Forms.LinkLabel lnkKinovea;
        private System.Windows.Forms.Label lblNewVersion;
        private System.Windows.Forms.Panel pageSoftware;
        private System.Windows.Forms.Button btnSoftware;
        private System.Windows.Forms.Label lblSoftware;
        private System.Windows.Forms.Label lblNewVersionFileSize;
        private System.Windows.Forms.Panel pnlButtonSoftware;
        private System.Windows.Forms.Panel pnlButtonManuals;
        private System.Windows.Forms.Button btnManuals;
        private System.Windows.Forms.Label lblManuals;
        private System.Windows.Forms.Panel pnlButtonVideos;
        private System.Windows.Forms.Button btnVideos;
        private System.Windows.Forms.Panel pageManuals;
        private System.Windows.Forms.Panel pageVideos;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckedListBox chklstManuals;
        private System.Windows.Forms.Label lblTotalSelectedManuals;
        private System.Windows.Forms.Label lblSelectManual;
        private System.Windows.Forms.Label lblFilterByLanguage;
        private System.Windows.Forms.ComboBox cmbLanguageFilter;
        private System.Windows.Forms.Label lblTotalSelectedVideos;
        private System.Windows.Forms.Label lblSelectVideos;
        private System.Windows.Forms.CheckedListBox chklstVideos;
        private System.Windows.Forms.Label lblChangeLog;
        private System.Windows.Forms.RichTextBox rtbxChangeLog;
        private System.Windows.Forms.Label lblInstruction;
        private System.Windows.Forms.Label lblVideos;
        private System.Windows.Forms.Label lblAllManualsUpToDate;
        private System.Windows.Forms.Label lblAllVideosUpToDate;

    }
}