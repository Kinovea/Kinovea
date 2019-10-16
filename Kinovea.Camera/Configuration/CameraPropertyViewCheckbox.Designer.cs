namespace Kinovea.Camera
{
    partial class CameraPropertyViewCheckbox
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
      this.cb = new System.Windows.Forms.CheckBox();
      this.SuspendLayout();
      // 
      // cb
      // 
      this.cb.AutoSize = true;
      this.cb.Location = new System.Drawing.Point(5, 8);
      this.cb.Name = "cb";
      this.cb.Size = new System.Drawing.Size(15, 14);
      this.cb.TabIndex = 108;
      this.cb.UseVisualStyleBackColor = true;
      this.cb.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
      // 
      // CameraPropertyViewCheckbox
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.cb);
      this.Name = "CameraPropertyViewCheckbox";
      this.Size = new System.Drawing.Size(400, 32);
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cb;
    }
}
