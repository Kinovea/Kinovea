namespace Kinovea.ScreenManager
{
    partial class FrameTracker
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
        	this.SuspendLayout();
        	// 
        	// FrameTracker
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.MinimumSize = new System.Drawing.Size(50, 20);
        	this.Name = "FrameTracker";
        	this.Size = new System.Drawing.Size(340, 20);
        	this.Paint += new System.Windows.Forms.PaintEventHandler(this.FrameTracker_Paint);
        	this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.FrameTracker_MouseMove);
        	this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FrameTracker_MouseMove);
        	this.Resize += new System.EventHandler(this.FrameTracker_Resize);
        	this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FrameTracker_MouseUp);
        	this.ResumeLayout(false);
        }

        #endregion

    }
}
