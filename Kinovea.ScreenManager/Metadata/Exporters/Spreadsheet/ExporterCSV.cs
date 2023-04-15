using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Kinovea.ScreenManager
{
    public class ExporterCSV
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Exports the time series to CSV into a single table.
        /// Everything is indexed to a single time column.
        /// Each object exports its points, each point exports its X and Y value.
        ///
        /// Examples output:
        /// - "1.0","2.0","3.0",,,"6.0"
        /// - "1,0";"2,0";"3,0";;;"6,0"
        /// </summary>
        public void Export(string path, MeasuredData md)
        {
            List<string> csv = new List<string>();
            
            NumberFormatInfo nfi = CSVHelper.GetCSVNFI();
            string listSeparator = CSVHelper.GetListSeparator(nfi);
            
            csv.Add(WriteHeaders(md, listSeparator));

            // Consolidate all the data in a single table indexed by global time.
            SortedDictionary<float, List<float>> points = new SortedDictionary<float, List<float>>();

            // Compute the total number of columns.
            int cols = 1;
            foreach (var series in md.Timeseries)
                foreach (var pair in series.Data)
                    cols += 2;

            // Loop through all the data and add time rows as we go.
            var col = 1;
            foreach (var series in md.Timeseries)
            {
                var times = series.Times;
                foreach (var pointList in series.Data)
                {
                    for (int i = 0; i < times.Count; i++)
                    {
                        float time = times[i];
                        if (!points.ContainsKey(time))
                        {
                            points[time] = new List<float>();
                            points[time].Add(time);

                            // Initialize the values for all series.
                            for (int j = 1; j < cols; j++)
                                points[time].Add(float.NaN);
                        }

                        points[time][col + 0] = pointList.Value[i].X;
                        points[time][col + 1] = pointList.Value[i].Y;
                    }

                    col += 2;
                }
            }

            // Project the table to CSV cells.
            foreach (var p in points)
            {
                IEnumerable<string> row = p.Value.Select(v => CSVHelper.WriteCell(v, nfi));
                csv.Add(CSVHelper.MakeRow(row, listSeparator));
            }

            if (csv.Count > 1)
                File.WriteAllLines(path, csv);
        }

        private string WriteHeaders(MeasuredData md, string listSeparator)
        {
            var headers = new List<string>();
            headers.Add(CSVHelper.WriteCell("Time"));
            foreach (var series in md.Timeseries)
            {
                foreach (var key in series.Data.Keys)
                {
                    headers.Add(CSVHelper.WriteCell(series.Name + "/" + key + "/X"));
                    headers.Add(CSVHelper.WriteCell(series.Name + "/" + key + "/Y"));
                }
            }

            return CSVHelper.MakeRow(headers, listSeparator);
        }
    }
}
