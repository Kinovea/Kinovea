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

        public bool Mirror { get; private set; }

        public object Specific { get; private set;}
        public CameraManager Manager { get; private set;}

        /// <summary>
        /// Build a full camera summary.
        /// This is normally used when we have obtained all the info from an XML blurb of that camera.
        /// </summary>
        public CameraSummary(string alias, string name, string identifier, Bitmap icon, Rectangle displayRectangle, CaptureAspectRatio aspectRatio, ImageRotation rotation, bool mirror, object specific, CameraManager manager)
        {
            this.Alias = alias;
            this.Name = name;
            this.Identifier = identifier;
            this.Icon = icon;
            this.DisplayRectangle = displayRectangle;
            this.AspectRatio = aspectRatio;
            this.Rotation = rotation;
            this.Mirror = mirror;
            this.Specific = specific;
            this.Manager = manager;
        }

        /// <summary>
        /// Build a default camera summary.
        /// This is used for certain camera types.
        /// </summary>
        public CameraSummary(string alias, string name, string identifier, Bitmap icon, CameraManager manager)
        {
            this.Alias = alias;
            this.Name = name;
            this.Identifier = identifier;
            this.Icon = icon;
            this.DisplayRectangle = Rectangle.Empty;
            this.AspectRatio = CaptureAspectRatio.Auto;
            this.Rotation = ImageRotation.Rotate0;
            this.Mirror = false;
            this.Specific = null;
            this.Manager = manager;
        }

        /// <summary>
        /// Build an invalid camera summary containing just the camera alias.
        /// This is used to prepare a capture screen that will then listen to new cameras being plugged in and try to match them against the alias.
        /// </summary>
        public CameraSummary(string alias)
        {
            this.Alias = alias;
            this.Name = null;
            this.Identifier = null;
            this.Icon = null;
            this.DisplayRectangle = Rectangle.Empty;
            this.AspectRatio = CaptureAspectRatio.Auto;
            this.Rotation = ImageRotation.Rotate0;
            this.Mirror = false;
            this.Specific = null;
            this.Manager = null;
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
        
        public void UpdateMirror(bool mirror)
        {
            this.Mirror = mirror;
        }

        public void UpdateSpecific(object specific)
        {
            this.Specific = specific;
        }
        
        
    }
}
