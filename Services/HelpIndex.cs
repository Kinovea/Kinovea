/*
Copyright © Joan Charmant 2008.
joan.charmant@gmail.com 
 
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
using System.Text;
using System.Xml;
using System.IO;
using System.Windows.Forms;

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
        public List<HelpItem>   UserGuides;
        public List<HelpItem>   HelpVideos;
        public bool             LoadSuccess;

        private XmlTextReader   m_XmlReader;
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

            // If we can't 
            try
            {
                m_XmlReader = new XmlTextReader(filePath);
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
            if (m_XmlReader != null)
            {
                try
                {
                    while (m_XmlReader.Read())
                    {
                        if ((m_XmlReader.IsStartElement()) && (m_XmlReader.Name == "kinovea"))
                        {
                            while (m_XmlReader.Read())
                            {
                                if (m_XmlReader.IsStartElement())
                                {
                                    if (m_XmlReader.Name == "software")
                                    {
                                        AppInfos = ParseAppInfos(); 
                                    }

                                    if (m_XmlReader.Name == "lang")
                                    {
                                        ParseHelpItems();
                                    }
                                }
                                else if (m_XmlReader.Name == "kinovea")
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
                    m_XmlReader.Close();
                }
            }
        }
        private ApplicationInfos ParseAppInfos()
        {
            ApplicationInfos ai = new ApplicationInfos();
            
            ai.Version = new ThreePartsVersion(m_XmlReader.GetAttribute("release"));

            while (m_XmlReader.Read())
            {
                if (m_XmlReader.IsStartElement())
                {
                    if (m_XmlReader.Name == "filesize")
                    {
                        ai.FileSizeInBytes = int.Parse(m_XmlReader.ReadString());
                    }

                    if (m_XmlReader.Name == "location")
                    {
                        ai.FileLocation = m_XmlReader.ReadString();
                    }

                    if (m_XmlReader.Name == "changelog")
                    {
                        ai.ChangelogLocation = m_XmlReader.ReadString();
                    }
                }
                else if (m_XmlReader.Name == "software")
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
            string lang = m_XmlReader.GetAttribute("id");

            while (m_XmlReader.Read())
            {
                if (m_XmlReader.IsStartElement())
                {
                    if (m_XmlReader.Name == "manual")
                    {
                        HelpItem hi = ParseHelpItem(lang, m_XmlReader.Name);
                        UserGuides.Add(hi);  
                    }

                    if (m_XmlReader.Name == "video")
                    {
                        HelpItem hi = ParseHelpItem(lang, m_XmlReader.Name);
                        HelpVideos.Add(hi);  
                    }
                }
                else if (m_XmlReader.Name == "lang")
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

            hi.Identification   = int.Parse(m_XmlReader.GetAttribute("id"));
            hi.Revision         = int.Parse(m_XmlReader.GetAttribute("revision"));
            hi.Language         = lang;

            while (m_XmlReader.Read())
            {
                if (m_XmlReader.IsStartElement())
                {
                    if (m_XmlReader.Name == "title")
                    {
                        hi.LocalizedTitle = m_XmlReader.ReadString(); 
                    }
                    if (m_XmlReader.Name == "filesize")
                    {
                        hi.FileSizeInBytes = int.Parse(m_XmlReader.ReadString());
                    }
                    if (m_XmlReader.Name == "location")
                    {
                        hi.FileLocation = m_XmlReader.ReadString();
                    }
                    if (m_XmlReader.Name == "comment")
                    {
                        hi.Comment = m_XmlReader.ReadString();
                    }
                }
                else if (m_XmlReader.Name == tag)
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

        #region Update
        public void UpdateIndex(HelpItem helpItem, int listId)
        {
            //-----------------------------------------------------
            // Vérifier s'il existe déjà, mettre à jour ou ajouter.
            //-----------------------------------------------------

            // 1. Choix de la liste.
            List<HelpItem> hiList;
            string szDownloadFolder = ""; ;
            if(listId == 0)
            {
                hiList = UserGuides;
                szDownloadFolder = Application.StartupPath + "\\" + Properties.Resources.ManualsFolder;
            }
            else
            {
                hiList = HelpVideos;
                szDownloadFolder = Application.StartupPath + "\\" + Properties.Resources.HelpVideosFolder;
            }

            // 2. Recherche de l'Item.
            bool found = false;
            int i = 0;
            while (!found && i < hiList.Count)
            {
                if (helpItem.Identification == hiList[i].Identification && helpItem.Language == hiList[i].Language)
                {
                    found = true;
                    // Mise à jour.
                    UpdateHelpItem(hiList[i], helpItem, szDownloadFolder);
                }
                else
                {
                    i++;
                }
            }

            if (!found)
            {
                // Ajout.
                HelpItem hiNew = new HelpItem();
                UpdateHelpItem(hiNew, helpItem, szDownloadFolder);
                hiList.Add(hiNew);  
            }
        }
        private void UpdateHelpItem(HelpItem _hiLocalCopy, HelpItem _hiUpdatedCopy, string _szFolder)
        {
            // rempli plus tard dynamiquement : _hiLocalCopy.Description
            _hiLocalCopy.FileLocation = _szFolder + "\\" + Path.GetFileName(_hiUpdatedCopy.FileLocation);
            _hiLocalCopy.FileSizeInBytes = _hiUpdatedCopy.FileSizeInBytes;
            _hiLocalCopy.Identification = _hiUpdatedCopy.Identification;
            _hiLocalCopy.Language = _hiUpdatedCopy.Language;
            _hiLocalCopy.LocalizedTitle = _hiUpdatedCopy.LocalizedTitle;
            _hiLocalCopy.Revision = _hiUpdatedCopy.Revision;
            _hiLocalCopy.Comment = _hiUpdatedCopy.Comment;
        }
        public void WriteToDisk()
        {
            try
            {
                XmlTextWriter LocalHelpIndexWriter = new XmlTextWriter(Application.StartupPath + "\\" + Properties.Resources.URILocalHelpIndex, null);
                LocalHelpIndexWriter.Formatting = Formatting.Indented;
                LocalHelpIndexWriter.WriteStartDocument();

                LocalHelpIndexWriter.WriteStartElement("kinovea");
                LocalHelpIndexWriter.WriteStartElement("software");
                LocalHelpIndexWriter.WriteAttributeString("release", AppInfos.Version.ToString());
                LocalHelpIndexWriter.WriteString(" ");// placeholder necessary due to the parser algo.
                LocalHelpIndexWriter.WriteEndElement();

                // On retrie les items par langues.
                List<LangGroup> LangList = new List<LangGroup>();
                SortByLang(LangList, UserGuides, "manual");
                SortByLang(LangList, HelpVideos, "video");

                // Ajouter les groupes de langues
                foreach (LangGroup lg in LangList)
                {
                    LocalHelpIndexWriter.WriteStartElement("lang");
                    LocalHelpIndexWriter.WriteAttributeString("id", lg.Lang);
                    for (int i = 0; i < lg.Items.Count; i++)
                    {
                        LocalHelpIndexWriter.WriteStartElement(lg.ItemTypes[i]);
                        LocalHelpIndexWriter.WriteAttributeString("id", lg.Items[i].Identification.ToString());
                        LocalHelpIndexWriter.WriteAttributeString("revision", lg.Items[i].Revision.ToString());

                        LocalHelpIndexWriter.WriteElementString("title", lg.Items[i].LocalizedTitle);
                        LocalHelpIndexWriter.WriteElementString("filesize", lg.Items[i].FileSizeInBytes.ToString());
                        LocalHelpIndexWriter.WriteElementString("location", lg.Items[i].FileLocation);
                        LocalHelpIndexWriter.WriteElementString("comment", lg.Items[i].Comment);

                        LocalHelpIndexWriter.WriteEndElement();
                    }
                    LocalHelpIndexWriter.WriteEndElement();
                }
                LocalHelpIndexWriter.WriteEndElement();
                LocalHelpIndexWriter.WriteEndDocument();
                LocalHelpIndexWriter.Flush();
                LocalHelpIndexWriter.Close();
            }
            catch (Exception)
            {
                // Possible cause: doesn't have rights to write.
            }
        }
        private void SortByLang(List<LangGroup> _SortedList, List<HelpItem> _InputList, string _szItemType)
        {
            foreach (HelpItem item in _InputList)
            {
                // Vérifier si la langue est connue
                int iLangIndex = -1;
                for (int i = 0; i < _SortedList.Count; i++)
                {
                    if (item.Language == _SortedList[i].Lang)
                    {
                        iLangIndex = i;
                    }
                }

                if (iLangIndex == -1)
                {
                    // ajouter l'item dans une nouvelle langue.
                    LangGroup lg = new LangGroup();
                    lg.Lang = item.Language;
                    lg.Items = new List<HelpItem>();
                    lg.Items.Add(item);
                    lg.ItemTypes = new List<string>();
                    lg.ItemTypes.Add(_szItemType);
                    _SortedList.Add(lg);
                }
                else
                {
                    // ajouter l'item dans sa langue.
                    _SortedList[iLangIndex].Items.Add(item);
                    _SortedList[iLangIndex].ItemTypes.Add(_szItemType);
                }
            }


        }
        #endregion
    }
}
