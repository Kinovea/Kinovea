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

namespace Kinovea.ScreenManager.Deprecated
{
    public class CommandAddKeyframe : IUndoableCommand
    {

        public string FriendlyName
        {
        	get { return ScreenManagerLang.ToolTip_AddKeyframe; }
        }

        private PlayerScreenUserInterface view;
        private long framePosition;
        private Metadata metadata;
        
        #region constructor
        public CommandAddKeyframe(PlayerScreenUserInterface view, Metadata metadata, long framePosition)
        {
            this.view = view;
            this.framePosition = framePosition;
            this.metadata = metadata;
        }
        #endregion

        public void Execute()
        {
            //view.OnAddKeyframe(framePosition);
        }

        public void Unexecute()
        {
            // The PlayerScreen used at execute time may not be valid anymore...
            // The MetaData used at execute time may not be valid anymore...
            // (ex: Add KF + Close screen + undo + undo)

            // Delete Keyframe at given position
            /*int index = metadata.GetKeyframeIndex(framePosition);
            if (index >= 0)
                view.OnRemoveKeyframe(index);*/
        }
    }
}