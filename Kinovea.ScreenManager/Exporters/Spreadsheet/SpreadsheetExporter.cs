using Kinovea.ScreenManager.Languages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Exports measurement data to spreadsheet formats.
    /// </summary>
    public class SpreadsheetExporter
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Export(SpreadsheetExportFormat format, PlayerScreen player)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = ScreenManagerLang.dlgExportSpreadsheet_Title;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.Filter = "LibreOffice calc (*.ods)|*.ods|Microsoft Excel (*.xlsx)|*.xlsx|CSV (*.csv)|*.csv|JSON (*.json)|*.json";
            int filterIndex;
            switch (format)
            {
                case SpreadsheetExportFormat.ODS:
                    filterIndex = 1;
                    break;
                case SpreadsheetExportFormat.XLSX:
                    filterIndex = 2;
                    break;
                case SpreadsheetExportFormat.CSV:
                    filterIndex = 3;
                    break;
                case SpreadsheetExportFormat.JSON:
                default:
                    filterIndex = 4;
                    break;
            }

            saveFileDialog.FilterIndex = filterIndex;
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(player.FrameServer.Metadata.VideoPath);

            if (saveFileDialog.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(saveFileDialog.FileName))
                return;

            try
            {
                Export(player.FrameServer.Metadata, saveFileDialog.FileName, format);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Exception encountered while exporting to spreadsheet.", e);
            }
        }

        private void Export(Metadata metadata, string file, SpreadsheetExportFormat format)
        {
            // The data is exported to an intermediate class containing only the measured data.
            // Each exporter then serialize this data to its target format.
            MeasuredData measuredData = metadata.CollectMeasuredData();

            switch (format)
            {
                case SpreadsheetExportFormat.ODS:
                    ExporterODS exporterODF = new ExporterODS();
                    exporterODF.Export(file, measuredData);
                    break;
                case SpreadsheetExportFormat.XLSX:
                    ExporterXLSX exporterXLSX = new ExporterXLSX();
                    exporterXLSX.Export(file, measuredData);
                    break;
                case SpreadsheetExportFormat.JSON:
                    ExporterJSON exporterJSON = new ExporterJSON();
                    exporterJSON.Export(file, measuredData);
                    break;
                case SpreadsheetExportFormat.CSV:
                    ExporterCSV exporterCSV = new ExporterCSV();
                    exporterCSV.Export(file, measuredData);
                    break;
            }
        }
    }
}
