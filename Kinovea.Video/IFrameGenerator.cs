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
using System.Drawing;

namespace Kinovea.Video
{
    /// <summary>
    /// VideoReaderGenerator helper objects.
    /// Must be able to generate a frame for any arbitrary timestamp asked.
    /// </summary>
    public interface IFrameGenerator
    {
        Bitmap Generate(long timestamp);
        Bitmap Generate(long timestamp, Size maxSize);

        void DisposePrevious(Bitmap previous);
        OpenVideoResult Initialize(string init);
        void Close();
        Size Size {get;}
    }
}
