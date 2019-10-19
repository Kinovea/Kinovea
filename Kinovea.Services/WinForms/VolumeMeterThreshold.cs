using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kinovea.Services
{
    /// <summary>
    /// A volume meter with a threshold hairline.
    /// Receives values in [0..1] and display them on a log scale.
    /// </summary> 
    public partial class VolumeMeterThreshold : Control
    {
        #region Events
        [Category("Property Changed")]
        public event EventHandler ThresholdChanged;
        #endregion

        /// <summary>
        /// Input level in [0..1].
        /// </summary>
        [DefaultValue(0.0)]
        public float Amplitude
        {
            get { return amplitude; }
            set
            {
                amplitude = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Threshold level in [0..1].
        /// </summary>
        [DefaultValue(0.8)]
        public float Threshold
        {
            get { return threshold; }
            set
            {
                threshold = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Threshold level linearized.
        /// This is only used for UI purposes.
        /// </summary>
        [DefaultValue(0)]
        public float ThresholdLinear
        {
            get
            {
                return Map(threshold);
            }
            set
            {
                threshold = Unmap(value);
                this.Invalidate();
            }
        }

        /// <summary>
        /// Assumed range of audible sounds picked up by the input device, in decibels.
        /// </summary>
        [DefaultValue(60.0)]
        public float DecibelRange
        {
            get { return decibelRange; }
            set
            {
                decibelRange = value;
                this.Invalidate();
            }
        }

        private float amplitude;
        private float threshold = 0.8f;
        private float decibelRange = 60;

        public VolumeMeterThreshold()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            InitializeComponent();
            this.MouseMove += VolumeMeterThreshold_MouseMove;
            this.MouseDown += VolumeMeterThreshold_MouseDown;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            int barHeight = 5;
            int top = (this.Height - barHeight) / 2;
            pe.Graphics.FillRectangle(Brushes.LightGray, 0, top, this.Width, barHeight);

            if (!this.Enabled)
                return;

            int amplitudePixels = GetPixels(Map(amplitude));
      
            if (amplitude >= threshold)
                pe.Graphics.FillRectangle(Brushes.Red, 0, top, amplitudePixels, barHeight);
            else
                pe.Graphics.FillRectangle(Brushes.Black, 0, top, amplitudePixels, barHeight);

            int thresholdPixels = GetPixels(Map(threshold));
            pe.Graphics.DrawLine(Pens.Red, thresholdPixels, 0, thresholdPixels, this.Height - 1);
        }

        private void VolumeMeterThreshold_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            float value = (float)e.X / this.Width;
            Threshold = Unmap(value);

            if (ThresholdChanged != null)
                ThresholdChanged(this, EventArgs.Empty);
        }

        private void VolumeMeterThreshold_MouseDown(object sender, MouseEventArgs e)
        {
            float value = (float)e.X / this.Width;
            Threshold = Unmap(value);

            if (ThresholdChanged != null)
                ThresholdChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Maps an amplitude value in [0..1] to a logarithmic curve.
        /// </summary>
        private float Map(float x)
        {
            // [0..1] -> [-range..0].
            float decibels = (float)(20 * Math.Log10(x));
            decibels = Math.Min(Math.Max(decibels, -decibelRange), 0);

            // [-range..0] -> [0..1].
            float value = (decibels + decibelRange) / decibelRange;
            return value;
        }

        /// <summary>
        /// Maps a slider location in [0..1] to the original amplitude value needed to map there.
        /// </summary>
        private float Unmap(float x)
        {
            x = Math.Min(Math.Max(x, 0.0f), 1.0f);
      
            // [0..1] -> [-range..0].
            float decibels = (x * decibelRange) - decibelRange;

            // [-range..0] -> [0..1].
            float value = (float)Math.Pow(10, decibels / 20);
            return value;
        }

        private int GetPixels(float value)
        {
            // Scale back to view space.
            int pixels = (int)(this.Width * value);
            pixels = Math.Min(pixels, this.Width - 1);
            return pixels;
        }
    }
}
