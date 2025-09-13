using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Kinovea.Services
{
    /// <summary>
    /// A workspace contains references to existing windows.
    /// The only purpose of a workspace is to start a set of windows with a single click.
    /// The link between a workspace and its windows is a weak one, the user can still 
    /// delete the windows, modify them, use the same windows in multiple workspaces, etc.
    /// We'll probably need some higher level logic to manage the association and make it feel
    /// like the workspaces owns the windows.
    /// </summary>
    public class WorkspaceDescriptor
    {
        #region Properties

        /// <summary>
        /// Unique id of the workspace.
        /// </summary>
        public Guid Id
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// User-defined name of the workspace.
        /// Two workspaces should not have the same name.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// List of windows referenced by the workspace.
        /// </summary>
        public List<Guid> WindowList
        {
            get { return windowList; }
        }
        #endregion

        #region Members
        private Guid id = Guid.NewGuid();
        private string name;
        private List<Guid> windowList = new List<Guid>();
        #endregion

        /// <summary>
        /// Replace the window list in the workspace.
        /// </summary>
        public void ReplaceWindows(List<Guid> ids)
        {
            windowList.Clear();
            windowList.AddRange(ids);
        }

        #region Serialization
        public void WriteXML(XmlWriter writer)
        {
            writer.WriteElementString("Id", id.ToString());

            if (!string.IsNullOrEmpty(name))
                writer.WriteElementString("Name", name.ToString());

            if (windowList.Count > 0)
            {
                foreach (var id in windowList)
                {
                    writer.WriteElementString("Window", id.ToString());
                }
            }
        }

        public void ReadXML(XmlReader reader)
        {
            reader.ReadStartElement();
            windowList.Clear();

            while (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.Name)
                {
                    case "Id":
                        id = XmlHelper.ParseGuid(reader.ReadElementContentAsString());
                        break;
                    case "Name":
                        name = reader.ReadElementContentAsString();
                        break;
                    case "Window":
                        Guid windowId = XmlHelper.ParseGuid(reader.ReadElementContentAsString());
                        windowList.Add(windowId);
                        break;
                    default:
                        reader.ReadOuterXml();
                        break;
                }
            }
        }
        #endregion
    }
}
