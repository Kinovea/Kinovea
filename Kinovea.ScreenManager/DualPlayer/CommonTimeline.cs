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

        public void Initialize(PlayerScreen leftPlayer, long leftSyncTime, PlayerScreen rightPlayer, long rightSyncTime)
        {
            syncInfos.Clear();

            leftPlayer.LocalSyncTime = leftSyncTime;
            rightPlayer.LocalSyncTime = rightSyncTime;

            PlayerSyncInfo leftInfo = new PlayerSyncInfo();
            leftInfo.SyncTime = leftSyncTime;
            leftInfo.LastTime = leftPlayer.LocalLastTime;

            PlayerSyncInfo rightInfo = new PlayerSyncInfo();
            rightInfo.SyncTime = rightSyncTime;
            rightInfo.LastTime = rightPlayer.LocalLastTime;

            // Start of each video in common time. One will start at 0 while the other will have an offset.
            long offsetLeft = 0;
            long offsetRight = 0;

            if (leftSyncTime < rightSyncTime)
                offsetLeft = rightSyncTime - leftSyncTime;
            else
                offsetRight = leftSyncTime - rightSyncTime;

            leftInfo.Offset = offsetLeft;
            rightInfo.Offset = offsetRight;

            syncInfos.Add(leftPlayer.Id, leftInfo);
            syncInfos.Add(rightPlayer.Id, rightInfo);

            frameTime = Math.Min(leftPlayer.LocalFrameTime, rightPlayer.LocalFrameTime);

            long leftEnd = GetCommonTime(leftPlayer, leftInfo.LastTime);
            long rightEnd = GetCommonTime(rightPlayer, rightInfo.LastTime);
            commonLastTime = Math.Max(leftEnd, rightEnd);
        }
        
        public long GetLocalTime(PlayerScreen player, long commonTime)
        {
            if (!syncInfos.ContainsKey(player.Id))
                return 0;

            return commonTime - syncInfos[player.Id].Offset;
        }

        public long GetCommonTime(PlayerScreen player, long localTime)
        {
            if (!syncInfos.ContainsKey(player.Id))
                return 0;

            return syncInfos[player.Id].Offset + localTime;
        }

        public bool IsOutOfBounds(PlayerScreen player, long commonTime)
        {
            if (!syncInfos.ContainsKey(player.Id))
                return true;

            long localTime = GetLocalTime(player, commonTime);
            return localTime < 0 || localTime > syncInfos[player.Id].LastTime;
        }
    }
}
