#region License
/*
Copyright © Joan Charmant 2013.
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
    public class GenericPosturePosition
    {
        public int Point { get; private set;}
        public string Symbol { get; private set;}
        public Color Color { get; private set; }
        public string OptionGroup { get; private set;}
        
        public GenericPosturePosition(XmlReader r)
        {
            //<Position point="2" />
            Color = Color.Transparent;
            OptionGroup = "";
            
            bool isEmpty = r.IsEmptyElement;
            
            if(r.MoveToAttribute("point"))
                Point = r.ReadContentAsInt();
            
            if(r.MoveToAttribute("symbol"))
                Symbol = r.ReadContentAsString();
            
            if(r.MoveToAttribute("color"))
                Color = XmlHelper.ParseColor(r.ReadContentAsString(), Color);
            
            if(r.MoveToAttribute("optionGroup"))
                OptionGroup = r.ReadContentAsString();

            r.ReadStartElement();
            
            if(isEmpty)
                return;
        }
    }
}