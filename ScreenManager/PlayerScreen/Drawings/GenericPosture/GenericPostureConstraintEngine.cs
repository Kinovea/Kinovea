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
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    public static class GenericPostureConstraintEngine
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public static void MoveHandle(GenericPosture posture, int handle, Point point, Keys modifiers)
        {
            try
            {
                // Update the point(s) attached to the handle based on the constraints.
                // Update all other points that may have been impacted.
                switch(posture.Handles[handle].Type)
                {
                    case HandleType.Point:
                        MovePointHandle(posture, handle, point, modifiers);
                        break;
                    case HandleType.Segment:
                        MoveSegmentHandle(posture, handle, point);
                        break;
                    case HandleType.Ellipse:
                        MoveEllipseHandle(posture, handle, point);
                        break;
                }
            }
            catch(Exception e)
            {
                log.DebugFormat("Error while moving handle");
                log.DebugFormat(e.ToString());
            }
        }
        private static void MovePointHandle(GenericPosture posture, int handle, Point point, Keys modifiers)
        {
            // Constraints. (position of the point managed by this handle).
            GenericPostureAbstractConstraint constraint = posture.Handles[handle].Constraint;
            PointF old = posture.Points[posture.Handles[handle].Reference];
            
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
                    case ConstraintType.VerticalSlide:
                        MovePointHandleAlongVertical(posture, handle, point);
                        break;
                    case ConstraintType.HorizontalSlide:
                        MovePointHandleAlongHorizontal(posture, handle, point);
                        break;
                    case ConstraintType.DistanceToPoint:
                        MovePointHandleAtDistance(posture, handle, point, constraint as GenericPostureConstraintDistanceToPoint, modifiers);
                        break;
                }
            }
            
            // Impacts. (positions of other points).
            foreach(GenericPostureAbstractImpact impact in posture.Handles[handle].Impacts)
            {
                switch(impact.Type)
                {
                    case ImpactType.LineAlign:
                        AlignPointSegment(posture, handle, impact as GenericPostureImpactLineAlign);
                        break;
                    case ImpactType.VerticalAlign:
                        AlignPointVertical(posture, handle, impact as GenericPostureImpactVerticalAlign);
                        break;
                    case ImpactType.HorizontalAlign:
                        AlignPointHorizontal(posture, handle, impact as GenericPostureImpactHorizontalAlign);
                        break;
                    case ImpactType.Pivot:
                        // Get rotation that was applied. Apply same rotation on all points.
                        GenericPostureImpactPivot impactPivot = impact as GenericPostureImpactPivot;
                        if(impact != null)
                        {
                            PointF a = posture.Points[impactPivot.Pivot];
                            PointF b = old;
                            PointF c = posture.Points[posture.Handles[handle].Reference];
                            float radians = GeometryHelper.GetAngle(a, b, c);
                            PivotPoints(posture, handle, radians, impact as GenericPostureImpactPivot);
                        }
                        break;
                } 
            }
            
        }
        private static void MoveSegmentHandle(GenericPosture posture, int handle, Point point)
        {
            // Constraints. (position of the point managed by this handle).
            GenericPostureAbstractConstraint constraint = posture.Handles[handle].Constraint;
            
            if(constraint == null)
            {
                MoveSegmentHandleFreely(posture, handle, point);
            }
            else
            {
                switch(constraint.Type)
                {
                    case ConstraintType.None:
                        MoveSegmentHandleFreely(posture, handle, point);
                        break;
                    case ConstraintType.HorizontalSlide:
                        MoveSegmentHandleAlongHorizontal(posture, handle, point);
                        break;
                    case ConstraintType.VerticalSlide:
                        MoveSegmentHandleAlongVertical(posture, handle, point);
                        break;
                }
            }
            
            // Impacts. (positions of other points).
            foreach(GenericPostureAbstractImpact impact in posture.Handles[handle].Impacts)
            {
                switch(impact.Type)
                {
                    case ImpactType.HorizontalSymmetry:
                        MoveSegmentSymmetrically(posture, handle, impact as GenericPostureImpactHorizontalSymmetry);
                        break;
                } 
            }
        }
        private static void MoveEllipseHandle(GenericPosture posture, int handle, Point point)
        {
            // Constraints. (position of the point managed by this handle).
            GenericPostureAbstractConstraint constraint = posture.Handles[handle].Constraint;
            
            if(constraint == null)
            {
                MoveEllipseHandleFreely(posture, handle, point);
            }
            else
            {
                switch(constraint.Type)
                {
                    case ConstraintType.None:
                        MoveEllipseHandleFreely(posture, handle, point);
                        break;
                }
            }
        }
        
        #region Constraints
        private static void MovePointHandleFreely(GenericPosture posture, int handle, Point point)
        {
            posture.Points[posture.Handles[handle].Reference] = point;
        }
        private static void MoveSegmentHandleFreely(GenericPosture posture, int handle, Point point)
        {
            Vector v = new Vector(posture.Handles[handle].GrabPoint, point);
            TranslateSegmentHandle(posture, handle, v);
        }
        private static void MoveEllipseHandleFreely(GenericPosture posture, int handle, Point point)
        {
            PointF center = posture.Points[posture.Ellipses[posture.Handles[handle].Reference].Center];
            Vector v = new Vector(center, point);
            float radius = v.Norm();
            posture.Ellipses[posture.Handles[handle].Reference].Radius = (int)radius;
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
            
            posture.Points[posture.Handles[handle].Reference] = GeometryHelper.GetClosestPoint(start, end, point, constraint.AllowedPosition, constraint.Margin);
        }
        private static void MovePointHandleAlongVertical(GenericPosture posture, int handle, Point point)
        {
            posture.Points[posture.Handles[handle].Reference] = new PointF(posture.Points[posture.Handles[handle].Reference].X, point.Y);
        }
        private static void MovePointHandleAlongHorizontal(GenericPosture posture, int handle, Point point)
        {
            posture.Points[posture.Handles[handle].Reference] = new PointF(point.X, posture.Points[posture.Handles[handle].Reference].Y);
        }
        private static void MoveSegmentHandleAlongHorizontal(GenericPosture posture, int handle, Point point)
        {
            Vector moved = new Vector(posture.Handles[handle].GrabPoint, point);
            Vector v = new Vector(moved.X, 0);
            TranslateSegmentHandle(posture, handle, v);
        }
        private static void MoveSegmentHandleAlongVertical(GenericPosture posture, int handle, Point point)
        {
            Vector moved = new Vector(posture.Handles[handle].GrabPoint, point);
            Vector v = new Vector(0, moved.Y);
            TranslateSegmentHandle(posture, handle, v);
        }
        private static void MovePointHandleAtDistance(GenericPosture posture, int handle, Point point, GenericPostureConstraintDistanceToPoint constraint, Keys modifiers)
        {
            if(constraint == null)
            {
                MovePointHandleFreely(posture, handle, point);
                return;
            }
            
            PointF parent = posture.Points[constraint.RefPoint];
            PointF child = posture.Points[posture.Handles[handle].Reference];
            float distance = GeometryHelper.GetDistance(parent, child);
            PointF temp = GeometryHelper.GetPointAtDistance(parent, point, distance);
            
            if((modifiers & Keys.Shift) == Keys.Shift)
                posture.Points[posture.Handles[handle].Reference] = GeometryHelper.GetPointAtConstraintAngle(parent, temp);
            else
                posture.Points[posture.Handles[handle].Reference] = temp;
        }
        private static void TranslateSegmentHandle(GenericPosture posture, int handle, Vector vector)
        {
            int start = posture.Segments[posture.Handles[handle].Reference].Start;
            int end = posture.Segments[posture.Handles[handle].Reference].End;
            
            TranslateSegment(posture, start, end, vector);
            
            posture.Handles[handle].GrabPoint = posture.Handles[handle].GrabPoint + vector;
        }
        private static void TranslateSegment(GenericPosture posture, int start, int end, Vector vector)
        {
            posture.Points[start] = posture.Points[start] + vector;
            posture.Points[end] = posture.Points[end] + vector;
        }
        #endregion

        #region Impacts
        private static void AlignPointSegment(GenericPosture posture, int handle, GenericPostureImpactLineAlign impact)
        {
            if(impact == null)
                return;
            
            PointF start = posture.Points[impact.Start];
            PointF end = posture.Points[impact.End];
            
            PointF result = GeometryHelper.GetClosestPoint(start, end, posture.Points[impact.PointToAlign], PointLinePosition.Anywhere, 10);
            posture.Points[impact.PointToAlign] = result;
        }
        private static void AlignPointVertical(GenericPosture posture, int handle, GenericPostureImpactVerticalAlign impact)
        {
            if(impact == null)
                return;
            
            PointF impacted = posture.Points[impact.PointRef];
            PointF impacting = posture.Points[posture.Handles[handle].Reference];
            
            posture.Points[impact.PointRef] = new PointF(impacted.X, impacting.Y);
        }
        private static void AlignPointHorizontal(GenericPosture posture, int handle, GenericPostureImpactHorizontalAlign impact)
        {
            if(impact == null)
                return;
            
            PointF impacted = posture.Points[impact.PointRef];
            PointF impacting = posture.Points[posture.Handles[handle].Reference];
            
            posture.Points[impact.PointRef] = new PointF(impacting.X, impacted.Y);
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
        private static void MoveSegmentSymmetrically(GenericPosture posture, int handle, GenericPostureImpactHorizontalSymmetry impact)
        {
            // Moves a segment so it is symmetric to another segment relatively to a vertical symmetry axis.
            
            GenericPostureSegment impacting = posture.Segments[impact.Impacting];
            GenericPostureSegment impacted = posture.Segments[impact.Impacted];
            GenericPostureSegment axis = posture.Segments[impact.Axis];
            
            Vector vStart = new Vector(posture.Points[impacting.Start], posture.Points[axis.Start]);
            posture.Points[impacted.Start] = new PointF(posture.Points[axis.Start].X + vStart.X, posture.Points[impacting.Start].Y);
            
            Vector vEnd = new Vector(posture.Points[impacting.End], posture.Points[axis.End]);
            posture.Points[impacted.End] = new PointF(posture.Points[axis.End].X + vEnd.X, posture.Points[impacting.End].Y);
        }
        
        #endregion

    }
}
