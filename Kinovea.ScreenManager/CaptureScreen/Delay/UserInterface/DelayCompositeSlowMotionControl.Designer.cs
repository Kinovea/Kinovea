namespace Kinovea.ScreenManager
{
    partial class DelayCompositeSlowMotionControl
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
            this.label1 = new System.Windows.Forms.Label();
            this.tbRefreshRate = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // cbImageCount
            // 
            this.cbImageCount.FormattingEnabled = true;
            this.cbImageCount.Location = new System.Drawing.Point(145, 3);
            this.cbImageCount.Name = "cbImageCount";
            this.cbImageCount.Size = new System.Drawing.Size(85, 21);
            this.cbImageCount.TabIndex = 21;
            this.cbImageCount.SelectedIndexChanged += new System.EventHandler(this.cbImageCount_SelectedIndexChanged);
            // 
            // lblImageCount
            // 
            this.lblImageCount.AutoSize = true;
            this.lblImageCount.Location = new System.Drawing.Point(11, 6);
            this.lblImageCount.Name = "lblImageCount";
            this.lblImageCount.Size = new System.Drawing.Size(98, 13);
            this.lblImageCount.TabIndex = 20;
            this.lblImageCount.Text = "Number of images :";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 13);
            this.label1.TabIndex = 22;
            this.label1.Text = "Slow motion factor :";
            // 
            // tbRefreshRate
            // 
            this.tbRefreshRate.Location = new System.Drawing.Point(145, 30);
            this.tbRefreshRate.Name = "tbRefreshRate";
            this.tbRefreshRate.Size = new System.Drawing.Size(85, 20);
            this.tbRefreshRate.TabIndex = 23;
            this.tbRefreshRate.TextChanged += new System.EventHandler(this.tbRefreshRate_TextChanged);
            // 
            // DelayCompositeSlowMotionControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.tbRefreshRate);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbImageCount);
            this.Controls.Add(this.lblImageCount);
            this.Name = "DelayCompositeSlowMotionControl";
            this.Size = new System.Drawing.Size(256, 107);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbImageCount;
        private System.Windows.Forms.Label lblImageCount;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbRefreshRate;
    }
}
