using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Kinovea.Services
{
    public class FilePropertyVisibility
    {
        public Dictionary<FileProperty, bool> Visible
        {
            get { return visible; }
        }

        private Dictionary<FileProperty, bool> visible= new Dictionary<FileProperty, bool>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public FilePropertyVisibility()
        {
            Visible.Add(FileProperty.Size, true);
            Visible.Add(FileProperty.Framerate, true);
            Visible.Add(FileProperty.Duration, true);
            Visible.Add(FileProperty.CreationTime, false);
            Visible.Add(FileProperty.HasKva, true);
        }

        public void WriteXML(XmlWriter writer)
        {
            foreach (KeyValuePair<FileProperty, bool> pair in Visible)
                writer.WriteElementString(pair.Key.ToString(), pair.Value.ToString().ToLower());
        }

        public static FilePropertyVisibility FromXML(XmlReader reader)
        {
            FilePropertyVisibility fpv = new FilePropertyVisibility();
            
            try
            {
                reader.ReadStartElement();
                
                while (reader.NodeType == XmlNodeType.Element)
                {
                    FileProperty prop = (FileProperty)Enum.Parse(typeof(FileProperty), reader.Name);
                    if (!fpv.visible.ContainsKey(prop))
                    {
                        reader.ReadOuterXml();
                        continue;
                    }

                    bool value = reader.ReadElementContentAsBoolean();
                    fpv.Visible[prop] = value;
                }

                reader.ReadEndElement();
            }
            catch
            {
                log.ErrorFormat("Error while parsing file property visiblity.");
            }

            
            return fpv;
        }
    }
}
