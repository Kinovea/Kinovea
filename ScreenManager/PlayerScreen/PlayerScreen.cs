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
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows.Forms;

using Kinovea.Services;
using Kinovea.VideoFiles;

[assembly: CLSCompliant(false)]
namespace Kinovea.ScreenManager
{
    public class PlayerScreen : AbstractScreen, IPlayerScreenUIHandler
    {
        #region Properties
        public override bool Full
        {
        	get { return m_FrameServer.VideoFile.Loaded; }	
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
		public override string FilePath
		{
			get { return m_FrameServer.VideoFile.FilePath; }
		}
		public override bool CapabilityDrawings
		{
			get { return true;}
		}
		public string FileName
		{
			get { return Path.GetFileName(m_FrameServer.VideoFile.FilePath); }
		}
        public FrameServerPlayer FrameServer
		{
			get { return m_FrameServer; }
			set { m_FrameServer = value; }
		}        
        public bool IsPlaying
        {
            get
            {
                if (!m_FrameServer.VideoFile.Loaded)
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
                if (!m_FrameServer.VideoFile.Loaded)
                {
                    return false;
                }
                else
                {
                    return (m_FrameServer.VideoFile.Selection.iAnalysisMode == 1);
                }
            }
        }
        public int CurrentFrame
        {
            get
            {
                // Get the approximate frame we should be on.
                // Only as accurate as the framerate is stable regarding to the timebase.
                
                // Timestamp (relative to selection start).
                Int64 iCurrentTimestamp = m_PlayerScreenUI.SyncCurrentPosition;
                return (int)(iCurrentTimestamp / m_FrameServer.VideoFile.Infos.iAverageTimeStampsPerFrame);
            }
            set
            {
                m_PlayerScreenUI.SyncSetCurrentFrame(value);
            }
        }
        public int LastFrame
        {
            get 
            {
                if (m_FrameServer.VideoFile.Selection.iAnalysisMode == 1)
                {
                    return m_FrameServer.VideoFile.Selection.iDurationFrame - 1;
                }
                else
                {
                    Int64 iDurationTimestamp = m_FrameServer.VideoFile.Infos.iDurationTimeStamps;
                    return (int)(iDurationTimestamp / m_FrameServer.VideoFile.Infos.iAverageTimeStampsPerFrame) -1;
                }
            }
        }
        public int FrameInterval
        {
            get 
            { 
            	// Returns the playback interval between frames in Milliseconds, taking slow motion slider into account.
				if (m_FrameServer.VideoFile.Loaded && m_FrameServer.VideoFile.Infos.iFrameInterval > 0)
				{
					return (int)((double)m_FrameServer.VideoFile.Infos.iFrameInterval / ((double)m_PlayerScreenUI.SlowmotionPercentage / 100));
				}
				else
				{
					return 40;
				}
	        }
        }
        public bool Synched
        {
            //get { return m_PlayerScreenUI.m_bSynched; }
            set { m_PlayerScreenUI.Synched = value;}
        }
        public Int64 SyncPosition
        {
            // Reference timestamp for synchronization, expressed in local timebase.
            get { return m_PlayerScreenUI.SyncPosition; }
            set { m_PlayerScreenUI.SyncPosition = value; }
        }
        public Int64 Position
        {
            // Used to feed SyncPosition. 
            get { return m_FrameServer.VideoFile.Selection.iCurrentTimeStamp - m_FrameServer.VideoFile.Infos.iFirstTimeStamp; }
        }
        public bool SyncMerge
        {
        	set 
        	{
        		m_PlayerScreenUI.SyncMerge = value;
        		RefreshImage();
        	}
        }
        public Bitmap SyncMergeImage
		{
			set 
			{
				m_PlayerScreenUI.SyncMergeImage = value;
			}
		}
        
        // Pseudo Filters (Impacts rendering)
        public bool Deinterlaced
        {
            get { return m_FrameServer.VideoFile.Infos.bDeinterlaced; }
            set
            {
                m_FrameServer.VideoFile.Infos.bDeinterlaced = value;
                
                // If there was a selection it must be imported again.
				// (This means we'll loose color adjustments.)
				if (m_FrameServer.VideoFile.Selection.iAnalysisMode == 1)
				{
					m_PlayerScreenUI.ImportSelectionToMemory(true);
				}
                
				RefreshImage();
            }
        }
        public VideoFiles.AspectRatio AspectRatio
        {
            get { return m_FrameServer.VideoFile.Infos.eAspectRatio; }
            set
            {
                m_FrameServer.VideoFile.ChangeAspectRatio(value);
                
                if (m_FrameServer.VideoFile.Selection.iAnalysisMode == 1)
				{
					m_PlayerScreenUI.ImportSelectionToMemory(true);
				}
                
                m_PlayerScreenUI.UpdateImageSize();
                RefreshImage();
            }
        }
        public bool Mirrored
        {
            get { return m_FrameServer.Metadata.Mirrored; }
            set
            {
                m_FrameServer.Metadata.Mirrored = value;
                RefreshImage();
            }
        }
        public bool ShowGrid
        {
            get { return m_FrameServer.Metadata.Grid.Visible; }
            set
            {
                m_FrameServer.Metadata.Grid.Visible = value;
                RefreshImage();
            }
        }
        public bool Show3DPlane
        {
            get { return m_FrameServer.Metadata.Plane.Visible; }
            set
            {
                m_FrameServer.Metadata.Plane.Visible = value;
                RefreshImage();
            }
        }
        public int DrawtimeFilterType
        {
        	get {return m_PlayerScreenUI.DrawtimeFilterType;}
        }
        #endregion

        #region members
        public PlayerScreenUserInterface m_PlayerScreenUI;
		
        private IScreenHandler m_ScreenHandler;
        private FrameServerPlayer m_FrameServer = new FrameServerPlayer();
        private Guid m_UniqueId;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public PlayerScreen(IScreenHandler _screenHandler)
        {
            log.Debug("Constructing a PlayerScreen.");
            m_ScreenHandler = _screenHandler;
            m_UniqueId = System.Guid.NewGuid();
            m_PlayerScreenUI = new PlayerScreenUserInterface(m_FrameServer, this);
        }
        #endregion

        #region IPlayerScreenUIHandler (and IScreenUIHandler) implementation
        public void ScreenUI_CloseAsked()
        {
        	m_ScreenHandler.Screen_CloseAsked(this);
        }
        public void ScreenUI_SetAsActiveScreen()
        {
        	m_ScreenHandler.Screen_SetActiveScreen(this);
        }
        public void PlayerScreenUI_IsReady(bool _bIntervalOnly)
        {
            // Used for synchronisation handling.
            m_ScreenHandler.Player_IsReady(this, _bIntervalOnly);
        }
        public void PlayerScreenUI_SelectionChanged(bool _bInitialization)
        {
            // Used for synchronisation handling.
            m_ScreenHandler.Player_SelectionChanged(this, _bInitialization);
        }
        public void PlayerScreenUI_ImageChanged(Bitmap _image)
        {
        	m_ScreenHandler.Player_ImageChanged(this, _image);
        }
        public void PlayerScreenUI_Reset()
        {
        	m_ScreenHandler.Player_Reset(this);
        }
        #endregion
        
        #region AbstractScreen Implementation
        public override void DisplayAsActiveScreen(bool _bActive)
        {
            m_PlayerScreenUI.DisplayAsActiveScreen(_bActive);
        }
        public override void BeforeClose()
        {
            // Called by the ScreenManager when this screen is about to be closed.
            m_PlayerScreenUI.StopPlaying();
            m_PlayerScreenUI.ResetToEmptyState();
        }
        public override void refreshUICulture()
        {
            m_PlayerScreenUI.RefreshUICulture();
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
        
        #region Other public methods called from the ScreenManager
        public void StopPlaying()
        {
            m_PlayerScreenUI.StopPlaying();
        }
        public void GotoNextFrame()
        {
            m_PlayerScreenUI.SyncSetCurrentFrame(-1);
        }
        public void ResetSelectionImages(MemoPlayerScreen _memo)
        {
            m_PlayerScreenUI.ResetSelectionImages(_memo);
        }
        public MemoPlayerScreen GetMemo()
        {
            return m_PlayerScreenUI.GetMemo();
        }
        public void SetDrawingtimeFilterOutput(DrawtimeFilterOutput _dfo)
        {
        	// A video filter just finished and is passing us its output object.
        	// It is used as a communication channel between the filter and the player.
        	m_PlayerScreenUI.SetDrawingtimeFilterOutput(_dfo);
        }
        public void Save()
        {
        	m_PlayerScreenUI.Save();
        }
        #endregion
    }
}
