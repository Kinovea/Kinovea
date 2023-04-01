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
      this.tbName = new System.Windows.Forms.TextBox();
      this.pnlTextArea = new System.Windows.Forms.Panel();
      this.btnBold = new System.Windows.Forms.Button();
      this.btnItalic = new System.Windows.Forms.Button();
      this.btnStrike = new System.Windows.Forms.Button();
      this.btnUnderline = new System.Windows.Forms.Button();
      this.btnForeColor = new System.Windows.Forms.Button();
      this.btnBackColor = new System.Windows.Forms.Button();
      this.pnlColors = new System.Windows.Forms.Panel();
      this.pnlFontStyle = new System.Windows.Forms.Panel();
      this.toolTips = new System.Windows.Forms.ToolTip(this.components);
      this.btnFrameColor = new System.Windows.Forms.Button();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.lblTimecode = new System.Windows.Forms.Label();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.pnlTextArea.SuspendLayout();
      this.pnlColors.SuspendLayout();
      this.pnlFontStyle.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
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
      this.rtbComment.Size = new System.Drawing.Size(373, 176);
      this.rtbComment.TabIndex = 10;
      this.rtbComment.Text = "";
      this.rtbComment.TextChanged += new System.EventHandler(this.rtbComment_TextChanged);
      // 
      // tbName
      // 
      this.tbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbName.BackColor = System.Drawing.Color.White;
      this.tbName.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.tbName.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbName.ForeColor = System.Drawing.Color.CornflowerBlue;
      this.tbName.Location = new System.Drawing.Point(87, 22);
      this.tbName.Name = "tbName";
      this.tbName.Size = new System.Drawing.Size(309, 22);
      this.tbName.TabIndex = 5;
      this.tbName.Text = "Name";
      this.tbName.TextChanged += new System.EventHandler(this.tbName_TextChanged);
      // 
      // pnlTextArea
      // 
      this.pnlTextArea.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pnlTextArea.BackColor = System.Drawing.Color.White;
      this.pnlTextArea.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.pnlTextArea.Controls.Add(this.rtbComment);
      this.pnlTextArea.Location = new System.Drawing.Point(10, 49);
      this.pnlTextArea.Name = "pnlTextArea";
      this.pnlTextArea.Size = new System.Drawing.Size(395, 198);
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
      this.pnlColors.Location = new System.Drawing.Point(122, 19);
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
      this.pnlFontStyle.Location = new System.Drawing.Point(10, 19);
      this.pnlFontStyle.Name = "pnlFontStyle";
      this.pnlFontStyle.Size = new System.Drawing.Size(107, 24);
      this.pnlFontStyle.TabIndex = 24;
      // 
      // btnFrameColor
      // 
      this.btnFrameColor.BackColor = System.Drawing.Color.SteelBlue;
      this.btnFrameColor.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnFrameColor.FlatAppearance.BorderSize = 0;
      this.btnFrameColor.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightSteelBlue;
      this.btnFrameColor.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
      this.btnFrameColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnFrameColor.Location = new System.Drawing.Point(15, 19);
      this.btnFrameColor.Name = "btnFrameColor";
      this.btnFrameColor.Size = new System.Drawing.Size(59, 56);
      this.btnFrameColor.TabIndex = 23;
      this.btnFrameColor.UseVisualStyleBackColor = false;
      this.btnFrameColor.Click += new System.EventHandler(this.btnFrameColor_Click);
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.lblTimecode);
      this.groupBox1.Controls.Add(this.tbName);
      this.groupBox1.Controls.Add(this.btnFrameColor);
      this.groupBox1.Location = new System.Drawing.Point(12, 12);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(413, 90);
      this.groupBox1.TabIndex = 25;
      this.groupBox1.TabStop = false;
      // 
      // lblTimecode
      // 
      this.lblTimecode.AutoSize = true;
      this.lblTimecode.BackColor = System.Drawing.Color.Transparent;
      this.lblTimecode.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblTimecode.ForeColor = System.Drawing.Color.Black;
      this.lblTimecode.Location = new System.Drawing.Point(85, 53);
      this.lblTimecode.Name = "lblTimecode";
      this.lblTimecode.Size = new System.Drawing.Size(54, 13);
      this.lblTimecode.TabIndex = 86;
      this.lblTimecode.Text = "Timecode";
      this.lblTimecode.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.BackColor = System.Drawing.Color.White;
      this.groupBox2.Controls.Add(this.pnlColors);
      this.groupBox2.Controls.Add(this.pnlFontStyle);
      this.groupBox2.Controls.Add(this.pnlTextArea);
      this.groupBox2.Location = new System.Drawing.Point(12, 108);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(413, 257);
      this.groupBox2.TabIndex = 26;
      this.groupBox2.TabStop = false;
      // 
      // formKeyframeComments
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(437, 377);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MinimumSize = new System.Drawing.Size(206, 200);
      this.Name = "formKeyframeComments";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "   Configuration…";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formKeyframeComments_FormClosing);
      this.pnlTextArea.ResumeLayout(false);
      this.pnlColors.ResumeLayout(false);
      this.pnlFontStyle.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
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
        private System.Windows.Forms.Panel pnlTextArea;

        #endregion

        private System.Windows.Forms.RichTextBox rtbComment;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Button btnFrameColor;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblTimecode;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}