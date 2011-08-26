#region License
/*
Copyright © Joan Charmant 2011.
joan.charmant@gmail.com 
 
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

namespace Kinovea.Video
{
    /// <summary>
    /// A section of the video, defined by sentinel timestamps.
    /// Can be used to describe the whole video, the working zone, the cache segment or any other section.
    /// Note that the [0;0] section is valid. It describe a 1-frame segment of timestamp 0.
    /// Negative timestamps are invalid and used to denote uninitialized section.
    /// </summary>
    public struct VideoSection : IComparable, IEquatable<VideoSection>
    {
        public static VideoSection Empty {
            get { return new VideoSection(-1,-1); }
        }
        public bool Wrapped {
            get { return End < Start;}
        }
        public readonly long Start;
        public readonly long End;
        
        public VideoSection(long _start, long _end)
        {
            Start = _start;
            End = _end;
        }
        public bool Contains(long _timestamp)
        {
            return _timestamp >= Start && _timestamp <= End;
        }
        public override string ToString()
        {
            if(Wrapped)
                return string.Format("? --> {0}] [{1} --> ?", End, Start);
            else 
                return string.Format("[{0} --> {1}]", Start, End);
        }
        
        #region Equality and comparison
        public int CompareTo(object _other)
        {
            // Comparison is done on duration.
            if (_other is VideoSection)
            {
                VideoSection other = (VideoSection)_other;
                
                if(this.Start == other.Start && this.End == other.End) 
                    return 0;
                else if(this.Start >= other.Start && this.End <= other.End)
                    return -1;
                else
                    return 1;
            }
            else 
            {
                throw new ArgumentException();
            }
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
            return Start == other.Start && End == other.End;
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
