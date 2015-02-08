#region License
/*
Copyright © Joan Charmant 2013.
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
using System.Drawing;
using Kinovea.Pipeline;

namespace Kinovea.Camera
{
    public class CameraThumbnailProducedEventArgs : EventArgs
    {
        public readonly CameraSummary Summary;
        public readonly Bitmap Thumbnail;
        public readonly ImageDescriptor ImageDescriptor;
        public readonly bool HadError;
        public readonly bool Cancelled;
        public CameraThumbnailProducedEventArgs(CameraSummary summary, Bitmap thumbnail, ImageDescriptor imageDescriptor, bool hadError, bool cancelled)
        {
            this.Summary = summary;
            this.Thumbnail = thumbnail;
            this.ImageDescriptor = imageDescriptor;
            this.HadError = hadError;
            this.Cancelled = cancelled;
        }
    }
}