﻿#region License
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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public static void MoveHandle(GenericPosture posture, int handle, Point point)
        {
            try
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
            catch(Exception e)
            {
                log.DebugFormat("Error while moving handle");
                log.DebugFormat(e.ToString());
            }
        }
        private static void MovePointHandle(GenericPosture posture, int handle, Point point)
        {
            // Constraints. (position of the point managed by this handle).
            GenericPostureAbstractConstraint constraint = posture.Handles[handle].Constraint;
            PointF old = posture.Points[posture.Handles[handle].RefPoint];
            
            if(constraint == null)
            {
                MovePointHandleFreely(posture, handle, point);
            }
            else
            {
                switch(constraint.Type)
                {
                    case ConstraintType.None:
                        MovePointHandleFreely(posture, handle, point);
                        break;
                    case ConstraintType.LineSlide:
                        MovePointHandleAlongLine(posture, handle, point, constraint as GenericPostureConstraintLineSlide);
                        break;
                    case ConstraintType.DistanceToPoint:
                        MovePointHandleAtDistance(posture, handle, point, constraint as GenericPostureConstraintDistanceToPoint);
                        break;
                }
            }
            
            // Impacts. (positions of other points).
            foreach(GenericPostureAbstractImpact impact in posture.Handles[handle].Impacts)
            {
                switch(impact.Type)
                {
                    case ImpactType.Align:
                        AlignPoint(posture, handle, impact as GenericPostureImpactAlign);
                        break;
                    case ImpactType.Pivot:
                        // Get rotation that was applied. Apply same rotation on all points.
                        GenericPostureImpactPivot impactPivot = impact as GenericPostureImpactPivot;
                        if(impact != null)
                        {
                            PointF a = posture.Points[impactPivot.Pivot];
                            PointF b = old;
                            PointF c = posture.Points[posture.Handles[handle].RefPoint];
                            float radians = GeometryHelper.GetAngle(a, b, c);
                            PivotPoints(posture, handle, radians, impact as GenericPostureImpactPivot);
                        }
                        break;
                } 
            }
            
        }
      
        #region Constraints
        private static void MovePointHandleFreely(GenericPosture posture, int handle, Point point)
        {
            posture.Points[posture.Handles[handle].RefPoint] = point;
        }
        private static void MovePointHandleAlongLine(GenericPosture posture, int handle, Point point, GenericPostureConstraintLineSlide constraint)
        {
            if(constraint == null)
            {
                MovePointHandleFreely(posture, handle, point);
                return;
            }
            
            PointF start = posture.Points[constraint.Start];
            PointF end = posture.Points[constraint.End];
            
            posture.Points[posture.Handles[handle].RefPoint] = GeometryHelper.GetClosestPoint(start, end, point, constraint.AllowedPosition, constraint.Margin);
        }
        private static void MovePointHandleAtDistance(GenericPosture posture, int handle, Point point, GenericPostureConstraintDistanceToPoint constraint)
        {
            if(constraint == null)
            {
                MovePointHandleFreely(posture, handle, point);
                return;
            }
            
            PointF parent = posture.Points[constraint.RefPoint];
            PointF child = posture.Points[posture.Handles[handle].RefPoint];
            float distance = GeometryHelper.GetDistance(parent, child);

            posture.Points[posture.Handles[handle].RefPoint] = GeometryHelper.GetPointAtDistance(parent, point, distance);
        }
        #endregion

        #region Impacts
        private static void AlignPoint(GenericPosture posture, int handle, GenericPostureImpactAlign impact)
        {
            if(impact == null)
                return;
            
            PointF start = posture.Points[handle];
            PointF end = posture.Points[impact.AlignWith];
            
            PointF result = GeometryHelper.GetClosestPoint(start, end, posture.Points[impact.PointToAlign], PointLinePosition.Anywhere, 10);
            posture.Points[impact.PointToAlign] = result;
        }
        private static void PivotPoints(GenericPosture posture, int handle, float radians, GenericPostureImpactPivot impact)
        {
            // Rotates a series of point around a pivot point.
            PointF pivot = posture.Points[impact.Pivot];
            
            foreach(int pointRef in impact.Impacted)
            {
                posture.Points[pointRef] = GeometryHelper.Pivot(pivot, posture.Points[pointRef], radians);
            }
        }
        #endregion
    }
}
