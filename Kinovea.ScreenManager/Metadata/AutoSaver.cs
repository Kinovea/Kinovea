#region License
/*
Copyright © Joan Charmant 2012.
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
using System.IO;
using System.Timers;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Automatically triggers metadata autosave at regular intervals.
    /// Implements its own IsDirty mechanics to avoid interfering with user manual saving.
    /// </summary>
    public class AutoSaver
    {
        #region Members
        private bool enabled;
        private Metadata metadata;
        private Timer timer = new Timer();
        private int referenceHash;
        private static readonly int interval = 30 * 1000;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public AutoSaver(Metadata metadata)
        {
            this.metadata = metadata;
            enabled = true;

            timer.Interval = interval;
            timer.Elapsed += (s, e) => Tick();
        }

        public void FreshStart()
        {
            Clear();
            Start();
        }

        public void Start()
        {
            if (!enabled || timer.Enabled)
                return;

            timer.Start();
        }

        public void Stop()
        {
            if (!enabled || !timer.Enabled)
                return;

            timer.Stop();
        }

        public void Clear()
        {
            if (!enabled || metadata == null)
                return;

            referenceHash = metadata.GetContentHash();
            log.DebugFormat("Autosave cleared. - {0}", referenceHash);
        }
        public void Tick()
        {
            Save();
        }

        private void Save()
        {
            if (!enabled || metadata == null)
                return;

            int hash = metadata.GetContentHash();
            if (hash != referenceHash)
            {
                log.DebugFormat("Autosave saving. Content hash:{0}, Reference:{1}.", hash, referenceHash);
                metadata.PerformAutosave();
                referenceHash = hash;
            }
        }
    }
}
