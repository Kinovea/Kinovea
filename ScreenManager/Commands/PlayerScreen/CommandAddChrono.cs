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
    public class CommandAddChrono : IUndoableCommand
    {

        public string FriendlyName
        {
            get { return ScreenManagerLang.CommandAddChrono_FriendlyName; }
        }
        
        private DelegateScreenInvalidate m_DoInvalidate;
        private DelegateDrawingUndrawn m_DoUndrawn;
        private Metadata m_Metadata;
        private DrawingChrono m_Chrono;
        private int m_iTotalChronos;

        #region constructor
        public CommandAddChrono(DelegateScreenInvalidate _invalidate, DelegateDrawingUndrawn _undrawn, Metadata _Metadata)
        {
            m_DoInvalidate = _invalidate;
        	m_DoUndrawn = _undrawn;
            m_Metadata = _Metadata;
            //m_iTotalChronos = m_Metadata.Chronos.Count;
            
            // Chrono (as all Drawings) are added to the list in reverse order.
            //m_Chrono = m_Metadata.Chronos[0];
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

            /*if (m_Metadata.Chronos.Count == m_iTotalChronos)
            {
                // first exec.
                // Nothing to do.
            }
            else if (m_Metadata.Chronos.Count == m_iTotalChronos - 1)
            {
                //Redo.
                //m_Metadata.Chronos.Insert(0, m_Chrono);
                m_Metadata.AddChrono(m_Chrono);
                m_DoInvalidate();
            }*/
        }
        public void Unexecute()
        {
            // Delete the last added chrono.
            
            // 1. Look for the keyframe
            /*if (m_Metadata.Chronos.Count > 0)
            {
                m_Metadata.Chronos.RemoveAt(0);
                m_DoUndrawn();
                m_DoInvalidate();
            }*/
        }
    }
}

