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

#pragma once
using namespace System;

namespace Kinovea { namespace Video { namespace FFMpeg
{
    // This structure helps determine what is in the FFMpeg internal buffer, 
    // and what is the timestamp of the final frame we are pushing to the Cache.
    public value class TimestampInfo
    {
    public :
        int64_t CurrentTimestamp;
        int64_t BufferedPTS;		// Read but NOT decoded by libav.
        int64_t LastDecodedPTS;		// Last *decoded* frame by libav.

        TimestampInfo(int64_t _current, int64_t _buffered, int64_t _decoded)
        {
            CurrentTimestamp = _current;
            BufferedPTS = _buffered;
            LastDecodedPTS = _decoded;
        }

        static property TimestampInfo Empty {
            TimestampInfo get() { return TimestampInfo(-1, int64_t::MaxValue, -1); }
        }
    };

}}}