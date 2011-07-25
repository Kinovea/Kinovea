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
        	get { throw new NotImplementedException(); }
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
		
        public Point MouseDelta
        {
            get { return m_MouseDelta; }
        }
		#endregion
        
        #region Members
        //--------------------------------------------------------------------
        // We do not keep the strecth/zoom factor here.
        // All coordinates must be given already descaled to image coordinates.
        //--------------------------------------------------------------------
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
        public override Cursor GetCursor(double _fStretchFactor)
        {
        	throw new NotImplementedException();
        }
        #endregion

        #region Public Interface
        public void OnMouseUp()
        {
            m_UserAction = UserAction.None;
        }
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
            	if (!IsOnTrack(_Metadata, _MouseCoordinates, _iCurrentTimeStamp))
                {
            		if (!IsOnExtraDrawing(_Metadata, _MouseCoordinates, _iCurrentTimeStamp))
                	{
            			// Moving the whole image (Direct Zoom)
						m_SelectedObjectType = SelectedObjectType.None;
						bHit = false;
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
                            	case SelectedObjectType.ExtraDrawing:
                            		if (_Metadata.SelectedExtraDrawing >= 0)
                            		{
                            			_Metadata.ExtraDrawings[_Metadata.SelectedExtraDrawing].MoveDrawing(deltaX, deltaY, _ModifierKeys);
                            		}
                            		break;
                                case SelectedObjectType.Drawing:
                                    if (_Metadata.SelectedDrawingFrame >= 0 && _Metadata.SelectedDrawing >= 0)
                                    {
                                        _Metadata.Keyframes[_Metadata.SelectedDrawingFrame].Drawings[_Metadata.SelectedDrawing].MoveDrawing(deltaX, deltaY, _ModifierKeys);
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
                            	case SelectedObjectType.ExtraDrawing:
                            		if (_Metadata.SelectedExtraDrawing >= 0)
                            		{
                            			_Metadata.ExtraDrawings[_Metadata.SelectedExtraDrawing].MoveHandle(_MouseLocation, m_iResizingHandle);		
                            		}
                            		break;
                                case SelectedObjectType.Drawing:
                                    if (_Metadata.SelectedDrawingFrame >= 0 && _Metadata.SelectedDrawing >= 0)
                                    {
                                        _Metadata.Keyframes[_Metadata.SelectedDrawingFrame].Drawings[_Metadata.SelectedDrawing].MoveHandle(_MouseLocation, m_iResizingHandle);
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
            
            Cursor cur = m_curHandOpen;
            switch(_type)
            {
            	case -1:
            		cur = (m_iLastCursorType == 0)?m_curHandOpen:m_curHandClose;
            		break;
            	case 1:
            		cur = m_curHandClose;
					break;
            }

            return cur;
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
        private bool IsOnExtraDrawing(Metadata _Metadata, Point _MouseCoordinates, long _iCurrentTimeStamp)
        {
        	// Test if we hit an unattached drawing.
        	
        	bool bIsOnDrawing = false;
            int hitRes = -1;
            int iCurrentDrawing = 0;

            while (hitRes < 0 && iCurrentDrawing < _Metadata.ExtraDrawings.Count)
            {
            	hitRes = _Metadata.ExtraDrawings[iCurrentDrawing].HitTest(_MouseCoordinates, _iCurrentTimeStamp);
                if (hitRes >= 0)
                {
                	bIsOnDrawing = true;
                	m_SelectedObjectType = SelectedObjectType.ExtraDrawing;
                	_Metadata.SelectedExtraDrawing = iCurrentDrawing;
                	
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
            
            return bIsOnDrawing;
        }
        private bool IsOnTrack(Metadata _Metadata, Point _MouseCoordinates, long _iCurrentTimeStamp)
        {
        	// Track have their own special hit test because we need to differenciate the interactive case from the edit case.
            bool bTrackHit = false;

            for (int i = 0; i < _Metadata.ExtraDrawings.Count; i++)
            {
            	Track trk = _Metadata.ExtraDrawings[i] as Track;
            	if(trk != null)
            	{
            		// Result: 
	            	// -1 = miss, 0 = on traj, 1 = on Cursor, 2 = on main label, 3+ = on keyframe label.
	            
	                int handle = trk.HitTest(_MouseCoordinates, _iCurrentTimeStamp);
	
	                if (handle >= 0)
	                {
	                    bTrackHit = true;
	                    m_SelectedObjectType = SelectedObjectType.ExtraDrawing;
                		_Metadata.SelectedExtraDrawing = i;
	
	                    if(handle > 1)
	                	{
	                		// Touched target or handler.
	                    	// The handler would have been saved inside the track object.
	                		m_UserAction = UserAction.Move;	
	                	}
	                    else if (trk.Status == Track.TrackStatus.Interactive)
	                    {
	                    	m_UserAction = UserAction.Resize;
	                    	m_iResizingHandle = handle;	
	                    }
	                    else
	                    {
	                    	// edit mode + 0 or 1.
	                    	m_UserAction = UserAction.Move;
	                    }
	                   
	                    break;
	                }
	            }	
            }

            return bTrackHit;
        }
        private void SetupHandCursors()
        {
        	// Hand cursor.
        	Bitmap bmpOpen = Kinovea.ScreenManager.Properties.Drawings.handopen24c;
            m_curHandOpen = new Cursor(bmpOpen.GetHicon());
            
            Bitmap bmpClose = Kinovea.ScreenManager.Properties.Drawings.handclose24b;
            m_curHandClose = new Cursor(bmpClose.GetHicon());

            m_iLastCursorType = 0;
        }
    	#endregion
    }

}
