
namespace Kinovea.ScreenManager
{
    partial class SidePanelTracking
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
      this.controlDrawingName = new Kinovea.ScreenManager.ControlDrawingName();
      this.controlDrawingTrackingSetup = new Kinovea.ScreenManager.ControlDrawingTrackingSetup();
      this.SuspendLayout();
      // 
      // controlDrawingName
      // 
      this.controlDrawingName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.controlDrawingName.BackColor = System.Drawing.Color.White;
      this.controlDrawingName.ForeColor = System.Drawing.Color.Gray;
      this.controlDrawingName.Location = new System.Drawing.Point(3, 5);
      this.controlDrawingName.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
      this.controlDrawingName.Name = "controlDrawingName";
      this.controlDrawingName.Size = new System.Drawing.Size(269, 49);
      this.controlDrawingName.TabIndex = 1;
      // 
      // controlDrawingTrackingSetup
      // 
      this.controlDrawingTrackingSetup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.controlDrawingTrackingSetup.BackColor = System.Drawing.Color.White;
      this.controlDrawingTrackingSetup.ForeColor = System.Drawing.Color.Gray;
      this.controlDrawingTrackingSetup.Location = new System.Drawing.Point(3, 64);
      this.controlDrawingTrackingSetup.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
      this.controlDrawingTrackingSetup.Name = "controlDrawingTrackingSetup";
      this.controlDrawingTrackingSetup.Size = new System.Drawing.Size(271, 244);
      this.controlDrawingTrackingSetup.TabIndex = 2;
      // 
      // SidePanelTracking
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.Controls.Add(this.controlDrawingTrackingSetup);
      this.Controls.Add(this.controlDrawingName);
      this.DoubleBuffered = true;
      this.Name = "SidePanelTracking";
      this.Size = new System.Drawing.Size(275, 624);
      this.ResumeLayout(false);

        }

        #endregion
        private ControlDrawingName controlDrawingName;
        private ControlDrawingTrackingSetup controlDrawingTrackingSetup;
    }
}
