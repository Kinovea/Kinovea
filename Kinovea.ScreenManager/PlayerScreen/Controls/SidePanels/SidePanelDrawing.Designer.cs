
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
      this.styleConfigurator1 = new Kinovea.ScreenManager.StyleConfigurator();
      this.SuspendLayout();
      // 
      // styleConfigurator1
      // 
      this.styleConfigurator1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.styleConfigurator1.BackColor = System.Drawing.Color.WhiteSmoke;
      this.styleConfigurator1.ForeColor = System.Drawing.Color.Gray;
      this.styleConfigurator1.Location = new System.Drawing.Point(3, 30);
      this.styleConfigurator1.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
      this.styleConfigurator1.Name = "styleConfigurator1";
      this.styleConfigurator1.Size = new System.Drawing.Size(269, 378);
      this.styleConfigurator1.TabIndex = 0;
      // 
      // SidePanelDrawing
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.Controls.Add(this.styleConfigurator1);
      this.DoubleBuffered = true;
      this.Name = "SidePanelDrawing";
      this.Size = new System.Drawing.Size(275, 595);
      this.ResumeLayout(false);

        }

        #endregion

        private StyleConfigurator styleConfigurator1;
    }
}
