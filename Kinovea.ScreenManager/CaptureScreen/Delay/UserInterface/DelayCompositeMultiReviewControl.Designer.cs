namespace Kinovea.ScreenManager
{
    partial class DelayCompositeMultiReviewControl
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
            this.cbImageCount = new System.Windows.Forms.ComboBox();
            this.lblImageCount = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cbImageCount
            // 
            this.cbImageCount.FormattingEnabled = true;
            this.cbImageCount.Location = new System.Drawing.Point(144, 3);
            this.cbImageCount.Name = "cbImageCount";
            this.cbImageCount.Size = new System.Drawing.Size(85, 21);
            this.cbImageCount.TabIndex = 21;
            this.cbImageCount.SelectedIndexChanged += new System.EventHandler(this.cbImageCount_SelectedIndexChanged);
            // 
            // lblImageCount
            // 
            this.lblImageCount.AutoSize = true;
            this.lblImageCount.Location = new System.Drawing.Point(10, 6);
            this.lblImageCount.Name = "lblImageCount";
            this.lblImageCount.Size = new System.Drawing.Size(98, 13);
            this.lblImageCount.TabIndex = 20;
            this.lblImageCount.Text = "Number of images :";
            // 
            // DelayCompositeMultiReviewControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.cbImageCount);
            this.Controls.Add(this.lblImageCount);
            this.Name = "DelayCompositeMultiReviewControl";
            this.Size = new System.Drawing.Size(256, 107);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbImageCount;
        private System.Windows.Forms.Label lblImageCount;
    }
}
