using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using System.Drawing;
using System.IO;

namespace Kinovea.ScreenManager
{
    public static class DrawingSerializer
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string SerializeToString(AbstractDrawing drawing)
        {
            // TODO: check for non serializable drawings. (bitmap/svg).

            string result = "";

            IKvaSerializable kvaDrawing = drawing as IKvaSerializable;
            if (kvaDrawing != null)
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = false;
                settings.CloseOutput = true;

                StringBuilder builder = new StringBuilder();

                using (XmlWriter w = XmlWriter.Create(builder, settings))
                {
                    Serialize(w, kvaDrawing);
                    w.Flush();
                    result = builder.ToString();
                }
            }

            return result;
        }

        public static AbstractDrawing DeserializeFromString(string data, Metadata metadata)
        {
            AbstractDrawing drawing = null;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            settings.CloseInput = true;

            using (XmlReader r = XmlReader.Create(new StringReader(data), settings))
            {
                r.MoveToContent();
                drawing = Deserialize(r, new PointF(1, 1), metadata);
            }

            return drawing;
        }

        public static void Serialize(XmlWriter w, IKvaSerializable drawing)
        {
            // The XML name for this drawing should be stored in its [XMLType] C# attribute.
            Type t = drawing.GetType();
            object[] attributes = t.GetCustomAttributes(typeof(XmlTypeAttribute), false);

            if (attributes.Length == 0)
                return;

            string xmlName = ((XmlTypeAttribute)attributes[0]).TypeName;

            w.WriteStartElement(xmlName);
            w.WriteAttributeString("id", drawing.Id.ToString());
            drawing.WriteXml(w);
            w.WriteEndElement();
        }

        public static AbstractDrawing Deserialize(XmlReader r, PointF scaling, Metadata metadata)
        {
            AbstractDrawing drawing = null;

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

                ConstructorInfo ci = t.GetConstructor(new[] { typeof(XmlReader), typeof(PointF), typeof(Metadata) });
                if (ci == null)
                    break;

                object[] parameters = new object[] { r, scaling, metadata };
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
