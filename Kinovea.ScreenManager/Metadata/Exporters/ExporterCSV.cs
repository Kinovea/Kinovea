using System;
using System.IO;
using System.Collections.Generic;

namespace Kinovea.ScreenManager
{
    public class ExporterCSV
    {
        private static string separator = ",";
        private static string quote = "\"";
        private static string escape = "\"\"";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Exports the time series to CSV into a single table.
        /// Everything is indexed to a single time column.
        /// Each object exports its points, each point exports its X and Y value.
        /// 
        /// Export format (compliant with RFC 4180):
        /// - Always use comma as the list separator, even in countries where the comma is the decimal separator.
        /// - Each field is enclosed in double quotes.
        /// - A double quote inside a field is escaped by another double quote preceding it.
        /// - Use the decimal separator of the current culture.
        /// - We do export numbers with comma as decimal separator if the user's system is configured this way.
        /// - Empty fields do not use the double quotes.
        /// - Records are separated by CRLF.
        ///
        /// Examples output:
        /// - "1.0", "2.0", "3.0",,,"6.0".
        /// - "1,0", "2,0", "3,0",,,"6,0".
        /// </summary>
        public void Export(string path, MeasuredData md)
        {
            List<string> csv = new List<string>();

            csv.Add(WriteHeaders(md));

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
            foreach (var valueList in points.Values)
            {
                List<string> row = new List<string>();
                foreach (var value in valueList)
                {
                    row.Add(WriteCell(value));
                }

                csv.Add(string.Join(separator, row.ToArray()));
            }

            if (csv.Count > 1)
                File.WriteAllLines(path, csv);
        }

        private string WriteHeaders(MeasuredData md)
        {
            var headers = new List<string>();
            headers.Add(WriteCell("Time"));
            foreach (var series in md.Timeseries)
            {
                foreach (var key in series.Data.Keys)
                {
                    headers.Add(WriteCell(series.Name + "/" + key + "/X"));
                    headers.Add(WriteCell(series.Name + "/" + key + "/Y"));
                }
            }

            return string.Join(separator, headers.ToArray());
        }

        
        /// <summary>
        /// Write a string value.
        /// </summary>
        private string WriteCell(string value)
        {
            value = value?.Replace(quote, escape);
            return quote + value + quote;
        }

        /// <summary>
        /// Write a numerical value using the current culture.
        /// </summary>
        private string WriteCell(float value)
        {
            if (float.IsNaN(value))
                return "";

            return WriteCell(value.ToString());
        }
    }
}
