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

namespace Kinovea.Video
{
    /// <summary>
    /// Flags indicating the capabilities of the specific file loaded by the reader.
    /// </summary>
    [Flags]
    public enum VideoCapabilities : int
    {
        None                    = 0,
        CanDecodeOnDemand       = 1,
        CanPreBuffer            = 2,
        CanCache                = 4,
        CanChangeWorkingZone    = 8,
        CanChangeAspectRatio    = 16,
        CanChangeDeinterlacing  = 32,
        CanChangeVideoDuration  = 64,
        CanChangeFrameRate      = 128,
        CanChangeDecodingSize   = 256,
        CanScaleIndefinitely    = 512,
        CanChangeImageRotation  = 1024,
        CanChangeDemosaicing    = 2048,
        CanStabilize            = 4096,
    }
    
    /// <summary>
    /// The current decoding mode the video reader is in.
    /// </summary>
    public enum VideoDecodingMode
    {
        NotInitialized, // The video is just opening or has closed and the reader is not fully initialized.
        OnDemand,       // each frame is decoded on the fly when the player needs it.
        PreBuffering,   // frames are decoded in a separate thread and pushed to a small buffer.
        Caching         // All the frames of the working zone have been loaded to a large buffer.
    }

    public enum OpenVideoResult
    {
        Success,
        UnknownError,
        NotSupported,
        FileNotOpenned,
        StreamInfoNotFound,
        VideoStreamNotFound,
        CodecNotFound,
        CodecNotOpened,
        CodecNotSupported,
        Cancelled,
        EmptyWatcher,
    }
    
    public enum SaveResult
    {
        Success,
        MuxerNotFound,
        MuxerParametersNotAllocated,
        MuxerParametersNotSet,
        VideoStreamNotCreated,
        EncoderNotFound,
        EncoderParametersNotAllocated,
        EncoderParametersNotSet,
        EncoderNotOpened,
        FileNotOpened,
        FileHeaderNotWritten,
        InputFrameNotAllocated,
        MetadataStreamNotCreated,
        MetadataNotWritten,
        ReadingError,
        UnknownError,
        MovieNotLoaded,
        TranscodeNotFinished,
        Cancelled
    }
}
