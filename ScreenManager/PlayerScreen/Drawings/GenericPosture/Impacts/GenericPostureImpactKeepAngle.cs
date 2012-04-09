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
    public class GenericPostureImpactKeepAngle : GenericPostureAbstractImpact
    {
        public int Origin { get; private set;}
        public int Leg1 { get; private set;}
        public int Leg2 { get; private set;}
        
        // Used temporarily during impact computation.
        public float OldAngle { get; set;}
        public float OldDistance { get; set;}
        
        public GenericPostureImpactKeepAngle(XmlReader r)
        {
            Type = ImpactType.KeepAngle;
            
            // <KeepAngle origin="1" leg1="0" leg2="2"/>
            bool isEmpty = r.IsEmptyElement;
            
            if(r.MoveToAttribute("origin"))
                Origin = r.ReadContentAsInt();
            
            if(r.MoveToAttribute("leg1"))
                Leg1 = r.ReadContentAsInt();
            
            if(r.MoveToAttribute("leg2"))
                Leg2 = r.ReadContentAsInt();
            
            r.ReadStartElement();
            
            //if(!isEmpty)
            //    r.ReadEndElement();
        }
    }
}


