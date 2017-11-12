namespace Kinovea.ScreenManager
{
    partial class FormMultiTrajectoryAnalysis
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
            this.pageAbout = new System.Windows.Forms.TabPage();
            this.lvCutoffFrequencies = new System.Windows.Forms.ListView();
            this.chSource = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chX = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chY = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.rtbInfo1 = new System.Windows.Forms.RichTextBox();
            this.lblCutoffFrequencies = new System.Windows.Forms.Label();
            this.plotDurbinWatson = new OxyPlot.WindowsForms.PlotView();
            this.rtbInfo2 = new System.Windows.Forms.RichTextBox();
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
            this.cmbTimeModel = new System.Windows.Forms.ComboBox();
            this.lblTimeModel = new System.Windows.Forms.Label();
            this.clbSources = new System.Windows.Forms.CheckedListBox();
            this.cmbPlotSpec = new System.Windows.Forms.ComboBox();
            this.lblData = new System.Windows.Forms.Label();
            this.tabControl.SuspendLayout();
            this.pagePlot.SuspendLayout();
            this.pageAbout.SuspendLayout();
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
            this.tabControl.Controls.Add(this.pageAbout);
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(766, 657);
            this.tabControl.TabIndex = 2;
            // 
            // pagePlot
            // 
            this.pagePlot.Controls.Add(this.plotView);
            this.pagePlot.Location = new System.Drawing.Point(4, 22);
            this.pagePlot.Name = "pagePlot";
            this.pagePlot.Padding = new System.Windows.Forms.Padding(3);
            this.pagePlot.Size = new System.Drawing.Size(758, 631);
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
            this.plotView.Size = new System.Drawing.Size(727, 604);
            this.plotView.TabIndex = 0;
            this.plotView.Text = "plotView1";
            this.plotView.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotView.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotView.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // pageAbout
            // 
            this.pageAbout.Controls.Add(this.lvCutoffFrequencies);
            this.pageAbout.Controls.Add(this.rtbInfo1);
            this.pageAbout.Controls.Add(this.lblCutoffFrequencies);
            this.pageAbout.Controls.Add(this.plotDurbinWatson);
            this.pageAbout.Controls.Add(this.rtbInfo2);
            this.pageAbout.Location = new System.Drawing.Point(4, 22);
            this.pageAbout.Name = "pageAbout";
            this.pageAbout.Padding = new System.Windows.Forms.Padding(3);
            this.pageAbout.Size = new System.Drawing.Size(758, 631);
            this.pageAbout.TabIndex = 1;
            this.pageAbout.Text = "About";
            this.pageAbout.UseVisualStyleBackColor = true;
            // 
            // lvCutoffFrequencies
            // 
            this.lvCutoffFrequencies.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvCutoffFrequencies.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lvCutoffFrequencies.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chSource,
            this.chX,
            this.chY});
            this.lvCutoffFrequencies.GridLines = true;
            this.lvCutoffFrequencies.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvCutoffFrequencies.Location = new System.Drawing.Point(490, 236);
            this.lvCutoffFrequencies.Name = "lvCutoffFrequencies";
            this.lvCutoffFrequencies.Size = new System.Drawing.Size(250, 207);
            this.lvCutoffFrequencies.TabIndex = 6;
            this.lvCutoffFrequencies.UseCompatibleStateImageBehavior = false;
            this.lvCutoffFrequencies.View = System.Windows.Forms.View.Details;
            // 
            // chSource
            // 
            this.chSource.Text = "Source";
            this.chSource.Width = 100;
            // 
            // chX
            // 
            this.chX.Text = "X";
            this.chX.Width = 73;
            // 
            // chY
            // 
            this.chY.Text = "Y";
            this.chY.Width = 73;
            // 
            // rtbInfo1
            // 
            this.rtbInfo1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbInfo1.BackColor = System.Drawing.Color.Silver;
            this.rtbInfo1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbInfo1.Location = new System.Drawing.Point(16, 7);
            this.rtbInfo1.Name = "rtbInfo1";
            this.rtbInfo1.Size = new System.Drawing.Size(724, 163);
            this.rtbInfo1.TabIndex = 0;
            this.rtbInfo1.Text = "";
            // 
            // lblCutoffFrequencies
            // 
            this.lblCutoffFrequencies.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCutoffFrequencies.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCutoffFrequencies.Location = new System.Drawing.Point(490, 211);
            this.lblCutoffFrequencies.Name = "lblCutoffFrequencies";
            this.lblCutoffFrequencies.Size = new System.Drawing.Size(250, 20);
            this.lblCutoffFrequencies.TabIndex = 3;
            this.lblCutoffFrequencies.Text = "Cutoff frequencies";
            this.lblCutoffFrequencies.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // plotDurbinWatson
            // 
            this.plotDurbinWatson.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.plotDurbinWatson.Location = new System.Drawing.Point(16, 203);
            this.plotDurbinWatson.Name = "plotDurbinWatson";
            this.plotDurbinWatson.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plotDurbinWatson.Size = new System.Drawing.Size(465, 299);
            this.plotDurbinWatson.TabIndex = 2;
            this.plotDurbinWatson.Text = "plotView1";
            this.plotDurbinWatson.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotDurbinWatson.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotDurbinWatson.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // rtbInfo2
            // 
            this.rtbInfo2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbInfo2.BackColor = System.Drawing.Color.Silver;
            this.rtbInfo2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbInfo2.Location = new System.Drawing.Point(16, 530);
            this.rtbInfo2.Name = "rtbInfo2";
            this.rtbInfo2.Size = new System.Drawing.Size(724, 95);
            this.rtbInfo2.TabIndex = 1;
            this.rtbInfo2.Text = "";
            // 
            // gbLabels
            // 
            this.gbLabels.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.gbLabels.Controls.Add(this.lblTitle);
            this.gbLabels.Controls.Add(this.tbYAxis);
            this.gbLabels.Controls.Add(this.tbXAxis);
            this.gbLabels.Controls.Add(this.lblYAxis);
            this.gbLabels.Controls.Add(this.lblXAxis);
            this.gbLabels.Controls.Add(this.tbTitle);
            this.gbLabels.Location = new System.Drawing.Point(784, 240);
            this.gbLabels.Name = "gbLabels";
            this.gbLabels.Size = new System.Drawing.Size(212, 138);
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
            this.tbYAxis.Size = new System.Drawing.Size(130, 20);
            this.tbYAxis.TabIndex = 4;
            this.tbYAxis.Text = "Y axis";
            this.tbYAxis.TextChanged += new System.EventHandler(this.LabelsChanged);
            // 
            // tbXAxis
            // 
            this.tbXAxis.Location = new System.Drawing.Point(63, 70);
            this.tbXAxis.Name = "tbXAxis";
            this.tbXAxis.Size = new System.Drawing.Size(130, 20);
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
            this.tbTitle.Size = new System.Drawing.Size(130, 20);
            this.tbTitle.TabIndex = 0;
            this.tbTitle.Text = "Plot title";
            this.tbTitle.TextChanged += new System.EventHandler(this.LabelsChanged);
            // 
            // gbExportData
            // 
            this.gbExportData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.gbExportData.Controls.Add(this.btnDataCopy);
            this.gbExportData.Controls.Add(this.btnExportData);
            this.gbExportData.Location = new System.Drawing.Point(784, 547);
            this.gbExportData.Name = "gbExportData";
            this.gbExportData.Size = new System.Drawing.Size(212, 118);
            this.gbExportData.TabIndex = 6;
            this.gbExportData.TabStop = false;
            this.gbExportData.Text = "Export data";
            // 
            // btnDataCopy
            // 
            this.btnDataCopy.Location = new System.Drawing.Point(18, 34);
            this.btnDataCopy.Name = "btnDataCopy";
            this.btnDataCopy.Size = new System.Drawing.Size(175, 23);
            this.btnDataCopy.TabIndex = 6;
            this.btnDataCopy.Text = "Copy to Clipboard";
            this.btnDataCopy.UseVisualStyleBackColor = true;
            this.btnDataCopy.Click += new System.EventHandler(this.btnDataCopy_Click);
            // 
            // btnExportData
            // 
            this.btnExportData.Location = new System.Drawing.Point(18, 74);
            this.btnExportData.Name = "btnExportData";
            this.btnExportData.Size = new System.Drawing.Size(175, 23);
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
            this.gbExportGraph.Location = new System.Drawing.Point(784, 386);
            this.gbExportGraph.Name = "gbExportGraph";
            this.gbExportGraph.Size = new System.Drawing.Size(212, 155);
            this.gbExportGraph.TabIndex = 5;
            this.gbExportGraph.TabStop = false;
            this.gbExportGraph.Text = "Export graph";
            // 
            // btnImageCopy
            // 
            this.btnImageCopy.Location = new System.Drawing.Point(18, 73);
            this.btnImageCopy.Name = "btnImageCopy";
            this.btnImageCopy.Size = new System.Drawing.Size(175, 23);
            this.btnImageCopy.TabIndex = 7;
            this.btnImageCopy.Text = "Copy to Clipboard";
            this.btnImageCopy.UseVisualStyleBackColor = true;
            this.btnImageCopy.Click += new System.EventHandler(this.btnImageCopy_Click);
            // 
            // btnExportGraph
            // 
            this.btnExportGraph.Location = new System.Drawing.Point(18, 115);
            this.btnExportGraph.Name = "btnExportGraph";
            this.btnExportGraph.Size = new System.Drawing.Size(175, 23);
            this.btnExportGraph.TabIndex = 4;
            this.btnExportGraph.Text = "Save to file";
            this.btnExportGraph.UseVisualStyleBackColor = true;
            this.btnExportGraph.Click += new System.EventHandler(this.btnExportGraph_Click);
            // 
            // lblPixels
            // 
            this.lblPixels.AutoSize = true;
            this.lblPixels.Location = new System.Drawing.Point(153, 33);
            this.lblPixels.Name = "lblPixels";
            this.lblPixels.Size = new System.Drawing.Size(33, 13);
            this.lblPixels.TabIndex = 3;
            this.lblPixels.Text = "pixels";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(75, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(13, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "×";
            // 
            // nudHeight
            // 
            this.nudHeight.Location = new System.Drawing.Point(94, 29);
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
            this.nudWidth.Location = new System.Drawing.Point(18, 29);
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
            this.gbSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbSource.Controls.Add(this.cmbTimeModel);
            this.gbSource.Controls.Add(this.lblTimeModel);
            this.gbSource.Controls.Add(this.clbSources);
            this.gbSource.Controls.Add(this.cmbPlotSpec);
            this.gbSource.Controls.Add(this.lblData);
            this.gbSource.Location = new System.Drawing.Point(784, 34);
            this.gbSource.Name = "gbSource";
            this.gbSource.Size = new System.Drawing.Size(212, 200);
            this.gbSource.TabIndex = 8;
            this.gbSource.TabStop = false;
            this.gbSource.Text = "Source";
            // 
            // cmbTimeModel
            // 
            this.cmbTimeModel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbTimeModel.FormattingEnabled = true;
            this.cmbTimeModel.Location = new System.Drawing.Point(63, 168);
            this.cmbTimeModel.Name = "cmbTimeModel";
            this.cmbTimeModel.Size = new System.Drawing.Size(130, 21);
            this.cmbTimeModel.TabIndex = 9;
            this.cmbTimeModel.SelectedIndexChanged += new System.EventHandler(this.PlotOption_Changed);
            // 
            // lblTimeModel
            // 
            this.lblTimeModel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTimeModel.AutoSize = true;
            this.lblTimeModel.Location = new System.Drawing.Point(15, 171);
            this.lblTimeModel.Name = "lblTimeModel";
            this.lblTimeModel.Size = new System.Drawing.Size(36, 13);
            this.lblTimeModel.TabIndex = 8;
            this.lblTimeModel.Text = "Time :";
            // 
            // clbSources
            // 
            this.clbSources.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.clbSources.CheckOnClick = true;
            this.clbSources.FormattingEnabled = true;
            this.clbSources.Location = new System.Drawing.Point(18, 19);
            this.clbSources.Name = "clbSources";
            this.clbSources.Size = new System.Drawing.Size(175, 94);
            this.clbSources.TabIndex = 7;
            this.clbSources.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clbSources_ItemCheck);
            this.clbSources.Click += new System.EventHandler(this.PlotOption_Changed);
            this.clbSources.SelectedIndexChanged += new System.EventHandler(this.PlotOption_Changed);
            this.clbSources.DoubleClick += new System.EventHandler(this.PlotOption_Changed);
            // 
            // cmbPlotSpec
            // 
            this.cmbPlotSpec.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbPlotSpec.FormattingEnabled = true;
            this.cmbPlotSpec.Location = new System.Drawing.Point(63, 137);
            this.cmbPlotSpec.Name = "cmbPlotSpec";
            this.cmbPlotSpec.Size = new System.Drawing.Size(130, 21);
            this.cmbPlotSpec.TabIndex = 6;
            this.cmbPlotSpec.SelectedIndexChanged += new System.EventHandler(this.PlotSpec_Changed);
            // 
            // lblData
            // 
            this.lblData.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblData.AutoSize = true;
            this.lblData.Location = new System.Drawing.Point(15, 140);
            this.lblData.Name = "lblData";
            this.lblData.Size = new System.Drawing.Size(36, 13);
            this.lblData.TabIndex = 5;
            this.lblData.Text = "Data :";
            // 
            // FormMultiTrajectoryAnalysis
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
            this.Name = "FormMultiTrajectoryAnalysis";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Trajectory analysis";
            this.tabControl.ResumeLayout(false);
            this.pagePlot.ResumeLayout(false);
            this.pageAbout.ResumeLayout(false);
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
        private System.Windows.Forms.ComboBox cmbPlotSpec;
        private System.Windows.Forms.Label lblData;
        private System.Windows.Forms.TabPage pageAbout;
        private System.Windows.Forms.RichTextBox rtbInfo1;
        private System.Windows.Forms.RichTextBox rtbInfo2;
        private OxyPlot.WindowsForms.PlotView plotDurbinWatson;
        private System.Windows.Forms.Label lblCutoffFrequencies;
        private System.Windows.Forms.CheckedListBox clbSources;
        private System.Windows.Forms.ComboBox cmbTimeModel;
        private System.Windows.Forms.Label lblTimeModel;
        private System.Windows.Forms.ListView lvCutoffFrequencies;
        private System.Windows.Forms.ColumnHeader chSource;
        private System.Windows.Forms.ColumnHeader chX;
        private System.Windows.Forms.ColumnHeader chY;
    }
}