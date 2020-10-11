using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class CommonTimeline
    {
        public long LastTime
        {
            get { return commonLastTime; }
        }

        public long FrameTime
        {
            get { return frameTime; }
        }

        Dictionary<Guid, PlayerSyncInfo> syncInfos = new Dictionary<Guid, PlayerSyncInfo>();
        private long commonLastTime;
        private long frameTime;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Initialize synchro using players current time origins.
        /// </summary>
        public void Initialize(PlayerScreen leftPlayer, PlayerScreen rightPlayer)
        {
            PlayerSyncInfo leftInfo = new PlayerSyncInfo();
            leftInfo.SyncTime = leftPlayer.LocalTimeOriginPhysical;
            leftInfo.LastTime = leftPlayer.LocalLastTime;
            
            PlayerSyncInfo rightInfo = new PlayerSyncInfo();
            rightInfo.SyncTime = rightPlayer.LocalTimeOriginPhysical;
            rightInfo.LastTime = rightPlayer.LocalLastTime;
            
            leftInfo.Scale = 1.0;
            rightInfo.Scale = 1.0;
            if (PreferencesManager.PlayerPreferences.SyncByMotion)
            {
                long leftDuration = leftInfo.LastTime - leftInfo.SyncTime;
                long rightDuration = rightInfo.LastTime - rightInfo.SyncTime;
                rightInfo.Scale = (double)rightDuration / leftDuration;
            }

            // Start of each video in common time. One will start at 0 while the other will have an offset.
            // This is what aligns the videos on their respective time origin.
            long offsetLeft = 0;
            long offsetRight = 0;
            long rightOrigin = (long)(rightPlayer.LocalTimeOriginPhysical / rightInfo.Scale);
            if (leftPlayer.LocalTimeOriginPhysical < rightOrigin)
                offsetLeft = rightOrigin - leftPlayer.LocalTimeOriginPhysical;
            else
                offsetRight = leftPlayer.LocalTimeOriginPhysical - rightOrigin;
            
            leftInfo.Offset = offsetLeft;
            rightInfo.Offset = offsetRight;

            syncInfos.Clear();
            syncInfos.Add(leftPlayer.Id, leftInfo);
            syncInfos.Add(rightPlayer.Id, rightInfo);

            frameTime = Math.Min((long)(leftPlayer.LocalFrameTime * leftInfo.Scale), (long)(rightPlayer.LocalFrameTime * rightInfo.Scale));

            long leftEnd = GetCommonTime(leftPlayer, leftInfo.LastTime);
            long rightEnd = GetCommonTime(rightPlayer, rightInfo.LastTime);
            commonLastTime = Math.Max(leftEnd, rightEnd);
        }
        
        /// <summary>
        /// Converts a common time into a local time for a specific player.
        /// </summary>
        public long GetLocalTime(PlayerScreen player, long commonTime)
        {
            if (!syncInfos.ContainsKey(player.Id))
                return 0;

            return ((long)(commonTime * syncInfos[player.Id].Scale)) - syncInfos[player.Id].Offset;
        }

        /// <summary>
        /// Converts a local time in a player into a common time.
        /// </summary>
        public long GetCommonTime(PlayerScreen player, long localTime)
        {
            if (!syncInfos.ContainsKey(player.Id))
                return 0;

            return syncInfos[player.Id].Offset + ((long)(localTime / syncInfos[player.Id].Scale));
        }

        /// <summary>
        /// Test whether a given common time is outside the range of the player.
        /// </summary>
        public bool IsOutOfBounds(PlayerScreen player, long commonTime)
        {
            if (!syncInfos.ContainsKey(player.Id))
                return true;

            long localTime = GetLocalTime(player, commonTime);
            return localTime < 0 || localTime > syncInfos[player.Id].LastTime;
        }
    }
}
