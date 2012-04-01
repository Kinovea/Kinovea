#region License
/*
Copyright © Joan Charmant 2012.
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
#endregion
using System;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public static class GenericPostureConstraintEngine
    {
        public static void MoveHandle(GenericPosture posture, int handle, Point point)
        {
            // Update the point(s) attached to the handle based on the constraints.
            // Update all other points that may have been impacted.
            switch(posture.Handles[handle].Type)
            {
                case HandleType.Point:
                    MovePointHandle(posture, handle, point);
                    break;
            }
        }
        private static void MovePointHandle(GenericPosture posture, int handle, Point point)
        {
            // switch constraint type.
            switch(posture.Handles[handle].ConstraintType)
            {
                case ConstraintType.None:
                    MovePointHandleFreely(posture, handle, point);
                    break;
                case ConstraintType.LineSlide:
                    GenericPostureConstraintLineSlide constraint = posture.Handles[handle].Constraint as GenericPostureConstraintLineSlide;
                    MovePointHandleAlongLine(posture, handle, point, constraint);
                    break;
            }
        }
        private static void MovePointHandleFreely(GenericPosture posture, int handle, Point point)
        {
            posture.Points[posture.Handles[handle].Reference] = point;
        }
        private static void MovePointHandleAlongLine(GenericPosture posture, int handle, Point point, GenericPostureConstraintLineSlide constraint)
        {
            if(constraint == null)
            {
                MovePointHandleFreely(posture, handle, point);
                return;
            }
            
            Point result;
            Point start = posture.Points[constraint.Start];
            Point end = posture.Points[constraint.End];
            
            switch(constraint.AllowedPosition)
            {
                case LineSlideAllowedPosition.Inbetween:
                    result = GeometryHelper.GetClosestPoint(start, end, point, true, 10);
                    break;
                default:
                    result = point;
                    break;
            }

            posture.Points[posture.Handles[handle].Reference] = result;
        }
    }
}
