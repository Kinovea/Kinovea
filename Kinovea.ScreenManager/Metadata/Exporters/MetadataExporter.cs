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
            // Export the data to an intermediate format that will be consumed by the exporters.
            MetadataSerializer serializer = new MetadataSerializer();
            string xmlString = serializer.SaveToSpreadsheetString(metadata);
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlString);
            
            switch(format)
            {
                //case MetadataExportFormat.ODF:
                //    ExporterODF exporterODF = new ExporterODF();
                //    exporterODF.Export(file, xml);
                //    break;

                //case MetadataExportFormat.MSXML:
                //    ExporterMSXML exporterMSXML = new ExporterMSXML();
                //    exporterMSXML.Export(file, xml);
                //    break;
                case MetadataExportFormat.XHTML:
                    ExporterXHTML exporterXHTML = new ExporterXHTML();
                    exporterXHTML.Export(file, xml);
                    break;
                // case MetadataExportFormat.TrajectoryText:
                //    ExporterTrajectoryText exporterTrajText = new ExporterTrajectoryText();
                //    exporterTrajText.Export(file, xml);
                //    break;
                case MetadataExportFormat.RAW:
                default:
                    xml.Save(file);
                    break;
            }
        }
    }
}
