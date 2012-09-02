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
            get { return ScreenManagerLang.mnuDeleteEndOfTrajectory; }
        }

        private PlayerScreenUserInterface m_psui;
        private Metadata m_Metadata;
        private DrawingTrack m_Track;
        private long m_iTimeStamp;
        public List<AbstractTrackPoint> m_Positions;

        #region constructor
        public CommandDeleteEndOfTrack(PlayerScreenUserInterface _psui, Metadata _Metadata, long _iTimeStamp)
        {
            m_psui = _psui;
            m_Metadata = _Metadata;
            m_Track = m_Metadata.ExtraDrawings[m_Metadata.SelectedExtraDrawing] as DrawingTrack;
            m_iTimeStamp = _iTimeStamp;
        }
        #endregion

       public void Execute()
        {
            // We store the old end-of-track values only here (and not in the ctor) 
            // because some points may be moved between the undo and 
            // the redo and we'll want to keep teir values.
            if(m_Track != null)
            {
            	m_Positions = m_Track.GetEndOfTrack(m_iTimeStamp);
	            m_Track.ChopTrajectory(m_iTimeStamp);
	            m_psui.pbSurfaceScreen.Invalidate();
            }
        }
        public void Unexecute()
        {
            // Revival of the discarded points.
            if(m_Positions != null && m_Track != null)
            {
            	m_Track.AppendPoints(m_iTimeStamp, m_Positions);
            }
            m_psui.pbSurfaceScreen.Invalidate();
        }
    }
}