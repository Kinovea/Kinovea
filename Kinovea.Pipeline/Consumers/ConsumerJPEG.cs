using System;
using System.Collections.Generic;
using System.Linq;
using Kinovea.Services;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Kinovea.Pipeline.Consumers
{
    /// <summary>
    /// For testing purpose only.
    /// Compress images using .NET JPEG codec, does not write anything to disk.
    /// </summary>
    public class ConsumerJPEG : AbstractConsumer
    {
        public override BenchmarkCounterBandwidth BenchmarkCounter
        {
            get { return counter; }
        }

        private ImageCodecInfo jpegCodec;
        private EncoderParameters encoderParameters;
        private Bitmap bitmap;
        private BitmapData bmpData;
        private Stopwatch sw = new Stopwatch();
        private BenchmarkCounterBandwidth counter = new BenchmarkCounterBandwidth();

        protected override void Initialize()
        {
            base.Initialize();

            jpegCodec = GetEncoderInfo("image/jpeg");

            Encoder encoder = Encoder.Quality;
            encoderParameters = new EncoderParameters(1);
            EncoderParameter encoderParameter = new EncoderParameter(encoder, 90L);
            encoderParameters.Param[0] = encoderParameter;

            // The bitmap is created upfront and left locked open until the resolution is changed.
            int width = 2048;
            int height = 1084;
            CreateBitmap(width, height, PixelFormat.Format8bppIndexed);
        }

        protected override void ProcessEntry(long position, Frame entry)
        {
            // We use a new MemoryStream in each loop.
            // However in real code it should be possible to keep file stream open and constantly write to it.
            using (MemoryStream jpegStream = new MemoryStream())
            {
                sw.Reset();
                sw.Start();

                // We first need to copy the bytes into a proper bitmap, as the jpeg encoder works with a bitmap object.
                // Marshal.Copy is about 1ms on 2K.
                Marshal.Copy(entry.Buffer, 0, bmpData.Scan0, bmpData.Stride * bitmap.Height);
                
                bitmap.Save(jpegStream, jpegCodec, encoderParameters);

                long streamPosition = jpegStream.Position;

                counter.Post((int)sw.ElapsedMilliseconds, frameLength);
            }
        }

        private void CreateBitmap(int width, int height, PixelFormat format)
        {
            bitmap = new Bitmap(width, height, format);

            if (bitmap.PixelFormat != PixelFormat.Format8bppIndexed)
                return;
            
            ColorPalette colorPalette = bitmap.Palette;

            for (int i = 0; i < 256; i++)
                colorPalette.Entries[i] = Color.FromArgb(i, i, i);
                
            bitmap.Palette = colorPalette;

            bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
        }

        private ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            for (int i = 0; i < codecs.Length; i++)
            {
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];
            }

            return null;
        }
    }
}
