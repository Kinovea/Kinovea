#region License
/*
Copyright © Joan Charmant 2012.
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
#endregion
using System;
using System.Globalization;
using System.Threading;
using System.Xml;

namespace Kinovea.Services
{
    public class GeneralPreferences
    {
        public bool ExplorerVisible
        {
            get { return explorerVisible; }
            set { explorerVisible = value;}
        }
        public int ExplorerSplitterDistance
        {
            get { return explorerSplitterDistance; }
            set { explorerSplitterDistance = value; }
        }
        
        private string uiCultureName;
        private bool explorerVisible = true;
        private int explorerSplitterDistance = 250;
        
        
        public GeneralPreferences()
        {
            uiCultureName = Thread.CurrentThread.CurrentUICulture.Name;            
        }
        
        public void Import()
    	{
    	    
    	}
        
        public void SetCulture(string cultureName)
        {
            uiCultureName = cultureName;
        }
        
        public CultureInfo GetSupportedCulture()
        {
        	// Returns the actual culture used in the UI.
        	CultureInfo ci = new CultureInfo(uiCultureName);
        	if(LanguageManager.IsSupportedCulture(ci))
        		return ci;
        	else
        		return new CultureInfo("en");
        }
        
        public void WriteXML(XmlWriter writer)
        {
            writer.WriteElementString("Culture", uiCultureName);
            writer.WriteElementString("ExplorerVisible", explorerVisible ? "true" : "false");
            writer.WriteElementString("ExplorerSplitterDistance", explorerSplitterDistance.ToString());
        }
        
        public void ReadXML(XmlReader reader)
        {
            reader.ReadStartElement();

            while(reader.NodeType == XmlNodeType.Element)
			{
				switch(reader.Name)
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
                    default:
                        reader.ReadOuterXml();
                        break;
				}
            }
            
            reader.ReadEndElement();   
        }
    }
}
