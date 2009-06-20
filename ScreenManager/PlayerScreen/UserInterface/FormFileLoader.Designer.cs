
using System.Windows.Forms;


namespace Kinovea.ScreenManager
{
    partial class formFileLoader
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
        	this.progressBar = new System.Windows.Forms.ProgressBar();
        	this.labelInfos = new System.Windows.Forms.Label();
        	this.buttonCancel = new System.Windows.Forms.Button();
        	this.bgMovieLoader = new System.ComponentModel.BackgroundWorker();
        	this.SuspendLayout();
        	// 
        	// progressBar
        	// 
        	this.progressBar.Location = new System.Drawing.Point(23, 13);
        	this.progressBar.Name = "progressBar";
        	this.progressBar.Size = new System.Drawing.Size(335, 22);
        	this.progressBar.Step = 1;
        	this.progressBar.TabIndex = 0;
        	// 
        	// labelInfos
        	// 
        	this.labelInfos.AutoSize = true;
        	this.labelInfos.Location = new System.Drawing.Point(20, 49);
        	this.labelInfos.Name = "labelInfos";
        	this.labelInfos.Size = new System.Drawing.Size(35, 13);
        	this.labelInfos.TabIndex = 1;
        	this.labelInfos.Text = "label1";
        	// 
        	// buttonCancel
        	// 
        	this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        	this.buttonCancel.Location = new System.Drawing.Point(273, 43);
        	this.buttonCancel.Name = "buttonCancel";
        	this.buttonCancel.Size = new System.Drawing.Size(85, 22);
        	this.buttonCancel.TabIndex = 2;
        	this.buttonCancel.Text = "Cancel";
        	this.buttonCancel.UseVisualStyleBackColor = true;
        	this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
        	// 
        	// bgMovieLoader
        	// 
        	this.bgMovieLoader.WorkerReportsProgress = true;
        	this.bgMovieLoader.WorkerSupportsCancellation = true;
        	this.bgMovieLoader.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgMovieLoader_DoWork);
        	this.bgMovieLoader.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgMovieLoader_RunWorkerCompleted);
        	this.bgMovieLoader.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgMovieLoader_ProgressChanged);
        	// 
        	// formFileLoader
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.ClientSize = new System.Drawing.Size(369, 76);
        	this.ControlBox = false;
        	this.Controls.Add(this.buttonCancel);
        	this.Controls.Add(this.labelInfos);
        	this.Controls.Add(this.progressBar);
        	this.DoubleBuffered = true;
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
        	this.MaximizeBox = false;
        	this.MinimizeBox = false;
        	this.Name = "formFileLoader";
        	this.ShowIcon = false;
        	this.ShowInTaskbar = false;
        	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        	this.Text = "File Load Progression";
        	this.ResumeLayout(false);
        	this.PerformLayout();
        }

        #endregion

        public System.Windows.Forms.ProgressBar progressBar;
        public Label labelInfos;
        private Button buttonCancel;
        private System.ComponentModel.BackgroundWorker bgMovieLoader;

    }
}