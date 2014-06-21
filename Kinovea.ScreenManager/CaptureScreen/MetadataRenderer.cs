﻿#region License
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
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Kinovea.ScreenManager
{
    public class MetadataRenderer
    {
        private Metadata metadata;
    
        public MetadataRenderer(Metadata metadata)
        {
            this.metadata = metadata;
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

            foreach (AbstractDrawing ad in metadata.ChronoManager.Drawings)
                ad.Draw(canvas, distorter, transformer, false, timestamp);

            foreach (AbstractDrawing ad in metadata.TrackManager.Drawings)
                ad.Draw(canvas, distorter, transformer, false, timestamp);
            
            foreach (AbstractDrawing ad in metadata.ExtraDrawings)
                ad.Draw(canvas, distorter, transformer, false, timestamp);
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
