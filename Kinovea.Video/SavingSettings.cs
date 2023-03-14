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
    public struct SavingSettings
    {
        public VideoSection Section;
        public string Metadata;
        public string File;
        public double InputFrameInterval;
        public double OutputFrameInterval;
        public int Duplication;
        public int KeyframeDuplication;
        public bool FlushDrawings;
        public bool KeyframesOnly;
        public bool PausedVideo;
        public ImageRetriever ImageRetriever;
        public long EstimatedTotal;
        
    }
}
