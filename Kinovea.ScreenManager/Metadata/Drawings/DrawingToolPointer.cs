/*
Copyright © Joan Charmant 2008.
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

using System;
using System.Drawing;
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    public class DrawingToolPointer : AbstractDrawingTool
    {
        #region Enum
        private enum SelectedObjectType
        {
            None,
            Track,   
            Chrono,  
            Drawing,
            ExtraDrawing,
            Grid,
            Plane
        }
        #endregion
        
        #region Properties
        public override string Name
        {
            get { return "Pointer"; }
        }
        public override string DisplayName
        {
            get { return ScreenManagerLang.ToolTip_DrawingToolPointer; }
        }
        public override Bitmap Icon
        {
            get { return Kinovea.ScreenManager.Properties.Drawings.handtool; }
        }
        public override bool Attached
        {
            get { return false; }
        }
        public override bool KeepTool
        {
            get { return true; }
        }
        public override bool KeepToolFrameChanged
        {
            get { return true; }
        }
        public override DrawingStyle StylePreset
        {
            get	{ throw new NotImplementedException(); }
            set	{ throw new NotImplementedException(); }
        }
        public override DrawingStyle DefaultStylePreset
        {
            get	{ throw new NotImplementedException(); }
        }
        
        public PointF MouseDelta
        {
            get { return mouseDelta; }
        }
        #endregion
        
        #region Members
        //--------------------------------------------------------------------
        // We do not keep the strecth/zoom factor here.
        // All coordinates must be given already descaled to image coordinates.
        //--------------------------------------------------------------------
        private ManipulationType manipulationType;
        private SelectedObjectType selectedObjectType;
        private PointF lastPoint;
        private PointF mouseDelta;
        private Point directZoomTopLeft;
        private int resizingHandle;
        private Size imgSize;
        private Cursor cursorHandOpen;
        private Cursor cursorHandClose;
        private int lastCursorType = 0;
        #endregion

        #region Constructor
        public DrawingToolPointer()
        {
            manipulationType = ManipulationType.None;
            selectedObjectType = SelectedObjectType.None;
            lastPoint = PointF.Empty;
            resizingHandle = 0;
            imgSize = new Size(320,240);
            mouseDelta = PointF.Empty;
            directZoomTopLeft = new Point(-1, -1);

            SetupHandCursors();
        }
        #endregion
        
        #region AbstractDrawingTool Implementation
        public override AbstractDrawing GetNewDrawing(PointF origin, long timestamp, long averageTimeStampsPerFrame, IImageToViewportTransformer transformer)
        {
            return null;
        }
        #endregion

        #region Public Interface
        public void OnMouseUp()
        {
            manipulationType = ManipulationType.None;
        }
        public bool OnMouseDown(Metadata metadata, int activeKeyFrameIndex, PointF mouseCoordinates, long currentTimeStamp, bool allFrames)
        {
            //--------------------------------------------------------------------------------------
            // Change the ManipulationType if we are on a Drawing, Track, etc.
            // When we later pass in the MouseMove function, we will have the right ManipulationType set
            // and we will be able to do the right thing.
            //
            // We give priority to Keyframes Drawings because they can be moved...
            // If a Drawing is under a Trajectory or Chrono, we have to be able to move it around...
            //--------------------------------------------------------------------------------------

            manipulationType = ManipulationType.None;
            metadata.UnselectAll();

            // Store position (descaled: in original image coords).
            lastPoint.X = mouseCoordinates.X;
            lastPoint.Y = mouseCoordinates.Y;

            if (IsOnDrawing(metadata, activeKeyFrameIndex, mouseCoordinates, currentTimeStamp, allFrames))
                return true;

            if (IsOnTrack(metadata, mouseCoordinates, currentTimeStamp))
                return true;

            if (IsOnChronometer(metadata, mouseCoordinates, currentTimeStamp))
                return true;

            if (IsOnExtraDrawing(metadata, mouseCoordinates, currentTimeStamp))
                return true;

            // Moving the whole image (Direct Zoom)
            selectedObjectType = SelectedObjectType.None;
            return false;
        }
        public bool OnMouseMove(Metadata metadata, PointF mouseLocation, Point newDirectZoomTopLeft, Keys modifiers)
        {
            // Note: We work with image coordinates at subpixel accuracy.
            // Note: We only get here if left mouse button is down.

            bool isMovingAnObject = true;
            float deltaX = 0;
            float deltaY = 0;

            if (this.directZoomTopLeft.X == -1)
            {
                // Initialize the zoom offset.
                this.directZoomTopLeft = new Point(newDirectZoomTopLeft.X, newDirectZoomTopLeft.Y);
            }

            // Find difference between previous and current position
            deltaX = (mouseLocation.X - lastPoint.X) - (newDirectZoomTopLeft.X - directZoomTopLeft.X);
            lastPoint.X = mouseLocation.X;
            
            deltaY = (mouseLocation.Y - lastPoint.Y) - (newDirectZoomTopLeft.Y - directZoomTopLeft.Y);
            lastPoint.Y = mouseLocation.Y;
            
            mouseDelta = new PointF(deltaX, deltaY);
            directZoomTopLeft = new Point(newDirectZoomTopLeft.X, newDirectZoomTopLeft.Y);

            if (deltaX == 0 && deltaY == 0)
                return false;

            switch (manipulationType)
            {
                case ManipulationType.Move:
                    {
                        switch (selectedObjectType)
                        {
                            case SelectedObjectType.ExtraDrawing:
                            case SelectedObjectType.Drawing:
                                if (metadata.HitDrawing != null)
                                    metadata.HitDrawing.MoveDrawing(deltaX, deltaY, modifiers, metadata.ImageTransform.Zooming);
                                break;
                            default:
                                isMovingAnObject = false;
                                break;
                        }
                    }
                    break;
                case ManipulationType.Resize:
                    {
                        switch (selectedObjectType)
                        {
                            case SelectedObjectType.ExtraDrawing:
                            case SelectedObjectType.Drawing:
                                if (metadata.HitDrawing != null)
                                    metadata.HitDrawing.MoveHandle(mouseLocation, resizingHandle, modifiers);
                                break;
                            default:
                                isMovingAnObject = false;
                                break;
                        }
                    }
                    break;
                default:
                    isMovingAnObject = false;
                    break;
            }
            
            return isMovingAnObject;
        }
        public void SetImageSize(Size newSize)
        {
            imgSize = newSize;
        }
        public void SetZoomLocation(Point point)
        {
            directZoomTopLeft = point;
        }
        public Cursor GetCursor()
        {
            return manipulationType == ManipulationType.None ? cursorHandOpen : cursorHandClose;
        }
        public Cursor GetCursor(int type)
        {
            // 0: Open hand, 1: Closed hand, -1: same as last time.
            
            Cursor cur = cursorHandOpen;
            switch(type)
            {
                case -1:
                    cur = lastCursorType == 0 ? cursorHandOpen : cursorHandClose;
                    break;
                case 1:
                    cur = cursorHandClose;
                    break;
            }

            return cur;
        }
        #endregion
        
        #region Helpers
        private bool IsOnDrawing(Metadata metadata, int activeKeyFrameIndex, PointF mouseCoordinates, long currentTimeStamp, bool allFrames)
        {
            if (metadata.Keyframes.Count == 0)
                return false;

            bool bIsOnDrawing = false;

            DistortionHelper distorter = metadata.CalibrationHelper.DistortionHelper;

            if (allFrames && metadata.Keyframes.Count > 0)
            {
                int[] zOrder = metadata.GetKeyframesZOrder(currentTimeStamp);

                for (int i = 0; i < zOrder.Length; i++)
                {
                    bIsOnDrawing = DrawingHitTest(metadata, zOrder[i], mouseCoordinates, currentTimeStamp, distorter, metadata.ImageTransform);
                    if (bIsOnDrawing)
                        break;
                }
            }
            else if (activeKeyFrameIndex >= 0)
            {
                bIsOnDrawing = DrawingHitTest(metadata, activeKeyFrameIndex, mouseCoordinates, metadata[activeKeyFrameIndex].Position, distorter, metadata.ImageTransform);
            }

            return bIsOnDrawing;
        }
        private bool DrawingHitTest(Metadata metadata, int keyFrameIndex, PointF mouseCoordinates, long currentTimeStamp, DistortionHelper distorter, ImageTransform transformer)
        {
            bool isOnDrawing = false;
            int hitResult = -1;
            int currentDrawing = 0;

            Keyframe kf = metadata.Keyframes[keyFrameIndex];
            while (hitResult < 0 && currentDrawing < kf.Drawings.Count)
            {
                hitResult = kf.Drawings[currentDrawing].HitTest(mouseCoordinates, currentTimeStamp, distorter, transformer, transformer.Zooming);

                if (hitResult < 0)
                {
                    currentDrawing++;
                    continue;
                }
                
                isOnDrawing = true;
                selectedObjectType = SelectedObjectType.Drawing;
                metadata.SelectDrawing(kf.Drawings[currentDrawing]);
                metadata.SelectKeyframe(kf);

                if (hitResult > 0)
                {
                    manipulationType = ManipulationType.Resize;
                    resizingHandle = hitResult;
                }
                else
                {
                    manipulationType = ManipulationType.Move;
                }
            }

            return isOnDrawing;
        }
        private bool IsOnExtraDrawing(Metadata metadata, PointF mouseCoordinates, long currentTimestamp)
        {
            // Test if we hit an unattached drawing.
            
            bool isOnDrawing = false;
            int hitResult = -1;
            int currentDrawing = 0;
            
            while (hitResult < 0 && currentDrawing < metadata.ExtraDrawings.Count)
            {
                hitResult = metadata.ExtraDrawings[currentDrawing].HitTest(mouseCoordinates, currentTimestamp, metadata.CalibrationHelper.DistortionHelper, metadata.ImageTransform, metadata.ImageTransform.Zooming);

                if (hitResult < 0)
                {
                    currentDrawing++;
                    continue;
                }

                isOnDrawing = true;
                selectedObjectType = SelectedObjectType.ExtraDrawing;
                metadata.SelectDrawing(metadata.ExtraDrawings[currentDrawing]);
                    
                if (hitResult > 0)
                {
                    manipulationType = ManipulationType.Resize;
                    resizingHandle = hitResult;
                }
                else
                {
                    manipulationType = ManipulationType.Move;
                }
            }
            
            return isOnDrawing;
        }
        private bool IsOnChronometer(Metadata metadata, PointF point, long currentTimestamp)
        {
            bool isOnDrawing = false;
            foreach (AbstractDrawing drawing in metadata.ChronoManager.Drawings)
            {
                int hitResult = drawing.HitTest(point, currentTimestamp, metadata.CalibrationHelper.DistortionHelper, metadata.ImageTransform, metadata.ImageTransform.Zooming);
                if (hitResult < 0)
                    continue;

                isOnDrawing = true;
                selectedObjectType = SelectedObjectType.ExtraDrawing;
                metadata.SelectDrawing(drawing);

                if (hitResult > 0)
                {
                    manipulationType = ManipulationType.Resize;
                    resizingHandle = hitResult;
                }
                else
                {
                    manipulationType = ManipulationType.Move;
                }

                break;
            }

            return isOnDrawing;
        }
        private bool IsOnTrack(Metadata metadata, PointF mouseCoordinates, long currentTimeStamp)
        {
            // Track have their own special hit test because we need to differenciate the interactive case from the edit case.
            bool isOnDrawing = false;
            foreach (AbstractDrawing drawing in metadata.TrackManager.Drawings)
            {
                DrawingTrack track = drawing as DrawingTrack;
                if (track == null)
                    continue;

                int hitResult = drawing.HitTest(mouseCoordinates, currentTimeStamp, metadata.CalibrationHelper.DistortionHelper, metadata.ImageTransform, metadata.ImageTransform.Zooming);
                if (hitResult < 0)
                    continue;

                isOnDrawing = true;
                selectedObjectType = SelectedObjectType.ExtraDrawing;
                metadata.SelectDrawing(drawing);

                manipulationType = ManipulationType.Move;

                switch (track.Status)
                {
                    case TrackStatus.Interactive:
                        if (hitResult == 0 || hitResult == 1)
                        {
                            manipulationType = ManipulationType.Resize;
                            resizingHandle = hitResult;
                        }
                        break;
                    case TrackStatus.Configuration:
                        if (hitResult > 1)
                        {
                            manipulationType = ManipulationType.Resize;
                            resizingHandle = hitResult;
                        }
                        break;
                }

                break;
            }

            return isOnDrawing;
        }
        private void SetupHandCursors()
        {
            // Hand cursor.
            Bitmap bmpOpen = Kinovea.ScreenManager.Properties.Drawings.handopen24c;
            cursorHandOpen = new Cursor(bmpOpen.GetHicon());
            
            Bitmap bmpClose = Kinovea.ScreenManager.Properties.Drawings.handclose24b;
            cursorHandClose = new Cursor(bmpClose.GetHicon());

            lastCursorType = 0;
        }
        #endregion
    }

}
