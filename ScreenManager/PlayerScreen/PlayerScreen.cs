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
using System.Text;
using System.Resources;
using System.Reflection;
using VideaPlayerServer;
using System.Windows.Forms;
using Videa.Services;

[assembly: CLSCompliant(false)]
namespace Videa.ScreenManager
{
    public class PlayerScreen : AbstractScreen
    {
        #region Delegates
       	// Assigned and implemented in ScreenManager.
       	// non visible here : (DelegateCloseMe) CloseMe, (DelegateSetMeAsActiveScreen) SetMeAsActiveScreen.
       	public delegate void PlayerIsReady(PlayerScreen _screen, bool _bIntervalOnly);
        public delegate void PlayerSelectionChanged(PlayerScreen _screen, bool _bInitialization);
        
        public PlayerIsReady           	m_PlayerIsReady;
        public PlayerSelectionChanged  	m_PlayerSelectionChanged;
        #endregion

        #region Properties
        public override bool Full
        {
        	get { return m_PlayerScreenUI.m_PlayerServer.m_bIsMovieLoaded; }	
        }
        public override UserControl UI
        {
        	get { return m_PlayerScreenUI; }	
        }
        public override Guid UniqueId
        {
            get { return m_UniqueId; }
            set { m_UniqueId = value; }
        }
        
        public bool HasMovie
        {
            get { return m_PlayerScreenUI.m_PlayerServer.m_bIsMovieLoaded; }
        }
        public bool IsPlaying
        {
            get
            {
                if (!m_PlayerScreenUI.m_PlayerServer.m_bIsMovieLoaded)
                {
                    return false;
                }
                else
                {
                    return (m_PlayerScreenUI.IsCurrentlyPlaying);
                }
            }
        }
        public bool IsInAnalysisMode
        {
            get
            {
                if (!m_PlayerScreenUI.m_PlayerServer.m_bIsMovieLoaded)
                {
                    return false;
                }
                else
                {
                    return (m_PlayerScreenUI.m_PlayerServer.m_PrimarySelection.iAnalysisMode == 1) ? true : false;
                }
            }
        }
        public int CurrentFrame
        {
            get
            {
                // Get the approximate frame we should be on.
                // Only as accurate as the framerate is stable regarding to the timebase.
                Int64 iCurrentTimestamp = m_PlayerScreenUI.m_PlayerServer.m_PrimarySelection.iCurrentTimeStamp;
                iCurrentTimestamp -= m_PlayerScreenUI.m_PlayerServer.m_InfosVideo.iFirstTimeStamp;

                return (int)(iCurrentTimestamp / m_PlayerScreenUI.m_PlayerServer.m_InfosVideo.iAverageTimeStampsPerFrame);
            }

            set
            {
                m_PlayerScreenUI.SetCurrentFrame(value);
            }
        }
        public Int64 Position
        {
            // Used to feed SyncPosition. 
            get { return m_PlayerScreenUI.m_PlayerServer.m_PrimarySelection.iCurrentTimeStamp - m_PlayerScreenUI.m_PlayerServer.m_InfosVideo.iFirstTimeStamp; }
        }
        public int LastFrame
        {
            get 
            {
                if (m_PlayerScreenUI.m_PlayerServer.m_PrimarySelection.iAnalysisMode == 1)
                {
                    return m_PlayerScreenUI.m_PlayerServer.m_PrimarySelection.iDurationFrame - 1;
                }
                else
                {
                    Int64 iDurationTimestamp = m_PlayerScreenUI.m_PlayerServer.m_InfosVideo.iDurationTimeStamps;
                    return (int)(iDurationTimestamp / m_PlayerScreenUI.m_PlayerServer.m_InfosVideo.iAverageTimeStampsPerFrame) -1;
                }
            }
        }
        public int FrameInterval
        {
            get { return m_PlayerScreenUI.GetFrameInterval(); }
        }
        public bool Synched
        {
            //get { return m_PlayerScreenUI.m_bSynched; }
            set { m_PlayerScreenUI.Synched = value; }
        }
        public Int64 SyncPosition
        {
            // Reference timestamp for synchronization, expressed in local timebase.
            get { return m_PlayerScreenUI.SyncPosition; }
            set { m_PlayerScreenUI.SyncPosition = value; }
        }

        public String FilePath
        {
            get { return m_FullFileName; }
            set { m_FullFileName = value; }
        }
        
        // Pseudo Filters (Impacts rendering)
        public bool Deinterlaced
        {
            get { return m_PlayerScreenUI.Deinterlaced; }
            set
            {
                m_PlayerScreenUI.Deinterlaced = value;
                RefreshImage();
            }
        }
        public bool Mirrored
        {
            get { return m_PlayerScreenUI.Mirrored; }
            set
            {
                m_PlayerScreenUI.Mirrored = value;
                RefreshImage();
            }
        }
        public bool ShowGrid
        {
            get { return m_PlayerScreenUI.ShowGrid; }
            set
            {
                m_PlayerScreenUI.ShowGrid = value;
                RefreshImage();
            }
        }
        public bool Show3DPlane
        {
            get { return m_PlayerScreenUI.Show3DPlane; }
            set
            {
                m_PlayerScreenUI.Show3DPlane = value;
                RefreshImage();
            }
        }
        public int DrawtimeFilterType
        {
        	get {return m_PlayerScreenUI.DrawtimeFilterType;}
        }
        #endregion

