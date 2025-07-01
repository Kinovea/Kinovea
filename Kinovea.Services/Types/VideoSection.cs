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

namespace Kinovea.Services
{
    /// <summary>
    /// A section of the video, defined by sentinel timestamps.
    /// 一段由监控时间戳界定的视频片段。
    /// Can be used to describe the whole video, the working zone, the cache segment or any other section.
    /// 可用于描述整个视频、工作区域、缓存段或者任何其他部分。
    /// Note that the [0;0] section is valid. It describes a one-frame segment at timestamp 0.
    /// 请注意，[0;0] 这一部分是有效的。它描述的是在时间戳为 0 时的一帧片段。
    /// Negative timestamps are invalid and used to denote uninitialized section.
    /// 负时间戳是无效的，用于表示未初始化的段。
    /// </summary>
    public struct VideoSection : IComparable, IEquatable<VideoSection>
    {
        public bool IsEmpty {
            get { return (End < 0); }
        }
        public bool Wrapped {
            get { return !IsEmpty && End < Start;}
        }

        
        public readonly long Start;
        public readonly long End;
        
        public VideoSection(long start, long end)
        {
            this.Start = start;
            this.End = end;
        }

        public static VideoSection MakeEmpty()
        {
            return new VideoSection(-1, -1);
        }


        /// <summary>
        /// Returns true if this timestamp is contained in the section.
        /// 如果此时间戳存在于该段落中，则返回真。
        /// The bounds are part of the section.
        /// 这些界限是该部分的一部分。
        /// </summary>
        public bool Contains(long timestamp)
        {
            // Not really defined if wrapping.
            return !IsEmpty && timestamp >= Start && timestamp <= End;
        }

        /// <summary>
        /// Returns true if the passed section is entirely contained in this section.
        /// 如果传入的段落完全包含于当前段落内，则返回真值。
        /// </summary>
        public bool Contains(VideoSection _other)
        {
            return !IsEmpty && (_other < this || _other == this);
        }
        public override string ToString()
        {
            if(Wrapped)
                return string.Format("? --> {0}] [{1} --> ?", End, Start);
            else 
                return string.Format("[{0} --> {1}]", Start, End);
        }


        #region Equality and comparison(平等与比较)
        public int CompareTo(object _other)
        {
            if (!(_other is VideoSection))
                throw new ArgumentException();

            // To be "less than", a section must be entirely enclosed in another and smaller.
            // 要成为“较小的部分”，该部分必须完全被另一个更小的部分所包围。

            VideoSection other = (VideoSection)_other;
            
            if(this.Start == other.Start && this.End == other.End) 
                return 0;
            else if(this.Start >= other.Start && this.End <= other.End)
                return -1;
            else
                return 1;
        }
        public static bool operator < (VideoSection v1, VideoSection v2)
        {
            return v1.CompareTo(v2) < 0;
        }
        public static bool operator > (VideoSection v1, VideoSection v2)
        {
            return v1.CompareTo(v2) > 0;
        }
        public override bool Equals(object obj)
        {
            if(!(obj is VideoSection))
                return false;
            return Equals((VideoSection)obj);
        }
        public bool Equals(VideoSection other)
        {
            return !IsEmpty && Start == other.Start && End == other.End;
        }
        public static bool operator ==(VideoSection v1, VideoSection v2)
        {
            return v1.Equals(v2);
        }
        public static bool operator !=(VideoSection v1, VideoSection v2)
        {
            return !v1.Equals(v2);
        }
        public override int GetHashCode()
        {
            return Start.GetHashCode() ^ End.GetHashCode();
        }
        #endregion
    }
}
