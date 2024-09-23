
namespace Kinovea.ScreenManager
{
    partial class SidePanelDrawing
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
      this.controlDrawingStyle = new Kinovea.ScreenManager.ControlDrawingStyle();
      this.controlDrawingName = new Kinovea.ScreenManager.ControlDrawingName();
      this.SuspendLayout();
      // 
      // styleConfigurator1
      // 
      this.controlDrawingStyle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.controlDrawingStyle.BackColor = System.Drawing.Color.White;
      this.controlDrawingStyle.ForeColor = System.Drawing.Color.Gray;
      this.controlDrawingStyle.Location = new System.Drawing.Point(3, 64);
      this.controlDrawingStyle.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
      this.controlDrawingStyle.Name = "styleConfigurator1";
      this.controlDrawingStyle.Size = new System.Drawing.Size(269, 269);
      this.controlDrawingStyle.TabIndex = 0;
      // 
      // controlDrawingName1
      // 
      this.controlDrawingName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.controlDrawingName.BackColor = System.Drawing.Color.White;
      this.controlDrawingName.ForeColor = System.Drawing.Color.Gray;
      this.controlDrawingName.Location = new System.Drawing.Point(3, 5);
      this.controlDrawingName.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
      this.controlDrawingName.Name = "controlDrawingName1";
      this.controlDrawingName.Size = new System.Drawing.Size(269, 49);
      this.controlDrawingName.TabIndex = 1;
      // 
      // SidePanelDrawing
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.Controls.Add(this.controlDrawingName);
      this.Controls.Add(this.controlDrawingStyle);
      this.DoubleBuffered = true;
      this.Name = "SidePanelDrawing";
      this.Size = new System.Drawing.Size(275, 624);
      this.ResumeLayout(false);

        }

        #endregion

        private ControlDrawingStyle controlDrawingStyle;
        private ControlDrawingName controlDrawingName;
    }
}
