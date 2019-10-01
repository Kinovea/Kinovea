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

        public float Threshold { get; set; } = 0.9f;

        private bool enabled;
        private WaveInEvent waveIn = null;
        private bool started;
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
                Stop();
                dummy.Dispose();
                waveIn.DataAvailable -= WaveIn_DataAvailable;
                waveIn.RecordingStopped -= WaveIn_RecordingStopped;
                waveIn.Dispose();
            }
        }
        #endregion

        public void Enable(bool value)
        {
            enabled = value;
            if (started && !enabled)
                Stop();
            else if (!started && enabled)

                Start();
        }
        public void Start()
        {
            if (started)
                return;

            if (WaveIn.DeviceCount == 0)
                return;

            if (waveIn == null)
            {
                waveIn = new WaveInEvent();
                waveIn.WaveFormat = new WaveFormat(22050, 1);
                waveIn.DataAvailable += WaveIn_DataAvailable;
                waveIn.RecordingStopped += WaveIn_RecordingStopped;
            }

            waveIn.DeviceNumber = 0;
            started = true;
            waveIn.StartRecording();

            WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveIn.DeviceNumber);
            log.DebugFormat("Audio input level monitor started: {0}", deviceInfo.ProductName);
        }

        public void Stop()
        {
            if (!started)
                return;

            started = false;
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

                Start();
            }
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (!enabled || !started)
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

            if (max < Threshold)
                return;

            log.DebugFormat("Audio input level above threshold: {0:0.000}.", max);

            dummy.BeginInvoke((Action)delegate
            {
                if (ThresholdPassed != null)
                    ThresholdPassed(this, EventArgs.Empty);
            });
        }
    }
}
