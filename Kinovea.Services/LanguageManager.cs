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
using System.Threading;

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
                if(object.ReferenceEquals(languages, null))
                    Initialize();
                
                return languages;
            }
        }
        
        #region Languages accessors by english name
        // This big list of static properties is to support language names in the credits box.
        // We should have a GetContributors method here instead ?
        public static string English
        {
            get { return Languages["en"]; }
        }
        public static string Catalan
        {
            get { return Languages["ca"]; }
        }
        public static string Czech
        {
            get { return Languages["cs"]; }
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
        public static string Korean
        {
            get { return Languages["ko"]; }
        }
        public static string Russian
        {
            get { return Languages["ru"]; }
        }
        public static string Serbian
        {
            get { return languages["sr-Latn-CS"]; }
        }
        public static string SerbianCyrl
        {
            get { return languages["sr-Cyrl-CS"]; }
        }
        public static string Japanese
        {
            get { return languages["ja"]; }
        }
        public static string Macedonian
        {
            get { return languages["mk"]; }
        }
        #endregion
        
        private static Dictionary<string, string> languages = null;
        
        public static void Initialize()
        {
            // Alphabetical order by native name. (Check Wikipedia order if in doubt).
            languages = new Dictionary<string, string>();
            languages.Add("ca", "Català");
            languages.Add("cs", "Čeština");
            languages.Add("da", "Dansk");
            languages.Add("de", "Deutsch");
            languages.Add("el", "Ελληνικά");
            languages.Add("en", "English");
            languages.Add("es", "Español");
            languages.Add("fr", "Français");
            languages.Add("ko", "한국어");
            languages.Add("it", "Italiano");
            languages.Add("lt", "Lietuvių");
            languages.Add("nl", "Nederlands");
            languages.Add("ja", "日本語");
            languages.Add("no", "Norsk");
            languages.Add("mk", "Македонски");
            languages.Add("pl", "Polski");
            languages.Add("pt", "Português");
            languages.Add("ro", "Română");
            languages.Add("ru", "Русский");
            languages.Add("sr-Cyrl-CS", "Српски");
            languages.Add("sr-Latn-CS", "Srpski");
            languages.Add("fi", "Suomi");
            languages.Add("sv", "Svenska");
            languages.Add("tr", "Türkçe");
            languages.Add("zh-CHS", "简体中文");
        }
        public static bool IsSupportedCulture(CultureInfo ci)
        {
            return Languages.ContainsKey(ci.Name) || (!ci.IsNeutralCulture && Languages.ContainsKey(ci.Parent.Name));
        }

        public static string GetCurrentCultureName()
        {
            string uiCultureName = Thread.CurrentThread.CurrentUICulture.Name;
            CultureInfo ci = new CultureInfo(uiCultureName);

            if (Languages.ContainsKey(uiCultureName))
                return uiCultureName;
            else if (!ci.IsNeutralCulture && Languages.ContainsKey(ci.Parent.Name))
                return ci.Parent.Name;
            else
                return "en";
        }
    }
}
