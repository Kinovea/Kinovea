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

            // Singleton tools and magnifier.
            tools.Add("NumberSequence", new DrawingToolNumberSequence());
            tools.Add("Spotlight", new DrawingToolSpotlight());
            tools.Add("CoordinateSystem", new DrawingToolCoordinateSystem());
            tools.Add("TestGrid", new DrawingToolTestGrid());
            tools.Add("Magnifier", new DrawingToolMagnifier());

            // Custom tools (externally defined).
            foreach (AbstractDrawingTool customTool in GenericPostureManager.Tools)
            {
                if (tools.ContainsKey(customTool.Name))
                    tools[customTool.Name] = customTool;
                else
                    tools.Add(customTool.Name, customTool);
            }
            
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
                                //log.DebugFormat("Imported {0} tool preset.", key);
                            }
                            else
                            {
                                log.ErrorFormat("Unknown tool: \"{0}\". Preset not imported.", key);
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

            if (Tools[tool].StylePreset == null)
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
        public static string GetToolName(AbstractDrawing drawing)
        {
            //------------------------------------------------------------
            // This is used as part of the "set style as default" feature.
            //
            // This requires some specific code to handle "style variants".
            // Style variants are for example the tools Line, Arrow, Squiggly line, Squiggly arrow.
            // These are all implemented by the same Drawing class.
            // The style can be changed on the fly, so you can start with a Line object and set it to have arrows, or vice versa.
            // When someone does "set style as default" on an object that has arrows, they most likely want to change the style of arrows.
            // For example if we add a line, change it to have an arrow and pass that file to someone else, 
            // when they click that arrow and do set style as default they can't expect that it will change the tool preset for bare lines.
            //------------------------------------------------------------

            if (drawing == null)
                return null;

            // Generic posture: the drawing object retains the tool identifier.
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

            // For tools that have style variants, we handle them separately and try to best-guess 
            // the most appropriate tool to change to avoid surprises.
            if (drawing.GetType() == typeof(DrawingLine))
            {
                return GetLineStyleVariant(drawing as DrawingLine);
            }
            else if (drawing.GetType() == typeof(DrawingPolyline))
            {
                return GetPolyLineStyleVariant(drawing as DrawingPolyline);
            }
            else if (drawing.GetType() == typeof(DrawingPlane))
            {
                return GetPlaneStyleVariant(drawing as DrawingPlane);
            }
            else if (drawing.GetType() == typeof(DrawingChrono))
            {
                return GetChronoStyleVariant(drawing as DrawingChrono);
            }

            // For others we match with the tool that instanciate that kind of drawings.
            foreach (var tool in tools.Values)
            {
                // For external standard tools, we check via the class of drawing they instanciate.
                if (tool is DrawingTool)
                {
                    if (drawing.GetType() == ((DrawingTool)tool).DrawingType)
                        return tool.Name;
                }
                else
                {
                    // For singleton drawings we have to check types one by one.
                    // The ones that are not listed here (auto numbers, spotlight, etc.) aren't supported.
                    if (drawing is DrawingCoordinateSystem)
                    {
                        return "CoordinateSystem";
                    }
                    else if (drawing is DrawingTestGrid)
                    {
                        return "TestGrid";
                    }
                    else if (drawing is DrawingNumberSequence)
                    {
                        return "NumberSequence";
                    }
                }
            }

            // At this point we don't recognize the drawing type or it's not supported for set style as default.
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
            HashSet<string> keysToRemove = new HashSet<string>();
            foreach(KeyValuePair<string, AbstractStyleElement> pair in preset.Elements)
            {
                if(!defaultStyle.Elements.ContainsKey(pair.Key))
                {
                    keysToRemove.Add(pair.Key);
                }
            }

            foreach (string key in keysToRemove)
                preset.Elements.Remove(key);

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

        private static string GetLineStyleVariant(DrawingLine drawing)
        {
            // Style variants of DrawingLine: line, arrow, arrow dash, arrow squiggly.
            if (!drawing.DrawingStyle.Elements.ContainsKey("arrows") || !drawing.DrawingStyle.Elements.ContainsKey("line shape"))
                return "Line";

            StyleElementLineEnding elementLineEnding = drawing.DrawingStyle.Elements["arrows"] as StyleElementLineEnding;
            StyleElementLineShape elementLineShape = drawing.DrawingStyle.Elements["line shape"] as StyleElementLineShape;
            if (elementLineEnding == null || elementLineShape == null)
                return "Line";

            LineEnding valueLineEnding = (LineEnding)elementLineEnding.Value;
            LineShape valueLineShape = (LineShape)elementLineShape.Value;

            if (valueLineEnding == LineEnding.None)
            {
                return "Line";
            }
            else
            {
                switch (valueLineShape)
                {
                    case LineShape.Solid: return "Arrow";
                    case LineShape.Dash: return "ArrowDash";
                    case LineShape.Squiggle: return "ArrowSquiggly";
                    default: return "Line";
                }
            }
        }

        private static string GetPolyLineStyleVariant(DrawingPolyline drawing)
        {
            // Style variants of DrawingPolyline: polyline, curve, arrow curve, arrow polyline, arrow polyline dash, arrow polyline squiggly.
            if (!drawing.DrawingStyle.Elements.ContainsKey("arrows") || !drawing.DrawingStyle.Elements.ContainsKey("line shape") || !drawing.DrawingStyle.Elements.ContainsKey("curved"))
                return "Polyline";

            StyleElementLineEnding elementLineEnding = drawing.DrawingStyle.Elements["arrows"] as StyleElementLineEnding;
            StyleElementLineShape elementLineShape = drawing.DrawingStyle.Elements["line shape"] as StyleElementLineShape;
            StyleElementToggle elementCurved = drawing.DrawingStyle.Elements["curved"] as StyleElementToggle;
            if (elementLineEnding == null || elementLineShape == null || elementCurved == null)
                return "Polyline";

            LineEnding valueLineEnding = (LineEnding)elementLineEnding.Value;
            LineShape valueLineShape = (LineShape)elementLineShape.Value;
            bool valueCurved = (bool)elementCurved.Value;

            if (valueLineEnding == LineEnding.None)
            {
                if (!valueCurved)
                    return "Polyline";
                else
                    return "Curve";
            }
            else
            {
                switch (valueLineShape)
                {
                    case LineShape.Solid: return "ArrowPolyline";
                    case LineShape.Dash: return "ArrowPolylineDash";
                    case LineShape.Squiggle: return "ArrowPolylineSquiggly";
                    default: return "Polyline";
                }
            }
        }

        private static string GetPlaneStyleVariant(DrawingPlane drawing)
        {
            // Style variants of DrawingPlane: Plane, Grid, Distance grid.
            // Plane is the perspective plane, grid is the flat grid.
            
            // Default when unknown is plane.
            if (!drawing.DrawingStyle.Elements.ContainsKey("perspective"))
                return "Plane";

            StyleElementToggle elementPerspective = drawing.DrawingStyle.Elements["perspective"] as StyleElementToggle;
            if (elementPerspective == null)
                return "Plane";

            bool valuePerspective = (bool)elementPerspective.Value;
            if (!valuePerspective)
                return "Grid";

            return "Plane";
        }

        private static string GetChronoStyleVariant(DrawingChrono drawing)
        {
            // Style variants of DrawingChrono: Chrono, Clock.
            if (!drawing.DrawingStyle.Elements.ContainsKey("clock"))
                return "Chrono";

            StyleElementToggle elementToggle = drawing.DrawingStyle.Elements["clock"] as StyleElementToggle;
            if (elementToggle == null)
                return "Chrono";

            bool valueClock = (bool)elementToggle.Value;
            if (valueClock)
                return "Clock";
            else
                return "Chrono";
        }

        #endregion
    }
}
