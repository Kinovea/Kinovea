using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Kinovea.ScreenManager
{
    public static class CursorManager
    {
        public static Cursor GetToolCursor(AbstractDrawingTool tool, double stretchFactor)
        {
            if (tool is DrawingToolPointer)
                return ((DrawingToolPointer)tool).GetCursor();

            // Custom cursors.
            // The general guideline is that 
            // - We try to use custom cursor where it make sense like for pencil.
            // - Otherwise we fallback to either precision cross or the tool icon.
            // - Drawings that splat themselves at once on the canvas (no dragging a second leg), should use the icon,
            // - Drawings that are created in several steps should use the precision cross.

            if (tool is DrawingToolGenericPosture || tool is DrawingToolAutoNumbers)
            {
                // Many of the GenericPosture tools have a color style element that doesn't really serve any purpose, 
                // as the lines making the drawing can have their own color defined in the XML.
                // They are also draw at once on the canvas like a stamp.
                // So for these we reuse the drawing tool icon as a cursor.
                return GetCursorIcon(tool);
            }
            else if (tool is DrawingTool)
            {
                // These are the standard tool but defined in XML.
                // For these we try to use a precision cursor or a semantic one.
                DrawingStyle style = ToolManager.GetStylePreset(tool.Name);

                if (tool.Name == "Pencil")
                {
                    return GetCursorPencil(style, stretchFactor);
                }
                else if (tool.Name == "CrossMark")
                {
                    return GetCursorCrossMark(style);
                }
                else if (tool.Name == "Grid" || 
                         tool.Name == "Plane" || 
                         tool.Name == "DistortionGrid" ||
                         tool.Name == "Label")
                {
                    // These are stamp-like.
                    return GetCursorIcon(tool);
                }
                else
                {
                    return GetCursorPrecision(style, false);
                }
            }
            else if (tool is DrawingToolCoordinateSystem ||
                     tool is DrawingToolMagnifier||
                     tool is DrawingToolSpotlight || 
                     tool is DrawingToolTestGrid)
            {
                // Special internal tools. 
                // Still nice to use a precision cursor with them, but the color might not make sense.
                DrawingStyle style = ToolManager.GetStylePreset(tool.Name);
                return GetCursorPrecision(style, false);
            }
            else
            {
                return null;
            }
        }

        public static Cursor GetManipulationCursor(AbstractDrawing drawing)
        {
            IDecorable decorable = drawing as IDecorable;
            if (decorable == null)
                return GetCursorPrecision(null, false);

            return GetCursorPrecision(decorable.DrawingStyle, false);
        }

        public static Cursor GetManipulationCursorMagnifier()
        {
            // Special case as the magnifier is not an AbstractDrawing.
            return GetCursorPrecision(null, false);
        }
        
        private static Cursor GetCursorIcon(AbstractDrawingTool tool)
        {
            if (tool.Icon == null)
                return Cursors.Cross;

            Bitmap b = new Bitmap(tool.Icon.Width, tool.Icon.Height);
            Graphics g = Graphics.FromImage(b);
            g.DrawImage(tool.Icon, 0, 0);

            return new Cursor(b.GetHicon());
        }

        private static Cursor GetCursorPencil(DrawingStyle style, double stretchFactor)
        {
            // Colored and sized circle with precision cross.
            string keyColor = "color";
            string keySize = "pen size";

            if (!style.Elements.ContainsKey(keyColor))
                return null;

            if (!style.Elements.ContainsKey(keySize))
                return null;
            
            Color c = (Color)style.Elements[keyColor].Value;
            int circleSize = (int)(stretchFactor * (int)style.Elements[keySize].Value);

            float crossSize = 15;

            Pen p = new Pen(c, 1);
            int bmpSize = Math.Max((int)crossSize, circleSize);
            Bitmap b = new Bitmap(bmpSize, bmpSize);
            Graphics g = Graphics.FromImage(b);
            
            float startCircle = (bmpSize - 1 - circleSize) / 2.0f;
            g.DrawEllipse(p, startCircle, startCircle, circleSize, circleSize);

            // Add precision cross
            float startCross = (bmpSize - crossSize) / 2.0f;
            float bmpCenter = bmpSize / 2.0f;
            g.DrawLine(p, startCross, bmpCenter, startCross + crossSize, bmpCenter);
            g.DrawLine(p, bmpCenter, startCross, bmpCenter, startCross + crossSize);

            g.Dispose();
            p.Dispose();

            return new Cursor(b.GetHicon());
        }

        private static Cursor GetCursorCrossMark(DrawingStyle style)
        {
            // Cross inside a semi transparent circle (same as drawing).
            string keyColor = "back color";
            if (!style.Elements.ContainsKey(keyColor))
                return null;

            Color c = (Color)style.Elements[keyColor].Value;
            Pen p = new Pen(c, 1);
            Bitmap b = new Bitmap(9, 9);
            Graphics g = Graphics.FromImage(b);

            // Center point is {4,4}
            g.DrawLine(p, 1, 4, 7, 4);
            g.DrawLine(p, 4, 1, 4, 7);

            SolidBrush tempBrush = new SolidBrush(Color.FromArgb(32, c));
            g.FillEllipse(tempBrush, 0, 0, 8, 8);
            tempBrush.Dispose();

            g.Dispose();
            p.Dispose();

            return new Cursor(b.GetHicon());
        }
        
        private static Cursor GetCursorPrecision(DrawingStyle style, bool invert)
        {
            // Try to find a color style element to use as the cursor color.
            Color color = Color.Empty;
            if (style != null && style.Elements != null && style.Elements.Count > 0)
            {
                foreach (AbstractStyleElement styleElement in style.Elements.Values)
                {
                    if (styleElement is StyleElementColor)
                    {
                        object value = ((StyleElementColor)styleElement).Value;
                        if (value is Color)
                        {
                            if (invert)
                                color = Color.FromArgb(((Color)value).ToArgb() ^ 0xffffff);
                            else
                                color = (Color)value;
                            break;
                        }
                    }
                }
            }

            if (color.IsEmpty)
                color = Color.White;

            // Creates a "precision" cursor, simple cross.
            Pen p = new Pen(color, 1);
            int size = 15;
            int half = (size - 1) / 2;
            Bitmap b = new Bitmap(size, size);
            Graphics g = Graphics.FromImage(b);
            
            g.DrawLine(p, 0, half, size-1, half);
            g.DrawLine(p, half, 0, half, size-1);

            g.Dispose();
            p.Dispose();

            return new Cursor(b.GetHicon());
        }
    }
}
