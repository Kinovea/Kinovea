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
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    public class DrawingToolPointer : AbstractDrawingTool
    {
    	#region Properties
        public Size ImgSize
        {
            get { return m_ImgSize; }
            set 
            { 
                m_ImgSize.Width = value.Width;
                m_ImgSize.Height = value.Height;
            }
        }
        public Point MouseDelta
        {
            get { return m_MouseDelta; }
        }
        public Point DirectZoomTopLeft
        {
            get { return new Point(m_DirectZoomTopLeft.X, m_DirectZoomTopLeft.Y); }
            set { m_DirectZoomTopLeft = new Point(value.X, value.Y); }
        }
		#endregion
        
        #region Members
        //--------------------------------------------------------------------
        // We do not keep the strecth/zoom factor here.
        // All coordinates must be given already descaled to image coordinates.
        //--------------------------------------------------------------------
        private enum UserAction
        {
            None,
            Move,           
            Resize
        }
        private enum SelectedObjectType
        {
            None,
            Track,   
            Chrono,  
            Drawing,
            Grid,
            Plane
        }
        private UserAction m_UserAction;
        private SelectedObjectType m_SelectedObjectType;
        private Point m_lastPoint;
        private Point m_MouseDelta;
        private Point m_DirectZoomTopLeft;
        private int m_iResizingHandle;
        private Size m_ImgSize;
        Cursor m_curHandOpen;
        Cursor m_curHandClose;
        int m_iLastCursorType = 0;
        #endregion

        #region Constructor
        public DrawingToolPointer()
        {
            m_UserAction = UserAction.None;
            m_SelectedObjectType = SelectedObjectType.None;
            m_lastPoint = new Point(0, 0);
            m_iResizingHandle = 0;
            m_ImgSize = new Size(320,240);
            m_MouseDelta = new Point(0, 0);
            m_DirectZoomTopLeft = new Point(-1, -1);

            SetupHandCursors();
        }
		#endregion
        
        #region AbstractDrawingTool Implementation
        public override AbstractDrawing GetNewDrawing(Point _Origin, long _iTimestamp, long _AverageTimeStampsPerFrame)
        {
            return null;
        }
        public override void OnMouseMove(Keyframe _Keyframe, Point _MouseCoordinates)
        {
            // Not used, see non overriding OnMouseMove below.
        }
        public override DrawingToolType OnMouseUp()
        {
            m_UserAction = UserAction.None;
            return DrawingToolType.Pointer;
        }
        public override Cursor GetCursor(Color _color, int _iSize)
        {
            // Very special case for the Pointer tool cursor.
            // We use _iSize values to define the type of cursor:
            // 0: Open hand, 1: Closed hand, -1: same as last time.
            
            Cursor cur;
            int iType = 0;
            if (_iSize == -1)
            {
                iType = m_iLastCursorType;
            }
            else
            {
                iType = _iSize;
            }
            m_iLastCursorType = iType;

            if (iType == 0)
            {
                cur = m_curHandOpen;
            }
            else
            {
                cur = m_curHandClose;
            }

            return cur;
        }
        #endregion

        #region Public Interface
        public bool OnMouseDown(Metadata _Metadata, int _iActiveKeyFrameIndex, Point _MouseCoordinates, long _iCurrentTimeStamp, bool _bAllFrames)
        {
            //--------------------------------------------------------------------------------------
            // Change the UserAction if we are on a Drawing, Track, etc.
            // When we later pass in the MouseMove function, we will have the right UserAction set
            // and we will be able to do the right thing.
            //
            // We give priority to Keyframes Drawings because they can be moved...
            // If a Drawing is under a Trajectory or Chrono, we have to be able to move it around...
            //
            // Maybe we could reuse the IsOndrawing, etc. functions from MetaData...
            //--------------------------------------------------------------------------------------

            bool bHit = true;
            m_UserAction = UserAction.None;

            _Metadata.UnselectAll();

            if (!IsOnDrawing(_Metadata, _iActiveKeyFrameIndex, _MouseCoordinates, _iCurrentTimeStamp, _bAllFrames))
            {
                if (!IsOnChrono(_Metadata, _MouseCoordinates, _iCurrentTimeStamp))
                {
                    if (!IsOnTrack(_Metadata, _MouseCoordinates, _iCurrentTimeStamp))
                    {
                        if (!IsOnGrids(_Metadata, _MouseCoordinates))
                        {
                            // Moving the whole image (Direct Zoom)
                            m_SelectedObjectType = SelectedObjectType.None;
                            bHit = false;
                        }
                    }
                }
            }

            // Store position (descaled: in original image coords).
            m_lastPoint.X = _MouseCoordinates.X;
            m_lastPoint.Y = _MouseCoordinates.Y;

            return bHit;
        }
        public bool OnMouseMove(Metadata _Metadata, int _iActiveKeyframeIndex, Point _MouseLocation, Point _DirectZoomTopLeft, Keys _ModifierKeys)
        {
            // Note: We work with descaled coordinates.
            // Note: We only get here if left mouse button is down.

            bool bIsMovingAnObject = true;
            int deltaX = 0;
            int deltaY = 0;

            if (m_DirectZoomTopLeft.X == -1)
            {
                // Initialize the zoom offset.
                m_DirectZoomTopLeft = new Point(_DirectZoomTopLeft.X, _DirectZoomTopLeft.Y);
            }

            // Find difference between previous and current position
            // X and Y are independant so we can slide on the edges in case of DrawingMove.
            if (_MouseLocation.X >= 0 && _MouseLocation.X <= m_ImgSize.Width)
            {
                deltaX = (_MouseLocation.X - m_lastPoint.X) - (_DirectZoomTopLeft.X - m_DirectZoomTopLeft.X);
                m_lastPoint.X = _MouseLocation.X;
            }
            if(_MouseLocation.Y >= 0 && _MouseLocation.Y <= m_ImgSize.Height)
            {
                deltaY = (_MouseLocation.Y - m_lastPoint.Y) - (_DirectZoomTopLeft.Y - m_DirectZoomTopLeft.Y);
                m_lastPoint.Y = _MouseLocation.Y;
            }

            m_MouseDelta = new Point(deltaX, deltaY);
            m_DirectZoomTopLeft = new Point(_DirectZoomTopLeft.X, _DirectZoomTopLeft.Y);

            if (deltaX != 0 || deltaY != 0)
            {
                switch (m_UserAction)
                {
                    case UserAction.Move:
                        {
                            switch (m_SelectedObjectType)
                            {
                                case SelectedObjectType.Grid:
                                    _Metadata.Grid.MouseMove(deltaX, deltaY, _ModifierKeys);
                                    break;
                                case SelectedObjectType.Plane:
                                    _Metadata.Plane.MouseMove(deltaX, deltaY, _ModifierKeys);
                                    break;
                                case SelectedObjectType.Track:
                                    if (_Metadata.SelectedTrack >= 0)
                                    {
                                        if (_Metadata.Tracks[_Metadata.SelectedTrack].EditMode)
                                        {
                                            _Metadata.Tracks[_Metadata.SelectedTrack].MoveCursor(deltaX, deltaY);
                                        }
                                        else
                                        {
                                            _Metadata.Tracks[_Metadata.SelectedTrack].MoveCursor(_MouseLocation.X, _MouseLocation.Y);
                                        }
                                    }
                                    break;
                                case SelectedObjectType.Chrono:
                                    if (_Metadata.SelectedChrono >= 0)
                                    {
                                        _Metadata.Chronos[_Metadata.SelectedChrono].MoveDrawing(deltaX, deltaY);
                                    }
                                    break;
                                case SelectedObjectType.Drawing:
                                    if (_Metadata.SelectedDrawingFrame >= 0 && _Metadata.SelectedDrawing >= 0)
                                    {
                                        _Metadata.Keyframes[_Metadata.SelectedDrawingFrame].Drawings[_Metadata.SelectedDrawing].MoveDrawing(deltaX, deltaY);
                                    }
                                    break;
                                default:
                                    bIsMovingAnObject = false;
                                    break;
                            }
                        }
                        break;
                    case UserAction.Resize:
                        {
                            switch (m_SelectedObjectType)
                            {
                                case SelectedObjectType.Grid:
                                    _Metadata.Grid.MoveHandleTo(_MouseLocation, m_iResizingHandle);
                                    break;
                                case SelectedObjectType.Plane:
                                    _Metadata.Plane.MoveHandleTo(_MouseLocation, m_iResizingHandle);
                                    break;
                                case SelectedObjectType.Track:
                                    if (_Metadata.SelectedTrack >= 0)
                                    {
                                        _Metadata.Tracks[_Metadata.SelectedTrack].MoveLabelTo(deltaX, deltaY, m_iResizingHandle);
                                    }
                                    break;
                                case SelectedObjectType.Drawing:
                                    if (_Metadata.SelectedDrawingFrame >= 0 && _Metadata.SelectedDrawing >= 0)
                                    {
                                        _Metadata.Keyframes[_Metadata.SelectedDrawingFrame].Drawings[_Metadata.SelectedDrawing].MoveHandleTo(_MouseLocation, m_iResizingHandle);
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
		#endregion
        
		#region Helpers
        private bool IsOnDrawing(Metadata _Metadata, int _iActiveKeyFrameIndex, Point _MouseCoordinates, long _iCurrentTimeStamp, bool _bAllFrames)
        {
            bool bIsOnDrawing = false;

            if (_bAllFrames && _Metadata.Keyframes.Count > 0)
            {
                int[] zOrder = _Metadata.GetKeyframesZOrder(_iCurrentTimeStamp);

                for (int i = 0; i < zOrder.Length; i++)
                {
                    bIsOnDrawing = DrawingHitTest(_Metadata, zOrder[i], _MouseCoordinates, _iCurrentTimeStamp);
                    if (bIsOnDrawing)
                    {
                        break;
                    }
                }
            }
            else if (_iActiveKeyFrameIndex >= 0)
            {
                bIsOnDrawing = DrawingHitTest(_Metadata, _iActiveKeyFrameIndex, _MouseCoordinates, _Metadata[_iActiveKeyFrameIndex].Position);
            }

            return bIsOnDrawing;
        }
        private bool DrawingHitTest(Metadata _Metadata, int _iKeyFrameIndex, Point _MouseCoordinates, long _iCurrentTimeStamp)
        {
            bool bDrawingHit = false;
            Keyframe kf = _Metadata.Keyframes[_iKeyFrameIndex];
            int hitRes = -1;
            int iCurrentDrawing = 0;

            while (hitRes < 0 && iCurrentDrawing < kf.Drawings.Count)
            {
                hitRes = kf.Drawings[iCurrentDrawing].HitTest(_MouseCoordinates, _iCurrentTimeStamp);
                if (hitRes >= 0)
                {
                    bDrawingHit = true;
                    m_SelectedObjectType = SelectedObjectType.Drawing;
                    _Metadata.SelectedDrawing = iCurrentDrawing;
                    _Metadata.SelectedDrawingFrame = _iKeyFrameIndex;

                    // Handler hit ?
                    if (hitRes > 0)
                    {
                        m_UserAction = UserAction.Resize;
                        m_iResizingHandle = hitRes;
                    }
                    else
                    {
                        m_UserAction = UserAction.Move;
                    }
                }
                else
                {
                    iCurrentDrawing++;
                }
            }

            return bDrawingHit;
        }
        private bool IsOnChrono(Metadata _Metadata, Point _MouseCoordinates, long _iCurrentTimeStamp)
        {
            bool bIsOnChrono = false;

            for (int i = 0; i < _Metadata.Chronos.Count; i++)
            {
                int handle = _Metadata.Chronos[i].HitTest(_MouseCoordinates, _iCurrentTimeStamp);

                if (handle >= 0)
                {
                    bIsOnChrono = true;
                    m_SelectedObjectType = SelectedObjectType.Chrono;
                    _Metadata.SelectedChrono = i;

                    m_UserAction = UserAction.Move;
                    
                    break;
                }
            }

            return bIsOnChrono;
        }
        private bool IsOnTrack(Metadata _Metadata, Point _MouseCoordinates, long _iCurrentTimeStamp)
        {
            bool bTrackHit = false;

            for (int i = 0; i < _Metadata.Tracks.Count; i++)
            {

                int handle = _Metadata.Tracks[i].HitTest(_MouseCoordinates, _iCurrentTimeStamp);

                if (handle >= 0)
                {
                    bTrackHit = true;
                    m_SelectedObjectType = SelectedObjectType.Track;
                    _Metadata.SelectedTrack = i;

                    if (_Metadata.Tracks[i].EditMode == false)
                    {
                        // Trajectory is in interactive mode.

                        if (handle == 0)
                        {
                            // Mouse touched the trajectory.
                            m_UserAction = UserAction.Move;

                            // We should update the playhead now. No need to wait for the next mouse move.
                            _Metadata.Tracks[i].MoveCursor(_MouseCoordinates.X, _MouseCoordinates.Y);
                        }
                        else if (handle == 1)
                        {
                            // Mouse touched the cursor. We'll update the playhead when mouse move.
                            m_UserAction = UserAction.Move;
                        }
                        else
                        {
                            // Mouse touched a Label.
                            m_UserAction = UserAction.Resize;
                            m_iResizingHandle = handle;
                        }
                    }
                    else
                    {
                        // Trajectory is in edit mode.
                        // We react to labels and cursor.

                        if (handle == 1)
                        {
                            // Mouse touched the cursor.
                            m_UserAction = UserAction.Move;
                        }
                        else if (handle > 1)
                        {
                            // Mouse touched a Label.
                            m_UserAction = UserAction.Resize;
                            m_iResizingHandle = handle;
                        }
                    }

                    break;
                }
            }

            return bTrackHit;
        }
        private bool IsOnGrids(Metadata _Metadata, Point _MouseCoordinates)
        {
            // Check wether we hit the Grid or the 3D Plane.
            bool bIsOnGrids = false;
            int handle = -1;

            if (_Metadata.Plane.Visible)
            {
                handle = _Metadata.Plane.HitTest(_MouseCoordinates);
                if (handle >= 0)
                {
                    bIsOnGrids = true;
                    m_SelectedObjectType = SelectedObjectType.Plane;
                    _Metadata.Plane.Selected = true;
                    // Handler hit ?
                    if (handle > 0)
                    {
                        m_UserAction = UserAction.Resize;
                        m_iResizingHandle = handle;
                    }
                    else
                    {
                        m_UserAction = UserAction.Move;
                    }
                }
            }

            if (!bIsOnGrids && _Metadata.Grid.Visible)
            {
                handle = _Metadata.Grid.HitTest(_MouseCoordinates);
                if (handle >= 0)
                {
                    bIsOnGrids = true;
                    m_SelectedObjectType = SelectedObjectType.Grid;
                    _Metadata.Grid.Selected = true;

                    // Handler hit ?
                    if (handle > 0)
                    {
                        m_UserAction = UserAction.Resize;
                        m_iResizingHandle = handle;
                    }
                    else
                    {
                        m_UserAction = UserAction.Move;
                    }
                }
            }

            return bIsOnGrids;
        }
        private void SetupHandCursors()
        {
            Bitmap bmpOpen = Kinovea.ScreenManager.Properties.Resources.handopen24b;
            m_curHandOpen = new Cursor(bmpOpen.GetHicon());

            Bitmap bmpClose = Kinovea.ScreenManager.Properties.Resources.handclose24c;
            m_curHandClose = new Cursor(bmpClose.GetHicon());

            m_iLastCursorType = 0;
        }
    	#endregion
    }

}
