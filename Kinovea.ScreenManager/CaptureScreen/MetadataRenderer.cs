#region License
/*
Copyright © Joan Charmant 2013.
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
using System.Drawing.Drawing2D;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Renderer for metadata.
    /// This renderer is currently only used by the capture screen and by the trajectory configuration window.
    /// The playback screen uses a different mechanism that hasn't been factored in yet.
    /// </summary>
    public class MetadataRenderer
    {
        private Metadata metadata;
        private bool renderTimedDrawings = true;
    
        public MetadataRenderer(Metadata metadata, bool renderTimedDrawings)
        {
            this.metadata = metadata;
            this.renderTimedDrawings = renderTimedDrawings;
        }
    
        public void Render(Graphics viewportCanvas, Point imageLocation, float imageZoom, long timestamp)
        {
            if(metadata == null)
                return;
            
            ImageToViewportTransformer transformer = new ImageToViewportTransformer(imageLocation, imageZoom);
            
            viewportCanvas.SmoothingMode = SmoothingMode.AntiAlias;
            RenderExtraDrawings(metadata, timestamp, viewportCanvas, transformer);
            RenderDrawings(metadata, timestamp, viewportCanvas, transformer);
            //RenderMagnifier();
        }
        
        private void RenderExtraDrawings(Metadata metadata, long timestamp, Graphics canvas, ImageToViewportTransformer transformer)
        {
            DistortionHelper distorter = null;

            metadata.DrawingCoordinateSystem.Draw(canvas, distorter, transformer, false, timestamp);
            metadata.DrawingTestGrid.Draw(canvas, distorter, transformer, false, timestamp);

            if (renderTimedDrawings)
            {
                metadata.SpotlightManager.Draw(canvas, distorter, transformer, false, timestamp);
                metadata.AutoNumberManager.Draw(canvas, distorter, transformer, false, timestamp);
            }

            if (renderTimedDrawings)
            {
                foreach (AbstractDrawing ad in metadata.ChronoManager.Drawings)
                    ad.Draw(canvas, distorter, transformer, false, timestamp);
            }

            if (renderTimedDrawings)
            {
                foreach (AbstractDrawing ad in metadata.TrackManager.Drawings)
                    ad.Draw(canvas, distorter, transformer, false, timestamp);
            }


        }

        private void RenderDrawings(Metadata metadata, long timestamp, Graphics canvas, ImageToViewportTransformer transformer)
        {
            DistortionHelper distorter = metadata.CalibrationHelper.DistortionHelper;

            foreach (Keyframe keyframe in metadata.Keyframes)
                foreach (AbstractDrawing drawing in keyframe.Drawings)
                    drawing.Draw(canvas, distorter, transformer, drawing == metadata.HitDrawing, timestamp);
        }
    }
}
