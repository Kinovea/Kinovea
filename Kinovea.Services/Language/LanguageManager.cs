#region License
/*
Copyright © Joan Charmant 2011.
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace Kinovea.Services
{
    public static class LanguageManager
    {
        public static Dictionary<string, string> Languages
        {
            get 
            { 
                return languages;
            }
        }
        
        private static Dictionary<string, string> languages = null;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        static LanguageManager()
        {
            // Alphabetical order by native name. (Check Wikipedia order if in doubt).
            // ISO 639-1: https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes

            languages = new Dictionary<string, string>();

            if (Software.Experimental || Debugger.IsAttached)
            {
                languages.Add("ar", "العَرَبِية");
                languages.Add("bg", "Български");
                languages.Add("ca", "Català");
                languages.Add("cs", "Čeština");
                languages.Add("da", "Dansk");
                languages.Add("de", "Deutsch");
                languages.Add("el", "Ελληνικά");
                languages.Add("en", "English");
                languages.Add("es", "Español");
                languages.Add("fa", "فارسی");
                languages.Add("fr", "Français");
                languages.Add("ko", "한국어");
                languages.Add("id", "Bahasa Indonesia");
                languages.Add("it", "Italiano");
                languages.Add("lt", "Lietuvių");
                languages.Add("mk", "Македонски");
                languages.Add("ms", "Bahasa Melayu");
                languages.Add("nl", "Nederlands");
                languages.Add("ja", "日本語");
                languages.Add("no", "Norsk bokmål");
                languages.Add("pl", "Polski");
                languages.Add("pt", "Português");
                languages.Add("ro", "Română");
                languages.Add("ru", "Русский");
                languages.Add("sr-Cyrl-RS", "Српски");
                languages.Add("sr-Latn-RS", "Srpski");
                languages.Add("fi", "Suomi");
                languages.Add("sv", "Svenska");
                languages.Add("th", "ไทย");
                languages.Add("tr", "Türkçe");
                languages.Add("zh-CHS", "简体中文");
            }
            else
            {
                // For the normal version we only include languages that match the inclusion heuristic described in inclusion.txt.
                languages.Add("ar", "العَرَبِية");
                languages.Add("bg", "Български");
                languages.Add("ca", "Català");
                languages.Add("cs", "Čeština"); 
                //languages.Add("da", "Dansk"); // Root < 75%
                languages.Add("de", "Deutsch");
                //languages.Add("el", "Ελληνικά"); // Root < 75%
                languages.Add("en", "English");
                languages.Add("es", "Español");
                languages.Add("fa", "فارسی");
                languages.Add("fr", "Français");
                //languages.Add("ko", "한국어"); // Root < 75%
                languages.Add("id", "Bahasa Indonesia"); 
                languages.Add("it", "Italiano"); 
                languages.Add("lt", "Lietuvių");
                languages.Add("mk", "Македонски");
                //languages.Add("ms", "Bahasa Melayu"); // Root < 75%
                languages.Add("nl", "Nederlands");
                //languages.Add("ja", "日本語"); // Root < 75%
                //languages.Add("no", "Norsk bokmål"); // Untranslated menu & player
                languages.Add("pl", "Polski");
                languages.Add("pt", "Português");
                languages.Add("ro", "Română");
                languages.Add("ru", "Русский");
                //languages.Add("sr-Cyrl-RS", "Српски"); // Root < 75%
                //languages.Add("sr-Latn-RS", "Srpski"); // Root < 75%
                //languages.Add("fi", "Suomi"); // Root < 75%
                languages.Add("sv", "Svenska"); // ?
                //languages.Add("th", "ไทย"); // Untranslated menu & player
                languages.Add("tr", "Türkçe");
                languages.Add("zh-CHS", "简体中文");
            }

        }

        /// <summary>
        /// Returns true if there is a translation for the passed in culture or a direct parent for a subculture.
        /// </summary>
        public static bool IsSupportedCulture(CultureInfo ci)
        {
            return languages.ContainsKey(ci.Name) || (!ci.IsNeutralCulture && languages.ContainsKey(ci.Parent.Name));
        }

        /// <summary>
        /// This method should only be used for UI purposes, to check a menu or select an entry in a combo box.
        /// It doesn't necessarily return the actual exact culture name, as some of them get mapped around.
        /// </summary>
        public static string GetCurrentCultureName()
        {
            string uiCultureName = Thread.CurrentThread.CurrentUICulture.Name;
            CultureInfo ci = new CultureInfo(uiCultureName);

            if (languages.ContainsKey(uiCultureName))
                return uiCultureName;
            else if (!ci.IsNeutralCulture && languages.ContainsKey(ci.Parent.Name))
                return ci.Parent.Name;
            else
                return "en";
        }
    }
}
