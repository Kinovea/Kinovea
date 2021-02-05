namespace Kinovea.ScreenManager
{
    partial class formVideoExport
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
      this.groupSaveMethod = new System.Windows.Forms.GroupBox();
      this.lblVideoExport = new System.Windows.Forms.Label();
      this.checkSlowMotion = new System.Windows.Forms.CheckBox();
      this.tbSaveBlended = new System.Windows.Forms.TextBox();
      this.btnSaveBlended = new System.Windows.Forms.Button();
      this.groupSaveMethod.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.Location = new System.Drawing.Point(313, 207);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(99, 24);
      this.btnOK.TabIndex = 35;
      this.btnOK.Text = "Generic_Save";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(418, 207);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(99, 24);
      this.btnCancel.TabIndex = 40;
      this.btnCancel.Text = "Generic_Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // groupSaveMethod
      // 
      this.groupSaveMethod.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupSaveMethod.Controls.Add(this.lblVideoExport);
      this.groupSaveMethod.Controls.Add(this.checkSlowMotion);
      this.groupSaveMethod.Controls.Add(this.tbSaveBlended);
      this.groupSaveMethod.Controls.Add(this.btnSaveBlended);
      this.groupSaveMethod.Location = new System.Drawing.Point(12, 12);
      this.groupSaveMethod.Name = "groupSaveMethod";
      this.groupSaveMethod.Size = new System.Drawing.Size(505, 181);
      this.groupSaveMethod.TabIndex = 25;
      this.groupSaveMethod.TabStop = false;
      // 
      // lblVideoExport
      // 
      this.lblVideoExport.AutoSize = true;
      this.lblVideoExport.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblVideoExport.Location = new System.Drawing.Point(81, 39);
      this.lblVideoExport.Name = "lblVideoExport";
      this.lblVideoExport.Size = new System.Drawing.Size(74, 16);
      this.lblVideoExport.TabIndex = 27;
      this.lblVideoExport.Text = "description";
      // 
      // checkSlowMotion
      // 
      this.checkSlowMotion.AutoSize = true;
      this.checkSlowMotion.Location = new System.Drawing.Point(84, 136);
      this.checkSlowMotion.Name = "checkSlowMotion";
      this.checkSlowMotion.Size = new System.Drawing.Size(201, 17);
      this.checkSlowMotion.TabIndex = 25;
      this.checkSlowMotion.Text = "dlgSaveAnalysisOrVideo_CheckSlow";
      this.checkSlowMotion.UseVisualStyleBackColor = true;
      // 
      // tbSaveBlended
      // 
      this.tbSaveBlended.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbSaveBlended.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.tbSaveBlended.ForeColor = System.Drawing.Color.DimGray;
      this.tbSaveBlended.Location = new System.Drawing.Point(84, 76);
      this.tbSaveBlended.Multiline = true;
      this.tbSaveBlended.Name = "tbSaveBlended";
      this.tbSaveBlended.Size = new System.Drawing.Size(407, 41);
      this.tbSaveBlended.TabIndex = 26;
      this.tbSaveBlended.Text = "Drawings and tools will be visible everywhere but not modifiable.\r\nExtra comments" +
    " attached to key images will not be saved.";
      // 
      // btnSaveBlended
      // 
      this.btnSaveBlended.BackColor = System.Drawing.Color.Transparent;
      this.btnSaveBlended.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.saveblended;
      this.btnSaveBlended.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.btnSaveBlended.FlatAppearance.BorderColor = System.Drawing.Color.Black;
      this.btnSaveBlended.FlatAppearance.BorderSize = 0;
      this.btnSaveBlended.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnSaveBlended.Location = new System.Drawing.Point(17, 31);
      this.btnSaveBlended.Name = "btnSaveBlended";
      this.btnSaveBlended.Size = new System.Drawing.Size(48, 32);
      this.btnSaveBlended.TabIndex = 24;
      this.btnSaveBlended.UseVisualStyleBackColor = false;
      // 
      // formVideoExport
      // 
      this.AcceptButton = this.btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(529, 243);
      this.Controls.Add(this.groupSaveMethod);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnCancel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "formVideoExport";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "dlgSaveAnalysisOrVideo_Title";
      this.groupSaveMethod.ResumeLayout(false);
      this.groupSaveMethod.PerformLayout();
      this.ResumeLayout(false);

        }
        private System.Windows.Forms.TextBox tbSaveBlended;
        private System.Windows.Forms.Button btnSaveBlended;

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupSaveMethod;
        private System.Windows.Forms.CheckBox checkSlowMotion;
        private System.Windows.Forms.Label lblVideoExport;
    }
}