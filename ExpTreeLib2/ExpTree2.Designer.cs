namespace Kinovea.ExpTreeLib2
{
    partial class ExpTree2
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
      this.tv1 = new System.Windows.Forms.TreeView();
      this.SuspendLayout();
      // 
      // tv1
      // 
      this.tv1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tv1.Location = new System.Drawing.Point(13, 13);
      this.tv1.Name = "tv1";
      this.tv1.Size = new System.Drawing.Size(280, 423);
      this.tv1.TabIndex = 0;
      // 
      // ExpTree2
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tv1);
      this.Name = "ExpTree2";
      this.Size = new System.Drawing.Size(306, 450);
      this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.TreeView tv1;
    }
}
