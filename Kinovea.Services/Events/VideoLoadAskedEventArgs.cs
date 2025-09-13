#region License
/*
Copyright © Joan Charmant 2013.
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

namespace Kinovea.Services
{
    /// <summary>
    /// Event raised when the user wants to load a video in a screen.
    /// </summary>
    public class VideoLoadAskedEventArgs : EventArgs
    {
        public readonly string Path;
        public readonly int Target;
        public VideoLoadAskedEventArgs(string path, int target)
        {
            this.Path = path;
            this.Target = target;
        }
    }
}