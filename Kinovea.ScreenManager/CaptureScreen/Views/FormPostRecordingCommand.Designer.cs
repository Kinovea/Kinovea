namespace Kinovea.ScreenManager
{
    partial class FormPostRecordingCommand
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
      this.gpCommand = new System.Windows.Forms.GroupBox();
      this.btnCSV = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(402, 349);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(99, 24);
      this.btnOK.TabIndex = 31;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(507, 349);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(99, 24);
      this.btnCancel.TabIndex = 32;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // gpCommand
      // 
      this.gpCommand.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.gpCommand.Location = new System.Drawing.Point(12, 12);
      this.gpCommand.Name = "gpCommand";
      this.gpCommand.Size = new System.Drawing.Size(594, 331);
      this.gpCommand.TabIndex = 52;
      this.gpCommand.TabStop = false;
      this.gpCommand.Text = "Post-recording command";
      // 
      // btnCSV
      // 
      this.btnCSV.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnCSV.Location = new System.Drawing.Point(12, 349);
      this.btnCSV.Name = "btnCSV";
      this.btnCSV.Size = new System.Drawing.Size(99, 24);
      this.btnCSV.TabIndex = 53;
      this.btnCSV.Text = "Copy to clipboard";
      this.btnCSV.UseVisualStyleBackColor = true;
      // 
      // FormPostRecordingCommand
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(618, 385);
      this.Controls.Add(this.btnCSV);
      this.Controls.Add(this.gpCommand);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnCancel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormPostRecordingCommand";
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.Text = "FormPRC";
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox gpCommand;
        private System.Windows.Forms.Button btnCSV;
    }
}