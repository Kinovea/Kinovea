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
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    public class MetadataManipulator
    {
        public bool IsUsingHandTool
        {
            get { return toolManager == null ? true : toolManager.IsUsingHandTool;}
        }
        
        private Metadata metadata;
        private ScreenToolManager toolManager;
        
        public MetadataManipulator(Metadata metadata, ScreenToolManager toolManager)
        {
            this.metadata = metadata;
            this.toolManager = toolManager;
        }
    
    
        public bool OnMouseLeftDown(Point mouse, Point imageLocation, float imageZoom)
        {
            if(metadata == null || toolManager == null)
                return false;
                
            // At this point we must know the current timestamp and metadata should be valid.
            // TODO: Handle magnifier.
            // TODO: see if this could handle whole image manipulation as well, but at the moment the resizers are stored in the viewport.
            
            bool handled = false;
            ImageToViewportTransformer transformer = new ImageToViewportTransformer(imageLocation, imageZoom);
            Point imagePoint = transformer.Untransform(mouse);
            
            metadata.AllDrawingTextToNormalMode();
            
            if(toolManager.IsUsingHandTool)
            {
                // TODO: Change cursor.
                handled = toolManager.HandTool.OnMouseDown(metadata, 0, imagePoint, 0, false);
            }
            else
            {
                handled = true;
                //CreateNewDrawing();
            }
            
            return handled;
        }
        
        public bool OnMouseLeftMove(Point mouse, Keys modifiers, Point imageLocation, float imageZoom)
        {
            if(metadata == null || toolManager == null)
                return false;
            
            bool handled = false;
            ImageToViewportTransformer transformer = new ImageToViewportTransformer(imageLocation, imageZoom);
            Point imagePoint = transformer.Untransform(mouse);
            
            if(toolManager.IsUsingHandTool)
            {
                // TODO: handle magnifier.
                handled = toolManager.HandTool.OnMouseMove(metadata, imagePoint, Point.Empty, modifiers);
            }
            else
            {
                
            }
            
            return handled;
        }
        
        public void OnMouseUp()
        {
            // TODO: Handle magnifier.
            // TODO: Memorize the action we just finished to enable undo.
            // TODO: keep tool or change tool.
            // m_ActiveTool = m_ActiveTool.KeepTool ? m_ActiveTool : m_PointerTool;
            
            if(toolManager.IsUsingHandTool)
            {
                toolManager.HandTool.OnMouseUp();
                
                // Force render if drawing is SVG.
            }
        }
        
        public Cursor GetCursor(float scale)
        {
            return toolManager.GetCursor(scale);
        }
        
        private void CreateNewDrawing()
        {
        
        }
        
        
        
    }
}
