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
      this.grpScreenList = new System.Windows.Forms.GroupBox();
      this.pnlScreenList = new System.Windows.Forms.Panel();
      this.lblScreen2 = new System.Windows.Forms.Label();
      this.btnScreen2 = new System.Windows.Forms.Button();
      this.lblScreen1 = new System.Windows.Forms.Label();
      this.btnScreen1 = new System.Windows.Forms.Button();
      this.btnUseCurrent = new System.Windows.Forms.Button();
      this.rbContinue = new System.Windows.Forms.RadioButton();
      this.rbScreenList = new System.Windows.Forms.RadioButton();
      this.rbOpenExplorer = new System.Windows.Forms.RadioButton();
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.lblId = new System.Windows.Forms.Label();
      this.grpIdentifier.SuspendLayout();
      this.grpStartup.SuspendLayout();
      this.grpScreenList.SuspendLayout();
      this.pnlScreenList.SuspendLayout();
      this.SuspendLayout();
      // 
      // grpIdentifier
      // 
      this.grpIdentifier.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpIdentifier.Controls.Add(this.lblId);
      this.grpIdentifier.Controls.Add(this.tbName);
      this.grpIdentifier.Location = new System.Drawing.Point(12, 12);
      this.grpIdentifier.Name = "grpIdentifier";
      this.grpIdentifier.Padding = new System.Windows.Forms.Padding(3, 3, 20, 3);
      this.grpIdentifier.Size = new System.Drawing.Size(454, 71);
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
      this.tbName.Size = new System.Drawing.Size(409, 26);
      this.tbName.TabIndex = 39;
      this.tbName.TextChanged += new System.EventHandler(this.tbName_TextChanged);
      this.tbName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbName_KeyDown);
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
      this.grpStartup.Size = new System.Drawing.Size(454, 268);
      this.grpStartup.TabIndex = 63;
      this.grpStartup.TabStop = false;
      this.grpStartup.Text = "On startup";
      // 
      // grpScreenList
      // 
      this.grpScreenList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpScreenList.Controls.Add(this.pnlScreenList);
      this.grpScreenList.Controls.Add(this.btnUseCurrent);
      this.grpScreenList.Location = new System.Drawing.Point(36, 106);
      this.grpScreenList.Name = "grpScreenList";
      this.grpScreenList.Padding = new System.Windows.Forms.Padding(3, 3, 20, 3);
      this.grpScreenList.Size = new System.Drawing.Size(395, 142);
      this.grpScreenList.TabIndex = 64;
      this.grpScreenList.TabStop = false;
      this.grpScreenList.Text = "Screen list";
      // 
      // pnlScreenList
      // 
      this.pnlScreenList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pnlScreenList.BackColor = System.Drawing.Color.WhiteSmoke;
      this.pnlScreenList.Controls.Add(this.lblScreen2);
      this.pnlScreenList.Controls.Add(this.btnScreen2);
      this.pnlScreenList.Controls.Add(this.lblScreen1);
      this.pnlScreenList.Controls.Add(this.btnScreen1);
      this.pnlScreenList.Location = new System.Drawing.Point(13, 19);
      this.pnlScreenList.Name = "pnlScreenList";
      this.pnlScreenList.Size = new System.Drawing.Size(366, 79);
      this.pnlScreenList.TabIndex = 66;
      // 
      // lblScreen2
      // 
      this.lblScreen2.AutoSize = true;
      this.lblScreen2.Location = new System.Drawing.Point(47, 52);
      this.lblScreen2.Name = "lblScreen2";
      this.lblScreen2.Size = new System.Drawing.Size(128, 13);
      this.lblScreen2.TabIndex = 70;
      this.lblScreen2.Text = "Playback: video file name";
      // 
      // btnScreen2
      // 
      this.btnScreen2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnScreen2.FlatAppearance.BorderSize = 0;
      this.btnScreen2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnScreen2.Image = global::Kinovea.Root.Properties.Resources.television;
      this.btnScreen2.Location = new System.Drawing.Point(14, 44);
      this.btnScreen2.Name = "btnScreen2";
      this.btnScreen2.Size = new System.Drawing.Size(25, 25);
      this.btnScreen2.TabIndex = 69;
      this.btnScreen2.UseVisualStyleBackColor = true;
      // 
      // lblScreen1
      // 
      this.lblScreen1.AutoSize = true;
      this.lblScreen1.Location = new System.Drawing.Point(47, 19);
      this.lblScreen1.Name = "lblScreen1";
      this.lblScreen1.Size = new System.Drawing.Size(109, 13);
      this.lblScreen1.TabIndex = 68;
      this.lblScreen1.Text = "Capture: camera alias";
      // 
      // btnScreen1
      // 
      this.btnScreen1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnScreen1.FlatAppearance.BorderSize = 0;
      this.btnScreen1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnScreen1.Image = global::Kinovea.Root.Properties.Resources.camera_video;
      this.btnScreen1.Location = new System.Drawing.Point(14, 12);
      this.btnScreen1.Name = "btnScreen1";
      this.btnScreen1.Size = new System.Drawing.Size(25, 25);
      this.btnScreen1.TabIndex = 67;
      this.btnScreen1.UseVisualStyleBackColor = true;
      // 
      // btnUseCurrent
      // 
      this.btnUseCurrent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnUseCurrent.Location = new System.Drawing.Point(13, 105);
      this.btnUseCurrent.Name = "btnUseCurrent";
      this.btnUseCurrent.Size = new System.Drawing.Size(179, 24);
      this.btnUseCurrent.TabIndex = 65;
      this.btnUseCurrent.Text = "Import current screens";
      this.btnUseCurrent.UseVisualStyleBackColor = true;
      this.btnUseCurrent.Click += new System.EventHandler(this.btnUseCurrent_Click);
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
      this.rbContinue.CheckedChanged += new System.EventHandler(this.rbStartupMode_CheckedChanged);
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
      this.rbScreenList.CheckedChanged += new System.EventHandler(this.rbStartupMode_CheckedChanged);
      // 
      // rbOpenExplorer
      // 
      this.rbOpenExplorer.AutoSize = true;
      this.rbOpenExplorer.Location = new System.Drawing.Point(22, 28);
      this.rbOpenExplorer.Name = "rbOpenExplorer";
      this.rbOpenExplorer.Size = new System.Drawing.Size(109, 17);
      this.rbOpenExplorer.TabIndex = 41;
      this.rbOpenExplorer.Text = "Open the file browser";
      this.rbOpenExplorer.UseVisualStyleBackColor = true;
      this.rbOpenExplorer.CheckedChanged += new System.EventHandler(this.rbStartupMode_CheckedChanged);
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(262, 382);
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
      this.btnCancel.Location = new System.Drawing.Point(367, 382);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(99, 24);
      this.btnCancel.TabIndex = 65;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // lblId
      // 
      this.lblId.AutoSize = true;
      this.lblId.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblId.ForeColor = System.Drawing.Color.Silver;
      this.lblId.Location = new System.Drawing.Point(376, 53);
      this.lblId.Name = "lblId";
      this.lblId.Size = new System.Drawing.Size(55, 13);
      this.lblId.TabIndex = 40;
      this.lblId.Text = "00000000";
      this.lblId.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // FormWindowProperties
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(478, 418);
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
      this.grpScreenList.ResumeLayout(false);
      this.pnlScreenList.ResumeLayout(false);
      this.pnlScreenList.PerformLayout();
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
        private System.Windows.Forms.Button btnUseCurrent;
        private System.Windows.Forms.Panel pnlScreenList;
        private System.Windows.Forms.Button btnScreen1;
        private System.Windows.Forms.Label lblScreen1;
        private System.Windows.Forms.Label lblScreen2;
        private System.Windows.Forms.Button btnScreen2;
        private System.Windows.Forms.Label lblId;
    }
}