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
using System.Drawing;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    public class CaptureScreen : AbstractScreen, ICaptureScreenUIHandler
    {
        #region Properties
        public override Guid UniqueId
        {
            get { return uid; }
            set { uid = value;}
        }
        public override bool Full
        {
        	get { return frameServer.IsConnected; }	
        }
        public override string FileName
		{
			get 
			{ 
				if(frameServer.IsConnected)
				{
					return frameServer.DeviceName;		
				}
				else
				{
					return ScreenManagerLang.statusEmptyScreen;	
				}
			}
		}
		public override string Status
		{
			get	{ return frameServer.Status;}
		}
        public override UserControl UI
        {
        	get { return view; }	
        }
		public override string FilePath
		{
			get { return ""; }
		}
		public override bool CapabilityDrawings
		{
			get { return true;}
		}
		public override ImageAspectRatio AspectRatio
		{
			get { return frameServer.AspectRatio; }
			set { frameServer.AspectRatio = value; }
		}
		public FrameServerCapture FrameServer
		{
			get { return frameServer; }
			set { frameServer = value; }
		}  
        public bool Shared
        {
        	set 
        	{
        		frameServer.Shared = value;
        		frameServer.UpdateMemoryCapacity();
        	}
        }
        public static readonly int HeartBeat = 1000;
        #endregion

        #region Members
        private IScreenHandler m_ScreenHandler; // ScreenManager seen through a limited interface.
        
        private CaptureScreenUserInterface	view;
		private FrameServerCapture frameServer = new FrameServerCapture();
		private Guid uid = System.Guid.NewGuid();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public CaptureScreen(IScreenHandler _screenHandler)
        {
        	log.Debug("Constructing a CaptureScreen.");
            m_ScreenHandler = _screenHandler;
            view = new CaptureScreenUserInterface(frameServer, this);
         
            BindCommands();
        }
        #endregion

        private void BindCommands()
        {
            // Provides implementation for behaviors triggered from the view, either as commands or as event handlers.
            view.DrawingAdded += (s, e) => frameServer.Metadata.AddDrawing(e.Drawing, e.KeyframeIndex);
        }
        
        #region ICaptureScreenUIHandler implementation
        // These should simply be events propagated to the screen manager.
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
        public void CaptureScreenUI_FileSaved()
        {
        	m_ScreenHandler.Capture_FileSaved(this);
        }
        public void CaptureScreenUI_LoadVideo(string _filepath)
        {
        	m_ScreenHandler.Capture_LoadVideo(this, _filepath);
        }
        #endregion
        
        #region AbstractScreen Implementation
        public override void DisplayAsActiveScreen(bool _bActive)
        {
        	view.DisplayAsActiveScreen(_bActive);
        }
        public override void refreshUICulture() 
        {
        	view.RefreshUICulture();
        }
        public override void BeforeClose()
        {
        	frameServer.BeforeClose();
        	view.BeforeClose();
        }
        public override void AfterClose()
        {
            // Fixme: all the stopping and cleaning is implemented in BeforeClose instead of AfterClose. 
            // It works while there is no cancellation possible.
        }
        public override bool OnKeyPress(Keys _key)
        {
        	return view.OnKeyPress(_key);
        }
		public override void RefreshImage()
		{
			// Not implemented.
		}
		public override void AddImageDrawing(string _filename, bool _bIsSvg)
        {
			view.AddImageDrawing(_filename, _bIsSvg);
        }
		public override void AddImageDrawing(Bitmap _bmp)
        {
        	view.AddImageDrawing(_bmp);
        }
		public override void FullScreen(bool _bFullScreen)
        {
            view.FullScreen(_bFullScreen);
        }
        #endregion
    }
}
