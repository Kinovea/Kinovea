#region License
/*
Copyright © Joan Charmant 2014.
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
using System.Timers;
using System.Runtime.InteropServices;

namespace Kinovea.Camera.FrameGenerator
{
    public class FrameGrabber : IFrameGrabber
    {
        #region Imports Win32
        [DllImport("winmm.dll", SetLastError = true)]
        private static extern uint timeSetEvent(int msDelay, int msResolution, TimerEventHandler handler, ref int userCtx, int eventType);

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern uint timeKillEvent(uint timerEventId);

        private const int TIME_PERIODIC = 0x01;
        private const int TIME_KILL_SYNCHRONOUS = 0x0100;

        #endregion

        public event EventHandler<CameraImageReceivedEventArgs> CameraImageReceived;
        public event EventHandler GrabbingStatusChanged;

        #region Property
        public bool Grabbing
        {
            get { return grabbing; }
        }

        public Size Size
        {
            get { return info.SelectedFrameSize; }
        }

        public float Framerate
        {
            get { return 25f; }
        }
        #endregion

        #region Members
        private delegate void TimerEventHandler(uint id, uint msg, ref int userCtx, int rsv1, int rsv2);
        private TimerEventHandler timerEventHandler;
        private uint timerId;
        
        private CameraSummary summary;
        private SpecificInfo info;
        private object locker = new object();
        private bool grabbing;
        private Generator generator = new Generator();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public FrameGrabber(CameraSummary summary)
        {
            this.summary = summary;
            this.info = summary.Specific as SpecificInfo;
            timerEventHandler = new TimerEventHandler(MultimediaTimer_Tick);
        }

        public void Start()
        {
            log.DebugFormat("Starting device {0}, {1}", summary.Alias, summary.Identifier);
            grabbing = true;
            this.info = summary.Specific as SpecificInfo;

            int framerate = info.SelectedFrameRate;
            if (framerate == 0)
                framerate = 25;

            int interval = 1000 / framerate;
            StartMultimediaTimer(interval);

            if (GrabbingStatusChanged != null)
                GrabbingStatusChanged(this, EventArgs.Empty);
        }

        public void Stop()
        {
            log.DebugFormat("Stopping device {0}", summary.Alias);
            StopMultimediaTimer();

            grabbing = false;
            
            if (GrabbingStatusChanged != null)
                GrabbingStatusChanged(this, EventArgs.Empty);
        }

        private void StartMultimediaTimer(int interval)
        {
            int userCtx = 0;
            timerId = timeSetEvent(interval, interval, timerEventHandler, ref userCtx, TIME_PERIODIC | TIME_KILL_SYNCHRONOUS);
        }

        private void StopMultimediaTimer()
        {
            if (timerId != 0)
                timeKillEvent(timerId);
            
            timerId = 0;
        }

        private void MultimediaTimer_Tick(uint id, uint msg, ref int userCtx, int rsv1, int rsv2)
        {
            Size size = info.SelectedFrameSize;
            Bitmap image = generator.Generate(size);

            if (CameraImageReceived != null)
                CameraImageReceived(this, new CameraImageReceivedEventArgs(summary, image));
        }
    }
}
