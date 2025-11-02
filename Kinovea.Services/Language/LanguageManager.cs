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
            

            if (Debugger.IsAttached)
                return;

            // For release, remove languages that don't match the inclusion heuristic below.
            //
            // Rationale:
            // It is frustrating and look unprofessional for users to select their language only to realize
            // that many parts of the interface are not actually translated.
            // 
            // Criteria:
            // 1. > 85% translated in total -> kept.
            // 2. < 50% translated in total -> removed.
            // 3. For the remaining languages, we look at
            // - the Root component (top level menu and preferences), 
            // - how the front UI looks like, File, View and Image, and the UI of the player.
            // If there are too many untranslated strings directly visible in the menu and the player UI
            // we do not include the language.

            // Reference: https://hosted.weblate.org/projects/kinovea/#languages
            // Last check: 2024-08-25.

            // Languages with less than 50% translation coverage.
            languages.Remove("az");         // Azerbaijani.
            languages.Remove("da");         // Danish.
            languages.Remove("el");         // Greek.
            languages.Remove("ms");         // Malay.
            languages.Remove("no");         // Norwegian.
            languages.Remove("sr-Latn-RS"); // Serbian (latin)
            languages.Remove("sl");         // Slovenian.
            languages.Remove("th");         // Thai.

            // Languages between 50% and 85%.
            //languages.Remove("ca");       // Catalan.
            //languages.Remove("ar");       // Arabic.
            //languages.Remove("bg");       // Bulgarian.
            //languages.Remove("cs");       // Czech
            //languages.Remove("de");       // German
            //languages.Remove("ko");       // Korean
            //languages.Remove("lt");       // Lithuanian
            languages.Remove("fa");       // Farsi.
            //languages.Remove("pl");       // Polish
            //languages.Remove("ro");       // Romanian
            languages.Remove("sr-Cyrl-RS"); // Serbian (cyrillic).
            languages.Remove("sv");         // Swedish.
            //languages.Remove("tr");       // Turkish.
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
