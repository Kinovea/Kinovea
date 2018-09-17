#region License
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
using System.Collections.Generic;
using System.Xml;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class GenericPostureImpactPivot : GenericPostureAbstractImpact
    {
        public int Pivot { get; private set;}
        public List<int> Impacted { get; private set;}
        
        public GenericPostureImpactPivot(XmlReader r)
        {
            Type = ImpactType.Pivot;
            Impacted = new List<int>();
            
            // <Pivot pivot="1" impacted="3;4;5"/>
            bool isEmpty = r.IsEmptyElement;
            
            if(r.MoveToAttribute("pivot"))
                Pivot = r.ReadContentAsInt();
            
            if(r.MoveToAttribute("impacted"))
                 Impacted = XmlHelper.ParseIntList(r.ReadContentAsString());
            
            r.ReadStartElement();
            
            //if(!isEmpty)
            //    r.ReadEndElement();
        }
    }
}


