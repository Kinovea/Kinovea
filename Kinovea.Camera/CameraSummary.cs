#region License
/*
Copyright © Joan Charmant 2012.
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
using System.Drawing;
using Kinovea.Video;

namespace Kinovea.Camera
{
    /// <summary>
    /// Small info to display the camera in the UI.
    /// </summary>
    public class CameraSummary
    {
        public string Alias { get; private set;}
        public string Name { get; private set;}
        public string Identifier { get; private set;}
        public Bitmap Icon { get; private set; }
        public Rectangle DisplayRectangle { get; set; }
        public CaptureAspectRatio AspectRatio { get; private set; }
        public ImageRotation Rotation { get; private set; }
        public object Specific { get; private set;}
        public CameraManager Manager { get; private set;}
        
        public CameraSummary(string alias, string name, string identifier, Bitmap icon, Rectangle displayRectangle, CaptureAspectRatio aspectRatio, ImageRotation rotation, object specific, CameraManager manager)
        {
            this.Alias = alias;
            this.Name = name;
            this.Identifier = identifier;
            this.Icon = icon;
            this.DisplayRectangle = displayRectangle;
            this.AspectRatio = aspectRatio;
            this.Rotation = rotation;
            this.Specific = specific;
            this.Manager = manager;
        }
        
        public void UpdateAlias(string alias, Bitmap icon)
        {
            this.Alias = alias;
            this.Icon = icon;
        }
        
        public void UpdateDisplayRectangle(Rectangle imageLocation)
        {
            this.DisplayRectangle = imageLocation;
        }
        
        public void UpdateAspectRatio(CaptureAspectRatio aspectRatio)
        {
            if(aspectRatio == this.AspectRatio)
                return;
                
            this.AspectRatio = aspectRatio;
        }

        public void UpdateRotation(ImageRotation rotation)
        {
            this.Rotation = rotation;
        }
        
        public void UpdateSpecific(object specific)
        {
            this.Specific = specific;
        }
        
        
    }
}
