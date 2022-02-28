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
                WriteContent(zip);
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

        private void WriteContent(ZipOutputStream zip)
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
            WriteSpreadsheet(w);
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

        private void WriteSpreadsheet(XmlTextWriter w)
        {
            w.WriteStartElement("office:spreadsheet");

            // A table is a sheet of the document, we only have one.
            w.WriteStartElement("table:table");
            w.WriteAttributeString("table:name", "Sheet1");
            WriteColumnStyle(w);
            WriteSheet(w);
            w.WriteEndElement();

            w.WriteEndElement();
        }

        private void WriteColumnStyle(XmlTextWriter w)
        {
            w.WriteStartElement("table:table-column");
            w.WriteEndElement();
        }

        private void WriteSheet(XmlTextWriter w)
        {
            // For each object we export.

            // Write headers.
            // Write data.
            // For each row.
            w.WriteStartElement("table:table-row");
            //WriteRowStyles(w, sheet, i);

            // For each column.
            //Range cell = sheet.getRange(i, j);
            WriteCell(w);


            w.WriteEndElement();
        }

        private void WriteRowStyles()
        {
            // Sheet s, Row i.
            // WriteRowHeight.
        }

        private void WriteCell(XmlTextWriter w)
        {
            //Style style = range.getStyle();

            // merged cells.
            w.WriteStartElement("table:table-cell");
            // SetCellStyle()
            WriteNumber(w);

            w.WriteEndElement();
        }

        private void WriteNumber(XmlTextWriter w)
        {
            // http://docs.oasis-open.org/office/v1.2/part1/cd04/OpenDocument-v1.2-part1-cd04.html#a_19_387_office_value-type

            w.WriteAttributeString("office:value-type", "float");
            w.WriteAttributeString("office:value", "42");
            //w.WriteElementString("text:p", "42.2");


            //Object v = range.getValue();
            //if (v != null)
            //{
            //    OfficeValueType valueType = OfficeValueType.ofJavaType(v.getClass());
            //    valueType.write(v, out);
            //writer.writeAttribute("office:value-type", getId());
            //writer.writeAttribute("office:value", value.toString());

            //out.writeStartElement("text:p");
            //    String text = v.toString();
        }
    }
}
