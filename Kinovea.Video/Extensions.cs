#region License
/*
Copyright © Joan Charmant 2011.
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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Text;

namespace Kinovea.Video
{
    public static class Extensions
    {
        /// <summary>
        /// Extract a rectangular region out of a bitmap.
        /// </summary>
        public static Bitmap ExtractTemplate(this Bitmap image, Rectangle region)
        {
            // TODO: test perfs by simply drawing in the new image.
            
            Bitmap template = new Bitmap(region.Width, region.Height, PixelFormat.Format32bppPArgb);
            
            BitmapData imageData = image.LockBits( new Rectangle( 0, 0, image.Width, image.Height ), ImageLockMode.ReadOnly, image.PixelFormat );
            BitmapData templateData = template.LockBits(new Rectangle( 0, 0, template.Width, template.Height ), ImageLockMode.ReadWrite, template.PixelFormat );
                
            int pixelSize = 4;
                
            int tplStride = templateData.Stride;
            int templateWidthInBytes = region.Width * pixelSize;
            int tplOffset = tplStride - templateWidthInBytes;
            
            int imgStride = imageData.Stride;
            int imageWidthInBytes = image.Width * pixelSize;
            int imgOffset = imgStride - (image.Width * pixelSize) + imageWidthInBytes - templateWidthInBytes;
            
            int startY = Math.Max(0, region.Top);
            int startX = Math.Max(0, region.Left);
            
            unsafe
            {
                byte* pTpl = (byte*) templateData.Scan0.ToPointer();
                byte* pImg = (byte*) imageData.Scan0.ToPointer()  + (imgStride * startY) + (pixelSize * startX);
                
                for ( int row = 0; row < region.Height; row++ )
                {
                    if(startY + row > imageData.Height - 1)
                        break;
                    
                    for ( int col = 0; col < templateWidthInBytes; col++, pTpl++, pImg++ )
                    {
                        if(startX * pixelSize + col < imageWidthInBytes)
                            *pTpl = *pImg;	
                    }
                    
                    pTpl += tplOffset;
                    pImg += imgOffset;
                }
            }
            
            image.UnlockBits(imageData);
            template.UnlockBits(templateData);
            
            return template;
        }

        public static Color Invert(this Color color)
        {
            return Color.FromArgb(color.A, 255 - color.R, 255 - color.G, 255 - color.B);
        }
    }
}
