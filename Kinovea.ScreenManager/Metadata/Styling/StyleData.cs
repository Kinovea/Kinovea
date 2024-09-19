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
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This class contains the union of all the style properties drawings can use.
    /// Each drawing tool is using a subset of the properties exposed here.
    /// 
    /// This class exposes drawing-ready values.
    ///
    /// The data fields are bound to style element values (editable in the UI) through 
    /// the Bind() method on the style element, passing the name of the property. 
    /// The binding will only work if types are compatible.
    /// 
    /// Difference between the style element type and the data field type:
    /// the style element contains metadata like min/max/step and the display name.
    /// The binding is between a specific property here and the wrapped value field 
    /// in the style element.
    /// 
    /// Operations
    /// - Init tool: default style elements (code or xml), no binding.
    /// - Create drawing: copy the drawing's parent tool elements and bind to data fields.
    /// - Import drawing from kva: import values into style elements created from tool.
    /// - Export drawing to kva: export value.
    /// - Import from prefs (presets): import style elements, values and metadata.
    /// - Export to prefs (presets): export values only.
    /// - Update style: export value to kva snippet for undo.
    /// - Undo style change: restore value from kva snippet.
    /// </summary>
    public class StyleData
    {
        #region Exposed function delegates

        /// <summary>
        /// Event raised when the value is changed dynamically through binding.
        /// This may be useful if the Drawing has several StyleHelper that must be linked somehow.
        /// An example use is when we change the main color of the track, we need to propagate the change
        /// to the small label attached (for the Label following mode).
        /// </summary>
        /// <remarks>The event is not raised when the value is changed manually through a property setter</remarks>
        public event EventHandler ValueChanged;
        #endregion

        #region Properties
        
        /// <summary>
        /// The main color of the object.
        /// This is not the foreground color of objects that are defined by their background color.
        /// For these objects (labels, chrono) the foreground color is computed on the fly in the helpers.
        /// See GetForegroundPen, GetForegroundBrush.
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        /// <summary>
        /// The background color.
        /// The foreground color (black or white) is computed automatically by the helpers.
        /// </summary>
        public Bicolor Bicolor
        {
            get { return bicolor; }
            set { bicolor = value; }
        }

        /// <summary>
        /// The width of lines.
        /// </summary>
        public int LineSize
        {
            get { return lineSize; }
            set { lineSize = value;}
        }

        /// <summary>
        /// The shape of lines for straight lines (solid, dashed, squiggly).
        /// </summary>
        public LineShape LineShape
        {
            get { return lineShape; }
            set { lineShape = value; }
        }

        /// <summary>
        /// The ending of lines for straight lines (round or arrows).
        /// </summary>
        public LineEnding LineEnding
        {
            get { return lineEnding; }
            set { lineEnding = value;}
        }

        /// <summary>
        /// The Font used for text.
        /// </summary>
        public Font Font
        {
            get { return font; }
            set
            {
                if(value != null)
                {
                    // We make temp copies of the variables because we call .Dispose() but
                    // it's possible that input value was pointing to the same reference.
                    string fontName = value.Name;
                    FontStyle fontStyle = value.Style;
                    float fontSize = value.Size;
                    font.Dispose();
                    font = new Font(fontName, fontSize, fontStyle);
                }
                else
                {
                    font.Dispose();
                    font = null;
                }
            }
        }

        /// <summary>
        /// The shape of the line for trajectories. 
        /// (solid or dashed and with markers or not).
        /// </summary>
        public TrackShape TrackShape
        {
            get { return trackShape; }
            set { trackShape = value;}
        }

        /// <summary>
        /// The shape of the line for the pencil tool and other 
        /// tools that don't need squiggly lines like circle and rectangle.
        /// (solid or dashed).
        /// </summary>
        public PenShape PenShape
        {
            get { return penShape; }
            set { penShape = value; }
        }

        /// <summary>
        /// Number of columns in the grid.
        /// </summary>
        public int GridCols
        {
            get { return gridCols; }
            set { gridCols = value; }
        }

        /// <summary>
        /// Number of rows in the grid.
        /// </summary>
        public int GridRows
        {
            get { return gridRows; }
            set { gridRows = value; }
        }

        /// <summary>
        /// Whether the polyline is a spline or not.
        /// </summary>
        public bool Curved
        {
            get { return toggles["curved"]; }
            set { toggles["curved"] = value; }
        }

        /// <summary>
        /// Whether the grid is flat or perspective.
        /// </summary>
        public bool Perspective
        {
            get { return toggles["perspective"]; }
            set { toggles["perspective"] = value; }
        }

        /// <summary>
        /// Chronometer or Clock.
        /// </summary>
        public bool Clock
        {
            get { return toggles["clock"]; }
            set { toggles["clock"] = value; }
        }

        public int ContentHash
        {
            get
            {
                int hash = 0;
                hash ^= color.GetHashCode();
                hash ^= bicolor.ContentHash;
                hash ^= lineSize.GetHashCode();
                hash ^= lineShape.GetHashCode();
                hash ^= lineEnding.GetHashCode();
                hash ^= font.GetHashCode();
                hash ^= trackShape.GetHashCode();
                hash ^= penShape.GetHashCode();
                hash ^= gridCols.GetHashCode();
                hash ^= gridRows.GetHashCode();
                hash ^= toggles.GetHashCode();
                return hash;
            }
        }
        #endregion

        #region Members
        private Color color = Color.Black;
        private Bicolor bicolor;
        private int lineSize = 1;
        private LineShape lineShape = LineShape.Solid;
        private Font font = new Font("Arial", 12, FontStyle.Regular);
        private LineEnding lineEnding = LineEnding.None;
        private TrackShape trackShape = TrackShape.Solid;
        private PenShape penShape = PenShape.Solid;
        private int gridCols;
        private int gridRows;
        private Dictionary<string, bool> toggles = new Dictionary<string, bool>();

        // Absolute minimum font size in screen space used for drawing text.
        private static readonly int minFontSize = 8;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public StyleData()
        {
            // Initialize toggles.
            toggles.Add("curved", false);
            toggles.Add("perspective", false);
            toggles.Add("clock", false);
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Update a property with a style element value.
        /// </summary>
        public void Set(string targetProperty, object value)
        {
            // Check type and import value if compatible with the target prop.
            bool imported = false;
            switch (targetProperty)
            {
                case "Color":
                    {
                        if (value is Color)
                        {
                            color = (Color)value;
                            imported = true;
                        }
                        break;
                    }
                case "Bicolor":
                    {
                        if (value is Color)
                        {
                            bicolor.Background = (Color)value;
                            imported = true;
                        }
                        break;
                    }
                case "LineSize":
                    {
                        if (value is int)
                        {
                            lineSize = (int)value;
                            imported = true;
                        }

                        break;
                    }
                case "LineShape":
                    {
                        if (value is LineShape)
                        {
                            lineShape = (LineShape)value;
                            imported = true;
                        }

                        break;
                    }
                case "LineEnding":
                    {
                        if (value is LineEnding)
                        {
                            lineEnding = (LineEnding)value;
                            imported = true;
                        }

                        break;
                    }
                case "TrackShape":
                    {
                        if (value is TrackShape)
                        {
                            trackShape = (TrackShape)value;
                            imported = true;
                        }

                        break;
                    }
                case "PenShape":
                    {
                        if (value is PenShape)
                        {
                            penShape = (PenShape)value;
                            imported = true;
                        }

                        break;
                    }
                case "Font":
                    {
                        // TODO: have a styleElementFont 
                        // with all the aspects of the font.
                        if (value is int)
                        {
                            // Recreate the font changing just the size.
                            string fontName = font.Name;
                            FontStyle fontStyle = font.Style;
                            font.Dispose();
                            font = new Font(fontName, (int)value, fontStyle);
                            imported = true;
                        }
                        break;
                    }
                case "GridCols":
                    {
                        if (value is int)
                        {
                            gridCols = (int)value;
                            imported = true;
                        }
                        break;
                    }
                case "GridRows":
                    {
                        if (value is int)
                        {
                            gridRows = (int)value;
                            imported = true;
                        }
                        break;
                    }
                case "Toggles/Curved":
                    {
                        if (value is bool)
                        {
                            toggles["curved"] = (bool)value;
                            imported = true;
                        }

                        break;
                    }
                case "Toggles/Perspective":
                    {
                        if (value is bool)
                        {
                            toggles["perspective"] = (bool)value;
                            imported = true;
                        }

                        break;
                    }
                case "Toggles/Clock":
                    {
                        if (value is bool)
                        {
                            toggles["clock"] = (bool)value;
                            imported = true;
                        }

                        break;
                    }
                default:
                    {
                        log.DebugFormat("Unknown target property \"{0}\".", targetProperty);
                        break;
                    }
            }

            if (imported)
            {
                if (ValueChanged != null)
                    ValueChanged(null, EventArgs.Empty);
            }
            else
            {
                log.DebugFormat("Could not import value \"{0}\" into property \"{1}\".", value.ToString(), targetProperty);
            }
        }

        /// <summary>
        /// Export a data field to a target data type for the value of a style element.
        /// </summary>
        public object Get(string sourceProperty, Type targetType)
        {
            bool converted = false;
            object result = null;
            switch (sourceProperty)
            {
                case "Color":
                    {
                        if (targetType == typeof(Color))
                        {
                            result = color;
                            converted = true;
                        }
                        break;
                    }
                case "Bicolor":
                    {
                        if (targetType == typeof(Color))
                        {
                            result = bicolor.Background;
                            converted = true;
                        }
                        break;
                    }
                case "LineSize":
                    {
                        if (targetType == typeof(int))
                        {
                            result = lineSize;
                            converted = true;
                        }
                        break;
                    }
                case "LineShape":
                    {
                        if (targetType == typeof(LineShape))
                        {
                            result = lineShape;
                            converted = true;
                        }
                        break;
                    }
                case "LineEnding":
                    {
                        if (targetType == typeof(LineEnding))
                        {
                            result = lineEnding;
                            converted = true;
                        }
                        break;
                    }
                case "TrackShape":
                    {
                        if (targetType == typeof(TrackShape))
                        {
                            result = trackShape;
                            converted = true;
                        }
                        break;
                    }
                case "PenShape":
                    {
                        if (targetType == typeof(PenShape))
                        {
                            result = penShape;
                            converted = true;
                        }
                        break;
                    }
                case "Font":
                    {
                        if (targetType == typeof(int))
                        {
                            result = (int)font.Size;
                            converted = true;
                        }
                        break;
                    }
                case "GridCols":
                    {
                        if (targetType == typeof(int))
                        {
                            result = gridCols;
                            converted = true;
                        }
                        break;
                    }
                case "GridRows":
                    {
                        if (targetType == typeof(int))
                        {
                            result = gridRows;
                            converted = true;
                        }
                        break;
                    }
                case "Toggles/Curved":
                    {
                        if (targetType == typeof(bool))
                        {
                            result = toggles["curved"];
                            converted = true;
                        }

                        break;
                    }
                case "Toggles/Perspective":
                    {
                        if (targetType == typeof(bool))
                        {
                            result = toggles["perspective"];
                            converted = true;
                        }

                        break;
                    }
                case "Toggles/Clock":
                    {
                        if (targetType == typeof(bool))
                        {
                            result = toggles["clock"];
                            converted = true;
                        }

                        break;
                    }
                default:
                    {
                        log.DebugFormat("Unknown source property \"{0}\".", sourceProperty);
                        break;
                    }
            }

            if (!converted)
            {
                log.DebugFormat("Could not convert property \"{0}\" to update value \"{1}\".", sourceProperty, targetType);
            }

            return result;
        }

        #region Pen and Brushes using the main color and line size properties
        /// <summary>
        /// Returns a Pen object of width = 1 with the main color.
        /// Used by marker and test grid.
        /// Can also be useful if we only care about the opacity and we will 
        /// override the width and color in the caller.
        /// </summary>
        public Pen GetPen(int alpha)
        {
            Color c = (alpha >= 0 && alpha <= 255) ? Color.FromArgb(alpha, color) : color;
            return NormalPen(new Pen(c, 1.0f));
        }

        /// <summary>
        /// Returns a pen of width = 1 with the main color.
        /// </summary>
        public Pen GetPen(float opacity)
        {
            return GetPen((int)(opacity * 255));
        }

        /// <summary>
        /// Returns a Pen to draw a straight line or contour.
        /// Integrates the main color, line size and solid vs dash.
        /// Line shape is finalized in the drawing itself for squiggly lines.
        /// Line ending is drawn in the drawing to have better arrows that what is provided by the Pen class.
        /// </summary>
        public Pen GetPen(int alpha, double stretchFactor)
        {
            Color c = (alpha >= 0 && alpha <= 255) ? Color.FromArgb(alpha, color) : color;
            float penWidth = (float)(lineSize * stretchFactor);
            penWidth = Math.Max(penWidth, 1.0f);
            
            Pen p = new Pen(c, penWidth);
            p.LineJoin = LineJoin.Round;
            p.DashStyle = trackShape.DashStyle;

            return p;
        }

        /// <summary>
        /// Returns a Pen to draw a straight line or contour.
        /// Integrates the main color, line size and solid vs dash.
        /// </summary>
        public Pen GetPen(double opacity, double stretchFactor)
        {
            return GetPen((int)(opacity * 255), stretchFactor);
        }

        /// <summary>
        /// Returns a Brush of the main color.
        /// Only use the color property.
        /// </summary>
        public SolidBrush GetBrush(int alpha)
        {
            Color c = (alpha >= 0 && alpha <= 255) ? Color.FromArgb(alpha, color) : color;
            return new SolidBrush(c);
        }

        /// <summary>
        /// Returns a Brush of the main color.
        /// </summary>
        /// </summary>
        public SolidBrush GetBrush(double opacity)
        {
            return GetBrush((int)(opacity * 255));
        }
        #endregion

        #region Get Font object
        public Font GetFont(float stretchFactor)
        {
            float fFontSize = GetRescaledFontSize(stretchFactor);
            return new Font(font.Name, fFontSize, font.Style);
        }
        public Font GetFontDefaultSize(int fontSize)
        {
            return new Font(font.Name, fontSize, font.Style);
        }
        #endregion

        #region Background color with auto foreground color

        /// <summary>
        /// Return the background color at the specified alpha.
        /// </summary>
        public Color GetBackgroundColor(int alpha)
        {
            Color background = bicolor.Background;
            Color c = (alpha >= 0 && alpha <= 255) ? Color.FromArgb(alpha, background) : background;
            return c;
        }

        /// <summary>
        /// Get a Pen of the background color.
        /// </summary>
        public Pen GetBackgroundPen(int alpha)
        {
            Color c = GetBackgroundColor(alpha);
            return NormalPen(new Pen(c, 1.0f));
        }

        /// <summary>
        /// Get a brush of the background color.
        /// </summary>
        public SolidBrush GetBackgroundBrush(int alpha)
        {
            Color c = GetBackgroundColor(alpha);
            return new SolidBrush(c);
        }

        /// <summary>
        /// Returns the foreground color (black or white) at the specified alpha.
        /// </summary>
        public Color GetForegroundColor(int alpha)
        {
            Color foreground = bicolor.Background.GetBrightness() >= 0.5 ? Color.Black : Color.White;
            Color c = (alpha >= 0 && alpha <= 255) ? Color.FromArgb(alpha, foreground) : foreground;
            return c;
        }

        /// <summary>
        /// Get a Pen of the foreground color (black or white).
        /// </summary>
        public Pen GetForegroundPen(int alpha)
        {
            Color c = GetForegroundColor(alpha);
            return NormalPen(new Pen(c, 1.0f));
        }

        /// <summary>
        /// Get a brush of the foreground color (black or white).
        /// </summary>
        public SolidBrush GetForegroundBrush(int alpha)
        {
            Color c = GetForegroundColor(alpha);
            return new SolidBrush(c);
        }
        #endregion

        #endregion

        #region Private Methods
        /// <summary>
        /// Get the strecthed font size.
        /// The final font size returned here may be out of the allowed range.
        /// The allowed range is in image space whereas the result here is for
        /// drawing so it is in screen space.
        /// Hard minimum of 8.
        /// </summary>
        private float GetRescaledFontSize(float stretchFactor)
        {
            float fontSize = (float)(font.Size * stretchFactor);
            fontSize = Math.Max(fontSize, minFontSize);
            return fontSize;
        }
        
        /// <summary>
        /// Set up a pen object with round start/end caps.
        /// </summary>
        private Pen NormalPen(Pen p)
        {
            p.StartCap = LineCap.Round;
            p.EndCap = LineCap.Round;
            p.LineJoin = LineJoin.Round;
            return p;
        }
        #endregion
    }
}
