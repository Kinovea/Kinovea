#region license
/*
Copyright © Joan Charmant 20012.
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
#endregion

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager.Deprecated
{
    public class CommandDeleteMultiDrawingItem : IUndoableCommand
    {
        public string FriendlyName 
        {
            get { return ScreenManagerLang.mnuDeleteDrawing + " (" + multiDrawing.DisplayName + ")"; }
        }

        private PlayerScreenUserInterface view;
        private Metadata metadata;
        private AbstractMultiDrawing multiDrawing;
        private object drawingItem;
        
        public CommandDeleteMultiDrawingItem(PlayerScreenUserInterface view, Metadata metadata)
        {
            this.view = view;
            this.metadata = metadata;
            //this.multiDrawing = metadata.ExtraDrawings[metadata.SelectedExtraDrawing] as AbstractMultiDrawing;
            this.drawingItem = multiDrawing.SelectedItem;
        }

        public void Execute()
        {
            if (drawingItem == null)
                return;
            
            //multiDrawing.Remove(drawingItem);
            view.DoInvalidate();
        }
        public void Unexecute()
        {
            if (drawingItem == null)
                return;
            
            //multiDrawing.Add(drawingItem);
            view.DoInvalidate();
        }
    }
}


