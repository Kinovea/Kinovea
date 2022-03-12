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
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class ExporterXLSX
    {
        private static readonly int margin = 2;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Export(string path, MeasuredData md)
        {
            using (SLDocument sl = new SLDocument())
            {
                Dictionary<string, SLStyle> styles = CreateStyles(sl);
                
                int row = 2;
                row += ExportKeyframes(sl, styles, md, row);
                row += ExportPositions(sl, styles, md, row);
                row += ExportDistances(sl, styles, md, row);
                row += ExportAngles(sl, styles, md, row);
                row += ExportTimes(sl, styles, md, row);
                row += ExportTimeseries(sl, styles, md, row);
                
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
            normal.Font.FontName = "Calibri";
            setAllBorders(normal);

            SLStyle header = normal.Clone();
            header.Alignment.Horizontal = HorizontalAlignmentValues.Center;
            setAllBorders(header);

            SLStyle kfHeader = header.Clone();
            kfHeader.Font.Bold = true;
            setBackgroundColor(kfHeader, System.Drawing.Color.FromArgb(210, 245, 176));

            SLStyle timeHeader = header.Clone();
            timeHeader.Font.Bold = true;
            setBackgroundColor(timeHeader, System.Drawing.Color.FromArgb(194, 223, 255));

            SLStyle trackHeader = header.Clone();
            trackHeader.Font.Bold = true;
            setBackgroundColor(trackHeader, System.Drawing.Color.FromArgb(255, 221, 253));

            SLStyle valueHeader = header.Clone();
            setBackgroundColor(valueHeader, System.Drawing.Color.FromArgb(232, 232, 232));

            SLStyle number = normal.Clone();
            number.FormatCode = "0.00";

            SLStyle time = normal.Clone();
            TimecodeFormat tcf = PreferencesManager.PlayerPreferences.TimecodeFormat;
            switch (tcf)
            {
                case TimecodeFormat.Frames:
                    time.FormatCode = "0";
                    break;
                case TimecodeFormat.ClassicTime:
                case TimecodeFormat.Normalized:
                case TimecodeFormat.TimeAndFrames:
                    time.FormatCode = "0.000";
                    break;
                default:
                    time.FormatCode = "0.###";
                    break;
            }
            
            styles.Add("normal", normal);
            styles.Add("kfHeader", kfHeader);
            styles.Add("timeHeader", timeHeader);
            styles.Add("trackHeader", trackHeader);
            styles.Add("valueHeader", valueHeader);
            styles.Add("number", number);
            styles.Add("time", time);

            return styles;
        }

        private int ExportKeyframes(SLDocument sl, Dictionary<string, SLStyle> styles, MeasuredData md, int row)
        {
            if (md.Keyframes.Count == 0)
                return 0;
            
            sl.SetCellValue(row, 1, "Key images");
            sl.MergeWorksheetCells(row, 1, row, 2);

            sl.SetCellValue(row + 1, 1, "Name");
            sl.SetCellValue(row + 1, 2, string.Format("Time ({0})", md.Units.TimeSymbol));

            for (int i = 0; i < md.Keyframes.Count; i++)
            {
                var kf = md.Keyframes[i];
                sl.SetCellValue(row + 2 + i, 1, kf.Name);
                sl.SetCellValue(row + 2 + i, 2, kf.Time);
            }

            sl.SetCellStyle(row, 1, row + md.Keyframes.Count + 1, 2, styles["normal"]);
            sl.SetCellStyle(row, 1, styles["kfHeader"]);
            sl.SetCellStyle(row + 1, 1, row + 1, 2, styles["valueHeader"]);
            sl.SetCellStyle(row + 2, 2, row + md.Keyframes.Count + 1, 2, styles["time"]);

            return md.Keyframes.Count + 2 + margin;
        }

        private int ExportPositions(SLDocument sl, Dictionary<string, SLStyle> styles, MeasuredData md, int row)
        {
            if (md.Positions.Count == 0)
                return 0;

            sl.SetCellValue(row, 1, "Positions");
            sl.MergeWorksheetCells(row, 1, row, 4);
            
            sl.SetCellValue(row + 1, 1, "Name");
            sl.SetCellValue(row + 1, 2, string.Format("Time ({0})", md.Units.TimeSymbol));
            sl.SetCellValue(row + 1, 3, string.Format("X ({0})", md.Units.LengthSymbol));
            sl.SetCellValue(row + 1, 4, string.Format("Y ({0})", md.Units.LengthSymbol));

            for (int i = 0; i < md.Positions.Count; i++)
            {
                var p = md.Positions[i];
                sl.SetCellValue(row + 2 + i, 1, p.Name);
                sl.SetCellValue(row + 2 + i, 2, p.Time);
                sl.SetCellValue(row + 2 + i, 3, p.X);
                sl.SetCellValue(row + 2 + i, 4, p.Y);
            }

            sl.SetCellStyle(row, 1, row + md.Positions.Count + 1, 4, styles["normal"]);
            sl.SetCellStyle(row, 1, styles["kfHeader"]);
            sl.SetCellStyle(row + 1, 1, row + 1, 4, styles["valueHeader"]);
            sl.SetCellStyle(row + 2, 2, row + md.Positions.Count + 1, 2, styles["time"]);
            sl.SetCellStyle(row + 2, 3, row + md.Positions.Count + 1, 4, styles["number"]);

            return md.Positions.Count + 2 + margin;
        }

        private int ExportDistances(SLDocument sl, Dictionary<string, SLStyle> styles, MeasuredData md, int row)
        {
            if (md.Distances.Count == 0)
                return 0;

            sl.SetCellValue(row, 1, "Distances");
            sl.MergeWorksheetCells(row, 1, row, 3);

            sl.SetCellValue(row + 1, 1, "Name");
            sl.SetCellValue(row + 1, 2, string.Format("Time ({0})", md.Units.TimeSymbol));
            sl.SetCellValue(row + 1, 3, string.Format("Length ({0})", md.Units.LengthSymbol));

            for (int i = 0; i < md.Distances.Count; i++)
            {
                var value = md.Distances[i];
                sl.SetCellValue(row + 2 + i, 1, value.Name);
                sl.SetCellValue(row + 2 + i, 2, value.Time);
                sl.SetCellValue(row + 2 + i, 3, value.Value);
            }

            sl.SetCellStyle(row, 1, row + md.Distances.Count + 1, 3, styles["normal"]);
            sl.SetCellStyle(row, 1, styles["kfHeader"]);
            sl.SetCellStyle(row + 1, 1, row + 1, 3, styles["valueHeader"]);
            sl.SetCellStyle(row + 2, 2, row + md.Distances.Count + 1, 2, styles["time"]);
            sl.SetCellStyle(row + 2, 3, row + md.Distances.Count + 1, 3, styles["number"]);

            return md.Distances.Count + 2 + margin;
        }

        private int ExportAngles(SLDocument sl, Dictionary<string, SLStyle> styles, MeasuredData md, int row)
        {
            if (md.Angles.Count == 0)
                return 0;

            sl.SetCellValue(row, 1, "Angles");
            sl.MergeWorksheetCells(row, 1, row, 3);

            sl.SetCellValue(row + 1, 1, "Name");
            sl.SetCellValue(row + 1, 2, string.Format("Time ({0})", md.Units.TimeSymbol));
            sl.SetCellValue(row + 1, 3, string.Format("Value ({0})", md.Units.AngleSymbol));

            for (int i = 0; i < md.Angles.Count; i++)
            {
                var value = md.Angles[i];
                sl.SetCellValue(row + 2 + i, 1, value.Name);
                sl.SetCellValue(row + 2 + i, 2, value.Time);
                sl.SetCellValue(row + 2 + i, 3, value.Value);
            }

            sl.SetCellStyle(row, 1, row + md.Angles.Count + 1, 3, styles["normal"]);
            sl.SetCellStyle(row, 1, styles["kfHeader"]);
            sl.SetCellStyle(row + 1, 1, row + 1, 3, styles["valueHeader"]);
            sl.SetCellStyle(row + 2, 2, row + md.Angles.Count + 1, 2, styles["time"]);
            sl.SetCellStyle(row + 2, 3, row + md.Angles.Count + 1, 3, styles["number"]);

            return md.Angles.Count + 2 + margin;
        }

        private int ExportTimes(SLDocument sl, Dictionary<string, SLStyle> styles, MeasuredData md, int row)
        {
            if (md.Times.Count == 0)
                return 0;

            sl.SetCellValue(row, 1, "Times");
            sl.MergeWorksheetCells(row, 1, row, 4);

            sl.SetCellValue(row + 1, 1, "Name");
            sl.SetCellValue(row + 1, 2, string.Format("Duration ({0})", md.Units.TimeSymbol));
            sl.SetCellValue(row + 1, 3, string.Format("Start ({0})", md.Units.TimeSymbol));
            sl.SetCellValue(row + 1, 4, string.Format("Stop ({0})", md.Units.TimeSymbol));

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
            sl.SetCellStyle(row + 2, 2, row + md.Times.Count + 1, 4, styles["time"]);

            return md.Times.Count + 2 + margin;
        }

        private int ExportTimeseries(SLDocument sl, Dictionary<string, SLStyle> styles, MeasuredData md, int row)
        {
            if (md.Timeseries.Count == 0)
                return 0;

            int oldRow = row;
            foreach (var timeline in md.Timeseries)
            {
                sl.SetCellValue(row, 1, timeline.Name);
                sl.SetCellValue(row + 2, 1, string.Format("Time ({0})", md.Units.TimeSymbol));
                
                // Add the headers for the individual trackable points.
                int col = 1;
                foreach (var pointName in timeline.Data.Keys)
                {
                    // Header with the name of the trackable point, such as "elbow".
                    sl.SetCellValue(row + 1, col + 1, pointName);
                    sl.MergeWorksheetCells(row + 1, col + 1, row + 1, col + 2);

                    // Second row of headers with the data column.
                    sl.SetCellValue(row + 2, col + 1, string.Format("X ({0})", md.Units.LengthSymbol));
                    sl.SetCellValue(row + 2, col + 2, string.Format("Y ({0})", md.Units.LengthSymbol));

                    col += 2;
                }

                // Merge the top-level header all the way to the right.
                sl.MergeWorksheetCells(row, 1, row, col);

                // Apply styles for the 3 header lines.
                sl.SetCellStyle(row, 1, styles["trackHeader"]);
                sl.SetCellStyle(row + 1, 1, row + 1, col, styles["valueHeader"]);
                sl.SetCellStyle(row + 2, 1, row + 2, col, styles["valueHeader"]);

                // Add each time row.
                for (int i = 0; i < timeline.Times.Count; i++)
                {
                    sl.SetCellValue(row + 3 + i, 1, timeline.Times[i]);
                    col = 2;
                    foreach (var pointValues in timeline.Data.Values)
                    {
                        sl.SetCellValue(row + 3 + i, col + 0, pointValues[i].X);
                        sl.SetCellValue(row + 3 + i, col + 1, pointValues[i].Y);
                        col += 2;
                    }
                }

                sl.SetCellStyle(row, 1, row + timeline.Times.Count + 2, timeline.Data.Keys.Count * 2 + 1, styles["normal"]);
                sl.SetCellStyle(row + 3, 1, row + timeline.Times.Count + 2, 1, styles["time"]);
                sl.SetCellStyle(row + 3, 2, row + timeline.Times.Count + 2, timeline.Data.Keys.Count * 2 + 1, styles["number"]);

                row += timeline.Times.Count + 3 + margin;
            }

            return (row - oldRow);
        }
    }
}
