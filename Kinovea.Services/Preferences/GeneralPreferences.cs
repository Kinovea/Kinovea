#region License
/*
Copyright © Joan Charmant 2012.
jcharmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.
*/
#endregion
using System;
using System.Globalization;
using System.Threading;
using System.Xml;
using System.Windows.Forms;
using System.Drawing;

namespace Kinovea.Services
{
    public class GeneralPreferences : IPreferenceSerializer
    {
        #region Properties
        public string Name
        {
            get { return "General"; }
        }

        public bool ExplorerVisible
        {
            get { return explorerVisible; }
            set { explorerVisible = value; }
        }

        public float ExplorerSplitterRatio
        {
            get { return explorerSplitterRatio; }
            set { explorerSplitterRatio = value; }
        }

        public float SidePanelSplitterRatio
        {
            get { return sidePanelSplitterRatio; }
            set { sidePanelSplitterRatio = value; }
        }

        public bool SidePanelVisible
        {
            get { return sidePanelVisible; }
            set { sidePanelVisible = value; }
        }

        public FormWindowState WindowState
        {
            get { return windowState; }
            set { windowState = value; }
        }
        
        public Rectangle WindowRectangle 
        {
            get { return windowRectangle; }
            set { windowRectangle = value; }
        }

        public Workspace Workspace
        {
            get { return workspace; }
            set { workspace = value; }
        }

        public bool EnableDebugLog
        {
            get { return enableDebugLog; }
            set { enableDebugLog = value; }
        }

        public bool AllowMultipleInstances
        {
            get { return allowMultipleInstances; }
            set { allowMultipleInstances = value; }
        }

        public bool InstancesOwnPreferences
        {
            get { return instancesOwnPreferences; }
            set { instancesOwnPreferences = value; }
        }

        public int PreferencePage
        {
            get { return preferencePage; }
            set { preferencePage = value; }
        }

        public string PointerKey
        {
            get { return pointerKey; }
            set { pointerKey = value; }
        }
        #endregion

        #region Members
        private string uiCultureName;
        private bool enableDebugLog = false;
        private bool allowMultipleInstances = true;
        private bool instancesOwnPreferences = true;
        private int preferencePage;
        private string pointerKey = "::default";

        // The following should be moved to the workspace as they are instance specific.
        #region Workspace
        private bool explorerVisible = true;
        private float explorerSplitterRatio = 0.2f;
        private float sidePanelSplitterRatio = 0.8f;
        private bool sidePanelVisible = false;
        private FormWindowState windowState = FormWindowState.Maximized;
        private Rectangle windowRectangle;
        private Workspace workspace = new Workspace();
        #endregion
        #endregion

        public GeneralPreferences()
        {
            uiCultureName = Thread.CurrentThread.CurrentUICulture.Name;
        }

        public void SetCulture(string cultureName)
        {
            uiCultureName = cultureName;
        }

        /// <summary>
        /// Returns a CultureInfo object corresponding to the culture selected by the user.
        /// The object is safe to be assigned CurrentThread.CurrentUICulture for this specific platform (codename may differ from the saved one).
        /// </summary>
        /// <returns></returns>
        public CultureInfo GetSupportedCulture()
        {
            CultureInfo ci = new CultureInfo(uiCultureName);
            if (LanguageManager.IsSupportedCulture(ci))
                return ci;
            else
                return new CultureInfo("en");
        }

        public void WriteXML(XmlWriter writer)
        {
            writer.WriteElementString("Culture", uiCultureName);
            writer.WriteElementString("EnableDebugLog", XmlHelper.WriteBoolean(enableDebugLog));
            writer.WriteElementString("AllowMultipleInstances", XmlHelper.WriteBoolean(allowMultipleInstances));
            writer.WriteElementString("InstancesOwnPreferences", XmlHelper.WriteBoolean(instancesOwnPreferences));
            writer.WriteElementString("PreferencesPage", preferencePage.ToString());
            writer.WriteElementString("Pointer", pointerKey);
            

            if (workspace != null && workspace.Screens != null && workspace.Screens.Count > 0)
            {
                writer.WriteStartElement("Workspace");
                workspace.WriteXML(writer);
                writer.WriteEndElement();
            }

            // TODO: the following should be moved to the workspace.
            writer.WriteElementString("ExplorerVisible", XmlHelper.WriteBoolean(explorerVisible));
            writer.WriteElementString("ExplorerSplitterRatio", XmlHelper.WriteFloat(explorerSplitterRatio));
            writer.WriteElementString("SidePanelSplitterRatio", XmlHelper.WriteFloat(sidePanelSplitterRatio));
            writer.WriteElementString("SidePanelVisible", XmlHelper.WriteBoolean(sidePanelVisible));
            writer.WriteElementString("WindowState", windowState.ToString());
            writer.WriteElementString("WindowRectangle", XmlHelper.WriteRectangleF(windowRectangle));
        }

        public void ReadXML(XmlReader reader)
        {
            reader.ReadStartElement();

            while (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.Name)
                {
                    case "Culture":
                        uiCultureName = reader.ReadElementContentAsString();
                        break;
                    case "EnableDebugLog":
                        enableDebugLog = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    case "AllowMultipleInstances":
                        allowMultipleInstances = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    case "InstancesOwnPreferences":
                        instancesOwnPreferences = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    case "PreferencesPage":
                        preferencePage = reader.ReadElementContentAsInt();
                        break;
                    case "Pointer":
                        pointerKey = reader.ReadElementContentAsString();
                        break;
                    case "Workspace":
                        workspace.ReadXML(reader);
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
                    default:
                        reader.ReadOuterXml();
                        break;
                }
            }

            reader.ReadEndElement();
        }
    }
}
