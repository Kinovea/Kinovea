using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Kinovea.Services
{
    /// <summary>
    /// Post-recording command.
    /// Contains a list of "instructions", an instruction is a call to an external program.
    /// </summary>
    public class UserCommand
    {
        #region Properties
        /// <summary>
        /// Unique id of the command.
        /// </summary>
        public Guid Id
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// List of instructions in the command.
        /// An instruction is a call to an external program with some arguments.
        /// </summary>
        public List<string> Instructions
        {
            get { return instructions; }
            set { instructions = value; }
        }

        #endregion

        #region Members
        private Guid id = Guid.NewGuid();
        private List<string> instructions = new List<string>();
        #endregion

        #region Serialization
        public void WriteXML(XmlWriter writer)
        {
            writer.WriteElementString("Id", id.ToString());

            if (instructions.Count > 0)
            {
                writer.WriteStartElement("Instructions");
                foreach (var instruction in instructions)
                {
                    writer.WriteElementString("Instruction", instruction);
                }
                writer.WriteEndElement();
            }
        }

        public void ReadXML(XmlReader reader)
        {
            reader.ReadStartElement();

            while (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.Name)
                {
                    case "Id":
                        id = XmlHelper.ParseGuid(reader.ReadElementContentAsString());
                        break;
                    case "Instructions":
                        ParseInstructions(reader);
                        break;
                    default:
                        reader.ReadOuterXml();
                        break;
                }
            }
        }

        private void ParseInstructions(XmlReader reader)
        {
            instructions.Clear();
            bool empty = reader.IsEmptyElement;

            reader.ReadStartElement();
            if (empty)
                return;

            while (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.Name == "Instruction")
                {
                    instructions.Add(reader.ReadElementContentAsString());
                }
                else
                {
                    reader.ReadOuterXml();
                }
            }

            reader.ReadEndElement();
        }
        #endregion
    }
}
