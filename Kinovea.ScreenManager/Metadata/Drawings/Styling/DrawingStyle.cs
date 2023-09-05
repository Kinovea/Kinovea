#region License
/*
Copyright © Joan Charmant 2011.
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

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Represents the styling elements of a drawing or drawing tool preset.
    /// Host a list of style elements needed to decorate the drawing.
    /// To see the available elements and their keys for a particular tool, check the constructor of the tool.
    /// In the case of XML defined tools, the list is declared in the XML in the DefaultStyle tag.
    /// </summary>
    public class DrawingStyle
    {
        #region Properties
        public Dictionary<string, AbstractStyleElement> Elements
        {
            get { return styleElements; }
        }
        #endregion
        
        #region Members
        private Dictionary<string, AbstractStyleElement> styleElements = new Dictionary<string, AbstractStyleElement>();
        private Dictionary<string, AbstractStyleElement> memo = new Dictionary<string, AbstractStyleElement>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Constructor
        public DrawingStyle(){}
        public DrawingStyle(XmlReader xmlReader)
        {
            ReadXml(xmlReader);
        }
        #endregion
        
        #region Public Methods
        public DrawingStyle Clone()
        {
            DrawingStyle clone = new DrawingStyle();
            foreach(KeyValuePair<string, AbstractStyleElement> element in styleElements)
            {
                clone.Elements.Add(element.Key, element.Value.Clone());
            }

            return clone;
        }
        public void ReadXml(XmlReader xmlReader)
        {			
            styleElements.Clear();
            
            xmlReader.ReadStartElement();	// <ToolPreset Key="ToolName"> or <DrawingStyle>
            while(xmlReader.NodeType == XmlNodeType.Element)
            {
                AbstractStyleElement styleElement = null;
                string key = xmlReader.GetAttribute("Key");
                
                switch(xmlReader.Name)
                {
                    case "Color":
                        styleElement = new StyleElementColor(xmlReader);
                        break;
                    case "FontSize":
                        styleElement = new StyleElementFontSize(xmlReader);
                        break;
                    case "PenSize":
                        styleElement = new StyleElementPenSize(xmlReader);
                        break;
                    case "LineSize":
                        styleElement = new StyleElementLineSize(xmlReader);
                        break;
                    case "LineShape":
                        styleElement = new StyleElementLineShape(xmlReader);
                        break;
                    case "Arrows":
                        styleElement = new StyleElementLineEnding(xmlReader);
                        break;
                    case "TrackShape":
                        styleElement = new StyleElementTrackShape(xmlReader);
                        break;
                    case "PenShape":
                        styleElement = new StyleElementPenShape(xmlReader);
                        break;
                    case "GridDivisions":
                        styleElement = new StyleElementGridDivisions(xmlReader);
                        break;
                    case "Toggle":
                        styleElement = new StyleElementToggle(xmlReader);
                        break;
                    default:
                        log.ErrorFormat("Could not import style element \"{0}\"", xmlReader.Name);
                        log.ErrorFormat("Content was: {0}", xmlReader.ReadOuterXml());
                        break;
                }
                
                if(styleElement != null)
                    styleElements.Add(key, styleElement);
            }
            
            xmlReader.ReadEndElement();
        }
        public void WriteXml(XmlWriter xmlWriter)
        {
            foreach(KeyValuePair<string, AbstractStyleElement> element in styleElements)
            {
                xmlWriter.WriteStartElement(element.Value.XmlName);
                xmlWriter.WriteAttributeString("Key", element.Key);
                element.Value.WriteXml(xmlWriter);
                xmlWriter.WriteEndElement();
            }
        }
        
        /// <summary>
        /// Binds a property in the style helper to an editable style element.
        /// Once bound, each time the element is edited in the UI, the property is updated,
        /// so the actual drawing automatically changes its style.
        /// 
        /// Style elements and properties need not be of the same type. The style helper knows how to
        /// map a FontSize element to its own Font property for example.
        /// 
        /// This binding goes style -> stylehelper. When a style helper is modified directly, style.readvalue() should be called
        /// to trigger the backwards binding.
        /// </summary>
        /// <param name="target">The drawing's style helper object</param>
        /// <param name="targetProperty">The name of the property in the style helper that needs automatic update</param>
        /// <param name="source">The style element that will push its change to the property</param>
        public void Bind(StyleHelper target, string targetProperty, string source)
        {
            AbstractStyleElement elem;
            bool found = styleElements.TryGetValue(source, out elem);
            if(found && elem != null)
                elem.Bind(target, targetProperty);
            else
                log.ErrorFormat("The element \"{0}\" was not found.", source);
        }
        public void RaiseValueChanged()
        {
            foreach(KeyValuePair<string, AbstractStyleElement> element in styleElements)
            {
                element.Value.RaiseValueChanged();
            }
        }

        /// <summary>
        /// Signals that a value stored in the style element has been modified manually.
        /// This will re-import the value into the style union.
        /// </summary>
        public void ReadValue()
        {
            foreach(KeyValuePair<string, AbstractStyleElement> element in styleElements)
            {
                element.Value.ReadValue();
            }
        }
        public void Memorize()
        {
            memo.Clear();
            foreach(KeyValuePair<string, AbstractStyleElement> element in styleElements)
            {
                memo.Add(element.Key, element.Value.Clone());
            }
        }
        public void Memorize(DrawingStyle drawingStyleMemo)
        {
            // This is used when the whole DrawingStyle has been recreated and we want it to 
            // remember its state before the recreation.
            // Used for style presets to carry the memo after XML load.
            memo.Clear();
            foreach(KeyValuePair<string, AbstractStyleElement> element in drawingStyleMemo.Elements)
            {
                memo.Add(element.Key, element.Value.Clone());
            }
        }
        public void Revert()
        {
            styleElements.Clear();
            foreach(KeyValuePair<string, AbstractStyleElement> element in memo)
            {
                styleElements.Add(element.Key, element.Value.Clone());
            }
        }
        public void Dump()
        {
            foreach(KeyValuePair<string, AbstractStyleElement> element in styleElements)
            {
                log.DebugFormat("{0}: {1}", element.Key, element.Value.ToString());
            }
            
            foreach(KeyValuePair<string, AbstractStyleElement> element in memo)
            {
                log.DebugFormat("Memo: {0}: {1}", element.Key, element.Value.ToString());
            }
        }
        #endregion
        
        /// <summary>
        /// Make sure that all options from the preset are in the passed style with matching names. 
        /// This is used to import old KVA with missing keys or keys that have since changed name.
        /// In this case we push the default value of the tool.
        /// We generally don't try to match the tool variant in these case, the important thing is that the 
        /// style elements are correct so we can at least change them later.
        /// </summary>
        public static void SanityCheck(DrawingStyle input, DrawingStyle preset)
        {
            foreach (string key in preset.Elements.Keys)
            {
                if (!input.Elements.ContainsKey(key))
                    input.Elements.Add(key, preset.Elements[key].Clone());
            }
        }

        /// <summary>
        /// Returns true if this toggle should be hidden from the user interface.
        /// </summary>
        public static bool IsHiddenToggle(StyleToggleVariant toggleType)
        {
            switch (toggleType)
            {
                case StyleToggleVariant.Perspective:
                case StyleToggleVariant.DistanceGrid:
                case StyleToggleVariant.Clock:
                    return true;
                default:
                    return false;
            }
        }
    }
}
