namespace Kinovea.ScreenManager
{
    partial class InfobarCapture
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
      this.btnCameraType = new System.Windows.Forms.Button();
      this.lblSignal = new System.Windows.Forms.Label();
      this.btnLoadStatus = new System.Windows.Forms.Button();
      this.lblLoad = new System.Windows.Forms.Label();
      this.button6 = new System.Windows.Forms.Button();
      this.lblDrops = new System.Windows.Forms.Label();
      this.lblQueue = new System.Windows.Forms.Label();
      this.button1 = new System.Windows.Forms.Button();
      this.panel1 = new System.Windows.Forms.Panel();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnCameraType
      // 
      this.btnCameraType.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.signal;
      this.btnCameraType.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.btnCameraType.FlatAppearance.BorderSize = 0;
      this.btnCameraType.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.btnCameraType.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.btnCameraType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnCameraType.Location = new System.Drawing.Point(4, 3);
      this.btnCameraType.Margin = new System.Windows.Forms.Padding(3, 1, 3, 0);
      this.btnCameraType.Name = "btnCameraType";
      this.btnCameraType.Size = new System.Drawing.Size(18, 18);
      this.btnCameraType.TabIndex = 8;
      this.btnCameraType.UseVisualStyleBackColor = true;
      // 
      // lblSignal
      // 
      this.lblSignal.AutoSize = true;
      this.lblSignal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.lblSignal.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblSignal.Location = new System.Drawing.Point(28, 5);
      this.lblSignal.Margin = new System.Windows.Forms.Padding(3);
      this.lblSignal.Name = "lblSignal";
      this.lblSignal.Size = new System.Drawing.Size(193, 13);
      this.lblSignal.TabIndex = 6;
      this.lblSignal.Text = "Signal: 1000.00 fps (1000 MB/s)";
      this.lblSignal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btnLoadStatus
      // 
      this.btnLoadStatus.FlatAppearance.BorderSize = 0;
      this.btnLoadStatus.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.btnLoadStatus.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.btnLoadStatus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnLoadStatus.Image = global::Kinovea.ScreenManager.Properties.Resources.load_sun;
      this.btnLoadStatus.Location = new System.Drawing.Point(251, 2);
      this.btnLoadStatus.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
      this.btnLoadStatus.Name = "btnLoadStatus";
      this.btnLoadStatus.Size = new System.Drawing.Size(18, 18);
      this.btnLoadStatus.TabIndex = 2;
      this.btnLoadStatus.UseVisualStyleBackColor = true;
      // 
      // lblLoad
      // 
      this.lblLoad.AutoSize = true;
      this.lblLoad.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.lblLoad.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblLoad.Location = new System.Drawing.Point(275, 5);
      this.lblLoad.Margin = new System.Windows.Forms.Padding(3);
      this.lblLoad.Name = "lblLoad";
      this.lblLoad.Size = new System.Drawing.Size(73, 13);
      this.lblLoad.TabIndex = 3;
      this.lblLoad.Text = "Load: 100 %";
      this.lblLoad.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // button6
      // 
      this.button6.FlatAppearance.BorderSize = 0;
      this.button6.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.button6.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.button6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.button6.Image = global::Kinovea.ScreenManager.Properties.Resources.drops;
      this.button6.Location = new System.Drawing.Point(381, 2);
      this.button6.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
      this.button6.Name = "button6";
      this.button6.Size = new System.Drawing.Size(18, 18);
      this.button6.TabIndex = 10;
      this.button6.UseVisualStyleBackColor = true;
      // 
      // lblDrops
      // 
      this.lblDrops.AutoSize = true;
      this.lblDrops.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.lblDrops.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblDrops.Location = new System.Drawing.Point(405, 5);
      this.lblDrops.Margin = new System.Windows.Forms.Padding(3);
      this.lblDrops.Name = "lblDrops";
      this.lblDrops.Size = new System.Drawing.Size(67, 13);
      this.lblDrops.TabIndex = 11;
      this.lblDrops.Text = "Drops: 100";
      this.lblDrops.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblQueue
      // 
      this.lblQueue.AutoSize = true;
      this.lblQueue.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.lblQueue.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblQueue.Location = new System.Drawing.Point(530, 6);
      this.lblQueue.Margin = new System.Windows.Forms.Padding(3);
      this.lblQueue.Name = "lblQueue";
      this.lblQueue.Size = new System.Drawing.Size(67, 13);
      this.lblQueue.TabIndex = 12;
      this.lblQueue.Text = "Queue: 100";
      this.lblQueue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // button1
      // 
      this.button1.FlatAppearance.BorderSize = 0;
      this.button1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.button1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.button1.Image = global::Kinovea.ScreenManager.Properties.Resources.bursts_16;
      this.button1.Location = new System.Drawing.Point(506, 3);
      this.button1.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(18, 18);
      this.button1.TabIndex = 13;
      this.button1.UseVisualStyleBackColor = true;
      // 
      // panel1
      // 
      this.panel1.BackColor = System.Drawing.Color.White;
      this.panel1.Controls.Add(this.btnCameraType);
      this.panel1.Controls.Add(this.lblSignal);
      this.panel1.Controls.Add(this.btnLoadStatus);
      this.panel1.Controls.Add(this.button1);
      this.panel1.Controls.Add(this.button6);
      this.panel1.Controls.Add(this.lblQueue);
      this.panel1.Controls.Add(this.lblLoad);
      this.panel1.Controls.Add(this.lblDrops);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel1.Location = new System.Drawing.Point(0, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(686, 22);
      this.panel1.TabIndex = 14;
      // 
      // InfobarCapture
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.BackColor = System.Drawing.Color.Transparent;
      this.Controls.Add(this.panel1);
      this.Name = "InfobarCapture";
      this.Size = new System.Drawing.Size(686, 22);
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnLoadStatus;
        private System.Windows.Forms.Label lblLoad;
        private System.Windows.Forms.Label lblSignal;
        private System.Windows.Forms.Button btnCameraType;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Label lblDrops;
        private System.Windows.Forms.Label lblQueue;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Panel panel1;
    }
}
