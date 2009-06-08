namespace Videa.ScreenManager
{
    partial class formDiapoExport
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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.grpboxConfig = new System.Windows.Forms.GroupBox();
            this.freqViewer = new Videa.ScreenManager.FrequencyViewer();
            this.lblInfosFrequency = new System.Windows.Forms.Label();
            this.trkInterval = new System.Windows.Forms.TrackBar();
            this.grpboxConfig.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkInterval)).BeginInit();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(263, 178);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(99, 24);
            this.btnOK.TabIndex = 10;
            this.btnOK.Text = "Enregistrer";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(368, 178);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(99, 24);
            this.btnCancel.TabIndex = 15;
            this.btnCancel.Text = "Annuler";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // grpboxConfig
            // 
            this.grpboxConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpboxConfig.BackColor = System.Drawing.Color.White;
            this.grpboxConfig.Controls.Add(this.freqViewer);
            this.grpboxConfig.Controls.Add(this.lblInfosFrequency);
            this.grpboxConfig.Controls.Add(this.trkInterval);
            this.grpboxConfig.Location = new System.Drawing.Point(12, 12);
            this.grpboxConfig.Name = "grpboxConfig";
            this.grpboxConfig.Size = new System.Drawing.Size(454, 149);
            this.grpboxConfig.TabIndex = 29;
            this.grpboxConfig.TabStop = false;
            this.grpboxConfig.Text = "Configuration";
            // 
            // freqViewer
            // 
            this.freqViewer.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.freqViewer.HorizontalLines = 2;
            this.freqViewer.Interval = 1000;
            this.freqViewer.Location = new System.Drawing.Point(15, 28);
            this.freqViewer.Name = "freqViewer";
            this.freqViewer.Size = new System.Drawing.Size(419, 27);
            this.freqViewer.TabIndex = 8;
            this.freqViewer.Total = 10000;
            // 
            // lblInfosFrequency
            // 
            this.lblInfosFrequency.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblInfosFrequency.AutoSize = true;
            this.lblInfosFrequency.Location = new System.Drawing.Point(11, 71);
            this.lblInfosFrequency.Name = "lblInfosFrequency";
            this.lblInfosFrequency.Size = new System.Drawing.Size(282, 13);
            this.lblInfosFrequency.TabIndex = 1;
            this.lblInfosFrequency.Text = "Durée de chaque diapositive : 40 centièmes de secondes.";
            // 
            // trkInterval
            // 
            this.trkInterval.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.trkInterval.Location = new System.Drawing.Point(13, 92);
            this.trkInterval.Name = "trkInterval";
            this.trkInterval.Size = new System.Drawing.Size(422, 45);
            this.trkInterval.TabIndex = 5;
            this.trkInterval.ValueChanged += new System.EventHandler(this.trkInterval_ValueChanged);
            // 
            // formDiapoExport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(479, 214);
            this.Controls.Add(this.grpboxConfig);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formDiapoExport";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FormDiapoExport";
            this.grpboxConfig.ResumeLayout(false);
            this.grpboxConfig.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkInterval)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpboxConfig;
        private FrequencyViewer freqViewer;
        private System.Windows.Forms.Label lblInfosFrequency;
        private System.Windows.Forms.TrackBar trkInterval;
    }
}