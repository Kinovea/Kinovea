﻿#region License
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
        public event EventHandler<DrawingEventArgs> DrawingModified;
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
            this.screenToolManager.HandTool.DrawingModified += (s, e) => DrawingModified?.Invoke(s, e);
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

            // Get the mouse point in image space.
            IImageToViewportTransformer transformer = new ImageToViewportTransformer(imageLocation, imageZoom);
            PointF imagePoint = transformer.Untransform(e.Location);

            metadata.AllDrawingTextToNormalMode();

            bool handled = false;
            if (screenToolManager.IsUsingHandTool || e.Button == MouseButtons.Middle)
            {
                // TODO: Change cursor.
                handled = screenToolManager.HandTool.OnMouseDown(metadata, transformer, fixedKeyframe, imagePoint, fixedTimestamp, true);
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

            // Get the mouse point in image space.
            IImageToViewportTransformer transformer = new ImageToViewportTransformer(imageLocation, imageZoom);
            PointF imagePoint = transformer.Untransform(e.Location);

            bool handled = false;
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
                screenToolManager.HandTool.OnMouseUp(metadata);
            }

            ImageToViewportTransformer transformer = new ImageToViewportTransformer(imageLocation, imageZoom);
            PointF imagePoint = transformer.Untransform(e.Location);
            metadata.InitializeCommit(null, imagePoint);

            screenToolManager.AfterToolUse();
        }

        public Cursor GetCursor(float scale)
        {
            return screenToolManager.GetCursor(scale);
        }

        public void InvalidateCursor()
        {
            screenToolManager.InvalidateCursor();
        }

        public void DeselectTool()
        {
            screenToolManager.SetActiveTool(null);
        }

        public void InitializeEndFromMenu(bool cancelLastPoint)
        {
            metadata.InitializeEnd(cancelLastPoint);
        }

        private void CreateNewDrawing(PointF imagePoint, IImageToViewportTransformer transformer)
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
                Keyframe kf = new Keyframe(0, "", Keyframe.DefaultColor, metadata);
                metadata.AddKeyframe(kf);
            }

            AddDrawing(imagePoint, keyframeIndex, timestampPerFrame, transformer);
        }

        private bool LabelMouseDown(PointF imagePoint, long currentTimestamp, IImageToViewportTransformer transformer)
        {
            bool hitExisting = false;
            foreach(DrawingText label in metadata.Labels())
            {
                int hit = label.HitTest(imagePoint, currentTimestamp, metadata.CalibrationHelper.DistortionHelper, transformer);
                if(hit >= 0)
                {
                    hitExisting = true;
                    label.SetEditMode(true, imagePoint, transformer);
                }
            }

            return hitExisting;
        }

        private void AddDrawing(PointF imagePoint, int keyframeIndex, int timestampPerFrame, IImageToViewportTransformer transformer)
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
