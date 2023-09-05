using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Services
{
    public enum FileExplorerCommands
    {
        LaunchSelected,
        RenameSelected,
        DeleteSelected
    }

    public enum ThumbnailViewerFilesCommands
    {
        LaunchSelected,
        RenameSelected,
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
        ToggleSyncMerge,
        AddKeyframe
    }

    public enum PlayerScreenCommands
    {
        // General
        ResetViewport,
        Close,

        // Playback control
        TogglePlay,
        IncreaseSpeed1,
        IncreaseSpeedRoundTo10,
        IncreaseSpeedRoundTo25,
        DecreaseSpeed1,
        DecreaseSpeedRoundTo10,
        DecreaseSpeedRoundTo25,

        // Frame by frame navigation
        GotoPreviousImage,
        GotoNextImage,
        GotoFirstImage,
        GotoLastImage,
        GotoPreviousImageForceLoop,
        BackwardRound10Percent,
        ForwardRound10Percent,
        BackwardRound1Percent,
        ForwardRound1Percent,
        GotoPreviousKeyframe,
        GotoNextKeyframe,
        GotoSyncPoint,
        
        // Synchronization
        IncreaseSyncAlpha,
        DecreaseSyncAlpha,
        ToggleSyncMerge,

        // Zoom
        IncreaseZoom,
        DecreaseZoom,
        ResetZoom,

        // Keyframes
        AddKeyframe,
        DeleteKeyframe,
        Preset1,
        Preset2,
        Preset3,
        Preset4,
        Preset5,
        Preset6,
        Preset7,
        Preset8,
        Preset9,
        Preset10,

        // Annotations
        CutDrawing,
        CopyDrawing,
        PasteDrawing,
        PasteInPlaceDrawing,
        DeleteDrawing,
        ValidateDrawing,
        CopyImage,
        ToggleDrawingsVisibility,
        ChronometerStartStop,
        ChronometerSplit,
    }

    public enum DualCaptureCommands
    {
        ToggleGrabbing,
        ToggleRecording,
        TakeSnapshot
    }

    public enum CaptureScreenCommands
    {
        // General
        ResetViewport,
        OpenConfiguration,
        Close,

        // Grabbing & recording
        ToggleGrabbing,
        ToggleRecording,
        TakeSnapshot,
        ToggleArmCaptureTrigger,

        // Zoom
        IncreaseZoom,
        DecreaseZoom,
        ResetZoom,

        // Frame by frame navigation
        GotoPreviousImage,
        GotoNextImage,
        GotoFirstImage,
        GotoLastImage,
        BackwardRound10Percent,
        ForwardRound10Percent,
        BackwardRound1Percent,
        ForwardRound1Percent,

        // Delay
        ToggleDelayedDisplay,
        IncreaseDelayOneSecond,
        DecreaseDelayOneSecond,
        IncreaseDelayOneFrame,
        DecreaseDelayOneFrame,
        IncreaseDelayHalfSecond,
        DecreaseDelayHalfSecond,
    }

}
