
namespace Kinovea.ScreenManager
{
    partial class SidePanelKeyframes
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
      this.components = new System.ComponentModel.Container();
      this.flowKeyframes = new System.Windows.Forms.FlowLayoutPanel();
      this.panel1 = new System.Windows.Forms.Panel();
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this.btnNext = new System.Windows.Forms.Button();
      this.btnPrev = new System.Windows.Forms.Button();
      this.btnShowAll = new System.Windows.Forms.Button();
      this.btnAddKeyframe = new System.Windows.Forms.Button();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // flowKeyframes
      // 
      this.flowKeyframes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.flowKeyframes.AutoScroll = true;
      this.flowKeyframes.BackColor = System.Drawing.Color.White;
      this.flowKeyframes.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
      this.flowKeyframes.Location = new System.Drawing.Point(0, 46);
      this.flowKeyframes.Name = "flowKeyframes";
      this.flowKeyframes.Size = new System.Drawing.Size(275, 549);
      this.flowKeyframes.TabIndex = 1;
      this.flowKeyframes.WrapContents = false;
      this.flowKeyframes.Layout += new System.Windows.Forms.LayoutEventHandler(this.flowKeyframes_Layout);
      // 
      // panel1
      // 
      this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.panel1.BackColor = System.Drawing.Color.WhiteSmoke;
      this.panel1.Controls.Add(this.btnNext);
      this.panel1.Controls.Add(this.btnPrev);
      this.panel1.Controls.Add(this.btnShowAll);
      this.panel1.Controls.Add(this.btnAddKeyframe);
      this.panel1.Location = new System.Drawing.Point(3, 5);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(269, 35);
      this.panel1.TabIndex = 11;
      // 
      // btnNext
      // 
      this.btnNext.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnNext.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnNext.FlatAppearance.BorderSize = 0;
      this.btnNext.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnNext.Image = global::Kinovea.ScreenManager.Properties.Resources.sort_right_16;
      this.btnNext.Location = new System.Drawing.Point(59, 7);
      this.btnNext.Name = "btnNext";
      this.btnNext.Size = new System.Drawing.Size(20, 20);
      this.btnNext.TabIndex = 12;
      this.btnNext.UseVisualStyleBackColor = true;
      this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
      // 
      // btnPrev
      // 
      this.btnPrev.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnPrev.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnPrev.FlatAppearance.BorderSize = 0;
      this.btnPrev.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnPrev.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnPrev.Image = global::Kinovea.ScreenManager.Properties.Resources.sort_left_16;
      this.btnPrev.Location = new System.Drawing.Point(33, 7);
      this.btnPrev.Name = "btnPrev";
      this.btnPrev.Size = new System.Drawing.Size(20, 20);
      this.btnPrev.TabIndex = 11;
      this.btnPrev.UseVisualStyleBackColor = true;
      this.btnPrev.Click += new System.EventHandler(this.btnPrev_Click);
      // 
      // btnShowAll
      // 
      this.btnShowAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnShowAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnShowAll.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnShowAll.FlatAppearance.BorderSize = 0;
      this.btnShowAll.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnShowAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnShowAll.Image = global::Kinovea.ScreenManager.Properties.Resources.bursts_16;
      this.btnShowAll.Location = new System.Drawing.Point(237, 7);
      this.btnShowAll.Name = "btnShowAll";
      this.btnShowAll.Size = new System.Drawing.Size(20, 20);
      this.btnShowAll.TabIndex = 10;
      this.btnShowAll.UseVisualStyleBackColor = true;
      this.btnShowAll.Click += new System.EventHandler(this.btnShowAll_Click);
      // 
      // btnAddKeyframe
      // 
      this.btnAddKeyframe.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnAddKeyframe.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnAddKeyframe.FlatAppearance.BorderSize = 0;
      this.btnAddKeyframe.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnAddKeyframe.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnAddKeyframe.Image = global::Kinovea.ScreenManager.Properties.Resources.add_image_16;
      this.btnAddKeyframe.Location = new System.Drawing.Point(7, 7);
      this.btnAddKeyframe.Name = "btnAddKeyframe";
      this.btnAddKeyframe.Size = new System.Drawing.Size(20, 20);
      this.btnAddKeyframe.TabIndex = 9;
      this.btnAddKeyframe.UseVisualStyleBackColor = true;
      this.btnAddKeyframe.Click += new System.EventHandler(this.btnAddKeyframe_Click);
      // 
      // SidePanelKeyframes
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.flowKeyframes);
      this.DoubleBuffered = true;
      this.Name = "SidePanelKeyframes";
      this.Size = new System.Drawing.Size(275, 595);
      this.panel1.ResumeLayout(false);
      this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.FlowLayoutPanel flowKeyframes;
        private System.Windows.Forms.Button btnAddKeyframe;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnShowAll;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnPrev;
    }
}
