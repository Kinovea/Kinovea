using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Services
{
    public enum FileExplorerCommands
    {
        RenameSelected,
        LaunchSelected,
        DeleteSelected
    }

    public enum ThumbnailViewerFilesCommands
    {
        RenameSelected,
        LaunchSelected,
        DeleteSelected,
        Refresh
    }

    public enum ThumbnailViewerCameraCommands
    {
        RenameSelected,
        LaunchSelected,
        Refresh
    }

    public enum ThumbnailViewerContainerCommands
    {
        IncreaseSize,
        DecreaseSize
    }

    public enum DualPlayerCommands
    {
        TogglePlay,

        GotoPreviousImage,
        GotoFirstImage,
        GotoPreviousKeyframe,

        GotoNextImage,
        GotoLastImage,
        GotoNextKeyframe,
        
        GotoSyncPoint,
        AddKeyframe
    }

    public enum PlayerScreenCommands
    {
        TogglePlay,
        ResetViewport,
        
        GotoPreviousImage,
        GotoPreviousImageForceLoop,
        GotoFirstImage,
        GotoPreviousKeyframe,
        BackwardRound10Percent,
        BackwardRound1Percent,

        GotoNextImage,
        GotoLastImage,
        GotoNextKeyframe,
        ForwardRound10Percent,
        ForwardRound1Percent,

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
        IncreaseSpeedRoundTo10,
        IncreaseSpeedRoundTo25,
        DecreaseSpeed1,
        DecreaseSpeedRoundTo10,
        DecreaseSpeedRoundTo25,

        Close
    }

    public enum CaptureScreenCommands
    {
        ToggleGrabbing,
        ToggleRecording,
        TakeSnapshot,
        ResetViewport,
        OpenConfiguration,
        IncreaseZoom,
        DecreaseZoom,
        ResetZoom,
        IncreaseDelay,
        DecreaseDelay,
        Close
    }

}
