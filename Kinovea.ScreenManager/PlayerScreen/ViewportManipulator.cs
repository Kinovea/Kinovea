﻿#region License
/*
Copyright © Joan Charmant 2012.
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
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Compute the final decoding and rendering sizes, based on original size, aspect ratio, zoom factor, stretching and container.
    /// </summary>
    public class ViewportManipulator
    {
        #region Properties
        public Size RenderingSize
        {
            get { return renderingSize; }
        }
        public Point RenderingLocation
        {
            get { return renderingLocation; }
        }
        public double Stretch
        {
            get { return stretchFactor; }
        }
        public Size DecodingSize
        {
            get { return decodingSize; }
        }
        public bool MayDrawUnscaled
        {
            get { return mayDrawUnscaled; }
        }
        public double RenderingZoomFactor
        {
            get { return renderingZoomFactor; }
        }
        #endregion

        #region Members
        private Size renderingSize;               // Size of the drawing surface.
        private Point renderingLocation;          // Location of the drawing surface relatively to the viewport container.
        private double stretchFactor = 1.0;       // Asked stretch factor. May be updated during the computation if it's too large to fit.
                                                  // This is the factor applied to the reference size in order to make it fit in the drawing surface.
        private Size decodingSize;                // Size at which we will ask the VideoReader to provide its frames, before rotation. (Not necessarily honored by the reader).
        private bool mayDrawUnscaled;
        private double renderingZoomFactor = 1.0; // Factor to apply to the reference zoom window to get the final window in images received by the reader. Also see below.

        // Reference data.
        private VideoReader reader;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public void Initialize(VideoReader reader)
        {
            this.reader = reader;
        }

        /// <summary>
        /// Compute decoding size, rendering size and adjust the stretch factor, based on:
        /// - original size, aspectratio size, reference size, 
        /// - container size, zoom factor, stretch factor.
        /// </summary>
        public void Manipulate(Size _containerSize, double _stretchFactor, bool _fillContainer, double _zoomFactor, bool _enableCustomDecodingSize, bool _scalable)
        {
            // One of the constraint has changed, recompute the sizes.
            bool sideway = reader.Info.ImageRotation == ImageRotation.Rotate90 || reader.Info.ImageRotation == ImageRotation.Rotate270;
            ComputeRenderingSize(sideway, _containerSize, _stretchFactor, _fillContainer);

            ComputeDecodingSize(sideway, _containerSize, _zoomFactor, _enableCustomDecodingSize, _scalable);

            // Rendering zoom factor
            // Used to find the final zoom window in the received images.
            // It is the factor to apply to the zoom window in the reference image.
            // Subtle: this is not the same as rendering size / reference size, because when there is zoom and stretch we can 
            // decide to decode at a bigger size than the rendering size in order to get a better picture, as long as it stays under 
            // the original image size. Thus we need to use the actual decoding size, and we need to compare it to the 
            // unrotated reference size, aka aspect ratio size, since the decoding size is before rotation.
            renderingZoomFactor = (double)decodingSize.Width / reader.Info.AspectRatioSize.Width;
        }

        /// <summary>
        /// Compute the rendering size, rendering location, and update the stretch factor if the original one doesn't fit in the container.
        /// The stretch factor is how much the user want to stretch the image and is based on image corner manipulation. It can go either way of 1.0f.
        /// </summary>
        private void ComputeRenderingSize(bool sideway, Size _containerSize, double _stretchFactor, bool _fillContainer)
        {
            Size referenceSize = reader.Info.ReferenceSize;
            stretchFactor = _stretchFactor;
            Size stretchedSize = new Size((int)(referenceSize.Width * stretchFactor), (int)(referenceSize.Height * stretchFactor));

            if (!stretchedSize.FitsIn(_containerSize) || _fillContainer)
            {
                // Ratio stretch based on the reference size.
                float ratioWidth = (float)_containerSize.Width / referenceSize.Width;
                float ratioHeight = (float)_containerSize.Height / referenceSize.Height;

                if (ratioWidth < ratioHeight)
                    renderingSize = new Size(_containerSize.Width, (int)(referenceSize.Height * ratioWidth));
                else
                    renderingSize = new Size((int)(referenceSize.Width * ratioHeight), _containerSize.Height);

                stretchFactor = Math.Min(ratioWidth, ratioHeight);
            }
            else
            {
                renderingSize = stretchedSize;
            }

            renderingLocation = new Point((_containerSize.Width - renderingSize.Width) / 2, (_containerSize.Height - renderingSize.Height) / 2);

            //log.DebugFormat("ComputeRenderingSize. sideways:{0}, container:{1}, stretch:{2:0.00} -> {3:0.00}, fill:{4}, reference:{5}, rendering:{6}", 
            //  sideways, _containerSize, _stretchFactor, stretchFactor, _fillContainer, referenceSize, renderingSize);
        }

        /// <summary>
        /// Compute the decoding size.
        /// </summary>
        private void ComputeDecodingSize(bool sideway, Size _containerSize, double _zoomFactor, bool _enableCustomDecodingSize, bool _scalable)
        {
            // Updates the following globals: decodingSize, mayDrawUnscaled, renderingZoomFactor.
            // Note: the decoding size doesn't care about rotation, as the pipeline is read > scale > rotate.
            Size aspectRatioSize = reader.Info.AspectRatioSize;
            Size unrotatedRenderingSize = renderingSize;
            if (sideway)
                unrotatedRenderingSize = new Size(renderingSize.Height, renderingSize.Width);

            if (!_enableCustomDecodingSize)
            {
                decodingSize = aspectRatioSize;
                mayDrawUnscaled = false;
            }
            else
            {
                // Zoom does not impact rendering viewport size, but it does impact decoding size.
                // The policy is to never ask the VideoReader to decode at a size larger than the container.
                // One interesting thing here is that it is possible for the zoom to be compensated by the forced squeeze,
                // for example when we zoom into a video that is too big for the viewport.
                if (_zoomFactor == 1.0)
                {
                    decodingSize = unrotatedRenderingSize;
                    mayDrawUnscaled = !sideway;
                }
                else
                {
                    // zoomDecodingSize is the size at which we would need to decode the image if we wanted to be able
                    // to draw the region of interest (zoom subwindow) directly on the viewport.
                    Size zoomDecodingSize = new Size((int)(unrotatedRenderingSize.Width * _zoomFactor), (int)(unrotatedRenderingSize.Height * _zoomFactor));
                    Size zoomDecodingSizeRotated = sideway ? new Size(zoomDecodingSize.Height, zoomDecodingSize.Width) : zoomDecodingSize;

                    // We don't actually care if the scaled image fits in the container as we are not rendering it fully anyway, 
                    // but we use the container size as an upper boundary to what we allow the decoding size to be, 
                    // in order not to put too much load on the decoder.
                    if (!_scalable && !zoomDecodingSizeRotated.FitsIn(_containerSize) && !zoomDecodingSize.FitsIn(aspectRatioSize))
                    {
                        // The max allowed decoding size is somewhat arbitrary, we could also use _containerSize at right ratio.
                        decodingSize = aspectRatioSize;
                        mayDrawUnscaled = false;
                    }
                    else
                    {
                        decodingSize = zoomDecodingSize;
                        mayDrawUnscaled = !sideway;
                    }
                }
            }

            //log.DebugFormat("ComputeDecodingSize: sideways:{0}, container:{1}, zoom:{2:0.00}, renderingSize:{3}, decodingSize:{4}", 
            //    sideways, _containerSize, _zoomFactor, renderingSize, decodingSize);
        }
    }
}
