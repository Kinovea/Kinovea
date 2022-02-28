#region License
/*
Copyright © Joan Charmant 2012.
jcharmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.
*/
#endregion
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;
using ICSharpCode.SharpZipLib.Zip;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Export ODS files.
    /// Validator: https://odfvalidator.org/
    /// </summary>
    public class ExporterODS
    { 
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Export(string path, MeasuredData md)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            using (ZipOutputStream zip = new ZipOutputStream(File.Create(path)))
            using (MemoryStream ms = new MemoryStream())
            using (XmlWriter xw = XmlWriter.Create(ms, settings))
            {
                // ODF files can't be opened if Zip64 is used in their creation.
                zip.UseZip64 = UseZip64.Off;

                WriteMimeType(zip);
                WriteManifest(zip);
                WriteContent(zip, md);
                WriteDefaultFile(zip, "office:document-styles", "styles.xml");
                WriteDefaultFile(zip, "office:document-meta", "meta.xml");
                WriteDefaultFile(zip, "office:document-settings", "settings.xml");
            }
        }

        private void WriteMimeType(ZipOutputStream zip)
        {
            // This file is not compressed to allow magic-byte identification without decompression.
            ZipEntry entry = new ZipEntry("mimetype");
            entry.CompressionMethod = CompressionMethod.Stored;
            zip.PutNextEntry(entry);
            string mimetype = "application/vnd.oasis.opendocument.spreadsheet";
            byte[] bytes = Encoding.UTF8.GetBytes(mimetype);
            zip.Write(bytes, 0, bytes.Length);
        }

        private void WriteManifest(ZipOutputStream zip)
        {
            MemoryStream ms = new MemoryStream();
            XmlTextWriter w = new XmlTextWriter(ms, new UTF8Encoding());
            w.Formatting = Formatting.Indented;

            w.WriteStartDocument();
            w.WriteStartElement("manifest:manifest");
            w.WriteAttributeString("xmlns:manifest", "urn:oasis:names:tc:opendocument:xmlns:manifest:1.0");
            w.WriteAttributeString("manifest:version", "1.2");

            w.WriteStartElement("manifest:file-entry");
            w.WriteAttributeString("manifest:full-path", "/");
            w.WriteAttributeString("manifest:version", "1.2");
            w.WriteAttributeString("manifest:media-type", "application/vnd.oasis.opendocument.spreadsheet");
            w.WriteEndElement();

            w.WriteStartElement("manifest:file-entry");
            w.WriteAttributeString("manifest:full-path", "content.xml");
            w.WriteAttributeString("manifest:media-type", "text/xml");
            w.WriteEndElement();

            w.WriteStartElement("manifest:file-entry");
            w.WriteAttributeString("manifest:full-path", "styles.xml");
            w.WriteAttributeString("manifest:media-type", "text/xml");
            w.WriteEndElement();

            w.WriteStartElement("manifest:file-entry");
            w.WriteAttributeString("manifest:full-path", "meta.xml");
            w.WriteAttributeString("manifest:media-type", "text/xml");
            w.WriteEndElement();

            w.WriteStartElement("manifest:file-entry");
            w.WriteAttributeString("manifest:full-path", "settings.xml");
            w.WriteAttributeString("manifest:media-type", "text/xml");
            w.WriteEndElement();

            w.WriteEndElement();
            w.WriteEndDocument();
            w.Flush();
            w.Close();

            byte[] bytes = ms.ToArray();
            ZipEntry entry = new ZipEntry("META-INF/manifest.xml");
            entry.CompressionMethod = CompressionMethod.Deflated;
            zip.PutNextEntry(entry);
            zip.Write(bytes, 0, bytes.Length);
        }

        private void WriteContent(ZipOutputStream zip, MeasuredData md)
        {
            MemoryStream ms = new MemoryStream();
            XmlTextWriter w = new XmlTextWriter(ms, new UTF8Encoding());
            w.Formatting = Formatting.Indented;

            w.WriteStartDocument();
            w.WriteStartElement("office:document-content");
            w.WriteAttributeString("xmlns:meta", "urn:oasis:names:tc:opendocument:xmlns:meta:1.0");
            w.WriteAttributeString("xmlns:office", "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
            w.WriteAttributeString("xmlns:fo", "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0");
            w.WriteAttributeString("xmlns:dc", "http://purl.org/dc/elements/1.1/");
            w.WriteAttributeString("xmlns:style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
            w.WriteAttributeString("xmlns:text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
            w.WriteAttributeString("xmlns:table", "urn:oasis:names:tc:opendocument:xmlns:table:1.0");
            w.WriteAttributeString("xmlns:number", "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0");
            w.WriteAttributeString("office:version", "1.2");

            w.WriteStartElement("office:body");
            WriteSpreadsheet(w, md);
            w.WriteEndElement();

            w.WriteEndElement();
            w.WriteEndDocument();
            w.Flush();
            w.Close();

            byte[] bytes = ms.ToArray();
            ZipEntry entry = new ZipEntry("content.xml");
            entry.CompressionMethod = CompressionMethod.Deflated;
            zip.PutNextEntry(entry);
            zip.Write(bytes, 0, bytes.Length);
        }

        private void WriteDefaultFile(ZipOutputStream zip, string rootElement, string filename)
        {
            MemoryStream ms = new MemoryStream();
            XmlTextWriter w = new XmlTextWriter(ms, new UTF8Encoding());
            w.Formatting = Formatting.Indented;

            w.WriteStartDocument();
            w.WriteStartElement(rootElement);
            w.WriteAttributeString("xmlns:office", "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
            w.WriteAttributeString("xmlns:grddl", "http://www.w3.org/2003/g/data-view#");
            w.WriteAttributeString("office:version", "1.2");

            w.WriteEndElement();
            w.WriteEndDocument();
            w.Flush();
            w.Close();

            byte[] bytes = ms.ToArray();
            ZipEntry entry = new ZipEntry(filename);
            entry.CompressionMethod = CompressionMethod.Deflated;
            zip.PutNextEntry(entry);
            zip.Write(bytes, 0, bytes.Length);
        }

        private void WriteSpreadsheet(XmlTextWriter w, MeasuredData md)
        {
            try
            {
                w.WriteStartElement("office:spreadsheet");

                // A table is a sheet of the document, we only have one.
                w.WriteStartElement("table:table");
                w.WriteAttributeString("table:name", "Sheet1");
                WriteColumnStyle(w);
                WriteSheet(w, md);
                w.WriteEndElement();

                w.WriteEndElement();
            }
            catch(Exception e)
            {
                log.ErrorFormat("Exception while generating ODS file. {0}", e);
            }
        }

        private void WriteColumnStyle(XmlTextWriter w)
        {
            w.WriteStartElement("table:table-column");
            w.WriteEndElement();
        }

        private void WriteSheet(XmlTextWriter w, MeasuredData md)
        {
            WriteKeyframes(w, md);
            WritePositions(w, md);
            WriteDistances(w, md);
            WriteAngles(w, md);
            WriteTimes(w, md);
            WriteTimeseries(w, md);

            // TODO: Autofit columns 1 to 4.
        }

        private void WriteKeyframes(XmlTextWriter w, MeasuredData md)
        {
            if (md.Keyframes.Count == 0)
                return;

            // Write headers.
            w.WriteStartElement("table:table-row");
            WriteCell(w, "Key images");
            w.WriteEndElement();

            w.WriteStartElement("table:table-row");
            WriteCell(w, "Name");
            WriteCell(w, string.Format("Time ({0})", md.Units.TimeSymbol));
            w.WriteEndElement();

            // Write data.
            for (int i = 0; i < md.Keyframes.Count; i++)
            {
                var kf = md.Keyframes[i];
                w.WriteStartElement("table:table-row");
                //WriteRowStyles(w, sheet, i);
                WriteCell(w, kf.Name);
                WriteCell(w, kf.Time);
                
                w.WriteEndElement();
            }
        }

        private void WritePositions(XmlTextWriter w, MeasuredData md)
        {
            if (md.Positions.Count == 0)
                return;

            WriteMargin(w);

            // Write headers.
            w.WriteStartElement("table:table-row");
            WriteCell(w, "Positions");
            w.WriteEndElement();

            w.WriteStartElement("table:table-row");
            WriteCell(w, "Name");
            WriteCell(w, string.Format("Time ({0})", md.Units.TimeSymbol));
            WriteCell(w, string.Format("X ({0})", md.Units.LengthSymbol));
            WriteCell(w, string.Format("Y ({0})", md.Units.LengthSymbol));
            w.WriteEndElement();

            // Write data.
            for (int i = 0; i < md.Positions.Count; i++)
            {
                var p = md.Positions[i];
                w.WriteStartElement("table:table-row");
                WriteCell(w, p.Name);
                WriteCell(w, p.Time);
                WriteCell(w, p.X);
                WriteCell(w, p.Y);
                w.WriteEndElement();
            }
        }

        private void WriteDistances(XmlTextWriter w, MeasuredData md)
        {
            if (md.Distances.Count == 0)
                return;

            WriteMargin(w);

            // Write headers.
            w.WriteStartElement("table:table-row");
            WriteCell(w, "Distances");
            w.WriteEndElement();

            w.WriteStartElement("table:table-row");
            WriteCell(w, "Name");
            WriteCell(w, string.Format("Time ({0})", md.Units.TimeSymbol));
            WriteCell(w, string.Format("Length ({0})", md.Units.LengthSymbol));
            w.WriteEndElement();

            // Write data.
            for (int i = 0; i < md.Distances.Count; i++)
            {
                var value = md.Distances[i];
                w.WriteStartElement("table:table-row");
                WriteCell(w, value.Name);
                WriteCell(w, value.Time);
                WriteCell(w, value.Value);
                w.WriteEndElement();
            }
        }

        private void WriteAngles(XmlTextWriter w, MeasuredData md)
        {
            if (md.Angles.Count == 0)
                return;

            WriteMargin(w);

            // Write headers.
            w.WriteStartElement("table:table-row");
            WriteCell(w, "Angles");
            w.WriteEndElement();

            w.WriteStartElement("table:table-row");
            WriteCell(w, "Name");
            WriteCell(w, string.Format("Time ({0})", md.Units.TimeSymbol));
            WriteCell(w, string.Format("Value ({0})", md.Units.AngleSymbol));
            w.WriteEndElement();

            // Write data.
            for (int i = 0; i < md.Angles.Count; i++)
            {
                var value = md.Angles[i];
                w.WriteStartElement("table:table-row");
                WriteCell(w, value.Name);
                WriteCell(w, value.Time);
                WriteCell(w, value.Value);
                w.WriteEndElement();
            }
        }

        private void WriteTimes(XmlTextWriter w, MeasuredData md)
        {
            if (md.Times.Count == 0)
                return;

            WriteMargin(w);

            // Write headers.
            w.WriteStartElement("table:table-row");
            WriteCell(w, "Times");
            w.WriteEndElement();

            w.WriteStartElement("table:table-row");
            WriteCell(w, "Name");
            WriteCell(w, string.Format("Duration ({0})", md.Units.TimeSymbol));
            WriteCell(w, string.Format("Start ({0})", md.Units.TimeSymbol));
            WriteCell(w, string.Format("Stop ({0})", md.Units.TimeSymbol));
            w.WriteEndElement();

            // Write data.
            for (int i = 0; i < md.Times.Count; i++)
            {
                var value = md.Times[i];
                w.WriteStartElement("table:table-row");
                WriteCell(w, value.Name);
                WriteCell(w, value.Duration);
                WriteCell(w, value.Start);
                WriteCell(w, value.Stop);
                w.WriteEndElement();
            }
        }

        private void WriteTimeseries(XmlTextWriter w, MeasuredData md)
        {
            if (md.Timeseries.Count == 0)
                return;

            WriteMargin(w);

            foreach (var timeline in md.Timeseries)
            {
                // Write the main header.
                w.WriteStartElement("table:table-row");
                WriteCell(w, timeline.Name);
                w.WriteEndElement();

                // Write second row of headers: point names.
                w.WriteStartElement("table:table-row");
                // First cell is empty (above Time header).
                WriteCell(w, "");
                foreach (var pointName in timeline.Data.Keys)
                {
                    WriteCell(w, pointName);
                    // Merge with next cell.
                    WriteCell(w, "");
                }
                w.WriteEndElement();

                // Third row of headers: Time and Coordinates.
                w.WriteStartElement("table:table-row");
                WriteCell(w, string.Format("Time ({0})", md.Units.TimeSymbol));
                foreach (var pointName in timeline.Data.Keys)
                {
                    WriteCell(w, string.Format("X ({0})", md.Units.LengthSymbol));
                    WriteCell(w, string.Format("Y ({0})", md.Units.LengthSymbol));
                }
                w.WriteEndElement();

                // Write data.
                for (int i = 0; i < timeline.Times.Count; i++)
                {
                    //var value = md.Times[i];
                    w.WriteStartElement("table:table-row");
                    WriteCell(w, timeline.Times[i]);
                    foreach (var pointValues in timeline.Data.Values)
                    {
                        WriteCell(w, pointValues[i].X);
                        WriteCell(w, pointValues[i].Y);
                    }
                    w.WriteEndElement();
                }

                WriteMargin(w);
            }
        }


        private void WriteMargin(XmlTextWriter w)
        {
            w.WriteStartElement("table:table-row");
            w.WriteAttributeString("table:number-rows-repeated", "2");
            w.WriteEndElement();
        }

        private void WriteRowStyles()
        {
            // Sheet s, Row i.
            // WriteRowHeight.
        }

        private void WriteCell(XmlTextWriter w, string value)
        {
            w.WriteStartElement("table:table-cell");
            w.WriteAttributeString("office:value-type", "string");
            w.WriteAttributeString("office:string-value", value);
            w.WriteEndElement();
        }

        private void WriteCell(XmlTextWriter w, float value)
        {
            w.WriteStartElement("table:table-cell");
            w.WriteAttributeString("office:value-type", "float");
            w.WriteAttributeString("office:value", value.ToString(CultureInfo.InvariantCulture));
            w.WriteEndElement();
        }
    }
}
