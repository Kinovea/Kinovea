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

namespace Kinovea.ScreenManager.Deprecated
{
    public class CommandAddDrawing : IUndoableCommand
    {

        public string FriendlyName
        {
            get
            {
            	return ScreenManagerLang.CommandAddDrawing_FriendlyName + " (" + drawing.DisplayName + ")";
            }
        }

        private Action doInvalidate;
        private Action doUndrawn;
        private long framePosition;
        private Metadata metadata;
        private int totalDrawings;
        private AbstractDrawing drawing;

        public CommandAddDrawing(Action invalidate, Action undrawn, Metadata metadata, long framePosition)
        {
        	this.doInvalidate = invalidate;
        	this.doUndrawn = undrawn;
        	
            this.framePosition = framePosition;
            this.metadata = metadata;

            int index = metadata.GetKeyframeIndex(framePosition);
            if (index >= 0 && metadata[index].Drawings.Count >= 0)
            {
                totalDrawings = metadata[index].Drawings.Count;
                drawing = metadata[index].Drawings[0];
            }
        }

        public void Execute()
        {
            // First execution : Work has already been done in the PlayerScreen (interactively).
            // Redo : We need to bring back the drawing to life.

            int index = metadata.GetKeyframeIndex(framePosition);
            if (index < 0 || metadata[index].Drawings.Count != totalDrawings - 1)
                return;
            
            metadata[index].Drawings.Insert(0, drawing);
            doInvalidate();
        }

        public void Unexecute()
        {
            // Delete the last drawing on Keyframe.
            int index = metadata.GetKeyframeIndex(framePosition);
            if (index < 0 || metadata[index].Drawings.Count <= 0)
                return;

            metadata[index].Drawings.RemoveAt(0);
            doUndrawn();
            doInvalidate();
        }
    }
}

