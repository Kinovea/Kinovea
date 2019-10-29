using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        /// <summary>
        /// Initialize synchro using players current time origins.
        /// </summary>
        public void Initialize(PlayerScreen leftPlayer, PlayerScreen rightPlayer)
        {
            // Start of each video in common time. One will start at 0 while the other will have an offset.
            long offsetLeft = 0;
            long offsetRight = 0;

            if (leftPlayer.LocalTimeOriginPhysical < rightPlayer.LocalTimeOriginPhysical)
                offsetLeft = rightPlayer.LocalTimeOriginPhysical - leftPlayer.LocalTimeOriginPhysical;
            else
                offsetRight = leftPlayer.LocalTimeOriginPhysical - rightPlayer.LocalTimeOriginPhysical;

            PlayerSyncInfo leftInfo = new PlayerSyncInfo();
            leftInfo.SyncTime = leftPlayer.LocalTimeOriginPhysical;
            leftInfo.LastTime = leftPlayer.LocalLastTime;
            leftInfo.Offset = offsetLeft;

            PlayerSyncInfo rightInfo = new PlayerSyncInfo();
            rightInfo.SyncTime = rightPlayer.LocalTimeOriginPhysical;
            rightInfo.LastTime = rightPlayer.LocalLastTime;
            rightInfo.Offset = offsetRight;

            syncInfos.Clear();
            syncInfos.Add(leftPlayer.Id, leftInfo);
            syncInfos.Add(rightPlayer.Id, rightInfo);

            frameTime = Math.Min(leftPlayer.LocalFrameTime, rightPlayer.LocalFrameTime);

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

            return commonTime - syncInfos[player.Id].Offset;
        }

        /// <summary>
        /// Converts a local time in a player into a common time.
        /// </summary>
        public long GetCommonTime(PlayerScreen player, long localTime)
        {
            if (!syncInfos.ContainsKey(player.Id))
                return 0;

            return syncInfos[player.Id].Offset + localTime;
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
