���Ҹ�����Ŀ�ṹ�������� Kinovea ���������

1. ������Ŀ��

```plaintext
Kinovea (������)
������ Program.cs - Ӧ�ó�����ڵ�
������ RootKernel - ����������
������ FormSplashScreen - ��������
```
������Ӧ�ó���ʵ���������߼������ù�����쳣����

2. ��������Ŀ��

```plaintext
������ĺͳ���㣺
������ Kinovea.Camera/                # ������Ľӿںͻ���
������ Kinovea.Camera.FrameGenerator/ # ��������������ڲ��Ժ�ģ��
������ Kinovea.Camera.GenICam/       # ͨ�ù�ҵ����ӿ�

��׼���֧�֣�
������ Kinovea.Camera.DirectShow/     # Windows DirectShow ���֧��
������ Kinovea.Camera.HTTP/          # ��������ͷ֧��

��ҵ���֧�֣�
������ Kinovea.Camera.Basler/        # Basler��ҵ���
������ Kinovea.Camera.Baumer/        # Baumer��ҵ���
������ Kinovea.Camera.Daheng/        # ��㹤ҵ���
������ Kinovea.Camera.IDS/           # IDS��ҵ���
```

3. ��Ƶ������Ŀ��

```plaintext
��Ƶ���ģ�
������ Kinovea.Video/                # ��Ƶ������Ľӿںͻ���
������ Kinovea.Pipeline/             # ��Ƶ����ܵ�

��Ƶ��ʽ֧�֣�
������ Kinovea.Video.Bitmap/         # λͼ����
������ Kinovea.Video.SVG/           # SVGʸ��ͼ��֧��
������ Kinovea.Video.GIF/           # GIF����֧��
������ Kinovea.Video.FFMpeg/        # FFMpeg��Ƶ�����
������ Kinovea.Video.Synthetic/     # �ϳ���Ƶ����
```

4. ������Ŀ��

```plaintext
Kinovea.CameraService/           # �������
������ Program.cs                    # ������ڵ�
������ appsettings.json             # �����ļ�
������ Dockerfile                   # Docker��������
```

����Ŀ����˵����

1. ���ģ�飺
   - `Kinovea.Camera`: ��������ӿںͻ���ʵ��
   - `Kinovea.Camera.DirectShow`: Windowsϵͳ����ͷ֧��
   - `Kinovea.Camera.HTTP`: ��������ͷ֧��
   - `Kinovea.Camera.FrameGenerator`: ���ڲ��Ե�֡������
   - ��ҵ���֧�֣�Basler��Baumer��Daheng��IDS��Ʒ�������ר��ʵ��

2. ��Ƶ����ģ�飺
   - `Kinovea.Video`: ��Ƶ������Ĺ���
   - `Kinovea.Pipeline`: ��Ƶ����ܵ���������Ƶ��
   - `Kinovea.Video.Bitmap`: λͼ������
   - `Kinovea.Video.SVG`: ʸ��ͼ��֧��
   - `Kinovea.Video.GIF`: GIF����֧��
   - `Kinovea.Video.FFMpeg`: ��Ƶ�����֧��
   - `Kinovea.Video.Synthetic`: �ϳ���Ƶ���ɣ����ڲ���

3. ����ģ�飺
   - `Kinovea.CameraService`: ΪWeb���ṩ��������API

��Ŀ���������ϵ��

```plaintext
Web��
������ Kinovea.CameraService
    ������ Kinovea.Camera
    ��   ������ Kinovea.Camera.DirectShow
    ��   ������ Kinovea.Camera.HTTP
    ��   ������ ���ֹ�ҵ���ʵ��
    ������ Kinovea.Video
        ������ Kinovea.Video.Bitmap
        ������ Kinovea.Video.SVG
        ������ Kinovea.Video.GIF
        ������ Kinovea.Video.Synthetic
```

�����΢�����֣�

1. ���ķ���
```plaintext
CameraService (API Gateway)
������ ������ֺ͹���
������ ��Ƶ������
������ ���ù���
```

2. �������������
```plaintext
CameraAdapterService
������ DirectShow������
������ HTTP���������
������ ��ҵ���������
```

3. ��Ƶ�������
```plaintext
VideoProcessingService
   ������ ��Ƶ�������
   ������ ͼ����
   ������ ��ý�崦��
```

4. �洢����
```plaintext
StorageService
   ������ ��Ƶ�洢
   ������ ͼ��洢
   ������ ���ù���
```

�����ļܹ�����ܹ���
1. ���ַ���Ķ����ԺͿ���չ��
2. ֧�ֲ�ͬ���������������
3. �ṩͳһ��API�ӿ�
4. ����ά��������

������ʵ��΢����ܹ�ʱ��
1. ʹ�ýӿڽ���������
2. ʵ�ַ���ע��ͷ���
3. ����ʵ��ļ�غ���־
4. ʵ�ֹ���ת�ƺ͸��ؾ���
5. ʹ����Ϣ���д����첽����





ͨ��������Ŀ�ṹ�ʹ��룬�ҽ���ϸ���� Kinovea ��������и�����Ŀ�ĺ��Ĺ��ܣ�

