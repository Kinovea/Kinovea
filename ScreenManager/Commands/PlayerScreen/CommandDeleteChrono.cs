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
    public class CommandDeleteChrono : IUndoableCommand
    {
        public string FriendlyName
        {
            get { return ScreenManagerLang.mnuChronoDelete; }
        }

        private PlayerScreenUserInterface m_psui;
        private Metadata m_Metadata;
        private DrawingChrono m_Chrono;

        #region constructor
        public CommandDeleteChrono(PlayerScreenUserInterface _psui, Metadata _Metadata)
        {
            m_psui = _psui;
            m_Metadata = _Metadata;
            m_Chrono = m_Metadata.ExtraDrawings[m_Metadata.SelectedExtraDrawing] as DrawingChrono;
        }
        #endregion

        public void Execute()
        {
            if(m_Chrono != null)
            {
                m_Metadata.ExtraDrawings.Remove(m_Chrono);
                m_psui.pbSurfaceScreen.Invalidate();
            }
        }
        public void Unexecute()
        {
            // Recreate the drawing.
            if(m_Chrono != null)
            {
                m_Metadata.ExtraDrawings.Add(m_Chrono);
                m_psui.pbSurfaceScreen.Invalidate();
            }
        }
    }
}


