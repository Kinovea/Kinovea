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
                szDownloadFolder = Software.ManualsDirectory;
            }
            else
            {
                hiList = HelpVideos;
                szDownloadFolder = Software.HelpVideosDirectory;
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
                XmlTextWriter LocalHelpIndexWriter = new XmlTextWriter(Software.LocalHelpIndex, null);
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
