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
using SpreadsheetLight;
using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;

namespace Kinovea.ScreenManager
{
    public class ExporterXLSX
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Export(string path, MeasuredData md)
        {
            using (SLDocument sl = new SLDocument())
            {
                int row = 2;
                row += ExportKeyframes(sl, md, row);
                row++;
                row += ExportPositions(sl, md, row);

                sl.SaveAs(path);
            }
        }

        private int ExportKeyframes(SLDocument sl, MeasuredData md, int row)
        {
            if (md.Keyframes.Count == 0)
                return 0;
            
            int writtenRows = 0;
            
            sl.SetCellValue(row, 1, "Keyframes");
            sl.MergeWorksheetCells(row, 1, row, 2);
            writtenRows++;

            sl.SetCellValue(row + writtenRows, 1, "Name");
            sl.SetCellValue(row + writtenRows, 2, "Time");
            writtenRows++;

            foreach (MeasuredDataKeyframe kf in md.Keyframes)
            {
                sl.SetCellValue(row + writtenRows, 1, kf.Name);
                sl.SetCellValue(row + writtenRows, 2, kf.Time);
                writtenRows++;
            }

            return writtenRows;
        }

        private int ExportPositions(SLDocument sl, MeasuredData md, int row)
        {
            if (md.Positions.Count == 0)
                return 0;

            int writtenRows = 0;

            sl.SetCellValue(row, 1, "Positions");
            sl.MergeWorksheetCells(row, 1, row, 4);
            writtenRows++;

            sl.SetCellValue(row + writtenRows, 1, "Name");
            sl.SetCellValue(row + writtenRows, 2, "X");
            sl.SetCellValue(row + writtenRows, 3, "Y");
            sl.SetCellValue(row + writtenRows, 4, "Time");
            writtenRows++;

            foreach (MeasuredDataPosition pos in md.Positions)
            {
                sl.SetCellValue(row + writtenRows, 1, pos.Name);
                sl.SetCellValue(row + writtenRows, 2, pos.X);
                sl.SetCellValue(row + writtenRows, 3, pos.Y);
                sl.SetCellValue(row + writtenRows, 4, pos.Time);
                writtenRows++;
            }

            return writtenRows;
        }
    }
}
