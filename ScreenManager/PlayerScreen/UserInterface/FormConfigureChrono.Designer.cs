namespace Kinovea.ScreenManager
{
    partial class formConfigureChrono
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
            this.grpConfig = new System.Windows.Forms.GroupBox();
            this.chkShowLabel = new System.Windows.Forms.CheckBox();
            this.tbLabel = new System.Windows.Forms.TextBox();
            this.cmbFontSize = new System.Windows.Forms.ComboBox();
            this.lblLabel = new System.Windows.Forms.Label();
            this.lblFontSize = new System.Windows.Forms.Label();
            this.lblColor = new System.Windows.Forms.Label();
            this.btnChronoColor = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.grpConfig.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpConfig
            // 
            this.grpConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpConfig.Controls.Add(this.chkShowLabel);
            this.grpConfig.Controls.Add(this.tbLabel);
            this.grpConfig.Controls.Add(this.cmbFontSize);
            this.grpConfig.Controls.Add(this.lblLabel);
            this.grpConfig.Controls.Add(this.lblFontSize);
            this.grpConfig.Controls.Add(this.lblColor);
            this.grpConfig.Controls.Add(this.btnChronoColor);
            this.grpConfig.Location = new System.Drawing.Point(12, 12);
            this.grpConfig.Name = "grpConfig";
            this.grpConfig.Size = new System.Drawing.Size(228, 201);
            this.grpConfig.TabIndex = 30;
            this.grpConfig.TabStop = false;
            this.grpConfig.Text = "Configuration";
            // 
            // chkShowLabel
            // 
            this.chkShowLabel.AutoSize = true;
            this.chkShowLabel.Location = new System.Drawing.Point(21, 162);
            this.chkShowLabel.Name = "chkShowLabel";
            this.chkShowLabel.Size = new System.Drawing.Size(118, 17);
            this.chkShowLabel.TabIndex = 20;
            this.chkShowLabel.Text = "Always Show Label";
            this.chkShowLabel.UseVisualStyleBackColor = true;
            this.chkShowLabel.CheckedChanged += new System.EventHandler(this.chkShowLabel_CheckedChanged);
            // 
            // tbLabel
            // 
            this.tbLabel.Location = new System.Drawing.Point(97, 116);
            this.tbLabel.Name = "tbLabel";
            this.tbLabel.Size = new System.Drawing.Size(111, 20);
            this.tbLabel.TabIndex = 15;
            this.tbLabel.TextChanged += new System.EventHandler(this.tbLabel_TextChanged);
            // 
            // cmbFontSize
            // 
            this.cmbFontSize.FormattingEnabled = true;
            this.cmbFontSize.Items.AddRange(new object[] {
            "8",
            "9",
            "10",
            "11",
            "12",
            "14",
            "16",
            "18",
            "20",
            "24",
            "28",
            "32",
            "36"});
            this.cmbFontSize.Location = new System.Drawing.Point(168, 71);
            this.cmbFontSize.Name = "cmbFontSize";
            this.cmbFontSize.Size = new System.Drawing.Size(41, 21);
            this.cmbFontSize.TabIndex = 10;
            this.cmbFontSize.Text = "12";
            this.cmbFontSize.SelectedValueChanged += new System.EventHandler(this.cmbFontSize_SelectedValueChanged);
            // 
            // lblLabel
            // 
            this.lblLabel.AutoSize = true;
            this.lblLabel.Location = new System.Drawing.Point(18, 120);
            this.lblLabel.Name = "lblLabel";
            this.lblLabel.Size = new System.Drawing.Size(39, 13);
            this.lblLabel.TabIndex = 38;
            this.lblLabel.Text = "Label :";
            // 
            // lblFontSize
            // 
            this.lblFontSize.AutoSize = true;
            this.lblFontSize.Location = new System.Drawing.Point(18, 79);
            this.lblFontSize.Name = "lblFontSize";
            this.lblFontSize.Size = new System.Drawing.Size(55, 13);
            this.lblFontSize.TabIndex = 37;
            this.lblFontSize.Text = "Font size :";
            // 
            // lblColor
            // 
            this.lblColor.AutoSize = true;
            this.lblColor.Location = new System.Drawing.Point(18, 39);
            this.lblColor.Name = "lblColor";
            this.lblColor.Size = new System.Drawing.Size(37, 13);
            this.lblColor.TabIndex = 36;
            this.lblColor.Text = "Color :";
            // 
            // btnChronoColor
            // 
            this.btnChronoColor.BackColor = System.Drawing.Color.Black;
            this.btnChronoColor.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnChronoColor.FlatAppearance.BorderSize = 0;
            this.btnChronoColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnChronoColor.Location = new System.Drawing.Point(97, 30);
            this.btnChronoColor.Name = "btnChronoColor";
            this.btnChronoColor.Size = new System.Drawing.Size(112, 25);
            this.btnChronoColor.TabIndex = 5;
            this.btnChronoColor.UseVisualStyleBackColor = false;
            this.btnChronoColor.Click += new System.EventHandler(this.btnChronoColor_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(36, 230);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(99, 24);
            this.btnOK.TabIndex = 25;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(141, 230);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(99, 24);
            this.btnCancel.TabIndex = 30;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // formConfigureChrono
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(252, 266);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.grpConfig);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formConfigureChrono";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "FormConfigureChrono";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formConfigureChrono_FormClosing);
            this.grpConfig.ResumeLayout(false);
            this.grpConfig.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpConfig;
        private System.Windows.Forms.Label lblColor;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblLabel;
        private System.Windows.Forms.Label lblFontSize;
        private System.Windows.Forms.TextBox tbLabel;
        private System.Windows.Forms.ComboBox cmbFontSize;
        public System.Windows.Forms.Button btnChronoColor;
        private System.Windows.Forms.CheckBox chkShowLabel;
    }
}