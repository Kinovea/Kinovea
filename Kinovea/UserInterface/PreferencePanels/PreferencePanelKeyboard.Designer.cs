namespace Kinovea.Root
{
    partial class PreferencePanelKeyboard
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lbCategories = new System.Windows.Forms.ListBox();
            this.btnApply = new System.Windows.Forms.Button();
            this.lblHotkey = new System.Windows.Forms.Label();
            this.btnClear = new System.Windows.Forms.Button();
            this.lblCategories = new System.Windows.Forms.Label();
            this.lblCommands = new System.Windows.Forms.Label();
            this.lvCommands = new System.Windows.Forms.ListView();
            this.colCommand = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colKey = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnDefault = new System.Windows.Forms.Button();
            this.tbHotkey = new Kinovea.Root.TextboxHotkey();
            this.SuspendLayout();
            // 
            // lbCategories
            // 
            this.lbCategories.FormattingEnabled = true;
            this.lbCategories.Location = new System.Drawing.Point(8, 24);
            this.lbCategories.Name = "lbCategories";
            this.lbCategories.Size = new System.Drawing.Size(137, 147);
            this.lbCategories.TabIndex = 0;
            this.lbCategories.SelectedIndexChanged += new System.EventHandler(this.lbCategories_SelectedIndexChanged);
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(193, 198);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(75, 23);
            this.btnApply.TabIndex = 5;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // lblHotkey
            // 
            this.lblHotkey.AutoSize = true;
            this.lblHotkey.Location = new System.Drawing.Point(11, 180);
            this.lblHotkey.Name = "lblHotkey";
            this.lblHotkey.Size = new System.Drawing.Size(41, 13);
            this.lblHotkey.TabIndex = 7;
            this.lblHotkey.Text = "Hotkey";
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(274, 198);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 23);
            this.btnClear.TabIndex = 8;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // lblCategories
            // 
            this.lblCategories.AutoSize = true;
            this.lblCategories.Location = new System.Drawing.Point(11, 5);
            this.lblCategories.Name = "lblCategories";
            this.lblCategories.Size = new System.Drawing.Size(57, 13);
            this.lblCategories.TabIndex = 9;
            this.lblCategories.Text = "Categories";
            // 
            // lblCommands
            // 
            this.lblCommands.AutoSize = true;
            this.lblCommands.Location = new System.Drawing.Point(154, 5);
            this.lblCommands.Name = "lblCommands";
            this.lblCommands.Size = new System.Drawing.Size(59, 13);
            this.lblCommands.TabIndex = 10;
            this.lblCommands.Text = "Commands";
            // 
            // lvCommands
            // 
            this.lvCommands.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colCommand,
            this.colKey});
            this.lvCommands.FullRowSelect = true;
            this.lvCommands.GridLines = true;
            this.lvCommands.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvCommands.Location = new System.Drawing.Point(151, 24);
            this.lvCommands.Name = "lvCommands";
            this.lvCommands.Size = new System.Drawing.Size(278, 147);
            this.lvCommands.TabIndex = 11;
            this.lvCommands.UseCompatibleStateImageBehavior = false;
            this.lvCommands.View = System.Windows.Forms.View.Details;
            this.lvCommands.SelectedIndexChanged += new System.EventHandler(this.lvCommands_SelectedIndexChanged);
            // 
            // colCommand
            // 
            this.colCommand.Text = "";
            this.colCommand.Width = 160;
            // 
            // colKey
            // 
            this.colKey.Text = "";
            this.colKey.Width = 129;
            // 
            // btnDefault
            // 
            this.btnDefault.Location = new System.Drawing.Point(354, 198);
            this.btnDefault.Name = "btnDefault";
            this.btnDefault.Size = new System.Drawing.Size(75, 23);
            this.btnDefault.TabIndex = 13;
            this.btnDefault.Text = "Default";
            this.btnDefault.UseVisualStyleBackColor = true;
            this.btnDefault.Click += new System.EventHandler(this.btnDefault_Click);
            // 
            // tbHotkey
            // 
            this.tbHotkey.Location = new System.Drawing.Point(8, 200);
            this.tbHotkey.Name = "tbHotkey";
            this.tbHotkey.Size = new System.Drawing.Size(179, 20);
            this.tbHotkey.TabIndex = 12;
            this.tbHotkey.Text = "None";
            // 
            // PreferencePanelKeyboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.btnDefault);
            this.Controls.Add(this.tbHotkey);
            this.Controls.Add(this.lvCommands);
            this.Controls.Add(this.lblCommands);
            this.Controls.Add(this.lblCategories);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.lblHotkey);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.lbCategories);
            this.Name = "PreferencePanelKeyboard";
            this.Size = new System.Drawing.Size(432, 236);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lbCategories;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Label lblHotkey;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label lblCategories;
        private System.Windows.Forms.Label lblCommands;
        private System.Windows.Forms.ListView lvCommands;
        private System.Windows.Forms.ColumnHeader colCommand;
        private System.Windows.Forms.ColumnHeader colKey;
        private TextboxHotkey tbHotkey;
        private System.Windows.Forms.Button btnDefault;


    }
}
