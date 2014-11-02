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
    /// Responsible for getting the stream of images and forwarding commands to the camera.
    /// </summary>
    public interface IFrameGrabber : IFrameProducer
    {
        //event EventHandler<CameraImageReceivedEventArgs> CameraImageReceived;

        event EventHandler GrabbingStatusChanged;
        
        bool Grabbing { get; }
        Size Size { get;}
        int Depth { get; }
        float Framerate { get; }
        
        void Start();
        void Stop();
    }
}
