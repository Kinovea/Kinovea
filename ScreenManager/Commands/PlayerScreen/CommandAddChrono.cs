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
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class CommandAddChrono : IUndoableCommand
    {

        public string FriendlyName
        {
            get
            {
                ResourceManager rm = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
                return rm.GetString("CommandAddChrono_FriendlyName", Thread.CurrentThread.CurrentUICulture);
            }
        }

        private PlayerScreenUserInterface m_psui;
        private Metadata m_Metadata;
        private DrawingChrono m_Chrono;
        private int m_iTotalChronos;

        #region constructor
        public CommandAddChrono(PlayerScreenUserInterface _psui, Metadata _Metadata)
        {
            // Chrono (as all Drawings) are added to the list in reverse order.
            m_psui = _psui;
            m_Metadata = _Metadata;
            m_iTotalChronos = m_Metadata.Chronos.Count;
            m_Chrono = m_Metadata.Chronos[0];
        }
        #endregion

        /// <summary>
        /// Execution de la commande
        /// </summary>
        public void Execute()
        {
            // We need to differenciate between two cases :
            // First execution : Work has already been done in the PlayerScreen (interactively).
            // Redo : We need to bring back the drawing to life.

            if (m_Metadata.Chronos.Count == m_iTotalChronos)
            {
                // first exec.
                // Nothing to do.
            }
            else if (m_Metadata.Chronos.Count == m_iTotalChronos - 1)
            {
                //Redo.
                m_Metadata.Chronos.Insert(0, m_Chrono);
                m_psui._surfaceScreen.Invalidate();
            }
        }
        public void Unexecute()
        {
            // Delete the last added chrono.
            
            // 1. Look for the keyframe
            if (m_Metadata.Chronos.Count > 0)
            {
                m_Metadata.Chronos.RemoveAt(0);
                m_psui.OnUndrawn();
                m_psui._surfaceScreen.Invalidate();
            }
        }
    }
}

