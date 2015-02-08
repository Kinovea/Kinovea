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
using Kinovea.Pipeline;

namespace Kinovea.Camera
{
    /// <summary>
    /// Interface for classes connecting the actual camera. 
    /// </summary>
    public interface ICaptureSource : IFrameProducer
    {
        event EventHandler GrabbingStatusChanged;
        
        bool Grabbing { get; }
        Size Size { get; }
        float Framerate { get; }
        double LiveDataRate { get; }

        ImageDescriptor Prepare();
        ImageDescriptor GetPrepareFailedImageDescriptor(ImageDescriptor input);
        void Start();
        void Stop();
    }
}
