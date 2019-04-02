using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Video;

namespace Kinovea.Camera.HTTP
{
    public class CameraHTTPClientJPEG : ICameraHTTPClient
    {
        public event NewFrameBufferEventHandler NewFrameBuffer;
        public event NewFrameEventHandler NewFrame;
        public event VideoSourceErrorEventHandler VideoSourceError;

        public bool IsRunning
        {
            get { return client.IsRunning; }
            set { throw new NotImplementedException(); }
        }

        private JPEGStream client;

        public CameraHTTPClientJPEG(string url, string login, string password)
        {
            client = new JPEGStream(url);
            if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password))
            {
                client.Login = login;
                client.Password = password;
            }

            client.NewFrameBuffer += client_NewFrameBuffer;
            client.NewFrame += client_NewFrame;
            client.VideoSourceError += client_VideoSourceError;
        }

        private void client_VideoSourceError(object sender, VideoSourceErrorEventArgs eventArgs)
        {
            if (VideoSourceError != null)
                VideoSourceError(sender, eventArgs);
        }

        private void client_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (NewFrame != null)
                NewFrame(sender, eventArgs);
        }

        private void client_NewFrameBuffer(object sender, NewFrameBufferEventArgs eventArgs)
        {
            if (NewFrameBuffer != null)
                NewFrameBuffer(sender, eventArgs);
        }

        public void Start()
        {
            client.Start();
        }

        public void Stop()
        {
            client.Stop();
        }

        public void SignalToStop()
        {
            client.SignalToStop();
        }
    }
}
