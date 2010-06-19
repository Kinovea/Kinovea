namespace Kinovea.ScreenManager
{
    partial class formKeyframeComments
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
        	this.rtbComment = new System.Windows.Forms.RichTextBox();
        	this.txtTitle = new System.Windows.Forms.TextBox();
        	this.SuspendLayout();
        	// 
        	// rtbComment
        	// 
        	this.rtbComment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        	        	        	| System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.rtbComment.BackColor = System.Drawing.Color.WhiteSmoke;
        	this.rtbComment.BorderStyle = System.Windows.Forms.BorderStyle.None;
        	this.rtbComment.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.rtbComment.Location = new System.Drawing.Point(12, 58);
        	this.rtbComment.Name = "rtbComment";
        	this.rtbComment.Size = new System.Drawing.Size(341, 184);
        	this.rtbComment.TabIndex = 10;
        	this.rtbComment.Text = "";
        	// 
        	// txtTitle
        	// 
        	this.txtTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.txtTitle.BackColor = System.Drawing.Color.WhiteSmoke;
        	this.txtTitle.BorderStyle = System.Windows.Forms.BorderStyle.None;
        	this.txtTitle.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.txtTitle.Location = new System.Drawing.Point(12, 20);
        	this.txtTitle.Name = "txtTitle";
        	this.txtTitle.Size = new System.Drawing.Size(341, 22);
        	this.txtTitle.TabIndex = 5;
        	this.txtTitle.Text = "Keyframe Title";
        	// 
        	// formKeyframeComments
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.ClientSize = new System.Drawing.Size(365, 254);
        	this.Controls.Add(this.txtTitle);
        	this.Controls.Add(this.rtbComment);
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
        	this.MinimumSize = new System.Drawing.Size(147, 132);
        	this.Name = "formKeyframeComments";
        	this.ShowInTaskbar = false;
        	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        	this.Text = "   Commentaire...";
        	this.MouseEnter += new System.EventHandler(this.formKeyframeComments_MouseEnter);
        	this.MouseLeave += new System.EventHandler(this.formKeyframeComments_MouseLeave);
        	this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formKeyframeComments_FormClosing);
        	this.ResumeLayout(false);
        	this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbComment;
        private System.Windows.Forms.TextBox txtTitle;
    }
}