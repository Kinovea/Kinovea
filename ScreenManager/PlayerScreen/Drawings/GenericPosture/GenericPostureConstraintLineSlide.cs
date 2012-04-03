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
using System.Xml;

namespace Kinovea.ScreenManager
{
    public class GenericPostureConstraintLineSlide : GenericPostureAbstractConstraint
    {
        public int Start { get; private set;}
        public int End { get; private set;}
        public PointLinePosition AllowedPosition { get; private set;}
        public int Margin { get; private set;}
        
        public GenericPostureConstraintLineSlide(XmlReader r)
        {
            // <LineSlide point1="0" point2="2" position="Inbetween"/>
            Type = ConstraintType.LineSlide;
            Margin = 10;
            
            bool isEmpty = r.IsEmptyElement;
            
            if(r.MoveToAttribute("point1"))
                Start = r.ReadContentAsInt();
            
            if(r.MoveToAttribute("point2"))
                End = r.ReadContentAsInt();
            
            if(r.MoveToAttribute("position"))
                AllowedPosition = (PointLinePosition) Enum.Parse(typeof(PointLinePosition), r.ReadContentAsString());
            
            r.ReadStartElement();
            
            //if(!isEmpty)
            //    r.ReadEndElement();
        }
    }
}
