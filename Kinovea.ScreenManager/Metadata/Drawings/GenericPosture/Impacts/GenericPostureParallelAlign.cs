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
using System.Collections.Generic;
using System.Xml;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class GenericPostureParallelAlign : GenericPostureAbstractImpact
    {
        public int PointToMove { get; private set;}
        public int A { get; private set;}
        public int B { get; private set;}
        public int C { get; private set;}
        
        public GenericPostureParallelAlign(XmlReader r)
        {
            Type = ImpactType.ParallelAlign;
            
            bool isEmpty = r.IsEmptyElement;
            
            if(r.MoveToAttribute("pointToMove"))
                PointToMove = r.ReadContentAsInt();
            
            if(r.MoveToAttribute("a"))
                A = r.ReadContentAsInt();
            
            if(r.MoveToAttribute("b"))
                B = r.ReadContentAsInt();
            
            if(r.MoveToAttribute("c"))
                C = r.ReadContentAsInt();
            
            r.ReadStartElement();
            
            //if(!isEmpty)
            //    r.ReadEndElement();
        }
    }
}


