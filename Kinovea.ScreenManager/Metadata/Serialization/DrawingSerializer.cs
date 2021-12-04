using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using System.Drawing;
using System.IO;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Used in the context of KVA serialization and undo/redo serialization.
    /// For undo/redo serialization, this class also handles chronos and tracks.
    /// </summary>
    public static class DrawingSerializer
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string SerializeMemento(Metadata metadata, AbstractDrawing drawing, SerializationFilter filter, bool saveTrackability)
        {
            IKvaSerializable kvaDrawing = drawing as IKvaSerializable;
            if (kvaDrawing == null)
                return "";

            string result = "";
            
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = false;
            settings.CloseOutput = true;

            StringBuilder builder = new StringBuilder();

            using (XmlWriter w = XmlWriter.Create(builder, settings))
            {
                w.WriteStartElement("DrawingMemento");

                Serialize(w, kvaDrawing, filter);

                if (saveTrackability && drawing is ITrackable)
                    metadata.TrackabilityManager.WriteTracker(w, drawing.Id);

                w.WriteEndElement();

                w.Flush();
                result = builder.ToString();
            }
        
            return result;
        }

        public static AbstractDrawing DeserializeMemento(string data, Metadata metadata)
        {
            if (string.IsNullOrEmpty(data))
                return null;

            AbstractDrawing drawing = null;
            PointF identityScale = new PointF(1, 1);

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            settings.CloseInput = true;

            using (XmlReader r = XmlReader.Create(new StringReader(data), settings))
            {
                r.MoveToContent();

                if (!(r.Name == "DrawingMemento"))
                    return null;

                r.ReadStartElement();

                drawing = Deserialize(r, identityScale, TimeHelper.IdentityTimestampMapper, metadata);

                if (drawing is ITrackable)
                {
                    metadata.TrackabilityManager.ReadTracker(r, identityScale, TimeHelper.IdentityTimestampMapper);
                    metadata.TrackabilityManager.Assign(drawing as ITrackable);
                }
            }

            return drawing;
        }

        public static void DeserializeModifyMemento(Guid managerId, Guid drawingId, string data, Metadata metadata)
        {
            AbstractDrawingManager manager = metadata.GetDrawingManager(managerId);
            IKvaSerializable drawing = manager.GetDrawing(drawingId) as IKvaSerializable;

            if (drawing == null)
                return;

            PointF identityScale = new PointF(1, 1);

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            settings.CloseInput = true;

            using (XmlReader r = XmlReader.Create(new StringReader(data), settings))
            {
                r.MoveToContent();

                if (!(r.Name == "DrawingMemento"))
                    return;

                r.ReadStartElement();

                drawing.ReadXml(r, identityScale, TimeHelper.IdentityTimestampMapper);

                if (drawing is ITrackable)
                {
                    metadata.TrackabilityManager.ReadTracker(r, identityScale, TimeHelper.IdentityTimestampMapper);
                    metadata.TrackabilityManager.Assign(drawing as ITrackable);
                }
            }
        }

        public static void Serialize(XmlWriter w, IKvaSerializable drawing, SerializationFilter filter)
        {
            if (drawing.Id == Guid.Empty)
                return;

            // The XML name for this drawing should be stored in its [XMLType] C# attribute.
            Type t = drawing.GetType();
            object[] attributes = t.GetCustomAttributes(typeof(XmlTypeAttribute), false);

            if (attributes.Length == 0)
                return;

            string xmlName = ((XmlTypeAttribute)attributes[0]).TypeName;

            w.WriteStartElement(xmlName);
            w.WriteAttributeString("id", drawing.Id.ToString());
            w.WriteAttributeString("name", drawing.Name);
            drawing.WriteXml(w, filter);
            w.WriteEndElement();
        }

        public static AbstractDrawing Deserialize(XmlReader r, PointF scaling, TimestampMapper timestampMapper, Metadata metadata)
        {
            AbstractDrawing drawing = null;

            if (r.IsEmptyElement)
            {
                r.ReadStartElement();
                return null;
            }

            // Find the right class to instanciate.
            // The class must derive from AbstractDrawing and have the corresponding [XmlType] C# attribute.
            bool drawingRead = false;
            Assembly a = Assembly.GetExecutingAssembly();
            foreach (Type t in a.GetTypes())
            {
                if (t.BaseType != typeof(AbstractDrawing))
                    continue;

                object[] attributes = t.GetCustomAttributes(typeof(XmlTypeAttribute), false);
                if (attributes.Length <= 0 || ((XmlTypeAttribute)attributes[0]).TypeName != r.Name)
                    continue;

                ConstructorInfo ci = t.GetConstructor(new[] { typeof(XmlReader), typeof(PointF), typeof(TimestampMapper), typeof(Metadata)});
                if (ci == null)
                    break;
 
                object[] parameters = new object[] { r, scaling, timestampMapper, metadata };
                drawing = (AbstractDrawing)Activator.CreateInstance(t, parameters);
                drawingRead = drawing != null;

                break;
            }

            if (!drawingRead)
            {
                string unparsed = r.ReadOuterXml();
                log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
            }

            return drawing;
        }
        
    }
}
