namespace Kinovea.ScreenManager
{
    partial class ThumbBox
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
        	this.lblTimecode = new System.Windows.Forms.Label();
        	this.btnComment = new System.Windows.Forms.Button();
        	this.btnClose = new System.Windows.Forms.Button();
        	this.pbThumbnail = new System.Windows.Forms.PictureBox();
        	((System.ComponentModel.ISupportInitialize)(this.pbThumbnail)).BeginInit();
        	this.SuspendLayout();
        	// 
        	// lblTimecode
        	// 
        	this.lblTimecode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.lblTimecode.BackColor = System.Drawing.Color.Black;
        	this.lblTimecode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.lblTimecode.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.lblTimecode.ForeColor = System.Drawing.Color.White;
        	this.lblTimecode.Location = new System.Drawing.Point(1, 62);
        	this.lblTimecode.Name = "lblTimecode";
        	this.lblTimecode.Size = new System.Drawing.Size(100, 14);
        	this.lblTimecode.TabIndex = 2;
        	this.lblTimecode.Text = "0:00:00:00";
        	this.lblTimecode.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        	this.lblTimecode.Visible = false;
        	this.lblTimecode.MouseLeave += new System.EventHandler(this.lblTimecode_MouseLeave);
        	this.lblTimecode.MouseEnter += new System.EventHandler(this.lblTimecode_MouseEnter);
        	// 
        	// btnComment
        	// 
        	this.btnComment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnComment.BackColor = System.Drawing.Color.Transparent;
        	this.btnComment.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.comment20x20;
        	this.btnComment.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
        	this.btnComment.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
        	this.btnComment.FlatAppearance.BorderSize = 0;
        	this.btnComment.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnComment.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
        	this.btnComment.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnComment.ForeColor = System.Drawing.Color.Transparent;
        	this.btnComment.Location = new System.Drawing.Point(5, 1);
        	this.btnComment.Name = "btnComment";
        	this.btnComment.Size = new System.Drawing.Size(15, 15);
        	this.btnComment.TabIndex = 3;
        	this.btnComment.UseVisualStyleBackColor = false;
        	this.btnComment.Visible = false;
        	this.btnComment.Click += new System.EventHandler(this.btnComment_Click);
        	// 
        	// btnClose
        	// 
        	this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnClose.BackColor = System.Drawing.Color.Transparent;
        	this.btnClose.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.close15x15;
        	this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
        	this.btnClose.FlatAppearance.BorderSize = 0;
        	this.btnClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
        	this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnClose.ForeColor = System.Drawing.Color.White;
        	this.btnClose.Location = new System.Drawing.Point(84, 1);
        	this.btnClose.Name = "btnClose";
        	this.btnClose.Size = new System.Drawing.Size(15, 15);
        	this.btnClose.TabIndex = 1;
        	this.btnClose.UseVisualStyleBackColor = false;
        	this.btnClose.Visible = false;
        	this.btnClose.MouseLeave += new System.EventHandler(this.btnClose_MouseLeave);
        	this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
        	// 
        	// pbThumbnail
        	// 
        	this.pbThumbnail.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        	        	        	| System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.pbThumbnail.BackColor = System.Drawing.Color.DimGray;
        	this.pbThumbnail.Location = new System.Drawing.Point(1, 1);
        	this.pbThumbnail.Name = "pbThumbnail";
        	this.pbThumbnail.Size = new System.Drawing.Size(100, 75);
        	this.pbThumbnail.TabIndex = 0;
        	this.pbThumbnail.TabStop = false;
        	this.pbThumbnail.MouseLeave += new System.EventHandler(this.pbThumbnail_MouseLeave);
        	this.pbThumbnail.Click += new System.EventHandler(this.pbThumbnail_Click);
        	this.pbThumbnail.MouseEnter += new System.EventHandler(this.pbThumbnail_MouseEnter);
        	// 
        	// ThumbBox
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackColor = System.Drawing.Color.SteelBlue;
        	this.Controls.Add(this.btnComment);
        	this.Controls.Add(this.lblTimecode);
        	this.Controls.Add(this.btnClose);
        	this.Controls.Add(this.pbThumbnail);
        	this.Name = "ThumbBox";
        	this.Size = new System.Drawing.Size(102, 77);
        	this.MouseLeave += new System.EventHandler(this.ThumbBox_MouseLeave);
        	((System.ComponentModel.ISupportInitialize)(this.pbThumbnail)).EndInit();
        	this.ResumeLayout(false);
        }

        #endregion

        public System.Windows.Forms.PictureBox pbThumbnail;
        public System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblTimecode;
        public System.Windows.Forms.Button btnComment;
    }
}
