using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using Kinovea.Services;
using System.Globalization;

namespace Kinovea.Services
{
    public class BenchmarkExporter
    {
        /*public void Export(Benchmarker benchmarker, BenchmarkFlags flags)
        {
            // TODO: add hardware and software information.

            /*XmlDocument doc = new XmlDocument();
            XmlElement xmlRoot = doc.CreateElement("KinoveaCaptureBenchmark");

            XmlElement xmlMetrics = doc.CreateElement("Metrics");
            ExportMetrics(doc, xmlMetrics, "Grab", benchmarker.GetGrabMetrics());
            ExportMetrics(doc, xmlMetrics, "Store", benchmarker.GetStoreMetrics());

            xmlRoot.AppendChild(xmlMetrics);
            doc.AppendChild(xmlRoot);

            if (!Directory.Exists(Software.BenchmarkDirectory))
                Directory.CreateDirectory(Software.BenchmarkDirectory);

            string filename = string.Format("{0:yyyyMMdd-HHmmss}.xml", DateTime.Now);

            doc.Save(Path.Combine(Software.BenchmarkDirectory, filename));* /
        }*/

        /*private void ExportMetrics(XmlDocument doc, XmlElement parent, string name, CounterMetrics metrics)
        {
            if (metrics == null)
                return;

            XmlElement xmlElement = doc.CreateElement(name);
            AddMetric(doc, xmlElement, "Percentile99", metrics.Percentile99, true);
            AddMetric(doc, xmlElement, "Percentile95", metrics.Percentile95, true);
            AddMetric(doc, xmlElement, "Median", metrics.Median, true);
            AddMetric(doc, xmlElement, "Average", metrics.Average, true);
            AddMetric(doc, xmlElement, "StandardDeviation", metrics.StandardDeviation, false);
            parent.AppendChild(xmlElement);
        }

        private void AddMetric(XmlDocument doc, XmlElement parent, string name, float value, bool fps)
        {
            XmlElement xmlElement = doc.CreateElement(name);
            
            if (fps)
            {
                XmlAttribute attr = doc.CreateAttribute("fps");
                attr.Value = string.Format("{0}", 1000.0f / value, CultureInfo.InvariantCulture);
                xmlElement.Attributes.Append(attr);
            }

            xmlElement.InnerText = string.Format("{0}", value, CultureInfo.InvariantCulture);
            parent.AppendChild(xmlElement);
        }*/
    }
}
