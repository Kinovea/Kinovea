#region License
/*
Copyright © Joan Charmant 2018.
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
using System.Drawing;
using System.Xml;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class GenericPosturePoint
    {
        public string Name { get; private set; }
        public PointF Value { get; private set; }
        public Color Color { get; private set; }
        
        public GenericPosturePoint(XmlReader r)
        {
            Name = "";
            Value = PointF.Empty;
            Color = Color.Transparent;
            
            bool isEmpty = r.IsEmptyElement;

            if (r.MoveToAttribute("name"))
                Name = r.ReadContentAsString();

            if (r.MoveToAttribute("value"))
                Value = XmlHelper.ParsePointF(r.ReadContentAsString());
            
            if (r.MoveToAttribute("color"))
                Color = XmlHelper.ParseColor(r.ReadContentAsString(), Color);
            
            r.ReadStartElement();

            if (isEmpty)
                return;

            // Read sub elements.
            //r.ReadEndElement();
        }

        public GenericPosturePoint(PointF value)
        {
            this.Name = "";
            this.Value = value;
            this.Color = Color.Transparent;
        }
    }
}