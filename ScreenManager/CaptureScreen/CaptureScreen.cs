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
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows.Forms;

namespace Videa.ScreenManager
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
        #endregion

        #region Members
		public CaptureScreenUserInterface	m_CaptureScreenUI;
		public ResourceManager             m_ResourceManager;
        private Guid m_UniqueId;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public CaptureScreen()
        {
        	log.Debug("Constructing a CaptureScreen.");
            m_UniqueId = System.Guid.NewGuid();
            
            //Gestion i18n
            m_ResourceManager = new ResourceManager("Videa.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
            
			// Create UI
            m_CaptureScreenUI = new CaptureScreenUserInterface(m_ResourceManager);
            
            // UI Delegates. (Assigned and implemented here, called from UI)
            m_CaptureScreenUI.m_CloseMeUI += new CaptureScreenUserInterface.DelegateCloseMeUI(ScreenUI_CloseAsked);
            m_CaptureScreenUI.m_SetMeAsActiveScreenUI += new CaptureScreenUserInterface.DelegateSetMeAsActiveScreenUI(ScreenUI_SetAsActiveScreen);
        }
        #endregion

        #region AbstractScreen Implementation
        public override void DisplayAsInactiveScreen()
        {
            // Not Implemented.
        }
        public override void DisplayAsActiveScreen()
        {
            // Not Implemented.
        }
        public override void refreshUICulture() 
        {
            // Not Implemented.
        }
        public override void CloseScreen()
        {
            // Not Implemented.
        }
        public override bool OnKeyPress(Keys _key)
        {
            bool bWasHandled = false;
            return bWasHandled;
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
