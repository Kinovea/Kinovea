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

        public static string SerializeMemento(Metadata metadata, Keyframe keyframe, SerializationFilter filter)
        {
            string result = "";
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = false;
            settings.CloseOutput = true;

            StringBuilder builder = new StringBuilder();

            using (XmlWriter w = XmlWriter.Create(builder, settings))
            {
                w.WriteStartElement("KeyframeMemento");

                Serialize(w, keyframe, filter);
                SerializeTrackers(w, metadata, keyframe);

                w.WriteEndElement();

                w.Flush();
                result = builder.ToString();
            }

            return result;
        }

        public static Keyframe DeserializeMemento(string data, Metadata metadata)
        {
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

                if (!(r.Name == "KeyframeMemento"))
                    return null;

                r.ReadStartElement();

                keyframe = Deserialize(r, identityScaling, TimeHelper.IdentityTimestampMapper, metadata);
                DeserializeTrackers(r, metadata, keyframe);
            }

            return keyframe;
        }

        public static void DeserializeModifyMemento(Guid keyframeId, string data, Metadata metadata)
        {
            Keyframe keyframe = metadata.GetKeyframe(keyframeId);
            if (keyframe == null)
                return;

            PointF identityScaling = new PointF(1, 1);

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            settings.CloseInput = true;

            using (XmlReader r = XmlReader.Create(new StringReader(data), settings))
            {
                r.MoveToContent();

                if (!(r.Name == "KeyframeMemento"))
                    return;

                r.ReadStartElement();

                keyframe.ReadXml(r, identityScaling, TimeHelper.IdentityTimestampMapper);
            }

            return;
        }

        public static void Serialize(XmlWriter w, Keyframe keyframe, SerializationFilter filter)
        {
            w.WriteStartElement("Keyframe");
            w.WriteAttributeString("id", keyframe.Id.ToString());
            keyframe.WriteXml(w, filter);
            w.WriteEndElement();
        }
    
        public static Keyframe Deserialize(XmlReader r, PointF scaling, TimestampMapper timestampMapper, Metadata metadata)
        {
            return new Keyframe(r, scaling, timestampMapper, metadata);
        }
    
        private static void SerializeTrackers(XmlWriter w, Metadata metadata, Keyframe keyframe)
        {
            w.WriteStartElement("Trackers");

            foreach (AbstractDrawing drawing in keyframe.Drawings)
            {
                ITrackable trackable = drawing as ITrackable;
                if (trackable == null)
                    continue;

                metadata.TrackabilityManager.WriteTracker(w, drawing.Id);
            }

            w.WriteEndElement();
        }

        private static void DeserializeTrackers(XmlReader r, Metadata metadata, Keyframe keyframe)
        {
            if (!(r.Name == "Trackers") || r.IsEmptyElement)
                return;

            r.ReadStartElement();

            PointF identityScale = new PointF(1, 1);

            while (r.NodeType == XmlNodeType.Element)
            {
                metadata.TrackabilityManager.ReadTracker(r, identityScale, TimeHelper.IdentityTimestampMapper);
            }

            r.ReadEndElement();

            foreach (AbstractDrawing drawing in keyframe.Drawings)
            {
                ITrackable trackable = drawing as ITrackable;
                if (trackable == null)
                    continue;

                metadata.TrackabilityManager.Assign(trackable, metadata.Tracks().ToList());
            }
        }
    }
}
