using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Kinovea.Services;

namespace Kinovea.Camera.IDS
{
    /// <summary>
    /// A helper class to manage import and export of collection of parameters.
    /// The IDS cameras do not automatically save their configuration between sessions.
    /// The API has helper methods to store and read .ini files with serialized parameters.
    /// </summary>
    public static class ProfileHelper
    {
        private static string profilesDirectory = Path.Combine(Software.CameraProfilesDirectory, "IDS");
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static ProfileHelper()
        {
            if (!Directory.Exists(profilesDirectory))
                Directory.CreateDirectory(profilesDirectory);
        }

        public static string GetProfileFilename(string identifier)
        {
            string filename = string.Format("{0}.ini", identifier);
            return Path.Combine(profilesDirectory, filename);
        }

        public static void Save(uEye.Camera camera, string filename)
        {
            try
            {
                camera.Parameter.Save(filename);
            }
            catch (Exception e)
            {
                log.Error(string.Format("Error while saving camera parameter set at {0}.", filename), e);
            }
        }

        public static bool Load(uEye.Camera camera, string filename)
        {
            bool result = false;
            try
            {
                if (File.Exists(filename))
                {
                    camera.Parameter.Load(filename);
                    result = true;
                }
                else
                {
                    log.DebugFormat("Camera parameter set not found.");
                }
            }
            catch (Exception e)
            {
                log.Error(string.Format("Error while loading camera parameter set at {0}.", filename), e);
            }

            return result;
        }

        public static void Delete(string identifier)
        {
            string filename = GetProfileFilename(identifier);

            try
            {
                if (File.Exists(filename))
                    File.Delete(filename);
            }
            catch (Exception e)
            {
                log.Error(string.Format("Error while deleting camera parameter set at {0}.", filename), e);
            }
        }
    }
}