1. ��Ƶ����ģ�� (Video Core)��
```plaintext
Kinovea.Video/
������ ���Ĺ��ܣ���Ƶ����Ļ��������
������ ��Ҫ�ӿں�ö�٣�
��   ������ VideoCapabilities - ��Ƶ���ܱ�־
��   ������ VideoDecodingMode - ��Ƶ����ģʽ
��   ������ OpenVideoResult - ��Ƶ�򿪽��
��   ������ SaveResult - ��Ƶ������
```

2. ��Ƶ��ʽ֧��ģ�飺
```plaintext
��Ƶ��������
������ Kinovea.Video.FFMpeg/    # FFmpeg��Ƶ�����֧��
��   ������ PlayerServer/        # ��Ƶ���ŷ�����
������ Kinovea.Video.Bitmap/    # λͼ����
������ Kinovea.Video.SVG/      # SVGʸ��ͼ��֧��
������ Kinovea.Video.GIF/      # GIF����֧��
������ Kinovea.Video.Synthetic/ # �ϳ���Ƶ���ɣ������ã�
```

3. �������ģ�飺
```plaintext
Kinovea.Camera/              # ������ĳ����
������ �����ӿڶ���
������ ���������

�����������
������ Kinovea.Camera.DirectShow/ # Windows DirectShow���
������ Kinovea.Camera.HTTP/       # ��������ͷ
������ Kinovea.Camera.FrameGenerator/ # ������֡������
```

4. ��ҵ���֧�֣�
```plaintext
��ҵ���ģ�飺
������ Kinovea.Camera.GenICam/   # ͨ�ù�ҵ����ӿ�
������ Kinovea.Camera.Basler/    # Basler���֧��
������ Kinovea.Camera.Baumer/    # Baumer���֧��
������ Kinovea.Camera.Daheng/    # ������֧��
������ Kinovea.Camera.IDS/       # IDS���֧��
```

5. ��Ƶ����ܵ���
```plaintext
Kinovea.Pipeline/           # ��Ƶ����ܵ�
������ ��Ƶ������
������ ֡����
������ ��ƵЧ������
```

6. Web����㣺
```plaintext
Kinovea.CameraService/     # ���Web����
������ Program.cs             # �������
������ appsettings.json      # �����ļ�
������ Dockerfile            # ��������

Kinovea.ApiGetway/        # API����
������ REM.md               # ˵���ĵ�
```

���Ĺ���˵����

1. ��Ƶ����������VideoCapabilities����
```csharp
[Flags]
public enum VideoCapabilities
{
    None = 0,                      // �����⹦��
    CanDecodeOnDemand = 1,        // �������
    CanPreBuffer = 2,             // Ԥ����֧��
    CanCache = 4,                 // ����֧��
    CanChangeWorkingZone = 8,     // �ɸ��Ĺ�����
    CanChangeAspectRatio = 16,    // �ɸ��Ŀ�߱�
    CanChangeDeinterlacing = 32,  // �ɸ���ȥ����
    CanChangeVideoDuration = 64,  // �ɸ�����Ƶʱ��
    CanChangeFrameRate = 128,     // �ɸ���֡��
    CanChangeDecodingSize = 256,  // �ɸ��Ľ���ߴ�
    CanScaleIndefinitely = 512,   // ����������
    CanChangeImageRotation = 1024,// ����תͼ��
    CanChangeDemosaicing = 2048,  // �ɸ���ȥ������
    CanStabilize = 4096,         // ���ȶ���
}
```

2. ��Ƶ����ģʽ��
```csharp
public enum VideoDecodingMode
{
    NotInitialized,  // δ��ʼ��
    OnDemand,        // �������
    PreBuffering,    // Ԥ����
    Caching         // ȫ����
}
```

�ܹ��ص㣺

1. ģ�黯��ƣ�
   - ���Ĺ��ܳ������
   - ���ʽ�ܹ�
   - ֧�ֶ�����Ƶ��ʽ���������

2. ��չ�ԣ�
   - ��������µ���Ƶ��ʽ֧��
   - ���ڼ����µ��������
   - ΢����ܹ�֧�ֺ�����չ

3. ����ԣ�
   - ֧�ֶ�����Ƶ����ģʽ
   - �����õ���Ƶ����ܵ�
   - ���Զ���Ĵ�������

4. �����Ż���
   - ֧�ֶ��ֽ���ģʽ
   - �������
   - Ԥ����֧��

�����ʹ�÷�ʽ��

1. ��Ƶ����
```csharp
// ѡ����ʵ���Ƶ������
IVideoReader reader = VideoReaderFactory.CreateReader(videoFormat);
// ���ô�������
if (reader.Capabilities.HasFlag(VideoCapabilities.CanPreBuffer))
{
    reader.DecodingMode = VideoDecodingMode.PreBuffering;
}
```

2. ������ɣ�
```csharp
// �������ʵ��
ICameraManager cameraManager = new CameraManager();
// ������֧��
cameraManager.RegisterCamera(new DirectShowCamera());
cameraManager.RegisterCamera(new HttpCamera());
```

3. ΢������
```yaml
services:
  camera-service:
    image: kinovea-cameraservice
    ports:
      - "5003:5003"
  video-processor:
    image: kinovea-videoprocessor
    depends_on:
      - camera-service
```

���ּܹ����֧�֣�
1. ������Ƶ����
2. ����������ͼ���
3. ����չ��΢������
4. �����ܵ���Ƶ������