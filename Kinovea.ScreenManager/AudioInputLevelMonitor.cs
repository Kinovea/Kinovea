using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kinovea.Services;

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
        public event EventHandler DeviceLost;

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
        private bool stopAsked;

        private Control dummy = new Control();
        private static DateTime quietPeriodStart = DateTime.MinValue;
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
                if (waveIn != null)
                {
                    waveIn.StopRecording();
                    waveIn.DataAvailable -= WaveIn_DataAvailable;
                    waveIn.RecordingStopped -= WaveIn_RecordingStopped;
                    waveIn.Dispose();
                }

                dummy.Dispose();
            }
        }
        #endregion

        #region Static methods
        /// <summary>
        /// Get all the audio devices on the system.
        /// </summary>
        public static List<AudioInputDevice> GetDevices()
        {
            List<AudioInputDevice> devices = new List<AudioInputDevice>();
            int waveInDevices = WaveIn.DeviceCount;
            for (int i = 0; i < waveInDevices; i++)
                devices.Add(new AudioInputDevice(WaveIn.GetCapabilities(i)));

            return devices;
        }

        /// <summary>
        /// Reset the start of the quiet period for all audio input level monitors.
        /// </summary>
        public static void StartQuietPeriod()
        {
            log.DebugFormat("Entering quiet period");
            quietPeriodStart = DateTime.Now;
        }

        /// <summary>
        /// Return true if we are currently within the quiet period.
        /// </summary>
        public static bool IsQuiet()
        {
            float quietPeriod = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.AudioQuietPeriod;
            double ellapsed = (DateTime.Now - quietPeriodStart).TotalSeconds;
            return quietPeriod != 0 && ellapsed < quietPeriod;
        }
        #endregion

        public void Start(string id)
        {
#if DEBUG
            if (!Enabled)
              throw new InvalidProgramException();
#endif

            if (!string.IsNullOrEmpty(id) && id == currentDeviceId && started)
                return;

            if (started)
            {
                // We must wait until the monitor is fully closed before restarting it.
                // We will call back into here from the "device stopped" event.
                log.DebugFormat("Audio input level monitor already started on a different device.");
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
                DeviceLost?.Invoke(this, EventArgs.Empty);
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
                    WaveInCapabilities wic = WaveIn.GetCapabilities(i);
                    if (wic.ProductName == id)
                    {
                        deviceNumber = i;
                        break;
                    }
                }
            }

            try
            {
                waveIn.DeviceNumber = deviceNumber;
                waveIn.StartRecording();
                started = true;
            
                WaveInCapabilities wic = WaveIn.GetCapabilities(waveIn.DeviceNumber);
                currentDeviceId = wic.ProductName;

                log.DebugFormat("Audio input level monitor started: {0}", wic.ProductName);
            }
            catch(Exception e)
            {
                log.ErrorFormat("The microphone is not available. {0}", e.Message);
                DeviceLost?.Invoke(this, EventArgs.Empty);
                currentDeviceId = null;
            }
        }

        public void Stop()
        {
            if (!started)
                return;

            stopAsked = true;
            waveIn.StopRecording();
            stopAsked = false;
            log.DebugFormat("Audio input level monitor stopped.");
        }

        private void WaveIn_RecordingStopped(object sender, StoppedEventArgs e)
        {
            // Three scenarios to come into here:
            // - The audio device was lost.
            // - We are stopping monitoring. (stopAsked = true).
            // - We are changing the monitored device. (changeDeviceAsked = true).

            started = false;
            
            if (e.Exception != null)
            {
                log.ErrorFormat("Audio input level monitor stopped unexpectedly. {0}", e.Exception.Message);
                waveIn.DataAvailable -= WaveIn_DataAvailable;
                waveIn.RecordingStopped -= WaveIn_RecordingStopped;
                waveIn.Dispose();
                waveIn = null;
                
                // Alert of the problem and try to force restart the device.
                dummy.BeginInvoke((Action)delegate
                {
                    if (!stopAsked)
                        Start(currentDeviceId);
                    else
                        currentDeviceId = null;
                });

                return;
            }
            
            currentDeviceId = null;
            if (stopAsked)
                return;
            
            if (changeDeviceAsked && !string.IsNullOrEmpty(nextDeviceId))
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

            if (IsQuiet())
            {
                log.DebugFormat("Audio input level above threshold during quiet period: ignored.");
                return;
            }

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
