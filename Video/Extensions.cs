#region License
/*
Copyright © Joan Charmant 2011.
joan.charmant@gmail.com 
 
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
using System.Text;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Kinovea.Video
{
    public static class Extensions
    {
        /// <summary>
        /// Deep clone of a bitmap.
        /// </summary>
        public static Bitmap CloneDeep(this Bitmap _bmp)
        {
            if(object.ReferenceEquals(_bmp, null))
                return null;
            
            Bitmap clone = new Bitmap(_bmp.Width, _bmp.Height, _bmp.PixelFormat);
            Graphics g = Graphics.FromImage(clone);
            g.DrawImageUnscaled(_bmp, 0, 0);
			return clone;
        }
    }
}
