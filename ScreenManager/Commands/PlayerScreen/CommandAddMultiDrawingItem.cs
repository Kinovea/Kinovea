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

using System;
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class CommandAddMultiDrawingItem : IUndoableCommand
    {
        public string FriendlyName 
        {
            get { return ScreenManagerLang.CommandAddDrawing_FriendlyName + " (" + multiDrawing.DisplayName + ")"; }
        }

        private Action doInvalidate;
        private Action doUndrawn;
        private int totalDrawings;
        private AbstractMultiDrawing multiDrawing;
        private object drawingItem;

        public CommandAddMultiDrawingItem(Action invalidate, Action undrawn, Metadata metadata)
        {
        	this.doInvalidate = invalidate;
            this.doUndrawn = undrawn;
            this.multiDrawing = metadata.ExtraDrawings[metadata.SelectedExtraDrawing] as AbstractMultiDrawing;
            this.drawingItem = multiDrawing.SelectedItem;
            this.totalDrawings = multiDrawing.Count;
        }
        
        public void Execute()
        {
            // Only treat redo. We don't need to do anything on first execution.
            if (multiDrawing.Count == totalDrawings)
                return;
            
            multiDrawing.Add(drawingItem);
            doInvalidate();
        }
        public void Unexecute()
        {
            multiDrawing.Remove(drawingItem);
            doUndrawn();
            doInvalidate();
        }
    }
}


