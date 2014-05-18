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

using System.Collections.Generic;
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class CommandDeleteEndOfTrack : IUndoableCommand
    {
        public string FriendlyName
        {
            get { return ScreenManagerLang.mnuDeleteEndOfTrajectory; }
        }

        private PlayerScreenUserInterface view;
        private Metadata metadata;
        private DrawingTrack track;
        private long framePosition;
        public List<AbstractTrackPoint> points;

        public CommandDeleteEndOfTrack(PlayerScreenUserInterface view, Metadata metadata, long framePosition)
        {
            this.view = view;
            this.metadata = metadata;
            //this.track = metadata.ExtraDrawings[metadata.SelectedExtraDrawing] as DrawingTrack;
            this.framePosition = framePosition;
        }

        public void Execute()
        {
            // We store the old end-of-track values only here (and not in the ctor) 
            // because some points may be moved between the undo and 
            // the redo and we'll want to keep teir values.
            if (track == null)
                return;
            
            points = track.GetEndOfTrack(framePosition);
            track.ChopTrajectory(framePosition);
            view.DoInvalidate();
        }
        public void Unexecute()
        {
            if(points != null && track != null)
                track.AppendPoints(framePosition, points);

            view.DoInvalidate();
        }
    }
}