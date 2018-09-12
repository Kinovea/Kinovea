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
            get { return tools; }
        }
        #endregion

        #region Members
        private static Dictionary<string, AbstractDrawingTool> tools = null;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public static void LoadTools()
        {
            tools = new Dictionary<string, AbstractDrawingTool>();

            // Built-ins
            // These tools cannot be easily externalized at the moment due to some specificities like a custom cursor or ctor parameters.
            tools.Add("CrossMark", new DrawingToolCrossMark());
            tools.Add("Grid", new DrawingToolGrid());
            tools.Add("Plane", new DrawingToolPlane());
            
            tools.Add("Spotlight", new DrawingToolSpotlight());
            tools.Add("AutoNumbers", new DrawingToolAutoNumbers());
            tools.Add("Magnifier", new DrawingToolMagnifier());
            tools.Add("CoordinateSystem", new DrawingToolCoordinateSystem());
            tools.Add("TestGrid", new DrawingToolTestGrid());

            // Custom tools (externally defined).
            foreach (AbstractDrawingTool customTool in GenericPostureManager.Tools)
                tools.Add(customTool.Name, customTool);
            
            // Standard tools externally defined.
            ImportExternalTools();

            LoadPresets();
        }
        
        public static void SavePresets()
        {
            SavePresets(Software.ColorProfileDirectory + "current.xml");
        }
        public static void SavePresets(string file)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.CloseOutput = true;
            
            using(XmlWriter w = XmlWriter.Create(file, settings))
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
        public static void LoadPresets(string file)
        {
            if(!File.Exists(file))
                return;
            
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            settings.CloseInput = true;
            
            using(XmlReader r = XmlReader.Create(file, settings))
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
        public static DrawingStyle GetStylePreset(string tool)
        {
            if (!Tools.ContainsKey(tool))
                return new DrawingStyle();

            return Tools[tool].StylePreset.Clone();
        }

        public static void SetStylePreset(string tool, DrawingStyle style)
        {
            if (string.IsNullOrEmpty(tool) || !Tools.ContainsKey(tool))
                return;

            Tools[tool].StylePreset = style;
        }

        public static void SetStylePreset(AbstractDrawing drawing, DrawingStyle style)
        {
            string tool = GetToolName(drawing);
            SetStylePreset(tool, style);
        }
        
        /// <summary>
        /// Try to find the tool that generates this kind of drawings.
        /// </summary>
        private static string GetToolName(AbstractDrawing drawing)
        {
            if (drawing == null)
                return null;

            // For external custom tools the drawing retains the tool identifier.
            if (drawing is DrawingGenericPosture)
            {
                Guid toolId = ((DrawingGenericPosture)drawing).ToolId;

                foreach (DrawingToolGenericPosture customTool in GenericPostureManager.Tools)
                {
                    if (customTool.ToolId == toolId)
                        return customTool.Name;
                }

                return null;
            }

            foreach (var tool in tools.Values)
            {
                // For external standard tools, we check via the class of drawing they instanciate.
                if (tool is DrawingTool)
                {
                    if (((DrawingTool)tool).DrawingType == drawing.GetType())
                    {
                        return tool.Name;
                    }
                }
                else
                {
                    // For internal standard tool, we have to check types one by one.
                    if (drawing is DrawingCrossMark)
                    {
                        return "CrossMark";
                    }
                    else if (drawing is DrawingPencil)
                    {
                        return "Pencil";
                    }
                    else if (drawing is DrawingPlane)
                    {
                        if (((DrawingPlane)drawing).InPerspective)
                            return "Plane";
                        else
                            return "Grid";
                    }
                }
            }

            // At this point we don't recognize the drawing type.
            return null;
        }

        #region Private Methods
        private static DrawingStyle ImportPreset(DrawingStyle defaultStyle, DrawingStyle preset)
        {
            // This is used when importing the presets from XML.
            // Styling options may be added or removed between releases.
            // Compare the drawing's style elements with the elements in the default preset.
            // TODO: this should be done for KVA too.
            
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
        
        private static void ImportExternalTools()
        {
            if (!Directory.Exists(Software.StandardToolsDirectory))
                return;

            foreach (string file in Directory.GetFiles(Software.StandardToolsDirectory))
            {
                AbstractDrawingTool tool = DrawingTool.CreateFromFile(file);
                if (tool != null && !tools.ContainsKey(tool.Name))
                    tools.Add(tool.Name, tool);
            }
        }

        #endregion
    }
}
