#region License
/*
Copyright © Joan Charmant 2009.
jcharmant@gmail.com 
 
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
using System;
using System.ComponentModel;
using System.Drawing;
using System.Resources;

using Kinovea.ScreenManager.Languages;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    public class VideoFilterReverse : AbstractVideoFilter
    {
        public override string Name {
            get { return ScreenManagerLang.VideoFilterReverse_FriendlyName; }
        }
        public override Bitmap Icon {
            get { return Properties.Resources.revert; }
        }
        public override void Activate(IWorkingZoneFramesContainer framesContainer, Action<InteractiveEffect> setInteractiveEffect)
        {
            // Should be quick so we don't go through the background thread.
            if(framesContainer != null)
                framesContainer.Revert();
        }
        protected override void Process(object sender, DoWorkEventArgs e)
        {
        }
    }
}
