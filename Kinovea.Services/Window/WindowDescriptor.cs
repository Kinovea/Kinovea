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
        public List<IScreenDescriptor> ScreenList 
        {
            get { return screenList; }
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

        /// <summary>
        /// Capture delay used the last time we closed a capture screen in this window.
        /// Used to provide good defaults if we ever reopen a capture screen in this window.
        /// </summary>
        public float LastCaptureDelay
        {
            get { return lastCaptureDelay; }
            set { lastCaptureDelay = value; }
        }

        /// <summary>
        /// Capture max duration used the last time we closed a capture screen in this window.
        /// </summary>
        public float LastCaptureMaxDuration
        {
            get { return lastCaptureMaxDuration; }
            set { lastCaptureMaxDuration = value; }
        }

        /// <summary>
        /// State of delayed display the last time we closed a capture screen in this window.
        /// </summary>
        public bool LastCaptureDelayedDisplay
        {
            get { return lastCaptureDelayedDisplay; }
            set { lastCaptureDelayedDisplay = value; }
        }

        /// <summary>
        /// Capture folder used the last time we closed a capture screen in this window.
        /// </summary>
        public Guid LastCaptureFolder
        {
            get { return lastCaptureFolder; }
            set { lastCaptureFolder = value; }
        }

        public string LastCaptureFileName
        {
            get { return lastCaptureFileName; }
            set { lastCaptureFileName = value; }
        }

        //public UserCommand UserCommandBackup
        //{
        //    get { return userCommandBackup; }
        //    set { userCommandBackup = value; }
        //}
        #endregion

        #region Members
        private Guid id = Guid.NewGuid();
        private DateTime lastSave;

        // Preferences
        private string name;
        private WindowStartupMode startupMode = WindowStartupMode.Continue;
        private List<IScreenDescriptor> screenList = new List<IScreenDescriptor>();

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

        // Capture screen state.
        // TODO: move this to a "closed screen" variable with the full screen state.
        private float lastCaptureDelay = 0f;
        private float lastCaptureMaxDuration = 0f;
        private bool lastCaptureDelayedDisplay = true;
        private Guid lastCaptureFolder = Guid.Empty;
        private string lastCaptureFileName = string.Empty;
        //private UserCommand userCommandBackup = null;
        #endregion

        /// <summary>
        /// Replace the screen list in the window with copies of the passed descriptors.
        /// </summary>
        public void ReplaceScreens(List<IScreenDescriptor> descriptors)
        {
            screenList.Clear();
            foreach (var d in descriptors)
            {
                screenList.Add(d.Clone());
            }
        }

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
                        writer.WriteStartElement("ScreenDescriptorPlayback");
                    else
                        writer.WriteStartElement("ScreenDescriptorCapture");

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
            writer.WriteElementString("LastCaptureDelay", XmlHelper.WriteFloat(lastCaptureDelay));
            writer.WriteElementString("LastCaptureMaxDuration", XmlHelper.WriteFloat(lastCaptureMaxDuration));
            writer.WriteElementString("LastCaptureDelayedDisplay", XmlHelper.WriteBoolean(lastCaptureDelayedDisplay));
            writer.WriteElementString("LastCaptureFolder", lastCaptureFolder.ToString());
            writer.WriteElementString("LastCaptureFileName", lastCaptureFileName.ToString());

            //if (userCommandBackup != null)
            //    userCommandBackup.WriteXML(writer);
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
                    case "LastCaptureDelay":
                        lastCaptureDelay = XmlHelper.ParseFloat(reader.ReadElementContentAsString());
                        break;
                    case "LastCaptureMaxDuration":
                        lastCaptureMaxDuration = XmlHelper.ParseFloat(reader.ReadElementContentAsString());
                        break;
                    case "LastCaptureDelayedDisplay":
                        lastCaptureDelayedDisplay = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    case "LastCaptureFolder":
                        lastCaptureFolder = XmlHelper.ParseGuid(reader.ReadElementContentAsString());
                        break;
                    case "LastCaptureFileName":
                        lastCaptureFileName = reader.ReadElementContentAsString();
                        break;
                    //case "UserCommandBackup":
                    //    userCommandBackup = new UserCommand(reader);
                    //    break;
                    default:
                        reader.ReadOuterXml();
                        break;
                }
            }
        }

        private void ParseScreenList(XmlReader reader)
        {
            screenList.Clear();
            bool empty = reader.IsEmptyElement;

            reader.ReadStartElement();

            if (empty)
                return;
            
            while (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.Name)
                {
                    case "ScreenDescriptorPlayback":
                        ScreenDescriptorPlayback sdp = new ScreenDescriptorPlayback();
                        sdp.ReadXml(reader);
                        screenList.Add(sdp);
                        break;
                    case "ScreenDescriptorCapture":
                        ScreenDescriptorCapture sdc = new ScreenDescriptorCapture();
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
                screenList[0] is ScreenDescriptorPlayback && ((ScreenDescriptorPlayback)screenList[0]).IsReplayWatcher &&
                screenList[1] is ScreenDescriptorPlayback && ((ScreenDescriptorPlayback)screenList[1]).IsReplayWatcher)
            {
                ((ScreenDescriptorPlayback)screenList[0]).IsDualReplay = true;
                ((ScreenDescriptorPlayback)screenList[1]).IsDualReplay = true;
            }

            reader.ReadEndElement();
        }
        #endregion
    }
}
