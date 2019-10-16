namespace Kinovea.Camera
{
    partial class CameraPropertyViewLinear
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
            this.cbAuto = new System.Windows.Forms.CheckBox();
            this.lblValue = new System.Windows.Forms.Label();
            this.tbValue = new System.Windows.Forms.TrackBar();
            this.lblName = new System.Windows.Forms.Label();
            this.nud = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.tbValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud)).BeginInit();
            this.SuspendLayout();
            // 
            // cbAuto
            // 
            this.cbAuto.AutoSize = true;
            this.cbAuto.Location = new System.Drawing.Point(381, 8);
            this.cbAuto.Name = "cbAuto";
            this.cbAuto.Size = new System.Drawing.Size(15, 14);
            this.cbAuto.TabIndex = 107;
            this.cbAuto.UseVisualStyleBackColor = true;
            this.cbAuto.CheckedChanged += new System.EventHandler(this.cbAuto_CheckedChanged);
            // 
            // lblValue
            // 
            this.lblValue.AutoSize = true;
            this.lblValue.BackColor = System.Drawing.Color.Transparent;
            this.lblValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblValue.ForeColor = System.Drawing.Color.Black;
            this.lblValue.Location = new System.Drawing.Point(109, 8);
            this.lblValue.Name = "lblValue";
            this.lblValue.Size = new System.Drawing.Size(13, 13);
            this.lblValue.TabIndex = 106;
            this.lblValue.Text = "0";
            this.lblValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbValue
            // 
            this.tbValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbValue.LargeChange = 1;
            this.tbValue.Location = new System.Drawing.Point(184, 5);
            this.tbValue.Name = "tbValue";
            this.tbValue.Size = new System.Drawing.Size(191, 45);
            this.tbValue.TabIndex = 105;
            this.tbValue.TickStyle = System.Windows.Forms.TickStyle.None;
            this.tbValue.ValueChanged += new System.EventHandler(this.tbValue_ValueChanged);
            // 
            // lblName
            // 
            this.lblName.BackColor = System.Drawing.Color.Transparent;
            this.lblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblName.ForeColor = System.Drawing.Color.Black;
            this.lblName.Location = new System.Drawing.Point(1, 2);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(104, 23);
            this.lblName.TabIndex = 104;
            this.lblName.Text = "Name:";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // nud
            // 
            this.nud.Location = new System.Drawing.Point(128, 5);
            this.nud.Name = "nud";
            this.nud.Size = new System.Drawing.Size(50, 20);
            this.nud.TabIndex = 108;
            this.nud.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nud.ValueChanged += new System.EventHandler(this.nud_ValueChanged);
            // 
            // CameraPropertyLinearView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.nud);
            this.Controls.Add(this.cbAuto);
            this.Controls.Add(this.lblValue);
            this.Controls.Add(this.tbValue);
            this.Controls.Add(this.lblName);
            this.Name = "CameraPropertyLinearView";
            this.Size = new System.Drawing.Size(400, 32);
            ((System.ComponentModel.ISupportInitialize)(this.tbValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cbAuto;
        private System.Windows.Forms.Label lblValue;
        private System.Windows.Forms.TrackBar tbValue;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.NumericUpDown nud;
    }
}
