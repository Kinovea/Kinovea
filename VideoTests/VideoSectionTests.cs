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
using System.Collections.Generic;
using NUnit.Framework;
using Kinovea.Video;

namespace Kinovea.Video.Tests
{
    [TestFixture]
    public class VideoSectionTests
    {
        [TestCase(-1, -1, Result=true)]
        [TestCase(5, -1, Result=true)]
        [TestCase(-1, 5, Result=true)]
        [TestCase(0, 0, Result=false)]
        [TestCase(5, 15, Result=false)]
        [TestCase(15, 5, Result=false)]
        public bool EmptynessTest(long _start, long _end)
        {
            VideoSection section = new VideoSection(_start, _end);
            return section.IsEmpty;
        }
        
        [TestCase(-1, -1, Result=false)]
        [TestCase(5, -1, Result=false)]
        [TestCase(-1, 5, Result=false)]
        [TestCase(0, 0, Result=false)]
        [TestCase(5, 15, Result=false)]
        [TestCase(15, 5, Result=true)]
        public bool WrappedTest(long _start, long _end)
        {
            VideoSection section = new VideoSection(_start, _end);
            return section.Wrapped;
        }
        
        [TestCase(-1, -1, 5, Result=false)]
        [TestCase(5, -1, 3, Result=false)]
        [TestCase(-1, 5, 3, Result=false)]
        [TestCase(0, 0, 0, Result=true)]
        [TestCase(0, 0, 5, Result=false)]
        [TestCase(5, 15, 10, Result=true)]
        [TestCase(5, 15, 20, Result=false)]
        [TestCase(15, 5, 10, Result=false)]
        [TestCase(15, 5, 20, Result=false)]
        public bool ContainsValueTest(long _start, long _end, long _value)
        {
            VideoSection section = new VideoSection(_start, _end);
            return section.Contains(_value);
        }
        
        [TestCase(-1, -1, -1, -1, Result=false)]
        [TestCase(5, 15, -1, -1, Result=false)]
        [TestCase(5, 15, 0, 10, Result=false)]
        [TestCase(5, 15, 5, 10, Result=true)]
        [TestCase(5, 15, 8, 12, Result=true)]
        [TestCase(5, 15, 10, 15, Result=true)]
        [TestCase(5, 15, 10, 20, Result=false)]
        [TestCase(5, 15, 20, 25, Result=false)]
        public bool ContainsSectionTest(long _start, long _end, long _start2, long _end2)
        {
            VideoSection section = new VideoSection(_start, _end);
            VideoSection section2 = new VideoSection(_start2, _end2);
            return section.Contains(section2);
        }
        
        [TestCase(-1, -1, -1, -1, Result=false)]
        [TestCase(5, 15, -1, -1, Result=false)]
        [TestCase(5, 15, 0, 10, Result=false)]
        [TestCase(5, 15, 5, 10, Result=false)]
        [TestCase(5, 15, 8, 12, Result=false)]
        [TestCase(5, 15, 10, 15, Result=false)]
        [TestCase(5, 15, 10, 25, Result=false)]
        [TestCase(5, 15, 20, 25, Result=false)]
        [TestCase(5, 15, 0, 15, Result=true)]
        [TestCase(5, 15, 0, 20, Result=true)]
        [TestCase(5, 15, 5, 20, Result=true)]
        public bool OperatorLessThanTest(long _start, long _end, long _start2, long _end2)
        {
            VideoSection section = new VideoSection(_start, _end);
            VideoSection section2 = new VideoSection(_start2, _end2);
            return section < section2;
        }
        
        [TestCase(-1, -1, -1, -1, Result=false)]
        [TestCase(5, 15, -1, -1, Result=false)]
        [TestCase(5, 15, 5, 15, Result=true)]
        [TestCase(5, 15, 5, 10, Result=false)]
        public bool OperatorEqualsTest(long _start, long _end, long _start2, long _end2)
        {
            VideoSection section = new VideoSection(_start, _end);
            VideoSection section2 = new VideoSection(_start2, _end2);
            return section == section2;
        }
    }
}