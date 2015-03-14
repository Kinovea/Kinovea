using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Services
{
    public class CaptureHistoryEntry
    {
        public string CaptureFile { get; private set; }
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }
        public string CameraAlias { get; private set; }
        public string CameraIdentifier { get; private set; }
        public double ConfiguredFramerate { get; private set; }
        public double ReceivedFramerate { get; private set; }
        public int Drops { get; private set; }

        public CaptureHistoryEntry(string captureFile, DateTime start, DateTime end, string cameraAlias, string cameraIdentifier, double configuredFramerate, double receivedFramerate, int drops)
        {
            this.CaptureFile = captureFile;
            this.Start = start;
            this.End = end;
            this.CameraAlias = cameraAlias;
            this.CameraIdentifier = cameraIdentifier;
            this.ConfiguredFramerate = configuredFramerate;
            this.ReceivedFramerate = receivedFramerate;
            this.Drops = drops;
        }
    }
}
