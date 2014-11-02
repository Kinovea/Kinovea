namespace CaptureBenchmark
{
    partial class View
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(View));
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnDrops = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.btnBrady = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.btnHeartbeat = new System.Windows.Forms.Button();
            this.btnCommitbeat = new System.Windows.Forms.Button();
            this.pb = new System.Windows.Forms.ProgressBar();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnSlow = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.btnOccslow = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.btnNoop = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnJPEG2 = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.btnJPEG1 = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.btnLZ4 = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.btnFrameNumber = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(110, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(206, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Tests the stability of the camera framerate.";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.btnDrops);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.btnBrady);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.btnHeartbeat);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.btnCommitbeat);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(361, 154);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Image grabbing";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(110, 118);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(213, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Simulates occasional consumer slowdowns.";
            // 
            // btnDrops
            // 
            this.btnDrops.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnDrops.Image = global::CaptureBenchmark.Properties.Resources.water;
            this.btnDrops.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnDrops.Location = new System.Drawing.Point(15, 113);
            this.btnDrops.Name = "btnDrops";
            this.btnDrops.Size = new System.Drawing.Size(89, 24);
            this.btnDrops.TabIndex = 6;
            this.btnDrops.Text = "Drops";
            this.btnDrops.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnDrops.UseVisualStyleBackColor = true;
            this.btnDrops.Click += new System.EventHandler(this.btnDrops_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(110, 89);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(217, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Tests producer reaction to thread slowdown.";
            // 
            // btnBrady
            // 
            this.btnBrady.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnBrady.Image = global::CaptureBenchmark.Properties.Resources.user_medical;
            this.btnBrady.Location = new System.Drawing.Point(15, 84);
            this.btnBrady.Name = "btnBrady";
            this.btnBrady.Size = new System.Drawing.Size(89, 23);
            this.btnBrady.TabIndex = 4;
            this.btnBrady.Text = "Bradycardia";
            this.btnBrady.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnBrady.UseVisualStyleBackColor = true;
            this.btnBrady.Click += new System.EventHandler(this.btnBrady_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(110, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(234, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Tests the copying of frames into central memory.";
            // 
            // btnHeartbeat
            // 
            this.btnHeartbeat.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnHeartbeat.Image = global::CaptureBenchmark.Properties.Resources.application_monitor;
            this.btnHeartbeat.Location = new System.Drawing.Point(15, 26);
            this.btnHeartbeat.Name = "btnHeartbeat";
            this.btnHeartbeat.Size = new System.Drawing.Size(89, 23);
            this.btnHeartbeat.TabIndex = 0;
            this.btnHeartbeat.Text = "Heartbeat";
            this.btnHeartbeat.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnHeartbeat.UseVisualStyleBackColor = true;
            this.btnHeartbeat.Click += new System.EventHandler(this.btnHeartbeat_Click);
            // 
            // btnCommitbeat
            // 
            this.btnCommitbeat.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnCommitbeat.Image = global::CaptureBenchmark.Properties.Resources.memory;
            this.btnCommitbeat.Location = new System.Drawing.Point(15, 55);
            this.btnCommitbeat.Name = "btnCommitbeat";
            this.btnCommitbeat.Size = new System.Drawing.Size(89, 23);
            this.btnCommitbeat.TabIndex = 2;
            this.btnCommitbeat.Text = "Commitbeat";
            this.btnCommitbeat.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnCommitbeat.UseVisualStyleBackColor = true;
            this.btnCommitbeat.Click += new System.EventHandler(this.btnCommitbeat_Click);
            // 
            // pb
            // 
            this.pb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pb.Location = new System.Drawing.Point(12, 337);
            this.pb.Name = "pb";
            this.pb.Size = new System.Drawing.Size(835, 23);
            this.pb.TabIndex = 4;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnSlow);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.btnOccslow);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.btnNoop);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Location = new System.Drawing.Point(12, 174);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(361, 125);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Test consumers";
            // 
            // btnSlow
            // 
            this.btnSlow.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnSlow.Image = global::CaptureBenchmark.Properties.Resources.application_monitor;
            this.btnSlow.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSlow.Location = new System.Drawing.Point(15, 84);
            this.btnSlow.Name = "btnSlow";
            this.btnSlow.Size = new System.Drawing.Size(89, 23);
            this.btnSlow.TabIndex = 4;
            this.btnSlow.Text = "Slow";
            this.btnSlow.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSlow.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSlow.UseVisualStyleBackColor = true;
            this.btnSlow.Click += new System.EventHandler(this.btnSlow_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(110, 89);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(149, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "Systematically slow consumer.";
            // 
            // btnOccslow
            // 
            this.btnOccslow.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnOccslow.Image = global::CaptureBenchmark.Properties.Resources.application_monitor;
            this.btnOccslow.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOccslow.Location = new System.Drawing.Point(15, 55);
            this.btnOccslow.Name = "btnOccslow";
            this.btnOccslow.Size = new System.Drawing.Size(89, 23);
            this.btnOccslow.TabIndex = 2;
            this.btnOccslow.Text = "Occslow";
            this.btnOccslow.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOccslow.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnOccslow.UseVisualStyleBackColor = true;
            this.btnOccslow.Click += new System.EventHandler(this.btnOccslow_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(110, 60);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(143, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "Occasionally slow consumer.";
            // 
            // btnNoop
            // 
            this.btnNoop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnNoop.Image = global::CaptureBenchmark.Properties.Resources.application_monitor;
            this.btnNoop.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNoop.Location = new System.Drawing.Point(15, 26);
            this.btnNoop.Name = "btnNoop";
            this.btnNoop.Size = new System.Drawing.Size(89, 23);
            this.btnNoop.TabIndex = 0;
            this.btnNoop.Text = "Noop";
            this.btnNoop.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNoop.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnNoop.UseVisualStyleBackColor = true;
            this.btnNoop.Click += new System.EventHandler(this.btnNoop_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(110, 31);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(120, 13);
            this.label8.TabIndex = 1;
            this.label8.Text = "No-operation consumer.";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnJPEG2);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.btnJPEG1);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.btnLZ4);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Location = new System.Drawing.Point(379, 18);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(441, 148);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Compression (No storage)";
            // 
            // btnJPEG2
            // 
            this.btnJPEG2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnJPEG2.Image = global::CaptureBenchmark.Properties.Resources.zip2;
            this.btnJPEG2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnJPEG2.Location = new System.Drawing.Point(15, 83);
            this.btnJPEG2.Name = "btnJPEG2";
            this.btnJPEG2.Size = new System.Drawing.Size(100, 23);
            this.btnJPEG2.TabIndex = 4;
            this.btnJPEG2.Text = "JPEG2";
            this.btnJPEG2.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnJPEG2.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(133, 88);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(257, 13);
            this.label11.TabIndex = 5;
            this.label11.Text = "Compress to JPEG (90%) using libjpeg-turbo encoder.";
            // 
            // btnJPEG1
            // 
            this.btnJPEG1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnJPEG1.Image = global::CaptureBenchmark.Properties.Resources.zip2;
            this.btnJPEG1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnJPEG1.Location = new System.Drawing.Point(15, 54);
            this.btnJPEG1.Name = "btnJPEG1";
            this.btnJPEG1.Size = new System.Drawing.Size(100, 23);
            this.btnJPEG1.TabIndex = 2;
            this.btnJPEG1.Text = "JPEG1";
            this.btnJPEG1.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnJPEG1.UseVisualStyleBackColor = true;
            this.btnJPEG1.Click += new System.EventHandler(this.btnJPEG1_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(133, 59);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(277, 13);
            this.label9.TabIndex = 3;
            this.label9.Text = "Compress to JPEG (90%) using .NET framework encoder.";
            // 
            // btnLZ4
            // 
            this.btnLZ4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnLZ4.Image = global::CaptureBenchmark.Properties.Resources.zip2;
            this.btnLZ4.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLZ4.Location = new System.Drawing.Point(15, 25);
            this.btnLZ4.Name = "btnLZ4";
            this.btnLZ4.Size = new System.Drawing.Size(100, 23);
            this.btnLZ4.TabIndex = 0;
            this.btnLZ4.Text = "LZ4";
            this.btnLZ4.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnLZ4.UseVisualStyleBackColor = true;
            this.btnLZ4.Click += new System.EventHandler(this.btnLZ4_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(133, 30);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(187, 13);
            this.label10.TabIndex = 1;
            this.label10.Text = "Compress images using LZ4 algorithm.";
            // 
            // btnFrameNumber
            // 
            this.btnFrameNumber.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnFrameNumber.Image = global::CaptureBenchmark.Properties.Resources.counter;
            this.btnFrameNumber.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnFrameNumber.Location = new System.Drawing.Point(15, 26);
            this.btnFrameNumber.Name = "btnFrameNumber";
            this.btnFrameNumber.Size = new System.Drawing.Size(89, 23);
            this.btnFrameNumber.TabIndex = 0;
            this.btnFrameNumber.Text = "Frame#";
            this.btnFrameNumber.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnFrameNumber.UseVisualStyleBackColor = true;
            this.btnFrameNumber.Click += new System.EventHandler(this.btnFrameNumber_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(110, 31);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(181, 13);
            this.label7.TabIndex = 1;
            this.label7.Text = "Stores the frame number in a text file.";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.btnFrameNumber);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Location = new System.Drawing.Point(379, 174);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(441, 125);
            this.groupBox4.TabIndex = 10;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Storage";
            // 
            // View
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(859, 372);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.pb);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "View";
            this.Text = "Capture benchmark";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnHeartbeat;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnCommitbeat;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnBrady;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ProgressBar pb;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnDrops;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnNoop;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btnSlow;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnOccslow;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnLZ4;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button btnFrameNumber;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnJPEG2;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button btnJPEG1;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.GroupBox groupBox4;
    }
}

