using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Video;

namespace Kinovea.Camera.HTTP
{
    public interface ICameraHTTPClient
    {
        // At the moment these events are still dependent on AForge for convenience.
        // Ideally these would be redeclared locally and each CameraHTTPClient would be a proxy 
        // from events declared in the underlying library.
        event NewFrameBufferEventHandler NewFrameBuffer;
        event NewFrameEventHandler NewFrame;
        event VideoSourceErrorEventHandler VideoSourceError;

        bool IsRunning { get; set; }

        void Start();
        void Stop();
        void SignalToStop();
    }
}
