namespace Kinovea.ScreenManager
{
    partial class FormTimeSections
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
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.grpConfig = new System.Windows.Forms.GroupBox();
      this.btnIndicator = new System.Windows.Forms.Button();
      this.grpConfig.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnOK
      // 
      this.btnOK.Anchor = System.Windows.Forms.AnchorStyles.Top;
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(275, 282);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(99, 24);
      this.btnOK.TabIndex = 33;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Top;
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(381, 282);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(99, 24);
      this.btnCancel.TabIndex = 34;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // grpConfig
      // 
      this.grpConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpConfig.Controls.Add(this.btnIndicator);
      this.grpConfig.Location = new System.Drawing.Point(12, 12);
      this.grpConfig.Name = "grpConfig";
      this.grpConfig.Padding = new System.Windows.Forms.Padding(3, 3, 20, 3);
      this.grpConfig.Size = new System.Drawing.Size(468, 264);
      this.grpConfig.TabIndex = 35;
      this.grpConfig.TabStop = false;
      // 
      // btnIndicator
      // 
      this.btnIndicator.FlatAppearance.BorderSize = 0;
      this.btnIndicator.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnIndicator.Image = global::Kinovea.ScreenManager.Properties.Resources.arrow_medium;
      this.btnIndicator.Location = new System.Drawing.Point(30, 43);
      this.btnIndicator.Name = "btnIndicator";
      this.btnIndicator.Size = new System.Drawing.Size(19, 23);
      this.btnIndicator.TabIndex = 24;
      this.btnIndicator.UseVisualStyleBackColor = true;
      // 
      // FormTimeSections
      // 
      this.AcceptButton = this.btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(492, 317);
      this.Controls.Add(this.grpConfig);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnCancel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormTimeSections";
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "FormTimeSections";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormTimeSections_FormClosing);
      this.grpConfig.ResumeLayout(false);
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpConfig;
        private System.Windows.Forms.Button btnIndicator;
    }
}