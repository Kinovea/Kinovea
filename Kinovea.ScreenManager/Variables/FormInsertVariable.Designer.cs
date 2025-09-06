namespace Kinovea.ScreenManager
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
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.olvVariables = new BrightIdeasSoftware.ObjectListView();
      ((System.ComponentModel.ISupportInitialize)(this.olvVariables)).BeginInit();
      this.SuspendLayout();
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(430, 293);
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
      this.btnOk.Location = new System.Drawing.Point(339, 293);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(85, 22);
      this.btnOk.TabIndex = 64;
      this.btnOk.Text = "Insert";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // olvVariables
      // 
      this.olvVariables.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.olvVariables.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.olvVariables.CellEditUseWholeCell = false;
      this.olvVariables.Cursor = System.Windows.Forms.Cursors.Default;
      this.olvVariables.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.olvVariables.FullRowSelect = true;
      this.olvVariables.GridLines = true;
      this.olvVariables.HeaderUsesThemes = true;
      this.olvVariables.HideSelection = false;
      this.olvVariables.Location = new System.Drawing.Point(12, 12);
      this.olvVariables.MultiSelect = false;
      this.olvVariables.Name = "olvVariables";
      this.olvVariables.Size = new System.Drawing.Size(503, 275);
      this.olvVariables.TabIndex = 65;
      this.olvVariables.UseCompatibleStateImageBehavior = false;
      this.olvVariables.View = System.Windows.Forms.View.Details;
      this.olvVariables.FormatRow += new System.EventHandler<BrightIdeasSoftware.FormatRowEventArgs>(this.olvVariables_FormatRow);
      this.olvVariables.DoubleClick += new System.EventHandler(this.olvVariables_DoubleClick);
      // 
      // FormInsertVariable
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(527, 327);
      this.Controls.Add(this.olvVariables);
      this.Controls.Add(this.btnOk);
      this.Controls.Add(this.btnCancel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormInsertVariable";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.Text = "Insert variable";
      ((System.ComponentModel.ISupportInitialize)(this.olvVariables)).EndInit();
      this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private BrightIdeasSoftware.ObjectListView olvVariables;
    }
}