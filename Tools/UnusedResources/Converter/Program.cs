using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Converter
{
  public class Program
  {
        /// <summary>
        /// Converts a text file containing translated strings for an older revision of the localization file or that aren't in the correct order.
        /// The output is another text file ready to be copy-pasted into a language column.
        /// </summary>
        public static void Main(string[] args)
        {
            ConvertOrder();
            


        }
    
        private static void ConvertOrder()
        {
            // Fix resource order.
            // The order in the final resource file depends on the order in the localization file which is NOT alphabetical.
            // The order in some exports is alphabetical if the translator started directly from the assembly files.
            // Input: each line contain an entry and the traduction separated by an equal sign.

            string sourceDirectory = @"D:\dev\Joan\Multimedia\Video\Kinovea\Source\Kinovea\Localization\Contrib\r22\Bulgarian";
            string resxDirectory = @"D:\dev\Joan\Multimedia\Video\Kinovea\Source\Kinovea\Localization\WorkingDirectory";
            List<string> modules = new List<string>() { "Camera", "FileBrowser", "Root", "ScreenManager", "Updater" };

            foreach (var module in modules)
            {
                List<string> resources = GetResourceList(Path.Combine(resxDirectory, module + "Lang.resx"));
                Dictionary<string, string> translations = GetTranslations(Path.Combine(sourceDirectory, "Kinovea." + module + ".txt"));

                List<string> lines = new List<string>();

                // Match the order.
                foreach (string resource in resources)
                {
                    if (translations.ContainsKey(resource))
                    {
                        string line = resource + ";" + translations[resource];
                        lines.Add(line);
                    }
                }

                // Export final file.
                string outputFile = Path.Combine(sourceDirectory, "Fixed_" + module + ".txt");
                File.WriteAllLines(outputFile, lines.ToArray());

            }
        }

        /// <summary>
        /// Import all the official resources into a list.
        /// </summary>
        private static List<string> GetResourceList(string module)
        {
            List<string> resources = new List<string>();
            
            XmlDocument doc = new XmlDocument();
            doc.Load(module);

            XmlNodeList nodes = doc.SelectNodes("/root/data");
            foreach (XmlNode node in nodes)
            {
                XmlAttribute nameAttribute = node.Attributes["name"];
                if (nameAttribute == null)
                    continue;

                string resource = nameAttribute.Value.Trim();
                resources.Add(resource);
            }

            return resources;
        }

        private static Dictionary<string, string> GetTranslations(string module)
        {
            Dictionary<string, string> translations = new Dictionary<string, string>();

            //List<string> lines = File.ReadAllLines(module, Encoding.UTF8).ToList();
            List<string> lines = File.ReadAllLines(module, Encoding.UTF8).ToList();

            //List<string> lines = new List<string>() { "Generic_Apply = \"test\"" };
            foreach (string line in lines)
            {
                string[] tokens = line.Split(new char[] { '=' }, 2);
                if (tokens.Length != 2)
                    continue;

                string resource = tokens[0].Trim();
                bool bad = !char.IsLetterOrDigit(resource[resource.Length - 1]);
                if (bad)
                    resource = resource.Substring(0, resource.Length - 1);

                string translation = tokens[1].Trim();
                translation = translation.Substring(1, translation.Length - 2);
                translations.Add(resource.ToString(), translation);
                Debug.WriteLine("[{0}] = \"{1}\"", resource, translation);
            }

            return translations;
        }
    }
}
