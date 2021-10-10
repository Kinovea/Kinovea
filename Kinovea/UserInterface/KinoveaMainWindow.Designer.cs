namespace Kinovea.Root
{
    partial class KinoveaMainWindow
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
        
        

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KinoveaMainWindow));
      this.menuStrip = new System.Windows.Forms.MenuStrip();
      this.statusStrip = new System.Windows.Forms.StatusStrip();
      this.toolStrip = new System.Windows.Forms.ToolStrip();
      this.SuspendLayout();
      // 
      // menuStrip
      // 
      this.menuStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(199)))), ((int)(((byte)(216)))));
      resources.ApplyResources(this.menuStrip, "menuStrip");
      this.menuStrip.Name = "menuStrip";
      // 
      // statusStrip
      // 
      this.statusStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(57)))), ((int)(((byte)(85)))));
      resources.ApplyResources(this.statusStrip, "statusStrip");
      this.statusStrip.Name = "statusStrip";
      // 
      // toolStrip
      // 
      this.toolStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(227)))), ((int)(((byte)(237)))));
      resources.ApplyResources(this.toolStrip, "toolStrip");
      this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
      this.toolStrip.Name = "toolStrip";
      // 
      // KinoveaMainWindow
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.Controls.Add(this.toolStrip);
      this.Controls.Add(this.menuStrip);
      this.Controls.Add(this.statusStrip);
      this.IsMdiContainer = true;
      this.KeyPreview = true;
      this.MainMenuStrip = this.menuStrip;
      this.Name = "KinoveaMainWindow";
      this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UserInterface_FormClosing);
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.MenuStrip menuStrip;
        public System.Windows.Forms.StatusStrip statusStrip;
        public System.Windows.Forms.ToolStrip toolStrip;
        
    }
}

