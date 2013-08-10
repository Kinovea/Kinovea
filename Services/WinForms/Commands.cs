using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Services
{
    public enum FileExplorerCommands
    {
        Rename,
        Launch,
        Delete
    }

    public enum ThumbnailViewerFilesCommands
    {
        Rename,
        Launch,
        Delete
    }

    public enum ThumbnailViewerCameraCommands
    {
        Rename,
        Launch,
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

        GotoSyncPoint,

        IncreaseZoom,
        DecreaseZoom,
        ResetZoom,

        IncreaseSyncAlpha,
        DecreaseSyncAlpha,

        AddKeyframe,
        DeleteKeyframe,
        DeleteDrawing,
        CopyImage,

        IncreaseSpeed1,
        IncreaseSpeedRound10,
        IncreaseSpeedRound25,
        DecreaseSpeed1,
        DecreaseSpeedRound10,
        DecreaseSpeedRound25,

        Close
    }

    public enum CaptureScreenCommands
    {
        ToggleGrabbing,
        ToggleRecording,
        ResetView,
        OpenConfiguration,
        IncreaseZoom,
        DecreaseZoom,
        ResetZoom,
        IncreaseDelay,
        DecreaseDelay,
        Close
    }

}
