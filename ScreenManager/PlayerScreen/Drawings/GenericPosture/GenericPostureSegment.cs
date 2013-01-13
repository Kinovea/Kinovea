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

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class GenericPostureSegment
    {
        public int Start { get; private set;}
        public int End { get; private set;}
        public SegmentLineStyle Style { get; private set;}
        public int Width { get; private set;}
        public bool ArrowEnd { get; private set; }
        public bool ArrowBegin { get; private set; }
        
        private string name;
        
        public GenericPostureSegment(XmlReader r)
        {
            //<Segment point1="0" point2="1" name="" style="Solid" width="1"/>
            Width = 2;
            Style = SegmentLineStyle.Solid;
            
            bool isEmpty = r.IsEmptyElement;
            
            if(r.MoveToAttribute("point1"))
                Start = r.ReadContentAsInt();
            
            if(r.MoveToAttribute("point2"))
                End = r.ReadContentAsInt();

            if(r.MoveToAttribute("name"))
                name = r.ReadContentAsString();

            if(r.MoveToAttribute("style"))
                Style = (SegmentLineStyle) Enum.Parse(typeof(SegmentLineStyle), r.ReadContentAsString());
            
            if(r.MoveToAttribute("width"))
                Width = r.ReadContentAsInt();

            if(r.MoveToAttribute("arrowBegin"))
                ArrowBegin = XmlHelper.ParseBoolean(r.ReadContentAsString());
            
            if(r.MoveToAttribute("arrowEnd"))
                ArrowEnd = XmlHelper.ParseBoolean(r.ReadContentAsString());
            
            r.ReadStartElement();
            
            if(isEmpty)
                return;
            
            // Read sub elements.
            //r.ReadEndElement();
        }
    }
}