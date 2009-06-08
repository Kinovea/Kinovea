namespace Videa.Root
{
    partial class HelpVideosDialog
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
        	this.lstVideos = new System.Windows.Forms.ListBox();
        	this.lblFilterByLanguage = new System.Windows.Forms.Label();
        	this.cmbLanguageFilter = new System.Windows.Forms.ComboBox();
        	this.lblSelectVideos = new System.Windows.Forms.Label();
        	this.lblAboutThisVideo = new System.Windows.Forms.Label();
        	this.rtbVideoComment = new System.Windows.Forms.RichTextBox();
        	this.btnWatch = new System.Windows.Forms.Button();
        	this.btnCancel = new System.Windows.Forms.Button();
        	this.btnVideos = new System.Windows.Forms.Button();
        	this.lblInstructionGetMore = new System.Windows.Forms.Label();
        	this.SuspendLayout();
        	// 
        	// lstVideos
        	// 
        	this.lstVideos.FormattingEnabled = true;
        	this.lstVideos.Items.AddRange(new object[] {
        	        	        	"video 1",
        	        	        	"video 2"});
        	this.lstVideos.Location = new System.Drawing.Point(16, 100);
        	this.lstVideos.Name = "lstVideos";
        	this.lstVideos.Size = new System.Drawing.Size(445, 121);
        	this.lstVideos.TabIndex = 10;
        	this.lstVideos.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.LstVideosMouseDoubleClick);
        	this.lstVideos.SelectedIndexChanged += new System.EventHandler(this.lstVideos_SelectedIndexChanged);
        	// 
        	// lblFilterByLanguage
        	// 
        	this.lblFilterByLanguage.AutoSize = true;
        	this.lblFilterByLanguage.Location = new System.Drawing.Point(175, 76);
        	this.lblFilterByLanguage.Name = "lblFilterByLanguage";
        	this.lblFilterByLanguage.Size = new System.Drawing.Size(96, 13);
        	this.lblFilterByLanguage.TabIndex = 10;
        	this.lblFilterByLanguage.Text = "Filter by language :";
        	// 
        	// cmbLanguageFilter
        	// 
        	this.cmbLanguageFilter.FormattingEnabled = true;
        	this.cmbLanguageFilter.Items.AddRange(new object[] {
        	        	        	"All",
        	        	        	"English",
        	        	        	"Français"});
        	this.cmbLanguageFilter.Location = new System.Drawing.Point(277, 73);
        	this.cmbLanguageFilter.Name = "cmbLanguageFilter";
        	this.cmbLanguageFilter.Size = new System.Drawing.Size(184, 21);
        	this.cmbLanguageFilter.TabIndex = 5;
        	this.cmbLanguageFilter.Text = "All";
        	this.cmbLanguageFilter.SelectedIndexChanged += new System.EventHandler(this.cmbLanguageFilter_SelectedIndexChanged);
        	// 
        	// lblSelectVideos
        	// 
        	this.lblSelectVideos.AutoSize = true;
        	this.lblSelectVideos.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.lblSelectVideos.Location = new System.Drawing.Point(102, 34);
        	this.lblSelectVideos.Name = "lblSelectVideos";
        	this.lblSelectVideos.Size = new System.Drawing.Size(261, 13);
        	this.lblSelectVideos.TabIndex = 8;
        	this.lblSelectVideos.Text = "Select which video you would like to watch :";
        	// 
        	// lblAboutThisVideo
        	// 
        	this.lblAboutThisVideo.AutoSize = true;
        	this.lblAboutThisVideo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.lblAboutThisVideo.Location = new System.Drawing.Point(35, 229);
        	this.lblAboutThisVideo.Name = "lblAboutThisVideo";
        	this.lblAboutThisVideo.Size = new System.Drawing.Size(112, 13);
        	this.lblAboutThisVideo.TabIndex = 11;
        	this.lblAboutThisVideo.Text = "About this Video...";
        	// 
        	// rtbVideoComment
        	// 
        	this.rtbVideoComment.BorderStyle = System.Windows.Forms.BorderStyle.None;
        	this.rtbVideoComment.Location = new System.Drawing.Point(16, 252);
        	this.rtbVideoComment.Name = "rtbVideoComment";
        	this.rtbVideoComment.Size = new System.Drawing.Size(445, 61);
        	this.rtbVideoComment.TabIndex = 12;
        	this.rtbVideoComment.Text = "This video will guide you through the most basic stuff in Kinovea :\nOpening a vid" +
        	"eo, selecting a working zone, and saving this zone back to a file.";
        	// 
        	// btnWatch
        	// 
        	this.btnWatch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnWatch.Location = new System.Drawing.Point(285, 355);
        	this.btnWatch.Name = "btnWatch";
        	this.btnWatch.Size = new System.Drawing.Size(85, 22);
        	this.btnWatch.TabIndex = 15;
        	this.btnWatch.Text = "Watch";
        	this.btnWatch.UseVisualStyleBackColor = true;
        	this.btnWatch.Click += new System.EventHandler(this.btnWatch_Click);
        	// 
        	// btnCancel
        	// 
        	this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnCancel.Location = new System.Drawing.Point(376, 355);
        	this.btnCancel.Name = "btnCancel";
        	this.btnCancel.Size = new System.Drawing.Size(85, 22);
        	this.btnCancel.TabIndex = 20;
        	this.btnCancel.Text = "Cancel";
        	this.btnCancel.UseVisualStyleBackColor = true;
        	this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
        	// 
        	// btnVideos
        	// 
        	this.btnVideos.FlatAppearance.BorderSize = 0;
        	this.btnVideos.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
        	this.btnVideos.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
        	this.btnVideos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnVideos.Image = global::Videa.Root.Properties.Resources.videos;
        	this.btnVideos.Location = new System.Drawing.Point(16, 15);
        	this.btnVideos.Name = "btnVideos";
        	this.btnVideos.Size = new System.Drawing.Size(80, 74);
        	this.btnVideos.TabIndex = 15;
        	this.btnVideos.UseVisualStyleBackColor = true;
        	// 
        	// lblInstructionGetMore
        	// 
        	this.lblInstructionGetMore.Location = new System.Drawing.Point(15, 322);
        	this.lblInstructionGetMore.Name = "lblInstructionGetMore";
        	this.lblInstructionGetMore.Size = new System.Drawing.Size(446, 30);
        	this.lblInstructionGetMore.TabIndex = 16;
        	this.lblInstructionGetMore.Text = "You can get more Videos through the Options > Check for Updates Menu";
        	// 
        	// HelpVideosDialog
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackColor = System.Drawing.Color.White;
        	this.ClientSize = new System.Drawing.Size(474, 389);
        	this.Controls.Add(this.lblInstructionGetMore);
        	this.Controls.Add(this.btnVideos);
        	this.Controls.Add(this.btnWatch);
        	this.Controls.Add(this.btnCancel);
        	this.Controls.Add(this.rtbVideoComment);
        	this.Controls.Add(this.lblAboutThisVideo);
        	this.Controls.Add(this.lblFilterByLanguage);
        	this.Controls.Add(this.cmbLanguageFilter);
        	this.Controls.Add(this.lblSelectVideos);
        	this.Controls.Add(this.lstVideos);
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
        	this.MaximizeBox = false;
        	this.MinimizeBox = false;
        	this.Name = "HelpVideosDialog";
        	this.ShowInTaskbar = false;
        	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        	this.Text = "HelpVideosDialog";
        	this.ResumeLayout(false);
        	this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ListBox lstVideos;
        private System.Windows.Forms.Label lblFilterByLanguage;
        private System.Windows.Forms.ComboBox cmbLanguageFilter;
        private System.Windows.Forms.Label lblSelectVideos;
        private System.Windows.Forms.Label lblAboutThisVideo;
        private System.Windows.Forms.RichTextBox rtbVideoComment;
        private System.Windows.Forms.Button btnWatch;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnVideos;
        private System.Windows.Forms.Label lblInstructionGetMore;
    }
}