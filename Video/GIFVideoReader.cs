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

namespace Kinovea.Video
{
    [SupportedExtensions(new string[] {".gif"})]
    public class GIFVideoReader : VideoReader
    {
        public override bool Loaded
        {
            get { return false; }
        }
        public override VideoInfo Info
        {
            get { return m_VideoInfo; }
        }
        public override VideoSection Selection
        {
            get { return m_Selection; }
        }
        public override bool Caching
        {
            get { return true; }
        }
        public override VideoFrame Current
        {
            get { return null; }
        }
        public override OpenVideoResult Open(string _FilePath)
        {
            return OpenVideoResult.NotSupported;
        }
        public override void Close()
        {
            // TODO
        }
        
        private VideoInfo m_VideoInfo;
        private VideoSection m_Selection;
        
    }
}
