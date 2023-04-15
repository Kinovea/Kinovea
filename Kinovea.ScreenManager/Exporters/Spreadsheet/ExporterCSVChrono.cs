using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Kinovea.ScreenManager
{
    public class ExporterCSVChrono
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Exports the chronometers to CSV into a single table.
        /// Everything is indexed to the time sections.
        /// Each object exports its sections.
        /// </summary>
        public void Export(string path, MeasuredData md)
        {
            List<string> csv = new List<string>();
            
            NumberFormatInfo nfi = CSVHelper.GetCSVNFI();
            string listSeparator = CSVHelper.GetListSeparator(nfi);

            // Collect all the time section names in one big list.
            List<string> names = new List<string>();
            foreach (var t in md.Times)
            {
                foreach (var s in t.Sections)
                {
                    if (!names.Contains(s.Name))
                        names.Add(s.Name);
                }
            }

            csv.Add(WriteHeaders(names, listSeparator));

            // Loop through all the time objects.
            // Each chronometer contributes two rows, one for splits and one for cumuls.
            // A chronometer fills a column if it has a matching time section.
            foreach (var t in md.Times)
            {
                List<string> rowCumul = new List<string>();
                List<string> rowSplits = new List<string>();

                rowCumul.Add(CSVHelper.WriteCell(t.Name + " (Cumulative)"));
                rowSplits.Add(CSVHelper.WriteCell(t.Name + " (Duration)"));

                // Go through the known sections in order.
                foreach (var name in names)
                {
                    var section = t.Sections.FirstOrDefault(s => s.Name == name);
                    if (section != null)
                    {
                        // We have that section, fill the cells.
                        rowCumul.Add(CSVHelper.WriteCell(section.Cumul, nfi));
                        rowSplits.Add(CSVHelper.WriteCell(section.Duration, nfi));
                    }
                    else
                    {
                        // We don't have the section, create empty cells.
                        rowCumul.Add(CSVHelper.WriteCell(""));
                        rowSplits.Add(CSVHelper.WriteCell(""));
                    }
                }

                csv.Add(CSVHelper.MakeRow(rowCumul, listSeparator));
                csv.Add(CSVHelper.MakeRow(rowSplits, listSeparator));
            }

            if (csv.Count > 1)
                File.WriteAllLines(path, csv);
        }

        private string WriteHeaders(List<string> names, string listSeparator)
        {
            var headers = new List<string>();
            headers.Add(CSVHelper.WriteCell(""));
            foreach (var name in names)
            {
                headers.Add(CSVHelper.WriteCell(name));
            }

            return CSVHelper.MakeRow(headers, listSeparator);
        }
    }
}
