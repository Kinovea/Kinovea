using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Kinovea.ScreenManager
{
    public static class MetadataExporter
    {
        public static void Export(Metadata metadata, string file, MetadataExportFormat format)
        {
            // The data is exported to an intermediate class containing only the measured data.
            // Each exporter then serialize this data to its target format.
            MeasuredData measuredData = metadata.CollectMeasuredData();

            switch (format)
            {
                case MetadataExportFormat.ODS:
                    ExporterODS exporterODF = new ExporterODS();
                    exporterODF.Export(file, measuredData);
                    break;
                case MetadataExportFormat.XLSX:
                    ExporterXLSX exporterXLSX = new ExporterXLSX();
                    exporterXLSX.Export(file, measuredData);
                    break;
                case MetadataExportFormat.JSON:
                    ExporterJSON exporterJSON = new ExporterJSON();
                    exporterJSON.Export(file, measuredData);
                    break;
            }
        }
    }
}
