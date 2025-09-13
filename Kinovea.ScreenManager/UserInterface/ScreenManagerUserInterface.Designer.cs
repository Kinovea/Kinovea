namespace Kinovea.ScreenManager
{
    partial class ScreenManagerUserInterface
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
      this.pnlScreens = new System.Windows.Forms.Panel();
      this.splitScreensPanel = new System.Windows.Forms.SplitContainer();
      this.splitScreens = new System.Windows.Forms.SplitContainer();
      this.pnlScreens.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitScreensPanel)).BeginInit();
      this.splitScreensPanel.Panel1.SuspendLayout();
      this.splitScreensPanel.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitScreens)).BeginInit();
      this.splitScreens.SuspendLayout();
      this.SuspendLayout();
      // 
      // pnlScreens
      // 
      this.pnlScreens.Controls.Add(this.splitScreensPanel);
      this.pnlScreens.Location = new System.Drawing.Point(14, 34);
      this.pnlScreens.Margin = new System.Windows.Forms.Padding(1);
      this.pnlScreens.Name = "pnlScreens";
      this.pnlScreens.Size = new System.Drawing.Size(574, 367);
      this.pnlScreens.TabIndex = 2;
      this.pnlScreens.Visible = false;
      this.pnlScreens.Resize += new System.EventHandler(this.pnlScreens_Resize);
      // 
      // splitScreensPanel
      // 
      this.splitScreensPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.splitScreensPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitScreensPanel.IsSplitterFixed = true;
      this.splitScreensPanel.Location = new System.Drawing.Point(0, 0);
      this.splitScreensPanel.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
      this.splitScreensPanel.Name = "splitScreensPanel";
      this.splitScreensPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitScreensPanel.Panel1
      // 
      this.splitScreensPanel.Panel1.Controls.Add(this.splitScreens);
      // 
      // splitScreensPanel.Panel2
      // 
      this.splitScreensPanel.Panel2.BackColor = System.Drawing.Color.White;
      this.splitScreensPanel.Size = new System.Drawing.Size(574, 367);
      this.splitScreensPanel.SplitterDistance = 315;
      this.splitScreensPanel.TabIndex = 0;
      // 
      // splitScreens
      // 
      this.splitScreens.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.splitScreens.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitScreens.Location = new System.Drawing.Point(0, 0);
      this.splitScreens.Name = "splitScreens";
      // 
      // splitScreens.Panel1
      // 
      this.splitScreens.Panel1.BackColor = System.Drawing.Color.White;
      this.splitScreens.Panel1.DragDrop += new System.Windows.Forms.DragEventHandler(this.splitScreens_Panel1_DragDrop);
      this.splitScreens.Panel1.DragOver += new System.Windows.Forms.DragEventHandler(this.DroppableArea_DragOver);
      // 
      // splitScreens.Panel2
      // 
      this.splitScreens.Panel2.BackColor = System.Drawing.Color.White;
      this.splitScreens.Panel2.DragDrop += new System.Windows.Forms.DragEventHandler(this.splitScreens_Panel2_DragDrop);
      this.splitScreens.Panel2.DragOver += new System.Windows.Forms.DragEventHandler(this.DroppableArea_DragOver);
      this.splitScreens.Size = new System.Drawing.Size(574, 315);
      this.splitScreens.SplitterDistance = 287;
      this.splitScreens.TabIndex = 0;
      // 
      // ScreenManagerUserInterface
      // 
      this.AllowDrop = true;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.WhiteSmoke;
      this.Controls.Add(this.pnlScreens);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "ScreenManagerUserInterface";
      this.Size = new System.Drawing.Size(720, 560);
      this.DragDrop += new System.Windows.Forms.DragEventHandler(this.ScreenManagerUserInterface_DragDrop);
      this.DragOver += new System.Windows.Forms.DragEventHandler(this.DroppableArea_DragOver);
      this.pnlScreens.ResumeLayout(false);
      this.splitScreensPanel.Panel1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitScreensPanel)).EndInit();
      this.splitScreensPanel.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitScreens)).EndInit();
      this.splitScreens.ResumeLayout(false);
      this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.SplitContainer splitScreensPanel;
        private System.Windows.Forms.SplitContainer splitScreens;
        private System.Windows.Forms.Panel pnlScreens;

    }
}
