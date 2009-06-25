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
using System.Threading;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class CommandDeleteChrono : IUndoableCommand
    {

        public string FriendlyName
        {
            get
            {
                ResourceManager rm = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
                return rm.GetString("mnuChronoDelete", Thread.CurrentThread.CurrentUICulture);
            }
        }

        private PlayerScreenUserInterface m_psui;
        private Metadata m_Metadata;
        private int m_iTotalChronos;
        private DrawingChrono m_Chrono;
        private int m_iChronoIndex;

        #region constructor
        public CommandDeleteChrono(PlayerScreenUserInterface _psui, Metadata _Metadata)
        {
            m_psui = _psui;
            m_Metadata = _Metadata;
            m_iChronoIndex = m_Metadata.SelectedChrono;
            m_iTotalChronos = m_Metadata.Chronos.Count;
            m_Chrono = m_Metadata.Chronos[m_iChronoIndex];
        }
        #endregion

        /// <summary>
        /// Execution de la commande
        /// </summary>
        public void Execute()
        {
            // It should work because all add/delete actions modify the undo stack.
            // When we come back here for a redo, we should be in the exact same state
            // as the first time.
            // Even if drawings were added in between, we can't come back here
            // before all those new drawings have been unstacked from the m_CommandStack stack.

            m_Metadata.Chronos.RemoveAt(m_iChronoIndex);
            m_Metadata.SelectedChrono = -1;
            m_psui._surfaceScreen.Invalidate();
        }
        public void Unexecute()
        {
            // Recreate the drawing.

            // 1. Look for the keyframe
            // We must insert exactly where we deleted, otherwise the drawing table gets messed up.
            // We must still be able to undo any Add action that where performed before.
            m_Metadata.Chronos.Insert(m_iChronoIndex, m_Chrono);
            m_psui._surfaceScreen.Invalidate();
        }
    }
}


