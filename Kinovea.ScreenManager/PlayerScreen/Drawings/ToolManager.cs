#region License
/*
Copyright © Joan Charmant 2011.
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
using System.IO;
using System.Xml;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Host of the global list of tools.
    /// Each screen type will have its own subset of supported tools.
    /// </summary>
    public static class ToolManager
    {
        #region Properties
        /// <summary>
        ///   Returns the cached list of tools.
        /// </summary>
        public static Dictionary<string, AbstractDrawingTool> Tools
        {
            get 
            {
                if (object.ReferenceEquals(m_Tools, null))
                {
                    Initialize();
                }
                
                return m_Tools; 
            }
        }
        
        // Maybe we could find a way to generate this list of properties automatically.
        // A custom tool in the vein of the ResXFileCodeGenerator that would take an XML file in,
        // and creates a set of accessor properties.
        public static DrawingToolAngle2D Angle
        {
            get { return (DrawingToolAngle2D)Tools["Angle"]; }
        }
        public static DrawingToolChrono Chrono
        {
            get { return (DrawingToolChrono)Tools["Chrono"]; }
        }
        public static DrawingToolCircle Circle
        {
            get { return (DrawingToolCircle)Tools["Circle"]; }
        }
        public static DrawingToolCross2D CrossMark
        {
            get { return (DrawingToolCross2D)Tools["CrossMark"]; }
        }
        public static DrawingToolLine2D Line
        {
            get { return (DrawingToolLine2D)Tools["Line"]; }
        }
        public static DrawingToolArrow Arrow
        {
            get { return (DrawingToolArrow)Tools["Arrow"]; }
        }
        public static DrawingToolPencil Pencil
        {
            get { return (DrawingToolPencil)Tools["Pencil"]; }
        }
        public static DrawingToolGenericPosture GenericPosture
        {
            get { return (DrawingToolGenericPosture)Tools["GenericPosture"]; }
        }
        public static DrawingToolText Label
        {
            get { return (DrawingToolText)Tools["Label"]; }
        }
        public static DrawingToolGrid Grid
        {
            get { return (DrawingToolGrid)Tools["Grid"]; }
        }
        public static DrawingToolPlane Plane
        {
            get { return (DrawingToolPlane)Tools["Plane"]; }
        }
        public static DrawingToolSpotlight Spotlight
        {
            get { return (DrawingToolSpotlight)Tools["Spotlight"]; }
        }
        public static DrawingToolAutoNumbers AutoNumbers
        {
            get { return (DrawingToolAutoNumbers)Tools["AutoNumbers"]; }
        }
        public static DrawingToolMagnifier Magnifier
        {
            get { return (DrawingToolMagnifier)Tools["Magnifier"]; }
        }
        public static DrawingToolCoordinateSystem CoordinateSystem
        {
            get { return (DrawingToolCoordinateSystem)Tools["CoordinateSystem"]; }
        }
        #endregion
        
        #region Members
        private static Dictionary<string, AbstractDrawingTool> m_Tools = null;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public static void SavePresets()
        {
            SavePresets(Software.ColorProfileDirectory + "current.xml");
        }
        public static void SavePresets(string _file)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.CloseOutput = true;
            
            using(XmlWriter w = XmlWriter.Create(_file, settings))
            {
                w.WriteStartElement("KinoveaColorProfile");
                w.WriteElementString("FormatVersion", "3.0");
                foreach(KeyValuePair<string, AbstractDrawingTool> tool in Tools)
                {
                    DrawingStyle preset = tool.Value.StylePreset;
                    if(preset != null && preset.Elements.Count > 0)
                    {
                        w.WriteStartElement("ToolPreset");
                        w.WriteAttributeString("Key", tool.Key);
                        preset.WriteXml(w);
                        w.WriteEndElement();
                    }
                }
                
                w.WriteEndElement();
            }
        }
        public static void LoadPresets()
        {
            LoadPresets(Software.ColorProfileDirectory + "current.xml");
        }
        public static void LoadPresets(string _file)
        {
            if(!File.Exists(_file))
                return;
            
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            settings.CloseInput = true;
            
            using(XmlReader r = XmlReader.Create(_file, settings))
            {
                try
                {
                    r.MoveToContent();
                    r.ReadStartElement();
                    string version = r.ReadElementContentAsString("FormatVersion", "");
                    if(version == "3.0")
                    {
                        while(r.NodeType == XmlNodeType.Element && r.Name == "ToolPreset")
                        {
                            string key = r.GetAttribute("Key");
                            DrawingStyle preset = new DrawingStyle(r);
                                
                            // Find the tool with this key and replace its preset style with the one we just read.
                            AbstractDrawingTool tool;
                            bool found = Tools.TryGetValue(key, out tool);
                            if(found)
                            {
                                // Carry on the memo so we can still do cancel and retrieve the old values.
                                DrawingStyle memo = tool.StylePreset.Clone();
                                tool.StylePreset = ImportPreset(memo, preset);
                                tool.StylePreset.Memorize(memo);
                            }
                            else
                            {
                                log.ErrorFormat("The tool \"{0}\" was not found. Preset not imported.", key);
                            }
                        }
                    }
                    else
                    {
                        log.ErrorFormat("Unsupported format ({0}) for tool presets", version);
                    }
                }
                catch(Exception)
                {
                    log.Error("An error happened during the parsing of the tool presets file");
                }
            }
        }
        
        #region Private Methods
        private static void Initialize()
        {
            m_Tools = new Dictionary<string, AbstractDrawingTool>();
            
            // The core drawing tools are hard wired.
            // Maybe in the future we can have a plug-in system with .dll containing extensions tools.
            // Note that the pointer "tool" is not listed, as each screen must have its own.
            m_Tools.Add("Angle", new DrawingToolAngle2D());
            m_Tools.Add("Chrono", new DrawingToolChrono());
            m_Tools.Add("Circle", new DrawingToolCircle());
            m_Tools.Add("CrossMark", new DrawingToolCross2D());
            m_Tools.Add("Line", new DrawingToolLine2D());
            m_Tools.Add("Arrow", new DrawingToolArrow());
            m_Tools.Add("Pencil", new DrawingToolPencil());
            m_Tools.Add("GenericPosture", new DrawingToolGenericPosture());
            m_Tools.Add("Label", new DrawingToolText());
            m_Tools.Add("Grid", new DrawingToolGrid());
            m_Tools.Add("Plane", new DrawingToolPlane());
            m_Tools.Add("Spotlight", new DrawingToolSpotlight());
            m_Tools.Add("AutoNumbers", new DrawingToolAutoNumbers());
            m_Tools.Add("Magnifier", new DrawingToolMagnifier());
            m_Tools.Add("CoordinateSystem", new DrawingToolCoordinateSystem());
            
            LoadPresets();
        }
        private static DrawingStyle ImportPreset(DrawingStyle defaultStyle, DrawingStyle preset)
        {
            // Styling options may be added or removed between releases.
            
            // Add options unknown to the preset.
            foreach(KeyValuePair<string, AbstractStyleElement> pair in defaultStyle.Elements)
            {
                if(!preset.Elements.ContainsKey(pair.Key))
                    preset.Elements.Add(pair.Key, pair.Value);
            }

            // Remove options unknown to the default.
            foreach(KeyValuePair<string, AbstractStyleElement> pair in preset.Elements)
            {
                if(!defaultStyle.Elements.ContainsKey(pair.Key))
                    preset.Elements.Remove(pair.Key);
            }

            return preset;
        }
        #endregion
    }
}
