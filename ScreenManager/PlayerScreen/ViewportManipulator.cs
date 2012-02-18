#region License
/*
Copyright © Joan Charmant 2011. joan.charmant@gmail.com 
Licensed under the MIT license: http://www.opensource.org/licenses/mit-license.php
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
        /*public Size ViewportSize;        // Size of the surface we are going to draw on.
        private Point m_ViewportLocation;   // Location of the surface we are going to draw on relatively to the container.
        private Size m_DecodingSize;        // Size at which we will ask the VideoReader to provide its frames. (Not necessarily honored by the reader).
        private bool m_CanDrawUnscaled;*/
        
        public Size RenderingSize {
            get { return m_renderingSize;}
        }
        public Point RenderingLocation {
            get { return m_renderingLocation;}
        }
        public double Stretch {
            get { return m_stretchFactor; }
        }
        public bool FillContainer {
            get { return m_fillContainer; }
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
        
        #region Members
        private Size m_renderingSize;               // Size of the surface we are going to draw on.
        private Point m_renderingLocation;          // Location of the surface we are going to draw on relatively to the container.
        private Size m_decodingSize;                // Size at which we will ask the VideoReader to provide its frames. (Not necessarily honored by the reader).
        private double m_stretchFactor = 1.0;       // input/output. May be updated during the computation.
        private bool m_fillContainer;               // input/output. May be updated during the computation.
        private bool m_mayDrawUnscaled;
        private double m_renderingZoomFactor = 1.0; // factor to apply on the reference size to get the rendering size x zoom.
                                                    // It will be used to locate the region of interest.
        // Reference data.
        private VideoReader m_reader;
        //private Size m_aspectRatioSize;         // Size of images in the original video, after the aspect ratio has been applied.
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public void Initialize(VideoReader _videoReader)
        {
            m_reader = _videoReader;
            
            //m_aspectRatioSize = _aspectRatioSize;
        }
        
        public void Manipulate(double _zoomFactor, double _stretchFactor, Size _containerSize, bool _fillContainer)
        {
            // One of the constraint has changed, recompute the sizes.
            Size aspectRatioSize = m_reader.Info.AspectRatioSize;
            
            log.DebugFormat("Input: zoom:{0:0.00}, stretch:{1:0.00}, container:{2}, fill:{3}, aspectRatioSize:{4}",
                           _zoomFactor, _stretchFactor, _containerSize, _fillContainer, aspectRatioSize);
            
            m_stretchFactor = _stretchFactor;
            m_fillContainer = _fillContainer;
            m_renderingZoomFactor = 1.0f;
            
            Size stretchedSize = new Size((int)(aspectRatioSize.Width * m_stretchFactor), (int)(aspectRatioSize.Height * m_stretchFactor));
            if(!stretchedSize.FitsIn(_containerSize) || m_fillContainer)
            {
                m_fillContainer = true;
                
                // What factor must be applied so that it fits in the container. 
                // If the image does not fit, the factor for at least one side is strictly less than 1.0f.
                double stretchWidth = (double)_containerSize.Width / aspectRatioSize.Width;
                double stretchHeight = (double)_containerSize.Height / aspectRatioSize.Height;
                
                // Stretch using the smaller factor, so that the other side fits in.
                if(stretchWidth < stretchHeight)
                {
                    m_stretchFactor = stretchWidth;
                    m_renderingSize = new Size(_containerSize.Width, (int)(aspectRatioSize.Height * m_stretchFactor));
                }
                else
                {
                    m_stretchFactor = stretchHeight;
                    m_renderingSize = new Size((int)(aspectRatioSize.Width * m_stretchFactor), _containerSize.Height);
                }
            }
            else
            {
                m_renderingSize = stretchedSize;
            }
            
            m_renderingLocation = new Point((_containerSize.Width - m_renderingSize.Width) / 2, (_containerSize.Height - m_renderingSize.Height) / 2);
            
            
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
                //Size scaledSize2 = new Size((int)(aspectRatioSize.Width * scaleFactor), (int)(aspectRatioSize.Height * scaleFactor));
                Size scaledSize = new Size((int)(m_renderingSize.Width * _zoomFactor), (int)(m_renderingSize.Height * _zoomFactor));
                
                // 
                
                
                //if(scaledSize != scaledSize2)
                    //log.DebugFormat("aspectRatio x zoom x stretch:{0}, rendering x zoom:{1}", scaledSize2, scaledSize);
                
                // We don't actually care if the scaled image fits in the container, but we use the container size as the
                // upper boundary to what we allow the decoding size to be, in order not to put too much load on the decoder.
                if(!scaledSize.FitsIn(_containerSize) && !scaledSize.FitsIn(aspectRatioSize))
                {
                    // Here we could also use _containerSize at right ratio. Will have to test perfs to know which is better.
                    // If in this branch, we cannot draw unscaled.
                    m_decodingSize = aspectRatioSize;
                    m_mayDrawUnscaled = false;
                    m_renderingZoomFactor = 1.0f;
                }
                else
                {
                    m_decodingSize = scaledSize;
                    m_mayDrawUnscaled = true;
                    m_renderingZoomFactor = (double)scaledSize.Width / aspectRatioSize.Width;
                    
                    //Size zoomSize = new Size((int)(aspectRatioSize.Width / _zoomFactor), (int)(aspectRatioSize.Height / _zoomFactor));
                    Size zoomSize = new Size((int)(scaledSize.Width / _zoomFactor), (int)(scaledSize.Height / _zoomFactor));
                    
                    
                    log.DebugFormat("Zoom will fit. Zoom window size should be:{0}, decoding size (scaled size):{1}, decoding zoom factor:{2}",
                                    zoomSize, scaledSize, m_renderingZoomFactor);
                }
            }
            
            log.DebugFormat("Output: stretch:{0:0.00}, fill:{1}, renderingSize:{2}, scaleFactor:{3:0.00}, decodingSize:{4}, mayDrawUnscaled:{5}", 
                            m_stretchFactor, m_fillContainer, m_renderingSize, _zoomFactor * m_stretchFactor, m_decodingSize, m_mayDrawUnscaled);
        }
    }
}
