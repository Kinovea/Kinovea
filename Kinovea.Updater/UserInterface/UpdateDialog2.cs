#region License
/*
Copyright © Joan Charmant 2010.
jcharmant@gmail.com 
 
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
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Kinovea.Services;
using Kinovea.Updater.Languages;
using System.Diagnostics;

namespace Kinovea.Updater
{
    /// <summary>
    /// This is a simple dialog showing the changelog and a button to download the latest version.
    /// We only come here if there is an actual update available.
    /// </summary>
    public partial class UpdateDialog2 : Form
    {
        private HelpIndex hiRemote;
        private ThreePartsVersion currentVersion;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public UpdateDialog2(HelpIndex hiRemote)
        {
            this.hiRemote = hiRemote;
            currentVersion = new ThreePartsVersion(Software.Version);
            InitializeComponent();
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
            string lnkTarget = "https://www.kinovea.org";
            lnkKinovea.Links.Add(0, lnkKinovea.Text.Length, lnkTarget);
            toolTip1.SetToolTip(lnkKinovea, lnkTarget);
        }
        
        private void btnDownload_Click(object sender, EventArgs e)
        {
            // We no longer attempt to download the file ourselves, just open the default browser on the link.
            Process.Start(hiRemote.AppInfos.FileLocation);
        }
    }
}
