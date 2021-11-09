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
            MetadataSerializer serializer = new MetadataSerializer();
            string kvaString = serializer.SaveToString(metadata);
            XmlDocument kva = new XmlDocument();
            kva.LoadXml(kvaString);
            
            switch(format)
            {
                case MetadataExportFormat.ODF:
                    ExporterODF exporterODF = new ExporterODF();
                    exporterODF.Export(file, kva);
                    break;
                    
                case MetadataExportFormat.MSXML:
                    ExporterMSXML exporterMSXML = new ExporterMSXML();
                    exporterMSXML.Export(file, kva);
                    break;
                 case MetadataExportFormat.XHTML:
                    ExporterXHTML exporterXHTML = new ExporterXHTML();
                    exporterXHTML.Export(file, kva);
                    break;
                 case MetadataExportFormat.TrajectoryText:
                    ExporterTrajectoryText exporterTrajText = new ExporterTrajectoryText();
                    exporterTrajText.Export(file, kva);
                    break;
            }
        }
    }
}
