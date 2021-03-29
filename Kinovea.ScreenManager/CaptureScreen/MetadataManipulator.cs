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
using System.Windows.Forms;
using Kinovea.Services;

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
                handled = screenToolManager.HandTool.OnMouseDown(metadata, fixedKeyframe, imagePoint, fixedTimestamp, true);
            }
            else
            {
                handled = true;
                CreateNewDrawing(imagePoint, transformer);
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
                    drawing.InitializeMove(imagePoint, modifiers);
            }
            
            return handled;
        }
        
        public void OnMouseUp(Bitmap bitmap, Point mouse, Keys modifiers, Point imageLocation, float imageZoom)
        {
            // TODO: Handle magnifier.
            // TODO: Memorize the action we just finished to enable undo.
            // TODO: keep tool or change tool.
            // m_ActiveTool = m_ActiveTool.KeepTool ? m_ActiveTool : m_PointerTool;

            if (screenToolManager.IsUsingHandTool)
            {
                screenToolManager.HandTool.OnMouseUp();
                metadata.AllDrawingTextToNormalMode();
                metadata.UpdateTrackPoint(bitmap);
            }
            
            ImageToViewportTransformer transformer = new ImageToViewportTransformer(imageLocation, imageZoom);
            PointF imagePoint = transformer.Untransform(mouse);
            metadata.InitializeCommit(null, imagePoint);

            screenToolManager.AfterToolUse();
        }
        
        public bool HitTest(Point mouse, Point imageLocation, float imageZoom)
        {
            // Note: at the moment this method does not support extra drawings.

            if(metadata == null)
                return false;
            
            ImageToViewportTransformer transformer = new ImageToViewportTransformer(imageLocation, imageZoom);
            PointF imagePoint = transformer.Untransform(mouse);
            
            int keyframeIndex = 0;
            return metadata.IsOnDrawing(keyframeIndex, imagePoint, fixedTimestamp);
        }
        
        public Cursor GetCursor(float scale)
        {
            return screenToolManager.GetCursor(scale);
        }

        public void InvalidateCursor()
        {
            screenToolManager.InvalidateCursor();
        }
        
        public void DeleteHitDrawing()
        {
            Keyframe keyframe = metadata.HitKeyframe;
            AbstractDrawing drawing = metadata.HitDrawing;

            if (keyframe == null || drawing == null)
                return;

            HistoryMemento memento = new HistoryMementoDeleteDrawing(metadata, keyframe.Id, drawing.Id, drawing.Name);
            metadata.DeleteDrawing(keyframe.Id, drawing.Id);
            metadata.HistoryStack.PushNewCommand(memento);
        }

        public void ConfigureDrawing(AbstractDrawing drawing, Action refresh)
        {
            Keyframe keyframe = metadata.HitKeyframe;
            IDecorable decorable = drawing as IDecorable;
            if (keyframe == null || drawing == null || decorable == null)
                return;

            HistoryMementoModifyDrawing memento = new HistoryMementoModifyDrawing(metadata, keyframe.Id, drawing.Id, drawing.Name, SerializationFilter.Style);

            FormConfigureDrawing2 fcd = new FormConfigureDrawing2(decorable, refresh);
            FormsHelper.Locate(fcd);
            fcd.ShowDialog();

            if (fcd.DialogResult == DialogResult.OK)
            {
                memento.UpdateCommandName(drawing.Name);
                metadata.HistoryStack.PushNewCommand(memento);
            }

            fcd.Dispose();
        }
        
        public void DeselectTool()
        {
            screenToolManager.SetActiveTool(null);
        }
        
        public void InitializeEndFromMenu(bool cancelLastPoint)
        {
            metadata.InitializeEnd(cancelLastPoint);
        }

        private void CreateNewDrawing(PointF imagePoint, ImageToViewportTransformer transformer)
        {
            int keyframeIndex = 0;
            int timestampPerFrame = 1;
            long currentTimestamp = 0;
            bool editingLabel = false;
            
            if(screenToolManager.ActiveTool == ToolManager.Tools["Label"])
                editingLabel = LabelMouseDown(imagePoint, currentTimestamp, transformer);

            if(editingLabel)
                return;

            if(metadata.Count == 0)
            {
                Keyframe kf = new Keyframe(0, "", metadata);
                metadata.AddKeyframe(kf);
            }

            AddDrawing(imagePoint, keyframeIndex, timestampPerFrame, transformer);
        }
        
        private bool LabelMouseDown(PointF imagePoint, long currentTimestamp, ImageToViewportTransformer transformer)
        {
            bool hitExisting = false;
            foreach(DrawingText label in metadata.Labels())
            {
                int hit = label.HitTest(imagePoint, currentTimestamp, metadata.CalibrationHelper.DistortionHelper, transformer, metadata.ImageTransform.Zooming);
                if(hit >= 0)
                {
                    hitExisting = true;
                    label.SetEditMode(true, imagePoint, transformer);
                }
            }
            
            return hitExisting;
        }
        
        private void AddDrawing(PointF imagePoint, int keyframeIndex, int timestampPerFrame, ImageToViewportTransformer transformer)
        {
            AbstractDrawing drawing = screenToolManager.ActiveTool.GetNewDrawing(imagePoint, keyframeIndex, timestampPerFrame, transformer);
            Guid keyframeId = metadata.GetKeyframeId(keyframeIndex);

            HistoryMementoAddDrawing memento = new HistoryMementoAddDrawing(metadata, keyframeId, drawing.Id, drawing.ToolDisplayName);
            metadata.AddDrawing(keyframeId, drawing);
            memento.UpdateCommandName(drawing.Name);
            metadata.HistoryStack.PushNewCommand(memento);
            
            // Special cases
            // TODO: implement the event handler to metadata DrawingAdded and finish the label in the handler.
            if(screenToolManager.ActiveTool == ToolManager.Tools["Label"])
            {
                if(LabelAdded != null)
                    LabelAdded(this, new DrawingEventArgs(drawing, keyframeId));
                
                ((DrawingText)drawing).SetEditMode(true, imagePoint, transformer);
            }
        }
    }
}
