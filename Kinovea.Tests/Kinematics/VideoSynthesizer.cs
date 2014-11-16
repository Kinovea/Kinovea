using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Video;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Drawing.Drawing2D;
using Kinovea.ScreenManager;

namespace Kinovea.Tests.Kinematics
{
    /// <summary>
    /// Creates video at a given framerate, size and with object moving at given speed.
    /// </summary>
    public class VideoSynthesizer
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void CreateVideo(string outputPath, int width, int height, double fps, double durationSeconds, MovingObject o)
        {

            // Note: this class is no longer buildable as the VideoRecorder has been deprecated in favor of the Pipeline framework.
            // Ideally this code would be in the KSV plug-in anyway.
            
            /*string extension = "avi";
            string filename = string.Format("{0:yyyyMMddTHHmmss}-{1}x{2}px-{3}s-{4}Hz.{5}", DateTime.Now, width, height, durationSeconds, fps, extension);
            string filepath = Path.Combine(outputPath, filename);
            int intervalMilliseconds = (int)(1000 / fps);
            Size frameSize = new Size(width, height);

            VideoRecorder recorder = new VideoRecorder();
            SaveResult result = recorder.Initialize(filepath, intervalMilliseconds, frameSize);

            if (result != SaveResult.Success)
                return;

            double a = 1000;

            int frameCount = (int)(durationSeconds * 1000 / intervalMilliseconds);
            List<Bitmap> frames = new List<Bitmap>();
            for (int i = 0; i < frameCount; i++)
            {
                Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
                Graphics g = Graphics.FromImage(bmp);

                double t = GetTime(i, fps);
                PointF point = GetPosition(t, a);
                DrawImage(g, point, i, t, a, width, height, durationSeconds, fps, o);

                frames.Add(bmp);
                recorder.EnqueueFrame(bmp);
                
                Thread.Sleep(50);
            }

            recorder.Close();

            foreach (Bitmap frame in frames)
                frame.Dispose();*/
        }

        private void DrawImage(Graphics g, PointF location, int frame, double t, double a, int width, int height, double duration, double fps, MovingObject o)
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;

            g.FillRectangle(Brushes.White, g.ClipBounds);
            Font f = new Font("Consolas", 16, FontStyle.Regular);

            // Information
            g.DrawString(string.Format(CultureInfo.InvariantCulture, "Image size (px)    : {0}×{1}", width, height), f, Brushes.Black, new PointF(10, 10));
            g.DrawString(string.Format(CultureInfo.InvariantCulture, "Video duration (s) : {0:0.000}", duration), f, Brushes.Black, new PointF(10, 30));
            g.DrawString(string.Format(CultureInfo.InvariantCulture, "Framerate (Hz)     : {0:0.000}", fps), f, Brushes.Black, new PointF(10, 50));

            g.DrawString(string.Format(CultureInfo.InvariantCulture, "Moving object      :"), f, Brushes.Black, new PointF(10, 70));
            //g.DrawString(string.Format(CultureInfo.InvariantCulture, "\tradius (px)    : {0}", o.Radius), f, Brushes.Black, new PointF(10, 90));
            g.DrawString(string.Format(CultureInfo.InvariantCulture, "\tx (px)         : {0:0.000} (fixed)", location.X), f, Brushes.Black, new PointF(10, 110));
            g.DrawString(string.Format(CultureInfo.InvariantCulture, "\ty (px)         : {0:0.000} (500 + 0.5 * a * t²)", location.Y), f, Brushes.Black, new PointF(10, 130));
            g.DrawString(string.Format(CultureInfo.InvariantCulture, "\ta (px/s²)      : {0}", a), f, Brushes.Black, new PointF(10, 150));
            g.DrawString(string.Format(CultureInfo.InvariantCulture, "\tt (s)          : {0:0.000} (frame number / framerate)", t), f, Brushes.Black, new PointF(10, 170));
            f.Dispose();

            Font f2 = new Font("Consolas", 20, FontStyle.Regular);
            g.DrawString(string.Format(CultureInfo.InvariantCulture, "Frame number : {0}", frame), f2, Brushes.Black, new PointF(10, 220));
            f2.Dispose();

            g.DrawLine(Pens.Black, 0, 270, 1000, 270);

            // Object
            //g.FillEllipse(Brushes.Black, new RectangleF(location.X - o.Radius, location.Y - o.Radius, o.Radius * 2, o.Radius * 2));
            g.DrawRectangle(Pens.Black, location.X, location.Y, 1, 1);
        }

        /// <summary>
        /// Returns the time to be used for motion computation.
        /// This is where systematic errors or rolling shutter may happen.
        /// The time coordinate at object location may not always be the same as the corresponding frame time.
        /// </summary>
        private double GetTime(int frame, double fps)
        {
            double t = (double)frame / fps;
            return t;
        }

        /// <summary>
        /// Returns the coordinates based on time in seconds.
        /// </summary>
        private PointF GetPosition(double t, double a)
        {
            PointF center = new PointF(500, 500);
            float x = center.X;
            float signal = (float)(0.5 * a * t * t);
            float noise = 0;
            float y = center.Y + signal + noise;

            return new PointF(x, y);
        }

        private PointF GetOscillating(double t)
        {
            float factor = 40;
            PointF center = new PointF(500, 500);
            float x = center.X;
            float signal = (float)(factor * Math.Sin(4 * Math.PI * t));
            //float noise = (float)((factor/100) * Math.Sin(40 * Math.PI * t));
            float noise = 0;
            float y = center.Y + signal + noise;

            return new PointF(x, y);
        }
    }
}
