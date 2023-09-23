
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
      this.pnlComment.SuspendLayout();
      this.SuspendLayout();
      // 
      // lblTimecode
      // 
      this.lblTimecode.AutoSize = true;
      this.lblTimecode.BackColor = System.Drawing.Color.Transparent;
      this.lblTimecode.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblTimecode.ForeColor = System.Drawing.Color.DimGray;
      this.lblTimecode.Location = new System.Drawing.Point(56, 33);
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
      this.tbName.Location = new System.Drawing.Point(59, 15);
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
      this.btnSidebar.Size = new System.Drawing.Size(5, 158);
      this.btnSidebar.TabIndex = 91;
      this.btnSidebar.UseVisualStyleBackColor = false;
      // 
      // pnlComment
      // 
      this.pnlComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pnlComment.Controls.Add(this.rtbComment);
      this.pnlComment.Location = new System.Drawing.Point(14, 53);
      this.pnlComment.Name = "pnlComment";
      this.pnlComment.Size = new System.Drawing.Size(245, 96);
      this.pnlComment.TabIndex = 92;
      // 
      // KeyframeCommentBox
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.WhiteSmoke;
      this.Controls.Add(this.pnlComment);
      this.Controls.Add(this.btnSidebar);
      this.Controls.Add(this.lblTimecode);
      this.Controls.Add(this.tbName);
      this.Controls.Add(this.btnColor);
      this.DoubleBuffered = true;
      this.ForeColor = System.Drawing.Color.Gray;
      this.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
      this.Name = "KeyframeCommentBox";
      this.Size = new System.Drawing.Size(271, 160);
      this.Click += new System.EventHandler(this.KeyframeCommentBox_Click);
      this.Enter += new System.EventHandler(this.KeyframeCommentBox_Enter);
      this.Resize += new System.EventHandler(this.KeyframeCommentBox_Resize);
      this.pnlComment.ResumeLayout(false);
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
    }
}
