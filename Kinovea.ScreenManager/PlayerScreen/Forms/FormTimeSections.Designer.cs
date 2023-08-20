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
      BrightIdeasSoftware.HeaderStateStyle headerStateStyle1 = new BrightIdeasSoftware.HeaderStateStyle();
      BrightIdeasSoftware.HeaderStateStyle headerStateStyle2 = new BrightIdeasSoftware.HeaderStateStyle();
      BrightIdeasSoftware.HeaderStateStyle headerStateStyle3 = new BrightIdeasSoftware.HeaderStateStyle();
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.olvSections = new BrightIdeasSoftware.ObjectListView();
      this.headerFormatStyle1 = new BrightIdeasSoftware.HeaderFormatStyle();
      ((System.ComponentModel.ISupportInitialize)(this.olvSections)).BeginInit();
      this.SuspendLayout();
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(317, 287);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(99, 24);
      this.btnOK.TabIndex = 33;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(423, 287);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(99, 24);
      this.btnCancel.TabIndex = 34;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // olvSections
      // 
      this.olvSections.AlternateRowBackColor = System.Drawing.Color.Gainsboro;
      this.olvSections.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.olvSections.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.SingleClickAlways;
      this.olvSections.CellEditUseWholeCell = false;
      this.olvSections.Cursor = System.Windows.Forms.Cursors.Default;
      this.olvSections.GridLines = true;
      this.olvSections.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.olvSections.HeaderFormatStyle = this.headerFormatStyle1;
      this.olvSections.HideSelection = false;
      this.olvSections.Location = new System.Drawing.Point(12, 23);
      this.olvSections.Name = "olvSections";
      this.olvSections.Size = new System.Drawing.Size(510, 256);
      this.olvSections.TabIndex = 25;
      this.olvSections.UseCompatibleStateImageBehavior = false;
      this.olvSections.View = System.Windows.Forms.View.Details;
      // 
      // headerFormatStyle1
      // 
      this.headerFormatStyle1.Hot = headerStateStyle1;
      headerStateStyle2.BackColor = System.Drawing.Color.Gainsboro;
      headerStateStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.headerFormatStyle1.Normal = headerStateStyle2;
      this.headerFormatStyle1.Pressed = headerStateStyle3;
      // 
      // FormTimeSections
      // 
      this.AcceptButton = this.btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(534, 320);
      this.Controls.Add(this.olvSections);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnCancel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormTimeSections";
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "FormTimeSections";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormTimeSections_FormClosing);
      ((System.ComponentModel.ISupportInitialize)(this.olvSections)).EndInit();
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private BrightIdeasSoftware.ObjectListView olvSections;
        private BrightIdeasSoftware.HeaderFormatStyle headerFormatStyle1;
    }
}