#region License
/*
Copyright © Joan Charmant 2012.
jcharmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.
*/
#endregion
using System;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public class TrackingContext
    {
        public long Time
        {
            get { return time; }
        }

        public Bitmap Image
        {
            get { return image; }
        }

        public OpenCvSharp.Mat CVImage
        {
            get { return cvImage; }
        }

        private long time;
        private Bitmap image;
        private OpenCvSharp.Mat cvImage;

        public TrackingContext(long time, Bitmap image)
        {
            this.time = time;
            this.image = image;

            cvImage = OpenCvSharp.Extensions.BitmapConverter.ToMat(image);
        }
        
        public void Dispose()
        {
            cvImage.Dispose();
        }

        public override string ToString()
        {
            return string.Format("[TrackingContext] Time:{0}, Image Size:{1}", time, image.Size);
        }

    }
}
