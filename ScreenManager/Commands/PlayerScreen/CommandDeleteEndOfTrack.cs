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
using System.Threading;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class CommandDeleteEndOfTrack : IUndoableCommand
    {
        public string FriendlyName
        {
            get
            {
                ResourceManager rm = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
                return rm.GetString("mnuDeleteEndOfTrajectory", Thread.CurrentThread.CurrentUICulture);
            }
        }

        private PlayerScreenUserInterface m_psui;
        private Metadata m_Metadata;
        private int m_iTrackIndex;
        private long m_iTimeStamp;
        public List<TrackPosition> m_Positions;

        #region constructor
        public CommandDeleteEndOfTrack(PlayerScreenUserInterface _psui, Metadata _Metadata, long _iTimeStamp)
        {
            m_psui = _psui;
            m_Metadata = _Metadata;
            m_iTrackIndex = m_Metadata.SelectedTrack;
            m_iTimeStamp = _iTimeStamp;
        }
        #endregion

        /// <summary>
        /// Execution de la commande
        /// </summary>
        public void Execute()
        {
            // We store the old end-of-track values only here (and not in the ctor) 
            // because some points may be moved between the undo and 
            // the redo and we'll want to keep teir values.
            m_Positions = m_Metadata.Tracks[m_iTrackIndex].GetEndOfTrack(m_iTimeStamp);
            m_Metadata.Tracks[m_iTrackIndex].ChopTrajectory(m_iTimeStamp);
            m_psui._surfaceScreen.Invalidate();
        }
        public void Unexecute()
        {
            // Revival of the discarded points.
            if(m_Positions != null)
            {
            	m_Metadata.Tracks[m_iTrackIndex].AppendPoints(m_iTimeStamp, m_Positions);
            }
            m_psui._surfaceScreen.Invalidate();
        }
    }
}


