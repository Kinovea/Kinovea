#region License
/*
Copyright © Joan Charmant 2012.
joan.charmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.

*/
#endregion
namespace Kinovea.ScreenManager
{
    partial class FormConfigureComposite
    {
        /// <summary>
        /// Designer variable used to keep track of non-visual components.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        
        /// <summary>
        /// Disposes resources used by the form.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                if (components != null) {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }
        
        /// <summary>
        /// This method is required for Windows Forms designer support.
        /// Do not change the method contents inside the source code editor. The Forms designer might
        /// not be able to load this method if it was changed manually.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.gbConfiguration = new System.Windows.Forms.GroupBox();
            this.rbDelay = new System.Windows.Forms.RadioButton();
            this.rbSlowMotion = new System.Windows.Forms.RadioButton();
            this.rbQuadrants = new System.Windows.Forms.RadioButton();
            this.gbConfiguration.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(58, 127);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(99, 24);
            this.btnOk.TabIndex = 16;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.BtnOKClick);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(163, 127);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(99, 24);
            this.btnCancel.TabIndex = 17;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // gbDelayMode
            // 
            this.gbConfiguration.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbConfiguration.Controls.Add(this.rbQuadrants);
            this.gbConfiguration.Controls.Add(this.rbSlowMotion);
            this.gbConfiguration.Controls.Add(this.rbDelay);
            this.gbConfiguration.Location = new System.Drawing.Point(12, 12);
            this.gbConfiguration.Name = "gbDelayMode";
            this.gbConfiguration.Size = new System.Drawing.Size(249, 109);
            this.gbConfiguration.TabIndex = 20;
            this.gbConfiguration.TabStop = false;
            this.gbConfiguration.Text = "Delay mode";
            // 
            // rbDelay
            // 
            this.rbDelay.AutoSize = true;
            this.rbDelay.Location = new System.Drawing.Point(17, 25);
            this.rbDelay.Name = "rbDelay";
            this.rbDelay.Size = new System.Drawing.Size(52, 17);
            this.rbDelay.TabIndex = 0;
            this.rbDelay.TabStop = true;
            this.rbDelay.Text = "Delay";
            this.rbDelay.UseVisualStyleBackColor = true;
            // 
            // rbSlowMotion
            // 
            this.rbSlowMotion.AutoSize = true;
            this.rbSlowMotion.Location = new System.Drawing.Point(17, 48);
            this.rbSlowMotion.Name = "rbSlowMotion";
            this.rbSlowMotion.Size = new System.Drawing.Size(82, 17);
            this.rbSlowMotion.TabIndex = 1;
            this.rbSlowMotion.TabStop = true;
            this.rbSlowMotion.Text = "Slow motion";
            this.rbSlowMotion.UseVisualStyleBackColor = true;
            // 
            // rbQuadrants
            // 
            this.rbQuadrants.AutoSize = true;
            this.rbQuadrants.Location = new System.Drawing.Point(17, 71);
            this.rbQuadrants.Name = "rbQuadrants";
            this.rbQuadrants.Size = new System.Drawing.Size(74, 17);
            this.rbQuadrants.TabIndex = 2;
            this.rbQuadrants.TabStop = true;
            this.rbQuadrants.Text = "Quadrants";
            this.rbQuadrants.UseVisualStyleBackColor = true;
            // 
            // FormConfigureComposite
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(274, 163);
            this.Controls.Add(this.gbConfiguration);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormConfigureComposite";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Configure delay mode";
            this.gbConfiguration.ResumeLayout(false);
            this.gbConfiguration.PerformLayout();
            this.ResumeLayout(false);

        }
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox gbConfiguration;
        private System.Windows.Forms.RadioButton rbQuadrants;
        private System.Windows.Forms.RadioButton rbSlowMotion;
        private System.Windows.Forms.RadioButton rbDelay;
    }
}
