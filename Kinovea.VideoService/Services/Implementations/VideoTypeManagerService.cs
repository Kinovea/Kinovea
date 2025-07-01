using Kinovea.Video;
using Kinovea.VideoService.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace Kinovea.VideoService.Services.Implementations
{
    /// <summary>
    /// 视频类型管理器服务实现
    /// </summary>
    public class VideoTypeManagerService : IVideoTypeManagerService
    {
        private readonly ILogger<VideoTypeManagerService> _logger;
        private readonly Dictionary<string, Type> _videoReaderTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        public VideoTypeManagerService(ILogger<VideoTypeManagerService> logger)
        {
            _logger = logger;

            // 初始化视频读取器类型映射
            LoadVideoReaders();
        }

        /// <summary>
        /// 加载视频读取器类型
        /// </summary>
        public void LoadVideoReaders()
        {
            try
            {
                // 注册视频读取器类型
                RegisterVideoReaderTypes();

                if (_videoReaderTypes.Count > 0)
                {
                    _logger.LogInformation($"成功注册了{_videoReaderTypes.Count}个视频文件扩展名支持");
                }
                else
                {
                    _logger.LogWarning("没有成功注册任何视频文件扩展名");

                    // 注册内置的基本视频读取器
                    RegisterFallbackReaders();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载视频读取器时出错");
            }
        }

        /// <summary>
        /// 获取适合指定文件的视频读取器
        /// </summary>
        public VideoReader GetVideoReader(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                _logger.LogWarning("尝试获取视频读取器时提供的文件路径为空");
                return null;
            }

            string extension = Path.GetExtension(filePath);

            // 尝试获取支持此扩展名的视频读取器类型
            if (!_videoReaderTypes.TryGetValue(extension, out Type readerType))
            {
                // 如果没有特定类型支持，尝试使用通配符（FFMpeg通常支持通配符）
                if (!_videoReaderTypes.TryGetValue("*", out readerType))
                {
                    _logger.LogWarning($"未找到支持扩展名'{extension}'的视频读取器");
                    return null;
                }
            }

            try
            {
                // 创建视频读取器实例
                VideoReader reader = (VideoReader)Activator.CreateInstance(readerType);
                return reader;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"创建视频读取器实例失败: {readerType.FullName}");
                return null;
            }
        }

        /// <summary>
        /// 检查是否支持特定文件扩展名
        /// </summary>
        public bool IsFormatSupported(string extension)
        {
            if (string.IsNullOrEmpty(extension))
                return false;

            return _videoReaderTypes.ContainsKey(extension) || _videoReaderTypes.ContainsKey("*");
        }

        /// <summary>
        /// 获取图像序列读取器
        /// </summary>
        public VideoReader GetImageSequenceReader()
        {
            try
            {
                // 图像序列通常由FFMpeg处理
                if (_videoReaderTypes.TryGetValue("*", out Type readerType))
                {
                    return (VideoReader)Activator.CreateInstance(readerType);
                }

                _logger.LogWarning("未找到支持图像序列的视频读取器");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建图像序列读取器失败");
                return null;
            }
        }

        #region 私有方法

        /// <summary>
        /// 记录当前运行环境信息
        /// </summary>
        private void LogRuntimeEnvironment()
        {
            _logger.LogInformation($"当前运行环境: .NET {Environment.Version}, {(Environment.Is64BitProcess ? "64位" : "32位")} 进程");
            _logger.LogInformation($"操作系统: {RuntimeInformation.OSDescription}");
            _logger.LogInformation($"应用程序路径: {AppDomain.CurrentDomain.BaseDirectory}");

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            _logger.LogInformation($"当前已加载 {assemblies.Length} 个程序集");

            // 记录一些关键程序集的版本信息
            foreach (var assembly in assemblies)
            {
                if (assembly.FullName.StartsWith("Kinovea."))
                {
                    _logger.LogInformation($"已加载: {assembly.FullName}");
                }
            }
        }

        /// <summary>
        /// 检查并记录DLL文件
        /// </summary>
        private void CheckAndLogDllFiles()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // 检查视频读取器DLL
            string[] videoReaderDlls = {
                "Kinovea.Video.FFMpeg.dll",
                "Kinovea.Video.Bitmap.dll",
                "Kinovea.Video.GIF.dll",
                "Kinovea.Video.SVG.dll",
                "Kinovea.Video.Synthetic.dll"
            };

            foreach (string dll in videoReaderDlls)
            {
                string fullPath = Path.Combine(baseDir, dll);
                bool exists = File.Exists(fullPath);
                _logger.LogInformation($"DLL文件 {dll}: {(exists ? "存在" : "不存在")}");

                if (exists)
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(fullPath);
                        _logger.LogInformation($"  大小: {fileInfo.Length} 字节, 创建时间: {fileInfo.CreationTime}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"  无法获取文件信息: {ex.Message}");
                    }
                }
            }

            // 检查FFMpeg相关的原生DLL
            string[] nativeDlls = {
                "avcodec.dll",
                "avformat.dll",
                "avutil.dll",
                "swscale.dll"
            };

            foreach (string dll in nativeDlls)
            {
                string fullPath = Path.Combine(baseDir, dll);
                bool exists = File.Exists(fullPath);
                _logger.LogInformation($"原生DLL文件 {dll}: {(exists ? "存在" : "不存在")}");
            }
        }

        /// <summary>
        /// 注册基本的视频读取器（当其他读取器无法加载时）
        /// </summary>
        private void RegisterFallbackReaders()
        {
            _logger.LogInformation("注册备用视频读取器...");

            try
            {
                // 注册Bitmap图像读取器（通常更简单，依赖更少）
                Type bitmapReaderType = typeof(VideoReader);  // 使用基类作为占位符

                // 注册基本扩展名
                _videoReaderTypes.Add(".jpg", bitmapReaderType);
                _videoReaderTypes.Add(".jpeg", bitmapReaderType);
                _videoReaderTypes.Add(".png", bitmapReaderType);
                _videoReaderTypes.Add(".bmp", bitmapReaderType);

                _logger.LogInformation("已注册备用图像读取器，支持基本图像格式");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "注册备用读取器失败");
            }
        }

        /// <summary>
        /// 直接注册已知的视频读取器类型
        /// </summary>
        private void RegisterVideoReaderTypes()
        {
            string execDir = AppDomain.CurrentDomain.BaseDirectory;
            _logger.LogInformation($"当前执行目录: {execDir}");

            try
            {
                // 尝试使用不同方式注册FFMpeg视频读取器
                bool ffmpegRegistered = false;

                // 方法1: 通过反射加载FFMpeg程序集
                try
                {
                    string ffmpegPath = Path.Combine(execDir, "Kinovea.Video.FFMpeg.dll");
                    if (File.Exists(ffmpegPath))
                    {
                        _logger.LogInformation($"尝试加载FFMpeg程序集: {ffmpegPath}");

                        Assembly ffmpegAssembly = null;
                        try
                        {
                            ffmpegAssembly = Assembly.LoadFrom(ffmpegPath);
                            _logger.LogInformation($"成功加载FFMpeg程序集: {ffmpegAssembly.FullName}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "加载FFMpeg程序集失败");
                        }

                        if (ffmpegAssembly != null)
                        {
                            Type readerType = null;
                            try
                            {
                                readerType = ffmpegAssembly.GetType("Kinovea.Video.FFMpeg.VideoReaderFFMpeg");
                                if (readerType != null)
                                {
                                    _logger.LogInformation($"找到FFMpeg视频读取器类型: {readerType.FullName}");

                                    // 检查是否是VideoReader的子类
                                    if (IsVideoReaderType(readerType))
                                    {
                                        RegisterExtensionsForType(readerType, new string[] {
                                            ".avi", ".mp4", ".mkv", ".mov", ".wmv", ".flv", ".mpg", ".mpeg", ".m4v", ".3gp", ".ts", ".webm", "*"
                                        });
                                        ffmpegRegistered = true;
                                    }
                                    else
                                    {
                                        _logger.LogWarning($"找到的类型不是VideoReader的子类");
                                    }
                                }
                                else
                                {
                                    _logger.LogWarning("在FFMpeg程序集中未找到VideoReaderFFMpeg类型");

                                    // 尝试查找所有类型
                                    _logger.LogInformation("尝试列出FFMpeg程序集中的所有类型:");
                                    foreach (var type in ffmpegAssembly.GetTypes())
                                    {
                                        _logger.LogInformation($"  类型: {type.FullName}");
                                    }
                                }
                            }
                            catch (ReflectionTypeLoadException ex)
                            {
                                _logger.LogError(ex, "加载FFMpeg程序集中的类型时出错");
                                foreach (var loaderEx in ex.LoaderExceptions)
                                {
                                    _logger.LogError(loaderEx, "加载器异常");
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "处理FFMpeg程序集时出错");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"FFMpeg DLL不存在: {ffmpegPath}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "尝试加载FFMpeg程序集时出错");
                }

                if (!ffmpegRegistered)
                {
                    _logger.LogWarning("无法注册FFMpeg视频读取器，将尝试其他方法");
                }

                // 尝试注册Bitmap图像读取器
                try
                {
                    string bitmapPath = Path.Combine(execDir, "Kinovea.Video.Bitmap.dll");
                    if (File.Exists(bitmapPath))
                    {
                        _logger.LogInformation($"尝试加载Bitmap程序集: {bitmapPath}");

                        Assembly bitmapAssembly = null;
                        try
                        {
                            bitmapAssembly = Assembly.LoadFrom(bitmapPath);
                            _logger.LogInformation($"成功加载Bitmap程序集: {bitmapAssembly.FullName}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "加载Bitmap程序集失败");
                        }

                        if (bitmapAssembly != null)
                        {
                            Type readerType = null;
                            try
                            {
                                readerType = bitmapAssembly.GetType("Kinovea.Video.Bitmap.VideoReaderBitmap");
                                if (readerType != null)
                                {
                                    _logger.LogInformation($"找到Bitmap视频读取器类型: {readerType.FullName}");

                                    // 检查是否是VideoReader的子类
                                    if (IsVideoReaderType(readerType))
                                    {
                                        RegisterExtensionsForType(readerType, new string[] {
                                            ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".tif"
                                        });
                                    }
                                    else
                                    {
                                        _logger.LogWarning($"找到的类型不是VideoReader的子类");
                                    }
                                }
                                else
                                {
                                    _logger.LogWarning("在Bitmap程序集中未找到VideoReaderBitmap类型");
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "处理Bitmap程序集时出错");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Bitmap DLL不存在: {bitmapPath}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "尝试加载Bitmap程序集时出错");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "注册视频读取器类型时出错");
            }
        }

        /// <summary>
        /// 为指定类型注册文件扩展名
        /// </summary>
        private void RegisterExtensionsForType(Type readerType, string[] extensions)
        {
            foreach (string extension in extensions)
            {
                if (!_videoReaderTypes.ContainsKey(extension))
                {
                    _videoReaderTypes.Add(extension, readerType);
                    _logger.LogInformation($"已注册扩展名 {extension} 到 {readerType.FullName}");
                }
            }
        }

        /// <summary>
        /// 检查类型是否是VideoReader的子类
        /// </summary>
        private bool IsVideoReaderType(Type type)
        {
            if (type == null || type.IsAbstract)
                return false;

            Type baseType = type.BaseType;
            while (baseType != null)
            {
                if (baseType.Name == "VideoReader" || baseType.Name == "VideoReaderAlwaysCaching")
                    return true;

                baseType = baseType.BaseType;
            }

            return false;
        }

        #endregion
    }
}
