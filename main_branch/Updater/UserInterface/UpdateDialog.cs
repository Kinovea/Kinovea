/*
Copyright © Joan Charmant 2008.
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

using CodeProject.Downloader;
using Kinovea.Services;

namespace Kinovea.Updater
{
    public partial class UpdateDialog : Form
    {
        #region Enums
        public enum ItemStatus
        {
            Synched,
            NeedUpdate,
            NewItem
        }
        #endregion
        
        #region Delegate
        public delegate void CallbackUpdateProgressBar(int percentDone);
        public delegate void CallbackDownloadComplete(int result);
        public delegate void CallbackMultipleDownloadComplete(int result);

        private CallbackUpdateProgressBar m_CallbackUpdateProgressBar;
        private CallbackDownloadComplete m_CallbackDownloadComplete;
        private CallbackMultipleDownloadComplete m_CallbackMultipleDownloadComplete;
        #endregion
       
        #region Members
        ResourceManager     m_ResourceManager;
        FileDownloader      m_Downloader                = null;
        bool                m_bDownloadStarted          = false;
        bool                m_bSoftwareUpToDate         = true; // Default to true to avoid downloading when there's nothing.
        
        private HelpIndex m_hiLocal;
        private HelpIndex m_hiRemote;

        private List<HelpItem> m_SourceItemList;            // List of HelpItems to download.
        private int m_CurrentSourceItemIndex;               // HelpItem currently downloaded.
        private string m_TargetFolder;                      // Target of the currently downloaded HelpItem
        private int m_HelpItemListId;                       // List owner of the currently downloaded HelpItem. (0:Manuals, 1:Videos)
        #endregion

        #region Constructors
        public UpdateDialog(ResourceManager _resManager, HelpIndex _hiLocal, HelpIndex _hiRemote)
        {
            InitializeComponent();

            m_ResourceManager = _resManager;

            m_SourceItemList = new List<HelpItem>();
            m_CurrentSourceItemIndex = 0;
            m_TargetFolder = "";
            m_HelpItemListId = 0;
            
            m_CallbackUpdateProgressBar = new CallbackUpdateProgressBar(UpdateProgressBar);
            m_CallbackDownloadComplete  = new CallbackDownloadComplete(DownloadComplete);
            m_CallbackMultipleDownloadComplete = new CallbackMultipleDownloadComplete(MultipleDownloadComplete);

            m_hiLocal = _hiLocal;
            m_hiRemote = _hiRemote;

            // Lien internet
            lnkKinovea.Links.Clear();
            string lnkTarget = "http://www.kinovea.org";
            lnkKinovea.Links.Add(0, lnkKinovea.Text.Length, lnkTarget);
            toolTip1.SetToolTip(lnkKinovea, lnkTarget);

            // Chaînes statiques et dynamiques
            SetupPages(_hiLocal, _hiRemote);

            // Organisation des panels
            InitPages();

        }
        #endregion

        #region EventHandlers
        private void btnCancel_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            // Cancellation.
            // Si un téléchargement est lancé, on l'arrète.
            // Sinon, on quitte la boîte.
            //-----------------------------------------------
            if (m_bDownloadStarted && m_Downloader != null)
            {
                m_bDownloadStarted = false;
                m_Downloader.Cancel();
                HideProgressBar();
            }
            else
            {
                Close();
            }
        }
        private void btnDownload_Click(object sender, EventArgs e)
        {
            //----------------------------------------------------------
            // Lancer le téléchargement en fonction de la page courante.
            //----------------------------------------------------------

            if (pageSoftware.Visible == true)
            {
                if (!m_bSoftwareUpToDate)
                {
                    // Fichier source
                    string szNewVersionUri = m_hiRemote.AppInfos.FileLocation;

                    // Dossier cible, défaut = desktop.
                    string szTargetFolder = "";
                    folderBrowserDialog.Description = m_ResourceManager.GetString("Updater_BrowseFolderDescription", Thread.CurrentThread.CurrentUICulture);
                    folderBrowserDialog.ShowNewFolderButton = true;
                    folderBrowserDialog.RootFolder = Environment.SpecialFolder.Desktop;

                    if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    {
                        szTargetFolder = folderBrowserDialog.SelectedPath;

                        //-----------------------------
                        // Lancer le téléchargement.
                        //-----------------------------
                        ShowProgressBar();

                        m_Downloader = new FileDownloader();
                        m_Downloader.DownloadComplete += new EventHandler(downloader_DownloadedComplete);
                        m_Downloader.ProgressChanged += new DownloadProgressHandler(downloader_ProgressChanged);

                        m_bDownloadStarted = true;
                        m_Downloader.AsyncDownload(szNewVersionUri, szTargetFolder);
                    }
                }
            }
            else 
            {
                // Liste de fichiers.
                m_SourceItemList.Clear();

                PreferencesManager pm = PreferencesManager.Instance();
                ResourceManager SharedResources = PreferencesManager.ResourceManager;

                // Choix de la liste de fichiers à télécharger
                CheckedListBox chklstbox;
                if (pageManuals.Visible == true)
                {
                    chklstbox = chklstManuals;
                    m_TargetFolder = Application.StartupPath + "\\" + SharedResources.GetString("ManualsFolder");

                    m_HelpItemListId = 0;
                }
                else
                {
                    chklstbox = chklstVideos;
                    m_TargetFolder = Application.StartupPath + "\\" + SharedResources.GetString("HelpVideosFolder");
                    m_HelpItemListId = 1;
                }

                // Ajout de tous les fichiers à télécharger
                for(int i=0;i<chklstbox.Items.Count;i++)
                {
                    if (chklstbox.GetItemCheckState(i) == CheckState.Checked)
                    {
                        m_SourceItemList.Add( (HelpItem)chklstbox.Items[i]);
                    }
                }
                
                if (m_SourceItemList.Count > 0)
                {
                    m_Downloader = new FileDownloader();
                    m_Downloader.DownloadComplete   += new EventHandler(downloader_MultipleDownloadedComplete);
                    m_Downloader.ProgressChanged    += new DownloadProgressHandler(downloader_ProgressChanged);

                    ShowProgressBar();
                    m_bDownloadStarted = true;
                    m_CurrentSourceItemIndex = 0;
                    m_Downloader.AsyncDownload(m_SourceItemList[m_CurrentSourceItemIndex].FileLocation, m_TargetFolder);
                }
            }
        }
        #endregion

        #region Download Process
        private void UpdateProgressBar(int percentDone)
        {
            //------------------------------------------
            // Fonction dans l'espace du thread de l'UI.
            // appelée via delegate.
            //------------------------------------------
            progressDownload.Value = percentDone;
        }
        private void DownloadComplete(int result)
        {
            //---------------------------------------------------------------
            // Fonction appellée uniquement lors du téléchargement de l'appli
            // Fonction dans l'espace du thread de l'UI appelée via delegate.
            //---------------------------------------------------------------
            m_bDownloadStarted = false;
            HideProgressBar();
            m_Downloader.DownloadComplete -= new EventHandler(downloader_DownloadedComplete);

            // Message de confirmation de réussite.
            MessageBox.Show(m_ResourceManager.GetString("Updater_mboxDownloadSuccess_Description", Thread.CurrentThread.CurrentUICulture).Replace("\\n","\n"),
                            m_ResourceManager.GetString("Updater_Title", Thread.CurrentThread.CurrentUICulture),
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
        }
        private void MultipleDownloadComplete(int result)
        {
            //------------------------------------------------------------------------------
            // Fonction appellée uniquement lors du téléchargement des vidéos ou des manuels
            // Fonction dans l'espace du thread de l'UI appelée via delegate.
            //------------------------------------------------------------------------------
            
            //------------------------------------------------------------
            // Mise à jour de l'index local
            // Ecriture sur le disque au cas où l'utilisateur 
            // annule avant la fin du téléchargement de la liste entière.
            //------------------------------------------------------------
            m_hiLocal.UpdateIndex(m_SourceItemList[m_CurrentSourceItemIndex], m_HelpItemListId);
            m_hiLocal.WriteToDisk();

            //--------------------------------------------------------------------------------------------
            // Mise à jour des check list box
            // On ne peut pas refaire un populate complet car on veut garder l'état des lignes (coché/non)
            // On se contente de supprimer l'item qu'on vient de télécharger.
            //---------------------------------------------------------------------------------------------
            CheckedListBox chklstbox;
            Label lblTotal;
            if (m_HelpItemListId == 0)
            {
                chklstbox = chklstManuals;
                lblTotal = lblTotalSelectedManuals;
            }
            else
            {
                chklstbox = chklstVideos;
                lblTotal = lblTotalSelectedVideos;
            }

            // 2. Recherche de l'Item.
            bool found = false;
            int i = 0;
            while (!found && i < chklstbox.Items.Count)
            {
                if (m_SourceItemList[m_CurrentSourceItemIndex].Identification == ((HelpItem)chklstbox.Items[i]).Identification && m_SourceItemList[m_CurrentSourceItemIndex].Language == ((HelpItem)chklstbox.Items[i]).Language)
                {
                    found = true;
                    chklstbox.Items.RemoveAt(i);
                    RematchTotal(chklstbox, lblTotal);
                    CheckIfListsEmpty();
                }
                else
                {
                    i++;
                }
            }

            //---------------------------------------------
            // Téléchargement du fichier suivant ou arrêt.
            //---------------------------------------------
            if (m_CurrentSourceItemIndex >= m_SourceItemList.Count - 1)
            {
                // Liste de téléchargement terminée.
                m_bDownloadStarted = false;
                HideProgressBar();

                m_Downloader.DownloadComplete -= new EventHandler(downloader_MultipleDownloadedComplete);
            }
            else
            {
                // Lancer le prochain téléchargement.
                progressDownload.Value = 0;
                m_CurrentSourceItemIndex++;
                m_Downloader.AsyncDownload(m_SourceItemList[m_CurrentSourceItemIndex].FileLocation, m_TargetFolder);
            }
        }
        private void downloader_ProgressChanged(object sender, DownloadEventArgs e)
        {
            //--------------------------------------------------
            // On est toujours dans l'espace du WorkerThread.
            //--------------------------------------------------
            if (m_bDownloadStarted)
            {
                BeginInvoke(m_CallbackUpdateProgressBar, e.PercentDone);
            }
        }
        private void downloader_DownloadedComplete(object sender, EventArgs e)
        {
            //--------------------------------------------------
            // On est toujours dans l'espace du WorkerThread.
            //--------------------------------------------------
            if (m_CallbackDownloadComplete != null)
            {
                BeginInvoke(m_CallbackDownloadComplete, 0);
            }
        }
        private void downloader_MultipleDownloadedComplete(object sender, EventArgs e)
        {
            //--------------------------------------------------
            // On est toujours dans l'espace du WorkerThread.
            //--------------------------------------------------
            if (m_CallbackMultipleDownloadComplete != null)
            {
                BeginInvoke(m_CallbackMultipleDownloadComplete, 0);
            }
        }
        private void HideProgressBar()
        {
            progressDownload.Value = 0;
            progressDownload.Visible = false;

            lblInstruction.Visible = true;
            btnDownload.Visible = true;
        }
        private void ShowProgressBar()
        {
            progressDownload.Visible = true;

            lblInstruction.Visible = false;
            btnDownload.Visible = false;
        }
        #endregion

        #region Links
        private void lnkKinovea_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Lancer le lien par le navigateur par défaut.
            ProcessStartInfo sInfo = new ProcessStartInfo(e.Link.LinkData.ToString());
            Process.Start(sInfo);
        }
        #endregion

        #region Pages Organisation & Transitions
        private void InitPages()
        {
            Height = 456;
            Width = 500;

            // Page affichée par défaut: Software
            ShowPageSoftware();

            pageManuals.Left = 15;
            pageManuals.Top = 121;
            pageSoftware.Left = 15;
            pageSoftware.Top = 121;
            pageVideos.Left = 15;
            pageVideos.Top = 121;
        }
        private void ShowPageSoftware()
        {
            pnlButtonManuals.BorderStyle    = BorderStyle.None;
            pnlButtonVideos.BorderStyle     = BorderStyle.None;
            pageManuals.Visible             = false;
            pageVideos.Visible              = false;

            pnlButtonSoftware.BorderStyle = BorderStyle.FixedSingle; 
            pageSoftware.Visible = true;
        }
        private void ShowPageManuals()
        {
            pnlButtonSoftware.BorderStyle = BorderStyle.None;
            pnlButtonVideos.BorderStyle = BorderStyle.None;
            pageSoftware.Visible = false;
            pageVideos.Visible = false;

            pnlButtonManuals.BorderStyle = BorderStyle.FixedSingle;
            pageManuals.Visible = true;
        }
        private void ShowPageVideos()
        {
            pnlButtonManuals.BorderStyle = BorderStyle.None;
            pnlButtonSoftware.BorderStyle = BorderStyle.None;
            pageManuals.Visible = false;
            pageSoftware.Visible = false;

            pnlButtonVideos.BorderStyle = BorderStyle.FixedSingle;
            pageVideos.Visible = true;
        }
        private void btnSoftware_Click(object sender, EventArgs e)
        {
            if (!m_bDownloadStarted) { ShowPageSoftware(); }
        }
        private void lblSoftware_Click(object sender, EventArgs e)
        {
            if (!m_bDownloadStarted) { ShowPageSoftware(); }
        }
        private void btnManuals_Click(object sender, EventArgs e)
        {
            if (!m_bDownloadStarted) { ShowPageManuals(); }
        }
        private void lblManuals_Click(object sender, EventArgs e)
        {
            if (!m_bDownloadStarted) { ShowPageManuals(); }
        }
        private void btnVideos_Click(object sender, EventArgs e)
        {
            if (!m_bDownloadStarted) { ShowPageVideos(); }
        }
        private void lblVideos_Click(object sender, EventArgs e)
        {
            if (!m_bDownloadStarted) { ShowPageVideos(); }
        }
        #endregion

        #region Setup Pages & ListBox Handling
        public void SetupPages(HelpIndex _hiLocalList, HelpIndex _hiRemoteList)
        {
            // Titre Fenetre principale.
            this.Text                = "   " + m_ResourceManager.GetString("Updater_Title", Thread.CurrentThread.CurrentUICulture);
            
            //----------------------------
            // Labels Statiques Communs
            //----------------------------
            btnCancel.Text      = m_ResourceManager.GetString("Updater_Quit", Thread.CurrentThread.CurrentUICulture);
            btnDownload.Text    = m_ResourceManager.GetString("Updater_Download", Thread.CurrentThread.CurrentUICulture);
            lblInstruction.Text = m_ResourceManager.GetString("Updater_Instruction", Thread.CurrentThread.CurrentUICulture);
            lblSoftware.Text    = m_ResourceManager.GetString("Updater_LblSoftware", Thread.CurrentThread.CurrentUICulture);
            lblManuals.Text     = m_ResourceManager.GetString("Updater_LblManuals", Thread.CurrentThread.CurrentUICulture);
            lblVideos.Text      = m_ResourceManager.GetString("Updater_LblVideos", Thread.CurrentThread.CurrentUICulture);
            lblAllManualsUpToDate.Text = m_ResourceManager.GetString("Updater_LblAllManualsUpToDate", Thread.CurrentThread.CurrentUICulture);
            lblAllVideosUpToDate.Text = m_ResourceManager.GetString("Updater_LblAllVideosUpToDate", Thread.CurrentThread.CurrentUICulture);

            lblAllManualsUpToDate.Top = chklstManuals.Top;
            lblAllManualsUpToDate.Left = chklstManuals.Left;
            lblAllVideosUpToDate.Top = chklstVideos.Top;
            lblAllVideosUpToDate.Left = chklstVideos.Left;

            //----------------------------
            // Page Software
            //----------------------------

            // Don't use the version info from the file, it may not be correct.
            ThreePartsVersion currentVersion = new ThreePartsVersion(PreferencesManager.ReleaseVersion);

            if (currentVersion < _hiRemoteList.AppInfos.Version)
            {
                labelInfos.Text = m_ResourceManager.GetString("Updater_Behind", Thread.CurrentThread.CurrentUICulture);

                String szNewVersion = m_ResourceManager.GetString("Updater_NewVersion", Thread.CurrentThread.CurrentUICulture);
                szNewVersion += " : " + _hiRemoteList.AppInfos.Version.ToString() + " - ( ";
                szNewVersion += m_ResourceManager.GetString("Updater_CurrentVersion", Thread.CurrentThread.CurrentUICulture);
                szNewVersion += " " + _hiLocalList.AppInfos.Version.ToString() + ")";
                lblNewVersion.Text = szNewVersion;

                String szNewVersionFileSize = m_ResourceManager.GetString("Updater_FileSize", Thread.CurrentThread.CurrentUICulture);
                szNewVersionFileSize += " " + String.Format("{0:0.00}",((double)_hiRemoteList.AppInfos.FileSizeInBytes / (1024*1024))) + " ";
                szNewVersionFileSize += m_ResourceManager.GetString("Updater_MegaBytes", Thread.CurrentThread.CurrentUICulture);
                lblNewVersionFileSize.Text = szNewVersionFileSize;

                lblChangeLog.Text = m_ResourceManager.GetString("Updater_LblChangeLog", Thread.CurrentThread.CurrentUICulture);
                lblChangeLog.Visible = true;
                
                // Load Changelog
                //txtChangeLog.Clear();
                rtbxChangeLog.Clear();
                if (_hiRemoteList.AppInfos.ChangelogLocation.Length > 0)
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_hiRemoteList.AppInfos.ChangelogLocation);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode == HttpStatusCode.OK && response.ContentLength > 0)
                    {
                        TextReader reader = new StreamReader(response.GetResponseStream());
                        
                        string line;
                        while ((line = reader.ReadLine()) != null) 
                        {
                            //txtChangeLog.AppendText("\n");
                            //txtChangeLog.AppendText(line);

                            rtbxChangeLog.AppendText("\n");
                            rtbxChangeLog.AppendText(line);
                        }
                    }
                    
                     //Dim URLReq As Net.HttpWebRequest
                       //Dim URLRes As Net.HttpWebRequest
                        //Dim FileStreamer As New IO.FileStream("c:\temp\file1", IO.FileMode.CreateNew)
                    
                    
                    
                    //StreamReader sr2  = new StreamReader(_hiRemoteList.AppInfos.ChangelogLocation);
                    //StringReader sr3  = new StringReader(_hiRemoteList.AppInfos.ChangelogLocation);

                    //FileStream stream = new FileStream(_hiRemoteList.AppInfos.ChangelogLocation, FileMode.Open, FileAccess.Read);
                    //rtbxChangeLog.LoadFile(stream, RichTextBoxStreamType.RichText);
                }
                rtbxChangeLog.Visible = true;
                
                m_bSoftwareUpToDate = false;
            }
            else
            {
                // OK à jour.
                labelInfos.Text = m_ResourceManager.GetString("Updater_UpToDate", Thread.CurrentThread.CurrentUICulture);
                lblNewVersion.Visible = false;
                lblNewVersionFileSize.Visible = false;
                lblChangeLog.Visible = false;
                rtbxChangeLog.Visible = false;
                
                m_bSoftwareUpToDate = true;
            }

            //--------------------------------
            // Page Manuals
            //--------------------------------
            lblSelectManual.Text = m_ResourceManager.GetString("Updater_LblSelectManuals", Thread.CurrentThread.CurrentUICulture);

            PopulateCheckedListBox(chklstManuals, _hiRemoteList.UserGuides, _hiLocalList.UserGuides, "");
            AutoCheckCulture(chklstManuals);

            String szTotalSelected = m_ResourceManager.GetString("Updater_LblTotalSelected", Thread.CurrentThread.CurrentUICulture);
            szTotalSelected += " " + String.Format("{0:0.0}", (double)ComputeTotalBytes(chklstManuals) / (1024 * 1024)) + " ";
            szTotalSelected += m_ResourceManager.GetString("Updater_MegaBytes", Thread.CurrentThread.CurrentUICulture);
            lblTotalSelectedManuals.Text = szTotalSelected;

            //---------------------------------------------------------------------------------------------------------
            // Page Videos
            //---------------------------------------------------------------------------------------------------------
            lblSelectVideos.Text = m_ResourceManager.GetString("Updater_LblSelectVideos", Thread.CurrentThread.CurrentUICulture);
            lblFilterByLanguage.Text = m_ResourceManager.GetString("Updater_LblFilterByLang", Thread.CurrentThread.CurrentUICulture);
            PopulateFilterComboBox();

            string szCultureName = ((LanguageIdentifier)cmbLanguageFilter.Items[cmbLanguageFilter.SelectedIndex]).CultureName;
            PopulateCheckedListBox(chklstVideos, _hiRemoteList.HelpVideos, _hiLocalList.HelpVideos, szCultureName);
            AutoCheckCulture(chklstVideos);
            RematchTotal(chklstVideos, lblTotalSelectedVideos);

            CheckIfListsEmpty();

            // Alignement à droite avec la RichTextBox du changelog.
            lblNewVersionFileSize.Left = rtbxChangeLog.Left + rtbxChangeLog.Width - lblNewVersionFileSize.Width;
        }
        private void PopulateCheckedListBox(CheckedListBox _chklstbox, List<HelpItem> _remoteItems, List<HelpItem> _localItems, string _CultureName)
        {
            _chklstbox.Items.Clear();
            foreach (HelpItem RemoteItem in _remoteItems)
            {
                // Format : 
                // Title ( up to date | update available | new item, file size ) 
                ItemStatus RemoteStatus = ItemStatus.Synched;
                string szDescription = RemoteItem.LocalizedTitle;
                szDescription += " ( ";

                // Is it a new item, if yes is it synched already ?
                bool found = false;
                int i = 0;
                while (!found && i < _localItems.Count)
                {
                    
                    if ( RemoteItem.Identification == _localItems[i].Identification && RemoteItem.Language == _localItems[i].Language)
                    {
                        found = true;
                        if (RemoteItem.Revision > _localItems[i].Revision)
                        {
                            RemoteStatus = ItemStatus.NeedUpdate;
                            szDescription += m_ResourceManager.GetString("Updater_HelpItem_NeedUpdate", Thread.CurrentThread.CurrentUICulture);
                        }
                        else
                        {
                            RemoteStatus = ItemStatus.Synched;
                            szDescription += m_ResourceManager.GetString("Updater_HelpItem_UpToDate", Thread.CurrentThread.CurrentUICulture);
                        }
                    }
                    else
                    {
                        i++;
                    }
                }

                if (!found)
                {
                    RemoteStatus = ItemStatus.NewItem;
                    szDescription += m_ResourceManager.GetString("Updater_HelpItem_NewItem", Thread.CurrentThread.CurrentUICulture);
                }

                szDescription += ", " + String.Format("{0:0.0}", ((double)RemoteItem.FileSizeInBytes / (1024 * 1024)));
                szDescription += " " + m_ResourceManager.GetString("Updater_MegaBytes", Thread.CurrentThread.CurrentUICulture);
                szDescription += " )";

                RemoteItem.Description = szDescription;

                if (RemoteStatus != ItemStatus.Synched)
                {
                    // Filtre langue
                    if (_CultureName == "" || RemoteItem.Language == _CultureName)
                    {
                        int row = _chklstbox.Items.Add(RemoteItem, false);
                    }
                }
            }


        }
        private void chklstManuals_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // Recalculer le total à télécharger. L'état de la ligne cliquée n'est pas encore mis à jour.
            int iTotal = ComputeTotalBytes(chklstManuals);
            if (e.NewValue == CheckState.Checked)
            {
                iTotal += ((HelpItem)chklstManuals.Items[e.Index]).FileSizeInBytes;
            }
            else if (e.NewValue == CheckState.Unchecked)
            {
                iTotal -= ((HelpItem)chklstManuals.Items[e.Index]).FileSizeInBytes;
            }

            String szTotalSelected = m_ResourceManager.GetString("Updater_LblTotalSelected", Thread.CurrentThread.CurrentUICulture);
            szTotalSelected += " " + String.Format("{0:0.0}", (double)iTotal / (1024 * 1024)) + " ";
            szTotalSelected += m_ResourceManager.GetString("Updater_MegaBytes", Thread.CurrentThread.CurrentUICulture);
            lblTotalSelectedManuals.Text = szTotalSelected;

        }
        private void chklstVideos_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // Recalculer le total à télécharger. L'état de la ligne cliquée n'est pas encore mis à jour.
            int iTotal = ComputeTotalBytes(chklstVideos);
            if (e.NewValue == CheckState.Checked)
            {
                iTotal += ((HelpItem)chklstVideos.Items[e.Index]).FileSizeInBytes;
            }
            else if (e.NewValue == CheckState.Unchecked)
            {
                iTotal -= ((HelpItem)chklstVideos.Items[e.Index]).FileSizeInBytes;
            }
            
            String szTotalSelected = m_ResourceManager.GetString("Updater_LblTotalSelected", Thread.CurrentThread.CurrentUICulture);
            szTotalSelected += " " + String.Format("{0:0.0}", (double)iTotal / (1024 * 1024)) + " ";
            szTotalSelected += m_ResourceManager.GetString("Updater_MegaBytes", Thread.CurrentThread.CurrentUICulture);
            lblTotalSelectedVideos.Text = szTotalSelected;
        }
        private int ComputeTotalBytes(CheckedListBox _chklstbox)
        {
            int iTotal = 0;

            for (int i = 0; i < _chklstbox.Items.Count; i++)
            {
                if (_chklstbox.GetItemCheckState(i) == CheckState.Checked)
                {
                    iTotal += ((HelpItem)_chklstbox.Items[i]).FileSizeInBytes;
                }
            }

            return iTotal;
        }
        private void PopulateFilterComboBox()
        {
            cmbLanguageFilter.Items.Clear();

            LanguageIdentifier liAll = new LanguageIdentifier("", m_ResourceManager.GetString("Updater_FilterAll", Thread.CurrentThread.CurrentUICulture));
            LanguageIdentifier liEnglish = new LanguageIdentifier("en", PreferencesManager.LanguageEnglish);
            LanguageIdentifier liFrench = new LanguageIdentifier("fr", PreferencesManager.LanguageFrench);

            cmbLanguageFilter.Items.Add(liAll);
            cmbLanguageFilter.Items.Add(liEnglish);
            cmbLanguageFilter.Items.Add(liFrench);

            cmbLanguageFilter.SelectedIndex = 0;
        }
        private void cmbLanguageFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Changement de langue.
            string szIsoLang = ((LanguageIdentifier)cmbLanguageFilter.Items[cmbLanguageFilter.SelectedIndex]).CultureName;
            PopulateCheckedListBox(chklstVideos, m_hiRemote.HelpVideos, m_hiLocal.HelpVideos, szIsoLang);
            AutoCheckCulture(chklstVideos);

            // On doit s'assurer que le total a été mis à jour, même si aucune action de check a été faite.
            RematchTotal(chklstVideos, lblTotalSelectedVideos);

            CheckIfListsEmpty();
        }
        private void AutoCheckCulture(CheckedListBox _chklstbox)
        {
            for (int i = 0; i < _chklstbox.Items.Count; i++)
            {
            	if (((HelpItem)_chklstbox.Items[i]).Language == PreferencesManager.Instance().GetSupportedCulture().Name)
                {
                    _chklstbox.SetItemChecked(i, true);
                }
            }
        }
        private void RematchTotal(CheckedListBox _chklstbox, Label _lblTotalSelected)
        {
            string szTotalSelected = m_ResourceManager.GetString("Updater_LblTotalSelected", Thread.CurrentThread.CurrentUICulture);
            szTotalSelected += " " + String.Format("{0:0.0}", (double)ComputeTotalBytes(_chklstbox) / (1024 * 1024)) + " ";
            szTotalSelected += m_ResourceManager.GetString("Updater_MegaBytes", Thread.CurrentThread.CurrentUICulture);
            _lblTotalSelected.Text = szTotalSelected;
        }
        private void CheckIfListsEmpty()
        {
            if (chklstManuals.Items.Count == 0)
            {
                chklstManuals.Visible = false;
                lblTotalSelectedManuals.Visible = false;
                lblAllManualsUpToDate.Visible = true;
            }
            else
            {
                chklstManuals.Visible = true;
                lblTotalSelectedManuals.Visible = true;
                lblAllManualsUpToDate.Visible = false;
            }

            if (chklstVideos.Items.Count == 0)
            {
                chklstVideos.Visible = false;
                lblTotalSelectedVideos.Visible = false;
                lblAllVideosUpToDate.Visible = true;
                if (((LanguageIdentifier)cmbLanguageFilter.SelectedItem).CultureName == "")
                {
                    lblAllVideosUpToDate.Text = m_ResourceManager.GetString("Updater_LblAllVideosUpToDate", Thread.CurrentThread.CurrentUICulture);
                    //lblFilterByLanguage.Visible = false;
                    //cmbLanguageFilter.Visible = false;
                }
                else
                {
                    lblAllVideosUpToDate.Text = m_ResourceManager.GetString("Updater_LblAllVideosUpToDateCategory", Thread.CurrentThread.CurrentUICulture);
                    //lblFilterByLanguage.Visible = true;
                    //cmbLanguageFilter.Visible = true;
                }
            }
            else
            {
                chklstVideos.Visible = true;
                lblTotalSelectedVideos.Visible = true;
                lblAllVideosUpToDate.Visible = false;
                lblFilterByLanguage.Visible = true;
                cmbLanguageFilter.Visible = true;
            }
        }
        #endregion

    }
}