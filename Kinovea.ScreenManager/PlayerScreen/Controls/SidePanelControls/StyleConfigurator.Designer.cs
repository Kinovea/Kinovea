
namespace Kinovea.ScreenManager
{
    partial class StyleConfigurator
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
      this.grpIdentifier = new System.Windows.Forms.GroupBox();
      this.tbName = new System.Windows.Forms.TextBox();
      this.grpConfig = new System.Windows.Forms.GroupBox();
      this.grpIdentifier.SuspendLayout();
      this.SuspendLayout();
      // 
      // grpIdentifier
      // 
      this.grpIdentifier.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpIdentifier.BackColor = System.Drawing.Color.White;
      this.grpIdentifier.Controls.Add(this.tbName);
      this.grpIdentifier.Location = new System.Drawing.Point(3, 3);
      this.grpIdentifier.Name = "grpIdentifier";
      this.grpIdentifier.Padding = new System.Windows.Forms.Padding(3, 3, 20, 3);
      this.grpIdentifier.Size = new System.Drawing.Size(265, 72);
      this.grpIdentifier.TabIndex = 36;
      this.grpIdentifier.TabStop = false;
      // 
      // tbName
      // 
      this.tbName.BackColor = System.Drawing.Color.WhiteSmoke;
      this.tbName.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.tbName.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
      this.tbName.Location = new System.Drawing.Point(16, 19);
      this.tbName.Name = "tbName";
      this.tbName.Size = new System.Drawing.Size(226, 18);
      this.tbName.TabIndex = 88;
      this.tbName.Text = "Name";
      this.tbName.TextChanged += new System.EventHandler(this.tbName_TextChanged);
      this.tbName.Enter += new System.EventHandler(this.tbName_Enter);
      this.tbName.Leave += new System.EventHandler(this.tbName_Leave);
      // 
      // grpConfig
      // 
      this.grpConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpConfig.BackColor = System.Drawing.Color.White;
      this.grpConfig.Location = new System.Drawing.Point(3, 81);
      this.grpConfig.Name = "grpConfig";
      this.grpConfig.Padding = new System.Windows.Forms.Padding(3, 3, 20, 3);
      this.grpConfig.Size = new System.Drawing.Size(265, 203);
      this.grpConfig.TabIndex = 35;
      this.grpConfig.TabStop = false;
      // 
      // StyleConfigurator
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.Controls.Add(this.grpIdentifier);
      this.Controls.Add(this.grpConfig);
      this.DoubleBuffered = true;
      this.ForeColor = System.Drawing.Color.Gray;
      this.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
      this.Name = "StyleConfigurator";
      this.Size = new System.Drawing.Size(271, 287);
      this.grpIdentifier.ResumeLayout(false);
      this.grpIdentifier.PerformLayout();
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpIdentifier;
        private System.Windows.Forms.GroupBox grpConfig;
        private System.Windows.Forms.TextBox tbName;
    }
}
