namespace Kinovea.ScreenManager
{
    partial class FormAngleAngleAnalysis
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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.pagePlot = new System.Windows.Forms.TabPage();
            this.plotView = new OxyPlot.WindowsForms.PlotView();
            this.gbLabels = new System.Windows.Forms.GroupBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.tbYAxis = new System.Windows.Forms.TextBox();
            this.tbXAxis = new System.Windows.Forms.TextBox();
            this.lblYAxis = new System.Windows.Forms.Label();
            this.lblXAxis = new System.Windows.Forms.Label();
            this.tbTitle = new System.Windows.Forms.TextBox();
            this.gbExportData = new System.Windows.Forms.GroupBox();
            this.btnDataCopy = new System.Windows.Forms.Button();
            this.btnExportData = new System.Windows.Forms.Button();
            this.gbExportGraph = new System.Windows.Forms.GroupBox();
            this.btnImageCopy = new System.Windows.Forms.Button();
            this.btnExportGraph = new System.Windows.Forms.Button();
            this.lblPixels = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.nudHeight = new System.Windows.Forms.NumericUpDown();
            this.nudWidth = new System.Windows.Forms.NumericUpDown();
            this.gbSource = new System.Windows.Forms.GroupBox();
            this.cbSourceY = new System.Windows.Forms.ComboBox();
            this.lblSourceYAxis = new System.Windows.Forms.Label();
            this.cbSourceX = new System.Windows.Forms.ComboBox();
            this.lblSourceXAxis = new System.Windows.Forms.Label();
            this.cmbDataSource = new System.Windows.Forms.ComboBox();
            this.lblData = new System.Windows.Forms.Label();
            this.tabControl.SuspendLayout();
            this.pagePlot.SuspendLayout();
            this.gbLabels.SuspendLayout();
            this.gbExportData.SuspendLayout();
            this.gbExportGraph.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWidth)).BeginInit();
            this.gbSource.SuspendLayout();
            this.SuspendLayout();
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
            this.tabControl.Size = new System.Drawing.Size(742, 657);
            this.tabControl.TabIndex = 2;
            // 
            // pagePlot
            // 
            this.pagePlot.Controls.Add(this.plotView);
            this.pagePlot.Location = new System.Drawing.Point(4, 22);
            this.pagePlot.Name = "pagePlot";
            this.pagePlot.Padding = new System.Windows.Forms.Padding(3);
            this.pagePlot.Size = new System.Drawing.Size(734, 631);
            this.pagePlot.TabIndex = 0;
            this.pagePlot.Text = "Plot";
            this.pagePlot.UseVisualStyleBackColor = true;
            // 
            // plotView
            // 
            this.plotView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.plotView.BackColor = System.Drawing.Color.White;
            this.plotView.Location = new System.Drawing.Point(16, 15);
            this.plotView.Name = "plotView";
            this.plotView.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plotView.Size = new System.Drawing.Size(703, 604);
            this.plotView.TabIndex = 0;
            this.plotView.Text = "plotView1";
            this.plotView.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotView.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotView.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // gbLabels
            // 
            this.gbLabels.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gbLabels.Controls.Add(this.lblTitle);
            this.gbLabels.Controls.Add(this.tbYAxis);
            this.gbLabels.Controls.Add(this.tbXAxis);
            this.gbLabels.Controls.Add(this.lblYAxis);
            this.gbLabels.Controls.Add(this.lblXAxis);
            this.gbLabels.Controls.Add(this.tbTitle);
            this.gbLabels.Location = new System.Drawing.Point(760, 179);
            this.gbLabels.Name = "gbLabels";
            this.gbLabels.Size = new System.Drawing.Size(236, 138);
            this.gbLabels.TabIndex = 7;
            this.gbLabels.TabStop = false;
            this.gbLabels.Text = "Labels";
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(15, 33);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(33, 13);
            this.lblTitle.TabIndex = 5;
            this.lblTitle.Text = "Title :";
            // 
            // tbYAxis
            // 
            this.tbYAxis.Location = new System.Drawing.Point(63, 102);
            this.tbYAxis.Name = "tbYAxis";
            this.tbYAxis.Size = new System.Drawing.Size(154, 20);
            this.tbYAxis.TabIndex = 4;
            this.tbYAxis.Text = "Y axis";
            this.tbYAxis.TextChanged += new System.EventHandler(this.LabelsChanged);
            // 
            // tbXAxis
            // 
            this.tbXAxis.Location = new System.Drawing.Point(63, 70);
            this.tbXAxis.Name = "tbXAxis";
            this.tbXAxis.Size = new System.Drawing.Size(154, 20);
            this.tbXAxis.TabIndex = 3;
            this.tbXAxis.Text = "X axis";
            this.tbXAxis.TextChanged += new System.EventHandler(this.LabelsChanged);
            // 
            // lblYAxis
            // 
            this.lblYAxis.AutoSize = true;
            this.lblYAxis.Location = new System.Drawing.Point(15, 105);
            this.lblYAxis.Name = "lblYAxis";
            this.lblYAxis.Size = new System.Drawing.Size(41, 13);
            this.lblYAxis.TabIndex = 2;
            this.lblYAxis.Text = "Y axis :";
            // 
            // lblXAxis
            // 
            this.lblXAxis.AutoSize = true;
            this.lblXAxis.Location = new System.Drawing.Point(15, 73);
            this.lblXAxis.Name = "lblXAxis";
            this.lblXAxis.Size = new System.Drawing.Size(41, 13);
            this.lblXAxis.TabIndex = 1;
            this.lblXAxis.Text = "X axis :";
            // 
            // tbTitle
            // 
            this.tbTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbTitle.Location = new System.Drawing.Point(63, 30);
            this.tbTitle.Name = "tbTitle";
            this.tbTitle.Size = new System.Drawing.Size(154, 20);
            this.tbTitle.TabIndex = 0;
            this.tbTitle.Text = "Plot title";
            this.tbTitle.TextChanged += new System.EventHandler(this.LabelsChanged);
            // 
            // gbExportData
            // 
            this.gbExportData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.gbExportData.Controls.Add(this.btnDataCopy);
            this.gbExportData.Controls.Add(this.btnExportData);
            this.gbExportData.Location = new System.Drawing.Point(760, 547);
            this.gbExportData.Name = "gbExportData";
            this.gbExportData.Size = new System.Drawing.Size(236, 118);
            this.gbExportData.TabIndex = 6;
            this.gbExportData.TabStop = false;
            this.gbExportData.Text = "Export data";
            // 
            // btnDataCopy
            // 
            this.btnDataCopy.Location = new System.Drawing.Point(18, 34);
            this.btnDataCopy.Name = "btnDataCopy";
            this.btnDataCopy.Size = new System.Drawing.Size(199, 23);
            this.btnDataCopy.TabIndex = 6;
            this.btnDataCopy.Text = "Copy to Clipboard";
            this.btnDataCopy.UseVisualStyleBackColor = true;
            this.btnDataCopy.Click += new System.EventHandler(this.btnDataCopy_Click);
            // 
            // btnExportData
            // 
            this.btnExportData.Location = new System.Drawing.Point(18, 74);
            this.btnExportData.Name = "btnExportData";
            this.btnExportData.Size = new System.Drawing.Size(199, 23);
            this.btnExportData.TabIndex = 5;
            this.btnExportData.Text = "Save to file";
            this.btnExportData.UseVisualStyleBackColor = true;
            this.btnExportData.Click += new System.EventHandler(this.btnExportData_Click);
            // 
            // gbExportGraph
            // 
            this.gbExportGraph.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.gbExportGraph.Controls.Add(this.btnImageCopy);
            this.gbExportGraph.Controls.Add(this.btnExportGraph);
            this.gbExportGraph.Controls.Add(this.lblPixels);
            this.gbExportGraph.Controls.Add(this.label1);
            this.gbExportGraph.Controls.Add(this.nudHeight);
            this.gbExportGraph.Controls.Add(this.nudWidth);
            this.gbExportGraph.Location = new System.Drawing.Point(760, 386);
            this.gbExportGraph.Name = "gbExportGraph";
            this.gbExportGraph.Size = new System.Drawing.Size(236, 155);
            this.gbExportGraph.TabIndex = 5;
            this.gbExportGraph.TabStop = false;
            this.gbExportGraph.Text = "Export graph";
            // 
            // btnImageCopy
            // 
            this.btnImageCopy.Location = new System.Drawing.Point(18, 73);
            this.btnImageCopy.Name = "btnImageCopy";
            this.btnImageCopy.Size = new System.Drawing.Size(199, 23);
            this.btnImageCopy.TabIndex = 7;
            this.btnImageCopy.Text = "Copy to Clipboard";
            this.btnImageCopy.UseVisualStyleBackColor = true;
            this.btnImageCopy.Click += new System.EventHandler(this.btnImageCopy_Click);
            // 
            // btnExportGraph
            // 
            this.btnExportGraph.Location = new System.Drawing.Point(18, 115);
            this.btnExportGraph.Name = "btnExportGraph";
            this.btnExportGraph.Size = new System.Drawing.Size(199, 23);
            this.btnExportGraph.TabIndex = 4;
            this.btnExportGraph.Text = "Save to file";
            this.btnExportGraph.UseVisualStyleBackColor = true;
            this.btnExportGraph.Click += new System.EventHandler(this.btnExportGraph_Click);
            // 
            // lblPixels
            // 
            this.lblPixels.AutoSize = true;
            this.lblPixels.Location = new System.Drawing.Point(169, 34);
            this.lblPixels.Name = "lblPixels";
            this.lblPixels.Size = new System.Drawing.Size(33, 13);
            this.lblPixels.TabIndex = 3;
            this.lblPixels.Text = "pixels";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(91, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(13, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "×";
            // 
            // nudHeight
            // 
            this.nudHeight.Location = new System.Drawing.Point(110, 30);
            this.nudHeight.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
            this.nudHeight.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudHeight.Name = "nudHeight";
            this.nudHeight.Size = new System.Drawing.Size(51, 20);
            this.nudHeight.TabIndex = 1;
            this.nudHeight.Value = new decimal(new int[] {
            512,
            0,
            0,
            0});
            // 
            // nudWidth
            // 
            this.nudWidth.Location = new System.Drawing.Point(34, 30);
            this.nudWidth.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
            this.nudWidth.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudWidth.Name = "nudWidth";
            this.nudWidth.Size = new System.Drawing.Size(51, 20);
            this.nudWidth.TabIndex = 0;
            this.nudWidth.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            // 
            // gbSource
            // 
            this.gbSource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gbSource.Controls.Add(this.cbSourceY);
            this.gbSource.Controls.Add(this.lblSourceYAxis);
            this.gbSource.Controls.Add(this.cbSourceX);
            this.gbSource.Controls.Add(this.lblSourceXAxis);
            this.gbSource.Controls.Add(this.cmbDataSource);
            this.gbSource.Controls.Add(this.lblData);
            this.gbSource.Location = new System.Drawing.Point(760, 34);
            this.gbSource.Name = "gbSource";
            this.gbSource.Size = new System.Drawing.Size(236, 139);
            this.gbSource.TabIndex = 8;
            this.gbSource.TabStop = false;
            this.gbSource.Text = "Source";
            // 
            // cbSourceY
            // 
            this.cbSourceY.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbSourceY.FormattingEnabled = true;
            this.cbSourceY.Location = new System.Drawing.Point(63, 66);
            this.cbSourceY.Name = "cbSourceY";
            this.cbSourceY.Size = new System.Drawing.Size(154, 21);
            this.cbSourceY.TabIndex = 10;
            this.cbSourceY.SelectedIndexChanged += new System.EventHandler(this.PlotOption_Changed);
            // 
            // lblSourceYAxis
            // 
            this.lblSourceYAxis.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSourceYAxis.AutoSize = true;
            this.lblSourceYAxis.Location = new System.Drawing.Point(15, 69);
            this.lblSourceYAxis.Name = "lblSourceYAxis";
            this.lblSourceYAxis.Size = new System.Drawing.Size(41, 13);
            this.lblSourceYAxis.TabIndex = 9;
            this.lblSourceYAxis.Text = "Y axis :";
            // 
            // cbSourceX
            // 
            this.cbSourceX.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbSourceX.FormattingEnabled = true;
            this.cbSourceX.Location = new System.Drawing.Point(63, 29);
            this.cbSourceX.Name = "cbSourceX";
            this.cbSourceX.Size = new System.Drawing.Size(154, 21);
            this.cbSourceX.TabIndex = 8;
            this.cbSourceX.SelectedIndexChanged += new System.EventHandler(this.PlotOption_Changed);
            // 
            // lblSourceXAxis
            // 
            this.lblSourceXAxis.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSourceXAxis.AutoSize = true;
            this.lblSourceXAxis.Location = new System.Drawing.Point(15, 32);
            this.lblSourceXAxis.Name = "lblSourceXAxis";
            this.lblSourceXAxis.Size = new System.Drawing.Size(41, 13);
            this.lblSourceXAxis.TabIndex = 7;
            this.lblSourceXAxis.Text = "X axis :";
            // 
            // cmbDataSource
            // 
            this.cmbDataSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbDataSource.FormattingEnabled = true;
            this.cmbDataSource.Location = new System.Drawing.Point(63, 104);
            this.cmbDataSource.Name = "cmbDataSource";
            this.cmbDataSource.Size = new System.Drawing.Size(154, 21);
            this.cmbDataSource.TabIndex = 6;
            this.cmbDataSource.SelectedIndexChanged += new System.EventHandler(this.PlotOption_Changed);
            // 
            // lblData
            // 
            this.lblData.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblData.AutoSize = true;
            this.lblData.Location = new System.Drawing.Point(15, 107);
            this.lblData.Name = "lblData";
            this.lblData.Size = new System.Drawing.Size(36, 13);
            this.lblData.TabIndex = 5;
            this.lblData.Text = "Data :";
            // 
            // FormAngleAngleAnalysis
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1008, 681);
            this.Controls.Add(this.gbSource);
            this.Controls.Add(this.gbLabels);
            this.Controls.Add(this.gbExportData);
            this.Controls.Add(this.gbExportGraph);
            this.Controls.Add(this.tabControl);
            this.MinimumSize = new System.Drawing.Size(720, 720);
            this.Name = "FormAngleAngleAnalysis";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Angle-Angle diagrams";
            this.tabControl.ResumeLayout(false);
            this.pagePlot.ResumeLayout(false);
            this.gbLabels.ResumeLayout(false);
            this.gbLabels.PerformLayout();
            this.gbExportData.ResumeLayout(false);
            this.gbExportGraph.ResumeLayout(false);
            this.gbExportGraph.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWidth)).EndInit();
            this.gbSource.ResumeLayout(false);
            this.gbSource.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage pagePlot;
        private OxyPlot.WindowsForms.PlotView plotView;
        private System.Windows.Forms.GroupBox gbLabels;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TextBox tbYAxis;
        private System.Windows.Forms.TextBox tbXAxis;
        private System.Windows.Forms.Label lblYAxis;
        private System.Windows.Forms.Label lblXAxis;
        private System.Windows.Forms.TextBox tbTitle;
        private System.Windows.Forms.GroupBox gbExportData;
        private System.Windows.Forms.Button btnDataCopy;
        private System.Windows.Forms.Button btnExportData;
        private System.Windows.Forms.GroupBox gbExportGraph;
        private System.Windows.Forms.Button btnImageCopy;
        private System.Windows.Forms.Button btnExportGraph;
        private System.Windows.Forms.Label lblPixels;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nudHeight;
        private System.Windows.Forms.NumericUpDown nudWidth;
        private System.Windows.Forms.GroupBox gbSource;
        private System.Windows.Forms.ComboBox cmbDataSource;
        private System.Windows.Forms.Label lblData;
        private System.Windows.Forms.ComboBox cbSourceY;
        private System.Windows.Forms.Label lblSourceYAxis;
        private System.Windows.Forms.ComboBox cbSourceX;
        private System.Windows.Forms.Label lblSourceXAxis;
    }
}