namespace Kinovea.ScreenManager
{
    partial class formVideoExport
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
            this.groupSaveMethod = new System.Windows.Forms.GroupBox();
            this.radioSaveBoth = new System.Windows.Forms.RadioButton();
            this.radioSaveMuxed = new System.Windows.Forms.RadioButton();
            this.radioSaveAnalysis = new System.Windows.Forms.RadioButton();
            this.radioSaveVideo = new System.Windows.Forms.RadioButton();
            this.groupOptions = new System.Windows.Forms.GroupBox();
            this.checkBlendDrawings = new System.Windows.Forms.CheckBox();
            this.checkSlowMotion = new System.Windows.Forms.CheckBox();
            this.groupSaveMethod.SuspendLayout();
            this.groupOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(263, 298);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(99, 24);
            this.btnOK.TabIndex = 35;
            this.btnOK.Text = "Enregistrer";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(368, 298);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(99, 24);
            this.btnCancel.TabIndex = 40;
            this.btnCancel.Text = "Annuler";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // groupSaveMethod
            // 
            this.groupSaveMethod.Controls.Add(this.radioSaveBoth);
            this.groupSaveMethod.Controls.Add(this.radioSaveMuxed);
            this.groupSaveMethod.Controls.Add(this.radioSaveAnalysis);
            this.groupSaveMethod.Controls.Add(this.radioSaveVideo);
            this.groupSaveMethod.Location = new System.Drawing.Point(12, 12);
            this.groupSaveMethod.Name = "groupSaveMethod";
            this.groupSaveMethod.Size = new System.Drawing.Size(455, 168);
            this.groupSaveMethod.TabIndex = 25;
            this.groupSaveMethod.TabStop = false;
            this.groupSaveMethod.Text = "Méthode d\'Enregistrement";
            // 
            // radioSaveBoth
            // 
            this.radioSaveBoth.AutoSize = true;
            this.radioSaveBoth.Location = new System.Drawing.Point(32, 131);
            this.radioSaveBoth.Name = "radioSaveBoth";
            this.radioSaveBoth.Size = new System.Drawing.Size(301, 17);
            this.radioSaveBoth.TabIndex = 20;
            this.radioSaveBoth.Text = "Enregistrer la vidéo et l\'analyse dans deux fichiers séparés.";
            this.radioSaveBoth.UseVisualStyleBackColor = true;
            // 
            // radioSaveMuxed
            // 
            this.radioSaveMuxed.AutoSize = true;
            this.radioSaveMuxed.Location = new System.Drawing.Point(32, 96);
            this.radioSaveMuxed.Name = "radioSaveMuxed";
            this.radioSaveMuxed.Size = new System.Drawing.Size(388, 17);
            this.radioSaveMuxed.TabIndex = 15;
            this.radioSaveMuxed.Text = "Enregistrer la vidéo et l\'analyse dans le même fichier, de façon indépendante.";
            this.radioSaveMuxed.UseVisualStyleBackColor = true;
            // 
            // radioSaveAnalysis
            // 
            this.radioSaveAnalysis.AutoSize = true;
            this.radioSaveAnalysis.Location = new System.Drawing.Point(32, 62);
            this.radioSaveAnalysis.Name = "radioSaveAnalysis";
            this.radioSaveAnalysis.Size = new System.Drawing.Size(179, 17);
            this.radioSaveAnalysis.TabIndex = 10;
            this.radioSaveAnalysis.Text = "Enregistrer uniquement l\'analyse.";
            this.radioSaveAnalysis.UseVisualStyleBackColor = true;
            this.radioSaveAnalysis.CheckedChanged += new System.EventHandler(this.radioSaveAnalysis_CheckedChanged);
            // 
            // radioSaveVideo
            // 
            this.radioSaveVideo.AutoSize = true;
            this.radioSaveVideo.Location = new System.Drawing.Point(32, 30);
            this.radioSaveVideo.Name = "radioSaveVideo";
            this.radioSaveVideo.Size = new System.Drawing.Size(176, 17);
            this.radioSaveVideo.TabIndex = 5;
            this.radioSaveVideo.Text = "Enregistrer uniquement la vidéo.";
            this.radioSaveVideo.UseVisualStyleBackColor = true;
            // 
            // groupOptions
            // 
            this.groupOptions.Controls.Add(this.checkBlendDrawings);
            this.groupOptions.Controls.Add(this.checkSlowMotion);
            this.groupOptions.Location = new System.Drawing.Point(15, 186);
            this.groupOptions.Name = "groupOptions";
            this.groupOptions.Size = new System.Drawing.Size(451, 100);
            this.groupOptions.TabIndex = 26;
            this.groupOptions.TabStop = false;
            this.groupOptions.Text = "Options";
            // 
            // checkBlendDrawings
            // 
            this.checkBlendDrawings.AutoSize = true;
            this.checkBlendDrawings.Location = new System.Drawing.Point(29, 69);
            this.checkBlendDrawings.Name = "checkBlendDrawings";
            this.checkBlendDrawings.Size = new System.Drawing.Size(193, 17);
            this.checkBlendDrawings.TabIndex = 30;
            this.checkBlendDrawings.Text = "Incruster les dessins sur les images.";
            this.checkBlendDrawings.UseVisualStyleBackColor = true;
            // 
            // checkSlowMotion
            // 
            this.checkSlowMotion.AutoSize = true;
            this.checkSlowMotion.Location = new System.Drawing.Point(29, 37);
            this.checkSlowMotion.Name = "checkSlowMotion";
            this.checkSlowMotion.Size = new System.Drawing.Size(314, 17);
            this.checkSlowMotion.TabIndex = 25;
            this.checkSlowMotion.Text = "Tenir compte du ralentit pour l\'enregistrement. (Ralentit : 27%)";
            this.checkSlowMotion.UseVisualStyleBackColor = true;
            // 
            // formVideoExport
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(479, 334);
            this.Controls.Add(this.groupOptions);
            this.Controls.Add(this.groupSaveMethod);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formVideoExport";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "   Enregistrer l\'Analyse ou la Vidéo...";
            this.groupSaveMethod.ResumeLayout(false);
            this.groupSaveMethod.PerformLayout();
            this.groupOptions.ResumeLayout(false);
            this.groupOptions.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupSaveMethod;
        private System.Windows.Forms.RadioButton radioSaveMuxed;
        private System.Windows.Forms.RadioButton radioSaveAnalysis;
        private System.Windows.Forms.RadioButton radioSaveVideo;
        private System.Windows.Forms.RadioButton radioSaveBoth;
        private System.Windows.Forms.GroupBox groupOptions;
        private System.Windows.Forms.CheckBox checkBlendDrawings;
        private System.Windows.Forms.CheckBox checkSlowMotion;
    }
}