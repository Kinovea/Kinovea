#region License
/*
Copyright © Joan Charmant 2021.
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
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;
using System.Collections.Generic;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using SpreadsheetLight;

namespace Kinovea.ScreenManager
{
    public class ExporterXLSX
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Export(string path, MeasuredData md)
        {
            using (SLDocument sl = new SLDocument())
            {
                Dictionary<string, SLStyle> styles = CreateStyles(sl);
                
                int row = 2;
                row += ExportKeyframes(sl, styles, md, row);
                row++;
                row += ExportPositions(sl, styles, md, row);
                row++;
                row += ExportDistances(sl, styles, md, row);
                row++;
                row += ExportAngles(sl, styles, md, row);
                row++;
                row += ExportTimes(sl, styles, md, row);

                sl.AutoFitColumn(1, 4);

                sl.SaveAs(path);
            }
        }

        private Dictionary<string, SLStyle> CreateStyles(SLDocument sl)
        {
            Dictionary<string, SLStyle> styles = new Dictionary<string, SLStyle>();

            void setAllBorders(SLStyle style)
            {
                var borderStyle = BorderStyleValues.Thin;
                var borderColor = System.Drawing.Color.Black;
                style.Border.LeftBorder.BorderStyle = borderStyle;
                style.Border.TopBorder.BorderStyle = borderStyle;
                style.Border.RightBorder.BorderStyle = borderStyle;
                style.Border.BottomBorder.BorderStyle = borderStyle;
                style.Border.LeftBorder.Color = borderColor;
                style.Border.TopBorder.Color = borderColor;
                style.Border.RightBorder.Color = borderColor;
                style.Border.BottomBorder.Color = borderColor;
            }

            void setBackgroundColor(SLStyle style, System.Drawing.Color color)
            {
                style.Fill.SetPatternType(PatternValues.Solid);
                style.Fill.SetPatternForegroundColor(color);
            }

            SLStyle normal = sl.CreateStyle();
            setAllBorders(normal);

            SLStyle header = normal.Clone();
            header.Alignment.Horizontal = HorizontalAlignmentValues.Center;
            setAllBorders(header);

            SLStyle kfHeader = header.Clone();
            kfHeader.Font.Bold = true;
            setBackgroundColor(kfHeader, System.Drawing.Color.FromArgb(210, 245, 176));

            SLStyle timeHeader = header.Clone();
            kfHeader.Font.Bold = true;
            setBackgroundColor(timeHeader, System.Drawing.Color.FromArgb(194, 223, 255));

            SLStyle valueHeader = header.Clone();
            setBackgroundColor(valueHeader, System.Drawing.Color.FromArgb(232, 232, 232));

            SLStyle number = normal.Clone();
            number.FormatCode = "0.00";
            
            styles.Add("normal", normal);
            styles.Add("kfHeader", kfHeader);
            styles.Add("timeHeader", timeHeader);
            styles.Add("valueHeader", valueHeader);
            styles.Add("number", number);

            return styles;
        }

        private int ExportKeyframes(SLDocument sl, Dictionary<string, SLStyle> styles, MeasuredData md, int row)
        {
            if (md.Keyframes.Count == 0)
                return 0;
            
            sl.SetCellValue(row, 1, "Key images");
            sl.MergeWorksheetCells(row, 1, row, 2);

            sl.SetCellValue(row + 1, 1, "Name");
            sl.SetCellValue(row + 1, 2, "Time");

            for (int i = 0; i < md.Keyframes.Count; i++)
            {
                var kf = md.Keyframes[i];
                sl.SetCellValue(row + 2 + i, 1, kf.Name);
                sl.SetCellValue(row + 2 + i, 2, kf.Time);
            }

            sl.SetCellStyle(row, 1, row + md.Keyframes.Count + 1, 2, styles["normal"]);
            sl.SetCellStyle(row, 1, styles["kfHeader"]);
            sl.SetCellStyle(row + 1, 1, row + 1, 2, styles["valueHeader"]);

            return md.Keyframes.Count + 2;
        }

        private int ExportPositions(SLDocument sl, Dictionary<string, SLStyle> styles, MeasuredData md, int row)
        {
            if (md.Positions.Count == 0)
                return 0;

            sl.SetCellValue(row, 1, "Positions");
            sl.MergeWorksheetCells(row, 1, row, 4);
            
            sl.SetCellValue(row + 1, 1, "Name");
            sl.SetCellValue(row + 1, 2, string.Format("X ({0})", md.Units.LengthSymbol));
            sl.SetCellValue(row + 1, 3, string.Format("Y ({0})", md.Units.LengthSymbol));
            sl.SetCellValue(row + 1, 4, "Time");
            
            for (int i = 0; i < md.Positions.Count; i++)
            {
                var p = md.Positions[i];
                sl.SetCellValue(row + 2 + i, 1, p.Name);
                sl.SetCellValue(row + 2 + i, 2, p.X);
                sl.SetCellValue(row + 2 + i, 3, p.Y);
                sl.SetCellValue(row + 2 + i, 4, p.Time);
            }

            sl.SetCellStyle(row, 1, row + md.Positions.Count + 1, 4, styles["normal"]);
            sl.SetCellStyle(row, 1, styles["kfHeader"]);
            sl.SetCellStyle(row + 1, 1, row + 1, 4, styles["valueHeader"]);
            sl.SetCellStyle(row + 2, 2, row + md.Positions.Count + 1, 3, styles["number"]);

            return md.Positions.Count + 2;
        }

        private int ExportDistances(SLDocument sl, Dictionary<string, SLStyle> styles, MeasuredData md, int row)
        {
            if (md.Distances.Count == 0)
                return 0;

            sl.SetCellValue(row, 1, "Distances");
            sl.MergeWorksheetCells(row, 1, row, 3);

            sl.SetCellValue(row + 1, 1, "Name");
            sl.SetCellValue(row + 1, 2, string.Format("Length ({0})", md.Units.LengthSymbol));
            sl.SetCellValue(row + 1, 3, "Time");

            for (int i = 0; i < md.Distances.Count; i++)
            {
                var value = md.Distances[i];
                sl.SetCellValue(row + 2 + i, 1, value.Name);
                sl.SetCellValue(row + 2 + i, 2, value.Value);
                sl.SetCellValue(row + 2 + i, 3, value.Time);
            }

            sl.SetCellStyle(row, 1, row + md.Distances.Count + 1, 3, styles["normal"]);
            sl.SetCellStyle(row, 1, styles["kfHeader"]);
            sl.SetCellStyle(row + 1, 1, row + 1, 3, styles["valueHeader"]);
            sl.SetCellStyle(row + 2, 2, row + md.Distances.Count + 1, 2, styles["number"]);

            return md.Distances.Count + 2;
        }

        private int ExportAngles(SLDocument sl, Dictionary<string, SLStyle> styles, MeasuredData md, int row)
        {
            if (md.Angles.Count == 0)
                return 0;

            sl.SetCellValue(row, 1, "Angles");
            sl.MergeWorksheetCells(row, 1, row, 3);

            sl.SetCellValue(row + 1, 1, "Name");
            sl.SetCellValue(row + 1, 2, string.Format("Value ({0})", md.Units.AngleSymbol));
            sl.SetCellValue(row + 1, 3, "Time");

            for (int i = 0; i < md.Angles.Count; i++)
            {
                var value = md.Angles[i];
                sl.SetCellValue(row + 2 + i, 1, value.Name);
                sl.SetCellValue(row + 2 + i, 2, value.Value);
                sl.SetCellValue(row + 2 + i, 3, value.Time);
            }

            sl.SetCellStyle(row, 1, row + md.Angles.Count + 1, 3, styles["normal"]);
            sl.SetCellStyle(row, 1, styles["kfHeader"]);
            sl.SetCellStyle(row + 1, 1, row + 1, 3, styles["valueHeader"]);
            sl.SetCellStyle(row + 2, 2, row + md.Angles.Count + 1, 2, styles["number"]);

            return md.Angles.Count + 2;
        }

        private int ExportTimes(SLDocument sl, Dictionary<string, SLStyle> styles, MeasuredData md, int row)
        {
            if (md.Times.Count == 0)
                return 0;

            sl.SetCellValue(row, 1, "Times");
            sl.MergeWorksheetCells(row, 1, row, 4);

            sl.SetCellValue(row + 1, 1, "Name");
            sl.SetCellValue(row + 1, 2, "Duration");
            sl.SetCellValue(row + 1, 3, "Start");
            sl.SetCellValue(row + 1, 4, "Stop");

            for (int i = 0; i < md.Times.Count; i++)
            {
                var value = md.Times[i];
                sl.SetCellValue(row + 2 + i, 1, value.Name);
                sl.SetCellValue(row + 2 + i, 2, value.Duration);
                sl.SetCellValue(row + 2 + i, 3, value.Start);
                sl.SetCellValue(row + 2 + i, 4, value.Stop);
            }

            sl.SetCellStyle(row, 1, row + md.Times.Count + 1, 4, styles["normal"]);
            sl.SetCellStyle(row, 1, styles["timeHeader"]);
            sl.SetCellStyle(row + 1, 1, row + 1, 4, styles["valueHeader"]);
            
            return md.Times.Count + 2;
        }
    }
}
