using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Services;
using System.IO;

namespace Kinovea.ScreenManager
{
    public static class RecoveryManager
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static List<ScreenDescriptorPlayback> GetRecoverables()
        {
            if (!Directory.Exists(Software.TempDirectory))
                return null;

            List<ScreenDescriptorPlayback> recoverables = new List<ScreenDescriptorPlayback>();
            try
            {
                foreach (string dir in Directory.GetDirectories(Software.TempDirectory))
                {
                    ScreenDescriptorPlayback sdp = GetRecoverable(dir);
                    if (sdp != null)
                    {
                        recoverables.Add(sdp);
                    }
                    else
                    {
                        Directory.Delete(dir, true);
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("An error happened while trying to get recoverable files.");
                log.Error(e.ToString());
            }

            return recoverables;
        }

        private static ScreenDescriptorPlayback GetRecoverable(string dir)
        {
            if (!Directory.Exists(dir))
                return null;

            ScreenDescriptorPlayback sdp = null;

            try
            {
                string dirName = Path.GetFileName(dir);
                Guid id = new Guid(dirName);

                string filename = null;
                DateTime lastSave = DateTime.MinValue;
                foreach (string entry in Directory.GetFiles(dir))
                {
                    if (Path.GetFileName(entry) != "autosave.kva")
                        continue;

                    filename = MetadataSerializer.ExtractFullPath(entry);
                    lastSave = File.GetLastWriteTime(entry);
                    break;
                }

                if (!string.IsNullOrEmpty(filename) && File.Exists(filename))
                {
                    sdp = new ScreenDescriptorPlayback();
                    sdp.Id = id;
                    sdp.FullPath = filename;
                    sdp.RecoveryLastSave = lastSave;
                }
                else
                {
                    log.Error("Recovery data were found but the referenced file couldn't be found.");
                }
            }
            catch (Exception e)
            {
                log.Error("An error happened while trying to get description of autosaved file");
                log.Error(e.ToString());
            }

            return sdp;
        }

    }
}
