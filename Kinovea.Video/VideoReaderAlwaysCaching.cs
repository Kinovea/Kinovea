#region License
/*
Copyright © Joan Charmant 2011.
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
using System.ComponentModel;

namespace Kinovea.Video
{
    /// <summary>
    /// A base class for video readers that always load the full video into the working zone.
    /// Provide some boiler plate code for irrelevant operations.
    /// Provide standard implementation for cache related operations, may be overriden if more specific behavior is needed.
    /// 
    /// Derived class can use the "Cache" protected property to manipulate the internal cache.
    /// </summary>
    public abstract class VideoReaderAlwaysCaching : VideoReader
    {
        #region Properties
        public override VideoFrame Current {
            get { return m_Cache.CurrentFrame;}
        }
        public override VideoSection WorkingZone {
            get { return m_Cache.WorkingZone; }
        }
        public override IWorkingZoneFramesContainer WorkingZoneFrames {
            get { return m_Cache;}
        }
        protected Cache Cache {
            get { return m_Cache;}
        }
        #endregion 
        
        private Cache m_Cache = new Cache();
        
        #region Methods
        public override bool MoveNext(int _skip, bool _decodeIfNecessary)
        {
            return m_Cache.MoveBy(_skip + 1);
        }
        public override bool MoveTo(long _timestamp)
        {
            return m_Cache.MoveTo(_timestamp);
        }
        public override void Close()
        {
            m_Cache.Clear();
        }
        
        public override void PostLoad(){}
        public override void UpdateWorkingZone(VideoSection _newZone, bool _forceReload, int _maxMemory, Action<DoWorkEventHandler> _workerFn){}
        public override void BeforeFrameEnumeration(){}
        public override void AfterFrameEnumeration(){}
        #endregion
    }
}
