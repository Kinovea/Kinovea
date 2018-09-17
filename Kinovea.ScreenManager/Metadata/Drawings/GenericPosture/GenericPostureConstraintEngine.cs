﻿#region License
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
using System.Drawing;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    public static class GenericPostureConstraintEngine
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public static void MoveHandle(GenericPosture posture, CalibrationHelper calibrationHelper, int handle, PointF point, Keys modifiers)
        {
            try
            {
                // Update the point(s) attached to the handle based on the constraints.
                // Update all other points that may have been impacted.
                switch(posture.Handles[handle].Type)
                {
                    case HandleType.Point:
                        MovePointHandle(posture, calibrationHelper, handle, point, modifiers);
                        break;
                    case HandleType.Segment:
                        MoveSegmentHandle(posture, calibrationHelper, handle, point);
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
        private static void MovePointHandle(GenericPosture posture, CalibrationHelper calibrationHelper, int handle, PointF point, Keys modifiers)
        {
            // Constraints. (position of the point managed by this handle).
            GenericPostureAbstractConstraint constraint = posture.Handles[handle].Constraint;
            PointF old = posture.Points[posture.Handles[handle].Reference];
            PrepareImpacts(posture, handle);
            
            if(constraint == null || 
               (!string.IsNullOrEmpty(constraint.OptionGroup) && !posture.OptionGroups[constraint.OptionGroup]))
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
                        MovePointHandleAlongVertical(posture, calibrationHelper, handle, point);
                        break;
                    case ConstraintType.HorizontalSlide:
                        MovePointHandleAlongHorizontal(posture, handle, point);
                        break;
                    case ConstraintType.DistanceToPoint:
                        MovePointHandleAtDistance(posture, handle, point, constraint as GenericPostureConstraintDistanceToPoint, modifiers);
                        break;
                    case ConstraintType.RotationSteps:
                        MovePointHandleByRotationSteps(posture, calibrationHelper, handle, point, constraint as GenericPostureConstraintRotationSteps);
                        break;
                    case ConstraintType.PerpendicularSlide:
                        MovePointHandleAlongPerpendicular(posture, calibrationHelper, handle, point, constraint as GenericPostureConstraintPerpendicularSlide);
                        break;
                    case ConstraintType.ParallelSlide:
                        MovePointHandleAlongParallel(posture, calibrationHelper, handle, point, constraint as GenericPostureConstraintParallelSlide);
                        break;
                }
            }
            
            ProcessPointImpacts(posture, calibrationHelper, handle, old);
        }
        
        private static void ProcessPointImpacts(GenericPosture posture, CalibrationHelper calibrationHelper, int handle, PointF old)
        {
            foreach(GenericPostureAbstractImpact impact in posture.Handles[handle].Impacts)
                ProcessPointImpact(posture, calibrationHelper, impact, handle, old);
        }
        private static void ProcessPointImpact(GenericPosture posture, CalibrationHelper calibrationHelper, GenericPostureAbstractImpact impact, int handle, PointF old)
        {
            switch(impact.Type)
            {
                case ImpactType.LineAlign:
                    AlignPointSegment(posture, impact as GenericPostureImpactLineAlign);
                    break;
                case ImpactType.VerticalAlign:
                    AlignPointVertical(posture, calibrationHelper, handle, impact as GenericPostureImpactVerticalAlign);
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
                        PivotPoints(posture, radians, impact as GenericPostureImpactPivot);
                    }
                    break;
                case ImpactType.KeepAngle:
                    KeepPointAngle(posture, impact as GenericPostureImpactKeepAngle);
                    break;
                case ImpactType.SegmentCenter:
                    SegmentCenter(posture, calibrationHelper, impact as GenericPostureImpactSegmentCenter);
                    break;
                case ImpactType.PerdpendicularAlign:
                    AlignPointPerpendicular(posture, calibrationHelper, impact as GenericPosturePerpendicularAlign);
                    break;
                case ImpactType.ParallelAlign:
                    AlignPointParallel(posture, calibrationHelper, impact as GenericPostureParallelAlign);
                    break;
            }
        }
        
        private static void MoveSegmentHandle(GenericPosture posture, CalibrationHelper calibrationHelper, int handle, PointF point)
        {
            int start = posture.Segments[posture.Handles[handle].Reference].Start;
            int end = posture.Segments[posture.Handles[handle].Reference].End;
            PointF oldStart = posture.Points[start];
            PointF oldEnd = posture.Points[end];
        
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
                        MoveSegmentSymmetrically(posture, impact as GenericPostureImpactHorizontalSymmetry);
                        break;
                } 
            }
            
            // Check if segments start and end points are handles, and apply impacts.
            foreach(GenericPostureHandle h in posture.Handles)
            {
                if(h.Type != HandleType.Point)
                    continue;
                
                int handleReference = h.Reference;

                if(handleReference == start)
                {
                    PrepareImpacts(posture, handleReference);
                    ProcessPointImpacts(posture, calibrationHelper, handleReference, oldStart);
                }
                else if(h.Reference == end)
                {
                    PrepareImpacts(posture, handleReference);
                    ProcessPointImpacts(posture, calibrationHelper, handleReference, oldEnd);
                }
            }
        }
        private static void MoveEllipseHandle(GenericPosture posture, int handle, PointF point)
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
        private static void MovePointHandleFreely(GenericPosture posture, int handle, PointF point)
        {
            posture.Points[posture.Handles[handle].Reference] = point;
        }
        private static void MoveSegmentHandleFreely(GenericPosture posture, int handle, PointF point)
        {
            Vector v = new Vector(posture.Handles[handle].GrabPoint, point);
            TranslateSegmentHandle(posture, handle, v);
        }
        private static void MoveEllipseHandleFreely(GenericPosture posture, int handle, PointF point)
        {
            PointF center = posture.Points[posture.Ellipses[posture.Handles[handle].Reference].Center];
            Vector v = new Vector(center, point);
            float radius = v.Norm();
            posture.Ellipses[posture.Handles[handle].Reference].Radius = (int)radius;
        }
        private static void MovePointHandleAlongLine(GenericPosture posture, int handle, PointF point, GenericPostureConstraintLineSlide constraint)
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
        private static void MovePointHandleAlongVertical(GenericPosture posture, CalibrationHelper calibrationHelper, int handle, PointF point)
        {
            /*PointF source = calibrationHelper.GetPoint(posture.Points[posture.Handles[handle].Reference]);
            PointF target = calibrationHelper.GetPoint(point.ToPointF());
        
            PointF result = new PointF(source.X, target.Y);
            PointF resultImage = calibrationHelper.GetImagePoint(result);
            
            posture.Points[posture.Handles[handle].Reference] = resultImage;*/
            
            posture.Points[posture.Handles[handle].Reference] = new PointF(posture.Points[posture.Handles[handle].Reference].X, point.Y);
        }
        private static void MovePointHandleAlongHorizontal(GenericPosture posture, int handle, PointF point)
        {
            posture.Points[posture.Handles[handle].Reference] = new PointF(point.X, posture.Points[posture.Handles[handle].Reference].Y);
        }
        private static void MoveSegmentHandleAlongHorizontal(GenericPosture posture, int handle, PointF point)
        {
            Vector moved = new Vector(posture.Handles[handle].GrabPoint, point);
            Vector v = new Vector(moved.X, 0);
            TranslateSegmentHandle(posture, handle, v);
        }
        private static void MoveSegmentHandleAlongVertical(GenericPosture posture, int handle, PointF point)
        {
            Vector moved = new Vector(posture.Handles[handle].GrabPoint, point);
            Vector v = new Vector(0, moved.Y);
            TranslateSegmentHandle(posture, handle, v);
        }
        private static void MovePointHandleAtDistance(GenericPosture posture, int handle, PointF point, GenericPostureConstraintDistanceToPoint constraint, Keys modifiers)
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
            
            int constraintAngleSubdivisions = 8; // (Constraint by 45° steps).
            
            if((modifiers & Keys.Shift) == Keys.Shift)
                posture.Points[posture.Handles[handle].Reference] = GeometryHelper.GetPointAtClosestRotationStepCardinal(parent, temp, constraintAngleSubdivisions);
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
        private static void MovePointHandleByRotationSteps(GenericPosture posture, CalibrationHelper calibrationHelper, int handle, PointF point, GenericPostureConstraintRotationSteps constraint)
        {
            if(constraint == null)
                return;
            
            PointF parent = posture.Points[constraint.Origin];
            PointF leg1 = posture.Points[constraint.Leg1];
            
            if(parent == leg1 || constraint.Step == 0)
                return;
            
            int constraintAngleSubdivisions = 360/constraint.Step;
            posture.Points[posture.Handles[handle].Reference] = GeometryHelper.GetPointAtClosestRotationStep(parent, leg1, point, constraintAngleSubdivisions);
        }
        private static void MovePointHandleAlongPerpendicular(GenericPosture posture, CalibrationHelper calibrationHelper, int handle, PointF point, GenericPostureConstraintPerpendicularSlide constraint)
        {
            if(constraint == null)
                return;
            
            PointF pivot = posture.Points[constraint.Origin];
            PointF leg1 = posture.Points[constraint.Leg1];
            
            if(pivot == leg1)
                return;
            
            PointF pivotPlane = calibrationHelper.GetPoint(pivot);
            PointF leg1Plane = calibrationHelper.GetPoint(leg1);
            PointF pointPlane = calibrationHelper.GetPoint(point);
            
            PointF resultPlane = GeometryHelper.GetPointAtAngle(pivotPlane, leg1Plane, pointPlane, 90);
            PointF result = calibrationHelper.GetImagePoint(resultPlane);
            
            posture.Points[posture.Handles[handle].Reference] = result;
        }
        private static void MovePointHandleAlongParallel(GenericPosture posture, CalibrationHelper calibrationHelper, int handle, PointF point, GenericPostureConstraintParallelSlide constraint)
        {
            if(constraint == null)
                return;
            
            PointF a = posture.Points[constraint.A];
            PointF b = posture.Points[constraint.B];
            PointF c = posture.Points[constraint.C];
            
            PointF aPlane = calibrationHelper.GetPoint(a);
            PointF bPlane = calibrationHelper.GetPoint(b);
            PointF cPlane = calibrationHelper.GetPoint(c);
            PointF pointPlane = calibrationHelper.GetPoint(point);
            
            PointF resultPlane = GeometryHelper.GetPointOnParallel(aPlane, bPlane, cPlane, pointPlane);
            PointF result = calibrationHelper.GetImagePoint(resultPlane);
            
            posture.Points[posture.Handles[handle].Reference] = result;
        }
        #endregion

        #region Impacts
        private static void PrepareImpacts(GenericPosture posture, int handle)
        {
            foreach(GenericPostureAbstractImpact impact in posture.Handles[handle].Impacts)
            {
                // If there is a KeepAngle impact, we'll later need to know the current angle.
                if(impact.Type == ImpactType.KeepAngle)
                {
                    GenericPostureImpactKeepAngle impactKeepAngle = impact as GenericPostureImpactKeepAngle;
                    
                    PointF origin = posture.Points[impactKeepAngle.Origin];
                    PointF leg1 = posture.Points[impactKeepAngle.Leg1];
                    PointF leg2 = posture.Points[impactKeepAngle.Leg2];
                    
                    if(origin == leg1 || origin == leg2)
                        continue;
                    
                    impactKeepAngle.OldAngle = GeometryHelper.GetAngle(origin, leg1, leg2);
                    impactKeepAngle.OldDistance = GeometryHelper.GetDistance(origin, leg2);
                }
            }
        }
        private static void AlignPointSegment(GenericPosture posture, GenericPostureImpactLineAlign impact)
        {
            if(impact == null)
                return;
            
            PointF start = posture.Points[impact.Start];
            PointF end = posture.Points[impact.End];
            
            if(start == end)
            {
                posture.Points[impact.PointToAlign] = start;
                return;
            }
            
            PointF result = GeometryHelper.GetClosestPoint(start, end, posture.Points[impact.PointToAlign], PointLinePosition.Anywhere, 10);
            posture.Points[impact.PointToAlign] = result;
        }
        private static void AlignPointVertical(GenericPosture posture, CalibrationHelper calibrationHelper, int handle, GenericPostureImpactVerticalAlign impact)
        {
            if(impact == null)
                return;
            
            /*PointF impacted = calibrationHelper.GetPoint(posture.Points[impact.PointRef]);
            PointF impacting = calibrationHelper.GetPoint(posture.Points[posture.Handles[handle].Reference]);
            
            PointF result = calibrationHelper.GetImagePoint(new PointF(impacted.X, impacting.Y));
            posture.Points[impact.PointRef] = result;*/
            
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
        private static void PivotPoints(GenericPosture posture, float radians, GenericPostureImpactPivot impact)
        {
            // Rotates a series of point around a pivot point.
            PointF pivot = posture.Points[impact.Pivot];
            
            foreach(int pointRef in impact.Impacted)
            {
                posture.Points[pointRef] = GeometryHelper.Pivot(pivot, posture.Points[pointRef], radians);
            }
        }
        private static void MoveSegmentSymmetrically(GenericPosture posture, GenericPostureImpactHorizontalSymmetry impact)
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
        private static void KeepPointAngle(GenericPosture posture, GenericPostureImpactKeepAngle impact)
        {
            // The point is moved so that the angle between its leg and the other leg is kept.
            if(impact == null)
                return;
            
            PointF origin = posture.Points[impact.Origin];
            PointF leg1 = posture.Points[impact.Leg1];
            
            if(origin == leg1)
                return;
             
            PointF result = GeometryHelper.GetPointAtAngleAndDistance(origin, leg1, impact.OldAngle, impact.OldDistance);
            posture.Points[impact.Leg2] = result;
        }
        private static void SegmentCenter(GenericPosture posture, CalibrationHelper calibrationHelper, GenericPostureImpactSegmentCenter impact)
        {
            // The point is moved so that it stays at the center of the specified segment.
            // This should take perspective into account.
            
            if(impact == null)
                return;
            
            PointF p1 = posture.Points[impact.Point1];
            PointF p2 = posture.Points[impact.Point2];
            
            PointF p1Plane = calibrationHelper.GetPoint(p1);
            PointF p2Plane = calibrationHelper.GetPoint(p2);
            
            PointF resultPlane = GeometryHelper.GetMiddlePoint(p1Plane, p2Plane);
            
            PointF result = calibrationHelper.GetImagePoint(resultPlane);
            
            posture.Points[impact.PointToMove] = result;
        }
        private static void AlignPointPerpendicular(GenericPosture posture, CalibrationHelper calibrationHelper, GenericPosturePerpendicularAlign impact)
        {
            // The point is moved so that it stays on a perpendicular segment relatively to another segment.
            
            if(impact == null)
                return;
            
            PointF pivot = posture.Points[impact.Origin];
            PointF leg1 = posture.Points[impact.Leg1];
            PointF pointToMove = posture.Points[impact.PointToMove];
            
            if(pivot == leg1)
                return;
            
            PointF pivotPlane = calibrationHelper.GetPoint(pivot);
            PointF leg1Plane = calibrationHelper.GetPoint(leg1);
            PointF pointPlane = calibrationHelper.GetPoint(pointToMove);
            
            PointF resultPlane = GeometryHelper.GetPointAtAngle(pivotPlane, leg1Plane, pointPlane, 90);
            PointF result = calibrationHelper.GetImagePoint(resultPlane);
            
            posture.Points[impact.PointToMove] = result;
        }
        private static void AlignPointParallel(GenericPosture posture, CalibrationHelper calibrationHelper, GenericPostureParallelAlign impact)
        {
            // The point is moved so that it stays on a segment parallel to another segment.
            
            if(impact == null)
                return;
            
            PointF a = posture.Points[impact.A];
            PointF b = posture.Points[impact.B];
            PointF c = posture.Points[impact.C];
            PointF pointToMove = posture.Points[impact.PointToMove];
            
            PointF aPlane = calibrationHelper.GetPoint(a);
            PointF bPlane = calibrationHelper.GetPoint(b);
            PointF cPlane = calibrationHelper.GetPoint(c);
            PointF pointPlane = calibrationHelper.GetPoint(pointToMove);
            
            PointF resultPlane = GeometryHelper.GetPointOnParallel(aPlane, bPlane, cPlane, pointPlane);
            PointF result = calibrationHelper.GetImagePoint(resultPlane);
            
            posture.Points[impact.PointToMove] = result;
        }
        #endregion

    }
}
