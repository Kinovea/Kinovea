namespace Kinovea.Root
{
    partial class FormAbout
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
        	System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAbout));
        	this.logoPictureBox = new System.Windows.Forms.PictureBox();
        	this.lblKinovea = new System.Windows.Forms.Label();
        	this.okButton = new System.Windows.Forms.Button();
        	this.labelCopyright = new System.Windows.Forms.Label();
        	this.lnkKinovea = new System.Windows.Forms.LinkLabel();
        	this.rtbInfos = new System.Windows.Forms.RichTextBox();
        	((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).BeginInit();
        	this.SuspendLayout();
        	// 
        	// logoPictureBox
        	// 
        	this.logoPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("logoPictureBox.Image")));
        	this.logoPictureBox.Location = new System.Drawing.Point(48, 13);
        	this.logoPictureBox.Name = "logoPictureBox";
        	this.logoPictureBox.Size = new System.Drawing.Size(356, 124);
        	this.logoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
        	this.logoPictureBox.TabIndex = 12;
        	this.logoPictureBox.TabStop = false;
        	// 
        	// lblKinovea
        	// 
        	this.lblKinovea.AutoSize = true;
        	this.lblKinovea.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.lblKinovea.Location = new System.Drawing.Point(11, 140);
        	this.lblKinovea.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
        	this.lblKinovea.MaximumSize = new System.Drawing.Size(0, 17);
        	this.lblKinovea.Name = "lblKinovea";
        	this.lblKinovea.Size = new System.Drawing.Size(67, 12);
        	this.lblKinovea.TabIndex = 19;
        	this.lblKinovea.Text = "Kinovea - 0.0.0";
        	this.lblKinovea.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        	// 
        	// okButton
        	// 
        	this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        	this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        	this.okButton.Location = new System.Drawing.Point(359, 472);
        	this.okButton.Name = "okButton";
        	this.okButton.Size = new System.Drawing.Size(75, 21);
        	this.okButton.TabIndex = 24;
        	this.okButton.Text = "&OK";
        	// 
        	// labelCopyright
        	// 
        	this.labelCopyright.AutoSize = true;
        	this.labelCopyright.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.labelCopyright.Location = new System.Drawing.Point(11, 154);
        	this.labelCopyright.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
        	this.labelCopyright.MaximumSize = new System.Drawing.Size(0, 17);
        	this.labelCopyright.Name = "labelCopyright";
        	this.labelCopyright.Size = new System.Drawing.Size(169, 12);
        	this.labelCopyright.TabIndex = 21;
        	this.labelCopyright.Text = "Copyright © 2006-2009 - Joan Charmant";
        	this.labelCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        	// 
        	// lnkKinovea
        	// 
        	this.lnkKinovea.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        	this.lnkKinovea.AutoSize = true;
        	this.lnkKinovea.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.lnkKinovea.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
        	this.lnkKinovea.LinkColor = System.Drawing.Color.Blue;
        	this.lnkKinovea.Location = new System.Drawing.Point(13, 472);
        	this.lnkKinovea.Name = "lnkKinovea";
        	this.lnkKinovea.Size = new System.Drawing.Size(81, 12);
        	this.lnkKinovea.TabIndex = 25;
        	this.lnkKinovea.TabStop = true;
        	this.lnkKinovea.Text = "www.kinovea.org";
        	this.lnkKinovea.VisitedLinkColor = System.Drawing.Color.Blue;
        	this.lnkKinovea.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkKinovea_LinkClicked);
        	// 
        	// rtbInfos
        	// 
        	this.rtbInfos.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        	        	        	| System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.rtbInfos.BackColor = System.Drawing.Color.WhiteSmoke;
        	this.rtbInfos.BorderStyle = System.Windows.Forms.BorderStyle.None;
        	this.rtbInfos.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.rtbInfos.Location = new System.Drawing.Point(13, 169);
        	this.rtbInfos.Name = "rtbInfos";
        	this.rtbInfos.ReadOnly = true;
        	this.rtbInfos.Size = new System.Drawing.Size(418, 292);
        	this.rtbInfos.TabIndex = 26;
        	this.rtbInfos.Text = "\n(hard coded in the source)";
        	// 
        	// FormAbout
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackColor = System.Drawing.Color.White;
        	this.ClientSize = new System.Drawing.Size(447, 506);
        	this.Controls.Add(this.rtbInfos);
        	this.Controls.Add(this.labelCopyright);
        	this.Controls.Add(this.lblKinovea);
        	this.Controls.Add(this.okButton);
        	this.Controls.Add(this.lnkKinovea);
        	this.Controls.Add(this.logoPictureBox);
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
        	this.MaximizeBox = false;
        	this.MinimizeBox = false;
        	this.Name = "FormAbout";
        	this.Padding = new System.Windows.Forms.Padding(10);
        	this.ShowIcon = false;
        	this.ShowInTaskbar = false;
        	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        	this.Text = "    About Kinovea";
        	((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).EndInit();
        	this.ResumeLayout(false);
        	this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox logoPictureBox;
        private System.Windows.Forms.Label lblKinovea;
        private System.Windows.Forms.Label labelCopyright;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.LinkLabel lnkKinovea;
        private System.Windows.Forms.RichTextBox rtbInfos;

    }
}