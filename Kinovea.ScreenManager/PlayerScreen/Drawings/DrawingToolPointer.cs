/*
Copyright © Joan Charmant 2008.
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
        private SelectedObjectType m_SelectedObjectType;
        private PointF lastPoint;
        private PointF mouseDelta;
        private Point m_DirectZoomTopLeft;
        private int m_iResizingHandle;
        private Size m_ImgSize;
        private Cursor cursorHandOpen;
        private Cursor cursorHandClose;
        private int m_iLastCursorType = 0;
        #endregion

        #region Constructor
        public DrawingToolPointer()
        {
            manipulationType = ManipulationType.None;
            m_SelectedObjectType = SelectedObjectType.None;
            lastPoint = PointF.Empty;
            m_iResizingHandle = 0;
            m_ImgSize = new Size(320,240);
            mouseDelta = PointF.Empty;
            m_DirectZoomTopLeft = new Point(-1, -1);

            SetupHandCursors();
        }
        #endregion
        
        #region AbstractDrawingTool Implementation
        public override AbstractDrawing GetNewDrawing(Point _Origin, long _iTimestamp, long _AverageTimeStampsPerFrame, IImageToViewportTransformer transformer)
        {
            return null;
        }
        public override Cursor GetCursor(double _fStretchFactor)
        {
            return manipulationType == ManipulationType.None ? cursorHandOpen : cursorHandClose;
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

            bool bHit = true;
            manipulationType = ManipulationType.None;

            metadata.UnselectAll();

            // Hit testing doesn't need subpixel accuracy.
            Point coords = mouseCoordinates.ToPoint();

            if (!IsOnDrawing(metadata, activeKeyFrameIndex, coords, currentTimeStamp, allFrames))
            {
                if (!IsOnTrack(metadata, coords, currentTimeStamp))
                {
                    if (!IsOnExtraDrawing(metadata, coords, currentTimeStamp))
                    {
                        // Moving the whole image (Direct Zoom)
                        m_SelectedObjectType = SelectedObjectType.None;
                        bHit = false;
                    }
                }
            }
            
            // Store position (descaled: in original image coords).
            lastPoint.X = mouseCoordinates.X;
            lastPoint.Y = mouseCoordinates.Y;

            return bHit;
        }
        public bool OnMouseMove(Metadata metadata, PointF mouseLocation, Point _DirectZoomTopLeft, Keys _ModifierKeys)
        {
            // Note: We work with image coordinates at subpixel accuracy.
            // Note: We only get here if left mouse button is down.

            bool bIsMovingAnObject = true;
            float deltaX = 0;
            float deltaY = 0;

            if (m_DirectZoomTopLeft.X == -1)
            {
                // Initialize the zoom offset.
                m_DirectZoomTopLeft = new Point(_DirectZoomTopLeft.X, _DirectZoomTopLeft.Y);
            }

            // Find difference between previous and current position
            deltaX = (mouseLocation.X - lastPoint.X) - (_DirectZoomTopLeft.X - m_DirectZoomTopLeft.X);
            lastPoint.X = mouseLocation.X;
            
            deltaY = (mouseLocation.Y - lastPoint.Y) - (_DirectZoomTopLeft.Y - m_DirectZoomTopLeft.Y);
            lastPoint.Y = mouseLocation.Y;
            
            mouseDelta = new PointF(deltaX, deltaY);
            m_DirectZoomTopLeft = new Point(_DirectZoomTopLeft.X, _DirectZoomTopLeft.Y);

            if (deltaX != 0 || deltaY != 0)
            {
                switch (manipulationType)
                {
                    case ManipulationType.Move:
                        {
                            switch (m_SelectedObjectType)
                            {
                                case SelectedObjectType.ExtraDrawing:
                                    if (metadata.SelectedExtraDrawing >= 0)
                                    {
                                        metadata.ExtraDrawings[metadata.SelectedExtraDrawing].MoveDrawing(deltaX, deltaY, _ModifierKeys, metadata.CoordinateSystem.Zooming);
                                    }
                                    break;
                                case SelectedObjectType.Drawing:
                                    if (metadata.SelectedDrawingFrame >= 0 && metadata.SelectedDrawing >= 0)
                                    {
                                        metadata.Keyframes[metadata.SelectedDrawingFrame].Drawings[metadata.SelectedDrawing].MoveDrawing(deltaX, deltaY, _ModifierKeys, metadata.CoordinateSystem.Zooming);
                                    }
                                    break;
                                default:
                                    bIsMovingAnObject = false;
                                    break;
                            }
                        }
                        break;
                    case ManipulationType.Resize:
                        {
                            switch (m_SelectedObjectType)
                            {
                                case SelectedObjectType.ExtraDrawing:
                                    if (metadata.SelectedExtraDrawing >= 0)
                                    {
                                        metadata.ExtraDrawings[metadata.SelectedExtraDrawing].MoveHandle(mouseLocation, m_iResizingHandle, _ModifierKeys);		
                                    }
                                    break;
                                case SelectedObjectType.Drawing:
                                    if (metadata.SelectedDrawingFrame >= 0 && metadata.SelectedDrawing >= 0)
                                    {
                                        metadata.Keyframes[metadata.SelectedDrawingFrame].Drawings[metadata.SelectedDrawing].MoveHandle(mouseLocation, m_iResizingHandle, _ModifierKeys);
                                    }
                                    break;
                                default:
                                    bIsMovingAnObject = false;
                                    break;
                            }
                        }
                        break;
                    default:
                        bIsMovingAnObject = false;
                        break;
                }
            }
            else
            {
                bIsMovingAnObject = false;
            }

            return bIsMovingAnObject;
        }
        public void SetImageSize(Size _size)
        {
            m_ImgSize = new Size(_size.Width, _size.Height);	
        }
        public void SetZoomLocation(Point _point)
        {
            m_DirectZoomTopLeft = new Point(_point.X, _point.Y);	
        }
        public Cursor GetCursor(int _type)
        {
            // 0: Open hand, 1: Closed hand, -1: same as last time.
            
            Cursor cur = cursorHandOpen;
            switch(_type)
            {
                case -1:
                    cur = (m_iLastCursorType == 0)?cursorHandOpen:cursorHandClose;
                    break;
                case 1:
                    cur = cursorHandClose;
                    break;
            }

            return cur;
        }
        #endregion
        
        #region Helpers
        private bool IsOnDrawing(Metadata _Metadata, int _iActiveKeyFrameIndex, Point mouseCoordinates, long _iCurrentTimeStamp, bool _bAllFrames)
        {
            bool bIsOnDrawing = false;
            
            if (_bAllFrames && _Metadata.Keyframes.Count > 0)
            {
                int[] zOrder = _Metadata.GetKeyframesZOrder(_iCurrentTimeStamp);

                for (int i = 0; i < zOrder.Length; i++)
                {
                    bIsOnDrawing = DrawingHitTest(_Metadata, zOrder[i], mouseCoordinates, _iCurrentTimeStamp, _Metadata.CoordinateSystem);
                    if (bIsOnDrawing)
                    {
                        break;
                    }
                }
            }
            else if (_iActiveKeyFrameIndex >= 0)
            {
                bIsOnDrawing = DrawingHitTest(_Metadata, _iActiveKeyFrameIndex, mouseCoordinates, _Metadata[_iActiveKeyFrameIndex].Position, _Metadata.CoordinateSystem);
            }

            return bIsOnDrawing;
        }
        private bool DrawingHitTest(Metadata _Metadata, int _iKeyFrameIndex, Point mouseCoordinates, long _iCurrentTimeStamp, CoordinateSystem transformer)
        {
            bool bDrawingHit = false;
            Keyframe kf = _Metadata.Keyframes[_iKeyFrameIndex];
            int hitRes = -1;
            int iCurrentDrawing = 0;

            while (hitRes < 0 && iCurrentDrawing < kf.Drawings.Count)
            {
                hitRes = kf.Drawings[iCurrentDrawing].HitTest(mouseCoordinates, _iCurrentTimeStamp, transformer, transformer.Zooming);
                if (hitRes >= 0)
                {
                    bDrawingHit = true;
                    m_SelectedObjectType = SelectedObjectType.Drawing;
                    _Metadata.SelectedDrawing = iCurrentDrawing;
                    _Metadata.SelectedDrawingFrame = _iKeyFrameIndex;
                    //_Metadata.HitDrawing = kf.Drawings[iCurrentDrawing];

                    // Handler hit ?
                    if (hitRes > 0)
                    {
                        manipulationType = ManipulationType.Resize;
                        m_iResizingHandle = hitRes;
                    }
                    else
                    {
                        manipulationType = ManipulationType.Move;
                    }
                }
                else
                {
                    iCurrentDrawing++;
                }
            }

            return bDrawingHit;
        }
        private bool IsOnExtraDrawing(Metadata metadata, Point mouseCoordinates, long currentTimestamp)
        {
            // Test if we hit an unattached drawing.
            
            bool isOnDrawing = false;
            int hitResult = -1;
            int currentDrawing = 0;

            while (hitResult < 0 && currentDrawing < metadata.ExtraDrawings.Count)
            {
                hitResult = metadata.ExtraDrawings[currentDrawing].HitTest(mouseCoordinates, currentTimestamp, metadata.CoordinateSystem, metadata.CoordinateSystem.Zooming);
                if (hitResult >= 0)
                {
                    isOnDrawing = true;
                    m_SelectedObjectType = SelectedObjectType.ExtraDrawing;
                    metadata.SelectedExtraDrawing = currentDrawing;
                    
                    // Handler hit ?
                    if (hitResult > 0)
                    {
                        manipulationType = ManipulationType.Resize;
                        m_iResizingHandle = hitResult;
                    }
                    else
                    {
                        manipulationType = ManipulationType.Move;
                    }
                }
                else
                {
                    currentDrawing++;
                }
            }
            
            return isOnDrawing;
        }
        private bool IsOnTrack(Metadata _Metadata, Point mouseCoordinates, long _iCurrentTimeStamp)
        {
            // Track have their own special hit test because we need to differenciate the interactive case from the edit case.
            bool bTrackHit = false;

            for (int i = 0; i < _Metadata.ExtraDrawings.Count; i++)
            {
                DrawingTrack trk = _Metadata.ExtraDrawings[i] as DrawingTrack;
                if(trk != null)
                {
                    // Handle signification depends on track status.
                    int handle = trk.HitTest(mouseCoordinates, _iCurrentTimeStamp, _Metadata.CoordinateSystem, _Metadata.CoordinateSystem.Zooming);
                    if (handle < 0)
                        continue;

                    bTrackHit = true;
                    m_SelectedObjectType = SelectedObjectType.ExtraDrawing;
                    _Metadata.SelectedExtraDrawing = i;

                    manipulationType = ManipulationType.Move;

                    switch (trk.Status)
                    {
                        case TrackStatus.Interactive:
                            if (handle == 0 || handle == 1)
                            {
                                manipulationType = ManipulationType.Resize;
                                m_iResizingHandle = handle;
                            }
                            break;
                        case TrackStatus.Configuration:
                            if (handle > 1)
                            {
                                manipulationType = ManipulationType.Resize;
                                m_iResizingHandle = handle;
                            }
                            break;
                    }

                    break;
                }	
            }

            return bTrackHit;
        }
        private void SetupHandCursors()
        {
            // Hand cursor.
            Bitmap bmpOpen = Kinovea.ScreenManager.Properties.Drawings.handopen24c;
            cursorHandOpen = new Cursor(bmpOpen.GetHicon());
            
            Bitmap bmpClose = Kinovea.ScreenManager.Properties.Drawings.handclose24b;
            cursorHandClose = new Cursor(bmpClose.GetHicon());

            m_iLastCursorType = 0;
        }
        #endregion
    }

}
