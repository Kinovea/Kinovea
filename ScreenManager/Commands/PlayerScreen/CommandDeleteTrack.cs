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
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class CommandDeleteTrack : IUndoableCommand
    {

        public string FriendlyName
        {
            get { return ScreenManagerLang.mnuDeleteTrajectory; }
        }

        private PlayerScreenUserInterface view;
        private Metadata metadata;
        private DrawingTrack track;
        
        public CommandDeleteTrack(PlayerScreenUserInterface view, Metadata metadata)
        {
            this.view = view;
            this.metadata = metadata;
            this.track = metadata.ExtraDrawings[metadata.SelectedExtraDrawing] as DrawingTrack;
        }

        public void Execute()
        {
            if (track == null)
                return;
            
            metadata.ExtraDrawings.Remove(track);
            view.DoInvalidate();
        }

        public void Unexecute()
        {
            if (track == null)
                return;
            
            metadata.ExtraDrawings.Add(track);
            view.DoInvalidate();
        }
    }
}


