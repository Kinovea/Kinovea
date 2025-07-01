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
using Kinovea.Services;

namespace Kinovea.Video
{
    public class VideoOptions
    {
        /// <summary>
        /// 图像宽高比
        /// </summary>
        public ImageAspectRatio ImageAspectRatio { get; set; }
        public ImageRotation ImageRotation { get; set; }
        public Demosaicing Demosaicing { get; set; }
        public bool Deinterlace { get; set; }

        public VideoOptions(ImageAspectRatio aspect, ImageRotation rotation, Demosaicing demosaicing, bool deinterlace)
        {
            ImageAspectRatio = aspect;
            ImageRotation = rotation;
            Demosaicing = demosaicing;
            Deinterlace = deinterlace;
        }
        
        public static VideoOptions Default {
            get { return new VideoOptions(ImageAspectRatio.Auto, ImageRotation.Rotate0, Demosaicing.None, false);}
        }
    }
}
