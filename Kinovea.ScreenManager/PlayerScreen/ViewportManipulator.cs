#region License
/*
Copyright © Joan Charmant 2012.
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
using Kinovea.Video;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Compute the presentation window size and location, and the stretch factor compared to the reference.
    /// </summary>
    public class ViewportManipulator
    {
        #region Properties

        /// <summary>
        /// Size of the rendering surface inside the viewport.
        /// </summary>
        public Size RenderingSize
        {
            get { return renderingSize; }
        }

        /// <summary>
        /// Location of the rendering surface inside the viewport.
        /// </summary>
        public Point RenderingLocation
        {
            get { return renderingLocation; }
        }

        /// <summary>
        /// Final stretch factor going from reference to presentation size.
        /// </summary>
        public double Stretch
        {
            get { return stretchFactor; }
        }
        #endregion

        #region Members
        private Size renderingSize;               
        private Point renderingLocation;

        // Asked stretch factor.
        // Will be updated during the computation if it's too large to fit.
        // This is the factor applied to the reference size in order to make it fit in the drawing surface.
        private double stretchFactor = 1.0;       
        private VideoReader reader;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public void Initialize(FrameServerPlayer frameServer)
        {
            this.reader = frameServer.VideoReader;
        }

        /// <summary>
        /// Compute the presentation window size and location, and stretch factor.
        /// This should be refactored when we switch to full viewport zooming.
        /// </summary>
        public void Manipulate(bool rotatedCanvas, Size _containerSize, double _stretchFactor, bool _fillContainer)
        {
            // Note: the reference size already takes image rotation into account.
            // rotatedCanvas is a different thing and was meant for Kinogram but is not used right now.
            Size referenceSize = reader.Geometry.ReferenceSize;
            stretchFactor = _stretchFactor;
            Size stretchedSize = new Size((int)(referenceSize.Width * stretchFactor), (int)(referenceSize.Height * stretchFactor));

            if (rotatedCanvas)
            {
                referenceSize = new Size(referenceSize.Height, referenceSize.Width);
                stretchedSize = new Size(stretchedSize.Height, stretchedSize.Width);
            }

            if (!stretchedSize.FitsIn(_containerSize) || _fillContainer)
            {
                // Ratio stretch based on the reference size.
                renderingSize = FitHelper.Fit(stretchedSize, _containerSize, true);
                stretchFactor = (double)renderingSize.Width / referenceSize.Width;
            }
            else
            {
                renderingSize = stretchedSize;
            }

            // Center the window in the container.
            renderingLocation = new Point(
                (_containerSize.Width - renderingSize.Width) / 2, 
                (_containerSize.Height - renderingSize.Height) / 2);
        }
    }
}
