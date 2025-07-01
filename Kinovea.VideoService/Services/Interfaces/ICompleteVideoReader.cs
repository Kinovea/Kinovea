namespace Kinovea.VideoService.Services.Interfaces
{
    /// <summary>
    /// 完整的视频读取器接口 - 组合所有子接口
    /// </summary>
    public interface ICompleteVideoReader :
        IVideoReader,
        IVideoFileOperations,
        IVideoPlayback,
        IVideoExport,
        IVideoProcessingOptions,
        IVideoFilterManagement,
        IKeyframeManagement,
        IVideoMetadataManagement,
        IVideoWorkingZoneManagement
    {
        // 组合接口，无需额外方法
    }
}
