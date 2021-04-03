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
        public string Name
        {
            get { return "General"; }
        }

        public bool ExplorerVisible
        {
            get { return explorerVisible; }
            set { explorerVisible = value; }
        }

        public int ExplorerSplitterDistance
        {
            get { return explorerSplitterDistance; }
            set { explorerSplitterDistance = value; }
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

        private string uiCultureName;
        private bool explorerVisible = true;
        private int explorerSplitterDistance = 250;
        private FormWindowState windowState = FormWindowState.Maximized;
        private Rectangle windowRectangle;
        private Workspace workspace = new Workspace();
        private bool allowMultipleInstances = true;
        private bool instancesOwnPreferences = true;
        private int preferencePage;

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
            writer.WriteElementString("ExplorerVisible", explorerVisible ? "true" : "false");
            writer.WriteElementString("ExplorerSplitterDistance", explorerSplitterDistance.ToString());
            writer.WriteElementString("WindowState", windowState.ToString());
            writer.WriteElementString("WindowRectangle", XmlHelper.WriteRectangleF(windowRectangle));

            if (workspace != null && workspace.Screens != null && workspace.Screens.Count > 0)
            {
                writer.WriteStartElement("Workspace");
                workspace.WriteXML(writer);
                writer.WriteEndElement();
            }

            writer.WriteElementString("AllowMultipleInstances", allowMultipleInstances ? "true" : "false");
            writer.WriteElementString("InstancesOwnPreferences", instancesOwnPreferences ? "true" : "false");
            writer.WriteElementString("PreferencesPage", preferencePage.ToString());
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
                    case "ExplorerVisible":
                        explorerVisible = XmlHelper.ParseBoolean(reader.ReadElementContentAsString());
                        break;
                    case "ExplorerSplitterDistance":
                        explorerSplitterDistance = reader.ReadElementContentAsInt();
                        break;
                    case "WindowState":
                        windowState = (FormWindowState)Enum.Parse(typeof(FormWindowState), reader.ReadElementContentAsString());
                        break;
                    case "WindowRectangle":
                        windowRectangle = XmlHelper.ParseRectangle(reader.ReadElementContentAsString());
                        break;
                    case "Workspace":
                        workspace.ReadXML(reader);
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
                    default:
                        reader.ReadOuterXml();
                        break;
                }
            }

            reader.ReadEndElement();
        }
    }
}
