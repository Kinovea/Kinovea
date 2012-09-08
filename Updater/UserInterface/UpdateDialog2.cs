#region License
/*
Copyright © Joan Charmant 2010.
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
using CodeProject.Downloader;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Kinovea.Services;
using Kinovea.Updater.Languages;

namespace Kinovea.Updater
{
	/// <summary>
	/// This is the simplified update dialog.
	/// It just handles the update of the software itself.
	/// We only come here if there is an actual update available.
	/// </summary>
	public partial class UpdateDialog2 : Form
	{
		#region Delegate
        public delegate void CallbackUpdateProgressBar(int percentDone);
        public delegate void CallbackDownloadComplete(int result);
		private Kinovea.Updater.UpdateDialog2.CallbackUpdateProgressBar m_CallbackUpdateProgressBar;
		private Kinovea.Updater.UpdateDialog2.CallbackDownloadComplete m_CallbackDownloadComplete;
        #endregion
		
		#region Members
		private HelpIndex m_hiRemote;
		private ThreePartsVersion m_currentVersion;
		private FileDownloader m_Downloader = new FileDownloader();
		private bool m_bDownloadStarted;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
		
		#region Constructors & initialisation
		public UpdateDialog2(HelpIndex _hiRemote)
		{
			m_hiRemote = _hiRemote;
			m_currentVersion = new ThreePartsVersion(Software.Version);
                
            InitializeComponent();
			
            m_Downloader.DownloadComplete += new EventHandler(downloader_DownloadedComplete);
            m_Downloader.ProgressChanged += new DownloadProgressHandler(downloader_ProgressChanged);

            m_CallbackUpdateProgressBar = new CallbackUpdateProgressBar(UpdateProgressBar);
            m_CallbackDownloadComplete  = new CallbackDownloadComplete(DownloadComplete);
            
            InitDialog();
		}
		private void InitDialog()
		{
			this.Text = "   " + UpdaterLang.Updater_Title;
			
			btnCancel.Text = UpdaterLang.Updater_Quit;
            btnDownload.Text = UpdaterLang.Updater_Download;
			labelInfos.Text = UpdaterLang.Updater_Behind;
			
            lblNewVersion.Text = String.Format("{0}: {1} - ({2} {3}).", 
                                               UpdaterLang.Updater_NewVersion, 
                                               m_hiRemote.AppInfos.Version.ToString(),
                                               UpdaterLang.Updater_CurrentVersion,
                                               m_currentVersion.ToString());

            lblNewVersionFileSize.Text = String.Format("{0} {1:0.00} {2}",
            									UpdaterLang.Updater_FileSize,
            									(double)m_hiRemote.AppInfos.FileSizeInBytes / (1024*1024),
            									UpdaterLang.Updater_MegaBytes);
            
            lblChangeLog.Text = UpdaterLang.Updater_LblChangeLog;
            
            rtbxChangeLog.Clear();
            if (m_hiRemote.AppInfos.ChangelogLocation.Length > 0)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(m_hiRemote.AppInfos.ChangelogLocation);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK && response.ContentLength > 0)
                {
                    TextReader reader = new StreamReader(response.GetResponseStream());
                    
                    string line;
                    while ((line = reader.ReadLine()) != null) 
                    {
                        rtbxChangeLog.AppendText("\n");
                        rtbxChangeLog.AppendText(line);
                    }
                }
            }
            
			// website link.
            lnkKinovea.Links.Clear();
            string lnkTarget = "http://www.kinovea.org";
            lnkKinovea.Links.Add(0, lnkKinovea.Text.Length, lnkTarget);
            toolTip1.SetToolTip(lnkKinovea, lnkTarget);
		}
		#endregion
		
		#region Download
		private void btnDownload_Click(object sender, EventArgs e)
		{
			// Get a destination folder.
			folderBrowserDialog.Description = UpdaterLang.Updater_BrowseFolderDescription;
            folderBrowserDialog.ShowNewFolderButton = true;
            folderBrowserDialog.RootFolder = Environment.SpecialFolder.Desktop;

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                // Start download.
                labelInfos.Visible = false;
                btnDownload.Visible = false;
                progressDownload.Visible = true;
                progressDownload.Left = 109;
                progressDownload.Width = 367;
                m_bDownloadStarted = true;
                log.Info("Starting the download of a new version.");
                m_Downloader.AsyncDownload(m_hiRemote.AppInfos.FileLocation, folderBrowserDialog.SelectedPath);
            }
		}
		private void downloader_ProgressChanged(object sender, DownloadEventArgs e)
        {
            // (In WorkerThread space)
            if (m_bDownloadStarted)
            {
                BeginInvoke(m_CallbackUpdateProgressBar, e.PercentDone);
            }
        }
        private void downloader_DownloadedComplete(object sender, EventArgs e)
        {
			// (In WorkerThread space)
            if (m_CallbackDownloadComplete != null)
            {
                BeginInvoke(m_CallbackDownloadComplete, 0);
            }
        }
        private void UpdateProgressBar(int _iPercentDone)
        {
            // In UI thread space.
            progressDownload.Value = _iPercentDone;
        }
        private void DownloadComplete(int _iResult)
        {
            // In UI thread space.
            this.Hide();
            log.Info("Download of the new version complete.");
            m_bDownloadStarted = false;
            m_Downloader.DownloadComplete -= new EventHandler(downloader_DownloadedComplete);
            this.DialogResult = DialogResult.OK;
            
            MessageBox.Show(UpdaterLang.Updater_mboxDownloadSuccess_Description.Replace("\\n","\n"), UpdaterLang.Updater_Title,
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            this.Close();
        }
		#endregion
		
		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.Hide();
			log.Info("Download cancelled.");
            
			// Cancel the ongoing download if any.
			// Todo: remove the partially downloaded file?
			if (m_bDownloadStarted && m_Downloader != null)
            {
                m_bDownloadStarted = false;
                m_Downloader.Cancel();
            }
			
			this.Close();
		}
	}
}
