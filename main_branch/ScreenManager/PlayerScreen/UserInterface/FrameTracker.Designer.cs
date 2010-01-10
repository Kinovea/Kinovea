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
            this.BumperLeft = new System.Windows.Forms.Label();
            this.BumperRight = new System.Windows.Forms.Label();
            this.NavCursor = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // BumperLeft
            // 
            this.BumperLeft.BackColor = System.Drawing.Color.White;
            this.BumperLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.BumperLeft.Location = new System.Drawing.Point(0, 0);
            this.BumperLeft.Name = "BumperLeft";
            this.BumperLeft.Size = new System.Drawing.Size(10, 20);
            this.BumperLeft.TabIndex = 7;
            // 
            // BumperRight
            // 
            this.BumperRight.BackColor = System.Drawing.Color.White;
            this.BumperRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.BumperRight.Location = new System.Drawing.Point(330, 0);
            this.BumperRight.Name = "BumperRight";
            this.BumperRight.Size = new System.Drawing.Size(10, 20);
            this.BumperRight.TabIndex = 8;
            // 
            // NavCursor
            // 
            this.NavCursor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.NavCursor.BackColor = System.Drawing.Color.Transparent;
            this.NavCursor.Cursor = System.Windows.Forms.Cursors.Hand;
            this.NavCursor.Image = global::Kinovea.ScreenManager.Properties.Resources.liqcursor;
            this.NavCursor.Location = new System.Drawing.Point(160, 0);
            this.NavCursor.Name = "NavCursor";
            this.NavCursor.Size = new System.Drawing.Size(26, 20);
            this.NavCursor.TabIndex = 10;
            this.NavCursor.MouseMove += new System.Windows.Forms.MouseEventHandler(this.NavCursor_MouseMove);
            this.NavCursor.MouseUp += new System.Windows.Forms.MouseEventHandler(this.NavCursor_MouseUp);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Image = global::Kinovea.ScreenManager.Properties.Resources.liqbumperright;
            this.label1.Location = new System.Drawing.Point(320, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(10, 20);
            this.label1.TabIndex = 11;
            // 
            // label2
            // 
            this.label2.Image = global::Kinovea.ScreenManager.Properties.Resources.liqbumperleft;
            this.label2.Location = new System.Drawing.Point(10, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(10, 20);
            this.label2.TabIndex = 12;
            // 
            // FrameTracker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.liqbackdock;
            this.Controls.Add(this.NavCursor);
            this.Controls.Add(this.BumperRight);
            this.Controls.Add(this.BumperLeft);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.MinimumSize = new System.Drawing.Size(50, 20);
            this.Name = "FrameTracker";
            this.Size = new System.Drawing.Size(340, 20);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FrameTracker_Paint);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.FrameTracker_MouseClick);
            this.Resize += new System.EventHandler(this.FrameTracker_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label BumperLeft;
        private System.Windows.Forms.Label BumperRight;
        private System.Windows.Forms.Label NavCursor;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}
