namespace Kinovea.ScreenManager
{
    partial class FormPointsAnalysis
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
            this.plotScatter = new OxyPlot.WindowsForms.Plot();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.pagePlot = new System.Windows.Forms.TabPage();
            this.tabControl.SuspendLayout();
            this.pagePlot.SuspendLayout();
            this.SuspendLayout();
            // 
            // plotScatter
            // 
            this.plotScatter.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.plotScatter.BackColor = System.Drawing.Color.White;
            this.plotScatter.KeyboardPanHorizontalStep = 0.1D;
            this.plotScatter.KeyboardPanVerticalStep = 0.1D;
            this.plotScatter.Location = new System.Drawing.Point(16, 16);
            this.plotScatter.Name = "plotScatter";
            this.plotScatter.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plotScatter.Size = new System.Drawing.Size(776, 471);
            this.plotScatter.TabIndex = 0;
            this.plotScatter.Text = "plot";
            this.plotScatter.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotScatter.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotScatter.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.pagePlot);
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(820, 533);
            this.tabControl.TabIndex = 1;
            // 
            // pagePlot
            // 
            this.pagePlot.Controls.Add(this.plotScatter);
            this.pagePlot.Location = new System.Drawing.Point(4, 22);
            this.pagePlot.Name = "pagePlot";
            this.pagePlot.Padding = new System.Windows.Forms.Padding(3);
            this.pagePlot.Size = new System.Drawing.Size(812, 507);
            this.pagePlot.TabIndex = 0;
            this.pagePlot.Text = "Plot";
            this.pagePlot.UseVisualStyleBackColor = true;
            // 
            // FormPointsAnalysis
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(844, 557);
            this.Controls.Add(this.tabControl);
            this.Name = "FormPointsAnalysis";
            this.Text = "Data Analysis";
            this.tabControl.ResumeLayout(false);
            this.pagePlot.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private OxyPlot.WindowsForms.Plot plotScatter;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage pagePlot;
    }
}