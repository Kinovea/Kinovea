using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnusedResources
{
    /// <summary>
    /// Find unused resources and collect them in text files for possible manual deletion.
    /// This is used to clean up the translation files.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            string sourceDirectory = @"D:\dev\Joan\Multimedia\Video\Kinovea\Bitbucket\_MASTER3";
            string resultDirectory = @"D:\dev\Joan\Multimedia\Video\Kinovea\Bitbucket\_MASTER3\Tools\UnusedResources\UnusedResources\Results";
            
            UnusedResourcesFinder finder = new UnusedResourcesFinder(sourceDirectory, resultDirectory);
            finder.ProcessAssembly("Kinovea", "Languages\\RootLang.resx");
            finder.ProcessAssembly("Kinovea.FileBrowser", "Languages\\FileBrowserLang.resx");
            finder.ProcessAssembly("Kinovea.ScreenManager", "Languages\\ScreenManagerLang.resx");
            finder.ProcessAssembly("Kinovea.Updater", "Languages\\UpdaterLang.resx");

            Console.WriteLine("done.");
        }
    }
}
