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
    public class GenericPostureImpactAlign : GenericPostureAbstractImpact
    {
        public int PointToAlign { get; private set;}
        public int AlignWith { get; private set;}
        
        public GenericPostureImpactAlign(XmlReader r)
        {
            Type = ImpactType.Align;
            
            // <Align pointToAlign="1" alignWith="0"/>
            bool isEmpty = r.IsEmptyElement;
            
            if(r.MoveToAttribute("pointToAlign"))
                PointToAlign = r.ReadContentAsInt();
            
            if(r.MoveToAttribute("alignWith"))
                AlignWith = r.ReadContentAsInt();
            
            r.ReadStartElement();
            
            //if(!isEmpty)
            //    r.ReadEndElement();
        }
    }
}

