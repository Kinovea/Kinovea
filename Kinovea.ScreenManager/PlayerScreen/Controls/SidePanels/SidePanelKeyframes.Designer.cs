
namespace Kinovea.ScreenManager
{
    partial class SidePanelKeyframes
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
      this.pnlKeyframes = new System.Windows.Forms.Panel();
      this.pnlCommentBox = new System.Windows.Forms.Panel();
      this.SuspendLayout();
      // 
      // pnlKeyframes
      // 
      this.pnlKeyframes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pnlKeyframes.AutoScroll = true;
      this.pnlKeyframes.AutoScrollMargin = new System.Drawing.Size(10, 0);
      this.pnlKeyframes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
      this.pnlKeyframes.Location = new System.Drawing.Point(3, 3);
      this.pnlKeyframes.Name = "pnlKeyframes";
      this.pnlKeyframes.Size = new System.Drawing.Size(269, 412);
      this.pnlKeyframes.TabIndex = 0;
      // 
      // pnlCommentBox
      // 
      this.pnlCommentBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pnlCommentBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
      this.pnlCommentBox.Location = new System.Drawing.Point(3, 421);
      this.pnlCommentBox.Name = "pnlCommentBox";
      this.pnlCommentBox.Size = new System.Drawing.Size(269, 171);
      this.pnlCommentBox.TabIndex = 1;
      // 
      // SidePanelKeyframes
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
      this.Controls.Add(this.pnlCommentBox);
      this.Controls.Add(this.pnlKeyframes);
      this.DoubleBuffered = true;
      this.Name = "SidePanelKeyframes";
      this.Size = new System.Drawing.Size(275, 595);
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlKeyframes;
        private System.Windows.Forms.Panel pnlCommentBox;
    }
}
