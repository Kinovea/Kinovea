namespace Kinovea.ScreenManager
{
    partial class InfobarPlayer
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
      this.lblSize = new System.Windows.Forms.Label();
      this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
      this.btnVideoType = new System.Windows.Forms.Button();
      this.lblFilename = new System.Windows.Forms.Label();
      this.btnSpacer1 = new System.Windows.Forms.Button();
      this.btnSize = new System.Windows.Forms.Button();
      this.btnSpacer2 = new System.Windows.Forms.Button();
      this.btnFPS = new System.Windows.Forms.Button();
      this.lblFps = new System.Windows.Forms.Label();
      this.toolTips = new System.Windows.Forms.ToolTip(this.components);
      this.btnMode = new System.Windows.Forms.Button();
      this.lblMode = new System.Windows.Forms.Label();
      this.btnSpacer3 = new System.Windows.Forms.Button();
      this.flowLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // lblSize
      // 
      this.lblSize.AutoSize = true;
      this.lblSize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.lblSize.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblSize.Location = new System.Drawing.Point(128, 3);
      this.lblSize.Margin = new System.Windows.Forms.Padding(3);
      this.lblSize.Name = "lblSize";
      this.lblSize.Size = new System.Drawing.Size(91, 13);
      this.lblSize.TabIndex = 1;
      this.lblSize.Text = "1920 × 1080 px";
      this.lblSize.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // flowLayoutPanel1
      // 
      this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.flowLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
      this.flowLayoutPanel1.Controls.Add(this.btnVideoType);
      this.flowLayoutPanel1.Controls.Add(this.lblFilename);
      this.flowLayoutPanel1.Controls.Add(this.btnSpacer1);
      this.flowLayoutPanel1.Controls.Add(this.btnSize);
      this.flowLayoutPanel1.Controls.Add(this.lblSize);
      this.flowLayoutPanel1.Controls.Add(this.btnSpacer2);
      this.flowLayoutPanel1.Controls.Add(this.btnFPS);
      this.flowLayoutPanel1.Controls.Add(this.lblFps);
      this.flowLayoutPanel1.Controls.Add(this.btnSpacer3);
      this.flowLayoutPanel1.Controls.Add(this.btnMode);
      this.flowLayoutPanel1.Controls.Add(this.lblMode);
      this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 2);
      this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
      this.flowLayoutPanel1.Name = "flowLayoutPanel1";
      this.flowLayoutPanel1.Size = new System.Drawing.Size(527, 21);
      this.flowLayoutPanel1.TabIndex = 2;
      this.flowLayoutPanel1.WrapContents = false;
      // 
      // btnVideoType
      // 
      this.btnVideoType.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.film_small;
      this.btnVideoType.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.btnVideoType.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnVideoType.FlatAppearance.BorderSize = 0;
      this.btnVideoType.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.btnVideoType.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.btnVideoType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnVideoType.Location = new System.Drawing.Point(3, 1);
      this.btnVideoType.Margin = new System.Windows.Forms.Padding(3, 1, 3, 0);
      this.btnVideoType.Name = "btnVideoType";
      this.btnVideoType.Size = new System.Drawing.Size(18, 18);
      this.btnVideoType.TabIndex = 8;
      this.btnVideoType.UseVisualStyleBackColor = true;
      this.btnVideoType.MouseDown += new System.Windows.Forms.MouseEventHandler(this.fileinfo_MouseDown);
      // 
      // lblFilename
      // 
      this.lblFilename.AutoSize = true;
      this.lblFilename.Cursor = System.Windows.Forms.Cursors.Hand;
      this.lblFilename.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.lblFilename.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblFilename.Location = new System.Drawing.Point(27, 3);
      this.lblFilename.Margin = new System.Windows.Forms.Padding(3);
      this.lblFilename.Name = "lblFilename";
      this.lblFilename.Size = new System.Drawing.Size(55, 13);
      this.lblFilename.TabIndex = 6;
      this.lblFilename.Text = "Filename";
      this.lblFilename.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.lblFilename.MouseDown += new System.Windows.Forms.MouseEventHandler(this.fileinfo_MouseDown);
      // 
      // btnSpacer1
      // 
      this.btnSpacer1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.btnSpacer1.FlatAppearance.BorderSize = 0;
      this.btnSpacer1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.btnSpacer1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.btnSpacer1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnSpacer1.Location = new System.Drawing.Point(88, 3);
      this.btnSpacer1.Name = "btnSpacer1";
      this.btnSpacer1.Size = new System.Drawing.Size(10, 18);
      this.btnSpacer1.TabIndex = 5;
      this.btnSpacer1.UseVisualStyleBackColor = true;
      // 
      // btnSize
      // 
      this.btnSize.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.imagesize_t1;
      this.btnSize.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.btnSize.FlatAppearance.BorderSize = 0;
      this.btnSize.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.btnSize.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.btnSize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnSize.Location = new System.Drawing.Point(104, 1);
      this.btnSize.Margin = new System.Windows.Forms.Padding(3, 1, 3, 0);
      this.btnSize.Name = "btnSize";
      this.btnSize.Size = new System.Drawing.Size(18, 18);
      this.btnSize.TabIndex = 0;
      this.btnSize.UseVisualStyleBackColor = true;
      // 
      // btnSpacer2
      // 
      this.btnSpacer2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.btnSpacer2.FlatAppearance.BorderSize = 0;
      this.btnSpacer2.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.btnSpacer2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.btnSpacer2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnSpacer2.Location = new System.Drawing.Point(225, 3);
      this.btnSpacer2.Name = "btnSpacer2";
      this.btnSpacer2.Size = new System.Drawing.Size(12, 18);
      this.btnSpacer2.TabIndex = 7;
      this.btnSpacer2.UseVisualStyleBackColor = true;
      // 
      // btnFPS
      // 
      this.btnFPS.FlatAppearance.BorderSize = 0;
      this.btnFPS.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.btnFPS.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.btnFPS.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnFPS.Image = global::Kinovea.ScreenManager.Properties.Resources.clock_flat2;
      this.btnFPS.Location = new System.Drawing.Point(243, 0);
      this.btnFPS.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
      this.btnFPS.Name = "btnFPS";
      this.btnFPS.Size = new System.Drawing.Size(18, 18);
      this.btnFPS.TabIndex = 2;
      this.btnFPS.UseVisualStyleBackColor = true;
      // 
      // lblFps
      // 
      this.lblFps.AutoSize = true;
      this.lblFps.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.lblFps.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblFps.Location = new System.Drawing.Point(267, 3);
      this.lblFps.Margin = new System.Windows.Forms.Padding(3);
      this.lblFps.Name = "lblFps";
      this.lblFps.Size = new System.Drawing.Size(61, 13);
      this.lblFps.TabIndex = 3;
      this.lblFps.Text = "50.00 fps";
      this.lblFps.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btnMode
      // 
      this.btnMode.FlatAppearance.BorderSize = 0;
      this.btnMode.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.btnMode.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.btnMode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnMode.Image = global::Kinovea.ScreenManager.Properties.Resources.camera_video;
      this.btnMode.Location = new System.Drawing.Point(352, 0);
      this.btnMode.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
      this.btnMode.Name = "btnMode";
      this.btnMode.Size = new System.Drawing.Size(18, 18);
      this.btnMode.TabIndex = 9;
      this.btnMode.UseVisualStyleBackColor = true;
      // 
      // lblMode
      // 
      this.lblMode.AutoSize = true;
      this.lblMode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.lblMode.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblMode.Location = new System.Drawing.Point(376, 3);
      this.lblMode.Margin = new System.Windows.Forms.Padding(3);
      this.lblMode.Name = "lblMode";
      this.lblMode.Size = new System.Drawing.Size(55, 13);
      this.lblMode.TabIndex = 10;
      this.lblMode.Text = "Analysis";
      this.lblMode.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btnSpacer3
      // 
      this.btnSpacer3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.btnSpacer3.FlatAppearance.BorderSize = 0;
      this.btnSpacer3.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.btnSpacer3.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.btnSpacer3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnSpacer3.Location = new System.Drawing.Point(334, 3);
      this.btnSpacer3.Name = "btnSpacer3";
      this.btnSpacer3.Size = new System.Drawing.Size(12, 18);
      this.btnSpacer3.TabIndex = 11;
      this.btnSpacer3.UseVisualStyleBackColor = true;
      // 
      // InfobarPlayer
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.BackColor = System.Drawing.Color.Transparent;
      this.Controls.Add(this.flowLayoutPanel1);
      this.Name = "InfobarPlayer";
      this.Size = new System.Drawing.Size(830, 27);
      this.flowLayoutPanel1.ResumeLayout(false);
      this.flowLayoutPanel1.PerformLayout();
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSize;
        private System.Windows.Forms.Label lblSize;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btnFPS;
        private System.Windows.Forms.Label lblFps;
        private System.Windows.Forms.Button btnSpacer1;
        private System.Windows.Forms.Label lblFilename;
        private System.Windows.Forms.Button btnSpacer2;
        private System.Windows.Forms.Button btnVideoType;
        private System.Windows.Forms.ToolTip toolTips;
        private System.Windows.Forms.Button btnMode;
        private System.Windows.Forms.Label lblMode;
        private System.Windows.Forms.Button btnSpacer3;
    }
}
