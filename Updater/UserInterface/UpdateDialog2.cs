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
        private Kinovea.Updater.UpdateDialog2.CallbackUpdateProgressBar callbackUpdateProgressBar;
        private Kinovea.Updater.UpdateDialog2.CallbackDownloadComplete callbackDownloadComplete;
        #endregion
        
        #region Members
        private HelpIndex hiRemote;
        private ThreePartsVersion currentVersion;
        private FileDownloader downloader = new FileDownloader();
        private bool downloadStarted;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Constructors & initialisation
        public UpdateDialog2(HelpIndex hiRemote)
        {
            this.hiRemote = hiRemote;
            currentVersion = new ThreePartsVersion(Software.Version);
                
            InitializeComponent();
            
            downloader.DownloadComplete += new EventHandler(downloader_DownloadedComplete);
            downloader.ProgressChanged += new DownloadProgressHandler(downloader_ProgressChanged);

            callbackUpdateProgressBar = new CallbackUpdateProgressBar(UpdateProgressBar);
            callbackDownloadComplete  = new CallbackDownloadComplete(DownloadComplete);
            
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
                hiRemote.AppInfos.Version.ToString(),
                UpdaterLang.Updater_CurrentVersion,
                currentVersion.ToString());

            lblNewVersionFileSize.Text = String.Format("{0} {1:0.00} {2}",
                UpdaterLang.Updater_FileSize,
                (double)hiRemote.AppInfos.FileSizeInBytes / (1024*1024),
                UpdaterLang.Updater_MegaBytes);
            
            lblChangeLog.Text = UpdaterLang.Updater_LblChangeLog;
            
            rtbxChangeLog.Clear();
            if (hiRemote.AppInfos.ChangelogLocation.Length > 0)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(hiRemote.AppInfos.ChangelogLocation);
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

            if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                return;
           
            // Start download.
            labelInfos.Visible = false;
            btnDownload.Visible = false;
            progressDownload.Visible = true;
            progressDownload.Left = 109;
            progressDownload.Width = 367;
            downloadStarted = true;
            log.Info("Starting the download of a new version.");
            downloader.AsyncDownload(hiRemote.AppInfos.FileLocation, folderBrowserDialog.SelectedPath);
        }
        private void downloader_ProgressChanged(object sender, DownloadEventArgs e)
        {
            // (In WorkerThread space)
            if (downloadStarted)
                BeginInvoke(callbackUpdateProgressBar, e.PercentDone);
        }
        private void downloader_DownloadedComplete(object sender, EventArgs e)
        {
            // (In WorkerThread space)
            if (callbackDownloadComplete != null)
                BeginInvoke(callbackDownloadComplete, 0);
        }
        private void UpdateProgressBar(int percentDone)
        {
            // In UI thread space.
            progressDownload.Value = percentDone;
        }
        private void DownloadComplete(int result)
        {
            // In UI thread space.
            this.Hide();
            log.Info("Download of the new version complete.");
            downloadStarted = false;
            downloader.DownloadComplete -= new EventHandler(downloader_DownloadedComplete);
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
            if (downloadStarted && downloader != null)
            {
                downloadStarted = false;
                downloader.Cancel();
            }
            
            this.Close();
        }
    }
}
