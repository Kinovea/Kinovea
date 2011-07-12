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
        	this.tbSaveAnalysis = new System.Windows.Forms.TextBox();
        	this.tbSaveBlended = new System.Windows.Forms.TextBox();
        	this.tbSaveMuxed = new System.Windows.Forms.TextBox();
        	this.btnSaveBlended = new System.Windows.Forms.Button();
        	this.btnSaveMuxed = new System.Windows.Forms.Button();
        	this.btnSaveAnalysis = new System.Windows.Forms.Button();
        	this.radioSaveBlended = new System.Windows.Forms.RadioButton();
        	this.radioSaveMuxed = new System.Windows.Forms.RadioButton();
        	this.radioSaveAnalysis = new System.Windows.Forms.RadioButton();
        	this.groupOptions = new System.Windows.Forms.GroupBox();
        	this.checkSlowMotion = new System.Windows.Forms.CheckBox();
        	this.groupSaveMethod.SuspendLayout();
        	this.groupOptions.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// btnOK
        	// 
        	this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnOK.Location = new System.Drawing.Point(405, 367);
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
        	this.btnCancel.Location = new System.Drawing.Point(510, 367);
        	this.btnCancel.Name = "btnCancel";
        	this.btnCancel.Size = new System.Drawing.Size(99, 24);
        	this.btnCancel.TabIndex = 40;
        	this.btnCancel.Text = "Generic_Cancel";
        	this.btnCancel.UseVisualStyleBackColor = true;
        	// 
        	// groupSaveMethod
        	// 
        	this.groupSaveMethod.Controls.Add(this.tbSaveAnalysis);
        	this.groupSaveMethod.Controls.Add(this.tbSaveBlended);
        	this.groupSaveMethod.Controls.Add(this.tbSaveMuxed);
        	this.groupSaveMethod.Controls.Add(this.btnSaveBlended);
        	this.groupSaveMethod.Controls.Add(this.btnSaveMuxed);
        	this.groupSaveMethod.Controls.Add(this.btnSaveAnalysis);
        	this.groupSaveMethod.Controls.Add(this.radioSaveBlended);
        	this.groupSaveMethod.Controls.Add(this.radioSaveMuxed);
        	this.groupSaveMethod.Controls.Add(this.radioSaveAnalysis);
        	this.groupSaveMethod.Location = new System.Drawing.Point(12, 12);
        	this.groupSaveMethod.Name = "groupSaveMethod";
        	this.groupSaveMethod.Size = new System.Drawing.Size(597, 258);
        	this.groupSaveMethod.TabIndex = 25;
        	this.groupSaveMethod.TabStop = false;
        	this.groupSaveMethod.Text = "dlgSaveAnalysisOrVideo_GroupSaveMethod";
        	// 
        	// tbSaveAnalysis
        	// 
        	this.tbSaveAnalysis.BorderStyle = System.Windows.Forms.BorderStyle.None;
        	this.tbSaveAnalysis.ForeColor = System.Drawing.Color.DimGray;
        	this.tbSaveAnalysis.Location = new System.Drawing.Point(108, 131);
        	this.tbSaveAnalysis.Multiline = true;
        	this.tbSaveAnalysis.Name = "tbSaveAnalysis";
        	this.tbSaveAnalysis.Size = new System.Drawing.Size(456, 39);
        	this.tbSaveAnalysis.TabIndex = 27;
        	this.tbSaveAnalysis.Text = "Only the drawings and tools will be saved, in a separate text file.\r\nThe file can" +
        	" be imported back on a video later.";
        	// 
        	// tbSaveBlended
        	// 
        	this.tbSaveBlended.BorderStyle = System.Windows.Forms.BorderStyle.None;
        	this.tbSaveBlended.ForeColor = System.Drawing.Color.DimGray;
        	this.tbSaveBlended.Location = new System.Drawing.Point(108, 202);
        	this.tbSaveBlended.Multiline = true;
        	this.tbSaveBlended.Name = "tbSaveBlended";
        	this.tbSaveBlended.Size = new System.Drawing.Size(456, 41);
        	this.tbSaveBlended.TabIndex = 26;
        	this.tbSaveBlended.Text = "Drawings and tools will be visible everywhere but not modifiable.\r\nExtra comments" +
        	" attached to key images will not be saved.";
        	// 
        	// tbSaveMuxed
        	// 
        	this.tbSaveMuxed.BorderStyle = System.Windows.Forms.BorderStyle.None;
        	this.tbSaveMuxed.ForeColor = System.Drawing.Color.DimGray;
        	this.tbSaveMuxed.Location = new System.Drawing.Point(108, 57);
        	this.tbSaveMuxed.Multiline = true;
        	this.tbSaveMuxed.Name = "tbSaveMuxed";
        	this.tbSaveMuxed.Size = new System.Drawing.Size(456, 42);
        	this.tbSaveMuxed.TabIndex = 25;
        	this.tbSaveMuxed.Text = "When opened in Kinovea, the drawings and tools will still be editable.\r\nWhen open" +
        	"ed in other players, the drawings and tools will not be visible.";
        	// 
        	// btnSaveBlended
        	// 
        	this.btnSaveBlended.BackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnSaveBlended.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.saveblended;
        	this.btnSaveBlended.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnSaveBlended.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        	this.btnSaveBlended.FlatAppearance.BorderSize = 0;
        	this.btnSaveBlended.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnSaveBlended.Location = new System.Drawing.Point(21, 176);
        	this.btnSaveBlended.Name = "btnSaveBlended";
        	this.btnSaveBlended.Size = new System.Drawing.Size(48, 32);
        	this.btnSaveBlended.TabIndex = 24;
        	this.btnSaveBlended.UseVisualStyleBackColor = false;
        	this.btnSaveBlended.Click += new System.EventHandler(this.BtnSaveBothClick);
        	// 
        	// btnSaveMuxed
        	// 
        	this.btnSaveMuxed.BackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnSaveMuxed.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.savemuxed;
        	this.btnSaveMuxed.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnSaveMuxed.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        	this.btnSaveMuxed.FlatAppearance.BorderSize = 0;
        	this.btnSaveMuxed.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnSaveMuxed.Location = new System.Drawing.Point(21, 31);
        	this.btnSaveMuxed.Name = "btnSaveMuxed";
        	this.btnSaveMuxed.Size = new System.Drawing.Size(48, 32);
        	this.btnSaveMuxed.TabIndex = 23;
        	this.btnSaveMuxed.UseVisualStyleBackColor = false;
        	this.btnSaveMuxed.Click += new System.EventHandler(this.BtnSaveMuxedClick);
        	// 
        	// btnSaveAnalysis
        	// 
        	this.btnSaveAnalysis.BackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnSaveAnalysis.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.savedata;
        	this.btnSaveAnalysis.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        	this.btnSaveAnalysis.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        	this.btnSaveAnalysis.FlatAppearance.BorderSize = 0;
        	this.btnSaveAnalysis.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnSaveAnalysis.Location = new System.Drawing.Point(21, 105);
        	this.btnSaveAnalysis.Name = "btnSaveAnalysis";
        	this.btnSaveAnalysis.Size = new System.Drawing.Size(48, 32);
        	this.btnSaveAnalysis.TabIndex = 22;
        	this.btnSaveAnalysis.UseVisualStyleBackColor = false;
        	this.btnSaveAnalysis.Click += new System.EventHandler(this.BtnSaveAnalysisClick);
        	// 
        	// radioSaveBlended
        	// 
        	this.radioSaveBlended.AutoSize = true;
        	this.radioSaveBlended.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.radioSaveBlended.Location = new System.Drawing.Point(81, 176);
        	this.radioSaveBlended.Name = "radioSaveBlended";
        	this.radioSaveBlended.Size = new System.Drawing.Size(350, 20);
        	this.radioSaveBlended.TabIndex = 20;
        	this.radioSaveBlended.Text = "Save video with drawings and tools applied on images";
        	this.radioSaveBlended.UseVisualStyleBackColor = true;
        	this.radioSaveBlended.CheckedChanged += new System.EventHandler(this.radio_CheckedChanged);
        	// 
        	// radioSaveMuxed
        	// 
        	this.radioSaveMuxed.AutoSize = true;
        	this.radioSaveMuxed.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.radioSaveMuxed.Location = new System.Drawing.Point(81, 31);
        	this.radioSaveMuxed.Name = "radioSaveMuxed";
        	this.radioSaveMuxed.Size = new System.Drawing.Size(301, 20);
        	this.radioSaveMuxed.TabIndex = 15;
        	this.radioSaveMuxed.Text = "Save video with drawings and tools modifiable";
        	this.radioSaveMuxed.UseVisualStyleBackColor = true;
        	this.radioSaveMuxed.CheckedChanged += new System.EventHandler(this.radio_CheckedChanged);
        	// 
        	// radioSaveAnalysis
        	// 
        	this.radioSaveAnalysis.AutoSize = true;
        	this.radioSaveAnalysis.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.radioSaveAnalysis.Location = new System.Drawing.Point(81, 105);
        	this.radioSaveAnalysis.Name = "radioSaveAnalysis";
        	this.radioSaveAnalysis.Size = new System.Drawing.Size(173, 20);
        	this.radioSaveAnalysis.TabIndex = 10;
        	this.radioSaveAnalysis.Text = "Save drawings and tools";
        	this.radioSaveAnalysis.UseVisualStyleBackColor = true;
        	this.radioSaveAnalysis.CheckedChanged += new System.EventHandler(this.radio_CheckedChanged);
        	// 
        	// groupOptions
        	// 
        	this.groupOptions.Controls.Add(this.checkSlowMotion);
        	this.groupOptions.Location = new System.Drawing.Point(12, 286);
        	this.groupOptions.Name = "groupOptions";
        	this.groupOptions.Size = new System.Drawing.Size(597, 73);
        	this.groupOptions.TabIndex = 26;
        	this.groupOptions.TabStop = false;
        	this.groupOptions.Text = "dlgSaveAnalysisOrVideo_GroupOptions";
        	// 
        	// checkSlowMotion
        	// 
        	this.checkSlowMotion.AutoSize = true;
        	this.checkSlowMotion.Location = new System.Drawing.Point(21, 35);
        	this.checkSlowMotion.Name = "checkSlowMotion";
        	this.checkSlowMotion.Size = new System.Drawing.Size(201, 17);
        	this.checkSlowMotion.TabIndex = 25;
        	this.checkSlowMotion.Text = "dlgSaveAnalysisOrVideo_CheckSlow";
        	this.checkSlowMotion.UseVisualStyleBackColor = true;
        	// 
        	// formVideoExport
        	// 
        	this.AcceptButton = this.btnOK;
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackColor = System.Drawing.Color.White;
        	this.CancelButton = this.btnCancel;
        	this.ClientSize = new System.Drawing.Size(621, 403);
        	this.Controls.Add(this.groupOptions);
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
        	this.groupOptions.ResumeLayout(false);
        	this.groupOptions.PerformLayout();
        	this.ResumeLayout(false);
        }
        private System.Windows.Forms.TextBox tbSaveMuxed;
        private System.Windows.Forms.TextBox tbSaveBlended;
        private System.Windows.Forms.TextBox tbSaveAnalysis;
        private System.Windows.Forms.RadioButton radioSaveBlended;
        private System.Windows.Forms.Button btnSaveBlended;
        private System.Windows.Forms.Button btnSaveAnalysis;
        private System.Windows.Forms.Button btnSaveMuxed;

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupSaveMethod;
        private System.Windows.Forms.RadioButton radioSaveMuxed;
        private System.Windows.Forms.RadioButton radioSaveAnalysis;
        private System.Windows.Forms.GroupBox groupOptions;
        private System.Windows.Forms.CheckBox checkSlowMotion;
    }
}