using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Kinovea.Services;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    public static class CSVHelper
    {
        private static string quote = "\"";
        private static string escape = "\"\"";

        /// <summary>
        /// Returns a NumberFormatInfo with the configured decimal separator.
        /// This either uses the system decimal separator or the user override.
        /// </summary>
        public static NumberFormatInfo GetCSVNFI()
        {
            NumberFormatInfo nfi = new NumberFormatInfo();
            switch (PreferencesManager.PlayerPreferences.CSVDecimalSeparator)
            {
                case CSVDecimalSeparator.Point:
                    nfi.NumberDecimalSeparator = ".";
                    break;
                case CSVDecimalSeparator.Comma:
                    nfi.NumberDecimalSeparator = ",";
                    break;
                case CSVDecimalSeparator.System:
                default:
                    NumberFormatInfo nfiSystem = CultureInfo.CurrentCulture.NumberFormat;
                    nfi.NumberDecimalSeparator = nfiSystem.NumberDecimalSeparator;
                    break;
            }

            return nfi;
        }

        /// <summary>
        /// Returns the list separator.
        /// </summary>
        public static string GetListSeparator(NumberFormatInfo nfi)
        {
            string listSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
            if (nfi.NumberDecimalSeparator == listSeparator && listSeparator == ",")
                listSeparator = ";";

            return listSeparator;
        }

        /// <summary>
        /// Write a string value.
        /// </summary>
        public static string WriteCell(string value)
        {
            value = value?.Replace(quote, escape);
            return quote + value + quote;
        }

        /// <summary>
        /// Write a numerical value.
        /// </summary>
        public static string WriteCell(float value, NumberFormatInfo nfi)
        {
            if (float.IsNaN(value))
                return "";

            return WriteCell(value.ToString(nfi));
        }

        public static string MakeRow(IEnumerable<string> cells, string listSeparator)
        {
            return string.Join(listSeparator, cells.ToArray());
        }

        public static void CopyToClipboard(List<string> csv)
        {
            if (csv.Count <= 1)
                return;

            StringBuilder b = new StringBuilder();
            foreach (string line in csv)
                b.AppendLine(line);

            string text = b.ToString();
            Clipboard.SetText(text);
        }
    }
}
