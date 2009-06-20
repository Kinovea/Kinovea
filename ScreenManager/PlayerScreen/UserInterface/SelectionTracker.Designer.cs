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
            this.BumperLeft = new System.Windows.Forms.Label();
            this.EndOfTrackLeft = new System.Windows.Forms.Label();
            this.HandlerRight = new System.Windows.Forms.Label();
            this.HandlerLeft = new System.Windows.Forms.Label();
            this.SelectedZone = new System.Windows.Forms.Panel();
            this.BumperRight = new System.Windows.Forms.Label();
            this.EndOfTrackRight = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // BumperLeft
            // 
            this.BumperLeft.BackColor = System.Drawing.Color.White;
            this.BumperLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.BumperLeft.Location = new System.Drawing.Point(0, 0);
            this.BumperLeft.Name = "BumperLeft";
            this.BumperLeft.Size = new System.Drawing.Size(10, 20);
            this.BumperLeft.TabIndex = 6;
            this.BumperLeft.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.BumperLeft_MouseDoubleClick);
            // 
            // EndOfTrackLeft
            // 
            this.EndOfTrackLeft.Image = global::Kinovea.ScreenManager.Properties.Resources.liqbumperleft;
            this.EndOfTrackLeft.Location = new System.Drawing.Point(10, 0);
            this.EndOfTrackLeft.Name = "EndOfTrackLeft";
            this.EndOfTrackLeft.Size = new System.Drawing.Size(12, 20);
            this.EndOfTrackLeft.TabIndex = 13;
            this.EndOfTrackLeft.DoubleClick += new System.EventHandler(this.EndOfTrackLeft_DoubleClick);
            // 
            // HandlerRight
            // 
            this.HandlerRight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.HandlerRight.BackColor = System.Drawing.Color.Transparent;
            this.HandlerRight.Cursor = System.Windows.Forms.Cursors.Hand;
            this.HandlerRight.Image = global::Kinovea.ScreenManager.Properties.Resources.liqhandlerright3;
            this.HandlerRight.Location = new System.Drawing.Point(244, 0);
            this.HandlerRight.Name = "HandlerRight";
            this.HandlerRight.Size = new System.Drawing.Size(14, 20);
            this.HandlerRight.TabIndex = 12;
            this.HandlerRight.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HandlerRight_MouseMove);
            this.HandlerRight.MouseUp += new System.Windows.Forms.MouseEventHandler(this.HandlerRight_MouseUp);
            // 
            // HandlerLeft
            // 
            this.HandlerLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.HandlerLeft.BackColor = System.Drawing.Color.Transparent;
            this.HandlerLeft.Cursor = System.Windows.Forms.Cursors.Hand;
            this.HandlerLeft.Image = global::Kinovea.ScreenManager.Properties.Resources.liqhandlerleft2;
            this.HandlerLeft.Location = new System.Drawing.Point(61, 0);
            this.HandlerLeft.Name = "HandlerLeft";
            this.HandlerLeft.Size = new System.Drawing.Size(14, 20);
            this.HandlerLeft.TabIndex = 11;
            this.HandlerLeft.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HandlerLeft_MouseMove);
            this.HandlerLeft.MouseUp += new System.Windows.Forms.MouseEventHandler(this.HandlerLeft_MouseUp);
            // 
            // SelectedZone
            // 
            this.SelectedZone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.SelectedZone.BackColor = System.Drawing.Color.White;
            this.SelectedZone.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.liqmiddlebar;
            this.SelectedZone.Location = new System.Drawing.Point(70, 0);
            this.SelectedZone.Name = "SelectedZone";
            this.SelectedZone.Size = new System.Drawing.Size(174, 20);
            this.SelectedZone.TabIndex = 10;
            this.SelectedZone.DoubleClick += new System.EventHandler(this.SelectedZone_DoubleClick);
            this.SelectedZone.MouseClick += new System.Windows.Forms.MouseEventHandler(this.SelectedZone_MouseClick);
            this.SelectedZone.Paint += new System.Windows.Forms.PaintEventHandler(this.SelectedZone_Paint);
            // 
            // BumperRight
            // 
            this.BumperRight.BackColor = System.Drawing.Color.White;
            this.BumperRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.BumperRight.Location = new System.Drawing.Point(305, 0);
            this.BumperRight.Name = "BumperRight";
            this.BumperRight.Size = new System.Drawing.Size(10, 20);
            this.BumperRight.TabIndex = 7;
            this.BumperRight.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.BumperRight_MouseDoubleClick);
            // 
            // EndOfTrackRight
            // 
            this.EndOfTrackRight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.EndOfTrackRight.BackColor = System.Drawing.Color.White;
            this.EndOfTrackRight.Image = global::Kinovea.ScreenManager.Properties.Resources.liqbumperright;
            this.EndOfTrackRight.Location = new System.Drawing.Point(295, 0);
            this.EndOfTrackRight.Name = "EndOfTrackRight";
            this.EndOfTrackRight.Size = new System.Drawing.Size(12, 20);
            this.EndOfTrackRight.TabIndex = 14;
            this.EndOfTrackRight.DoubleClick += new System.EventHandler(this.EndOfTrackRight_DoubleClick);
            // 
            // SelectionTracker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.liqbackdock;
            this.Controls.Add(this.HandlerRight);
            this.Controls.Add(this.HandlerLeft);
            this.Controls.Add(this.SelectedZone);
            this.Controls.Add(this.BumperRight);
            this.Controls.Add(this.BumperLeft);
            this.Controls.Add(this.EndOfTrackLeft);
            this.Controls.Add(this.EndOfTrackRight);
            this.Name = "SelectionTracker";
            this.Size = new System.Drawing.Size(315, 20);
            this.DoubleClick += new System.EventHandler(this.SelectionTracker_DoubleClick);
            this.Resize += new System.EventHandler(this.SelectionTracker_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label BumperLeft;
        private System.Windows.Forms.Label BumperRight;
        private System.Windows.Forms.Label HandlerRight;
        private System.Windows.Forms.Label HandlerLeft;
        private System.Windows.Forms.Panel SelectedZone;
        private System.Windows.Forms.ToolTip toolTips;
        private System.Windows.Forms.Label EndOfTrackLeft;
        private System.Windows.Forms.Label EndOfTrackRight;
    }
}
