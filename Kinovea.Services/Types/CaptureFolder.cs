using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Kinovea.Services
{
    public class CaptureFolder
    {
        /// <summary>
        /// Unique id for this capture folder.
        /// The screens reference the folder by id so even if the 
        /// path change they are still pointing to the right folder.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// A short custom name to identify the folder in menus.
        /// </summary>
        public string ShortName { get; set; }
        
        /// <summary>
        /// Full path to the folder, may contain context variables.
        /// </summary>
        public string Path { get; set; }

        public CaptureFolder()
        {
            Id = Guid.NewGuid();

            string root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Path = System.IO.Path.Combine(root, "Capture");
        }

        public CaptureFolder Clone()
        {
            CaptureFolder clone = new CaptureFolder
            {
                Id = this.Id,
                ShortName = this.ShortName,
                Path = this.Path
            };

            return clone;
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(ShortName) ? Path : ShortName;
        }


        #region Serialization
        public void WriteXML(XmlWriter w)
        {
            w.WriteElementString("Id", Id.ToString());
            w.WriteElementString("ShortName", ShortName);
            w.WriteElementString("Path", Path);
        }

        public CaptureFolder(XmlReader r)
           : this()
        {
            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "Id":
                        Id = XmlHelper.ParseGuid(r.ReadElementContentAsString());
                        break;
                    case "ShortName":
                        ShortName = r.ReadElementContentAsString();
                        break;
                    case "Path":
                        Path = r.ReadElementContentAsString();
                        break;
                    default:
                        r.ReadOuterXml();
                        break;
                }
            }

            r.ReadEndElement();
        }
        #endregion
    }
}
