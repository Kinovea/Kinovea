/*
Copyright © Joan Charmant 2008.
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


using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace Kinovea.Services
{
    public class ApplicationInfos
    {
        public ThreePartsVersion Version;
        public int FileSizeInBytes;
        public String FileLocation;
        public String ChangelogLocation;
    }

    public class LangGroup
    {
        public string Lang;
        public List<HelpItem> Items;
        public List<string> ItemTypes;
    }

    public class HelpIndex
    {
        #region Members
        public ApplicationInfos AppInfos;
        public List<HelpItem> UserGuides;
        public List<HelpItem> HelpVideos;
        public bool LoadSuccess;

        private XmlTextReader   xmlReader;
        #endregion

        #region Construction
        public HelpIndex()
        {
            // Used for writing the conf to file.
            Init();
        }
        public HelpIndex(string filePath)
        {
            // Used to read conf from file.
            Init();

            try
            {
                xmlReader = new XmlTextReader(filePath);
                ParseConfigFile();
            }
            catch (System.Exception)
            {
                LoadSuccess = false;
            }
        }
        #endregion

        #region Init
        private void Init()
        {
            AppInfos = new ApplicationInfos();
            UserGuides = new List<HelpItem>();
            HelpVideos = new List<HelpItem>();
            LoadSuccess = true;
        }
        #endregion

        #region Parsing
        private void ParseConfigFile()
        {
            //-----------------------------------------------------------
            // Fill the local variables with infos found in the XML file.
            //-----------------------------------------------------------
            if (xmlReader != null)
            {
                try
                {
                    while (xmlReader.Read())
                    {
                        if ((xmlReader.IsStartElement()) && (xmlReader.Name == "kinovea"))
                        {
                            while (xmlReader.Read())
                            {
                                if (xmlReader.IsStartElement())
                                {
                                    if (xmlReader.Name == "software")
                                    {
                                        AppInfos = ParseAppInfos(); 
                                    }

                                    if (xmlReader.Name == "lang")
                                    {
                                        ParseHelpItems();
                                    }
                                }
                                else if (xmlReader.Name == "kinovea")
                                {
                                    break;
                                }
                                else
                                {
                                    // Fermeture d'un tag interne.
                                }
                            }
                        }
                    }
                    LoadSuccess = true;
                }
                catch (Exception)
                {
                    // Une erreur est survenue pendant le parsing.
                    LoadSuccess = false;
                }
                finally
                {
                    xmlReader.Close();
                }
            }
        }
        private ApplicationInfos ParseAppInfos()
        {
            ApplicationInfos ai = new ApplicationInfos();
            
            ai.Version = new ThreePartsVersion(xmlReader.GetAttribute("release"));

            while (xmlReader.Read())
            {
                if (xmlReader.IsStartElement())
                {
                    if (xmlReader.Name == "filesize")
                    {
                        ai.FileSizeInBytes = int.Parse(xmlReader.ReadString());
                    }

                    if (xmlReader.Name == "location")
                    {
                        ai.FileLocation = xmlReader.ReadString();
                    }

                    if (xmlReader.Name == "changelog")
                    {
                        ai.ChangelogLocation = xmlReader.ReadString();
                    }
                }
                else if (xmlReader.Name == "software")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
            }

            return ai;
        }
        private void ParseHelpItems()
        {
            string lang = xmlReader.GetAttribute("id");

            while (xmlReader.Read())
            {
                if (xmlReader.IsStartElement())
                {
                    if (xmlReader.Name == "manual")
                    {
                        HelpItem hi = ParseHelpItem(lang, xmlReader.Name);
                        UserGuides.Add(hi);  
                    }

                    if (xmlReader.Name == "video")
                    {
                        HelpItem hi = ParseHelpItem(lang, xmlReader.Name);
                        HelpVideos.Add(hi);  
                    }
                }
                else if (xmlReader.Name == "lang")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
            }
        }
        private HelpItem ParseHelpItem(string lang, string tag)
        {
            HelpItem hi = new HelpItem();

            hi.Identification   = int.Parse(xmlReader.GetAttribute("id"));
            hi.Revision         = int.Parse(xmlReader.GetAttribute("revision"));
            hi.Language         = lang;

            while (xmlReader.Read())
            {
                if (xmlReader.IsStartElement())
                {
                    if (xmlReader.Name == "title")
                    {
                        hi.LocalizedTitle = xmlReader.ReadString(); 
                    }
                    if (xmlReader.Name == "filesize")
                    {
                        hi.FileSizeInBytes = int.Parse(xmlReader.ReadString());
                    }
                    if (xmlReader.Name == "location")
                    {
                        hi.FileLocation = xmlReader.ReadString();
                    }
                    if (xmlReader.Name == "comment")
                    {
                        hi.Comment = xmlReader.ReadString();
                    }
                }
                else if (xmlReader.Name == tag)
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
            }

            return hi;
        }
        #endregion
    }
}
