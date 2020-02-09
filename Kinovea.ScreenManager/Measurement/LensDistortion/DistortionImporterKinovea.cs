using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Xml;
using System.Globalization;
using System.IO;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public static class DistortionImporterKinovea
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static DistortionParameters Import(string path, Size imageSize)
        {
            if(!File.Exists(path))
                return null;

            DistortionParameters parameters = new DistortionParameters(imageSize);
            
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreComments = true;
                settings.IgnoreProcessingInstructions = true;
                settings.IgnoreWhitespace = true;
                settings.CloseInput = true;

                using (XmlReader r = XmlReader.Create(path, settings))
                {
                    r.MoveToContent();
                    parameters = DistortionSerializer.Deserialize(r, imageSize);
                }
            }
            catch
            {
                log.ErrorFormat("Import of lens distortion parameters failed.");
            }

            return parameters;
        }
        
        public static void Export(string path, DistortionParameters parameters, Size imageSize)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.CloseOutput = true;

            using (XmlWriter w = XmlWriter.Create(path, settings))
            {
                DistortionSerializer.Serialize(w, parameters, true, imageSize);
            }
        }
    }
}
