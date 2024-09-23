
namespace Kinovea.ScreenManager
{
    partial class ControlDrawingTrackingSetup
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
      this.pnlConfig = new System.Windows.Forms.Panel();
      this.SuspendLayout();
      // 
      // pnlConfig
      // 
      this.pnlConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pnlConfig.BackColor = System.Drawing.Color.White;
      this.pnlConfig.ForeColor = System.Drawing.Color.Gray;
      this.pnlConfig.Location = new System.Drawing.Point(3, 3);
      this.pnlConfig.Name = "pnlConfig";
      this.pnlConfig.Size = new System.Drawing.Size(265, 219);
      this.pnlConfig.TabIndex = 93;
      this.pnlConfig.Click += new System.EventHandler(this.pnlConfig_Click);
      // 
      // ControlDrawingStyle
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Gainsboro;
      this.Controls.Add(this.pnlConfig);
      this.DoubleBuffered = true;
      this.ForeColor = System.Drawing.Color.Gray;
      this.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
      this.Name = "ControlDrawingStyle";
      this.Size = new System.Drawing.Size(271, 244);
      this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel pnlConfig;
    }
}
