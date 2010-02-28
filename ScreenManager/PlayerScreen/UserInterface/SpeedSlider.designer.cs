namespace Kinovea.ScreenManager
{
    partial class SpeedSlider
    {

        private System.Windows.Forms.Button btnCursor;
        private System.Windows.Forms.Button btnRail;

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
        	this.toolTips = new System.Windows.Forms.ToolTip(this.components);
        	this.btnCursor = new System.Windows.Forms.Button();
        	this.btnIncrease = new System.Windows.Forms.Button();
        	this.btnDecrease = new System.Windows.Forms.Button();
        	this.btnRail = new System.Windows.Forms.Button();
        	this.SuspendLayout();
        	// 
        	// btnCursor
        	// 
        	this.btnCursor.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.SpeedTrkCursor7;
        	this.btnCursor.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        	this.btnCursor.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnCursor.FlatAppearance.BorderSize = 0;
        	this.btnCursor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnCursor.Location = new System.Drawing.Point(80, 0);
        	this.btnCursor.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
        	this.btnCursor.Name = "btnCursor";
        	this.btnCursor.Size = new System.Drawing.Size(10, 10);
        	this.btnCursor.TabIndex = 0;
        	this.btnCursor.Text = "button1";
        	this.btnCursor.UseVisualStyleBackColor = true;
        	this.btnCursor.MouseMove += new System.Windows.Forms.MouseEventHandler(this.btnCursor_MouseMove);
        	// 
        	// btnIncrease
        	// 
        	this.btnIncrease.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.btnIncrease.BackColor = System.Drawing.Color.White;
        	this.btnIncrease.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        	this.btnIncrease.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnIncrease.FlatAppearance.BorderSize = 0;
        	this.btnIncrease.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnIncrease.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnIncrease.Image = global::Kinovea.ScreenManager.Properties.Resources.SpeedTrkIncrease2;
        	this.btnIncrease.Location = new System.Drawing.Point(190, 0);
        	this.btnIncrease.Name = "btnIncrease";
        	this.btnIncrease.Size = new System.Drawing.Size(10, 10);
        	this.btnIncrease.TabIndex = 3;
        	this.btnIncrease.Text = "button2";
        	this.btnIncrease.UseVisualStyleBackColor = false;
        	this.btnIncrease.MouseClick += new System.Windows.Forms.MouseEventHandler(this.btnIncrease_MouseClick);
        	// 
        	// btnDecrease
        	// 
        	this.btnDecrease.BackColor = System.Drawing.Color.White;
        	this.btnDecrease.Cursor = System.Windows.Forms.Cursors.Hand;
        	this.btnDecrease.FlatAppearance.BorderSize = 0;
        	this.btnDecrease.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
        	this.btnDecrease.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnDecrease.Image = global::Kinovea.ScreenManager.Properties.Resources.SpeedTrkDecrease2;
        	this.btnDecrease.Location = new System.Drawing.Point(0, 0);
        	this.btnDecrease.Name = "btnDecrease";
        	this.btnDecrease.Size = new System.Drawing.Size(10, 10);
        	this.btnDecrease.TabIndex = 2;
        	this.btnDecrease.Text = "button1";
        	this.btnDecrease.UseVisualStyleBackColor = false;
        	this.btnDecrease.MouseClick += new System.Windows.Forms.MouseEventHandler(this.btnDecrease_MouseClick);
        	// 
        	// btnRail
        	// 
        	this.btnRail.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        	        	        	| System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.btnRail.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.SpeedTrkBack5;
        	this.btnRail.FlatAppearance.BorderSize = 0;
        	this.btnRail.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.btnRail.Location = new System.Drawing.Point(15, 0);
        	this.btnRail.Name = "btnRail";
        	this.btnRail.Size = new System.Drawing.Size(170, 10);
        	this.btnRail.TabIndex = 1;
        	this.btnRail.Text = "button1";
        	this.btnRail.UseVisualStyleBackColor = true;
        	this.btnRail.MouseMove += new System.Windows.Forms.MouseEventHandler(this.btnRail_MouseMove);
        	this.btnRail.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnRail_MouseDown);
        	// 
        	// SpeedSlider
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
        	this.Controls.Add(this.btnCursor);
        	this.Controls.Add(this.btnIncrease);
        	this.Controls.Add(this.btnDecrease);
        	this.Controls.Add(this.btnRail);
        	this.MinimumSize = new System.Drawing.Size(20, 10);
        	this.Name = "SpeedSlider";
        	this.Size = new System.Drawing.Size(200, 10);
        	this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ToolTip toolTips;
        private System.Windows.Forms.Button btnDecrease;
        private System.Windows.Forms.Button btnIncrease;

        
    }
}
