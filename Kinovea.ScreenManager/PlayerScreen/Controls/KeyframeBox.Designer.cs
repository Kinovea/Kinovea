namespace Kinovea.ScreenManager
{
    partial class KeyframeBox
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
      this.lblName = new System.Windows.Forms.Label();
      this.btnClose = new System.Windows.Forms.Button();
      this.pbThumbnail = new System.Windows.Forms.PictureBox();
      this.toolTips = new System.Windows.Forms.ToolTip(this.components);
      ((System.ComponentModel.ISupportInitialize)(this.pbThumbnail)).BeginInit();
      this.SuspendLayout();
      // 
      // lblName
      // 
      this.lblName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.lblName.BackColor = System.Drawing.Color.Black;
      this.lblName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.lblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblName.ForeColor = System.Drawing.Color.White;
      this.lblName.Location = new System.Drawing.Point(2, 61);
      this.lblName.Name = "lblName";
      this.lblName.Size = new System.Drawing.Size(98, 14);
      this.lblName.TabIndex = 2;
      this.lblName.Text = "0:00:00:00";
      this.lblName.TextAlign = System.Drawing.ContentAlignment.TopCenter;
      this.lblName.Click += new System.EventHandler(this.lblTimecode_Click);
      this.lblName.MouseEnter += new System.EventHandler(this.Controls_MouseEnter);
      this.lblName.MouseLeave += new System.EventHandler(this.Controls_MouseLeave);
      // 
      // btnClose
      // 
      this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnClose.BackColor = System.Drawing.Color.Transparent;
      this.btnClose.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.bullet_close_black;
      this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnClose.FlatAppearance.BorderSize = 0;
      this.btnClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnClose.ForeColor = System.Drawing.Color.White;
      this.btnClose.Location = new System.Drawing.Point(82, 0);
      this.btnClose.Name = "btnClose";
      this.btnClose.Size = new System.Drawing.Size(15, 15);
      this.btnClose.TabIndex = 1;
      this.btnClose.UseVisualStyleBackColor = false;
      this.btnClose.Visible = false;
      this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
      this.btnClose.MouseLeave += new System.EventHandler(this.Controls_MouseLeave);
      // 
      // pbThumbnail
      // 
      this.pbThumbnail.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.pbThumbnail.BackColor = System.Drawing.Color.DimGray;
      this.pbThumbnail.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
      this.pbThumbnail.Location = new System.Drawing.Point(2, 2);
      this.pbThumbnail.Name = "pbThumbnail";
      this.pbThumbnail.Size = new System.Drawing.Size(98, 73);
      this.pbThumbnail.TabIndex = 0;
      this.pbThumbnail.TabStop = false;
      this.pbThumbnail.DragDrop += new System.Windows.Forms.DragEventHandler(this.Controls_DragDrop);
      this.pbThumbnail.DragOver += new System.Windows.Forms.DragEventHandler(this.Controls_DragOver);
      this.pbThumbnail.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Controls_MouseDown);
      this.pbThumbnail.MouseEnter += new System.EventHandler(this.Controls_MouseEnter);
      this.pbThumbnail.MouseLeave += new System.EventHandler(this.Controls_MouseLeave);
      this.pbThumbnail.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Controls_MouseMove);
      this.pbThumbnail.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Controls_MouseUp);
      // 
      // KeyframeBox
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.SteelBlue;
      this.Controls.Add(this.lblName);
      this.Controls.Add(this.btnClose);
      this.Controls.Add(this.pbThumbnail);
      this.Name = "KeyframeBox";
      this.Size = new System.Drawing.Size(102, 77);
      this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Controls_DragDrop);
      this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Controls_MouseDown);
      this.MouseLeave += new System.EventHandler(this.Controls_MouseLeave);
      this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Controls_MouseMove);
      this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Controls_MouseUp);
      ((System.ComponentModel.ISupportInitialize)(this.pbThumbnail)).EndInit();
      this.ResumeLayout(false);

        }
        private System.Windows.Forms.ToolTip toolTips;

        #endregion

        public System.Windows.Forms.PictureBox pbThumbnail;
        public System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblName;
    }
}
