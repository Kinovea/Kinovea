#region License
/*
Copyright © Joan Charmant 2013.
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
namespace Kinovea.Camera.HTTP
{
    partial class ConnectionWizard
    {
        /// <summary>
        /// Designer variable used to keep track of non-visual components.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        
        /// <summary>
        /// Disposes resources used by the control.
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
            this.tbURL = new System.Windows.Forms.TextBox();
            this.gpURL = new System.Windows.Forms.GroupBox();
            this.lblUser = new System.Windows.Forms.Label();
            this.tbUser = new System.Windows.Forms.TextBox();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.gpAuthentication = new System.Windows.Forms.GroupBox();
            this.gpNetwork = new System.Windows.Forms.GroupBox();
            this.cbFormat = new System.Windows.Forms.ComboBox();
            this.lblFormat = new System.Windows.Forms.Label();
            this.tbPort = new System.Windows.Forms.TextBox();
            this.lblHost = new System.Windows.Forms.Label();
            this.lblPort = new System.Windows.Forms.Label();
            this.tbHost = new System.Windows.Forms.TextBox();
            this.gpURL.SuspendLayout();
            this.gpAuthentication.SuspendLayout();
            this.gpNetwork.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbURL
            // 
            this.tbURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tbURL.Location = new System.Drawing.Point(6, 20);
            this.tbURL.Name = "tbURL";
            this.tbURL.Size = new System.Drawing.Size(353, 20);
            this.tbURL.TabIndex = 0;
            this.tbURL.TextChanged += new System.EventHandler(this.TbURLTextChanged);
            // 
            // gpURL
            // 
            this.gpURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.gpURL.Controls.Add(this.tbURL);
            this.gpURL.Location = new System.Drawing.Point(14, 227);
            this.gpURL.Name = "gpURL";
            this.gpURL.Size = new System.Drawing.Size(377, 53);
            this.gpURL.TabIndex = 3;
            this.gpURL.TabStop = false;
            this.gpURL.Text = "Final URL";
            // 
            // lblUser
            // 
            this.lblUser.Location = new System.Drawing.Point(12, 25);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(145, 20);
            this.lblUser.TabIndex = 3;
            this.lblUser.Text = "User";
            this.lblUser.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbUser
            // 
            this.tbUser.Location = new System.Drawing.Point(182, 25);
            this.tbUser.Name = "tbUser";
            this.tbUser.Size = new System.Drawing.Size(177, 20);
            this.tbUser.TabIndex = 0;
            this.tbUser.TextChanged += new System.EventHandler(this.TbUserTextChanged);
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(182, 62);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.Size = new System.Drawing.Size(177, 20);
            this.tbPassword.TabIndex = 1;
            this.tbPassword.TextChanged += new System.EventHandler(this.TbPasswordTextChanged);
            // 
            // lblPassword
            // 
            this.lblPassword.Location = new System.Drawing.Point(12, 62);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(145, 20);
            this.lblPassword.TabIndex = 5;
            this.lblPassword.Text = "Password";
            this.lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // gpAuthentication
            // 
            this.gpAuthentication.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.gpAuthentication.Controls.Add(this.tbPassword);
            this.gpAuthentication.Controls.Add(this.lblUser);
            this.gpAuthentication.Controls.Add(this.lblPassword);
            this.gpAuthentication.Controls.Add(this.tbUser);
            this.gpAuthentication.Location = new System.Drawing.Point(14, 121);
            this.gpAuthentication.Name = "gpAuthentication";
            this.gpAuthentication.Size = new System.Drawing.Size(377, 100);
            this.gpAuthentication.TabIndex = 2;
            this.gpAuthentication.TabStop = false;
            this.gpAuthentication.Text = "Authentication";
            // 
            // gpNetwork
            // 
            this.gpNetwork.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.gpNetwork.Controls.Add(this.cbFormat);
            this.gpNetwork.Controls.Add(this.lblFormat);
            this.gpNetwork.Controls.Add(this.tbPort);
            this.gpNetwork.Controls.Add(this.lblHost);
            this.gpNetwork.Controls.Add(this.lblPort);
            this.gpNetwork.Controls.Add(this.tbHost);
            this.gpNetwork.Location = new System.Drawing.Point(14, 15);
            this.gpNetwork.Name = "gpNetwork";
            this.gpNetwork.Size = new System.Drawing.Size(377, 100);
            this.gpNetwork.TabIndex = 1;
            this.gpNetwork.TabStop = false;
            this.gpNetwork.Text = "Network";
            // 
            // cbFormat
            // 
            this.cbFormat.FormattingEnabled = true;
            this.cbFormat.Items.AddRange(new object[] {
            "MJPEG",
            "JPEG"});
            this.cbFormat.Location = new System.Drawing.Point(124, 64);
            this.cbFormat.Name = "cbFormat";
            this.cbFormat.Size = new System.Drawing.Size(72, 21);
            this.cbFormat.TabIndex = 2;
            this.cbFormat.Text = "MJPEG";
            this.cbFormat.SelectedIndexChanged += new System.EventHandler(this.CbFormatSelectedIndexChanged);
            // 
            // lblFormat
            // 
            this.lblFormat.Location = new System.Drawing.Point(12, 63);
            this.lblFormat.Name = "lblFormat";
            this.lblFormat.Size = new System.Drawing.Size(51, 20);
            this.lblFormat.TabIndex = 10;
            this.lblFormat.Text = "Format";
            this.lblFormat.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbPort
            // 
            this.tbPort.Location = new System.Drawing.Point(317, 23);
            this.tbPort.Name = "tbPort";
            this.tbPort.Size = new System.Drawing.Size(42, 20);
            this.tbPort.TabIndex = 1;
            this.tbPort.TextChanged += new System.EventHandler(this.TbPortTextChanged);
            // 
            // lblHost
            // 
            this.lblHost.Location = new System.Drawing.Point(12, 23);
            this.lblHost.Name = "lblHost";
            this.lblHost.Size = new System.Drawing.Size(91, 20);
            this.lblHost.TabIndex = 3;
            this.lblHost.Text = "Host (IP address)";
            this.lblHost.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPort
            // 
            this.lblPort.Location = new System.Drawing.Point(271, 23);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(40, 20);
            this.lblPort.TabIndex = 5;
            this.lblPort.Text = "Port";
            this.lblPort.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbHost
            // 
            this.tbHost.Location = new System.Drawing.Point(124, 23);
            this.tbHost.Name = "tbHost";
            this.tbHost.Size = new System.Drawing.Size(141, 20);
            this.tbHost.TabIndex = 0;
            this.tbHost.TextChanged += new System.EventHandler(this.TbHost_TextChanged);
            // 
            // ConnectionWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.Controls.Add(this.gpNetwork);
            this.Controls.Add(this.gpAuthentication);
            this.Controls.Add(this.gpURL);
            this.Name = "ConnectionWizard";
            this.Size = new System.Drawing.Size(404, 332);
            this.gpURL.ResumeLayout(false);
            this.gpURL.PerformLayout();
            this.gpAuthentication.ResumeLayout(false);
            this.gpAuthentication.PerformLayout();
            this.gpNetwork.ResumeLayout(false);
            this.gpNetwork.PerformLayout();
            this.ResumeLayout(false);

        }
        private System.Windows.Forms.Label lblFormat;
        private System.Windows.Forms.ComboBox cbFormat;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.Label lblHost;
        private System.Windows.Forms.GroupBox gpNetwork;
        private System.Windows.Forms.TextBox tbHost;
        private System.Windows.Forms.TextBox tbPort;
        private System.Windows.Forms.GroupBox gpAuthentication;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.TextBox tbUser;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.GroupBox gpURL;
        private System.Windows.Forms.TextBox tbURL;
    }
}
