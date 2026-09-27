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
      this.lblSignal = new System.Windows.Forms.Label();
      this.lblLoad = new System.Windows.Forms.Label();
      this.lblDrops = new System.Windows.Forms.Label();
      this.lblQueue = new System.Windows.Forms.Label();
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnSignal = new System.Windows.Forms.Button();
      this.btnLoad = new System.Windows.Forms.Button();
      this.btnQueue = new System.Windows.Forms.Button();
      this.btnDrops = new System.Windows.Forms.Button();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // lblSignal
      // 
      this.lblSignal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.lblSignal.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblSignal.Location = new System.Drawing.Point(28, 3);
      this.lblSignal.Margin = new System.Windows.Forms.Padding(3);
      this.lblSignal.Name = "lblSignal";
      this.lblSignal.Size = new System.Drawing.Size(156, 15);
      this.lblSignal.TabIndex = 6;
      this.lblSignal.Text = "Signal: 1000.00 fps";
      this.lblSignal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblLoad
      // 
      this.lblLoad.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.lblLoad.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblLoad.Location = new System.Drawing.Point(214, 3);
      this.lblLoad.Margin = new System.Windows.Forms.Padding(3);
      this.lblLoad.Name = "lblLoad";
      this.lblLoad.Size = new System.Drawing.Size(100, 15);
      this.lblLoad.TabIndex = 3;
      this.lblLoad.Text = "Load: 100 %";
      this.lblLoad.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblDrops
      // 
      this.lblDrops.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.lblDrops.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblDrops.Location = new System.Drawing.Point(342, 3);
      this.lblDrops.Margin = new System.Windows.Forms.Padding(3);
      this.lblDrops.Name = "lblDrops";
      this.lblDrops.Size = new System.Drawing.Size(95, 15);
      this.lblDrops.TabIndex = 11;
      this.lblDrops.Text = "Drops: 100";
      this.lblDrops.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblQueue
      // 
      this.lblQueue.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.lblQueue.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblQueue.Location = new System.Drawing.Point(469, 4);
      this.lblQueue.Margin = new System.Windows.Forms.Padding(3);
      this.lblQueue.Name = "lblQueue";
      this.lblQueue.Size = new System.Drawing.Size(107, 13);
      this.lblQueue.TabIndex = 12;
      this.lblQueue.Text = "Queue: 100";
      this.lblQueue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // panel1
      // 
      this.panel1.BackColor = System.Drawing.Color.White;
      this.panel1.Controls.Add(this.btnSignal);
      this.panel1.Controls.Add(this.lblSignal);
      this.panel1.Controls.Add(this.btnLoad);
      this.panel1.Controls.Add(this.btnQueue);
      this.panel1.Controls.Add(this.btnDrops);
      this.panel1.Controls.Add(this.lblQueue);
      this.panel1.Controls.Add(this.lblLoad);
      this.panel1.Controls.Add(this.lblDrops);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel1.Location = new System.Drawing.Point(0, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(686, 22);
      this.panel1.TabIndex = 14;
      // 
      // btnSignal
      // 
      this.btnSignal.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.signal;
      this.btnSignal.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.btnSignal.FlatAppearance.BorderSize = 0;
      this.btnSignal.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.btnSignal.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.btnSignal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnSignal.Location = new System.Drawing.Point(4, 1);
      this.btnSignal.Margin = new System.Windows.Forms.Padding(3, 1, 3, 0);
      this.btnSignal.Name = "btnSignal";
      this.btnSignal.Size = new System.Drawing.Size(18, 18);
      this.btnSignal.TabIndex = 8;
      this.btnSignal.UseVisualStyleBackColor = true;
      // 
      // btnLoad
      // 
      this.btnLoad.FlatAppearance.BorderSize = 0;
      this.btnLoad.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.btnLoad.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.btnLoad.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnLoad.Image = global::Kinovea.ScreenManager.Properties.Resources.load_sun;
      this.btnLoad.Location = new System.Drawing.Point(190, 0);
      this.btnLoad.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
      this.btnLoad.Name = "btnLoad";
      this.btnLoad.Size = new System.Drawing.Size(18, 18);
      this.btnLoad.TabIndex = 2;
      this.btnLoad.UseVisualStyleBackColor = true;
      // 
      // btnQueue
      // 
      this.btnQueue.FlatAppearance.BorderSize = 0;
      this.btnQueue.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.btnQueue.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.btnQueue.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnQueue.Image = global::Kinovea.ScreenManager.Properties.Resources.bursts_16;
      this.btnQueue.Location = new System.Drawing.Point(445, 1);
      this.btnQueue.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
      this.btnQueue.Name = "btnQueue";
      this.btnQueue.Size = new System.Drawing.Size(18, 18);
      this.btnQueue.TabIndex = 13;
      this.btnQueue.UseVisualStyleBackColor = true;
      // 
      // btnDrops
      // 
      this.btnDrops.FlatAppearance.BorderSize = 0;
      this.btnDrops.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.btnDrops.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.btnDrops.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnDrops.Image = global::Kinovea.ScreenManager.Properties.Resources.missed_call_16;
      this.btnDrops.Location = new System.Drawing.Point(320, 0);
      this.btnDrops.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
      this.btnDrops.Name = "btnDrops";
      this.btnDrops.Size = new System.Drawing.Size(18, 18);
      this.btnDrops.TabIndex = 10;
      this.btnDrops.UseVisualStyleBackColor = true;
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
      this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Label lblLoad;
        private System.Windows.Forms.Label lblSignal;
        private System.Windows.Forms.Button btnSignal;
        private System.Windows.Forms.Button btnDrops;
        private System.Windows.Forms.Label lblDrops;
        private System.Windows.Forms.Label lblQueue;
        private System.Windows.Forms.Button btnQueue;
        private System.Windows.Forms.Panel panel1;
    }
}
