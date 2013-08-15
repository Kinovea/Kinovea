#region License
/*
Copyright © Joan Charmant 2009.
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// VideoFilterMosaic.
    /// - Input        : all images.
    /// - Output       : One image, same size.
    /// - Operation    : Compose a sample of the images in a grid view.
    /// - Type         : Interactive.
    /// - Previewable  : No.
    /// </summary>
    public class VideoFilterMosaic : AbstractVideoFilter
    {
        #region Properties
        public override string Name {
            get { return ScreenManagerLang.VideoFilterMosaic_FriendlyName; }
        }
        public override Bitmap Icon {
            get { return Properties.Resources.mosaic; }
        }
        #endregion
        
        #region AbstractVideoFilter Implementation
        public override void Activate(IWorkingZoneFramesContainer _framesContainer, Action<InteractiveEffect> _setInteractiveEffect)
        {
            InteractiveEffect effect = new InteractiveEffect();
            
            // Usage of closures to capture internal state for the effect.
            // The Parameter object will be shared between the delegates, but scoped to this InteractiveEffect instance.
            Parameters p = new Parameters();
            effect.Draw = (canvas, frames) => Draw(canvas, frames, p);
            effect.MouseWheel = (scroll) => MouseWheel(scroll, p);
            
            _setInteractiveEffect(effect);
        }
        protected override void Process(object sender, DoWorkEventArgs e)
        {
        }
        #endregion
        
        #region Interactive Effect methods
        private void Draw(Graphics _canvas, IWorkingZoneFramesContainer _framesContainer, Parameters _parameters)
        {
            if(_parameters == null || _framesContainer == null || _framesContainer.Frames == null || _framesContainer.Frames.Count < 1)
                return;
            
            List<Bitmap> selectedFrames = GetImages(_framesContainer, _parameters.Spots);	
            
            if(selectedFrames == null || selectedFrames.Count < 1)
                return;
            
            _canvas.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            _canvas.CompositingQuality = CompositingQuality.HighSpeed;
            _canvas.InterpolationMode = InterpolationMode.Bilinear;
            _canvas.SmoothingMode = SmoothingMode.HighQuality;
            
            // We reserve n² placeholders, so we have exactly as many images on width than on height.
            // Example: 32 images as input -> 6x6 images with the last 4 not filled.
            // + each image must be scaled down by a factor of 1/6.
            
            int n = (int)Math.Sqrt(_parameters.Spots);
            int thumbWidth = (int)_canvas.VisibleClipBounds.Width / n;
            int thumbHeight = (int)_canvas.VisibleClipBounds.Height / n;
                        
      Rectangle rSrc = new Rectangle(0, 0, selectedFrames[0].Width, selectedFrames[0].Height);
      Font f = new Font("Arial", GetFontSize(thumbWidth), FontStyle.Bold);
            
            for(int i=0;i<n;i++)
            {
                for(int j=0;j<n;j++)
                {
                    int iImageIndex = j*n + i;
                    if(iImageIndex >= selectedFrames.Count || selectedFrames[iImageIndex] == null)
                        continue;
                    
                    Rectangle rDst = new Rectangle(i*thumbWidth, j*thumbHeight, thumbWidth, thumbHeight);
                    
                    _canvas.DrawImage(selectedFrames[iImageIndex], rDst, rSrc, GraphicsUnit.Pixel);
                    DrawImageNumber(_canvas, iImageIndex, rDst, f);
                }
            }
            
            f.Dispose();
        }
        private void MouseWheel(int _scroll, Parameters _parameters)
        {
            // Change the number of frames to use for the composite.
            int n = (int)Math.Sqrt((double)_parameters.Spots);
            n = _scroll > 0 ? Math.Min(10, n + 1) : Math.Max(2, n - 1);
            _parameters.Spots = n*n;
        }
        #endregion
        
        #region Private methods
        private List<Bitmap> GetImages( IWorkingZoneFramesContainer _framesContainer, int _spots)
        {
            float step = (float)_framesContainer.Frames.Count / _spots;
            List<Bitmap> bitmaps = _framesContainer.Frames.Select(f => f.Image).ToList();
            return bitmaps.Where((bmp, i) => i % step < 1).ToList();
        }
        private int GetFontSize(int width)
        {
            // Return the font size for the image number based on the thumb width.
            int fontSize = 10;

            if(width >= 200)
                fontSize = 18;
            else if(width >= 150)
                fontSize = 14;
            
            return fontSize;
        }
        private void DrawImageNumber(Graphics _canvas, int _index, Rectangle _rDst, Font _font)
        {
            string number = String.Format(" {0}", _index + 1);
            SizeF bgSize = _canvas.MeasureString(number, _font);
            bgSize = new SizeF(bgSize.Width + 6, bgSize.Height + 2);
            
            // 1. Draw background.
            GraphicsPath gp = new GraphicsPath();
            gp.StartFigure();
            gp.AddLine(_rDst.Left, _rDst.Top, _rDst.Left + bgSize.Width, _rDst.Top);
            gp.AddLine(_rDst.Left + bgSize.Width, _rDst.Top, _rDst.Left + bgSize.Width, _rDst.Top + (bgSize.Height / 2));
            gp.AddArc(_rDst.Left, _rDst.Top, bgSize.Width, bgSize.Height, 0, 90);
            gp.AddLine(_rDst.Left + (bgSize.Width/2), _rDst.Top + bgSize.Height, _rDst.Left, _rDst.Top + bgSize.Height);
            gp.CloseFigure();
            _canvas.FillPath(Brushes.Black, gp);
            gp.Dispose();
            
            // 2. Draw image number.
            _canvas.DrawString(number, _font, Brushes.White, _rDst.Location);
        }
        #endregion
        
        private class Parameters
        {
            public int Spots {
                get {return m_Spots; }
                set {m_Spots = value;}
            }
            private int m_Spots = 16;
        }
    }
}


