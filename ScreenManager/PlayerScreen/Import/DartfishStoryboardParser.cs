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
using System.Xml;

namespace Kinovea.ScreenManager
{
	public class DartfishStoryboardParser : IMetadataParser
    {
        public void Parse(XmlTextReader _xmlReader, Metadata _metadata)
        {
            // We will try to parse the Data from the storyboard,
            // and fill our Metadata class as best as we can.
			//
            // The .storyboard file contains the global drawings only.
			//
            // As these drawings are global, we will tie them to the first 
            // key image and turn persistence to infinity. 
            while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "Data")
                    {
                        ParseData(_xmlReader, _metadata);
                    }
                }
                else if (_xmlReader.Name == "Storyboard")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
            }
        }
        private void ParseData(XmlTextReader _xmlReader, Metadata _metadata)
        {
            while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "ODProject")
                    {
                        ParseODProject(_xmlReader, _metadata);
                    }
                }
                else if (_xmlReader.Name == "Data")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
            }
        }
        private void ParseODProject(XmlTextReader _xmlReader, Metadata _metadata)
        {
            while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                { 
                    switch(_xmlReader.Name)
                    {
                        case "ODStream":
                    		// Not supported.
                            break;
                        case "dmmStream_0":
                            // Not supported.
                            break;
                        case "drawingStream_0":
                            // TODO:
                            break;
                        case "DataSources_0":
                            // Not supported.
                            break;
                        case "MosaicManager_0":
                            // Not supported.
                            break;
                        default:
                            // Not supported.
                            break;
                    }
                }
                else if (_xmlReader.Name == "ODProject")
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
}
