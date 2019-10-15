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

        public static bool Load(uEye.Camera camera, string identifier)
        {
            string filename = GetProfileFilename(identifier);
            bool result = false;
            try
            {
                if (File.Exists(filename))
                {
                    log.DebugFormat("Loading IDS camera parameters from {0}.", Path.GetFileName(filename));
                    camera.Parameter.Load(filename);

                    // We do not support all incoming parameters. Incompatible parameters may happen when the 
                    // profile is imported from an external source.
                    bool reload = FixAbsoluteAOI(camera, filename);
                    if (reload)
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
                {
                    log.DebugFormat("Deleting IDS camera parameters at {0}.", Path.GetFileName(filename));
                    File.Delete(filename);
                }
            }
            catch (Exception e)
            {
                log.Error(string.Format("Error while deleting camera parameter set at {0}.", filename), e);
            }
        }

        /// <summary>
        /// Replaces the profile used by Kinovea by an external one.
        /// </summary>
        public static void Replace(string identifier, string sourceFilename)
        {
            string destFilename = GetProfileFilename(identifier);
            try
            {
                //if (File.Exists(destFilename))
                //  File.Delete(filename);
                log.DebugFormat("Replacing IDS camera parameters {0} <- {1}.", Path.GetFileName(destFilename), Path.GetFileName(sourceFilename));
                File.Copy(sourceFilename, destFilename, true);
            }
            catch (Exception e)
            {
                log.Error(string.Format("Error while importing camera parameter set at {0}.", destFilename), e);
            }
        }

        private static bool FixAbsoluteAOI(uEye.Camera camera, string filename)
        {
            // "Show AOI only".
            // Unfortunately IDS api doesn't support writing the absolute position parameter which controls whether 
            // we get images of just the AOI size (absolute = false) or images of the full size and black borders (absolute = true).
            // Since we do not want to support this weird feature at the moment, we rewrite the .ini on the fly and reload.
            bool absX, absY;
            camera.Size.AOI.GetAbsX(out absX);
            camera.Size.AOI.GetAbsY(out absY);
            if (!absX && !absY)
                return false;

            string[] lines = File.ReadAllLines(filename);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("Start X absolute"))
                {
                    lines[i] = "Start X absolute=0";
                    continue;
                }

                if (lines[i].StartsWith("Start Y absolute"))
                {
                    lines[i] = "Start Y absolute=0";
                    continue;
                }
            }

            File.WriteAllLines(filename, lines);
            return true;
        }
    }
}
