#region License
/*
Copyright © Joan Charmant 2009.
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
using System.Windows.Forms;
using System.Xml;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// IVideoFilter defines the way all Video filters should behave.
    /// A VideoFilter here is an object that creates a display frame
    /// on the fly by processing the cached frames.
    /// </summary>
    public interface IVideoFilter : IDisposable
    {
        #region Properties
        /// <summary>
        /// List of context menus specific to the filter.
        /// </summary>
        List<ToolStripItem> ContextMenu { get; }

        /// <summary>
        /// Rendered bitmap to be displayed on the viewport.
        /// </summary>
        Bitmap Current { get; }

        bool CanExportVideo { get; }

        bool CanExportImage { get; }
        #endregion

        #region Methods
        void ResetData();

        void SetFrames(IWorkingZoneFramesContainer framesContainer);

        void UpdateSize(Size size);

        void UpdateTime(long timestamp);

        void StartMove(PointF p);

        void StopMove();

        void Move(float dx, float dy, Keys modifiers);

        void ExportVideo(IDrawingHostView host);

        void ExportImage(IDrawingHostView host);

        void WriteData(XmlWriter w);

        void ReadData(XmlReader r);
        #endregion
    }
}
