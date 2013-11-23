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
            this.plotVelocity = new OxyPlot.WindowsForms.Plot();
            this.plotAcceleration = new OxyPlot.WindowsForms.Plot();
            this.plotCoordinates = new OxyPlot.WindowsForms.Plot();
            this.SuspendLayout();
            // 
            // plotVelocity
            // 
            this.plotVelocity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.plotVelocity.BackColor = System.Drawing.Color.WhiteSmoke;
            this.plotVelocity.KeyboardPanHorizontalStep = 0.1D;
            this.plotVelocity.KeyboardPanVerticalStep = 0.1D;
            this.plotVelocity.Location = new System.Drawing.Point(12, 268);
            this.plotVelocity.Name = "plotVelocity";
            this.plotVelocity.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plotVelocity.Size = new System.Drawing.Size(648, 250);
            this.plotVelocity.TabIndex = 0;
            this.plotVelocity.Text = "plot1";
            this.plotVelocity.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotVelocity.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotVelocity.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // plotAcceleration
            // 
            this.plotAcceleration.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.plotAcceleration.BackColor = System.Drawing.Color.WhiteSmoke;
            this.plotAcceleration.KeyboardPanHorizontalStep = 0.1D;
            this.plotAcceleration.KeyboardPanVerticalStep = 0.1D;
            this.plotAcceleration.Location = new System.Drawing.Point(12, 524);
            this.plotAcceleration.Name = "plotAcceleration";
            this.plotAcceleration.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plotAcceleration.Size = new System.Drawing.Size(648, 250);
            this.plotAcceleration.TabIndex = 1;
            this.plotAcceleration.Text = "plot1";
            this.plotAcceleration.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotAcceleration.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotAcceleration.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // plotCoordinates
            // 
            this.plotCoordinates.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.plotCoordinates.BackColor = System.Drawing.Color.WhiteSmoke;
            this.plotCoordinates.KeyboardPanHorizontalStep = 0.1D;
            this.plotCoordinates.KeyboardPanVerticalStep = 0.1D;
            this.plotCoordinates.Location = new System.Drawing.Point(12, 12);
            this.plotCoordinates.Name = "plotCoordinates";
            this.plotCoordinates.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plotCoordinates.Size = new System.Drawing.Size(648, 250);
            this.plotCoordinates.TabIndex = 2;
            this.plotCoordinates.Text = "plot1";
            this.plotCoordinates.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotCoordinates.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotCoordinates.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // FormTimeseries
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(672, 785);
            this.Controls.Add(this.plotCoordinates);
            this.Controls.Add(this.plotAcceleration);
            this.Controls.Add(this.plotVelocity);
            this.Name = "FormTimeseries";
            this.Text = "FormTimeseries";
            this.ResumeLayout(false);

        }

        #endregion

        private OxyPlot.WindowsForms.Plot plotVelocity;
        private OxyPlot.WindowsForms.Plot plotAcceleration;
        private OxyPlot.WindowsForms.Plot plotCoordinates;
    }
}