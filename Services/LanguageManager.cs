#region License
/*
Copyright © Joan Charmant 2011.
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
using System.Collections.Generic;
using System.Globalization;

namespace Kinovea.Services
{
    /// <summary>
    /// Description of LanguageManager.
    /// </summary>
    public static class LanguageManager
    {
        public static Dictionary<string, string> Languages
        {
            get 
            { 
                if(object.ReferenceEquals(m_Languages, null))
                    Initialize();
                
                return m_Languages;
            }
        }
        
        #region Languages accessors by english name
        // This big list of static properties is to support language names in the credits box.
        // We should have a GetContributors method here instead ?
        public static string English
		{
			get { return Languages["en"]; }
		}
        public static string Danish
		{
			get { return Languages["da"]; }
		}
        public static string Dutch
		{
			get { return Languages["nl"]; }
		}
        public static string German
		{
			get { return Languages["de"]; }
		}
        public static string Portuguese
		{
			get { return Languages["pt"]; }
		}
        public static string Spanish
		{
			get { return Languages["es"]; }
		}
        public static string Italian
		{
			get { return Languages["it"]; }
		}
        public static string Romanian
		{
			get { return Languages["ro"]; }
		}
        public static string Polish
		{
			get { return Languages["pl"]; }
		}
        public static string Finnish
		{
			get { return Languages["fi"]; }
		}
        public static string Norwegian
		{
			get { return Languages["no"]; }
		}
        public static string Chinese
		{
			get { return Languages["zh-CHS"]; }
		}
        public static string Turkish
		{
			get { return Languages["tr"]; }
		}
        public static string Greek
		{
			get { return Languages["el"]; }
		}
        public static string Lithuanian
		{
			get { return Languages["lt"]; }
		}
        public static string Swedish
		{
			get { return Languages["sv"]; }
		}
        #endregion
        
        private static Dictionary<string, string> m_Languages = null;
        
        public static void Initialize()
        {
            // Alphabetical order by native name.
            m_Languages = new Dictionary<string, string>();
            m_Languages.Add("da", "Dansk");
            m_Languages.Add("de", "Deutsch");
            m_Languages.Add("el", "Ελληνικά");
            m_Languages.Add("en", "English");
            m_Languages.Add("es", "Español");
            m_Languages.Add("fr", "Français");
            m_Languages.Add("it", "Italiano");
            m_Languages.Add("lt", "Lietuvių");
            m_Languages.Add("nl", "Nederlands");
            m_Languages.Add("no", "Norsk");
            m_Languages.Add("pl", "Polski");
            m_Languages.Add("pt", "Português");
            m_Languages.Add("ro", "Română");
            m_Languages.Add("fi", "Suomi");
            m_Languages.Add("sv", "Svenska");
            m_Languages.Add("tr", "Türkçe");
            m_Languages.Add("zh-CHS", "简体中文");
        }
        public static bool IsSupportedCulture(CultureInfo ci)
        {
        	string neutral = ci.IsNeutralCulture ? ci.Name : ci.Parent.Name;
        	return Languages.ContainsKey(neutral);
        }
    }
}
