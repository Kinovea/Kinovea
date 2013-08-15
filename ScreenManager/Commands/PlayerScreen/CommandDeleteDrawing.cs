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
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class CommandDeleteDrawing : IUndoableCommand
    {

        public string FriendlyName 
        {
            get { return ScreenManagerLang.CommandDeleteDrawing_FriendlyName + " (" + drawing.DisplayName + ")"; }
        }

        private Action doScreenInvalidate;
        private long framePosition;
        private Metadata metadata;
        private AbstractDrawing drawing;
        private int drawingIndex;

        public CommandDeleteDrawing(Action invalidate, Metadata metadata, long framePosition, int drawingIndex)
        {
            this.doScreenInvalidate = invalidate;
            this.framePosition = framePosition;
            this.metadata = metadata;
            this.drawingIndex = drawingIndex;
            
            int index = metadata.GetKeyframeIndex(framePosition);
            if (index >= 0)
                drawing = metadata[index].Drawings[drawingIndex];
        }
        
        public void Execute()
        {
            // It should work because all add/delete actions modify the undo stack.
            // When we come back here for a redo, we should be in the exact same state
            // as the first time.
            // Even if drawings were added in between, we can't come back here
            // before all those new drawings have been unstacked from the m_CommandStack stack.

            int index = metadata.GetKeyframeIndex(framePosition);
            if (index < 0)
                return;
            
            metadata.DeleteDrawing(index, drawingIndex);
            doScreenInvalidate();
        }

        public void Unexecute()
        {
            int index = metadata.GetKeyframeIndex(framePosition);
            if (index < 0)
                return;
            
            // We must insert exactly where we deleted, otherwise the drawing table gets messed up.
            // We must still be able to undo any Add action that where performed before.
            metadata.UndeleteDrawing(index, drawingIndex, drawing);
            doScreenInvalidate();
        }
    }
}


