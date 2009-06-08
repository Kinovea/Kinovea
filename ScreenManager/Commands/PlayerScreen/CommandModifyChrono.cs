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
using System.Threading;
using Videa.Services;

namespace Videa.ScreenManager
{
    public class CommandModifyChrono : IUndoableCommand
    {
        public string FriendlyName
        {
            get
            {
                ResourceManager rm = new ResourceManager("Videa.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());

                string friendlyName = "";
                switch (m_ModifType)
                {
                    case DrawingChrono.ChronoModificationType.TimeStart:
                        friendlyName = rm.GetString("mnuChronoStart", Thread.CurrentThread.CurrentUICulture);
                        break;
                    case DrawingChrono.ChronoModificationType.TimeStop:
                        friendlyName = rm.GetString("mnuChronoStop", Thread.CurrentThread.CurrentUICulture);
                        break;
                    case DrawingChrono.ChronoModificationType.TimeHide:
                        friendlyName = rm.GetString("mnuChronoHide", Thread.CurrentThread.CurrentUICulture);
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
        private int m_iChronoIndex;
        private DrawingChrono.ChronoModificationType m_ModifType;
        private long m_iStartCountingTimestamp;         
        private long m_iStopCountingTimestamp;          
        private long m_iInvisibleTimestamp;
        private bool m_bCountdown;
        private long m_iNewValue;				
		#endregion
        
        #region constructor
        public CommandModifyChrono(PlayerScreenUserInterface _psui, Metadata _Metadata, DrawingChrono.ChronoModificationType _modifType, long _newValue)
        {
        	// In the special case of Countdown toggle, the new value will be 0 -> false, true otherwise .
            m_psui = _psui;
            m_Metadata = _Metadata;
            m_iChronoIndex = m_Metadata.SelectedChrono;
            m_iNewValue = _newValue;
            m_ModifType = _modifType;

            // old values
            m_iStartCountingTimestamp = m_Metadata.Chronos[m_iChronoIndex].TimeStart;
            m_iStopCountingTimestamp = m_Metadata.Chronos[m_iChronoIndex].TimeStop;
            m_iInvisibleTimestamp = m_Metadata.Chronos[m_iChronoIndex].TimeInvisible;
            m_bCountdown = m_Metadata.Chronos[m_iChronoIndex].CountDown;
        }
        #endregion

        /// <summary>
        /// Execution de la commande
        /// </summary>
        public void Execute()
        {
            switch (m_ModifType)
            {
                case DrawingChrono.ChronoModificationType.TimeStart:
                    m_Metadata.Chronos[m_iChronoIndex].Start(m_iNewValue);
                    break;
                case DrawingChrono.ChronoModificationType.TimeStop:
                    m_Metadata.Chronos[m_iChronoIndex].Stop(m_iNewValue);
                    break;
                case DrawingChrono.ChronoModificationType.TimeHide:
                    m_Metadata.Chronos[m_iChronoIndex].Hide(m_iNewValue);
                    break;
                case DrawingChrono.ChronoModificationType.Countdown:
                    if(m_iNewValue == 0)
                    	m_Metadata.Chronos[m_iChronoIndex].CountDown = false;
                    else
                    	m_Metadata.Chronos[m_iChronoIndex].CountDown = true;
                    break;
                default:
                    break;
            }

            m_psui._surfaceScreen.Invalidate();
        }
        public void Unexecute()
        {
            // The 'execute' action might have forced a modification on other values. (e.g. stop before start)
            // We must reinject all the old values.
            m_Metadata.Chronos[m_iChronoIndex].Start(m_iStartCountingTimestamp);
            m_Metadata.Chronos[m_iChronoIndex].Stop(m_iStopCountingTimestamp);
            m_Metadata.Chronos[m_iChronoIndex].Hide(m_iInvisibleTimestamp);
            m_Metadata.Chronos[m_iChronoIndex].CountDown = m_bCountdown;
            
            m_psui._surfaceScreen.Invalidate();
        }
    }
}


