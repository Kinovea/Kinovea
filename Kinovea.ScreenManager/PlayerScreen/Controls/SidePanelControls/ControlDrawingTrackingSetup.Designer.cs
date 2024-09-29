
namespace Kinovea.ScreenManager
{
    partial class ControlDrawingTrackingSetup
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
      this.pnlViewport = new System.Windows.Forms.Panel();
      this.grpTracking = new System.Windows.Forms.GroupBox();
      this.nudTolerance = new System.Windows.Forms.NumericUpDown();
      this.label7 = new System.Windows.Forms.Label();
      this.nudSearchWindowHeight = new System.Windows.Forms.NumericUpDown();
      this.nudObjWindowHeight = new System.Windows.Forms.NumericUpDown();
      this.nudSearchWindowWidth = new System.Windows.Forms.NumericUpDown();
      this.nudObjWindowWidth = new System.Windows.Forms.NumericUpDown();
      this.nudKeepAlive = new System.Windows.Forms.NumericUpDown();
      this.label6 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.cbTrackingAlgorithm = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.lblSearchWindow = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.lblObjectWindow = new System.Windows.Forms.Label();
      this.grpTracking.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudTolerance)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudSearchWindowHeight)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudObjWindowHeight)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudSearchWindowWidth)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudObjWindowWidth)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudKeepAlive)).BeginInit();
      this.SuspendLayout();
      // 
      // pnlViewport
      // 
      this.pnlViewport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pnlViewport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
      this.pnlViewport.Location = new System.Drawing.Point(0, 0);
      this.pnlViewport.Name = "pnlViewport";
      this.pnlViewport.Size = new System.Drawing.Size(362, 270);
      this.pnlViewport.TabIndex = 53;
      this.pnlViewport.Resize += new System.EventHandler(this.pnlViewport_Resize);
      // 
      // grpTracking
      // 
      this.grpTracking.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpTracking.BackColor = System.Drawing.Color.White;
      this.grpTracking.Controls.Add(this.nudTolerance);
      this.grpTracking.Controls.Add(this.label7);
      this.grpTracking.Controls.Add(this.nudSearchWindowHeight);
      this.grpTracking.Controls.Add(this.nudObjWindowHeight);
      this.grpTracking.Controls.Add(this.nudSearchWindowWidth);
      this.grpTracking.Controls.Add(this.nudObjWindowWidth);
      this.grpTracking.Controls.Add(this.nudKeepAlive);
      this.grpTracking.Controls.Add(this.label6);
      this.grpTracking.Controls.Add(this.label3);
      this.grpTracking.Controls.Add(this.cbTrackingAlgorithm);
      this.grpTracking.Controls.Add(this.label2);
      this.grpTracking.Controls.Add(this.label1);
      this.grpTracking.Controls.Add(this.label5);
      this.grpTracking.Controls.Add(this.lblSearchWindow);
      this.grpTracking.Controls.Add(this.label4);
      this.grpTracking.Controls.Add(this.lblObjectWindow);
      this.grpTracking.Location = new System.Drawing.Point(0, 276);
      this.grpTracking.Name = "grpTracking";
      this.grpTracking.Size = new System.Drawing.Size(362, 241);
      this.grpTracking.TabIndex = 54;
      this.grpTracking.TabStop = false;
      this.grpTracking.Text = "Tracking";
      // 
      // nudTolerance
      // 
      this.nudTolerance.Location = new System.Drawing.Point(175, 138);
      this.nudTolerance.Name = "nudTolerance";
      this.nudTolerance.Size = new System.Drawing.Size(40, 20);
      this.nudTolerance.TabIndex = 64;
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.ForeColor = System.Drawing.SystemColors.ControlText;
      this.label7.Location = new System.Drawing.Point(25, 140);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(58, 13);
      this.label7.TabIndex = 63;
      this.label7.Text = "Tolerance:";
      // 
      // nudSearchWindowHeight
      // 
      this.nudSearchWindowHeight.Location = new System.Drawing.Point(243, 70);
      this.nudSearchWindowHeight.Maximum = new decimal(new int[] {
            400,
            0,
            0,
            0});
      this.nudSearchWindowHeight.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
      this.nudSearchWindowHeight.Name = "nudSearchWindowHeight";
      this.nudSearchWindowHeight.Size = new System.Drawing.Size(40, 20);
      this.nudSearchWindowHeight.TabIndex = 62;
      this.nudSearchWindowHeight.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
      this.nudSearchWindowHeight.ValueChanged += new System.EventHandler(this.nudSearchWindow_ValueChanged);
      // 
      // nudObjWindowHeight
      // 
      this.nudObjWindowHeight.Location = new System.Drawing.Point(243, 102);
      this.nudObjWindowHeight.Maximum = new decimal(new int[] {
            400,
            0,
            0,
            0});
      this.nudObjWindowHeight.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
      this.nudObjWindowHeight.Name = "nudObjWindowHeight";
      this.nudObjWindowHeight.Size = new System.Drawing.Size(40, 20);
      this.nudObjWindowHeight.TabIndex = 61;
      this.nudObjWindowHeight.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
      this.nudObjWindowHeight.ValueChanged += new System.EventHandler(this.nudObjWindow_ValueChanged);
      // 
      // nudSearchWindowWidth
      // 
      this.nudSearchWindowWidth.Location = new System.Drawing.Point(175, 70);
      this.nudSearchWindowWidth.Maximum = new decimal(new int[] {
            400,
            0,
            0,
            0});
      this.nudSearchWindowWidth.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
      this.nudSearchWindowWidth.Name = "nudSearchWindowWidth";
      this.nudSearchWindowWidth.Size = new System.Drawing.Size(40, 20);
      this.nudSearchWindowWidth.TabIndex = 60;
      this.nudSearchWindowWidth.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
      this.nudSearchWindowWidth.ValueChanged += new System.EventHandler(this.nudSearchWindow_ValueChanged);
      // 
      // nudObjWindowWidth
      // 
      this.nudObjWindowWidth.Location = new System.Drawing.Point(175, 102);
      this.nudObjWindowWidth.Maximum = new decimal(new int[] {
            400,
            0,
            0,
            0});
      this.nudObjWindowWidth.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
      this.nudObjWindowWidth.Name = "nudObjWindowWidth";
      this.nudObjWindowWidth.Size = new System.Drawing.Size(40, 20);
      this.nudObjWindowWidth.TabIndex = 59;
      this.nudObjWindowWidth.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
      this.nudObjWindowWidth.ValueChanged += new System.EventHandler(this.nudObjWindow_ValueChanged);
      // 
      // nudKeepAlive
      // 
      this.nudKeepAlive.Location = new System.Drawing.Point(175, 164);
      this.nudKeepAlive.Name = "nudKeepAlive";
      this.nudKeepAlive.Size = new System.Drawing.Size(40, 20);
      this.nudKeepAlive.TabIndex = 58;
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.ForeColor = System.Drawing.SystemColors.ControlText;
      this.label6.Location = new System.Drawing.Point(25, 166);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(100, 13);
      this.label6.TabIndex = 57;
      this.label6.Text = "Keep alive (frames):";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.ForeColor = System.Drawing.SystemColors.ControlText;
      this.label3.Location = new System.Drawing.Point(25, 29);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(97, 13);
      this.label3.TabIndex = 55;
      this.label3.Text = "Tracking algorithm:";
      // 
      // cbTrackingAlgorithm
      // 
      this.cbTrackingAlgorithm.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
      this.cbTrackingAlgorithm.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbTrackingAlgorithm.FormattingEnabled = true;
      this.cbTrackingAlgorithm.ItemHeight = 16;
      this.cbTrackingAlgorithm.Location = new System.Drawing.Point(175, 20);
      this.cbTrackingAlgorithm.Name = "cbTrackingAlgorithm";
      this.cbTrackingAlgorithm.Size = new System.Drawing.Size(144, 22);
      this.cbTrackingAlgorithm.TabIndex = 52;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.ForeColor = System.Drawing.SystemColors.ControlText;
      this.label2.Location = new System.Drawing.Point(295, 72);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(18, 13);
      this.label2.TabIndex = 54;
      this.label2.Text = "px";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.ForeColor = System.Drawing.SystemColors.ControlText;
      this.label1.Location = new System.Drawing.Point(295, 105);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(18, 13);
      this.label1.TabIndex = 53;
      this.label1.Text = "px";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.ForeColor = System.Drawing.SystemColors.ControlText;
      this.label5.Location = new System.Drawing.Point(221, 73);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(13, 13);
      this.label5.TabIndex = 51;
      this.label5.Text = "×";
      // 
      // lblSearchWindow
      // 
      this.lblSearchWindow.AutoSize = true;
      this.lblSearchWindow.ForeColor = System.Drawing.SystemColors.ControlText;
      this.lblSearchWindow.Location = new System.Drawing.Point(25, 74);
      this.lblSearchWindow.Name = "lblSearchWindow";
      this.lblSearchWindow.Size = new System.Drawing.Size(83, 13);
      this.lblSearchWindow.TabIndex = 47;
      this.lblSearchWindow.Text = "Search window:";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.ForeColor = System.Drawing.SystemColors.ControlText;
      this.label4.Location = new System.Drawing.Point(221, 105);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(13, 13);
      this.label4.TabIndex = 45;
      this.label4.Text = "×";
      // 
      // lblObjectWindow
      // 
      this.lblObjectWindow.AutoSize = true;
      this.lblObjectWindow.ForeColor = System.Drawing.SystemColors.ControlText;
      this.lblObjectWindow.Location = new System.Drawing.Point(25, 105);
      this.lblObjectWindow.Name = "lblObjectWindow";
      this.lblObjectWindow.Size = new System.Drawing.Size(80, 13);
      this.lblObjectWindow.TabIndex = 43;
      this.lblObjectWindow.Text = "Object window:";
      // 
      // ControlDrawingTrackingSetup
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Gainsboro;
      this.Controls.Add(this.grpTracking);
      this.Controls.Add(this.pnlViewport);
      this.DoubleBuffered = true;
      this.ForeColor = System.Drawing.Color.Gray;
      this.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
      this.Name = "ControlDrawingTrackingSetup";
      this.Size = new System.Drawing.Size(362, 517);
      this.grpTracking.ResumeLayout(false);
      this.grpTracking.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudTolerance)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudSearchWindowHeight)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudObjWindowHeight)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudSearchWindowWidth)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudObjWindowWidth)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudKeepAlive)).EndInit();
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlViewport;
        private System.Windows.Forms.GroupBox grpTracking;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblSearchWindow;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblObjectWindow;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbTrackingAlgorithm;
        private System.Windows.Forms.NumericUpDown nudObjWindowHeight;
        private System.Windows.Forms.NumericUpDown nudSearchWindowWidth;
        private System.Windows.Forms.NumericUpDown nudObjWindowWidth;
        private System.Windows.Forms.NumericUpDown nudKeepAlive;
        private System.Windows.Forms.NumericUpDown nudTolerance;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown nudSearchWindowHeight;
    }
}
