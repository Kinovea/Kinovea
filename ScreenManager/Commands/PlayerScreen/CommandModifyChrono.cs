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

using Kinovea.ScreenManager.Languages;
using System;
using System.Reflection;
using System.Resources;
using System.Threading;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class CommandModifyChrono : IUndoableCommand
    {
        public string FriendlyName
        {
            get
            {
                string friendlyName = "";
                switch (m_ModifType)
                {
                    case ChronoModificationType.TimeStart:
                        friendlyName = ScreenManagerLang.mnuChronoStart;
                        break;
                    case ChronoModificationType.TimeStop:
                        friendlyName = ScreenManagerLang.mnuChronoStop;
                        break;
                    case ChronoModificationType.TimeHide:
                        friendlyName = ScreenManagerLang.mnuChronoHide;
                        break;
                    case ChronoModificationType.Countdown:
                        friendlyName = ScreenManagerLang.mnuChronoCountdown;
                        break;
                    default:
                        break;
                }
                return friendlyName;
            }
        }

        #region Members
        private PlayerScreenUserInterface m_psui;
        private Metadata m_Metadata;
        private DrawingChrono m_Chrono;
        
        // New value
        private ChronoModificationType m_ModifType;
        private long m_iNewValue;

        // Memo
        private long m_iStartCountingTimestamp;
        private long m_iStopCountingTimestamp;          
        private long m_iInvisibleTimestamp;
        private bool m_bCountdown;				
		#endregion
        
        #region constructor
        public CommandModifyChrono(PlayerScreenUserInterface _psui, Metadata _Metadata, ChronoModificationType _modifType, long _newValue)
        {
        	// In the special case of Countdown toggle, the new value will be 0 -> false, true otherwise .
            m_psui = _psui;
            m_Metadata = _Metadata;
            m_Chrono = m_Metadata.ExtraDrawings[m_Metadata.SelectedExtraDrawing] as DrawingChrono;
            m_iNewValue = _newValue;
            m_ModifType = _modifType;

            // Save old values
            if(m_Chrono != null)
            {
            	m_iStartCountingTimestamp = m_Chrono.TimeStart;
	            m_iStopCountingTimestamp = m_Chrono.TimeStop;
	            m_iInvisibleTimestamp = m_Chrono.TimeInvisible;
	            m_bCountdown = m_Chrono.CountDown;
            }
        }
        #endregion

        /// <summary>
        /// Execution de la commande
        /// </summary>
        public void Execute()
        {
        	if(m_Chrono != null)
        	{
        		switch (m_ModifType)
	            {
	                case ChronoModificationType.TimeStart:
	                    m_Chrono.Start(m_iNewValue);
	                    break;
	                case ChronoModificationType.TimeStop:
	                    m_Chrono.Stop(m_iNewValue);
	                    break;
	                case ChronoModificationType.TimeHide:
	                    m_Chrono.Hide(m_iNewValue);
	                    break;
	                case ChronoModificationType.Countdown:
	                    m_Chrono.CountDown = (m_iNewValue != 0);
	                    break;
	                default:
	                    break;
	            }
        	}
            
            m_psui.pbSurfaceScreen.Invalidate();
        }
        public void Unexecute()
        {
            // The 'execute' action might have forced a modification on other values. (e.g. stop before start)
            // We must reinject all the old values.
            if(m_Chrono != null)
            {
	            m_Chrono.Start(m_iStartCountingTimestamp);
	            m_Chrono.Stop(m_iStopCountingTimestamp);
	            m_Chrono.Hide(m_iInvisibleTimestamp);
	            m_Chrono.CountDown = m_bCountdown;
            }
            
            m_psui.pbSurfaceScreen.Invalidate();
        }
    }
}


