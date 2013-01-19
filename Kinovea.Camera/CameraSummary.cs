#region License
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
#endregion
using System;
using System.Drawing;

namespace Kinovea.Camera
{
    /// <summary>
    /// Small info to display the camera in the UI.
    /// </summary>
    public class CameraSummary
    {
        public string Alias { get; private set;}
        public string Identifier { get; private set;}
        public CameraManager Manager { get; private set;}
        
        public CameraSummary(string alias, string identifier, CameraManager manager)
        {
            this.Alias = alias;
            this.Identifier = identifier;
            this.Manager = manager;
        }
    }
}
