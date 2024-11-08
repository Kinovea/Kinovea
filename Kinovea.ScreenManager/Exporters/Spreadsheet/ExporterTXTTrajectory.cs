using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    public class ExporterTXTTrajectory
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Exports the trajectories to TXT into multiple sections.
        /// Only individual trajectories are exported.
        /// Follows the GNU Plot format.
        /// </summary>
        public void Export(string path, MeasuredData md)
        {
            List<string> txt = new List<string>();

            txt.Add("#Kinovea Trajectory data export");
            txt.Add("#T X Y");
            txt.Add("");

            NumberFormatInfo nfi = CSVHelper.GetCSVNFI();
            string listSeparator = " ";

            foreach (var series in md.Timeseries)
            {
                // Write header
                txt.Add(string.Format("# {0}", series.Name));

                // Write Data
                // Format: Space separated, no quotes, configured decimal separator.
                // time is always numeric.
                for (int i = 0; i < series.Times.Count; i++)
                {
                    List<string> cells = new List<string>();
                    cells.Add(series.Times[i].ToString(nfi));
                    foreach (var pointValues in series.Data.Values)
                    {
                        cells.Add(pointValues[i].X.ToString(nfi));
                        cells.Add(pointValues[i].Y.ToString(nfi));
                    }
                    
                    txt.Add(string.Join(listSeparator, cells));
                }

                // Write margin
                txt.Add("");
            }

            File.WriteAllLines(path, txt);
        }
    }
}
