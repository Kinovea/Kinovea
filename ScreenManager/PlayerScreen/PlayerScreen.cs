#region Licence
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
#endregion

using Kinovea.ScreenManager.Languages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows.Forms;
using Kinovea.Services;

[assembly: CLSCompliant(false)]
namespace Kinovea.ScreenManager
{
    public class PlayerScreen : AbstractScreen, IPlayerScreenUIHandler
    {
        #region Properties
        public override bool Full
        {
        	get { return m_FrameServer.Loaded; }	
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
		public override string FileName
		{
			get 
			{ 
				if(m_FrameServer.Loaded)
				{
					return Path.GetFileName(m_FrameServer.VideoReader.FilePath);		
				}
				else
				{
					return ScreenManagerLang.statusEmptyScreen;	
				}
			}
		}
		public override string Status
		{
			get	{return FileName;}
		}
		public override string FilePath
		{
			get { return m_FrameServer.VideoReader.FilePath; }
		}
		public override bool CapabilityDrawings
		{
			get { return true;}
		}
		public override ImageAspectRatio AspectRatio
        {
            get { return m_FrameServer.VideoReader.ImageAspectRatio; }
            set
            {
                m_FrameServer.VideoReader.ImageAspectRatio = value;
                
                if (m_FrameServer.VideoReader.Caching)
					m_PlayerScreenUI.ImportSelectionToMemory(true);
                
                m_PlayerScreenUI.UpdateImageSize();
                RefreshImage();
            }
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
                if (!m_FrameServer.Loaded)
                    return false;
                else
                    return m_PlayerScreenUI.IsCurrentlyPlaying;
            }
        }
        public bool IsSingleFrame
        {
        	get
            {
                if (!m_FrameServer.Loaded)
                    return false;
                else
                    return m_FrameServer.VideoReader.SingleFrame;
            }	
        }
        public bool IsCaching
        {
            get
            {
                if (!m_FrameServer.Loaded)
                    return false;
                else
                    return m_FrameServer.VideoReader.Caching;
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
                return (int)(iCurrentTimestamp / m_FrameServer.VideoReader.Info.AverageTimeStampsPerFrame);
            }
        }
        public int SelectionLastTimestamp
        {
            get 
            {
                return (int)m_FrameServer.VideoReader.WorkingZone.End;
            }
        }
        public double FrameInterval
        {
            get 
            { 
            	// Returns the playback interval between frames in Milliseconds, taking slow motion slider into account.
				if (m_FrameServer.Loaded && m_FrameServer.VideoReader.Info.FrameIntervalMilliseconds > 0)
					return m_PlayerScreenUI.FrameInterval;
				else
					return 40;
	        }
        }
        public double RealtimePercentage
        {
        	get { return m_PlayerScreenUI.RealtimePercentage; }
        	set { m_PlayerScreenUI.RealtimePercentage = value;}
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
            get { return m_FrameServer.VideoReader.Current.Timestamp - m_FrameServer.VideoReader.Info.FirstTimeStamp; }
        }
        public bool SyncMerge
        {
        	set 
        	{
        		m_PlayerScreenUI.SyncMerge = value;
        		RefreshImage();
        	}
        }
        public bool DualSaveInProgress
        {
        	set { m_PlayerScreenUI.DualSaveInProgress = value; }
        }
        
        // Pseudo Filters (Impacts rendering)
        public bool Deinterlaced
        {
            get { return m_FrameServer.VideoReader.Deinterlace; }
            set
            {
                m_FrameServer.VideoReader.Deinterlace = value;
                
                // If there was a selection it must be imported again.
				// (This means we'll loose color adjustments.)
				if (m_FrameServer.VideoReader.Caching)
					m_PlayerScreenUI.ImportSelectionToMemory(true);
                
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
        public void ScreenUI_UpdateStatusBarAsked()
        {
        	m_ScreenHandler.Screen_UpdateStatusBarAsked(this);
        }
        public void PlayerScreenUI_SpeedChanged(bool _bIntervalOnly)
        {
            // Used for synchronisation handling.
            m_ScreenHandler.Player_SpeedChanged(this, _bIntervalOnly);
        }
        public void PlayerScreenUI_PauseAsked()
        {
        	m_ScreenHandler.Player_PauseAsked(this);
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
        public void PlayerScreenUI_SendImage(Bitmap _image)
        {
        	m_ScreenHandler.Player_SendImage(this, _image);
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
            // Note: We shouldn't call ResetToEmptyState here because we will want
            // the close screen routine to detect if there is something left in the 
            // metadata and alerts the user.
            m_PlayerScreenUI.StopPlaying();
        }
        public override void refreshUICulture()
        {
            m_PlayerScreenUI.RefreshUICulture();
        }
        public override bool OnKeyPress(Keys _key)
        {
        	return m_PlayerScreenUI.OnKeyPress(_key);
        }
        public override void RefreshImage()
        {
            m_PlayerScreenUI.RefreshImage();
        }
        public override void AddImageDrawing(string _filename, bool _bIsSvg)
        {
        	m_PlayerScreenUI.AddImageDrawing(_filename, _bIsSvg);
        }
        public override void AddImageDrawing(Bitmap _bmp)
        {
        	m_PlayerScreenUI.AddImageDrawing(_bmp);
        }
        public override void FullScreen(bool _bFullScreen)
        {
            m_PlayerScreenUI.FullScreen(_bFullScreen);
        }
        #endregion
        
        #region Other public methods called from the ScreenManager
        public void StopPlaying()
        {
            m_PlayerScreenUI.StopPlaying();
        }
        public void GotoNextFrame(bool _bAllowUIUpdate)
        {
            m_PlayerScreenUI.SyncSetCurrentFrame(-1, _bAllowUIUpdate);
        }
        public void GotoFrame(int _frame, bool _bAllowUIUpdate)
        {
        	m_PlayerScreenUI.SyncSetCurrentFrame(_frame, _bAllowUIUpdate);
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
        public void SetSyncMergeImage(Bitmap _SyncMergeImage, bool _bUpdateUI)
		{
        	m_PlayerScreenUI.SetSyncMergeImage(_SyncMergeImage, _bUpdateUI);
		}
        public void Save()
        {
        	m_PlayerScreenUI.Save();
        }
        public void ConfigureHighSpeedCamera()
        {
        	m_PlayerScreenUI.DisplayConfigureSpeedBox(true);
        }
        public long GetOutputBitmap(Graphics _canvas, Bitmap _sourceImage, long _iTimestamp, bool _bFlushDrawings, bool _bKeyframesOnly)
        {
        	return m_PlayerScreenUI.GetOutputBitmap(_canvas, _sourceImage, _iTimestamp, _bFlushDrawings, _bKeyframesOnly);
        }
        public Bitmap GetFlushedImage()
        {
        	return m_PlayerScreenUI.GetFlushedImage();
        }
        #endregion
    }
}
