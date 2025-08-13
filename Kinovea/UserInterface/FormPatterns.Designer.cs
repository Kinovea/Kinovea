namespace Kinovea.Root
{
    partial class FormPatterns
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
      this.colPattern = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.colContext = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.SuspendLayout();
      // 
      // lvSymbols
      // 
      this.lvSymbols.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lvSymbols.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colPattern,
            this.colContext});
      this.lvSymbols.HideSelection = false;
      this.lvSymbols.Location = new System.Drawing.Point(12, 12);
      this.lvSymbols.Name = "lvSymbols";
      this.lvSymbols.Size = new System.Drawing.Size(406, 284);
      this.lvSymbols.TabIndex = 0;
      this.lvSymbols.UseCompatibleStateImageBehavior = false;
      this.lvSymbols.View = System.Windows.Forms.View.Details;
      // 
      // colPattern
      // 
      this.colPattern.Text = "Variable";
      this.colPattern.Width = 128;
      // 
      // colContext
      // 
      this.colContext.Text = "Value";
      this.colContext.Width = 264;
      // 
      // FormPatterns
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(430, 308);
      this.Controls.Add(this.lvSymbols);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormPatterns";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.Text = "   Context variables";
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvSymbols;
        private System.Windows.Forms.ColumnHeader colContext;
        private System.Windows.Forms.ColumnHeader colPattern;
    }
}