        #region members
        public bool                         m_bIsMovieLoaded;
        public PlayerScreenUserInterface    m_PlayerScreenUI;
        public String                       m_sFileName;
        private string                      m_FullFileName;
        public ResourceManager              m_ResourceManager;
        private Guid m_UniqueId;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public PlayerScreen()
        {
            log.Debug("Constructing a PlayerScreen.");
            m_UniqueId = System.Guid.NewGuid();

            //Gestion i18n
            m_ResourceManager = new ResourceManager("Videa.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
            
            m_bIsMovieLoaded = false;
            m_sFileName = "";

            // Create UI and PlayerServer.
            m_PlayerScreenUI = new PlayerScreenUserInterface(m_ResourceManager);

            // UI Delegates. (Assigned and implemented here, called from UI)
            m_PlayerScreenUI.m_CloseMeUI += new PlayerScreenUserInterface.DelegateCloseMeUI(ScreenUI_CloseAsked);
            m_PlayerScreenUI.m_SetMeAsActiveScreenUI += new PlayerScreenUserInterface.DelegateSetMeAsActiveScreenUI(ScreenUI_SetAsActiveScreen);
            
            m_PlayerScreenUI.m_ReportReady                  += new PlayerScreenUserInterface.ReportReady(PlayerScreenUI_IsReady);
            m_PlayerScreenUI.m_ReportSelectionChanged       += new PlayerScreenUserInterface.ReportSelectionChanged(PlayerScreenUI_SelectionChanged);
        }
        #endregion

        #region AbstractScreen Implementation
        public override void DisplayAsInactiveScreen()
        {
            //--------------------------------------------------------------------------------------------------------
            // L'écran n'est plus l'écran actif.
            // Fonction appelée depuis le ScreenManager, lorsqu'un autre écran vient de se signaler comme écran actif.
            //--------------------------------------------------------------------------------------------------------
            m_PlayerScreenUI.DisplayAsInactiveScreen();
        }
        public override void DisplayAsActiveScreen()
        {
            m_PlayerScreenUI.DisplayAsActiveScreen();
        }
        public override void CloseScreen()
        {
            // Fonction appelée lors de la fermeture complète de l'appli.
            //base.CloseScreen();
            m_PlayerScreenUI.StopPlaying();
            m_PlayerScreenUI.UnloadMovie();
        }
        public override void refreshUICulture()
        {
            // Changement de langue demandé (ou chargement)
            m_PlayerScreenUI.RefreshUICulture(m_ResourceManager);
        }
        public override bool OnKeyPress(Keys _keycode)
        {
        	return m_PlayerScreenUI.OnKeyPress(_keycode);
        }
        public override void RefreshImage()
        {
            m_PlayerScreenUI.RefreshImage();
        }
        #endregion

        #region Délégués appelées depuis l'UI
        private void ScreenUI_CloseAsked()
        {
        	// Note: CloseMe variable is:
        	// - defined in AbstractScreen 
        	// - assigned in CommandAddPlayerScreen.
        	// - implemented in ScreenManager. 
            if (CloseMe != null) { CloseMe(this); }
        }
        private void ScreenUI_SetAsActiveScreen()
        {
            // Note: SetMeAsActiveScreen variable is:
        	// - defined in AbstractScreen 
        	// - assigned in CommandAddPlayerScreen.
        	// - implemented in ScreenManager. 
            if (SetMeAsActiveScreen != null) { SetMeAsActiveScreen(this); }
        }
        private void PlayerScreenUI_IsReady(bool _bIntervalOnly)
        {
            // Utilisé dans le cadre de la synchro.
            if (m_PlayerIsReady != null) { m_PlayerIsReady(this, _bIntervalOnly); }
        }
        private void PlayerScreenUI_SelectionChanged(bool _bInitialization)
        {
            // Utilisé dans le cadre de la synchro.
            if (m_PlayerSelectionChanged != null) { m_PlayerSelectionChanged(this, _bInitialization); }
        }
        #endregion
        
        public void StopPlaying()
        {
            // fonction appelée depuis le Superviseur, via le ScreenManager
            // Lorsque l'utilisateur lance la boîte de dialogue ouvrir.
            m_PlayerScreenUI.StopPlaying();
        }

        public void GotoNextFrame()
        {
            m_PlayerScreenUI.SetCurrentFrame(-1);
        }

        public MemoPlayerScreen GetMemo()
        {
            return m_PlayerScreenUI.GetMemo();
        }
        public void ResetSelectionImages(MemoPlayerScreen _memo)
        {
            m_PlayerScreenUI.ResetSelectionImages(_memo);
        }
        public void SetDrawingtimeFilterOutput(DrawtimeFilterOutput _dfo)
        {
        	// A video filter just finished and is passing us its output object.
        	// It is used as a communication channel between the filter and the player.
        	m_PlayerScreenUI.SetDrawingtimeFilterOutput(_dfo);
        }
    }
}
