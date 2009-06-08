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
    public class CommandAddDrawing : IUndoableCommand
    {

        public string FriendlyName
        {
            get
            {
                ResourceManager rm = new ResourceManager("Videa.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
                return rm.GetString("CommandAddDrawing_FriendlyName", Thread.CurrentThread.CurrentUICulture) + " (" + m_Drawing.ToString() + ")";
            }
        }

        private PlayerScreenUserInterface m_psui;
        private long m_iFramePosition;
        private Metadata m_Metadata;
        private int m_iTotalDrawings;
        private AbstractDrawing m_Drawing;

        #region constructor
        public CommandAddDrawing(PlayerScreenUserInterface _psui, Metadata _Metadata, long _iFramePosition)
        {
            m_psui = _psui;
            m_iFramePosition = _iFramePosition;
            m_Metadata = _Metadata;

            int iIndex = GetKeyframeIndex();
            if (iIndex >= 0)
            {
                m_iTotalDrawings = m_Metadata[iIndex].Drawings.Count;
                m_Drawing = m_Metadata[iIndex].Drawings[0];
            }
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

            int iIndex = GetKeyframeIndex();
            if (iIndex >= 0)
            {
                if (m_Metadata[iIndex].Drawings.Count == m_iTotalDrawings)
                {
                    // first exec.
                    // Nothing to do.
                }
                else if (m_Metadata[iIndex].Drawings.Count == m_iTotalDrawings - 1)
                {
                    //Redo.
                    m_Metadata[iIndex].Drawings.Insert(0, m_Drawing);
                    m_psui._surfaceScreen.Invalidate();
                }
            }
        }
        public void Unexecute()
        {
            // Delete the last drawing on Keyframe.
            
            // 1. Look for the keyframe
            int iIndex = GetKeyframeIndex();
            if (iIndex >= 0)
            {
                if (m_Metadata[iIndex].Drawings.Count > 0)
                {
                    m_Metadata[iIndex].Drawings.RemoveAt(0);
                    m_psui.OnUndrawn();
                    m_psui._surfaceScreen.Invalidate();
                }
            }
            else
            {
                // Keyframe may have been deleted since we added the drawing.
                // All CommandAddDrawing for this frame are now orphans...
            }
        }
        private int GetKeyframeIndex()
        {
            int iIndex = -1;
            for (int i = 0; i < m_Metadata.Count; i++)
            {
                if (m_Metadata[i].Position == m_iFramePosition)
                {
                    iIndex = i;
                }
            }

            return iIndex;
        }
    }
}

