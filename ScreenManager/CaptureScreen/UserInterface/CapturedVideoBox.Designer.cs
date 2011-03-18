﻿namespace Kinovea.ScreenManager
{
    partial class CapturedVideoBox
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
        	this.components = new System.ComponentModel.Container();
        	this.btnClose = new System.Windows.Forms.Button();
        	this.pbThumbnail = new System.Windows.Forms.PictureBox();
        	this.tbTitle = new System.Windows.Forms.TextBox();
        	this.toolTips = new System.Windows.Forms.ToolTip(this.components);
        	((System.ComponentModel.ISupportInitialize)(this.pbThumbnail)).BeginInit();
        	this.SuspendLayout();
        	// 
        	// btnClose
        	// 
        	this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnClose.BackColor = System.Drawing.Color.Transparent;
        	this.btnClose.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.bullet_close_black;
        	this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
        	this.btnClose.FlatAppearance.BorderSize = 0;
        	this.btnClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
        	this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
        	this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnClose.ForeColor = System.Drawing.Color.White;
        	this.btnClose.Location = new System.Drawing.Point(84, 1);
        	this.btnClose.Name = "btnClose";
        	this.btnClose.Size = new System.Drawing.Size(15, 15);
        	this.btnClose.TabIndex = 1;
        	this.btnClose.UseVisualStyleBackColor = false;
        	this.btnClose.Visible = false;
        	this.btnClose.MouseLeave += new System.EventHandler(this.Controls_MouseLeave);
        	this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
        	// 
        	// pbThumbnail
        	// 
        	this.pbThumbnail.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        	        	        	| System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.pbThumbnail.BackColor = System.Drawing.Color.DimGray;
        	this.pbThumbnail.Location = new System.Drawing.Point(1, 1);
        	this.pbThumbnail.Name = "pbThumbnail";
        	this.pbThumbnail.Size = new System.Drawing.Size(100, 75);
        	this.pbThumbnail.TabIndex = 0;
        	this.pbThumbnail.TabStop = false;
        	this.pbThumbnail.MouseLeave += new System.EventHandler(this.Controls_MouseLeave);
        	this.pbThumbnail.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pbThumbnail_MouseMove);
        	this.pbThumbnail.Click += new System.EventHandler(this.pbThumbnail_Click);
        	this.pbThumbnail.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.pbThumbnail_MouseDoubleClick);
        	this.pbThumbnail.MouseEnter += new System.EventHandler(this.Controls_MouseEnter);
        	// 
        	// tbTitle
        	// 
        	this.tbTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.tbTitle.BackColor = System.Drawing.Color.Black;
        	this.tbTitle.BorderStyle = System.Windows.Forms.BorderStyle.None;
        	this.tbTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.tbTitle.ForeColor = System.Drawing.Color.White;
        	this.tbTitle.Location = new System.Drawing.Point(1, 65);
        	this.tbTitle.MaxLength = 16;
        	this.tbTitle.Name = "tbTitle";
        	this.tbTitle.ReadOnly = true;
        	this.tbTitle.Size = new System.Drawing.Size(100, 11);
        	this.tbTitle.TabIndex = 4;
        	this.tbTitle.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
        	this.tbTitle.TextChanged += new System.EventHandler(this.TbTitleTextChanged);
        	this.tbTitle.MouseLeave += new System.EventHandler(this.Controls_MouseLeave);
        	this.tbTitle.Click += new System.EventHandler(this.pbThumbnail_Click);
        	this.tbTitle.Enter += new System.EventHandler(this.TbTitleEnter);
        	this.tbTitle.MouseEnter += new System.EventHandler(this.Controls_MouseEnter);
        	// 
        	// CapturedVideoBox
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackColor = System.Drawing.Color.White;
        	this.Controls.Add(this.tbTitle);
        	this.Controls.Add(this.btnClose);
        	this.Controls.Add(this.pbThumbnail);
        	this.Name = "CapturedVideoBox";
        	this.Size = new System.Drawing.Size(102, 77);
        	this.MouseLeave += new System.EventHandler(this.Controls_MouseLeave);
        	((System.ComponentModel.ISupportInitialize)(this.pbThumbnail)).EndInit();
        	this.ResumeLayout(false);
        	this.PerformLayout();
        }
        private System.Windows.Forms.ToolTip toolTips;
        private System.Windows.Forms.TextBox tbTitle;

        #endregion

        public System.Windows.Forms.PictureBox pbThumbnail;
        public System.Windows.Forms.Button btnClose;
    }
}
