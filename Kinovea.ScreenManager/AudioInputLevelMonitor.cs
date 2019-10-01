using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Monitors the audio input level and raises an event when it's past a user-defined threshold.
    /// This is used for capture automation.
    /// </summary>
    public class AudioInputLevelMonitor : IDisposable
    {
        public event EventHandler ThresholdPassed;
        public event EventHandler<float> LevelChanged;

        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                if (!enabled && started)
                    Stop();
            }
        }
        public float Threshold { get; set; } = 0.9f;
        
        private bool enabled;
        private WaveInEvent waveIn = null;
        private bool started;
        private string currentDeviceId;
        private bool changeDeviceAsked;
        private string nextDeviceId;

        private Control dummy = new Control();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Construction & disposal
        public AudioInputLevelMonitor()
        {
            IntPtr forceHandleCreation = dummy.Handle; // Needed to show that the main thread "owns" this Control.
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~AudioInputLevelMonitor()
        {
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                waveIn.StopRecording();
                waveIn.DataAvailable -= WaveIn_DataAvailable;
                waveIn.RecordingStopped -= WaveIn_RecordingStopped;
                waveIn.Dispose();
                dummy.Dispose();
            }
        }
        #endregion

        public static List<AudioInputDevice> GetDevices()
        {
            List<AudioInputDevice> devices = new List<AudioInputDevice>();
            int waveInDevices = WaveIn.DeviceCount;
            for (int i = 0; i < waveInDevices; i++)
                devices.Add(new AudioInputDevice(WaveIn.GetCapabilities(i)));

            return devices;
        }

        public void Start(string id)
        {
#if DEBUG
            if (!Enabled)
              throw new InvalidProgramException();
#endif

            if (!string.IsNullOrEmpty(id) && id == currentDeviceId)
                return;

            if (started)
            {
                // We must wait until the recorder is fully closed before restarting it.
                changeDeviceAsked = true;
                nextDeviceId = id;
                Stop();
                return;
            }

            changeDeviceAsked = false;
            nextDeviceId = null;

            if (WaveIn.DeviceCount == 0)
            {
                log.DebugFormat("Audio input level monitor failed to start, no input device available.");
                return;
            }

            if (waveIn == null)
            {
                waveIn = new WaveInEvent();
                waveIn.WaveFormat = new WaveFormat(22050, 1);
                waveIn.DataAvailable += WaveIn_DataAvailable;
                waveIn.RecordingStopped += WaveIn_RecordingStopped;
            }

            int deviceNumber = 0;
            if (!string.IsNullOrEmpty(id))
            {
                int waveInDevices = WaveIn.DeviceCount;
                for (int i = 0; i < waveInDevices; i++)
                {
                    WaveInCapabilities caps = WaveIn.GetCapabilities(i);
                    if (caps.ProductGuid.ToString() == id)
                    {
                        deviceNumber = i;
                        break;
                    }
                }
            }

            waveIn.DeviceNumber = deviceNumber;
            started = true;
            waveIn.StartRecording();

            WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveIn.DeviceNumber);
            currentDeviceId = deviceInfo.ProductGuid.ToString();

            log.DebugFormat("Audio input level monitor started: {0}", deviceInfo.ProductName);
        }

        public void Stop()
        {
            if (!started)
                return;

            waveIn.StopRecording();
            log.DebugFormat("Audio input level monitor stopped.");
        }

        private void WaveIn_RecordingStopped(object sender, StoppedEventArgs e)
        {
            started = false;
            
            if (e.Exception != null)
            {
                log.ErrorFormat("Audio input level monitor stopped unexpectedly. {0}", e.Exception.Message);
                waveIn.DataAvailable -= WaveIn_DataAvailable;
                waveIn.RecordingStopped -= WaveIn_RecordingStopped;
                waveIn.Dispose();
                waveIn = null;

                dummy.BeginInvoke((Action)delegate {
                    Start(currentDeviceId);
                });
            }

            currentDeviceId = null;

            if (changeDeviceAsked && !string.IsNullOrEmpty(nextDeviceId) && nextDeviceId != Guid.Empty.ToString())
            {
                // This happens when we want to switch from one device to another.
                // Now that we know the recording has properly stopped, we can restart it on the new device.
                dummy.BeginInvoke((Action)delegate {
                    Start(nextDeviceId);
                });
            }
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (!Enabled || !started)
                return;

            // Measure the peak level over the period and send an event if above threshold.
            float max = 0;
            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                // This project runs in 'checked' mode for Debug builds.
                // The following operation leverages the wrapping happening from the arithmetic overflow.
                // The << operator makes the result into an int larger than short.MaxValue.
                short sample;
                unchecked
                {
                    sample = (short)((e.Buffer[index + 1] << 8) | e.Buffer[index + 0]);
                }
                
                float sample32 = Math.Abs(sample / 32768f);

                if (sample32 > max)
                    max = sample32;
            }

            //log.DebugFormat("Audio input level: {0:0.000}.", max);

            if (LevelChanged != null)
            {
                dummy.BeginInvoke((Action)delegate {
                    LevelChanged(this, max);
                });
            }

            if (max < Threshold)
                return;

            log.DebugFormat("Audio input level above threshold: {0:0.000}.", max);

            if (ThresholdPassed != null)
            {
                dummy.BeginInvoke((Action)delegate {
                    ThresholdPassed(this, EventArgs.Empty);
                });
            }
        }
    }
}
