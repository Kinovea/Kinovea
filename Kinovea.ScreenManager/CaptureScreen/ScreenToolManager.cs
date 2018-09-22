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
        private List<AbstractDrawingTool> tools = new List<AbstractDrawingTool>();
        
        public ScreenToolManager()
        {
            handTool = new DrawingToolPointer();
            activeTool = handTool;
        }
        
        public void SetActiveTool(AbstractDrawingTool tool)
        {
            activeTool = tool ?? handTool;
        }
        public void AfterFrameChanged()
        {
            activeTool = activeTool.KeepToolFrameChanged ? activeTool : handTool;
        }
        public void AfterToolUse()
        {
            activeTool = activeTool.KeepTool ? activeTool : handTool;
        }
        public Cursor GetCursor(float scale)
        {
            return CursorManager.GetToolCursor(activeTool, scale);
        }
    }
}
