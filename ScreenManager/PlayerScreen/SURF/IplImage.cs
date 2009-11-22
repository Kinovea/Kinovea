
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace OpenSURF
{
    public class IplImage
    {
        public float[] imageData;
        public int width;
        public int height;
        public int widthStep; // taille en bytes d'une ligne

        public IplImage(float[] imageData, int width, int height, int widthStep)
        {
            this.imageData = imageData;
            this.width = width;
            this.height = height;
            this.widthStep = widthStep;
        }

        public IplImage BuildIntegral(string Path)
        {
        	// Builds the ImageIntegral. maybe takes a file if already saved.
            IplImage vret = null;

            float[] imageDataD = COpenSURF.ImageIntegral(imageData, width, height, widthStep);

            if (Path != null)
            {
                FileStream pfd = null;
                try
                {
                    pfd = new FileStream(Path + ".INT", FileMode.Create, FileAccess.Write);
                    BinaryWriter pbw = new BinaryWriter(pfd);
                    pbw.Write(width);
                    pbw.Write(height);
                    foreach (float value in imageDataD)
                    {
                        pbw.Write(value);
                    }
                }
                finally
                {
                    if (pfd != null) pfd.Close();
                }
            }

            vret = new IplImage(imageDataD, width, height, widthStep);

            return vret;
        }

        unsafe public static IplImage LoadImage(string Path)
        {
            Bitmap pbitmap = null;
            try
            {
                pbitmap = new Bitmap(Path);
                return LoadImage(pbitmap);
            }
            finally
            {
                if (pbitmap != null) pbitmap.Dispose();
            }
        }

        unsafe public static IplImage LoadImage(Bitmap pBitmap)
        {
            IplImage vret = null;

            if (pBitmap == null) return vret;

            BitmapData bitmapdata = null;
            bool bLockBits = true;
            try
            {

                bitmapdata = pBitmap.LockBits(new Rectangle(0, 0, pBitmap.Width, pBitmap.Height),
                                                    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                bLockBits = true;

                int width = pBitmap.Width;
                int height = pBitmap.Height;
                int widthStep = width*sizeof(float);
                float[] imageData = new float[width * height];
                int iximagedata = 0;

                byte* pscan0 = (byte*)bitmapdata.Scan0;
                for (int i = 0; i < bitmapdata.Height; i++)
                {
                    byte* prow = pscan0;
                    pscan0 += bitmapdata.Stride;
                    for (int j = 0; j < bitmapdata.Width; j++)
                    {
                        byte b = *prow++;
                        byte g = *prow++;
                        byte r = *prow++;
                        byte a = *prow++;
                        // value = 0.3 R + 0.59 G + 0.11 B
                        double luminance = (0.3d * (double)r + 0.59d * (double)g + 0.11d * (float)b);
                        imageData[iximagedata++] = (float)(luminance / 255d); // TBC
                    }
                }

                vret = new IplImage(imageData, width, height, widthStep);

            }
            finally
            {
                if (bLockBits)
                {
                    pBitmap.UnlockBits(bitmapdata);
                }
            }

            return vret;
        }

    }
}
