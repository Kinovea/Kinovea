namespace Kinovea.ScreenManager
{
    partial class formFramesImport
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
        	this.bgWorker = new System.ComponentModel.BackgroundWorker();
        	this.buttonCancel = new System.Windows.Forms.Button();
        	this.SuspendLayout();
        	// 
        	// progressBar
        	// 
        	this.progressBar.Location = new System.Drawing.Point(20, 12);
        	this.progressBar.Name = "progressBar";
        	this.progressBar.Size = new System.Drawing.Size(335, 22);
        	this.progressBar.Step = 1;
        	this.progressBar.TabIndex = 1;
        	this.progressBar.UseWaitCursor = true;
        	// 
        	// labelInfos
        	// 
        	this.labelInfos.AutoSize = true;
        	this.labelInfos.Location = new System.Drawing.Point(17, 47);
        	this.labelInfos.Name = "labelInfos";
        	this.labelInfos.Size = new System.Drawing.Size(36, 13);
        	this.labelInfos.TabIndex = 2;
        	this.labelInfos.Text = "[Infos]";
        	this.labelInfos.UseWaitCursor = true;
        	// 
        	// bgWorker
        	// 
        	this.bgWorker.WorkerReportsProgress = true;
        	this.bgWorker.WorkerSupportsCancellation = true;
        	this.bgWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgWorker_DoWork);
        	this.bgWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgWorker_RunWorkerCompleted);
        	this.bgWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgWorker_ProgressChanged);
        	// 
        	// buttonCancel
        	// 
        	this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        	this.buttonCancel.Location = new System.Drawing.Point(270, 42);
        	this.buttonCancel.Name = "buttonCancel";
        	this.buttonCancel.Size = new System.Drawing.Size(85, 22);
        	this.buttonCancel.TabIndex = 3;
        	this.buttonCancel.Text = "Cancel";
        	this.buttonCancel.UseVisualStyleBackColor = true;
        	this.buttonCancel.UseWaitCursor = true;
        	this.buttonCancel.Click += new System.EventHandler(this.ButtonCancelClick);
        	// 
        	// formFramesImport
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.CancelButton = this.buttonCancel;
        	this.ClientSize = new System.Drawing.Size(369, 76);
        	this.ControlBox = false;
        	this.Controls.Add(this.buttonCancel);
        	this.Controls.Add(this.labelInfos);
        	this.Controls.Add(this.progressBar);
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
        	this.MaximizeBox = false;
        	this.MinimizeBox = false;
        	this.Name = "formFramesImport";
        	this.Opacity = 0.9;
        	this.ShowIcon = false;
        	this.ShowInTaskbar = false;
        	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        	this.Text = "   Importing Frames...";
        	this.UseWaitCursor = true;
        	this.Load += new System.EventHandler(this.formFramesImport_Load);
        	this.ResumeLayout(false);
        	this.PerformLayout();
        }
        private System.Windows.Forms.Button buttonCancel;

        #endregion

        public System.Windows.Forms.ProgressBar progressBar;
        public System.Windows.Forms.Label labelInfos;
        private System.ComponentModel.BackgroundWorker bgWorker;
    }
}