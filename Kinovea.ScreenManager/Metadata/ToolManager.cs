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
        private static Dictionary<string, AbstractDrawingTool> tools = new Dictionary<string, AbstractDrawingTool>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public static void LoadTools()
        {
            tools.Clear();

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
                    StyleElements preset = tool.Value.StyleElements;
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

        /// <summary>
        /// Load the last saved style for each drawing tool.
        /// </summary>
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
                            StyleElements preset = new StyleElements(r);

                            // Find the tool with this key and replace its preset style with the one we just read.
                            AbstractDrawingTool tool;
                            bool found = Tools.TryGetValue(key, out tool);
                            if(found)
                            {
                                // Carry on the memo so we can still do cancel and retrieve the old values.
                                StyleElements memo = tool.StyleElements.Clone();
                                tool.StyleElements = ImportPreset(memo, preset);
                                tool.StyleElements.Memorize(memo);
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

        /// <summary>
        /// Get the default style elements for a given tool.
        /// This is used when creating drawings of this tool type.
        /// </summary>
        public static StyleElements GetDefaultStyleElements(string tool)
        {
            if (!Tools.ContainsKey(tool))
                return new StyleElements();

            if (Tools[tool].StyleElements == null)
                return new StyleElements();

            return Tools[tool].StyleElements.Clone();
        }

        /// <summary>
        /// Reset the style elements of a tool (preset) from the current elements of a drawing of this tool.
        /// This overload is used for singleton tools like coordinate system, test grid, etc.
        /// </summary>
        public static void SetToolStyleFromDrawing(string tool, StyleElements styleElements)
        {
            if (string.IsNullOrEmpty(tool) || !Tools.ContainsKey(tool))
                return;

            Tools[tool].StyleElements = styleElements;
        }

        /// <summary>
        /// Reset the style elements of a tool (preset) from the current elements of a drawing of this tool.
        /// This is used for the "set style as default" feature.
        /// </summary>
        public static void SetToolStyleFromDrawing(AbstractDrawing drawing, StyleElements styleElements)
        {
            string tool = GetToolName(drawing);
            SetToolStyleFromDrawing(tool, styleElements);
        }

        /// <summary>
        /// Find the tool that generates this kind of drawings.
        /// This is used as part of the "set style as default" feature.
        /// </summary>
        public static string GetToolName(AbstractDrawing drawing)
        {
            //------------------------------------------------------------
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
            // the most appropriate tool based on the current style configuration.
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

        /// <summary>
        /// Import the saved style preset of a given tool into a new style elements collection.
        /// This is used when importing the presets from XML.
        /// </summary>
        private static StyleElements ImportPreset(StyleElements defaultStyle, StyleElements preset)
        {
            // Styling options may be added or removed between releases.
            // The default style is coming from the tool and may have extra metadata like min/max values.
            // We start with the default style and import values from the saved preset.

            // Start by making a deep copy of the default style from the tool.
            StyleElements result = new StyleElements();
            foreach (string key in defaultStyle.Elements.Keys)
            {
                result.Elements.Add(key, defaultStyle.Elements[key].Clone());
            }

            // Import the saved values of the preset (last used value for this tool type)
            result.ImportValues(preset);
            return result;
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

        /// <summary>
        /// Returns the actual tool name from a Line DrawingLine instance.
        /// "Line", "Arrow", "ArrowDash", "ArrowSquiggly".
        /// </summary>
        private static string GetLineStyleVariant(DrawingLine drawing)
        {
            // Style variants of DrawingLine: line, arrow, arrow dash, arrow squiggly.
            if (!drawing.StyleElements.Elements.ContainsKey("arrows") || !drawing.StyleElements.Elements.ContainsKey("line shape"))
                return "Line";

            StyleElementLineEnding elementLineEnding = drawing.StyleElements.Elements["arrows"] as StyleElementLineEnding;
            StyleElementLineShape elementLineShape = drawing.StyleElements.Elements["line shape"] as StyleElementLineShape;
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

        /// <summary>
        /// Returns the actual tool name from a DrawingPolyline instance.
        /// "Polyline", "Curve", "ArrowPolyline", "ArrowPolyLineDash", "ArrowPolylineSquiggly"
        /// </summary>
        private static string GetPolyLineStyleVariant(DrawingPolyline drawing)
        {
            // Style variants of DrawingPolyline: polyline, curve, arrow curve, arrow polyline, arrow polyline dash, arrow polyline squiggly.
            if (!drawing.StyleElements.Elements.ContainsKey("arrows") || !drawing.StyleElements.Elements.ContainsKey("line shape") || !drawing.StyleElements.Elements.ContainsKey("curved"))
                return "Polyline";

            StyleElementLineEnding elementLineEnding = drawing.StyleElements.Elements["arrows"] as StyleElementLineEnding;
            StyleElementLineShape elementLineShape = drawing.StyleElements.Elements["line shape"] as StyleElementLineShape;
            StyleElementToggle elementCurved = drawing.StyleElements.Elements["curved"] as StyleElementToggle;
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

        /// <summary>
        /// Returns the actual tool name from a DrawingPlane instance.
        /// DrawingPlane class manages "Plane" and "Grid".
        /// Plane is the perspective plane, grid is the flat grid.
        /// </summary>
        private static string GetPlaneStyleVariant(DrawingPlane drawing)
        {
            // Style variants of DrawingPlane: Plane, Grid, Distance grid.
            // Plane is the perspective plane, grid is the flat grid.
            
            // Default when unknown is plane.
            if (!drawing.StyleElements.Elements.ContainsKey("perspective"))
                return "Plane";

            StyleElementToggle elementPerspective = drawing.StyleElements.Elements["perspective"] as StyleElementToggle;
            if (elementPerspective == null)
                return "Plane";

            bool valuePerspective = (bool)elementPerspective.Value;
            if (!valuePerspective)
                return "Grid";

            return "Plane";
        }

        /// <summary>
        /// Returns the actual tool name from a DrawingChrono instance.
        /// "Chrono", "Clock".
        /// </summary>
        private static string GetChronoStyleVariant(DrawingChrono drawing)
        {
            // Style variants of DrawingChrono: Chrono, Clock.
            if (!drawing.StyleElements.Elements.ContainsKey("clock"))
                return "Chrono";

            StyleElementToggle elementToggle = drawing.StyleElements.Elements["clock"] as StyleElementToggle;
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
