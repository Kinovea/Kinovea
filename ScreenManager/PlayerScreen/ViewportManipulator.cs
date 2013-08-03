#region License
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
        public Size RenderingSize {
            get { return m_renderingSize;}
        }
        public Point RenderingLocation {
            get { return m_renderingLocation;}
        }
        public double Stretch {
            get { return m_stretchFactor; }
        }
        public Size DecodingSize {
            get { return m_decodingSize;}
        }
        public bool MayDrawUnscaled {
            get { return m_mayDrawUnscaled;}
        }
        public double RenderingZoomFactor {
            get { return m_renderingZoomFactor; }
        }
        #endregion
        
        #region Members
        private Size m_renderingSize;               // Size of the surface we are going to draw on.
        private Point m_renderingLocation;          // Location of the surface we are going to draw on relatively to the container.
        private Size m_decodingSize;                // Size at which we will ask the VideoReader to provide its frames. (Not necessarily honored by the reader).
        private double m_stretchFactor = 1.0;       // input/output. May be updated during the computation.
        private bool m_mayDrawUnscaled;
        private double m_renderingZoomFactor = 1.0; // factor to apply on the reference size to get the rendering size x zoom.
                                                    // It will be used to locate the region of interest.
        // Reference data.
        private VideoReader m_reader;
        #endregion
        
        public void Initialize(VideoReader _videoReader)
        {
            m_reader = _videoReader;
        }
        
        public void Manipulate(Size _containerSize, double _stretchFactor, bool _fillContainer, double _zoomFactor, bool _enableCustomDecodingSize, bool _scalable)
        {
            // One of the constraint has changed, recompute the sizes.
            Size aspectRatioSize = m_reader.Info.AspectRatioSize;
            
            ComputeRenderingSize(aspectRatioSize, _containerSize, _stretchFactor, _fillContainer);
            ComputeDecodingSize(aspectRatioSize, _containerSize, _zoomFactor, _enableCustomDecodingSize, _scalable);
        }
        
        private void ComputeRenderingSize(Size _aspectRatioSize, Size _containerSize, double _stretchFactor, bool _fillContainer)
        {
            // Updates the following globals: m_stretchFactor, m_renderingSize, m_renderingLocation.
            
            //log.DebugFormat("Input: zoom:{0:0.00}, stretch:{1:0.00}, container:{2}, fill:{3}, aspectRatioSize:{4}",
            //               _zoomFactor, _stretchFactor, _containerSize, _fillContainer, aspectRatioSize);
            
            m_stretchFactor = _stretchFactor;
            
            Size stretchedSize = new Size((int)(_aspectRatioSize.Width * m_stretchFactor), (int)(_aspectRatioSize.Height * m_stretchFactor));
            if(!stretchedSize.FitsIn(_containerSize) || _fillContainer)
            {
                // What factor must be applied so that it fits in the container. 
                // If the image does not fit, the factor for at least one side is strictly less than 1.0f.
                double stretchWidth = (double)_containerSize.Width / _aspectRatioSize.Width;
                double stretchHeight = (double)_containerSize.Height / _aspectRatioSize.Height;
                
                // Stretch using the smaller factor, so that the other side fits in.
                if(stretchWidth < stretchHeight)
                {
                    m_stretchFactor = stretchWidth;
                    m_renderingSize = new Size(_containerSize.Width, (int)(_aspectRatioSize.Height * m_stretchFactor));
                }
                else
                {
                    m_stretchFactor = stretchHeight;
                    m_renderingSize = new Size((int)(_aspectRatioSize.Width * m_stretchFactor), _containerSize.Height);
                }
            }
            else
            {
                m_renderingSize = stretchedSize;
            }
            
            m_renderingLocation = new Point((_containerSize.Width - m_renderingSize.Width) / 2, (_containerSize.Height - m_renderingSize.Height) / 2);
        }
        private void ComputeDecodingSize(Size _aspectRatioSize, Size _containerSize, double _zoomFactor, bool _enableCustomDecodingSize, bool _scalable)
        {
            // Updates the following globals: m_decodingSize, m_mayDrawUnscaled, m_renderingZoomFactor.
            
            if(!_enableCustomDecodingSize)
            {
                m_decodingSize = _aspectRatioSize;
                m_mayDrawUnscaled = false;
            }
            else
            {
                // Zoom does not impact rendering viewport size, but it does impact decoding size.
                // The policy is to never ask the VideoReader to decode at a size larger than the container.
                // The intersting thing here is that it is possible for the zoom to be compensated by the forced squeeze,
                // for example when we zoom into a video that is too big for the viewport.
                if(_zoomFactor == 1.0)
                {
                    m_decodingSize = m_renderingSize;
                    m_mayDrawUnscaled = true;
                }
                else
                {
                    // scaledSize is the size at which we would need to decode the image if we wanted to be able
                    // to draw the region of interest (zoom sub window) directly on the viewport without additional scaling.
                    double scaleFactor = _zoomFactor * m_stretchFactor;
                    Size scaledSize = new Size((int)(m_renderingSize.Width * _zoomFactor), (int)(m_renderingSize.Height * _zoomFactor));
                    
                    // We don't actually care if the scaled image fits in the container, but we use the container size as the
                    // upper boundary to what we allow the decoding size to be, in order not to put too much load on the decoder.
                    if(!_scalable && !scaledSize.FitsIn(_containerSize) && !scaledSize.FitsIn(_aspectRatioSize))
                    {
                        // Here we could also use _containerSize at right ratio. Will have to test perfs to know which is better.
                        // If in this branch, we cannot draw unscaled.
                        m_decodingSize = _aspectRatioSize;
                        m_mayDrawUnscaled = false;
                        //log.DebugFormat("Will not decode at size larger than container or aspectRatioSize. {0}", scaledSize);
                    }
                    else
                    {
                        m_decodingSize = scaledSize;
                        m_mayDrawUnscaled = true;
                        Size zoomSize = new Size((int)(scaledSize.Width / _zoomFactor), (int)(scaledSize.Height / _zoomFactor));
                        //log.DebugFormat("Zoom will fit. Zoom window size should be:{0}, decoding size (scaled size):{1}, decoding zoom factor:{2}",
                        //                zoomSize, scaledSize, m_renderingZoomFactor);
                    }
                }
            }
            
            m_renderingZoomFactor = (double)m_decodingSize.Width / _aspectRatioSize.Width;
            
            //log.DebugFormat("Output: stretch:{0:0.00}, renderingSize:{1}, scaleFactor:{2:0.00}, decodingSize:{3}, mayDrawUnscaled:{4}", 
            //                m_stretchFactor, m_renderingSize, _zoomFactor * m_stretchFactor, m_decodingSize, m_mayDrawUnscaled);
        }
    }
}
