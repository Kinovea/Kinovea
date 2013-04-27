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
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Kinovea.ScreenManager
{
    public class KVARenderer
    {
        public static void Render(Metadata metadata, long timestamp, Graphics canvas, Point location, float zoom)
        {
            if(metadata == null)
                return;
            
            ImageToViewportTransformer transformer = new ImageToViewportTransformer(zoom, location);
            
            canvas.SmoothingMode = SmoothingMode.AntiAlias;
            RenderExtraDrawings(metadata, timestamp, canvas, transformer);
            RenderDrawings(metadata, timestamp, canvas, transformer);
            //RenderMagnifier();
        }
        
        private static void RenderExtraDrawings(Metadata metadata, long timestamp, Graphics canvas, ImageToViewportTransformer transformer)
        {
            foreach(AbstractDrawing ad in metadata.ExtraDrawings)
                ad.Draw(canvas, transformer, false, timestamp);
        }
        
        private static void RenderDrawings(Metadata metadata, long timestamp, Graphics canvas, ImageToViewportTransformer transformer)
        {
            foreach(Keyframe keyframe in metadata.Keyframes)
                for (int i = keyframe.Drawings.Count - 1; i >= 0; i--)
                    keyframe.Drawings[i].Draw(canvas, transformer, i == metadata.SelectedDrawing, timestamp);
        }
    }
}
