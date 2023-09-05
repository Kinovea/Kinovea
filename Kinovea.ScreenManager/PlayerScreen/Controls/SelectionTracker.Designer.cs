namespace Kinovea.ScreenManager
{
    partial class SelectionTracker
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
        	// SelectionTracker
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.Margin = new System.Windows.Forms.Padding(4);
        	this.Name = "SelectionTracker";
        	this.Size = new System.Drawing.Size(420, 25);
        	this.Paint += new System.Windows.Forms.PaintEventHandler(this.SelectionTracker_Paint);
        	this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SelectionTracker_MouseMove);
        	this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SelectionTracker_MouseDown);
        	this.Resize += new System.EventHandler(this.SelectionTracker_Resize);
        	this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.SelectionTracker_MouseUp);
        	this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ToolTip toolTips;
    }
}
