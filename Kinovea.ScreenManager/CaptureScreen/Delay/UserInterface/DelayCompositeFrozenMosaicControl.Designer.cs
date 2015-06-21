namespace Kinovea.ScreenManager
{
    partial class DelayCompositeFrozenMosaicControl
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
            this.tbRefreshRate = new System.Windows.Forms.TextBox();
            this.lblRefreshRate = new System.Windows.Forms.Label();
            this.tbStart = new System.Windows.Forms.TextBox();
            this.lblStart = new System.Windows.Forms.Label();
            this.tbInterval = new System.Windows.Forms.TextBox();
            this.lblInterval = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cbImageCount
            // 
            this.cbImageCount.FormattingEnabled = true;
            this.cbImageCount.Location = new System.Drawing.Point(147, 2);
            this.cbImageCount.Name = "cbImageCount";
            this.cbImageCount.Size = new System.Drawing.Size(85, 21);
            this.cbImageCount.TabIndex = 21;
            this.cbImageCount.SelectedIndexChanged += new System.EventHandler(this.cbImageCount_SelectedIndexChanged);
            // 
            // lblImageCount
            // 
            this.lblImageCount.AutoSize = true;
            this.lblImageCount.Location = new System.Drawing.Point(13, 5);
            this.lblImageCount.Name = "lblImageCount";
            this.lblImageCount.Size = new System.Drawing.Size(98, 13);
            this.lblImageCount.TabIndex = 20;
            this.lblImageCount.Text = "Number of images :";
            // 
            // tbRefreshRate
            // 
            this.tbRefreshRate.Location = new System.Drawing.Point(147, 31);
            this.tbRefreshRate.Name = "tbRefreshRate";
            this.tbRefreshRate.Size = new System.Drawing.Size(87, 20);
            this.tbRefreshRate.TabIndex = 25;
            this.tbRefreshRate.TextChanged += new System.EventHandler(this.tbRefreshRate_TextChanged);
            // 
            // lblRefreshRate
            // 
            this.lblRefreshRate.AutoSize = true;
            this.lblRefreshRate.Location = new System.Drawing.Point(15, 33);
            this.lblRefreshRate.Name = "lblRefreshRate";
            this.lblRefreshRate.Size = new System.Drawing.Size(71, 13);
            this.lblRefreshRate.TabIndex = 24;
            this.lblRefreshRate.Text = "Refresh rate :";
            // 
            // tbStart
            // 
            this.tbStart.Location = new System.Drawing.Point(147, 57);
            this.tbStart.Name = "tbStart";
            this.tbStart.Size = new System.Drawing.Size(87, 20);
            this.tbStart.TabIndex = 27;
            this.tbStart.TextChanged += new System.EventHandler(this.tbStart_TextChanged);
            // 
            // lblStart
            // 
            this.lblStart.AutoSize = true;
            this.lblStart.Location = new System.Drawing.Point(15, 59);
            this.lblStart.Name = "lblStart";
            this.lblStart.Size = new System.Drawing.Size(35, 13);
            this.lblStart.TabIndex = 26;
            this.lblStart.Text = "Start :";
            // 
            // tbInterval
            // 
            this.tbInterval.Location = new System.Drawing.Point(147, 83);
            this.tbInterval.Name = "tbInterval";
            this.tbInterval.Size = new System.Drawing.Size(87, 20);
            this.tbInterval.TabIndex = 29;
            this.tbInterval.TextChanged += new System.EventHandler(this.tbInterval_TextChanged);
            // 
            // lblInterval
            // 
            this.lblInterval.AutoSize = true;
            this.lblInterval.Location = new System.Drawing.Point(15, 85);
            this.lblInterval.Name = "lblInterval";
            this.lblInterval.Size = new System.Drawing.Size(48, 13);
            this.lblInterval.TabIndex = 28;
            this.lblInterval.Text = "Interval :";
            // 
            // DelayCompositeFrozenMosaicControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.tbInterval);
            this.Controls.Add(this.lblInterval);
            this.Controls.Add(this.tbStart);
            this.Controls.Add(this.lblStart);
            this.Controls.Add(this.tbRefreshRate);
            this.Controls.Add(this.lblRefreshRate);
            this.Controls.Add(this.cbImageCount);
            this.Controls.Add(this.lblImageCount);
            this.Name = "DelayCompositeFrozenMosaicControl";
            this.Size = new System.Drawing.Size(256, 107);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbImageCount;
        private System.Windows.Forms.Label lblImageCount;
        private System.Windows.Forms.TextBox tbRefreshRate;
        private System.Windows.Forms.Label lblRefreshRate;
        private System.Windows.Forms.TextBox tbStart;
        private System.Windows.Forms.Label lblStart;
        private System.Windows.Forms.TextBox tbInterval;
        private System.Windows.Forms.Label lblInterval;
    }
}
