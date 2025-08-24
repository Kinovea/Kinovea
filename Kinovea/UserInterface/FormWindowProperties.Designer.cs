namespace Kinovea.Root
{
    partial class FormWindowProperties
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
      this.grpIdentifier = new System.Windows.Forms.GroupBox();
      this.tbName = new System.Windows.Forms.TextBox();
      this.grpStartup = new System.Windows.Forms.GroupBox();
      this.rbContinue = new System.Windows.Forms.RadioButton();
      this.rbScreenList = new System.Windows.Forms.RadioButton();
      this.rbOpenExplorer = new System.Windows.Forms.RadioButton();
      this.grpScreenList = new System.Windows.Forms.GroupBox();
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.grpIdentifier.SuspendLayout();
      this.grpStartup.SuspendLayout();
      this.SuspendLayout();
      // 
      // grpIdentifier
      // 
      this.grpIdentifier.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpIdentifier.Controls.Add(this.tbName);
      this.grpIdentifier.Location = new System.Drawing.Point(12, 12);
      this.grpIdentifier.Name = "grpIdentifier";
      this.grpIdentifier.Padding = new System.Windows.Forms.Padding(3, 3, 20, 3);
      this.grpIdentifier.Size = new System.Drawing.Size(386, 71);
      this.grpIdentifier.TabIndex = 62;
      this.grpIdentifier.TabStop = false;
      this.grpIdentifier.Text = "Name";
      // 
      // tbName
      // 
      this.tbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbName.Location = new System.Drawing.Point(22, 24);
      this.tbName.Name = "tbName";
      this.tbName.Size = new System.Drawing.Size(341, 26);
      this.tbName.TabIndex = 39;
      this.tbName.TextChanged += new System.EventHandler(this.tbName_TextChanged);
      // 
      // grpStartup
      // 
      this.grpStartup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpStartup.Controls.Add(this.grpScreenList);
      this.grpStartup.Controls.Add(this.rbContinue);
      this.grpStartup.Controls.Add(this.rbScreenList);
      this.grpStartup.Controls.Add(this.rbOpenExplorer);
      this.grpStartup.Location = new System.Drawing.Point(12, 100);
      this.grpStartup.Name = "grpStartup";
      this.grpStartup.Padding = new System.Windows.Forms.Padding(3, 3, 20, 3);
      this.grpStartup.Size = new System.Drawing.Size(386, 241);
      this.grpStartup.TabIndex = 63;
      this.grpStartup.TabStop = false;
      this.grpStartup.Text = "On startup";
      // 
      // rbContinue
      // 
      this.rbContinue.AutoSize = true;
      this.rbContinue.Checked = true;
      this.rbContinue.Location = new System.Drawing.Point(22, 51);
      this.rbContinue.Name = "rbContinue";
      this.rbContinue.Size = new System.Drawing.Size(151, 17);
      this.rbContinue.TabIndex = 42;
      this.rbContinue.TabStop = true;
      this.rbContinue.Text = "Continue where you left off";
      this.rbContinue.UseVisualStyleBackColor = true;
      // 
      // rbScreenList
      // 
      this.rbScreenList.AutoSize = true;
      this.rbScreenList.Location = new System.Drawing.Point(22, 74);
      this.rbScreenList.Name = "rbScreenList";
      this.rbScreenList.Size = new System.Drawing.Size(129, 17);
      this.rbScreenList.TabIndex = 43;
      this.rbScreenList.TabStop = true;
      this.rbScreenList.Text = "Open specific content";
      this.rbScreenList.UseVisualStyleBackColor = true;
      // 
      // rbOpenExplorer
      // 
      this.rbOpenExplorer.AutoSize = true;
      this.rbOpenExplorer.Location = new System.Drawing.Point(22, 28);
      this.rbOpenExplorer.Name = "rbOpenExplorer";
      this.rbOpenExplorer.Size = new System.Drawing.Size(125, 17);
      this.rbOpenExplorer.TabIndex = 41;
      this.rbOpenExplorer.Text = "Open the file explorer";
      this.rbOpenExplorer.UseVisualStyleBackColor = true;
      this.rbOpenExplorer.CheckedChanged += new System.EventHandler(this.rbOpenExplorer_CheckedChanged);
      // 
      // grpScreenList
      // 
      this.grpScreenList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpScreenList.Location = new System.Drawing.Point(36, 106);
      this.grpScreenList.Name = "grpScreenList";
      this.grpScreenList.Padding = new System.Windows.Forms.Padding(3, 3, 20, 3);
      this.grpScreenList.Size = new System.Drawing.Size(327, 115);
      this.grpScreenList.TabIndex = 64;
      this.grpScreenList.TabStop = false;
      this.grpScreenList.Text = "Screen list";
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(194, 355);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(99, 24);
      this.btnOK.TabIndex = 64;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(299, 355);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(99, 24);
      this.btnCancel.TabIndex = 65;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // FormWindowProperties
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(410, 391);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.grpStartup);
      this.Controls.Add(this.grpIdentifier);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormWindowProperties";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.Text = "Active window properties";
      this.grpIdentifier.ResumeLayout(false);
      this.grpIdentifier.PerformLayout();
      this.grpStartup.ResumeLayout(false);
      this.grpStartup.PerformLayout();
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpIdentifier;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.GroupBox grpStartup;
        private System.Windows.Forms.RadioButton rbContinue;
        private System.Windows.Forms.RadioButton rbScreenList;
        private System.Windows.Forms.RadioButton rbOpenExplorer;
        private System.Windows.Forms.GroupBox grpScreenList;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}