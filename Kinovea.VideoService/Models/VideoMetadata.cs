namespace Kinovea.VideoService.Models
{
    /// <summary>
    /// ��ƵԪ����
    /// </summary>
    public class VideoMetadata
    {
        /// <summary>
        /// ��Ƶ�����ʽ
        /// </summary>
        public string Codec { get; set; }

        /// <summary>
        /// ��Ƶ֡��
        /// </summary>
        public double FrameRate { get; set; }

        /// <summary>
        /// ��Ƶʱ��(����)
        /// </summary>
        public long Duration { get; set; }

        /// <summary>
        /// ��Ƶ���
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// ��Ƶ�߶�
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// ��Ƶ��֡��
        /// </summary>
        public long FrameCount { get; set; }

        /// <summary>
        /// ������(bps)
        /// </summary>
        public int BitRate { get; set; }
    }
}