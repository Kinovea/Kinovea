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
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;

using ICSharpCode.SharpZipLib.Zip;

namespace Kinovea.ScreenManager
{
    public class ExporterODF
    { 
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public void Export(string path, XmlDocument kva)
        {
            string stylesheet = Application.StartupPath + "\\xslt\\kva2odf-en.xsl";
            XslCompiledTransform xslt = new XslCompiledTransform();
            xslt.Load(stylesheet);
            
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            
            try
            {
                using (ZipOutputStream zos = new ZipOutputStream(File.Create(path)))
                using (MemoryStream ms = new MemoryStream())
                using (XmlWriter xw = XmlWriter.Create(ms, settings))
                {
                    zos.UseZip64 = UseZip64.Dynamic;
                
                    xslt.Transform(kva, xw);
                
                    AddODFZipFile(zos, "content.xml", ms.ToArray());
                
                    AddODFZipFile(zos, "meta.xml", GetODFMeta());
                    AddODFZipFile(zos, "settings.xml", GetODFSettings());
                    AddODFZipFile(zos, "styles.xml", GetODFStyles());
                    
                    AddODFZipFile(zos, "META-INF/manifest.xml", GetODFManifest());
                }
            }
            catch(Exception ex)
            {
                log.Error("Exception thrown during export to ODF.");
                log.Error(ex.Message);
                log.Error(ex.Source);
                log.Error(ex.StackTrace);
            }
        }
        
        private byte[] GetODFMeta()
        {
            return GetMinimalODF("office:document-meta");
        }
        private byte[] GetODFStyles()
        {
            return GetMinimalODF("office:document-styles");
        }
        private byte[] GetODFSettings()
        {
            return GetMinimalODF("office:document-settings");
        }
        private byte[] GetMinimalODF(string _element)
        {
            // Return the minimal xml data for required files in a byte array so in can be written to zip.
            // A bit trickier than necessary because .NET StringWriter is UTF-16 and we want UTF-8.
            
            MemoryStream ms = new MemoryStream();
            XmlTextWriter xmlw = new XmlTextWriter(ms, new System.Text.UTF8Encoding());
            xmlw.Formatting = Formatting.Indented; 
                
            xmlw.WriteStartDocument();
            xmlw.WriteStartElement(_element);
            xmlw.WriteAttributeString("xmlns", "office", null, "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
            
            xmlw.WriteStartAttribute("office:version");
            xmlw.WriteString("1.1"); 
            xmlw.WriteEndAttribute();
                 
            xmlw.WriteEndElement();
            xmlw.Flush();
            xmlw.Close();
            
            return ms.ToArray();
        }
        private byte[] GetODFManifest()
        {
            MemoryStream ms = new MemoryStream();
            XmlTextWriter xmlw = new XmlTextWriter(ms, new System.Text.UTF8Encoding());
            xmlw.Formatting = Formatting.Indented; 
                
            xmlw.WriteStartDocument();
            xmlw.WriteStartElement("manifest:manifest");
            xmlw.WriteAttributeString("xmlns", "manifest", null, "urn:oasis:names:tc:opendocument:xmlns:manifest:1.0");
            
            // Manifest itself
            xmlw.WriteStartElement("manifest:file-entry");
            xmlw.WriteStartAttribute("manifest:media-type");
            xmlw.WriteString("application/vnd.oasis.opendocument.spreadsheet");
            xmlw.WriteEndAttribute();
            xmlw.WriteStartAttribute("manifest:full-path");
            xmlw.WriteString("/");
            xmlw.WriteEndAttribute();
            xmlw.WriteEndElement();
            
            // Minimal set of files.
            OutputODFManifestEntry(xmlw, "content.xml");
            OutputODFManifestEntry(xmlw, "styles.xml");
            OutputODFManifestEntry(xmlw, "meta.xml");
            OutputODFManifestEntry(xmlw, "settings.xml");
            
            xmlw.WriteEndElement();
            xmlw.Flush();
            xmlw.Close();
            
            return ms.ToArray();	
        }
        private void OutputODFManifestEntry(XmlTextWriter _xmlw, string _file)
        {
            _xmlw.WriteStartElement("manifest:file-entry");
            _xmlw.WriteStartAttribute("manifest:media-type");
            _xmlw.WriteString("text/xml");
            _xmlw.WriteEndAttribute();
            _xmlw.WriteStartAttribute("manifest:full-path");
            _xmlw.WriteString(_file);
            _xmlw.WriteEndAttribute();
            _xmlw.WriteEndElement();
        }
        private void AddODFZipFile(ZipOutputStream _zos, string _file, byte[] _data)
        {
            ZipEntry entry = new ZipEntry(_file);
            
            entry.DateTime = DateTime.Now;
            entry.Size = _data.Length; 

            _zos.PutNextEntry(entry);
            _zos.Write(_data, 0, _data.Length);
        }
    }
}
