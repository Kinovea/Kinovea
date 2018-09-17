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
using System.Xml;

namespace Kinovea.ScreenManager
{
    public class GenericPostureImpactLineAlign : GenericPostureAbstractImpact
    {
        public int Start { get; private set;}
        public int End { get; private set;}
        public int PointToAlign { get; private set;}
        
        public GenericPostureImpactLineAlign(XmlReader r)
        {
            Type = ImpactType.LineAlign;
            
            bool isEmpty = r.IsEmptyElement;
            
            if(r.MoveToAttribute("pointToAlign"))
                PointToAlign = r.ReadContentAsInt();
            
            if(r.MoveToAttribute("point1"))
                Start = r.ReadContentAsInt();
            
            if(r.MoveToAttribute("point2"))
                End = r.ReadContentAsInt();
            
            r.ReadStartElement();
            
            //if(!isEmpty)
            //    r.ReadEndElement();
        }
    }
}

