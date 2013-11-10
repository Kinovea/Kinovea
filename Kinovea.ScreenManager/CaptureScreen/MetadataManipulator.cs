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
        #region Events
        public event EventHandler<DrawingEventArgs> LabelAdded;
        #endregion
    
        #region Properties
        public bool IsUsingHandTool
        {
            get { return screenToolManager == null ? true : screenToolManager.IsUsingHandTool;}
        }
        
        public AbstractDrawing HitDrawing
        {
            get { return metadata.HitDrawing;}
        }
        
        #endregion
        
        #region Members
        private Metadata metadata;
        private ScreenToolManager screenToolManager;
        private long fixedTimestamp;
        private int fixedKeyframe;
        #endregion
        
        public MetadataManipulator(Metadata metadata, ScreenToolManager screenToolManager)
        {
            this.metadata = metadata;
            this.screenToolManager = screenToolManager;
        }
        
        public void SetFixedTimestamp(long timestamp)
        {
            this.fixedTimestamp = timestamp;
        }

        public void SetFixedKeyframe(int index)
        {
            this.fixedKeyframe = index;
        }

        public bool OnMouseLeftDown(Point mouse, Point imageLocation, float imageZoom)
        {
            if(metadata == null || screenToolManager == null)
                return false;
                
            // At this point we must know the current timestamp and metadata should be valid.
            // TODO: Handle magnifier.
            // TODO: see if this could handle whole image manipulation as well, but at the moment the resizers are stored in the viewport.
            
            bool handled = false;
            ImageToViewportTransformer transformer = new ImageToViewportTransformer(imageLocation, imageZoom);
            PointF imagePoint = transformer.Untransform(mouse);
            
            metadata.AllDrawingTextToNormalMode();
            
            if(screenToolManager.IsUsingHandTool)
            {
                // TODO: Change cursor.
                handled = screenToolManager.HandTool.OnMouseDown(metadata, fixedKeyframe, imagePoint, fixedTimestamp, false);
            }
            else
            {
                handled = true;
                CreateNewDrawing(imagePoint.ToPoint(), transformer);
            }
            
            return handled;
        }
        
        public bool OnMouseLeftMove(Point mouse, Keys modifiers, Point imageLocation, float imageZoom)
        {
            if(metadata == null || screenToolManager == null)
                return false;
            
            bool handled = false;
            ImageToViewportTransformer transformer = new ImageToViewportTransformer(imageLocation, imageZoom);
            PointF imagePoint = transformer.Untransform(mouse);
            
            if(screenToolManager.IsUsingHandTool)
            {
                // TODO: handle magnifier.
                handled = screenToolManager.HandTool.OnMouseMove(metadata, imagePoint, Point.Empty, modifiers);
            }
            else
            {
                // Setting second point of a drawing.
                IInitializable drawing = metadata.HitDrawing as IInitializable;
                if(drawing != null)
                    drawing.ContinueSetup(imagePoint.ToPoint(), modifiers);
            }
            
            return handled;
        }
        
        public void OnMouseUp(Bitmap bitmap)
        {
            // TODO: Handle magnifier.
            // TODO: Memorize the action we just finished to enable undo.
            // TODO: keep tool or change tool.
            // m_ActiveTool = m_ActiveTool.KeepTool ? m_ActiveTool : m_PointerTool;
            
            if(screenToolManager.IsUsingHandTool)
            {
                screenToolManager.HandTool.OnMouseUp();
                metadata.UpdateTrackPoint(bitmap);
                // On Poke.
                // magnifier on mouse up.
                
                // If we were resizing an SVG drawing, trigger a render.
                // TODO: this is currently triggered on every mouse up, not only on resize !
                /*int selectedFrame = m_FrameServer.Metadata.SelectedDrawingFrame;
                int selectedDrawing = m_FrameServer.Metadata.SelectedDrawing;
                if(selectedFrame != -1 && selectedDrawing  != -1)
                {
                    DrawingSVG d = m_FrameServer.Metadata.Keyframes[selectedFrame].Drawings[selectedDrawing] as DrawingSVG;
                    if(d != null)
                    {
                        d.ResizeFinished();
                    }
                }*/
            }
            else
            {
                // todo: save tool addition as a command.
                screenToolManager.AfterToolUse();
                // todo: start deselection timer.
            }
        }
        
        public bool HitTest(Point mouse, Point imageLocation, float imageZoom)
        {
            if(metadata == null)
                return false;
            
            ImageToViewportTransformer transformer = new ImageToViewportTransformer(imageLocation, imageZoom);
            PointF imagePoint = transformer.Untransform(mouse);
            
            int keyframeIndex = 0;
            return metadata.IsOnDrawing(keyframeIndex, imagePoint.ToPoint(), fixedTimestamp);
        }
        
        public Cursor GetCursor(float scale)
        {
            return screenToolManager.GetCursor(scale);
        }
        
        public void DeleteHitDrawing()
        {
            metadata.DeleteHitDrawing();
        }
        
        public void DeselectTool()
        {
            screenToolManager.SetActiveTool(null);
        }
        
        private void CreateNewDrawing(Point imagePoint, ImageToViewportTransformer transformer)
        {
            int keyframeIndex = 0;
            int timestampPerFrame = 1;
            long currentTimestamp = 0;
            bool editingLabel = false;
            
            if(screenToolManager.ActiveTool == ToolManager.Label)
                editingLabel = LabelMouseDown(imagePoint, currentTimestamp, transformer);

            if(editingLabel)
                return;
                
            AddDrawing(imagePoint, keyframeIndex, timestampPerFrame, transformer);
        }
        
        private bool LabelMouseDown(Point imagePoint, long currentTimestamp, ImageToViewportTransformer transformer)
        {
            bool hitExisting = false;
            foreach(DrawingText label in metadata.Labels())
            {
                int hit = label.HitTest(imagePoint, currentTimestamp, transformer, metadata.CoordinateSystem.Zooming);
                if(hit >= 0)
                {
                    hitExisting = true;
                    label.SetEditMode(true, transformer);
                }
            }
            
            return hitExisting;
        }
        
        private void AddDrawing(Point imagePoint, int keyframeIndex, int timestampPerFrame, ImageToViewportTransformer transformer)
        {
            AbstractDrawing drawing = screenToolManager.ActiveTool.GetNewDrawing(imagePoint, keyframeIndex, timestampPerFrame);
            metadata.AddDrawing(drawing, keyframeIndex);
            
            // Special cases
            if(screenToolManager.ActiveTool == ToolManager.Label)
            {
                if(LabelAdded != null)
                    LabelAdded(this, new DrawingEventArgs(drawing, keyframeIndex));
                
                ((DrawingText)drawing).SetEditMode(true, transformer);
            }
        }
        
        
    }
}
