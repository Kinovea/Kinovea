namespace Kinovea.ScreenManager
{
    partial class FormTimeseries
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
            this.plotHorzVelocity = new OxyPlot.WindowsForms.Plot();
            this.plotHorzAcceleration = new OxyPlot.WindowsForms.Plot();
            this.plotCoordinates = new OxyPlot.WindowsForms.Plot();
            this.plotDurbinWatson = new OxyPlot.WindowsForms.Plot();
            this.SuspendLayout();
            // 
            // plotHorzVelocity
            // 
            this.plotHorzVelocity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.plotHorzVelocity.BackColor = System.Drawing.Color.WhiteSmoke;
            this.plotHorzVelocity.KeyboardPanHorizontalStep = 0.1D;
            this.plotHorzVelocity.KeyboardPanVerticalStep = 0.1D;
            this.plotHorzVelocity.Location = new System.Drawing.Point(12, 276);
            this.plotHorzVelocity.Name = "plotHorzVelocity";
            this.plotHorzVelocity.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plotHorzVelocity.Size = new System.Drawing.Size(710, 218);
            this.plotHorzVelocity.TabIndex = 0;
            this.plotHorzVelocity.Text = "plot1";
            this.plotHorzVelocity.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotHorzVelocity.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotHorzVelocity.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // plotHorzAcceleration
            // 
            this.plotHorzAcceleration.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.plotHorzAcceleration.BackColor = System.Drawing.Color.WhiteSmoke;
            this.plotHorzAcceleration.KeyboardPanHorizontalStep = 0.1D;
            this.plotHorzAcceleration.KeyboardPanVerticalStep = 0.1D;
            this.plotHorzAcceleration.Location = new System.Drawing.Point(12, 500);
            this.plotHorzAcceleration.Name = "plotHorzAcceleration";
            this.plotHorzAcceleration.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plotHorzAcceleration.Size = new System.Drawing.Size(710, 250);
            this.plotHorzAcceleration.TabIndex = 1;
            this.plotHorzAcceleration.Text = "plot1";
            this.plotHorzAcceleration.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotHorzAcceleration.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotHorzAcceleration.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // plotCoordinates
            // 
            this.plotCoordinates.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.plotCoordinates.BackColor = System.Drawing.Color.WhiteSmoke;
            this.plotCoordinates.KeyboardPanHorizontalStep = 0.1D;
            this.plotCoordinates.KeyboardPanVerticalStep = 0.1D;
            this.plotCoordinates.Location = new System.Drawing.Point(372, 12);
            this.plotCoordinates.Name = "plotCoordinates";
            this.plotCoordinates.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plotCoordinates.Size = new System.Drawing.Size(350, 258);
            this.plotCoordinates.TabIndex = 2;
            this.plotCoordinates.Text = "plot1";
            this.plotCoordinates.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotCoordinates.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotCoordinates.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // plotDurbinWatson
            // 
            this.plotDurbinWatson.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.plotDurbinWatson.BackColor = System.Drawing.Color.WhiteSmoke;
            this.plotDurbinWatson.KeyboardPanHorizontalStep = 0.1D;
            this.plotDurbinWatson.KeyboardPanVerticalStep = 0.1D;
            this.plotDurbinWatson.Location = new System.Drawing.Point(12, 12);
            this.plotDurbinWatson.Name = "plotDurbinWatson";
            this.plotDurbinWatson.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plotDurbinWatson.Size = new System.Drawing.Size(350, 258);
            this.plotDurbinWatson.TabIndex = 4;
            this.plotDurbinWatson.Text = "plot1";
            this.plotDurbinWatson.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotDurbinWatson.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotDurbinWatson.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // FormTimeseries
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(734, 762);
            this.Controls.Add(this.plotDurbinWatson);
            this.Controls.Add(this.plotCoordinates);
            this.Controls.Add(this.plotHorzAcceleration);
            this.Controls.Add(this.plotHorzVelocity);
            this.Name = "FormTimeseries";
            this.Text = "FormTimeseries";
            this.ResumeLayout(false);

        }

        #endregion

        private OxyPlot.WindowsForms.Plot plotHorzVelocity;
        private OxyPlot.WindowsForms.Plot plotHorzAcceleration;
        private OxyPlot.WindowsForms.Plot plotCoordinates;
        private OxyPlot.WindowsForms.Plot plotDurbinWatson;
    }
}