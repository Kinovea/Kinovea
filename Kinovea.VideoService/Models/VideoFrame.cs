namespace Kinovea.VideoService.Models
{
    /// <summary>
    /// ��Ƶ֡ģ��
    /// </summary>
    public class VideoFrame
    {
        /// <summary>
        /// ֡����
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// ʱ���
        /// </summary>
        public TimeSpan Timestamp { get; set; }

        /// <summary>
        /// ֡����
        /// </summary>
        public long Index { get; set; }

        /// <summary>
        /// ͼ���ʽ
        /// </summary>
        public string Format { get; set; }
    }
}