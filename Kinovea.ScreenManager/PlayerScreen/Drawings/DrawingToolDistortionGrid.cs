#region License
/*
Copyright © Joan Charmant 2014.
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
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A 3D grid.
    /// </summary>
    public class DrawingToolDistortionGrid : AbstractDrawingTool
    {
        #region Properties
        public override string DisplayName
        {
            get { return "Distortion grid"; }
        }
        public override Bitmap Icon
        {
            get { return Properties.Drawings.plane; }
        }
        public override bool Attached
        {
            get { return true; }
        }
        public override bool KeepTool
        {
            get { return false; }
        }
        public override bool KeepToolFrameChanged
        {
            get { return false; }
        }
        public override DrawingStyle StylePreset
        {
            get { return stylePreset; }
            set { stylePreset = value; }
        }
        public override DrawingStyle DefaultStylePreset
        {
            get { return defaultStylePreset; }
        }
        #endregion

        #region Members
        private DrawingStyle defaultStylePreset = new DrawingStyle();
        private DrawingStyle stylePreset;
        #endregion

        public DrawingToolDistortionGrid()
        {
            defaultStylePreset.Elements.Add("color", new StyleElementColor(Color.DarkRed));
            stylePreset = defaultStylePreset.Clone();
        }

        #region Public Methods
        public override AbstractDrawing GetNewDrawing(Point origin, long timestamp, long averageTimeStampsPerFrame, IImageToViewportTransformer transformer)
        {
            return new DrawingDistortionGrid(timestamp, averageTimeStampsPerFrame, stylePreset);
        }
        public override Cursor GetCursor(double stretchFactor)
        {
            return Cursors.Cross;
        }
        #endregion
    }
}
