using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public static class CursorManager
    {
        public static Cursor GetCursor(AbstractDrawingTool tool, double stretchFactor)
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
                    return GetCursorPrecision(style);
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
                return GetCursorPrecision(style);
            }
            else
            {
                return null;
            }
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
            // Colored and sized circle.
            string keyColor = "color";
            string keySize = "pen size";

            if (!style.Elements.ContainsKey(keyColor))
                return null;

            if (!style.Elements.ContainsKey(keySize))
                return null;
            
            Color c = (Color)style.Elements[keyColor].Value;
            int size = (int)(stretchFactor * (int)style.Elements[keySize].Value);

            Pen p = new Pen(c, 1);
            Bitmap b = new Bitmap(size + 2, size + 2);
            Graphics g = Graphics.FromImage(b);
            g.DrawEllipse(p, 1, 1, size, size);

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
        
        private static Cursor GetCursorPrecision(DrawingStyle style)
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
                            color = (Color)value;
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
