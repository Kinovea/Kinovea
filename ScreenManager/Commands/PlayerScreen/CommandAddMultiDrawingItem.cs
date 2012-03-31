/*
Copyright © Joan Charmant 2012.
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
    public class CommandAddMultiDrawingItem : IUndoableCommand
    {
        public string FriendlyName {
            get { return ScreenManagerLang.CommandAddDrawing_FriendlyName + " (" + m_MultiDrawing.ToString() + ")"; }
        }

        private Action m_DoInvalidate;
        private Action m_DoUndrawn;
        private int m_iTotalDrawings;
        private AbstractMultiDrawing m_MultiDrawing;
        private object m_DrawingItem;

        public CommandAddMultiDrawingItem(Action _invalidate, Action _undrawn, Metadata _Metadata)
        {
        	m_DoInvalidate = _invalidate;
        	m_DoUndrawn = _undrawn;
            m_MultiDrawing = _Metadata.ExtraDrawings[_Metadata.SelectedExtraDrawing] as AbstractMultiDrawing;
            m_DrawingItem = m_MultiDrawing.SelectedItem;
            m_iTotalDrawings = m_MultiDrawing.Count;
        }
        
        public void Execute()
        {
            // Only treat redo. We don't need to do anything on first execution.
            if(m_MultiDrawing.Count != m_iTotalDrawings)
            {
                m_MultiDrawing.Add(m_DrawingItem);
                m_DoInvalidate();
            }
        }
        public void Unexecute()
        {
            m_MultiDrawing.Remove(m_DrawingItem);
            m_DoUndrawn();
            m_DoInvalidate();
        }
    }
}


