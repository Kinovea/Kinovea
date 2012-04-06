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
using System.Xml;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class GenericPostureHitZonePolygon : GenericPostureAbstractHitZone
    {
        public List<int> Points { get; private set;}
        
        public GenericPostureHitZonePolygon(XmlReader r)
        {
            Type = HitZoneType.Polygon;
            
            // <Polygon points="0;1;2;3;4"/>
            bool isEmpty = r.IsEmptyElement;
            
            if(r.MoveToAttribute("points"))
                Points = XmlHelper.ParseIntList(r.ReadContentAsString());
            
            r.ReadStartElement();
            
            //if(!isEmpty)
            //    r.ReadEndElement();
        }
    }
}

