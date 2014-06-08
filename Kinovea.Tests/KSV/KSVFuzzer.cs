using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Xml;
using Kinovea.Services;
using System.Globalization;

namespace Kinovea.Tests
{
    /// <summary>
    /// Generates random ksv.
    /// TODO:
    /// Add a summary as an xML comment to get stats on what was generated.
    /// </summary>
    public class KSVFuzzer
    {
        private const string KSV_VERSION = "1.0";
        private Random random = new Random();

        public void CreateKSV(string outputPath)
        {
            string filename = string.Format("{0:yyyyMMddTHHmmss}.ksv", DateTime.Now);
            string output = Path.Combine(outputPath, filename);

            XmlTextWriter w = new XmlTextWriter(output, Encoding.UTF8);
            w.Formatting = Formatting.Indented;

            w.WriteStartDocument();
            w.WriteStartElement("KinoveaSyntheticVideo");

            WriteGeneralInformation(w);
            
            w.WriteEndElement();
            w.WriteEndDocument();

            w.Flush();
            w.Close();
        }

        private void WriteGeneralInformation(XmlTextWriter w)
        {
            Size imageSize = random.NextSize(50, 2048);
            double framesPerSecond = random.NextDouble(1, 100);
            int durationFrames = random.Next(10, 1000);
            Color backgroundColor = random.NextColor(255);
            bool frameNumber = true;

            w.WriteElementString("FormatVersion", KSV_VERSION);
            w.WriteElementString("ImageSize", string.Format("{0};{1}", imageSize.Width, imageSize.Height));
            w.WriteElementString("FramesPerSecond", string.Format(CultureInfo.InvariantCulture, "{0}", framesPerSecond));
            w.WriteElementString("DurationFrames", string.Format(CultureInfo.InvariantCulture, "{0}", durationFrames));
            w.WriteElementString("BackgroundColor", XmlHelper.WriteColor(backgroundColor, true));
            w.WriteElementString("FrameNumber", frameNumber.ToString().ToLower());
        }
    }
}
