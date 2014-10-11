using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Kinovea.Services;
using System.Globalization;
using System.Xml.Xsl;
using System.Windows.Forms;
using System.IO;

namespace Kinovea.ScreenManager
{
    public class MetadataConverter
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string Convert(string kva, bool isFile)
        {
            // the kva parameter can either be a filepath or the xml string. 
            // We return the same kind of string as passed in.
            string result = kva;

            XmlDocument kvaDoc = new XmlDocument();
            try
            {
                if (isFile)
                    kvaDoc.Load(kva);
                else
                    kvaDoc.LoadXml(kva);
            }
            catch (Exception e)
            {
                log.ErrorFormat("The file couldn't be loaded. No conversion or reading will be attempted.");
                log.Error(e.Message);
                return result;
            }

            string tempFile = Path.Combine(Software.SettingsDirectory, "temp.kva");
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            XmlNode formatNode = kvaDoc.DocumentElement.SelectSingleNode("descendant::FormatVersion");
            double format;
            bool read = double.TryParse(formatNode.InnerText, NumberStyles.Any, CultureInfo.InvariantCulture, out format);
            if (!read)
            {
                log.ErrorFormat("The format couldn't be read. No conversion will be attempted. Read:{0}", formatNode.InnerText);
                return result;
            }

            if (format < 2.0 && format >= 1.3)
            {
                log.DebugFormat("Older format detected ({0}). Starting conversion", format);

                try
                {
                    XslCompiledTransform xslt = new XslCompiledTransform();
                    string stylesheet = Application.StartupPath + "\\xslt\\kva-1.5to2.0.xsl";
                    xslt.Load(stylesheet);

                    if (isFile)
                    {
                        using (XmlWriter xw = XmlWriter.Create(tempFile, settings))
                        {
                            xslt.Transform(kvaDoc, xw);
                        }

                        result = tempFile;
                    }
                    else
                    {
                        StringBuilder builder = new StringBuilder();
                        using (XmlWriter xw = XmlWriter.Create(builder, settings))
                        {
                            xslt.Transform(kvaDoc, xw);
                        }

                        result = builder.ToString();
                    }

                    log.DebugFormat("Older format converted.");
                }
                catch (Exception)
                {
                    log.ErrorFormat("An error occurred during KVA conversion. Conversion aborted.", format.ToString());
                }
            }
            else if (format <= 1.2)
            {
                log.ErrorFormat("Format too old ({0}). No conversion will be attempted.", format.ToString());
            }

            return result;
        }
    }
}
