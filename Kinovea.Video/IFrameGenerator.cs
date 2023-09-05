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
using System.Drawing;
using Kinovea.Services;

namespace Kinovea.Video
{
    /// <summary>
    /// VideoReaderGenerator helper objects.
    /// Must be able to generate a frame for any arbitrary timestamp asked.
    /// The frame generator owns the memory of the generated images.
    /// </summary>
    public interface IFrameGenerator
    {
        /// <summary>
        /// Size of images as stored in the file.
        /// </summary>
        Size OriginalSize { get; }

        /// <summary>
        /// Size of images after aspect ratio fix and rotation.
        /// </summary>
        Size ReferenceSize { get; }

        /// <summary>
        /// Orientation of images.
        /// </summary>
        ImageRotation ImageRotation { get; }

        /// <summary>
        /// Open the file.
        /// </summary>
        OpenVideoResult Open(string filename);

        /// <summary>
        /// Close the file and clean resources.
        /// </summary>
        void Close();

        /// <summary>
        /// Change the orientation of the output image. 
        /// </summary>
        void SetRotation(ImageRotation rotation);

        /// <summary>
        /// Produce the image at the passed timestamp.
        /// </summary>
        Bitmap Generate(long timestamp);
        
        /// <summary>
        /// Produce the image at the passed timestamp and size.
        /// </summary>
        Bitmap Generate(long timestamp, Size maxSize);
        
        /// <summary>
        /// Dispose the last generated image.
        /// </summary>
        void DisposePrevious(Bitmap previous);
    }
}
