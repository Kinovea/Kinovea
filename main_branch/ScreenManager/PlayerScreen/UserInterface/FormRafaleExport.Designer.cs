namespace Kinovea.ScreenManager
{
    partial class formRafaleExport
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
            this.grpboxInfos = new System.Windows.Forms.GroupBox();
            this.lblInfosFileSuffix = new System.Windows.Forms.Label();
            this.lblInfosTotalSeconds = new System.Windows.Forms.Label();
            this.lblInfosTotalFrames = new System.Windows.Forms.Label();
            this.lblInfosFrequency = new System.Windows.Forms.Label();
            this.grpboxConfig = new System.Windows.Forms.GroupBox();
            this.chkKeyframesOnly = new System.Windows.Forms.CheckBox();
            this.freqViewer = new Kinovea.ScreenManager.FrequencyViewer();
            this.chkBlend = new System.Windows.Forms.CheckBox();
            this.trkInterval = new System.Windows.Forms.TrackBar();
            this.grpboxInfos.SuspendLayout();
            this.grpboxConfig.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkInterval)).BeginInit();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(262, 359);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(99, 24);
            this.btnOK.TabIndex = 20;
            this.btnOK.Text = "Enregistrer";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(367, 359);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(99, 24);
            this.btnCancel.TabIndex = 25;
            this.btnCancel.Text = "Annuler";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // grpboxInfos
            // 
            this.grpboxInfos.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpboxInfos.Controls.Add(this.lblInfosFileSuffix);
            this.grpboxInfos.Controls.Add(this.lblInfosTotalSeconds);
            this.grpboxInfos.Controls.Add(this.lblInfosTotalFrames);
            this.grpboxInfos.Location = new System.Drawing.Point(13, 233);
            this.grpboxInfos.Name = "grpboxInfos";
            this.grpboxInfos.Size = new System.Drawing.Size(454, 120);
            this.grpboxInfos.TabIndex = 27;
            this.grpboxInfos.TabStop = false;
            this.grpboxInfos.Text = "Informations";
            // 
            // lblInfosFileSuffix
            // 
            this.lblInfosFileSuffix.AutoSize = true;
            this.lblInfosFileSuffix.Location = new System.Drawing.Point(14, 82);
            this.lblInfosFileSuffix.Name = "lblInfosFileSuffix";
            this.lblInfosFileSuffix.Size = new System.Drawing.Size(352, 13);
            this.lblInfosFileSuffix.TabIndex = 7;
            this.lblInfosFileSuffix.Text = "Le suffixe sera ajouté automatiquement aux images. ( => fichier.00.00.jpg)";
            // 
            // lblInfosTotalSeconds
            // 
            this.lblInfosTotalSeconds.AutoSize = true;
            this.lblInfosTotalSeconds.Location = new System.Drawing.Point(12, 54);
            this.lblInfosTotalSeconds.Name = "lblInfosTotalSeconds";
            this.lblInfosTotalSeconds.Size = new System.Drawing.Size(326, 13);
            this.lblInfosTotalSeconds.TabIndex = 2;
            this.lblInfosTotalSeconds.Text = "Durée couverte par la séquence d\'images exportées : 20 secondes.";
            // 
            // lblInfosTotalFrames
            // 
            this.lblInfosTotalFrames.AutoSize = true;
            this.lblInfosTotalFrames.Location = new System.Drawing.Point(11, 27);
            this.lblInfosTotalFrames.Name = "lblInfosTotalFrames";
            this.lblInfosTotalFrames.Size = new System.Drawing.Size(220, 13);
            this.lblInfosTotalFrames.TabIndex = 2;
            this.lblInfosTotalFrames.Text = "Nombre total d\'images exportées : 83 images.";
            // 
            // lblInfosFrequency
            // 
            this.lblInfosFrequency.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblInfosFrequency.AutoSize = true;
            this.lblInfosFrequency.Location = new System.Drawing.Point(11, 71);
            this.lblInfosFrequency.Name = "lblInfosFrequency";
            this.lblInfosFrequency.Size = new System.Drawing.Size(363, 13);
            this.lblInfosFrequency.TabIndex = 1;
            this.lblInfosFrequency.Text = "Fréquence d\'enregistrement : une image tous les 40 centièmes de seconde.";
            // 
            // grpboxConfig
            // 
            this.grpboxConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpboxConfig.BackColor = System.Drawing.Color.White;
            this.grpboxConfig.Controls.Add(this.chkKeyframesOnly);
            this.grpboxConfig.Controls.Add(this.freqViewer);
            this.grpboxConfig.Controls.Add(this.lblInfosFrequency);
            this.grpboxConfig.Controls.Add(this.chkBlend);
            this.grpboxConfig.Controls.Add(this.trkInterval);
            this.grpboxConfig.Location = new System.Drawing.Point(12, 12);
            this.grpboxConfig.Name = "grpboxConfig";
            this.grpboxConfig.Size = new System.Drawing.Size(454, 211);
            this.grpboxConfig.TabIndex = 28;
            this.grpboxConfig.TabStop = false;
            this.grpboxConfig.Text = "Configuration";
            // 
            // chkKeyframesOnly
            // 
            this.chkKeyframesOnly.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.chkKeyframesOnly.AutoSize = true;
            this.chkKeyframesOnly.Location = new System.Drawing.Point(12, 175);
            this.chkKeyframesOnly.Name = "chkKeyframesOnly";
            this.chkKeyframesOnly.Size = new System.Drawing.Size(213, 17);
            this.chkKeyframesOnly.TabIndex = 15;
            this.chkKeyframesOnly.Text = "Enregistrer uniquement les Images Clés.";
            this.chkKeyframesOnly.UseVisualStyleBackColor = true;
            this.chkKeyframesOnly.CheckedChanged += new System.EventHandler(this.chkKeyframesOnly_CheckedChanged);
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
            // chkBlend
            // 
            this.chkBlend.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.chkBlend.AutoSize = true;
            this.chkBlend.Checked = true;
            this.chkBlend.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBlend.Location = new System.Drawing.Point(12, 143);
            this.chkBlend.Name = "chkBlend";
            this.chkBlend.Size = new System.Drawing.Size(193, 17);
            this.chkBlend.TabIndex = 10;
            this.chkBlend.Text = "Incruster les dessins sur les images.";
            this.chkBlend.UseVisualStyleBackColor = true;
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
            // formRafaleExport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(479, 395);
            this.Controls.Add(this.grpboxConfig);
            this.Controls.Add(this.grpboxInfos);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formRafaleExport";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "   Enregistrer une Séquence d\'Images";
            this.grpboxInfos.ResumeLayout(false);
            this.grpboxInfos.PerformLayout();
            this.grpboxConfig.ResumeLayout(false);
            this.grpboxConfig.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkInterval)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpboxInfos;
        private System.Windows.Forms.Label lblInfosTotalSeconds;
        private System.Windows.Forms.Label lblInfosFrequency;
        private System.Windows.Forms.GroupBox grpboxConfig;
        private System.Windows.Forms.Label lblInfosTotalFrames;
        private System.Windows.Forms.CheckBox chkBlend;
        private System.Windows.Forms.TrackBar trkInterval;
        private FrequencyViewer freqViewer;
        private System.Windows.Forms.Label lblInfosFileSuffix;
        private System.Windows.Forms.CheckBox chkKeyframesOnly;
    }
}