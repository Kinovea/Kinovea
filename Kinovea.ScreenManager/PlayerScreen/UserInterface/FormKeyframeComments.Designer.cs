namespace Kinovea.ScreenManager
{
    partial class formKeyframeComments
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        	this.components = new System.ComponentModel.Container();
        	this.rtbComment = new System.Windows.Forms.RichTextBox();
        	this.txtTitle = new System.Windows.Forms.TextBox();
        	this.pnlTextArea = new System.Windows.Forms.Panel();
        	this.btnBold = new System.Windows.Forms.Button();
        	this.btnItalic = new System.Windows.Forms.Button();
        	this.btnStrike = new System.Windows.Forms.Button();
        	this.btnUnderline = new System.Windows.Forms.Button();
        	this.pnlTitle = new System.Windows.Forms.Panel();
        	this.btnForeColor = new System.Windows.Forms.Button();
        	this.btnBackColor = new System.Windows.Forms.Button();
        	this.pnlColors = new System.Windows.Forms.Panel();
        	this.pnlFontStyle = new System.Windows.Forms.Panel();
        	this.toolTips = new System.Windows.Forms.ToolTip(this.components);
        	this.pnlTextArea.SuspendLayout();
        	this.pnlTitle.SuspendLayout();
        	this.pnlColors.SuspendLayout();
        	this.pnlFontStyle.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// rtbComment
        	// 
        	this.rtbComment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        	        	        	| System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.rtbComment.AutoWordSelection = true;
        	this.rtbComment.BackColor = System.Drawing.Color.White;
        	this.rtbComment.BorderStyle = System.Windows.Forms.BorderStyle.None;
        	this.rtbComment.DetectUrls = false;
        	this.rtbComment.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.rtbComment.Location = new System.Drawing.Point(10, 10);
        	this.rtbComment.Name = "rtbComment";
        	this.rtbComment.Size = new System.Drawing.Size(348, 161);
        	this.rtbComment.TabIndex = 10;
        	this.rtbComment.Text = "";
        	// 
        	// txtTitle
        	// 
        	this.txtTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.txtTitle.BackColor = System.Drawing.Color.White;
        	this.txtTitle.BorderStyle = System.Windows.Forms.BorderStyle.None;
        	this.txtTitle.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.txtTitle.ForeColor = System.Drawing.Color.CornflowerBlue;
        	this.txtTitle.Location = new System.Drawing.Point(10, 2);
        	this.txtTitle.Name = "txtTitle";
        	this.txtTitle.Size = new System.Drawing.Size(348, 22);
        	this.txtTitle.TabIndex = 5;
        	this.txtTitle.Text = "Keyframe Title";
        	// 
        	// pnlTextArea
        	// 
        	this.pnlTextArea.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        	        	        	| System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.pnlTextArea.BackColor = System.Drawing.Color.White;
        	this.pnlTextArea.Controls.Add(this.rtbComment);
        	this.pnlTextArea.Location = new System.Drawing.Point(12, 82);
        	this.pnlTextArea.Name = "pnlTextArea";
        	this.pnlTextArea.Size = new System.Drawing.Size(368, 181);
        	this.pnlTextArea.TabIndex = 15;
        	// 
        	// btnBold
        	// 
        	this.btnBold.BackColor = System.Drawing.Color.Transparent;
        	this.btnBold.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnBold.FlatAppearance.BorderSize = 0;
        	this.btnBold.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightSteelBlue;
        	this.btnBold.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
        	this.btnBold.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnBold.Image = global::Kinovea.ScreenManager.Properties.Resources.text_bold;
        	this.btnBold.Location = new System.Drawing.Point(4, 2);
        	this.btnBold.Name = "btnBold";
        	this.btnBold.Size = new System.Drawing.Size(20, 20);
        	this.btnBold.TabIndex = 16;
        	this.btnBold.UseVisualStyleBackColor = false;
        	this.btnBold.Click += new System.EventHandler(this.btnBold_Click);
        	// 
        	// btnItalic
        	// 
        	this.btnItalic.BackColor = System.Drawing.Color.Transparent;
        	this.btnItalic.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnItalic.FlatAppearance.BorderSize = 0;
        	this.btnItalic.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightSteelBlue;
        	this.btnItalic.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
        	this.btnItalic.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnItalic.Image = global::Kinovea.ScreenManager.Properties.Resources.text_italic;
        	this.btnItalic.Location = new System.Drawing.Point(30, 2);
        	this.btnItalic.Name = "btnItalic";
        	this.btnItalic.Size = new System.Drawing.Size(20, 20);
        	this.btnItalic.TabIndex = 17;
        	this.btnItalic.UseVisualStyleBackColor = false;
        	this.btnItalic.Click += new System.EventHandler(this.btnItalic_Click);
        	// 
        	// btnStrike
        	// 
        	this.btnStrike.BackColor = System.Drawing.Color.Transparent;
        	this.btnStrike.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnStrike.FlatAppearance.BorderSize = 0;
        	this.btnStrike.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnStrike.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
        	this.btnStrike.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnStrike.Image = global::Kinovea.ScreenManager.Properties.Resources.text_strikethrough;
        	this.btnStrike.Location = new System.Drawing.Point(82, 2);
        	this.btnStrike.Name = "btnStrike";
        	this.btnStrike.Size = new System.Drawing.Size(20, 20);
        	this.btnStrike.TabIndex = 19;
        	this.btnStrike.UseVisualStyleBackColor = false;
        	this.btnStrike.Click += new System.EventHandler(this.btnStrike_Click);
        	// 
        	// btnUnderline
        	// 
        	this.btnUnderline.BackColor = System.Drawing.Color.Transparent;
        	this.btnUnderline.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnUnderline.FlatAppearance.BorderSize = 0;
        	this.btnUnderline.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightSteelBlue;
        	this.btnUnderline.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
        	this.btnUnderline.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnUnderline.Image = global::Kinovea.ScreenManager.Properties.Resources.text_underline;
        	this.btnUnderline.Location = new System.Drawing.Point(56, 2);
        	this.btnUnderline.Name = "btnUnderline";
        	this.btnUnderline.Size = new System.Drawing.Size(20, 20);
        	this.btnUnderline.TabIndex = 18;
        	this.btnUnderline.UseVisualStyleBackColor = false;
        	this.btnUnderline.Click += new System.EventHandler(this.btnUnderline_Click);
        	// 
        	// pnlTitle
        	// 
        	this.pnlTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.pnlTitle.BackColor = System.Drawing.Color.White;
        	this.pnlTitle.Controls.Add(this.txtTitle);
        	this.pnlTitle.Location = new System.Drawing.Point(12, 11);
        	this.pnlTitle.Name = "pnlTitle";
        	this.pnlTitle.Size = new System.Drawing.Size(368, 26);
        	this.pnlTitle.TabIndex = 20;
        	// 
        	// btnForeColor
        	// 
        	this.btnForeColor.BackColor = System.Drawing.Color.Transparent;
        	this.btnForeColor.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnForeColor.FlatAppearance.BorderSize = 0;
        	this.btnForeColor.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightSteelBlue;
        	this.btnForeColor.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
        	this.btnForeColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnForeColor.Image = global::Kinovea.ScreenManager.Properties.Resources.text_forecolor;
        	this.btnForeColor.Location = new System.Drawing.Point(3, 2);
        	this.btnForeColor.Name = "btnForeColor";
        	this.btnForeColor.Size = new System.Drawing.Size(20, 20);
        	this.btnForeColor.TabIndex = 21;
        	this.btnForeColor.UseVisualStyleBackColor = false;
        	this.btnForeColor.Click += new System.EventHandler(this.btnForeColor_Click);
        	// 
        	// btnBackColor
        	// 
        	this.btnBackColor.BackColor = System.Drawing.Color.Transparent;
        	this.btnBackColor.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnBackColor.FlatAppearance.BorderSize = 0;
        	this.btnBackColor.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightSteelBlue;
        	this.btnBackColor.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
        	this.btnBackColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnBackColor.Image = global::Kinovea.ScreenManager.Properties.Resources.text_backcolor;
        	this.btnBackColor.Location = new System.Drawing.Point(29, 2);
        	this.btnBackColor.Name = "btnBackColor";
        	this.btnBackColor.Size = new System.Drawing.Size(20, 20);
        	this.btnBackColor.TabIndex = 22;
        	this.btnBackColor.UseVisualStyleBackColor = false;
        	this.btnBackColor.Click += new System.EventHandler(this.btnBackColor_Click);
        	// 
        	// pnlColors
        	// 
        	this.pnlColors.BackColor = System.Drawing.Color.WhiteSmoke;
        	this.pnlColors.Controls.Add(this.btnForeColor);
        	this.pnlColors.Controls.Add(this.btnBackColor);
        	this.pnlColors.Location = new System.Drawing.Point(124, 52);
        	this.pnlColors.Name = "pnlColors";
        	this.pnlColors.Size = new System.Drawing.Size(53, 24);
        	this.pnlColors.TabIndex = 23;
        	// 
        	// pnlFontStyle
        	// 
        	this.pnlFontStyle.BackColor = System.Drawing.Color.WhiteSmoke;
        	this.pnlFontStyle.Controls.Add(this.btnStrike);
        	this.pnlFontStyle.Controls.Add(this.btnUnderline);
        	this.pnlFontStyle.Controls.Add(this.btnItalic);
        	this.pnlFontStyle.Controls.Add(this.btnBold);
        	this.pnlFontStyle.Location = new System.Drawing.Point(12, 52);
        	this.pnlFontStyle.Name = "pnlFontStyle";
        	this.pnlFontStyle.Size = new System.Drawing.Size(107, 24);
        	this.pnlFontStyle.TabIndex = 24;
        	// 
        	// formKeyframeComments
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackColor = System.Drawing.Color.Gainsboro;
        	this.ClientSize = new System.Drawing.Size(392, 274);
        	this.Controls.Add(this.pnlFontStyle);
        	this.Controls.Add(this.pnlColors);
        	this.Controls.Add(this.pnlTitle);
        	this.Controls.Add(this.pnlTextArea);
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
        	this.MinimumSize = new System.Drawing.Size(208, 206);
        	this.Name = "formKeyframeComments";
        	this.ShowInTaskbar = false;
        	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        	this.Text = "   Commentaire...";
        	this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formKeyframeComments_FormClosing);
        	this.pnlTextArea.ResumeLayout(false);
        	this.pnlTitle.ResumeLayout(false);
        	this.pnlTitle.PerformLayout();
        	this.pnlColors.ResumeLayout(false);
        	this.pnlFontStyle.ResumeLayout(false);
        	this.ResumeLayout(false);
        }
        private System.Windows.Forms.ToolTip toolTips;
        private System.Windows.Forms.Panel pnlColors;
        private System.Windows.Forms.Panel pnlFontStyle;
        private System.Windows.Forms.Button btnBold;
        private System.Windows.Forms.Button btnItalic;
        private System.Windows.Forms.Button btnStrike;
        private System.Windows.Forms.Button btnUnderline;
        private System.Windows.Forms.Button btnForeColor;
        private System.Windows.Forms.Button btnBackColor;
        private System.Windows.Forms.Panel pnlTitle;
        private System.Windows.Forms.Panel pnlTextArea;

        #endregion

        private System.Windows.Forms.RichTextBox rtbComment;
        private System.Windows.Forms.TextBox txtTitle;
    }
}