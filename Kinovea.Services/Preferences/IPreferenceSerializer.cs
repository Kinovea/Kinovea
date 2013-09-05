using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Kinovea.Services
{
    public interface IPreferenceSerializer
    {
        string Name { get; }
        void WriteXML(XmlWriter writer);
        void ReadXML(XmlReader reader);
    }
}
