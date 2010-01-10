namespace Kinovea.ScreenManager
{
    partial class formConfigureGrids
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
            this.cmbDivisions = new System.Windows.Forms.ComboBox();
            this.lblDivisions = new System.Windows.Forms.Label();
            this.grpConfig.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(10, 234);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(99, 24);
            this.btnOK.TabIndex = 15;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(115, 234);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(99, 24);
            this.btnCancel.TabIndex = 20;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // grpConfig
            // 
            this.grpConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpConfig.Controls.Add(this.cmbDivisions);
            this.grpConfig.Controls.Add(this.lblDivisions);
            this.grpConfig.Location = new System.Drawing.Point(12, 12);
            this.grpConfig.Name = "grpConfig";
            this.grpConfig.Size = new System.Drawing.Size(202, 202);
            this.grpConfig.TabIndex = 33;
            this.grpConfig.TabStop = false;
            this.grpConfig.Text = "Configuration";
            // 
            // cmbDivisions
            // 
            this.cmbDivisions.FormattingEnabled = true;
            this.cmbDivisions.Items.AddRange(new object[] {
            "2",
            "3",
            "4",
            "5",
            "6",
            "8",
            "10",
            "12",
            "16",
            "20"});
            this.cmbDivisions.Location = new System.Drawing.Point(141, 159);
            this.cmbDivisions.Name = "cmbDivisions";
            this.cmbDivisions.Size = new System.Drawing.Size(41, 21);
            this.cmbDivisions.TabIndex = 10;
            this.cmbDivisions.Text = "8";
            this.cmbDivisions.SelectedValueChanged += new System.EventHandler(this.cmbDivisions_SelectedValueChanged);
            // 
            // lblDivisions
            // 
            this.lblDivisions.AutoSize = true;
            this.lblDivisions.Location = new System.Drawing.Point(19, 162);
            this.lblDivisions.Name = "lblDivisions";
            this.lblDivisions.Size = new System.Drawing.Size(105, 13);
            this.lblDivisions.TabIndex = 40;
            this.lblDivisions.Text = "Number of divisions :";
            // 
            // formConfigureGrids
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(226, 270);
            this.Controls.Add(this.grpConfig);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "formConfigureGrids";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "   Configure Grid";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formConfigureGrids_FormClosing);
            this.grpConfig.ResumeLayout(false);
            this.grpConfig.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpConfig;
        private System.Windows.Forms.ComboBox cmbDivisions;
        private System.Windows.Forms.Label lblDivisions;
    }
}