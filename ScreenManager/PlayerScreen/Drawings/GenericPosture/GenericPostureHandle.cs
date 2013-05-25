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
using System.Collections.Generic;
using System.Drawing;
using System.Xml;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class GenericPostureHandle
    {
        public HandleType Type { get; private set;}
        public Color Color { get; private set; }
        public int Reference { get; private set;}
        public bool Trackable { get; private set;}
        public PointF GrabPoint { get; set;}
        public GenericPostureAbstractConstraint Constraint { get; private set;}
        public List<GenericPostureAbstractImpact> Impacts { get; private set;}
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public GenericPostureHandle(XmlReader r)
        {
            //<Handle type="Point" reference="0" trackable="true">
            //  <Constraint type="LineSlide">
            //    <LineSlide point1="0" point2="2" position="Inbetween"/>
            //  </Constraint>
            //  <Impact type="Align">
            //    <Align pointToAlign="1" AlignWith="2"/>
            //  </Impact>
            //</Handle>
            
            // TODO: maybe use the same pattern for Handle than for constraints, impacts and hit zones.
            
            Constraint = null;
            Impacts = new List<GenericPostureAbstractImpact>();
            Color = Color.Transparent;
            
            bool isEmpty = r.IsEmptyElement;
            
            if(r.MoveToAttribute("type"))
                Type = (HandleType) Enum.Parse(typeof(HandleType), r.ReadContentAsString());
            
            if(r.MoveToAttribute("reference"))
                Reference = r.ReadContentAsInt();
            
            if(r.MoveToAttribute("trackable"))
                Trackable = r.ReadContentAsBoolean();
            
            if(r.MoveToAttribute("color"))
                Color = XmlHelper.ParseColor(r.ReadContentAsString(), Color);
            
            r.ReadStartElement();
            
            if(isEmpty)
                return;
            
            while(r.NodeType == XmlNodeType.Element)
            {
                if(r.Name == "Constraint")
                {
                    ParseConstraint(r);
                }
                else if(r.Name == "Impact")
                {
                    ParseImpacts(r);
                }
                else if(r.Name == "Impacts")
                {
                    ParseImpacts(r);
                }
                else
                {
                    string outerXml = r.ReadOuterXml();
                    log.DebugFormat("Unparsed content: {0}", outerXml);
                }
            }
            
            r.ReadEndElement();
        }
        private void ParseConstraint(XmlReader r)
        {
            // A "constraint" represents the valid positions where the handle can go.
            bool isEmpty = r.IsEmptyElement;
            ConstraintType type = ConstraintType.None;
            
            if(r.MoveToAttribute("type"))
                type = (ConstraintType) Enum.Parse(typeof(ConstraintType), r.ReadContentAsString());
            
            r.ReadStartElement();
            
            switch(type)
            {
                case ConstraintType.None:
                    Constraint = null;
                    break;
                case ConstraintType.LineSlide:
                    Constraint = new GenericPostureConstraintLineSlide(r);
                    break;
                case ConstraintType.VerticalSlide:
                    Constraint = new GenericPostureConstraintVerticalSlide();
                    break;
                case ConstraintType.HorizontalSlide:
                    Constraint = new GenericPostureConstraintHorizontalSlide();
                    break;
                case ConstraintType.DistanceToPoint:
                    Constraint = new GenericPostureConstraintDistanceToPoint(r);
                    break;
                case ConstraintType.RotationSteps:
                    Constraint = new GenericPostureConstraintRotationSteps(r);
                    break;
                case ConstraintType.PerpendicularSlide:
                    Constraint = new GenericPostureConstraintPerpendicularSlide(r);
                    break;
                case ConstraintType.ParallelSlide:
                    Constraint = new GenericPostureConstraintParallelSlide(r);
                    break;
                default:
                    string outerXml = r.ReadOuterXml();
                    log.DebugFormat("Unparsed content: {0}", outerXml);
                    break;
            }
            
            if(!isEmpty)
                r.ReadEndElement();
        }
        private void ParseImpacts(XmlReader r)
        {
            r.ReadStartElement();
            
            while(r.NodeType == XmlNodeType.Element)
            {
                if(r.Name == "Align")
                {
                    GenericPostureImpactLineAlign impact = new GenericPostureImpactLineAlign(r);
                    if(impact != null)
                        Impacts.Add(impact);
                }
                else if(r.Name == "VerticalAlign")
                {
                    GenericPostureImpactVerticalAlign impact = new GenericPostureImpactVerticalAlign(r);
                    if(impact != null)
                        Impacts.Add(impact);
                }
                else if(r.Name == "HorizontalAlign")
                {
                    GenericPostureImpactHorizontalAlign impact = new GenericPostureImpactHorizontalAlign(r);
                    if(impact != null)
                        Impacts.Add(impact);
                }
                else if(r.Name == "Pivot")
                {
                    GenericPostureImpactPivot impact = new GenericPostureImpactPivot(r);
                    if(impact != null)
                        Impacts.Add(impact);
                }
                else if(r.Name == "KeepAngle")
                {
                    GenericPostureImpactKeepAngle impact = new GenericPostureImpactKeepAngle(r);
                    if(impact != null)
                        Impacts.Add(impact);
                }
                else if(r.Name == "HorizontalSymmetry")
                {
                    GenericPostureImpactHorizontalSymmetry impact = new GenericPostureImpactHorizontalSymmetry(r);
                    if(impact != null)
                        Impacts.Add(impact);
                }
                else if(r.Name == "SegmentCenter")
                {
                    GenericPostureImpactSegmentCenter impact = new GenericPostureImpactSegmentCenter(r);
                    if(impact != null)
                        Impacts.Add(impact);
                }
                else if(r.Name == "PerdpendicularAlign")
                {
                    GenericPosturePerpendicularAlign impact = new GenericPosturePerpendicularAlign(r);
                    if(impact != null)
                        Impacts.Add(impact);
                }
                else if(r.Name == "ParallelAlign")
                {
                    GenericPostureParallelAlign impact = new GenericPostureParallelAlign(r);
                    if(impact != null)
                        Impacts.Add(impact);
                }
                else
                {
                    string outerXml = r.ReadOuterXml();
                    log.DebugFormat("Unparsed content in XML: {0}", outerXml);
                }
            }
            
            r.ReadEndElement();
        }
    }
}