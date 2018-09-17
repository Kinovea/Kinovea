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
using System.Xml;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// The target point must be kept at the center of a given segment. The impacting point would be the extremities of the segment.
    /// </summary>
    public class GenericPostureImpactSegmentCenter : GenericPostureAbstractImpact
    {
        public int PointToMove { get; private set;}
        public int Point1 { get; private set;}
        public int Point2 { get; private set;}

        // TODO: turn this into an amount of displacement relative to the segment length.
        // "center" becomes 0.5f, and we can specify others.

        public GenericPostureImpactSegmentCenter(XmlReader r)
        {
            Type = ImpactType.SegmentCenter;
            
            bool isEmpty = r.IsEmptyElement;
            
            if(r.MoveToAttribute("pointToMove"))
                PointToMove = r.ReadContentAsInt();
            
            if(r.MoveToAttribute("point1"))
                Point1 = r.ReadContentAsInt();
            
            if(r.MoveToAttribute("point2"))
                Point2 = r.ReadContentAsInt();
            
            r.ReadStartElement();
            
            //if(!isEmpty)
            //    r.ReadEndElement();
        }
    }
}




