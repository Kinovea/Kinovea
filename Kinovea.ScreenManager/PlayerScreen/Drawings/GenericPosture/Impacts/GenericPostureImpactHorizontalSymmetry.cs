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
    /// <summary>
    /// A quite ad-hoc impact for when a vertical segment is moved, another vertical segment should be moved symmetrically relatively to a third vertical segment.
    /// Added for Posture tool.
    /// </summary>
    public class GenericPostureImpactHorizontalSymmetry : GenericPostureAbstractImpact
    {
        public int Impacted { get; private set;}
        public int Impacting { get; private set;}
        public int Axis { get; private set;}
        
        public GenericPostureImpactHorizontalSymmetry(XmlReader r)
        {
            Type = ImpactType.HorizontalSymmetry;
            
            bool isEmpty = r.IsEmptyElement;
            
            if(r.MoveToAttribute("impacting"))
                Impacting = r.ReadContentAsInt();
            
            if(r.MoveToAttribute("impacted"))
                Impacted = r.ReadContentAsInt();
            
            if(r.MoveToAttribute("axis"))
                Axis = r.ReadContentAsInt();
            
            r.ReadStartElement();
            
            //if(!isEmpty)
            //    r.ReadEndElement();
        }
    }
}




