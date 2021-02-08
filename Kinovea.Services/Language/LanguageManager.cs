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
            get { return languages["sr-Latn-RS"]; }
        }
        public static string SerbianCyrl
        {
            get { return languages["sr-Cyrl-RS"]; }
        }
        public static string Japanese
        {
            get { return languages["ja"]; }
        }
        public static string Macedonian
        {
            get { return languages["mk"]; }
        }
        public static string Arabic
        {
            get { return Languages["ar"]; }
        }
        public static string Farsi
        {
            get { return Languages["fa"]; }
        }
        public static string Bulgarian
        {
            get { return Languages["bg"]; }
        }
        #endregion

        private static Dictionary<string, string> languages = null;
        private static List<string> legacyLanguages = null;
        private static bool useOldSerbianCodes = false;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        static LanguageManager()
        {
            // Alphabetical order by native name. (Check Wikipedia order if in doubt).
            languages = new Dictionary<string, string>();
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
            languages.Add("sr-Cyrl-RS", "Српски");
            languages.Add("sr-Latn-RS", "Srpski");
            languages.Add("fi", "Suomi");
            languages.Add("sv", "Svenska");
            languages.Add("tr", "Türkçe");
            languages.Add("zh-CHS", "简体中文");

            legacyLanguages = new List<string>();
            legacyLanguages.Add("sr-Cyrl-CS");
            legacyLanguages.Add("sr-Latn-CS");

            // Culture name for Serbian were changed in Windows 10. 
            // We try to instanciate the culture with the new name to see were we stand, and remap the names around otherwise.
            // This requires that we compile each serbian satellite assembly twice.
            // The new satellites are compiled automatically (Building under Windows 10), the old ones have to be built manually, outside Visual Studio.
            try
            {
                CultureInfo testSerbian = new CultureInfo("sr-Cyrl-RS");
                useOldSerbianCodes = false;

                // This uses the variable to make sure the instanciation attempt doesn't get optimized away.
                log.DebugFormat("Using new Serbian culture codes. (i.e: {0}).", testSerbian.Name); 
            }
            catch (ArgumentException)
            {
                useOldSerbianCodes = true;
                log.DebugFormat("Failed to instanciate Serbian locale, switch to old codename.");
            }
        }

        /// <summary>
        /// Returns true if there is a translation for the passed in culture or a direct parent for a subculture.
        /// </summary>
        /// <param name="ci"></param>
        /// <returns></returns>
        public static bool IsSupportedCulture(CultureInfo ci)
        {
            return languages.ContainsKey(ci.Name) || legacyLanguages.Contains(ci.Name) || (!ci.IsNeutralCulture && languages.ContainsKey(ci.Parent.Name));
        }

        /// <summary>
        /// This method should only be used for UI purposes, to check a menu or select an entry in a combo box.
        /// It doesn't necessarily return the actual exact culture name, as some of them get mapped around.
        /// </summary>
        public static string GetCurrentCultureName()
        {
            string uiCultureName = Thread.CurrentThread.CurrentUICulture.Name;
            CultureInfo ci = new CultureInfo(uiCultureName);

            if (useOldSerbianCodes)
            {
                // Remap the old codes to the new ones so that the UI looks correct.
                if (ci.Name == "sr-Cyrl-CS")
                    return "sr-Cyrl-RS";
                else if (ci.Name == "sr-Latn-CS")
                    return "sr-Latn-RS";
            }

            if (languages.ContainsKey(uiCultureName))
                return uiCultureName;
            else if (!ci.IsNeutralCulture && languages.ContainsKey(ci.Parent.Name))
                return ci.Parent.Name;
            else
                return "en";
        }

        /// <summary>
        /// Get a culture name supported on this specific platform.
        /// This may return a different culture name than the one passed in for a few languages.
        /// </summary>
        public static string FixCultureName(string name)
        {
            if (useOldSerbianCodes)
            {
                if (name == "sr-Cyrl-RS")
                    return "sr-Cyrl-CS";
                else if (name == "sr-Latn-RS")
                    return "sr-Latn-CS";
            }

            return name;
        }
    }
}
