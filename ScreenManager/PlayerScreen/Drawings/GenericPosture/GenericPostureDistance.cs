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
using System.Drawing;
using System.Xml;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class GenericPostureDistance
    {
        public int Point1 { get; private set;}
        public int Point2 { get; private set;}
        public string Symbol { get; private set;}
        public Color Color { get; private set; }
        
        public GenericPostureDistance(XmlReader r)
        {
            //<Distance point1="2" point2="4" />
            Color = Color.Transparent;
            
            bool isEmpty = r.IsEmptyElement;
            
            if(r.MoveToAttribute("point1"))
                Point1 = r.ReadContentAsInt();
            
            if(r.MoveToAttribute("point2"))
                Point2 = r.ReadContentAsInt();
            
            if(r.MoveToAttribute("symbol"))
                Symbol = r.ReadContentAsString();
            
            if(r.MoveToAttribute("color"))
                Color = XmlHelper.ParseColor(r.ReadContentAsString(), Color);

            r.ReadStartElement();
            
            if(isEmpty)
                return;
            
        }
    }
}