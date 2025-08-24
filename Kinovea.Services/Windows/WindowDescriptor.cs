using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Kinovea.Services
{
    /// <summary>
    /// State and preferences specific to a window (named or unnamed instance).
    /// </summary>
    public class WindowDescriptor
    {
        #region Properties
        
        /// <summary>
        /// Unique id of the window.
        /// </summary>
        public Guid Id
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// User-defined name of the window.
        /// Two windows should not have the same name.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Last save date of the window.
        /// </summary>
        public DateTime LastSave
        {
            get { return lastSave; }
            set { lastSave = value; }
        }

        /// <summary>
        /// Startup mode of the window.
        /// </summary>
        public WindowStartupMode StartupMode
        {
            get { return startupMode; }
            set { startupMode = value; }
        }

        /// <summary>
        /// List of screens in the window and their content.
        /// </summary>
        public List<IScreenDescription> ScreenList 
        { 
            get; 
        }

        /// <summary>
        /// Whether the explorer pane is visible or not.
        /// </summary>
        public bool ExplorerVisible
        {
            get { return explorerVisible; }
            set { explorerVisible = value; }
        }

        /// <summary>
        /// Vertical splitter between the explorer and the screen manager.
        /// </summary>
        public float ExplorerSplitterRatio
        {
            get { return explorerSplitterRatio; }
            set { explorerSplitterRatio = value; }
        }

        /// <summary>
        /// Vertical splitter inside the player screen between the viewport and the side panel.
        /// </summary>
        public float SidePanelSplitterRatio
        {
            get { return sidePanelSplitterRatio; }
            set { sidePanelSplitterRatio = value; }
        }

        /// <summary>
        /// Whether the side panel is visible.
        /// </summary>
        public bool SidePanelVisible
        {
            get { return sidePanelVisible; }
            set { sidePanelVisible = value; }
        }

        /// <summary>
        /// State of the main window.
        /// </summary>
        public FormWindowState WindowState
        {
            get { return windowState; }
            set { windowState = value; }
        }

        /// <summary>
        /// Position and size of the main window.
        /// </summary>
        public Rectangle WindowRectangle
        {
            get { return windowRectangle; }
            set { windowRectangle = value; }
        }
        /// <summary>
        /// Horizontal splitter between folders and files in the file explorer tab.
        /// </summary>
        public float ExplorerFilesSplitterRatio
        {
            get { return explorerFilesSplitterRatio; }
            set { explorerFilesSplitterRatio = value; }
        }

        /// <summary>
        /// Horizontal splitter between folders and files in the shortcuts explorer tab.
        /// </summary>
        public float ShortcutsFilesSplitterRatio
        {
            get { return shortcutsFilesSplitterRatio; }
            set { shortcutsFilesSplitterRatio = value; }
        }

        /// <summary>
        /// Active tab in the file explorer panel.
        /// </summary>
        public ActiveFileBrowserTab ActiveTab
        {
            get { return activeTab; }
            set { activeTab = value; }
        }
        #endregion

        #region Members
        private Guid id = Guid.NewGuid();
        private DateTime lastSave;

        // Preferences
        private string name;
        private WindowStartupMode startupMode = WindowStartupMode.Continue;
        private List<IScreenDescription> screenList = new List<IScreenDescription>();

        // State
        private bool explorerVisible = true;
        private float explorerSplitterRatio = 0.2f;
        private float sidePanelSplitterRatio = 0.8f;
        private bool sidePanelVisible = false;
        private FormWindowState windowState = FormWindowState.Maximized;
        private Rectangle windowRectangle;
        private float explorerFilesSplitterRatio = 0.25f;
        private float shortcutsFilesSplitterRatio = 0.25f;
        private ActiveFileBrowserTab activeTab = ActiveFileBrowserTab.Explorer;

        #endregion

        #region Serialization
        public void WriteXML(XmlWriter writer)
        {
            writer.WriteElementString("Id", id.ToString());

            if (!string.IsNullOrEmpty(name))
                writer.WriteElementString("Name", name.ToString());

            writer.WriteElementString("LastSave", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            writer.WriteElementString("StartupMode", startupMode.ToString());

            if (screenList.Count > 0)
            {
                writer.WriteStartElement("ScreenList");
                foreach (var screen in screenList)
                {
                    if (screen.ScreenType == ScreenType.Playback)
                        writer.WriteStartElement("ScreenDescriptionPlayback");
                    else
                        writer.WriteStartElement("ScreenDescriptionCapture");

                    screen.WriteXml(writer);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }

            writer.WriteElementString("ExplorerVisible", XmlHelper.WriteBoolean(explorerVisible));
            writer.WriteElementString("ExplorerSplitterRatio", XmlHelper.WriteFloat(explorerSplitterRatio));
            writer.WriteElementString("SidePanelSplitterRatio", XmlHelper.WriteFloat(sidePanelSplitterRatio));
            writer.WriteElementString("SidePanelVisible", XmlHelper.WriteBoolean(sidePanelVisible));
            writer.WriteElementString("WindowState", windowState.ToString());
            writer.WriteElementString("WindowRectangle", XmlHelper.WriteRectangleF(windowRectangle));
            writer.WriteElementString("ExplorerFilesSplitterRatio", XmlHelper.WriteFloat(explorerFilesSplitterRatio));
            writer.WriteElementString("ShortcutsFilesSplitterRatio", XmlHelper.WriteFloat(shortcutsFilesSplitterRatio));
            writer.WriteElementString("ActiveTab", activeTab.ToString());
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
                    case "Name":
                        name = reader.ReadElementContentAsString();
                        break;
                    case "LastSave":
                        lastSave = XmlHelper.ParseDateTime(reader.ReadElementContentAsString());
                        break;
                    case "StartupMode":
                        startupMode = XmlHelper.ParseEnum<WindowStartupMode>(reader.ReadElementContentAsString(), WindowStartupMode.Continue);
                        break;
                    case "ScreenList":
                        ParseScreenList(reader);
                        break;
                    case "ExplorerVisible":
                        explorerVisible = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    case "ExplorerSplitterRatio":
                        explorerSplitterRatio = XmlHelper.ParseFloat(reader.ReadElementContentAsString());
                        break;
                    case "SidePanelSplitterRatio":
                        sidePanelSplitterRatio = XmlHelper.ParseFloat(reader.ReadElementContentAsString());
                        break;
                    case "SidePanelVisible":
                        sidePanelVisible = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    case "WindowState":
                        windowState = (FormWindowState)Enum.Parse(typeof(FormWindowState), reader.ReadElementContentAsString());
                        break;
                    case "WindowRectangle":
                        windowRectangle = XmlHelper.ParseRectangle(reader.ReadElementContentAsString());
                        break;
                    case "ExplorerFilesSplitterRatio":
                        explorerFilesSplitterRatio = XmlHelper.ParseFloat(reader.ReadElementContentAsString());
                        if (explorerFilesSplitterRatio <= 0)
                            explorerFilesSplitterRatio = 0.25f;
                        break;
                    case "ShortcutsFilesSplitterRatio":
                        shortcutsFilesSplitterRatio = XmlHelper.ParseFloat(reader.ReadElementContentAsString());
                        if (explorerFilesSplitterRatio <= 0)
                            explorerFilesSplitterRatio = 0.25f;
                        break;
                    case "ActiveTab":
                        activeTab = XmlHelper.ParseEnum<ActiveFileBrowserTab>(reader.ReadElementContentAsString(), ActiveFileBrowserTab.Explorer);
                        break;
                    default:
                        reader.ReadOuterXml();
                        break;
                }
            }
        }

        private void ParseScreenList(XmlReader reader)
        {
            reader.MoveToContent();
            bool isEmpty = reader.IsEmptyElement;

            if (reader.Name != "ScreenList" && reader.Name != "ScreenList" || isEmpty)
            {
                reader.ReadOuterXml();
                return;
            }

            reader.ReadStartElement();
            //reader.ReadElementContentAsString("FormatVersion", "");

            screenList.Clear();

            while (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.Name)
                {
                    case "ScreenDescriptionPlayback":
                        ScreenDescriptionPlayback sdp = new ScreenDescriptionPlayback();
                        sdp.ReadXml(reader);
                        screenList.Add(sdp);
                        break;
                    case "ScreenDescriptionCapture":
                        ScreenDescriptionCapture sdc = new ScreenDescriptionCapture();
                        sdc.Readxml(reader);
                        screenList.Add(sdc);
                        break;
                    default:
                        reader.ReadOuterXml();
                        break;
                }
            }

            // Extra flag for dual replay.
            if (screenList.Count == 2 &&
                screenList[0] is ScreenDescriptionPlayback && ((ScreenDescriptionPlayback)screenList[0]).IsReplayWatcher &&
                screenList[1] is ScreenDescriptionPlayback && ((ScreenDescriptionPlayback)screenList[1]).IsReplayWatcher)
            {
                ((ScreenDescriptionPlayback)screenList[0]).IsDualReplay = true;
                ((ScreenDescriptionPlayback)screenList[1]).IsDualReplay = true;
            }

            reader.ReadEndElement();
        }
        #endregion
    }
}
