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
    /// Collect the style elements of one drawing or one drawing tool (default style).
    /// 
    /// A style element is a brigde between the UI and the style data.
    /// The style data contains the union of all possible style properties of drawings.
    /// 
    /// A given style element/style data pair may be indexed by 
    /// different keys compared to another drawing.
    /// 
    /// A style element is linked to one particular property and manages a 
    /// UI control that exposes that property to the UI.
    /// 
    /// To see the available elements and their keys for a particular tool, check the constructor of the tool.
    /// In the case of XML defined tools, the list is declared in the XML in the DefaultStyle tag.
    /// 
    /// There are 3 ways we create styles.
    /// - from scratch in the code. For the default style of certain things (tracks, kinogram).
    /// - from XML by reading the tool-level default style.
    /// - from XML by reading an existing KVA file containing an instance of a drawing.
    /// 
    /// Since style elements may be added or removed between versions we should always import 
    /// the styles read from KVA into a default reference style of the current version.
    /// </summary>
    public class StyleElements
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
        public StyleElements(){}

        /// <summary>
        /// Create the sytle by reading all the style elements from XML.
        /// This can be called in the context of a tool, a preset or a saved drawing.
        /// 
        /// For a drawing this should be followed by a call to Import(), 
        /// to import the XML parsed values into a reference style cloned 
        /// from the drawing tool's default or from a preset.
        /// </summary>
        public StyleElements(XmlReader xmlReader)
        {
            ReadXml(xmlReader);
        }
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Deep clone of the style elements.
        /// This includes any metadata like min/max and display name.
        /// </summary>
        public StyleElements Clone()
        {
            StyleElements clone = new StyleElements();
            foreach(KeyValuePair<string, AbstractStyleElement> element in styleElements)
            {
                clone.Elements.Add(element.Key, element.Value.Clone());
            }

            return clone;
        }
        
        /// <summary>
        /// Import the values of a style into this collection, by key.
        /// </summary>
        public void ImportValues(StyleElements other)
        {
            foreach (KeyValuePair<string, AbstractStyleElement> element in other.styleElements)
            {
                if (styleElements.ContainsKey(element.Key))
                    styleElements[element.Key].Value = element.Value.Value;
            }
        }

        /// <summary>
        /// Read style elements from XML and import the values into our elements.
        /// This is used to import drawings from KVA files (or KVA fragments for undo).
        /// The existing style should be a copy of the default style or preset for the tool.
        public void ImportXML(XmlReader xmlReader)
        {
            StyleElements style = new StyleElements(xmlReader);
            ImportValues(style);
        }

        /// <summary>
        /// Read a drawing style from XML.
        /// </summary>
        public void ReadXml(XmlReader xmlReader)
        {			
            styleElements.Clear();
            
            xmlReader.ReadStartElement();	// <ToolPreset Key="ToolName"> or <DrawingStyle>
            while(xmlReader.NodeType == XmlNodeType.Element)
            {
                AbstractStyleElement styleElement = null;
                string key = xmlReader.GetAttribute("Key");

                switch (xmlReader.Name)
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
                    case "Int":
                        styleElement = new StyleElementInt(xmlReader);
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
        /// Binds the value in a style element to a data property in the style data union.
        /// Initialize the data with the current value of the style element.
        /// 
        /// Style element values and data properties are not necessarily of the same type. 
        /// For example we could have an style element wrapping an int and 
        /// push it into the font size of the Font property.
        /// 
        /// This binding goes styleElement -> styleData. 
        /// When the styleData is modified directly, ImportValueFromData() should be called
        /// to trigger the backwards binding.
        /// </summary>
        public void Bind(StyleData targetStyleData, string targetProperty, string elementKey)
        {
            AbstractStyleElement styleElement;
            bool found = styleElements.TryGetValue(elementKey, out styleElement);
            if(found && styleElement != null)
            {
                styleElement.SetBindTarget(targetStyleData, targetProperty);

                // Immediately push value to the data property.
                styleElement.ExportValueToData();
            }
            else
            {
                log.ErrorFormat("The element \"{0}\" was not found.", elementKey);
            }
        }

        /// <summary>
        /// Re-import the data into the style element values.
        /// Used when the data is modified directly.
        /// </summary>
        public void ImportValuesFromData()
        {
            foreach(KeyValuePair<string, AbstractStyleElement> element in styleElements)
            {
                element.Value.ImportValueFromData();
            }
        }

        /// <summary>
        /// Memorize the current state of the style elements before a change.
        /// This state may be restored by calling Restore().
        /// </summary>
        public void Memorize()
        {
            memo.Clear();
            foreach(KeyValuePair<string, AbstractStyleElement> element in styleElements)
            {
                memo.Add(element.Key, element.Value.Clone());
            }
        }

        /// <summary>
        /// Memorize the state of the passed style elements before a change.
        /// This is used when the whole DrawingStyle has been recreated and we want it to 
        /// remember its state before the recreation.
        /// Used for style presets to carry the memo after XML load.
        /// </summary>
        public void Memorize(StyleElements drawingStyleMemo)
        {
            memo.Clear();
            foreach(KeyValuePair<string, AbstractStyleElement> element in drawingStyleMemo.Elements)
            {
                memo.Add(element.Key, element.Value.Clone());
            }
        }

        /// <summary>
        /// Restore the previously saved state into the style elements
        /// and update the underlying style data.
        /// </summary>
        public void Restore()
        {
            styleElements.Clear();
            foreach(KeyValuePair<string, AbstractStyleElement> element in memo)
            {
                styleElements.Add(element.Key, element.Value.Clone());
            }

            // Force write from style elements to style data.
            foreach (KeyValuePair<string, AbstractStyleElement> element in styleElements)
            {
                element.Value.ExportValueToData();
            }
        }

        /// <summary>
        /// Dump the current values of style elements and the memo.
        /// </summary>
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
        /// This is also useful if the default style of a drawing variant is missing some elements.
        /// </summary>
        public static void SanityCheck(StyleElements input, StyleElements preset)
        {
            // This shouldn't be necessary anymore.
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
                case StyleToggleVariant.Clock:
                    return true;
                default:
                    return false;
            }
        }
    }
}
