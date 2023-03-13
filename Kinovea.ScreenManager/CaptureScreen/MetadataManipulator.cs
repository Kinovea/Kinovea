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

        public Keyframe HitKeyframe
        {
            get { return metadata.HitKeyframe; }
        }

        public bool DrawingInitializing
        {
            get { return metadata.DrawingInitializing; }
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

        /// <summary>
        /// Start manipulation motion.
        /// </summary>
        public bool StartMove(MouseEventArgs e, Point imageLocation, float imageZoom)
        {
            if(metadata == null || screenToolManager == null)
                return false;

            // At this point we must know the current timestamp and metadata should be valid.

            // Possible contexts:
            // Creation of a new drawing, 
            // Start moving an existing drawing or a handle.
            // Start new step of multi-step initialization.

            // TODO: Handle magnifier.
            // TODO: see if this could handle whole image manipulation as well, but at the moment the resizers are stored in the viewport.

            bool handled = false;

            // Get the mouse point in image space.
            ImageToViewportTransformer transformer = new ImageToViewportTransformer(imageLocation, imageZoom);
            PointF imagePoint = transformer.Untransform(e.Location);
            
            metadata.AllDrawingTextToNormalMode();

            

            if (screenToolManager.IsUsingHandTool || e.Button == MouseButtons.Middle)
            {
                // TODO: Change cursor.
                handled = screenToolManager.HandTool.OnMouseDown(metadata, fixedKeyframe, imagePoint, fixedTimestamp, true);
            }
            else if (!metadata.DrawingInitializing)
            {
                CreateNewDrawing(imagePoint, transformer);
                handled = true;
            }
            else
            {
                // The active drawing is at initialization stage, it will receive the point commit during mouse up.
                handled = true;
            }

            return handled;
        }
        
        /// <summary>
        /// Continue moving the current object.
        /// </summary>
        public bool ContinueMove(MouseEventArgs e, Keys modifiers, Point imageLocation, float imageZoom)
        {
            if(metadata == null || screenToolManager == null)
                return false;

            bool handled = false;

            // Get the mouse point in image space.
            ImageToViewportTransformer transformer = new ImageToViewportTransformer(imageLocation, imageZoom);
            PointF imagePoint = transformer.Untransform(e.Location);
            
            if (e.Button == MouseButtons.None && metadata.DrawingInitializing)
            {
                IInitializable initializableDrawing = metadata.HitDrawing as IInitializable;
                if (initializableDrawing != null)
                {
                    initializableDrawing.InitializeMove(imagePoint, modifiers);
                    handled = true;
                }
            }
            else if(e.Button == MouseButtons.Left)
            {
                if (!screenToolManager.IsUsingHandTool)
                {
                    // Initialization of a drawing that is in the process of being added.
                    // (ex: dragging the second point of a line that we just added).
                    // Tools that are not IInitializable should reset to Pointer tool right after creation.
                    IInitializable drawing = metadata.HitDrawing as IInitializable;
                    if(drawing != null)
                        drawing.InitializeMove(imagePoint, modifiers);

                    handled = true;
                }
                else
                {
                    // Manipulation of an existing drawing via a handle.
                    // TODO: handle magnifier.
                    // TODO: handle video filters.
                    handled = screenToolManager.HandTool.OnMouseMove(metadata, imagePoint, Point.Empty, modifiers);
                }
            }
            else if (e.Button == MouseButtons.Middle)
            {
                // Middle mouse button: allow to move stuff even if we have a tool selected.
                handled = screenToolManager.HandTool.OnMouseMove(metadata, imagePoint, Point.Empty, modifiers);
            }

            return handled;
        }
        
        /// <summary>
        /// Stop moving the current object.
        /// </summary>
        public void StopMove(MouseEventArgs e, Bitmap bitmap, Keys modifiers, Point imageLocation, float imageZoom)
        {
            // TODO: Handle magnifier.
            // TODO: Memorize the action we just finished to enable undo.
            // TODO: keep tool or change tool.
            // m_ActiveTool = m_ActiveTool.KeepTool ? m_ActiveTool : m_PointerTool;
            
            if (e.Button != MouseButtons.Left)
                return;

            if (screenToolManager.IsUsingHandTool)
            {
                metadata.AllDrawingTextToNormalMode();
                metadata.UpdateTrackPoint(bitmap);
                screenToolManager.HandTool.OnMouseUp();
            }

            ImageToViewportTransformer transformer = new ImageToViewportTransformer(imageLocation, imageZoom);
            PointF imagePoint = transformer.Untransform(e.Location);
            metadata.InitializeCommit(null, imagePoint);

            screenToolManager.AfterToolUse();
        }
        
        /// <summary>
        /// Check if the passed point is on any drawing.
        /// The hit drawing, if any, will be placed in metadata.hitDrawing.
        /// The passed point is in screen space coordinates (transformed).
        /// </summary>
        public void HitTest(Point p, Point imageLocation, float imageZoom)
        {
            if(metadata == null)
                return;
            
            ImageToViewportTransformer transformer = new ImageToViewportTransformer(imageLocation, imageZoom);
            PointF imagePoint = transformer.Untransform(p);
            
            int keyframeIndex = 0;
            metadata.DeselectAll();
            metadata.IsOnDrawing(keyframeIndex, imagePoint, fixedTimestamp);
            metadata.IsOnDetachedDrawing(imagePoint, fixedTimestamp);
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
            IDecorable decorable = drawing as IDecorable;
            if (drawing == null || decorable == null)
                return;
            
            AbstractDrawingManager owner = metadata.HitDrawingOwner;
            HistoryMementoModifyDrawing memento = null;
            if (owner != null)
                memento = new HistoryMementoModifyDrawing(metadata, owner.Id, drawing.Id, drawing.Name, SerializationFilter.Style);

            FormConfigureDrawing2 fcd = new FormConfigureDrawing2(decorable, refresh);
            FormsHelper.Locate(fcd);
            fcd.ShowDialog();

            if (fcd.DialogResult == DialogResult.OK)
            {
                if (memento != null)
                {
                    memento.UpdateCommandName(drawing.Name);
                    metadata.HistoryStack.PushNewCommand(memento);
                }

                // If this was a singleton drawing also update the tool-level preset.
                if (metadata.HitDrawing is DrawingCoordinateSystem)
                {
                    ToolManager.SetStylePreset("CoordinateSystem", ((DrawingCoordinateSystem)metadata.HitDrawing).DrawingStyle);
                    ToolManager.SavePresets();
                }
                else if (metadata.HitDrawing is DrawingTestGrid)
                {
                    ToolManager.SetStylePreset("TestGrid", ((DrawingTestGrid)metadata.HitDrawing).DrawingStyle);
                    ToolManager.SavePresets();
                }
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
                Keyframe kf = new Keyframe(0, "", metadata, "", Keyframe.DefaultColor);
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
