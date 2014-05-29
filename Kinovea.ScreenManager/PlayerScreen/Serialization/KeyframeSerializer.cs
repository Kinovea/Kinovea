using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Drawing;
using System.IO;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public static class KeyframeSerializer
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string SerializeMemento(Keyframe keyframe)
        {
            string result = "";
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = false;
            settings.CloseOutput = true;

            StringBuilder builder = new StringBuilder();

            using (XmlWriter w = XmlWriter.Create(builder, settings))
            {
                Serialize(w, keyframe);
                w.Flush();
                result = builder.ToString();
            }

            return result;
        }

        public static Keyframe DeserializeMemento(string data, Metadata metadata)
        {
            // TODO:
            // Also handle thumbnail and trackable drawings.

            Keyframe keyframe = null;
            
            PointF identityScaling = new PointF(1, 1);
            
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            settings.CloseInput = true;

            using (XmlReader r = XmlReader.Create(new StringReader(data), settings))
            {
                r.MoveToContent();
                keyframe = Deserialize(r, identityScaling, TimeHelper.IdentityTimestampMapper, metadata);
            }

            return keyframe;
        }

        public static void Serialize(XmlWriter w, Keyframe keyframe)
        {
            w.WriteStartElement("Keyframe");
            w.WriteAttributeString("id", keyframe.Id.ToString());
            keyframe.WriteXml(w);
            w.WriteEndElement();
        }
    
        public static Keyframe Deserialize(XmlReader r, PointF scaling, TimestampMapper timestampMapper, Metadata metadata)
        {
            return new Keyframe(r, scaling, timestampMapper, metadata);
        }
    }
}
