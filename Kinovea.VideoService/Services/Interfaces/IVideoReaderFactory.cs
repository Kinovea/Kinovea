namespace Kinovea.VideoService.Services.Interfaces
{
    /// <summary>
    /// ��Ƶ��ȡ�������ӿ�
    /// </summary>
    public interface IVideoReaderFactory
    {
        /// <summary>
        /// �����ʺ�ָ����Ƶ��ʽ�Ķ�ȡ��
        /// </summary>
        /// <param name="format">��Ƶ��ʽ</param>
        IVideoReader CreateReader(string format);

        /// <summary>
        /// ��ȡ֧�ֵ���Ƶ��ʽ�б�
        /// </summary>
        IEnumerable<string> GetSupportedFormats();
    }
}