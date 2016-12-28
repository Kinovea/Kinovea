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
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using Kinovea.Pipeline;
using Kinovea.Camera.Languages;
using Kinovea.Services;

namespace Kinovea.Camera.HTTP
{
    public partial class ConnectionWizard : UserControl, IConnectionWizard
    {
        private CameraManagerHTTP manager;
        private bool testing;
        private bool urlImport;
        private bool paramImport;
        private string host;
        private string port;
        private string user;
        private string password;
        private string path;
        private string format;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public ConnectionWizard(CameraManagerHTTP manager)
        {
            this.manager = manager;
            InitializeComponent();
            Initialize();

            BackColor = Color.White;
            host = "192.168.0.10";
            path = "/mjpg/video.mjpg";
            tbHost.Text = host;
            format = "MJPEG";
        }

        private void Initialize()
        {
            gpNetwork.Text = CameraLang.FormConnectionWizard_Network;
            lblHost.Text = CameraLang.FormConnectionWizard_Host;
            lblPort.Text = CameraLang.FormConnectionWizard_Port;
            lblFormat.Text = CameraLang.FormConnectionWizard_Format;

            gpAuthentication.Text = CameraLang.FormConnectionWizard_Authentication;
            lblUser.Text = CameraLang.FormConnectionWizard_User;
            lblPassword.Text = CameraLang.FormConnectionWizard_Password;

            gpURL.Text = CameraLang.FormConnectionWizard_URL;
        }
        
        public CameraSummary GetResult()
        {
            string id = Guid.NewGuid().ToString();
            CameraSummary summary = manager.GetDefaultCameraSummary(id);
            summary.UpdateSpecific(CreateSpecific());
            return summary;
        }
        
        public void TestCamera()
        {
            if(testing)
                return;
            
            // Spawn a thread to get a snapshot.
            testing = true;
            CameraSummary testSummary = GetResult();
            SnapshotRetriever retriever = new SnapshotRetriever(manager, testSummary);
            retriever.CameraThumbnailProduced += SnapshotRetriever_CameraThumbnailProduced;
            ThreadPool.QueueUserWorkItem(retriever.Run);
        }
        
        public SpecificInfo GetSpecific()
        {
            return CreateSpecific();
        }
        
        public void Populate(SpecificInfo specific)
        {
            // This is used in the context of reconfiguring an existing camera.
            host = specific.Host;
            port = specific.Port;
            user = specific.User;
            password = specific.Password;
            path = specific.Path;
            format = specific.Format;
            
            urlImport = true;
            paramImport = true;
            PopulateFields();
            PopulateURL();
            urlImport = false;
            paramImport = false;
        }

        private void BtnShowPasswordClick(object sender, EventArgs e)
        {
            tbPassword.UseSystemPasswordChar = !tbPassword.UseSystemPasswordChar;
            tbPassword.Invalidate();
        }
        
        private void ImportFromURL(string url)
        {
            // Parse url to individual components.
            try
            {
                Uri uri = new Uri(url);
                
                user = "";
                password = "";
                if(!string.IsNullOrEmpty(uri.UserInfo))
                {
                    string[] split = uri.UserInfo.Split(':');
                    user = split[0];
                    password = split[1];
                }
                
                host = uri.Host;
                port = uri.Port == 80 ? "" : uri.Port.ToString();
                path = uri.PathAndQuery;
                
                // Try to guess format
                string extension = Path.GetExtension(path).ToLower();
                if(extension == ".mjpeg" || extension == ".mjpg")
                    format = "MJPEG";
                else if(extension == ".jpeg" || extension == ".jpg")
                    format = "JPEG";
            }
            catch(Exception e)
            {
                log.ErrorFormat(e.ToString());
            }
        }
        
        private void PopulateFields()
        {
            tbUser.Text = user;
            tbPassword.Text = password;
            tbHost.Text = host;
            tbPort.Text = port;
            cbFormat.Text = format;
        }
        
        private void PopulateURL()
        {
            tbURL.Text = manager.BuildURL(CreateSpecific());
        }
        
        #region Parameters event handlers
        private void TbUserTextChanged(object sender, EventArgs e)
        {
            if(urlImport)
                return;
            
            user = tbUser.Text;
            AfterParameterTextChanged();
        }
        private void TbPasswordTextChanged(object sender, EventArgs e)
        {
            if(urlImport)
                return;
            
            password = tbPassword.Text;
            AfterParameterTextChanged();
        }
        private void TbHost_TextChanged(object sender, EventArgs e)
        {
            if(urlImport)
                return;
            
            host = tbHost.Text;
            AfterParameterTextChanged();
        }
        private void TbPortTextChanged(object sender, EventArgs e)
        {
            if(urlImport)
                return;
            
            port = tbPort.Text;
            AfterParameterTextChanged();
        }
        private void CbFormatSelectedIndexChanged(object sender, EventArgs e)
        {
            if(urlImport)
                return;
            
            format = cbFormat.Text;
            AfterParameterTextChanged();
        }
        private void TbURLTextChanged(object sender, EventArgs e)
        {
            if(paramImport)
                return;
                
            ImportFromURL(tbURL.Text);
            AfterURLTextChanged();
        }
        #endregion
        
        private void AfterParameterTextChanged()
        {   
            paramImport = true;
            PopulateURL();
            paramImport = false;
        }
        
        private void AfterURLTextChanged()
        {
            urlImport = true;
            PopulateFields();
            urlImport = false;
        }
        
        private SpecificInfo CreateSpecific()
        {
            SpecificInfo specific = new SpecificInfo();
            specific.User = user;
            specific.Password = password;
            specific.Host = host;
            specific.Port = port;
            specific.Path = path;
            specific.Format = format;
            return specific;
        }
        
        private void SnapshotRetriever_CameraThumbnailProduced(object sender, CameraThumbnailProducedEventArgs e)
        {
            SnapshotRetriever retriever = sender as SnapshotRetriever;
            if (retriever == null)
                return;

            if (e.HadError || e.ImageDescriptor == ImageDescriptor.Invalid || e.Thumbnail == null)
            {
                string title = CameraLang.FormHandshakeResult_Failure_Title;
                string message = CameraLang.FormHandshakeResult_Failure_Message;
                MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                FormHandshakeResult result = new FormHandshakeResult(e.Thumbnail);
                result.ShowDialog();
                result.Dispose();
                e.Thumbnail.Dispose();
            }

            AfterCameraTest(retriever);
        }
        
        private void AfterCameraTest(SnapshotRetriever retriever)
        {
            retriever.CameraThumbnailProduced -= SnapshotRetriever_CameraThumbnailProduced;
            testing = false;
        }
    }
}
