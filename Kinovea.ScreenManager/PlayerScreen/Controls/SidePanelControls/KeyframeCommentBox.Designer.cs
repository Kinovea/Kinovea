
namespace Kinovea.ScreenManager
{
    partial class KeyframeCommentBox
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
      this.tbName = new System.Windows.Forms.TextBox();
      this.btnColor = new System.Windows.Forms.Button();
      this.rtbComment = new System.Windows.Forms.RichTextBox();
      this.btnSidebar = new System.Windows.Forms.Button();
      this.pnlComment = new System.Windows.Forms.Panel();
      this.btnComments = new System.Windows.Forms.Button();
      this.btnTopbar = new System.Windows.Forms.Button();
      this.btnRightBar = new System.Windows.Forms.Button();
      this.btnBottomBar = new System.Windows.Forms.Button();
      this.button2 = new System.Windows.Forms.Button();
      this.button1 = new System.Windows.Forms.Button();
      this.btnClose = new System.Windows.Forms.Button();
      this.pbThumbnail = new System.Windows.Forms.PictureBox();
      this.pnlComment.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pbThumbnail)).BeginInit();
      this.SuspendLayout();
      // 
      // lblTimecode
      // 
      this.lblTimecode.AutoSize = true;
      this.lblTimecode.BackColor = System.Drawing.Color.Transparent;
      this.lblTimecode.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblTimecode.ForeColor = System.Drawing.Color.DimGray;
      this.lblTimecode.Location = new System.Drawing.Point(78, 33);
      this.lblTimecode.Name = "lblTimecode";
      this.lblTimecode.Size = new System.Drawing.Size(54, 13);
      this.lblTimecode.TabIndex = 89;
      this.lblTimecode.Text = "Timecode";
      this.lblTimecode.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // tbName
      // 
      this.tbName.BackColor = System.Drawing.Color.WhiteSmoke;
      this.tbName.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.tbName.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
      this.tbName.Location = new System.Drawing.Point(81, 13);
      this.tbName.Name = "tbName";
      this.tbName.Size = new System.Drawing.Size(51, 18);
      this.tbName.TabIndex = 87;
      this.tbName.Text = "Name";
      this.tbName.TextChanged += new System.EventHandler(this.tbName_TextChanged);
      this.tbName.Enter += new System.EventHandler(this.tbName_Enter);
      this.tbName.Leave += new System.EventHandler(this.tbName_Leave);
      // 
      // btnColor
      // 
      this.btnColor.BackColor = System.Drawing.Color.Black;
      this.btnColor.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnColor.FlatAppearance.BorderSize = 0;
      this.btnColor.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightSteelBlue;
      this.btnColor.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
      this.btnColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnColor.Location = new System.Drawing.Point(14, 11);
      this.btnColor.Name = "btnColor";
      this.btnColor.Size = new System.Drawing.Size(35, 35);
      this.btnColor.TabIndex = 88;
      this.btnColor.UseVisualStyleBackColor = false;
      this.btnColor.Click += new System.EventHandler(this.btnColor_Click);
      // 
      // rtbComment
      // 
      this.rtbComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.rtbComment.AutoWordSelection = true;
      this.rtbComment.BackColor = System.Drawing.Color.Silver;
      this.rtbComment.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.rtbComment.DetectUrls = false;
      this.rtbComment.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rtbComment.Location = new System.Drawing.Point(3, 3);
      this.rtbComment.Name = "rtbComment";
      this.rtbComment.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
      this.rtbComment.Size = new System.Drawing.Size(235, 82);
      this.rtbComment.TabIndex = 90;
      this.rtbComment.Text = "Comment";
      this.rtbComment.TextChanged += new System.EventHandler(this.rtbComment_TextChanged);
      this.rtbComment.Enter += new System.EventHandler(this.rtbComment_Enter);
      this.rtbComment.Leave += new System.EventHandler(this.rtbComment_Leave);
      // 
      // btnSidebar
      // 
      this.btnSidebar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.btnSidebar.BackColor = System.Drawing.Color.Silver;
      this.btnSidebar.FlatAppearance.BorderSize = 0;
      this.btnSidebar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnSidebar.Location = new System.Drawing.Point(1, 1);
      this.btnSidebar.Name = "btnSidebar";
      this.btnSidebar.Size = new System.Drawing.Size(5, 284);
      this.btnSidebar.TabIndex = 91;
      this.btnSidebar.UseVisualStyleBackColor = false;
      // 
      // pnlComment
      // 
      this.pnlComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pnlComment.Controls.Add(this.btnComments);
      this.pnlComment.Controls.Add(this.rtbComment);
      this.pnlComment.Location = new System.Drawing.Point(12, 178);
      this.pnlComment.Name = "pnlComment";
      this.pnlComment.Size = new System.Drawing.Size(245, 90);
      this.pnlComment.TabIndex = 92;
      // 
      // btnComments
      // 
      this.btnComments.BackColor = System.Drawing.Color.Transparent;
      this.btnComments.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.keyframe_comment;
      this.btnComments.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.btnComments.Cursor = System.Windows.Forms.Cursors.Default;
      this.btnComments.FlatAppearance.BorderSize = 0;
      this.btnComments.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.btnComments.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.btnComments.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnComments.Location = new System.Drawing.Point(3, 3);
      this.btnComments.Name = "btnComments";
      this.btnComments.Size = new System.Drawing.Size(20, 20);
      this.btnComments.TabIndex = 100;
      this.btnComments.UseVisualStyleBackColor = false;
      // 
      // btnTopbar
      // 
      this.btnTopbar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.btnTopbar.BackColor = System.Drawing.Color.Silver;
      this.btnTopbar.FlatAppearance.BorderSize = 0;
      this.btnTopbar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnTopbar.Location = new System.Drawing.Point(5, 1);
      this.btnTopbar.Name = "btnTopbar";
      this.btnTopbar.Size = new System.Drawing.Size(263, 5);
      this.btnTopbar.TabIndex = 95;
      this.btnTopbar.UseVisualStyleBackColor = false;
      // 
      // btnRightBar
      // 
      this.btnRightBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.btnRightBar.BackColor = System.Drawing.Color.Silver;
      this.btnRightBar.FlatAppearance.BorderSize = 0;
      this.btnRightBar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnRightBar.Location = new System.Drawing.Point(265, 1);
      this.btnRightBar.Name = "btnRightBar";
      this.btnRightBar.Size = new System.Drawing.Size(5, 284);
      this.btnRightBar.TabIndex = 96;
      this.btnRightBar.UseVisualStyleBackColor = false;
      // 
      // btnBottomBar
      // 
      this.btnBottomBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.btnBottomBar.BackColor = System.Drawing.Color.Silver;
      this.btnBottomBar.FlatAppearance.BorderSize = 0;
      this.btnBottomBar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnBottomBar.Location = new System.Drawing.Point(4, 280);
      this.btnBottomBar.Name = "btnBottomBar";
      this.btnBottomBar.Size = new System.Drawing.Size(263, 5);
      this.btnBottomBar.TabIndex = 97;
      this.btnBottomBar.UseVisualStyleBackColor = false;
      // 
      // button2
      // 
      this.button2.BackColor = System.Drawing.Color.Transparent;
      this.button2.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.keyframe_time2;
      this.button2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.button2.Cursor = System.Windows.Forms.Cursors.Default;
      this.button2.FlatAppearance.BorderSize = 0;
      this.button2.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.button2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.button2.Location = new System.Drawing.Point(55, 29);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(20, 20);
      this.button2.TabIndex = 99;
      this.button2.UseVisualStyleBackColor = false;
      // 
      // button1
      // 
      this.button1.BackColor = System.Drawing.Color.Transparent;
      this.button1.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.keyframe_bookmark;
      this.button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.button1.Cursor = System.Windows.Forms.Cursors.Default;
      this.button1.FlatAppearance.BorderSize = 0;
      this.button1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.button1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.button1.Location = new System.Drawing.Point(54, 12);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(20, 20);
      this.button1.TabIndex = 98;
      this.button1.UseVisualStyleBackColor = false;
      // 
      // btnClose
      // 
      this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnClose.BackColor = System.Drawing.Color.Transparent;
      this.btnClose.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.close_square;
      this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnClose.FlatAppearance.BorderSize = 0;
      this.btnClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnClose.Location = new System.Drawing.Point(237, 11);
      this.btnClose.Name = "btnClose";
      this.btnClose.Size = new System.Drawing.Size(20, 20);
      this.btnClose.TabIndex = 94;
      this.btnClose.UseVisualStyleBackColor = false;
      this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
      this.btnClose.MouseEnter += new System.EventHandler(this.btnClose_MouseEnter);
      this.btnClose.MouseLeave += new System.EventHandler(this.btnClose_MouseLeave);
      // 
      // pbThumbnail
      // 
      this.pbThumbnail.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pbThumbnail.Location = new System.Drawing.Point(12, 52);
      this.pbThumbnail.Name = "pbThumbnail";
      this.pbThumbnail.Size = new System.Drawing.Size(245, 120);
      this.pbThumbnail.TabIndex = 93;
      this.pbThumbnail.TabStop = false;
      this.pbThumbnail.Click += new System.EventHandler(this.pbThumbnail_Click);
      // 
      // KeyframeCommentBox
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Gainsboro;
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.btnBottomBar);
      this.Controls.Add(this.btnRightBar);
      this.Controls.Add(this.btnTopbar);
      this.Controls.Add(this.btnClose);
      this.Controls.Add(this.pbThumbnail);
      this.Controls.Add(this.pnlComment);
      this.Controls.Add(this.btnSidebar);
      this.Controls.Add(this.lblTimecode);
      this.Controls.Add(this.tbName);
      this.Controls.Add(this.btnColor);
      this.DoubleBuffered = true;
      this.ForeColor = System.Drawing.Color.Gray;
      this.Margin = new System.Windows.Forms.Padding(3, 10, 3, 10);
      this.Name = "KeyframeCommentBox";
      this.Size = new System.Drawing.Size(271, 286);
      this.Click += new System.EventHandler(this.KeyframeCommentBox_Click);
      this.Enter += new System.EventHandler(this.KeyframeCommentBox_Enter);
      this.Resize += new System.EventHandler(this.KeyframeCommentBox_Resize);
      this.pnlComment.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pbThumbnail)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTimecode;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Button btnColor;
        private System.Windows.Forms.RichTextBox rtbComment;
        private System.Windows.Forms.Button btnSidebar;
        private System.Windows.Forms.Panel pnlComment;
        private System.Windows.Forms.PictureBox pbThumbnail;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnTopbar;
        private System.Windows.Forms.Button btnRightBar;
        private System.Windows.Forms.Button btnBottomBar;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btnComments;
    }
}
