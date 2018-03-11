using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace UnusedResources
{
    public class UnusedResourcesFinder
    {
        private string root;
        private string result;
        private HashSet<string> ignore = new HashSet<string>() { "bin", "obj"};
        private string designer;

        public UnusedResourcesFinder(string root, string result)
        {
            this.root = root;
            this.result = result;

            if (Directory.Exists(result))
                Directory.Delete(result, true);

            Directory.CreateDirectory(result);
        }

        public void ProcessAssembly(string resx, List<string> directories, List<string> patterns)
        {
            string module = Path.GetFileNameWithoutExtension(resx);
            designer = module + ".Designer.cs";
            HashSet<string> defined = FindDefined(Path.Combine(root, resx));

            // Look into each file and try to find each resource string.
            HashSet<string> used = new HashSet<string>();
            foreach (string directory in directories)
            {
                Console.WriteLine("Looking for unused resources in {0}", Path.GetFileName(directory));
                string dir = Path.Combine(root, directory);
                ProcessDirectory(dir, patterns, defined, used);
            }
            
            IEnumerable<string> unused = defined.Except(used);
            
            SaveToFile(module, unused);
        }

        /// <summary>
        /// Collect all resources name from the resx file. 
        /// </summary>
        private HashSet<string> FindDefined(string resx)
        {
            HashSet<string> defined = new HashSet<string>();

            XmlDocument doc = new XmlDocument();
            doc.Load(resx);

            XmlNodeList nodes = doc.SelectNodes("/root/data");
            foreach (XmlNode node in nodes)
                defined.Add(node.Attributes["name"].Value);

            return defined;
        }

        private void ProcessDirectory(string directory, List<string> patterns, HashSet<string> defined, HashSet<string> used)
        {
            if (ignore.Contains(Path.GetFileName(directory)))
                return;

            // Process the files in this directory.
            foreach (string pattern in patterns)
            {
                foreach (string file in Directory.GetFiles(directory, pattern))
                {
                    if (Path.GetFileName(file) == designer)
                        continue;

                    // Look for each resource in this file and note which ones are actually used.
                    foreach (string resource in defined)
                    {
                        // Skip if we already found that resource somewhere else.
                        if (used.Contains(resource))
                            continue;

                        using (StreamReader r = new StreamReader(file))
                        {
                            string source = r.ReadToEnd();
                            bool found = source.Contains(resource);
                            if (found)
                                used.Add(resource);
                        }
                    }
                }
            }

            // Recurse into subdirectories.
            foreach (string dir in Directory.GetDirectories(directory))
            {
                ProcessDirectory(dir, patterns, defined, used);
            }
        }
        
        private void SaveToFile(string module, IEnumerable<string> unused)
        {
            if (unused.Count() == 0)
                return;

            string file = Path.Combine(result, module + ".txt");
            File.WriteAllLines(file, unused.ToArray());
        }
    }
}
