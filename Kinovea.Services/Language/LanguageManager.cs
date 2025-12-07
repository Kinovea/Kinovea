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
        private static List<string> lowCoverage = new List<string>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        static LanguageManager()
        {
            // Alphabetical order by native name, using Wikipedia as reference.
            // ISO 639-1: https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes

            languages = new Dictionary<string, string>();
            languages.Add("ar", "العَرَبِية");
            languages.Add("fa", "فارسی");

            languages.Add("ja", "日本語");
            languages.Add("zh-CHS", "简体中文");
            languages.Add("zh-CHT", "繁體中文");
            languages.Add("ko", "한국어");
            languages.Add("th", "ไทย");

            languages.Add("az", "Azərbaycanca");
            languages.Add("bg", "Български");
            languages.Add("ca", "Català");
            languages.Add("cs", "Čeština");
            languages.Add("da", "Dansk");
            languages.Add("de", "Deutsch");
            languages.Add("el", "Ελληνικά");
            languages.Add("en", "English");
            languages.Add("es", "Español");
            languages.Add("et", "Eesti");
            languages.Add("fr", "Français");
            languages.Add("hr", "Hrvatski");
            languages.Add("id", "Bahasa Indonesia");
            languages.Add("it", "Italiano");
            languages.Add("lt", "Lietuvių");
            languages.Add("mk", "Македонски");
            languages.Add("ms", "Bahasa Melayu");
            languages.Add("nl", "Nederlands");
            languages.Add("no", "Norsk bokmål");
            languages.Add("pl", "Polski");
            languages.Add("pt", "Português");
            languages.Add("ro", "Română");
            languages.Add("ru", "Русский");
            languages.Add("sl", "Slovenščina");
            languages.Add("sr-Cyrl-RS", "Српски");
            languages.Add("sr-Latn-RS", "Srpski");
            languages.Add("fi", "Suomi");
            languages.Add("sv", "Svenska");
            languages.Add("tr", "Türkçe");
            languages.Add("uk", "Українська");
            languages.Add("vi", "Tiếng Việt");

            //------------------------------------------
            // Add low-coverage languages to a list.
            // The user can enable all languages in the preferences.
            // Low coverage is defined as less than 50% coverage in either 
            // the whole project, the Root component or the ScreenManager component.
            // Reference: https://hosted.weblate.org/projects/kinovea/#languages
            //
            // Last check: 2025-12-07.
            //------------------------------------------
            lowCoverage.Add("az");         // Azerbaijani.
            lowCoverage.Add("bg");         // Bulgarian.
            lowCoverage.Add("da");         // Danish.
            lowCoverage.Add("el");         // Greek.
            lowCoverage.Add("et");         // Estonian.
            lowCoverage.Add("fa");         // Persian.
            lowCoverage.Add("ms");         // Malay.
            lowCoverage.Add("no");         // Norwegian.
            lowCoverage.Add("sr-Latn-RS"); // Serbian (latin)
            lowCoverage.Add("sr-Cyrl-RS"); // Serbian (cyrillic).
            lowCoverage.Add("sl");         // Slovenian.
            lowCoverage.Add("sv");         // Swedish.
            lowCoverage.Add("th");         // Thai.
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

        public static Dictionary<string, string> GetEnabledLanguages(bool enableAllLanguages)
        {
            Dictionary<string, string> availableLanguages = new Dictionary<string, string>();
            foreach (var kvp in languages)
            {
                if (enableAllLanguages || !lowCoverage.Contains(kvp.Key))
                {
                    availableLanguages.Add(kvp.Key, kvp.Value);
                }
            }

            return availableLanguages;
        }
    }
}
