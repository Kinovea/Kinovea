#region License
/*
Copyright © Joan Charmant 2011. joan.charmant@gmail.com 
Licensed under the MIT license: http://www.opensource.org/licenses/mit-license.php
*/
#endregion
using System;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    public class SummaryLoadedEventArgs : EventArgs
    {
        public readonly VideoSummary Summary;
        public readonly int Progress;
        public SummaryLoadedEventArgs(VideoSummary _summary, int _progress)
        {
            Summary = _summary;
            Progress = _progress;
        }
    }
}
