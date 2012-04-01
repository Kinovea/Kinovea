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

namespace Kinovea.ScreenManager
{
    public class GenericPostureHandle
    {
        public HandleType Type { get; private set;}
        public int Reference { get; private set;}
        public ConstraintType ConstraintType { get; private set;}
        public GenericPostureAbstractConstraint Constraint { get; private set;}
        public ImpactType ImpactType { get; private set;}
        public GenericPostureAbstractImpact Impact { get; private set;}
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public GenericPostureHandle(XmlReader r)
        {
            //<Handle type="Point" reference="0">
            //  <Constraint type="LineSlide">
            //    <LineSlide point1="0" point2="2" position="Inbetween"/>
            //  </Constraint>
            //  <Impact type="Align">
            //    <Align pointToAlign="1" AlignWith="2"/>
            //  </Impact>
            //</Handle>
            
            ConstraintType = ConstraintType.None;
            Constraint = null;
            ImpactType = ImpactType.None;
            Impact = null;
            
            bool isEmpty = r.IsEmptyElement;
            
            if(r.MoveToAttribute("type"))
                Type = (HandleType) Enum.Parse(typeof(HandleType), r.ReadContentAsString());
            
            if(r.MoveToAttribute("reference"))
                Reference = r.ReadContentAsInt();
            
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
                    ParseImpact(r);
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
            
            if(r.MoveToAttribute("type"))
                ConstraintType = (ConstraintType) Enum.Parse(typeof(ConstraintType), r.ReadContentAsString());
            
            r.ReadStartElement();
            
            switch(ConstraintType)
            {
                case ConstraintType.None:
                    Constraint = null;
                    break;
                case ConstraintType.LineSlide:
                    Constraint = new GenericPostureConstraintLineSlide(r);
                    break;
                default:
                    string outerXml = r.ReadOuterXml();
                    log.DebugFormat("Unparsed content: {0}", outerXml);
                    break;
            }
            
            if(!isEmpty)
                r.ReadEndElement();
        }
        private void ParseImpact(XmlReader r)
        {
            // An "impact" reprsent how other points are constrained by the position of this handle.
            bool isEmpty = r.IsEmptyElement;
            
            if(r.MoveToAttribute("type"))
                ImpactType = (ImpactType) Enum.Parse(typeof(ImpactType), r.ReadContentAsString());
            
            r.ReadStartElement();
            
            switch(ImpactType)
            {
                case ImpactType.None:
                    Impact = null;
                    break;
                case ImpactType.Align:
                    Impact = new GenericPostureImpactAlign(r);
                    break;
                default:
                    string outerXml = r.ReadOuterXml();
                    log.DebugFormat("Unparsed content: {0}", outerXml);
                    break;
            }
            
            if(!isEmpty)
                r.ReadEndElement();
        }
    }
}