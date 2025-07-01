using System.Drawing;

namespace Kinovea.VideoService.Models
{
    /// <summary>
    /// ��Ƶ֡ģ��
    /// </summary>
    public class VideoFrame
    {
        /// <summary>
        /// ֡ͼ��
        /// </summary>
        public Bitmap Image { get; set; }

        /// <summary>
        /// ʱ���(����)
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// ֡����
        /// </summary>
        public int FrameIndex { get; set; }

        /// <summary>
        /// �Ƿ�Ϊ�ؼ�֡
        /// </summary>
        public bool IsKeyFrame { get; set; }
    }
}