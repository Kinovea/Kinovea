#region License
/*
Copyright © Joan Charmant 2011. joan.charmant@gmail.com 
Licensed under the MIT license: http://www.opensource.org/licenses/mit-license.php
*/
#endregion
using System;
using System.IO;
using System.Timers;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Automatically saves the metadata at regular intervals if needed.
    /// </summary>
    public class AutoSaver
    {
        #region Members
        private bool m_Enabled;
        private Metadata m_Metadata;
        private Timer m_Timer = new Timer();
        private int m_RefHash;
        private static readonly int m_Interval = 60 * 1000;
        //private static readonly int m_Interval = 5 * 1000;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Guid m_Guid = Guid.NewGuid();
        private string m_AutosaveDirectory;
        private string m_AutosaveDiscoveryFile;
        #endregion
        
        public AutoSaver()
        {
            m_Enabled = false;
            
            m_Timer.Interval = m_Interval;
            m_Timer.Elapsed += Timer_Elapsed;
            m_AutosaveDirectory = PreferencesManager.SettingsFolder + PreferencesManager.ResourceManager.GetString("TempFolder");
            m_AutosaveDiscoveryFile = String.Format("{0}\\autosave-{1}.txt", m_AutosaveDirectory, m_Guid);
        }
        public void SetMetadata(Metadata _metadata)
        {
            if(!m_Enabled || _metadata == null)
                return;

            m_Metadata = _metadata;
            Clean();
        }
        public void Start()
        {
            if(!m_Enabled || m_Timer.Enabled)
                return;
            
            m_Timer.Start();
            log.DebugFormat("Autosaver start.");
        }
        public void Stop()
        {
            if(!m_Enabled || !m_Timer.Enabled)
                return;
            
            m_Timer.Stop();
            log.DebugFormat("Autosaver stop");
        }
        public void Clean()
        {
            if(!m_Enabled || m_Metadata == null)
                return;
            
           m_RefHash = m_Metadata.GetHashCode();
           DeleteDiscoveryFile();
           log.DebugFormat("Autosave cleaned. - {0}", m_RefHash);
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(m_Metadata == null)
                return;
            
            int hash = m_Metadata.GetHashCode();
            if(hash != m_RefHash)
            {
                Save();
                log.DebugFormat("Autosave saving. - {0}", hash);
                m_RefHash = hash;
            }
        }
        private void Save()
        {
            if(!m_Enabled)
                return;
            
            // TODO: Save to temporary file.
            CreateDiscoveryFile();
        }
        private void CreateDiscoveryFile()
        {
            try
            {
                if(!Directory.Exists(m_AutosaveDirectory))
                   Directory.CreateDirectory(m_AutosaveDirectory);
                
                if(!File.Exists(m_AutosaveDiscoveryFile))
                    File.Create(m_AutosaveDiscoveryFile).Dispose();
            }
            catch(Exception)
            {
                log.ErrorFormat("An error occurred during creating the autosave discovery file.");
            }
        }
        private void DeleteDiscoveryFile()
        {
            try
            {
                if(File.Exists(m_AutosaveDiscoveryFile))
                    File.Delete(m_AutosaveDiscoveryFile);
            }
            catch(Exception)
            {
                log.ErrorFormat("An error occurred during deleting the autosave discovery file.");
            }
        }
    }
}
