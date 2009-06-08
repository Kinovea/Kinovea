namespace Videa.Root
{
    partial class SupervisorUserInterface
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
            this.splitWorkSpace = new System.Windows.Forms.SplitContainer();
            this.buttonCloseExplo = new System.Windows.Forms.Button();
            this.splitWorkSpace.Panel1.SuspendLayout();
            this.splitWorkSpace.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitWorkSpace
            // 
            this.splitWorkSpace.BackColor = System.Drawing.Color.White;
            this.splitWorkSpace.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitWorkSpace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitWorkSpace.Location = new System.Drawing.Point(0, 0);
            this.splitWorkSpace.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.splitWorkSpace.Name = "splitWorkSpace";
            // 
            // splitWorkSpace.Panel1
            // 
            this.splitWorkSpace.Panel1.Controls.Add(this.buttonCloseExplo);
            this.splitWorkSpace.Panel1.Click += new System.EventHandler(this.splitWorkSpace_Panel1_Click);
            this.splitWorkSpace.Panel1MinSize = 4;
            this.splitWorkSpace.Size = new System.Drawing.Size(960, 560);
            this.splitWorkSpace.SplitterDistance = 200;
            this.splitWorkSpace.TabIndex = 0;
            this.splitWorkSpace.DoubleClick += new System.EventHandler(this._splitWorkSpace_DoubleClick);
            this.splitWorkSpace.MouseMove += new System.Windows.Forms.MouseEventHandler(this._splitWorkSpace_MouseMove);
            this.splitWorkSpace.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer1_SplitterMoved);
            // 
            // buttonCloseExplo
            // 
            this.buttonCloseExplo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCloseExplo.BackColor = System.Drawing.Color.Transparent;
            this.buttonCloseExplo.Cursor = System.Windows.Forms.Cursors.Default;
            this.buttonCloseExplo.FlatAppearance.BorderSize = 0;
            this.buttonCloseExplo.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.buttonCloseExplo.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.buttonCloseExplo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCloseExplo.Image = global::Videa.Root.Properties.Resources.closegrey;
            this.buttonCloseExplo.Location = new System.Drawing.Point(176, -1);
            this.buttonCloseExplo.Name = "buttonCloseExplo";
            this.buttonCloseExplo.Size = new System.Drawing.Size(20, 20);
            this.buttonCloseExplo.TabIndex = 1;
            this.buttonCloseExplo.UseVisualStyleBackColor = false;
            this.buttonCloseExplo.Click += new System.EventHandler(this.buttonCloseExplo_Click);
            // 
            // SupervisorUserInterface
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.Controls.Add(this.splitWorkSpace);
            this.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.MinimumSize = new System.Drawing.Size(960, 560);
            this.Name = "SupervisorUserInterface";
            this.Size = new System.Drawing.Size(960, 560);
            this.Load += new System.EventHandler(this.SupervisorUserInterface_Load);
            this.splitWorkSpace.Panel1.ResumeLayout(false);
            this.splitWorkSpace.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion


        //Sous modules
        public System.Windows.Forms.SplitContainer splitWorkSpace;
        public System.Windows.Forms.Button buttonCloseExplo;

    }
}
