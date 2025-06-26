namespace Kinovea.VideoService.Models
{
    /// <summary>
    /// ��ƵԪ����
    /// </summary>
    public class VideoMetadata
    {
        /// <summary>
        /// ��Ƶʱ��
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// ֡��
        /// </summary>
        public double FrameRate { get; set; }

        /// <summary>
        /// ��Ƶ���
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// ��Ƶ�߶�
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// ��Ƶ�����ʽ
        /// </summary>
        public string Codec { get; set; }

        /// <summary>
        /// ��Ƶ������ʽ
        /// </summary>
        public string Container { get; set; }
    }
}