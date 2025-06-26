using Kinovea.Video;
using Kinovea.VideoService.Models;

namespace Kinovea.VideoService.Services.Interfaces
{
    /// <summary>
    /// ��Ƶ��ȡ���ӿ�
    /// </summary>
    public interface IVideoReader : IDisposable
    {
        /// <summary>
        /// ��Ƶ��������
        /// </summary>
        VideoCapabilities Capabilities { get; }

        /// <summary>
        /// ����Ƶ�ļ�
        /// </summary>
        Task<OpenVideoResult> OpenAsync(string filePath);

        /// <summary>
        /// ��ȡָ��ʱ������Ƶ֡
        /// </summary>
        Task<Kinovea.VideoService.Models.VideoFrame> GetFrameAsync(TimeSpan position);

        /// <summary>
        /// ��ȡ��ƵԪ����
        /// </summary>
        Task<VideoMetadata> GetMetadataAsync();

        /// <summary>
        /// �ر���Ƶ
        /// </summary>
        Task CloseAsync();
    }
}