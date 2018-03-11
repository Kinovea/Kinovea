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
            // Workflow:
            // - Run tool.
            // - Remove lines in the master localization file.
            // - Deploy locales.
            // - Compile & test if used strings were removed.
            // - Run tool again.
            
            string sourceDirectory = @"D:\dev\Joan\Multimedia\Video\Kinovea\Bitbucket\_MASTER4";
            string resultDirectory = @"D:\dev\Joan\Multimedia\Video\Kinovea\Bitbucket\_MASTER4\Tools\UnusedResources\UnusedResources\Results-20180311";
            
            UnusedResourcesFinder finder = new UnusedResourcesFinder(sourceDirectory, resultDirectory);

            List<string> directories = new List<string>();
            List<string> patterns = new List<string>();

            //---------------------------------------------
            // WARNING
            // Look for calls to "ResourceManager.GetString("
            // This indicates that the code is building the resource name from a variable, which is not detected by this tool.
            //---------------------------------------------

            // Dynamic: dlgPreferences_Capture_PatternXXX.
            directories.Clear();
            directories.Add("Kinovea");
            patterns.Clear();
            patterns.Add("*.cs");
            finder.ProcessAssembly("Kinovea\\Languages\\RootLang.resx", directories, patterns);

            directories.Clear();
            directories.Add("Kinovea.FileBrowser");
            patterns.Clear();
            patterns.Add("*.cs");
            finder.ProcessAssembly("Kinovea.FileBrowser\\Languages\\FileBrowserLang.resx", directories, patterns);


            // Dynamic: FileProperty_XXX, drawing tools display name (from XML file actually, so should be caught).
            directories.Clear();
            directories.Add("Kinovea.ScreenManager");
            directories.Add("Tools\\DrawingTools");
            directories.Add("Kinovea.Video");
            directories.Add("Kinovea.Video.Bitmap");
            directories.Add("Kinovea.Video.FFMpeg");
            directories.Add("Kinovea.Video.GIF");
            directories.Add("Kinovea.Video.SVG");
            directories.Add("Kinovea.Video.Synthetic");
            directories.Add("Kinovea.Pipeline");
            patterns.Clear();
            patterns.Add("*.cs");
            patterns.Add("*.cpp");
            patterns.Add("*.xml");
            finder.ProcessAssembly("Kinovea.ScreenManager\\Languages\\ScreenManagerLang.resx", directories, patterns);

            directories.Clear();
            directories.Add("Kinovea.Updater");
            patterns.Clear();
            patterns.Add("*.cs");
            finder.ProcessAssembly("Kinovea.Updater\\Languages\\UpdaterLang.resx", directories, patterns);

            directories.Clear();
            directories.Add("Kinovea.Camera");
            directories.Add("Kinovea.Camera.Basler");
            directories.Add("Kinovea.Camera.DirectShow");
            directories.Add("Kinovea.Camera.FrameGenerator");
            directories.Add("Kinovea.Camera.HTTP");
            directories.Add("Kinovea.Camera.IDS");
            patterns.Clear();
            patterns.Add("*.cs");
            finder.ProcessAssembly("Kinovea.Camera\\Languages\\CameraLang.resx", directories, patterns);

            Console.WriteLine("done.");
        }
    }
}
