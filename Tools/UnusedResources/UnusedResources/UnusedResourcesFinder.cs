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

        public void ProcessAssembly(string directory, string resx)
        {
            string dir = Path.Combine(root, directory);
            string module = Path.GetFileNameWithoutExtension(resx);
            designer = module + ".Designer.cs";
            HashSet<string> defined = FindDefined(Path.Combine(dir, resx));
            HashSet<string> unused = FindUnused(dir, defined);
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

        /// <summary>
        /// Browse the sources and check usage of each defined resource.
        /// </summary>
        private HashSet<string> FindUnused(string directory, HashSet<string> defined)
        {
            Console.WriteLine("Looking for unused resources in {0}", Path.GetFileName(directory));
            HashSet<string> unused = new HashSet<string>();

            foreach (string resource in defined)
            {
                if (!IsResourceUsed(directory, resource))
                    unused.Add(resource);
            }

            return unused;
        }

        /// <summary>
        /// Recursively test files in a directory and return whether the specified resource was found anywhere.
        /// </summary>
        private bool IsResourceUsed(string directory, string resource)
        {
            if (ignore.Contains(Path.GetFileName(directory)))
                return false;

            foreach (string file in Directory.GetFiles(directory, "*.cs"))
            {
                if (Path.GetFileName(file) == designer)
                    continue;

                using (StreamReader r = new StreamReader(file))
                {
                    string source = r.ReadToEnd();
                    bool used = source.Contains(resource);
                    if (used)
                        return true;
                }
            }

            foreach (string dir in Directory.GetDirectories(directory))
            {
                bool used = IsResourceUsed(dir, resource);
                if (used)
                    return true;
            }

            return false;
        }

        private void SaveToFile(string module, HashSet<string> unused)
        {
            if (unused.Count == 0)
                return;

            string file = Path.Combine(result, module + ".txt");
            File.WriteAllLines(file, unused.ToArray());
        }
    }
}
