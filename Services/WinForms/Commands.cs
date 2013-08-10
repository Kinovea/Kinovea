using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Services
{
    public enum ThumbnailViewerFilesCommands
    {
        Rename,
        Launch,
        Delete
    }

    public enum ThumbnailViewerContainerCommands
    {
        IncreaseSize,
        DecreaseSize
    }

    public enum PlayerScreenCommands
    {
        TogglePlay,
        ResetView,
        
        GotoPreviousImage,
        GotoPreviousImageForceLoop,
        GotoFirstImage,
        GotoPreviousKeyframe, 

        GotoNextImage,
        GotoLastImage,
        GotoNextKeyframe,

        IncreaseZoom,
        DecreaseZoom,
        ResetZoom,

        IncreaseSyncAlpha,
        DecreaseSyncAlpha,

        AddKeyframe,
        DeleteKeyframe,
        DeleteDrawing,

        IncreaseSpeed1,
        IncreaseSpeedRound10,
        IncreaseSpeedRound25,
        DecreaseSpeed1,
        DecreaseSpeedRound10,
        DecreaseSpeedRound25,

        Close
    }

}
