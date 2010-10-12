namespace Kinovea.ScreenManager
{
    partial class SpeedSlider
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
        	this.toolTips = new System.Windows.Forms.ToolTip(this.components);
        	this.SuspendLayout();
        	// 
        	// SpeedSlider
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
        	this.MinimumSize = new System.Drawing.Size(20, 10);
        	this.Name = "SpeedSlider";
        	this.Size = new System.Drawing.Size(200, 10);
        	this.Paint += new System.Windows.Forms.PaintEventHandler(this.SpeedSlider_Paint);
        	this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SpeedSlider_MouseMove);
        	this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SpeedSlider_MouseDown);
        	this.Resize += new System.EventHandler(this.SpeedSlider_Resize);
        	this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.SpeedSlider_MouseUp);
        	this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ToolTip toolTips;

        
    }
}
