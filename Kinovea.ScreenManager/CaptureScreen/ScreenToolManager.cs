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
using System.Collections.Generic;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Simple accessor to the hand tool and the currently active tool.
    /// </summary>
    public class ScreenToolManager
    {
        public bool IsUsingHandTool
        {
            get { return activeTool == handTool;}
        }
        
        public DrawingToolPointer HandTool
        {
            get { return handTool; }
        }
        public AbstractDrawingTool ActiveTool
        {
            get { return activeTool;}
        }
    
        private AbstractDrawingTool activeTool;
        private DrawingToolPointer handTool;
        private Cursor cursor;
        private bool invalidateCursor = true;
        private ScreenPointerManager cursorManager = new ScreenPointerManager();
        private List<AbstractDrawingTool> tools = new List<AbstractDrawingTool>();
        
        public ScreenToolManager()
        {
            handTool = new DrawingToolPointer();
            activeTool = handTool;
        }

        /// <summary>
        /// Enable solo mode for a drawing.
        /// The hit tests will only work for this drawing.
        /// This is used for special rendering surfaces like tracking configuration.
        /// </summary>
        public void SetSoloMode(bool isSolo, Guid soloId, bool configureTracking)
        {
            handTool.SetSoloMode(isSolo, soloId, configureTracking);
        }

        public void SetActiveTool(AbstractDrawingTool tool)
        {
            activeTool = tool ?? handTool;
            invalidateCursor = true;
        }
        public void AfterFrameChanged()
        {
            if (!activeTool.KeepToolFrameChanged)
            {
                activeTool = handTool;
                invalidateCursor = true;
            }
        }

        public void AfterToolUse()
        {
            if (!activeTool.KeepTool)
            {
                activeTool = handTool;
                invalidateCursor = true;
            }
        }

        /// <summary>
        /// Get the current cursor.
        /// This is called on every mouse move in case we change from image resizer cursor to normal tool cursor.
        /// </summary>
        public Cursor GetCursor(float scale)
        {
            if (cursor == null || invalidateCursor)
            {
                invalidateCursor = false;
                cursor = cursorManager.GetToolCursor(activeTool, scale);
            }
            
            return cursor;
        }

        /// <summary>
        /// Invalidate the cursor. 
        /// This must be called after a tool change or image scale change.
        /// </summary>
        public void InvalidateCursor()
        {
            invalidateCursor = true;
        }
    }
}
