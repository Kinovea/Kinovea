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

namespace Kinovea { namespace Video { namespace FFMpeg
{
	public enum class ReadResult
	{
		Success,

        /// A seek operation would have landed in the same GOP as the current one.
        /// This is used by summary extraction to avoid decoding the
        /// same thumbnail twice.
        Same,

        /// End of file.
        EOFReached,

        /// A newer player state has been detected.
        /// Current workload has been abandoned.
        NewJob,

        ThreadCancelled,

        NotReady,
		NotConverted,
        UnknownError,
	};
}}}
