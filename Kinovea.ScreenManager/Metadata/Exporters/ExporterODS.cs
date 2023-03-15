using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;
using ICSharpCode.SharpZipLib.Zip;
using Kinovea.Services;

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
            w.WriteAttributeString("xmlns:svg", "urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0");
            w.WriteAttributeString("xmlns:text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
            w.WriteAttributeString("xmlns:table", "urn:oasis:names:tc:opendocument:xmlns:table:1.0");
            w.WriteAttributeString("xmlns:number", "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0");
            w.WriteAttributeString("office:version", "1.2");

            try
            {
                WriteFonts(w);
                WriteAutomaticStyles(w);
                WriteBody(w, md);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Exception while generating ODS file. {0}", e);
            }

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

        private void WriteFonts(XmlTextWriter w)
        {
            // http://docs.oasis-open.org/office/v1.2/part1/cd04/OpenDocument-v1.2-part1-cd04.html#a_3_14__office_font-face-decls_
            w.WriteStartElement("office:font-face-decls");
            
            w.WriteStartElement("style:font-face");
            w.WriteAttributeString("style:name", "Calibri");
            w.WriteAttributeString("svg:font-family", "Calibri");
            w.WriteEndElement();

            w.WriteStartElement("style:font-face");
            w.WriteAttributeString("style:name", "Liberation Sans");
            w.WriteAttributeString("svg:font-family", "&apos;Liberation Sans&apos;");
            w.WriteEndElement();

            w.WriteStartElement("style:font-face");
            w.WriteAttributeString("style:name", "Arial");
            w.WriteAttributeString("svg:font-family", "Arial");
            w.WriteAttributeString("style:font-family-generic", "system");
            w.WriteEndElement();

            w.WriteEndElement();
        }

        private void WriteNumberStyles(XmlTextWriter w)
        {
            // General values
            w.WriteStartElement("number:number-style");
            w.WriteAttributeString("style:name", "N0");
            {
                w.WriteStartElement("number:number");
                w.WriteAttributeString("number:min-integer-digits", "1");
                w.WriteAttributeString("number:decimal-places", "2");
                w.WriteEndElement();
            }
            w.WriteEndElement();

            // Times
            TimecodeFormat tcf = PreferencesManager.PlayerPreferences.TimecodeFormat;
            int decimalPlaces = -1;
            switch (tcf)
            {
                case TimecodeFormat.Frames:
                    decimalPlaces = 0;
                    break;
                case TimecodeFormat.ClassicTime:
                case TimecodeFormat.Normalized:
                case TimecodeFormat.TimeAndFrames:
                    decimalPlaces = 3;
                    break;
            }

            w.WriteStartElement("number:number-style");
            w.WriteAttributeString("style:name", "N1");
            {
                w.WriteStartElement("number:number");
                w.WriteAttributeString("number:min-integer-digits", "1");
                if (decimalPlaces >= 0)
                    w.WriteAttributeString("number:decimal-places", decimalPlaces.ToString());
                w.WriteEndElement();
            }
            w.WriteEndElement();
        }

        private void WriteColumnStyle(XmlTextWriter w)
        {
            // Unfortunately it looks like LibreOffice doesn't implement Open Document's "style:use-optimal-column-width".
            // https://bugs.documentfoundation.org/show_bug.cgi?id=113604

            w.WriteStartElement("style:style");
            w.WriteAttributeString("style:name", "CO1");
            w.WriteAttributeString("style:family", "table-column");
            {
                w.WriteStartElement("style:table-column-properties");
                w.WriteAttributeString("style:use-optimal-column-width", "true");
                w.WriteEndElement();
            }
            w.WriteEndElement();
        }

        private void WriteTableCellStyle(XmlTextWriter w, string name, string bgColor = "", bool centered = false, bool bold = false, string dataStyleName = "")
        {
            w.WriteStartElement("style:style");
            w.WriteAttributeString("style:name", name);
            w.WriteAttributeString("style:family", "table-cell");

            if (!string.IsNullOrEmpty(dataStyleName))
            {
                w.WriteAttributeString("style:data-style-name", dataStyleName);
            }

            // Indentation for clarity.
            {
                w.WriteStartElement("style:table-cell-properties");
                w.WriteAttributeString("fo:border", "0.06pt solid #000000");
                if (!string.IsNullOrEmpty(bgColor))
                    w.WriteAttributeString("fo:background-color", bgColor);
                w.WriteEndElement();

                w.WriteStartElement("style:paragraph-properties");
                if (centered)
                    w.WriteAttributeString("fo:text-align", "center");
                w.WriteEndElement();

                w.WriteStartElement("style:text-properties");
                w.WriteAttributeString("style:font-name", "Calibri");
                w.WriteAttributeString("fo:font-size", "11pt");
                if (bold)
                    w.WriteAttributeString("fo:font-weight", "bold");
                w.WriteEndElement();
            }
            
            w.WriteEndElement();
        }


        private void WriteAutomaticStyles(XmlTextWriter w)
        {
            //--------------------------------------------------------------------------------    
            // Automatic styles: styles applied by an application behind the scenes when the user assigns a style to a 
            // specific column/row/cell or range of cells. This corresponds to "Formatting" in Calc, whereas "Style" refers
            // to the built-in named styles (Default, Accent, Heading, etc.).
            // The automatic styles are not named/visible in Calc UI and can't be reused by the user except via "clone formatting" menu.
            //
            // It seems impossible to use a custom style as a parent of another custom style, and create a hierarchy of styles.
            // http://docs.oasis-open.org/office/v1.2/part1/cd04/OpenDocument-v1.2-part1-cd04.html#attribute-style_parent-style-name
            // "The parent style cannot be an automatic style and shall exist."
            // When I define a hierarchy of automatic styles here, the properties of the parent styles are not picked up by the children styles.
            // When generating files from Calc, it's also never generating a hierarchy of automatic styles, it always states the full 
            // description of each property for each style.
            // Exporting the styles.xml file with customised "Default" style also doesn't seem to work.
            // Using a hierarchy of automatic styles in styles.xml also doesn't seem to work.
            //
            // Basically it means we need to describe styles in full instead of relying on inherited properties.
            //--------------------------------------------------------------------------------

            w.WriteStartElement("office:automatic-styles");
            {
                WriteNumberStyles(w);
                WriteColumnStyle(w);
                WriteTableCellStyle(w, "kfHeader", "#d2f5b0", true, true);
                WriteTableCellStyle(w, "timeHeader", "#c2dfff", true, true);
                WriteTableCellStyle(w, "trackHeader", "#ffddfd", true, true);
                WriteTableCellStyle(w, "valueHeader", "#e8e8e8", true, false);
                WriteTableCellStyle(w, "name", "", false, false);
                WriteTableCellStyle(w, "number", "", false, false, "N0");
                WriteTableCellStyle(w, "time", "", false, false, "N1");
            }
            w.WriteEndElement();
        }

        private void WriteBody(XmlTextWriter w, MeasuredData md)
        {
            w.WriteStartElement("office:body");
            w.WriteStartElement("office:spreadsheet");

            // A table is one sheet of the document, we only have one.
            w.WriteStartElement("table:table");
            w.WriteAttributeString("table:name", "Sheet1");
            WriteTable(w, md);
            w.WriteEndElement();

            w.WriteEndElement();
            w.WriteEndElement();
        }
       
        /// <summary>
        /// Writes the actual content of the whole spreadsheet.
        /// </summary>
        private void WriteTable(XmlTextWriter w, MeasuredData md)
        {
            w.WriteStartElement("table:table-column");
            w.WriteAttributeString("table:style-name", "CO1");
            w.WriteEndElement();

            WriteKeyframes(w, md);
            WritePositions(w, md);
            WriteDistances(w, md);
            WriteAngles(w, md);
            WriteTimes(w, md);
            WriteTimeseries(w, md);
        }

        private void WriteKeyframes(XmlTextWriter w, MeasuredData md)
        {
            if (md.Keyframes.Count == 0)
                return;

            // Write headers.
            w.WriteStartElement("table:table-row");
            WriteCell(w, "Key images", "kfHeader", 2);
            w.WriteEndElement();

            w.WriteStartElement("table:table-row");
            WriteCell(w, "Name", "valueHeader");
            WriteCell(w, string.Format("Time ({0})", md.Units.TimeSymbol), "valueHeader");
            w.WriteEndElement();

            // Write data.
            for (int i = 0; i < md.Keyframes.Count; i++)
            {
                var kf = md.Keyframes[i];
                w.WriteStartElement("table:table-row");
                WriteCell(w, kf.Name, "name");
                WriteCell(w, kf.Time, "time");
                
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
            WriteCell(w, "Positions", "kfHeader", 4);
            w.WriteEndElement();

            w.WriteStartElement("table:table-row");
            WriteCell(w, "Name", "valueHeader");
            WriteCell(w, string.Format("Time ({0})", md.Units.TimeSymbol), "valueHeader");
            WriteCell(w, string.Format("X ({0})", md.Units.LengthSymbol), "valueHeader");
            WriteCell(w, string.Format("Y ({0})", md.Units.LengthSymbol), "valueHeader");
            w.WriteEndElement();

            // Write data.
            for (int i = 0; i < md.Positions.Count; i++)
            {
                var p = md.Positions[i];
                w.WriteStartElement("table:table-row");
                WriteCell(w, p.Name, "name");
                WriteCell(w, p.Time, "time");
                WriteCell(w, p.X, "number");
                WriteCell(w, p.Y, "number");
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
            WriteCell(w, "Distances", "kfHeader", 3);
            w.WriteEndElement();

            w.WriteStartElement("table:table-row");
            WriteCell(w, "Name", "valueHeader");
            WriteCell(w, string.Format("Time ({0})", md.Units.TimeSymbol), "valueHeader");
            WriteCell(w, string.Format("Length ({0})", md.Units.LengthSymbol), "valueHeader");
            w.WriteEndElement();

            // Write data.
            for (int i = 0; i < md.Distances.Count; i++)
            {
                var value = md.Distances[i];
                w.WriteStartElement("table:table-row");
                WriteCell(w, value.Name, "name");
                WriteCell(w, value.Time, "time");
                WriteCell(w, value.Value, "number");
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
            WriteCell(w, "Angles", "kfHeader", 3);
            w.WriteEndElement();

            w.WriteStartElement("table:table-row");
            WriteCell(w, "Name", "valueHeader");
            WriteCell(w, string.Format("Time ({0})", md.Units.TimeSymbol), "valueHeader");
            WriteCell(w, string.Format("Value ({0})", md.Units.AngleSymbol), "valueHeader");
            w.WriteEndElement();

            // Write data.
            for (int i = 0; i < md.Angles.Count; i++)
            {
                var value = md.Angles[i];
                w.WriteStartElement("table:table-row");
                WriteCell(w, value.Name, "name");
                WriteCell(w, value.Time, "time");
                WriteCell(w, value.Value, "number");
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
            WriteCell(w, "Times", "timeHeader", 5);
            w.WriteEndElement();

            w.WriteStartElement("table:table-row");
            WriteCell(w, "Name", "valueHeader");
            WriteCell(w, string.Format("Duration ({0})", md.Units.TimeSymbol), "valueHeader");
            WriteCell(w, string.Format("Cumulative ({0})", md.Units.TimeSymbol), "valueHeader");
            WriteCell(w, string.Format("Start ({0})", md.Units.TimeSymbol), "valueHeader");
            WriteCell(w, string.Format("Stop ({0})", md.Units.TimeSymbol), "valueHeader");
            w.WriteEndElement();

            // Write data.
            for (int i = 0; i < md.Times.Count; i++)
            {
                var value = md.Times[i];
                w.WriteStartElement("table:table-row");
                WriteCell(w, value.Name, "name");
                WriteCell(w, value.Duration, "time");
                WriteCell(w, value.Cumul, "time");
                WriteCell(w, value.Start, "time");
                WriteCell(w, value.Stop, "time");
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
                WriteCell(w, timeline.Name, "trackHeader", timeline.Data.Keys.Count * 2 + 1);
                w.WriteEndElement();

                // Write second row of headers: point names.
                w.WriteStartElement("table:table-row");
                // First cell is empty (above Time header).
                WriteCell(w, "", "valueHeader");
                foreach (var pointName in timeline.Data.Keys)
                {
                    WriteCell(w, pointName, "valueHeader", 2);
                    WriteCell(w, "", "valueHeader");
                }
                w.WriteEndElement();

                // Third row of headers: Time and Coordinates.
                w.WriteStartElement("table:table-row");
                WriteCell(w, string.Format("Time ({0})", md.Units.TimeSymbol), "valueHeader");
                foreach (var pointName in timeline.Data.Keys)
                {
                    WriteCell(w, string.Format("X ({0})", md.Units.LengthSymbol), "valueHeader");
                    WriteCell(w, string.Format("Y ({0})", md.Units.LengthSymbol), "valueHeader");
                }
                w.WriteEndElement();

                // Write data.
                for (int i = 0; i < timeline.Times.Count; i++)
                {
                    //var value = md.Times[i];
                    w.WriteStartElement("table:table-row");
                    WriteCell(w, timeline.Times[i], "time");
                    foreach (var pointValues in timeline.Data.Values)
                    {
                        WriteCell(w, pointValues[i].X, "number");
                        WriteCell(w, pointValues[i].Y, "number");
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
            w.WriteStartElement("table:table-cell");
            w.WriteEndElement();
            w.WriteEndElement();
        }

        /// <summary>
        /// Writes a string value cell with optional style and span.
        /// </summary>
        private void WriteCell(XmlTextWriter w, string value, string style = "", int span = 1)
        {
            w.WriteStartElement("table:table-cell");

            if (!string.IsNullOrEmpty(style))
                w.WriteAttributeString("table:style-name", style);

            if (span > 1)
                w.WriteAttributeString("table:number-columns-spanned", span.ToString());

            w.WriteAttributeString("office:value-type", "string");
            w.WriteAttributeString("office:string-value", value);

            w.WriteElementString("text:p", value);

            w.WriteEndElement();
        }

        /// <summary>
        /// Writes a float value cell.
        /// </summary>
        private void WriteCell(XmlTextWriter w, float value, string style = "")
        {
            w.WriteStartElement("table:table-cell");
            if (!string.IsNullOrEmpty(style))
                w.WriteAttributeString("table:style-name", style);
            w.WriteAttributeString("office:value-type", "float");
            w.WriteAttributeString("office:value", value.ToString(CultureInfo.InvariantCulture));

            w.WriteElementString("text:p", value.ToString());

            w.WriteEndElement();
        }
    }
}
