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

namespace Kinovea.ScreenManager
{
	public class DartfishLibraryItemParser : IMetadataParser
    {
        private XmlTextReader m_XmlReader;
        private Metadata m_Metadata;
        private int m_iDepth;

        public  void Parse(XmlTextReader _xmlReader, Metadata _metadata)
        {
        	//-------------------------------------------------------
            // The .dartclip file will contain Keyframes data.
            // For global drawings, see .storyboard files...
			//
            // File contains nested LIBRARY_ITEM tags, 
            // we keep track of them with m_iDepth.
			//-------------------------------------------------------
			
            m_iDepth = 1;
            m_Metadata = _metadata;
            m_XmlReader = _xmlReader;
            
            //------------------------------------------------
            // Top level Item :
            // <NAME>filename.avi</NAME>
            // <ID>{50C2A5F9-95AF-474C-8D6F-392DDA59E947}</ID>
            // <VERSION subversion="1">2.0</VERSION>
            // <THUMBNAIL_INDEX>0</THUMBNAIL_INDEX>
            // <TYPE>1</TYPE>
            // <LIBRARY_ITEM>
            //------------------------------------------------
            while (m_XmlReader.Read())
            {
                if (m_XmlReader.IsStartElement())
                {
                    if (m_XmlReader.Name == "LIBRARY_ITEM")
                    {
                        m_iDepth++;
                        ParseLibraryItem();
                    }
                }
                else if (m_XmlReader.Name == "LIBRARY_ITEM")
                {
                    m_iDepth--;
                    if (m_iDepth == 0)
                    {
                        break;
                    }
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
            }
        }

        private void ParseLibraryItem()
        {
            // ItemType="Marker"
            string ItemType = m_XmlReader.GetAttribute("ItemType");
            if (ItemType == "Marker")
            {
                // Autres Attributs:
                // IN="37200000" 
                // UNIT="RefTime" 
                // OUT="37200000"

                while (m_XmlReader.Read())
                {
                    if (m_XmlReader.IsStartElement())
                    {
                        if (m_XmlReader.Name == "Library.MDProperties")
                        {
                            ParseProperties();
                        }
                        else if(m_XmlReader.Name == "Data")
                        {
                            ParseData();
                        }
                        else
                        {
                            // Unkown node.
                        }
                    }
                    else if (m_XmlReader.Name == "LIBRARY_ITEM")
                    {
                        m_iDepth--;
                        break;
                    }
                    else
                    {
                        // Fermeture d'un tag interne.
                    }
                }
            }
            else
            {
                // Unsupported.
            }
        }
        private void ParseProperties()
        {
            while (m_XmlReader.Read())
            {
                if (m_XmlReader.IsStartElement())
                {
                    if (m_XmlReader.Name == "Property")
                    {
                        // <Property Name="Comment" DefaultValue=""/>   
                        // <Property Name="Title" DefaultValue="">
                        //      <![CDATA[1]]>
                        // </Property>
                    }
                    else
                    {
                        // Unkown node.
                    }
                }
                else if (m_XmlReader.Name == "Library.MDProperties")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
            }
        }
        private void ParseData()
        {
            string id = m_XmlReader.GetAttribute("Id");
            if (id == "ODKeyPosition")
            {
                //---------------------------------------------------
                // Here is the fat string, 
                // ASCII encapsulated in XML encapsulated in CDATA...
                //---------------------------------------------------
                ParseODKeyPosition(m_XmlReader.ReadString());
            }
            else
            {
                // Unsupported.
            }
        }
        private void ParseODKeyPosition(string _xmlString)
        {

            // This is a whole new XML stream.
            
            StringReader reader = new StringReader(_xmlString);
            XmlTextReader xmlKeyPositionReader = new XmlTextReader(reader);

            if ((xmlKeyPositionReader.IsStartElement()) && (xmlKeyPositionReader.Name == "ODKeyPosition"))
            {
                while (xmlKeyPositionReader.Read())
                {
                    if (xmlKeyPositionReader.IsStartElement())
                    {
                        if (xmlKeyPositionReader.Name == "DrawingStream")
                        {
                            string payload = xmlKeyPositionReader.GetAttribute("Value");
  
                            // finally, we get something valuable...
                            ParsePayload(payload);
                        }
                        else
                        {
                            // Unknown
                        }
                    }
                    else if (xmlKeyPositionReader.Name == "ODKeyPosition")
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
        private void ParsePayload(string _data)
        {
            // This is it.
            // Now we have a big ASCII string: space separated values.
            // Offsets Reverse Engineered for Interoperability.

            // TODO: extract info with regexp.
			// 1. Récupérer toutes les valeurs dans un grand tableau.
			// 2. Créer des objets à partir du tableau.
			// 3. Au passage documenter le format.
			
            
        }
    
    }
}
