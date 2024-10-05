#region License
/*
Copyright © Joan Charmant 2012.
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

namespace Kinovea.ScreenManager
{
    /// <summary>
    ///  Simple event args for Added, Removed, Modified type of events.
    /// </summary>
    public class DrawingEventArgs : EventArgs
    {
        public AbstractDrawing Drawing
        {
            get { return drawing; }
        }
        
        public Guid ManagerId
        {
            get { return managerId; }
        }

        public DrawingAction DrawingAction
        {
            get { return drawingAction; }
        }

        private readonly AbstractDrawing drawing;
        private readonly Guid managerId;
        private readonly DrawingAction drawingAction;
                
        public DrawingEventArgs(AbstractDrawing drawing, Guid managerId, DrawingAction action = DrawingAction.Unknown)
        {
            this.drawing = drawing;
            this.managerId = managerId;
            this.drawingAction = action;
        }
    }
}
