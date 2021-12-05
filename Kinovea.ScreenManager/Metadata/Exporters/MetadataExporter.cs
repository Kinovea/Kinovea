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

            //MetadataSerializer serializer = new MetadataSerializer();
            //string xmlString = serializer.SaveToSpreadsheetString(metadata);
            //XmlDocument xml = new XmlDocument();
            //xml.LoadXml(xmlString);

            switch (format)
            {
                //    //case MetadataExportFormat.ODF:
                //    //    ExporterODF exporterODF = new ExporterODF();
                //    //    exporterODF.Export(file, xml);
                //    //    break;

                case MetadataExportFormat.XLSX:
                    ExporterXLSX exporterXLSX = new ExporterXLSX();
                    exporterXLSX.Export(file, measuredData);
                    break;

                    //    case MetadataExportFormat.XHTML:
                    //        ExporterXHTML exporterXHTML = new ExporterXHTML();
                    //        exporterXHTML.Export(file, xml);
                    //        break;
                    //    // case MetadataExportFormat.TrajectoryText:
                    //    //    ExporterTrajectoryText exporterTrajText = new ExporterTrajectoryText();
                    //    //    exporterTrajText.Export(file, xml);
                    //    //    break;
                    //    case MetadataExportFormat.RAW:
                    //    default:
                    //        xml.Save(file);
                    //        break;
                    //}
            }
        }
    }
}
