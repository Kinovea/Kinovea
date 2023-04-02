
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
      this.flowKeyframes = new System.Windows.Forms.FlowLayoutPanel();
      this.SuspendLayout();
      // 
      // flowKeyframes
      // 
      this.flowKeyframes.AutoScroll = true;
      this.flowKeyframes.BackColor = System.Drawing.Color.White;
      this.flowKeyframes.Dock = System.Windows.Forms.DockStyle.Fill;
      this.flowKeyframes.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
      this.flowKeyframes.Location = new System.Drawing.Point(0, 0);
      this.flowKeyframes.Name = "flowKeyframes";
      this.flowKeyframes.Size = new System.Drawing.Size(275, 595);
      this.flowKeyframes.TabIndex = 1;
      this.flowKeyframes.WrapContents = false;
      this.flowKeyframes.Layout += new System.Windows.Forms.LayoutEventHandler(this.flowKeyframes_Layout);
      // 
      // SidePanelKeyframes
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.Controls.Add(this.flowKeyframes);
      this.DoubleBuffered = true;
      this.Name = "SidePanelKeyframes";
      this.Size = new System.Drawing.Size(275, 595);
      this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.FlowLayoutPanel flowKeyframes;
    }
}
