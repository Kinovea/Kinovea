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
using System.Reflection;
using System.Resources;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    public class CaptureScreen : AbstractScreen
    {
        #region Properties
        public override Guid UniqueId
        {
            get { return m_UniqueId; }
            set { m_UniqueId = value;}
        }
        public override bool Full
        {
        	get { return false; }	
        }
        public override UserControl UI
        {
        	get { return m_CaptureScreenUI; }	
        }
		public override string FilePath
		{
			get { return ""; }
		}
		public override bool CapabilityDrawings
		{
			get { return true;}
		}
		public bool ShowGrid
        {
            get { return m_FrameServer.Metadata.Grid.Visible; }
            set { m_FrameServer.Metadata.Grid.Visible = value;}
        }
        public bool Show3DPlane
        {
            get { return m_FrameServer.Metadata.Plane.Visible; }
            set { m_FrameServer.Metadata.Plane.Visible = value;}
        }
        #endregion

        #region Members
        public CaptureScreenUserInterface	m_CaptureScreenUI;
        
		private FrameServerCapture m_FrameServer = new FrameServerCapture();
		private ResourceManager m_ResourceManager = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
        private Guid m_UniqueId;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public CaptureScreen()
        {
        	log.Debug("Constructing a CaptureScreen.");
            m_UniqueId = System.Guid.NewGuid();
            
            //Gestion i18n
            m_ResourceManager = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
            
			// Create UI
            m_CaptureScreenUI = new CaptureScreenUserInterface(m_FrameServer);
            
            // UI Delegates. (Assigned and implemented here, called from UI)
            m_CaptureScreenUI.m_CloseMeUI += new CaptureScreenUserInterface.DelegateCloseMeUI(ScreenUI_CloseAsked);
            m_CaptureScreenUI.m_SetMeAsActiveScreenUI += new CaptureScreenUserInterface.DelegateSetMeAsActiveScreenUI(ScreenUI_SetAsActiveScreen);
        }
        #endregion

        #region AbstractScreen Implementation
        public override void DisplayAsInactiveScreen()
        {
            m_CaptureScreenUI.DisplayAsActiveScreen(false);
        }
        public override void DisplayAsActiveScreen()
        {
        	m_CaptureScreenUI.DisplayAsActiveScreen(true);
        }
        public override void refreshUICulture() 
        {
        	m_CaptureScreenUI.RefreshUICulture();
        }
        public override void BeforeClose()
        {
        	m_CaptureScreenUI.UnloadMovie();
        }
        public override bool OnKeyPress(Keys _key)
        {
        	return m_CaptureScreenUI.OnKeyPress(_key);
        }
		public override void RefreshImage()
		{
			// Not implemented.
		}
        #endregion

        #region Délégués appelées depuis l'UI
        private void ScreenUI_CloseAsked()
        {
        	// Note: CloseMe variable is:
        	// - defined in AbstractScreen 
        	// - assigned in CommandAddCaptureScreen.
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
        #endregion
    
        public void Activate()
        {
        	m_CaptureScreenUI.PostCreation();
        }
    
    }
}
