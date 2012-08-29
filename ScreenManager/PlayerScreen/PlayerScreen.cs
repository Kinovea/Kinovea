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
using Kinovea.Video;

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
                return m_FrameServer.Loaded ? Path.GetFileName(m_FrameServer.VideoReader.FilePath) :
				                              ScreenManagerLang.statusEmptyScreen;
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
            get { return m_FrameServer.VideoReader.Options.ImageAspectRatio; }
            set
            {
                bool uncached = m_FrameServer.VideoReader.ChangeAspectRatio(value);
                
                if (uncached && m_FrameServer.VideoReader.DecodingMode == VideoDecodingMode.Caching)
                    m_PlayerScreenUI.UpdateWorkingZone(true);
                    
                m_PlayerScreenUI.AspectRatioChanged();
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
                    return m_FrameServer.VideoReader.IsSingleFrame;
            }	
        }
        public bool IsCaching
        {
            get
            {
                if (!m_FrameServer.Loaded)
                    return false;
                else
                    return m_FrameServer.VideoReader.DecodingMode == VideoDecodingMode.Caching;
            }
        }
        public long CurrentFrame
        {
            get
            {
                // Get the approximate frame we should be on.
                // Only as accurate as the framerate is stable regarding to the timebase.
                
                // SyncCurrentPosition timestamp is already relative to selection start).
                return (long)((double)m_PlayerScreenUI.SyncCurrentPosition / m_FrameServer.VideoReader.Info.AverageTimeStampsPerFrame);
            }
        }
        public long EstimatedFrames
        {
            get 
            {
                // Used to compute the total duration of the common track bar.
                return m_FrameServer.VideoReader.EstimatedFrames;
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
        public long SyncPosition
        {
            // Reference timestamp for synchronization, expressed in local timebase.
            get { return m_PlayerScreenUI.SyncPosition; }
            set { m_PlayerScreenUI.SyncPosition = value; }
        }
        public long Position
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
            get { return m_FrameServer.VideoReader.Options.Deinterlace; }
            set
            {
                bool uncached = m_FrameServer.VideoReader.ChangeDeinterlace(value);
                
                if (uncached && m_FrameServer.VideoReader.DecodingMode == VideoDecodingMode.Caching)
					m_PlayerScreenUI.UpdateWorkingZone(true);
                
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
        public bool InteractiveFiltering {
        	get {return m_PlayerScreenUI.InteractiveFiltering;}
        }
        #endregion

        #region members
        public PlayerScreenUserInterface m_PlayerScreenUI; // <-- FIXME: Rely on a IPlayerScreenUI or IPlayerScreenView rather than the concrete implementation.
		
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
            
            BindCommands();
        }
        #endregion

        private void BindCommands()
        {
            // Provides implementation for behaviors triggered from the view, either as commands or as event handlers.
            // Fixme: those using FrameServer.Metadata work only because the Metadata object is never replaced during the PlayerScreen life.
            
            // Event handlers
            m_PlayerScreenUI.TrackableDrawingAdded += (s, e) => RegisterTrackableDrawing(e.TrackableDrawing);
            m_FrameServer.Metadata.TrackableDrawingAdded += (s, e) => RegisterTrackableDrawing(e.TrackableDrawing);
            m_PlayerScreenUI.TrackableDrawingDeleted += (s, e) => TrackableDrawingDeleted(e.TrackableDrawing);
            m_FrameServer.Metadata.TrackableDrawingDeleted += (s, e) => TrackableDrawingDeleted(e.TrackableDrawing);
            
            // Commands
            m_PlayerScreenUI.ToggleTrackingCommand = new ToggleCommand(ToggleTracking, IsTracking);
            m_PlayerScreenUI.TrackDrawingsCommand = new RelayCommand<VideoFrame>(TrackDrawings);
            
        }
        
        
        #region IPlayerScreenUIHandler (and IScreenUIHandler) implementation
        
        // TODO: turn all these dependencies into commands.
        
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
            if(m_FrameServer.Loaded)
                m_PlayerScreenUI.StopPlaying();
        }
        public override void AfterClose()
        {
            if(m_FrameServer.Loaded)
            {
                m_FrameServer.VideoReader.Close();
                m_PlayerScreenUI.ResetToEmptyState();
            }
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
        public void GotoFrame(long _frame, bool _bAllowUIUpdate)
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
        public void SetInteractiveEffect(InteractiveEffect _effect)
        {
            m_PlayerScreenUI.SetInteractiveEffect(_effect);
        }
        public void DeactivateInteractiveEffect()
        {
            m_PlayerScreenUI.DeactivateInteractiveEffect();
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
   
        private void RegisterTrackableDrawing(ITrackable trackableDrawing)
		{
		    m_FrameServer.Metadata.TrackabilityManager.Add(trackableDrawing, m_FrameServer.VideoReader.Current);
		}
        
        private void TrackableDrawingDeleted(ITrackable trackableDrawing)
        {
           m_FrameServer.Metadata.TrackabilityManager.Remove(trackableDrawing);
        }
        private void ToggleTracking(object parameter)
        {
            ITrackable trackableDrawing = ConvertToTrackable(parameter);
            if(trackableDrawing == null)
                return;
            
            m_FrameServer.Metadata.TrackabilityManager.ToggleTracking(trackableDrawing);
        }
        private bool IsTracking(object parameter)
        {
            ITrackable trackableDrawing = ConvertToTrackable(parameter);
            if(trackableDrawing == null)
                return false;
            
            return m_FrameServer.Metadata.TrackabilityManager.IsTracking(trackableDrawing);
        }
        
        private ITrackable ConvertToTrackable(object parameter)
        {
            ITrackable trackableDrawing = null;
            
            if(parameter is AbstractMultiDrawing)
            {
                AbstractMultiDrawing manager = parameter as AbstractMultiDrawing;
                if(manager != null)
                    trackableDrawing = manager.SelectedItem as ITrackable;    
            }
            else
            {
                trackableDrawing = parameter as ITrackable;
            }
            
            return trackableDrawing;
        }
        private void TrackDrawings(VideoFrame frameToUse)
        {
            VideoFrame frame = frameToUse ?? m_FrameServer.VideoReader.Current;
            m_FrameServer.Metadata.TrackabilityManager.Track(frame);
        }
    }
}
