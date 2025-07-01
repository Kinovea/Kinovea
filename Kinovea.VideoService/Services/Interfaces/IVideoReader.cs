using Kinovea.Services;
using Kinovea.Video;
using Kinovea.Video.FFMpeg;
using Kinovea.VideoService.Models;
using System.ComponentModel;
using System.Drawing;
using Kinovea.ScreenManager;

namespace Kinovea.VideoService.Services.Interfaces
{
    /// <summary>
    /// ��Ƶ��ȡ���ӿ�
    /// </summary>
    public interface IVideoReader : IDisposable
    {
        /// <summary>
        /// ��Ƶ��ȡ���ӿ�
        /// </summary>
        public interface IVideoReader : IDisposable
        {

            #region ��Ƶ��ȡ����

            /// <summary>
            /// ����Ƶ�ļ�
            /// </summary>
            /// <param name="filePath">��Ƶ�ļ�·��</param>
            /// <returns>�Ƿ�ɹ���</returns>
            OpenVideoResult Open(string filePath);

            /// <summary>
            /// �ر���Ƶ
            /// </summary>
            void CloseVideo(VideoReader reader);
            /// <summary>
            /// ��ȡ��ƵժҪ����ȡ��Ƶ�Ļ�����Ϣ������ͼ��
            /// </summary>
            /// <param name="filePath">��Ƶ�ļ�·��</param>
            /// <param name="thumbsToLoad">��Ҫ���ص�����ͼ����</param>
            /// <param name="maxImageSize">����ͼ�����ߴ�</param>
            /// <returns>��ƵժҪ����</returns>

            VideoSummary ExtractVideoSummary(string filePath, int thumbsToLoad, Size maxImageSize);

            #endregion

            #region ��Ƶ���Ź���
            /// <summary>
            /// ��˳�򲥷���Ƶ��ÿһ֡ , �ƶ���ָ��֡
            /// </summary>
            /// <param name="reader">��Ƶ</param>
            /// <param name="skip">������֡��</param>
            /// <param name="decodeIfNecessary">�Ƿ����</param>
            /// <returns>�Ƿ�ɹ��ƶ�</returns>
            bool MoveToNextFrame(VideoReader reader, int skip, bool decodeIfNecessary);

            /// <summary>
            /// ��ת��ָ��֡��ֱ�Ӷ�λ����Ƶ��ָ��֡��
            /// </summary>
            /// <param name="reader">��Ƶ��ȡ��</param>
            /// <param name="from">��ʼ֡</param>
            /// <param name="target">Ŀ��֡</param>
            /// <returns>�Ƿ�ɹ��ƶ�</returns>
            bool MoveToSpecificFrame(VideoReader reader, long from, long target);

            #endregion


            #region ��Ƶ��������

            /// <summary>
            /// ������Ƶ������������Ƶ����Ϊָ����ʽ���ļ�
            /// </summary>
            /// <param name="format"></param>
            /// <param name="player1"></param>
            /// <param name="player2"></param>
            /// <param name="dualPlayer"></param>
            void ExportVideo(VideoExportFormat format, PlayerScreen player1, PlayerScreen player2, DualPlayerController dualPlayer);
            #endregion

            #region ��Ƶ����ѡ�����ù���
            /// <summary>
            /// ������Ƶ����ѡ�������ͼ���߱ȡ���ת��ȥ�����ˡ�ȥ���еȡ�
            /// </summary>
            /// <param name="reader"></param>
            /// <param name="options"></param>
            void SetVideoOptions(VideoReader reader, VideoOptions options);
            #endregion

            #region ��Ƶɸѡ��������
            /// <summary>
            /// ������Ƶɸѡ����Ϊ��ƵӦ���ض���ɸѡ����
            /// </summary>
            /// <param name="player"></param>
            /// <param name="type"></param>
            void ActivateVideoFilter(PlayerScreen player, VideoFilterType type);

            /// <summary>
            /// ͣ����Ƶɸѡ�����Ƴ���ǰӦ�õ���Ƶɸѡ����
            /// </summary>
            /// <param name="player"></param>
            void DeactivateVideoFilter(PlayerScreen player);
            #endregion

            #region ��Ƶ�ؼ�֡������
            /// <summary>
            /// ��ӹؼ�֡������Ƶ�б�ǹؼ�֡��
            /// </summary>
            /// <param name="player"></param>
            void AddKeyframe(PlayerScreen player);

            /// <summary>
            /// ��ת����һ���ؼ�֡�����ٶ�λ����һ���ؼ�֡��
            /// </summary>
            /// <param name="player"></param>
            void GotoPreviousKeyframe(PlayerScreen player);

            /// <summary>
            /// ��ת����һ���ؼ�֡�����ٶ�λ����һ���ؼ�֡��
            /// </summary>
            /// <param name="player"></param>
            void GotoNextKeyframe(PlayerScreen player);

            #endregion

            #region ��ƵԪ���ݹ�����
            /// <summary>
            /// ��ȡ��ƵԪ���ݣ���ȡ��Ƶ�����Ԫ������Ϣ��
            /// </summary>
            /// <param name="reader"></param>
            /// <returns></returns>
            string ReadVideoMetadata(VideoReaderFFMpeg reader);
            #endregion

            #region ��Ƶ������������
            /// <summary>
            /// ������Ƶ�����������û������Ƶ�Ĺ���������
            /// </summary>
            /// <param name="reader"></param>
            /// <param name="newZone"></param>
            /// <param name="forceReload"></param>
            /// <param name="maxMemory"></param>
            /// <param name="workerFn"></param>
            void UpdateVideoWorkingZone(VideoReader reader, VideoSection newZone, bool forceReload, int maxMemory, Action<DoWorkEventHandler> workerFn);
            #endregion
        }
    }
}