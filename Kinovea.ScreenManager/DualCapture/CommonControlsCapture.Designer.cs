namespace Kinovea.ScreenManager
{
    partial class CommonControlsCapture
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
            this.components = new System.ComponentModel.Container();
            this.lblInfo = new System.Windows.Forms.Label();
            this.toolTips = new System.Windows.Forms.ToolTip(this.components);
            this.btnGrab = new System.Windows.Forms.Button();
            this.btnRecord = new System.Windows.Forms.Button();
            this.btnSnapshot = new System.Windows.Forms.Button();
            this.btnSwap = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInfo.Location = new System.Drawing.Point(14, 15);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(78, 12);
            this.lblInfo.TabIndex = 10;
            this.lblInfo.Text = "Common controls";
            // 
            // btnGrab
            // 
            this.btnGrab.BackColor = System.Drawing.Color.Transparent;
            this.btnGrab.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnGrab.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGrab.FlatAppearance.BorderSize = 0;
            this.btnGrab.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnGrab.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnGrab.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGrab.Image = global::Kinovea.ScreenManager.Properties.Capture.circled_play_16;
            this.btnGrab.Location = new System.Drawing.Point(103, 8);
            this.btnGrab.MinimumSize = new System.Drawing.Size(30, 25);
            this.btnGrab.Name = "btnGrab";
            this.btnGrab.Size = new System.Drawing.Size(30, 25);
            this.btnGrab.TabIndex = 31;
            this.btnGrab.UseVisualStyleBackColor = false;
            this.btnGrab.Click += new System.EventHandler(this.btnGrab_Click);
            // 
            // btnRecord
            // 
            this.btnRecord.BackColor = System.Drawing.Color.Transparent;
            this.btnRecord.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnRecord.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRecord.FlatAppearance.BorderSize = 0;
            this.btnRecord.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnRecord.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnRecord.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRecord.Image = global::Kinovea.ScreenManager.Properties.Capture.circle_16;
            this.btnRecord.Location = new System.Drawing.Point(170, 8);
            this.btnRecord.MinimumSize = new System.Drawing.Size(20, 25);
            this.btnRecord.Name = "btnRecord";
            this.btnRecord.Size = new System.Drawing.Size(25, 25);
            this.btnRecord.TabIndex = 32;
            this.btnRecord.UseVisualStyleBackColor = false;
            this.btnRecord.Click += new System.EventHandler(this.btnRecord_Click);
            // 
            // btnSnapshot
            // 
            this.btnSnapshot.BackColor = System.Drawing.Color.Transparent;
            this.btnSnapshot.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnSnapshot.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSnapshot.FlatAppearance.BorderSize = 0;
            this.btnSnapshot.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnSnapshot.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnSnapshot.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSnapshot.Image = global::Kinovea.ScreenManager.Properties.Capture.screenshot_16;
            this.btnSnapshot.Location = new System.Drawing.Point(139, 8);
            this.btnSnapshot.MinimumSize = new System.Drawing.Size(25, 25);
            this.btnSnapshot.Name = "btnSnapshot";
            this.btnSnapshot.Size = new System.Drawing.Size(25, 25);
            this.btnSnapshot.TabIndex = 33;
            this.btnSnapshot.Tag = "";
            this.btnSnapshot.UseVisualStyleBackColor = false;
            this.btnSnapshot.Click += new System.EventHandler(this.btnSnapshot_Click);
            // 
            // btnSwap
            // 
            this.btnSwap.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnSwap.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSwap.FlatAppearance.BorderSize = 0;
            this.btnSwap.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
            this.btnSwap.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSwap.Image = global::Kinovea.ScreenManager.Properties.Resources.flatswap3d;
            this.btnSwap.Location = new System.Drawing.Point(198, 8);
            this.btnSwap.Margin = new System.Windows.Forms.Padding(0);
            this.btnSwap.MinimumSize = new System.Drawing.Size(18, 18);
            this.btnSwap.Name = "btnSwap";
            this.btnSwap.Size = new System.Drawing.Size(25, 25);
            this.btnSwap.TabIndex = 11;
            this.btnSwap.UseVisualStyleBackColor = true;
            this.btnSwap.Click += new System.EventHandler(this.btnSwap_Click);
            // 
            // CommonControlsCapture
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.btnGrab);
            this.Controls.Add(this.btnRecord);
            this.Controls.Add(this.btnSnapshot);
            this.Controls.Add(this.btnSwap);
            this.Controls.Add(this.lblInfo);
            this.Name = "CommonControlsCapture";
            this.Size = new System.Drawing.Size(665, 45);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Button btnSwap;
        private System.Windows.Forms.ToolTip toolTips;
        private System.Windows.Forms.Button btnGrab;
        private System.Windows.Forms.Button btnRecord;
        private System.Windows.Forms.Button btnSnapshot;
    }
}
