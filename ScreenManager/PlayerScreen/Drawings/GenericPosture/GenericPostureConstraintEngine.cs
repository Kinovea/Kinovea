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
            // Constrain position of the point managed by this handle.
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
            
            // Constrain position of other points.
            switch(posture.Handles[handle].ImpactType)
            {
                case ImpactType.Align:
                    GenericPostureImpactAlign impact = posture.Handles[handle].Impact as GenericPostureImpactAlign;
                    AlignPoint(posture, handle, impact);
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
            
            Point start = posture.Points[constraint.Start];
            Point end = posture.Points[constraint.End];
            
            Point result = GeometryHelper.GetClosestPoint(start, end, point, constraint.AllowedPosition, constraint.Margin);

            posture.Points[posture.Handles[handle].Reference] = result;
        }
        private static void AlignPoint(GenericPosture posture, int handle, GenericPostureImpactAlign impact)
        {
            if(impact == null)
                return;
            
            Point start = posture.Points[handle];
            Point end = posture.Points[impact.AlignWith];
            
            Point result = GeometryHelper.GetClosestPoint(start, end, posture.Points[impact.PointToAlign], PointLinePosition.OnSegment, 10);
            
            posture.Points[impact.PointToAlign] = result;
        }
    }
}
