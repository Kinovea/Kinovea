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
    public class GenericPostureSegment
    {
        public int Start { get; private set;}
        public int End { get; private set;}
        
        private string name;
        private string style;
        private int width;
        
        public GenericPostureSegment(int start, int end)
        {
            this.Start = start;
            this.End = end;
        }
        public GenericPostureSegment(XmlReader r)
        {
            //<Segment point1="0" point2="1" name="" style="Solid" width="1"/>
                
            bool isEmpty = r.IsEmptyElement;
            
            if(r.MoveToAttribute("point1"))
                Start = r.ReadContentAsInt();
            
            if(r.MoveToAttribute("point2"))
                End = r.ReadContentAsInt();

            if(r.MoveToAttribute("name"))
                name = r.ReadContentAsString();

            if(r.MoveToAttribute("style"))
                style = r.ReadContentAsString();
            
            if(r.MoveToAttribute("width"))
                width = r.ReadContentAsInt();

            r.ReadStartElement();
            
            if(isEmpty)
                return;
            
            // Read sub elements.
            //r.ReadEndElement();
        }
    }
}