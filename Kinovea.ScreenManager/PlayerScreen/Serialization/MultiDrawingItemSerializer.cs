using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Drawing;
using System.Reflection;
using System.Xml.Serialization;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class MultiDrawingItemSerializer
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string SerializeToString(AbstractMultiDrawing manager, AbstractMultiDrawingItem item)
        {
            // TODO: export other data needed for undo, like the associated trackable drawing.

            IKvaSerializable kvaDrawing = item as IKvaSerializable;
            if (kvaDrawing == null)
                return "";

            string result = "";

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = false;
            settings.CloseOutput = true;

            StringBuilder builder = new StringBuilder();

            using (XmlWriter w = XmlWriter.Create(builder, settings))
            {
                DrawingSerializer.Serialize(w, kvaDrawing);
                w.Flush();
                result = builder.ToString();
            }

            return result;
        }

        public static AbstractMultiDrawingItem DeserializeFromString(string data, Metadata metadata)
        {
            AbstractMultiDrawingItem item = null;

            PointF identityScaling = new PointF(1, 1);

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            settings.CloseInput = true;

            using (XmlReader r = XmlReader.Create(new StringReader(data), settings))
            {
                r.MoveToContent();
                item = Deserialize(r, identityScaling, TimeHelper.IdentityTimestampMapper, metadata);
            }

            return item;
        }

        public static AbstractMultiDrawingItem Deserialize(XmlReader r, PointF scaling, TimestampMapper timestampMapper, Metadata metadata)
        {
            AbstractMultiDrawingItem item = null;

            // Find the right class to instanciate.
            // The class must derive from AbstractMultiDrawingItem and have the corresponding [XmlType] C# attribute.
            bool itemRead = false;
            Assembly a = Assembly.GetExecutingAssembly();
            foreach (Type t in a.GetTypes())
            {
                if (t.BaseType != typeof(AbstractMultiDrawingItem))
                    continue;

                object[] attributes = t.GetCustomAttributes(typeof(XmlTypeAttribute), false);
                if (attributes.Length <= 0 || ((XmlTypeAttribute)attributes[0]).TypeName != r.Name)
                    continue;

                ConstructorInfo ci = t.GetConstructor(new[] { typeof(XmlReader), typeof(PointF), typeof(TimestampMapper), typeof(Metadata) });
                if (ci == null)
                    break;

                object[] parameters = new object[] { r, scaling, timestampMapper, metadata};
                item = (AbstractMultiDrawingItem)Activator.CreateInstance(t, parameters);
                itemRead = item != null;

                break;
            }

            if (!itemRead)
            {
                string unparsed = r.ReadOuterXml();
                log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
            }

            return item;
        }
    }
}
