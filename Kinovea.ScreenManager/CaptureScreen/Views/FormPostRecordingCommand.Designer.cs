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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPostRecordingCommand));
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.gpCommand = new System.Windows.Forms.GroupBox();
      this.cbEnable = new System.Windows.Forms.CheckBox();
      this.fastColoredTextBox1 = new FastColoredTextBoxNS.FastColoredTextBox();
      this.btnInsertVariable = new System.Windows.Forms.Button();
      this.btnSaveAndContinue = new System.Windows.Forms.Button();
      this.gpCommand.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.fastColoredTextBox1)).BeginInit();
      this.SuspendLayout();
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(365, 345);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(99, 24);
      this.btnOK.TabIndex = 31;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(470, 345);
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
      this.gpCommand.Controls.Add(this.cbEnable);
      this.gpCommand.Controls.Add(this.fastColoredTextBox1);
      this.gpCommand.Controls.Add(this.btnInsertVariable);
      this.gpCommand.Location = new System.Drawing.Point(12, 12);
      this.gpCommand.Name = "gpCommand";
      this.gpCommand.Size = new System.Drawing.Size(557, 327);
      this.gpCommand.TabIndex = 52;
      this.gpCommand.TabStop = false;
      // 
      // cbEnable
      // 
      this.cbEnable.AutoSize = true;
      this.cbEnable.Location = new System.Drawing.Point(15, 19);
      this.cbEnable.Name = "cbEnable";
      this.cbEnable.Size = new System.Drawing.Size(178, 17);
      this.cbEnable.TabIndex = 58;
      this.cbEnable.Text = "Enable post-recording command";
      this.cbEnable.UseVisualStyleBackColor = true;
      this.cbEnable.CheckedChanged += new System.EventHandler(this.cbEnable_CheckedChanged);
      // 
      // fastColoredTextBox1
      // 
      this.fastColoredTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.fastColoredTextBox1.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
      this.fastColoredTextBox1.AutoScrollMinSize = new System.Drawing.Size(228, 30);
      this.fastColoredTextBox1.BackBrush = null;
      this.fastColoredTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.fastColoredTextBox1.CharHeight = 15;
      this.fastColoredTextBox1.CharWidth = 7;
      this.fastColoredTextBox1.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.fastColoredTextBox1.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
      this.fastColoredTextBox1.Font = new System.Drawing.Font("Consolas", 9.75F);
      this.fastColoredTextBox1.IsReplaceMode = false;
      this.fastColoredTextBox1.Location = new System.Drawing.Point(15, 55);
      this.fastColoredTextBox1.Name = "fastColoredTextBox1";
      this.fastColoredTextBox1.Paddings = new System.Windows.Forms.Padding(0);
      this.fastColoredTextBox1.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
      this.fastColoredTextBox1.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("fastColoredTextBox1.ServiceColors")));
      this.fastColoredTextBox1.Size = new System.Drawing.Size(525, 218);
      this.fastColoredTextBox1.TabIndex = 57;
      this.fastColoredTextBox1.Text = "# Comment\r\nprogram -argument %variable% ";
      this.fastColoredTextBox1.Zoom = 100;
      this.fastColoredTextBox1.TextChanged += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.fastColoredTextBox1_TextChanged);
      // 
      // btnInsertVariable
      // 
      this.btnInsertVariable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnInsertVariable.Location = new System.Drawing.Point(409, 279);
      this.btnInsertVariable.Name = "btnInsertVariable";
      this.btnInsertVariable.Size = new System.Drawing.Size(131, 23);
      this.btnInsertVariable.TabIndex = 56;
      this.btnInsertVariable.Text = "Insert a variable…";
      this.btnInsertVariable.UseVisualStyleBackColor = true;
      // 
      // btnSaveAndContinue
      // 
      this.btnSaveAndContinue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnSaveAndContinue.Location = new System.Drawing.Point(12, 345);
      this.btnSaveAndContinue.Name = "btnSaveAndContinue";
      this.btnSaveAndContinue.Size = new System.Drawing.Size(161, 24);
      this.btnSaveAndContinue.TabIndex = 53;
      this.btnSaveAndContinue.Text = "Save and continue";
      this.btnSaveAndContinue.UseVisualStyleBackColor = true;
      this.btnSaveAndContinue.Click += new System.EventHandler(this.btnSaveAndContinue_Click);
      // 
      // FormPostRecordingCommand
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(581, 381);
      this.Controls.Add(this.btnSaveAndContinue);
      this.Controls.Add(this.gpCommand);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnCancel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(415, 380);
      this.Name = "FormPostRecordingCommand";
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
      this.Text = "FormPRC";
      this.gpCommand.ResumeLayout(false);
      this.gpCommand.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.fastColoredTextBox1)).EndInit();
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox gpCommand;
        private System.Windows.Forms.Button btnSaveAndContinue;
        private System.Windows.Forms.Button btnInsertVariable;
        private FastColoredTextBoxNS.FastColoredTextBox fastColoredTextBox1;
        private System.Windows.Forms.CheckBox cbEnable;
    }
}