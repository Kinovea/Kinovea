namespace Kinovea.Root
{
    partial class FormInsertVariable
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
      this.lvSymbols = new System.Windows.Forms.ListView();
      this.colDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.colKeyword = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.colExample = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.SuspendLayout();
      // 
      // lvSymbols
      // 
      this.lvSymbols.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lvSymbols.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colDescription,
            this.colExample,
            this.colKeyword});
      this.lvSymbols.HideSelection = false;
      this.lvSymbols.Location = new System.Drawing.Point(12, 12);
      this.lvSymbols.Name = "lvSymbols";
      this.lvSymbols.Size = new System.Drawing.Size(333, 295);
      this.lvSymbols.TabIndex = 0;
      this.lvSymbols.UseCompatibleStateImageBehavior = false;
      this.lvSymbols.View = System.Windows.Forms.View.Details;
      // 
      // colDescription
      // 
      this.colDescription.Text = "Variable";
      this.colDescription.Width = 100;
      // 
      // colKeyword
      // 
      this.colKeyword.Text = "Keyword";
      this.colKeyword.Width = 100;
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(260, 313);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(85, 22);
      this.btnCancel.TabIndex = 63;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(169, 313);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(85, 22);
      this.btnOk.TabIndex = 64;
      this.btnOk.Text = "Insert";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // colExample
      // 
      this.colExample.Text = "Example";
      this.colExample.Width = 126;
      // 
      // FormInsertVariable
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(357, 347);
      this.Controls.Add(this.btnOk);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.lvSymbols);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormInsertVariable";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.Text = "Insert variable";
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvSymbols;
        private System.Windows.Forms.ColumnHeader colKeyword;
        private System.Windows.Forms.ColumnHeader colDescription;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.ColumnHeader colExample;
    }
}