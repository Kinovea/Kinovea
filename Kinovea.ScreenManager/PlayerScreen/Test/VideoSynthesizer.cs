using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Video;
using System.Drawing;
using Kinovea.Video.FFMpeg;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Globalization;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Creates video at a given framerate, size and with object moving at given speed.
    /// </summary>
    public class VideoSynthesizer
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void CreateVideo(string path, int width, int height, double fps, double durationSeconds, MovingObject o)
        {
            string extension = "avi";
            string filename = string.Format("{0:yyyyMMddTHHmmss}-{1}x{2}px-{3}s-{4}Hz.{5}", DateTime.Now, width, height, durationSeconds, fps, extension);
            string filepath = Path.Combine(path, filename);
            int intervalMilliseconds = (int)(1000 / fps);
            Size frameSize = new Size(width, height);

            Random random = new Random();

            VideoRecorder recorder = new VideoRecorder();
            SaveResult result = recorder.Initialize(filepath, intervalMilliseconds, frameSize);

            if (result != SaveResult.Success)
                return;

            int x = 0;
            int y = 500;
            int frameCount = (int)(durationSeconds * 1000 / intervalMilliseconds);
            List<Bitmap> frames = new List<Bitmap>();
            for (int i = 0; i < frameCount; i++)
            {
                Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
                Graphics g = Graphics.FromImage(bmp);

                x += (int)(o.SpeedX * ((double)intervalMilliseconds / 1000));
                y += (int)(o.SpeedY * ((double)intervalMilliseconds / 1000));
                double rndX = random.NextDouble() * o.NoiseX * 2 - o.NoiseX;
                double rndY = random.NextDouble() * o.NoiseY * 2 - o.NoiseY;
                PointF position = new PointF((float)(x + rndX), (float)(y + rndY));

                DrawImage(g, position, i, width, height, durationSeconds, fps, o);

                frames.Add(bmp);
                recorder.EnqueueFrame(bmp);
                
                Thread.Sleep(50);
            }

            recorder.Close();

            foreach (Bitmap frame in frames)
                frame.Dispose();
        }

        private void DrawImage(Graphics g, PointF location, int frame, int width, int height, double duration, double fps, MovingObject o)
        {
            g.FillRectangle(Brushes.White, g.ClipBounds);
            Font f = new Font("Consolas", 16, FontStyle.Regular);

            // Information
            g.DrawString(string.Format(CultureInfo.InvariantCulture, "Image size (px)    : {0}×{1}", width, height), f, Brushes.Black, new PointF(10, 10));
            g.DrawString(string.Format(CultureInfo.InvariantCulture, "Video duration (s) : {0:0.000}", duration), f, Brushes.Black, new PointF(10, 30));
            g.DrawString(string.Format(CultureInfo.InvariantCulture, "Framerate (Hz)     : {0:0.000}", fps), f, Brushes.Black, new PointF(10, 50));

            g.DrawString(string.Format(CultureInfo.InvariantCulture, "Moving object      :"), f, Brushes.Black, new PointF(10, 70));
            g.DrawString(string.Format(CultureInfo.InvariantCulture, "\tradius (px)    : {0}", o.Radius), f, Brushes.Black, new PointF(10, 90));
            g.DrawString(string.Format(CultureInfo.InvariantCulture, "\tspeed x (px/s) : {0:0.000}", o.SpeedX), f, Brushes.Black, new PointF(10, 110));
            g.DrawString(string.Format(CultureInfo.InvariantCulture, "\tspeed y (px/s) : {0:0.000}", o.SpeedY), f, Brushes.Black, new PointF(10, 130));
            g.DrawString(string.Format(CultureInfo.InvariantCulture, "\tnoise x (px)   : ±{0:0.000}", o.NoiseX), f, Brushes.Black, new PointF(10, 150));
            g.DrawString(string.Format(CultureInfo.InvariantCulture, "\tnoise y (px)   : ±{0:0.000}", o.NoiseY), f, Brushes.Black, new PointF(10, 170));
            f.Dispose();

            Font f2 = new Font("Consolas", 20, FontStyle.Regular);
            g.DrawString(string.Format(CultureInfo.InvariantCulture, "Frame number : {0}", frame), f2, Brushes.Black, new PointF(10, 200));
            f2.Dispose();

            // Object
            g.FillEllipse(Brushes.Black, location.X, location.Y, o.Radius * 2, o.Radius * 2);
        }


    }
}
