#region License
/*
Copyright © Joan Charmant 2011.
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
using System.Collections.ObjectModel;
using System.Drawing;

using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// An autonomous video effect. 
    /// It is self contained and user interactions are tunelled to it.
    /// 
    /// Usage (see VideoFilterMosaic):
    /// Create an instance of this class and assign the properties with delegates or lambdas.
    /// Pass the instance to SetInteractiveEffect() inside the Activate() method of the wrapper effect.
    /// The object will live in the PlayerScreen. 
    /// If the methods need to share common data, consider defining the delegates 
    /// as closures that would each capture the shared data from outer scope.
    /// </summary>
    public class InteractiveEffect
    {
        public Action<Graphics, IWorkingZoneFramesContainer> Draw {get;set;}
        public Action<int> MouseWheel {get;set;}
    }
}
