
namespace Kinovea.ScreenManager
{
    partial class ControlDrawingName
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
      this.tbName = new System.Windows.Forms.TextBox();
      this.button1 = new System.Windows.Forms.Button();
      this.panel1 = new System.Windows.Forms.Panel();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tbName
      // 
      this.tbName.BackColor = System.Drawing.Color.WhiteSmoke;
      this.tbName.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.tbName.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
      this.tbName.Location = new System.Drawing.Point(42, 12);
      this.tbName.Name = "tbName";
      this.tbName.Size = new System.Drawing.Size(59, 18);
      this.tbName.TabIndex = 88;
      this.tbName.Text = "Name";
      this.tbName.TextChanged += new System.EventHandler(this.tbName_TextChanged);
      this.tbName.Enter += new System.EventHandler(this.tbName_Enter);
      this.tbName.Leave += new System.EventHandler(this.tbName_Leave);
      // 
      // button1
      // 
      this.button1.BackColor = System.Drawing.Color.Transparent;
      this.button1.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.keyframe_id;
      this.button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.button1.Cursor = System.Windows.Forms.Cursors.Default;
      this.button1.FlatAppearance.BorderSize = 0;
      this.button1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.button1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.button1.Location = new System.Drawing.Point(10, 11);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(20, 20);
      this.button1.TabIndex = 99;
      this.button1.UseVisualStyleBackColor = false;
      // 
      // panel1
      // 
      this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.panel1.BackColor = System.Drawing.Color.WhiteSmoke;
      this.panel1.Controls.Add(this.button1);
      this.panel1.Controls.Add(this.tbName);
      this.panel1.Location = new System.Drawing.Point(4, 4);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(263, 40);
      this.panel1.TabIndex = 94;
      // 
      // ControlDrawingName
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.Controls.Add(this.panel1);
      this.DoubleBuffered = true;
      this.ForeColor = System.Drawing.Color.Gray;
      this.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
      this.Name = "ControlDrawingName";
      this.Size = new System.Drawing.Size(271, 49);
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Panel panel1;
    }
}
