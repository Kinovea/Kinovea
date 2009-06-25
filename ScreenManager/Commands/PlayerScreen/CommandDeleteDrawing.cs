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
    public class CommandDeleteDrawing : IUndoableCommand
    {

        public string FriendlyName
        {
            get
            {
                ResourceManager rm = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
                return rm.GetString("CommandDeleteDrawing_FriendlyName", Thread.CurrentThread.CurrentUICulture) + " (" + m_Drawing.ToString() + ")";
            }
        }

        private PlayerScreenUserInterface m_psui;
        private long m_iFramePosition;
        private Metadata m_Metadata;
        private AbstractDrawing m_Drawing;
        private int m_iDrawingIndex;

        #region constructor
        public CommandDeleteDrawing(PlayerScreenUserInterface _psui, Metadata _Metadata, long _iFramePosition, int _iDrawingIndex)
        {
            m_psui = _psui;
            m_iFramePosition = _iFramePosition;
            m_Metadata = _Metadata;
            m_iDrawingIndex = _iDrawingIndex;

            int iIndex = GetKeyframeIndex();
            if (iIndex >= 0)
            {
                m_Drawing = m_Metadata[iIndex].Drawings[m_iDrawingIndex];
            }
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

            int iIndex = GetKeyframeIndex();
            if (iIndex >= 0)
            {
                m_Metadata[iIndex].Drawings.RemoveAt(m_iDrawingIndex);
                m_Metadata.SelectedDrawing = -1;
                m_Metadata.SelectedDrawingFrame = -1;
                m_psui._surfaceScreen.Invalidate();
            }
        }
        public void Unexecute()
        {
            // Recreate the drawing.

            // 1. Look for the keyframe
            int iIndex = GetKeyframeIndex();
            if (iIndex >= 0)
            {
                // We must insert exactly where we deleted, otherwise the drawing table gets messed up.
                // We must still be able to undo any Add action that where performed before.
                m_Metadata[iIndex].Drawings.Insert(m_iDrawingIndex, m_Drawing);
                m_psui._surfaceScreen.Invalidate();
            }
            else
            {
                // Keyframe may have been deleted since we added the drawing.
                // All CommandDeleteDrawing for this frame are now orphans...
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


