using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Kinovea.Services;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    public class WriterFFMpegCLI
    {
        #region Members
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion


        /// <summary>
        /// Saves the passed images to a video, based on the settings.
        /// Runs on the background thread of the worker.
        /// Checks for cancellation and reports progress to the worker.
        /// </summary>
        public SaveResult Save(VideoExportSettings settings, IEnumerable<Bitmap> images, BackgroundWorker worker)
        {
            if (settings == null || images == null || worker == null)
            {
                return SaveResult.UnknownError;
            }

            string ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");
            if (!File.Exists(ffmpegPath))
            {
                log.ErrorFormat("ffmpeg.exe not found: {0}", ffmpegPath);
                return SaveResult.FFMpegNotFound;
            }

            using (IEnumerator<Bitmap> enumerator = images.GetEnumerator())
            {
                bool hasFrame;

                try
                {
                    hasFrame = enumerator.MoveNext();
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error enumerating images for export: {0}", ex);
                    return SaveResult.InputError;
                }

                if (!hasFrame || enumerator.Current == null)
                {
                    log.ErrorFormat("Image enumeration returned no frames.");
                    return SaveResult.InputError;
                }

                // TODO: get output size from user.
                Size inputSize = settings.RenderingSize;
                Size outputSize = settings.RenderingSize;

                string arguments = WriterFFMpegCliHelper.BuildArguments(settings, inputSize, outputSize);

                log.Debug("FFmpeg commandline:");
                log.DebugFormat(arguments);

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true
                };

                using (Process ffmpeg = new Process())
                {
                    ffmpeg.StartInfo = startInfo;
                    return SaveFrames(ffmpeg, settings, enumerator, inputSize, worker);
                }
            }
        }

        private static SaveResult SaveFrames(Process ffmpeg, VideoExportSettings settings, IEnumerator<Bitmap> enumerator, Size inputSize, BackgroundWorker worker)
        {
            bool processStarted = false;
            Task<string> stderrTask = null;

            try
            {
                // This will throw Win32Exception if something is wrong with ffmpeg.exe or the command.
                if (!ffmpeg.Start())
                {
                    log.ErrorFormat("Could not start ffmpeg.exe");
                    return SaveResult.FFMpegNotStarted;
                }

                processStarted = true;

                // Drain stderr concurrently.
                // Otherwise FFmpeg can block when the redirected stderr pipe becomes full.
                stderrTask = ffmpeg.StandardError.ReadToEndAsync();

                bool cancelled = false;
                int frameCount = 0;
                bool hasFrame = true;

                // Staging frame buffer.
                int frameBufferSize = inputSize.Width * inputSize.Height * 3;
                byte[] frameBuffer = new byte[frameBufferSize];

                Stream ffmpegInput = ffmpeg.StandardInput.BaseStream;

                while (hasFrame)
                {
                    if (worker.CancellationPending)
                    {
                        cancelled = true;
                        break;
                    }

                    // Get the bitmap into a tightly packed byte buffer.
                    BitmapHelper.CopyBgr24ToPackedBuffer(enumerator.Current, frameBuffer);

                    // Synchronous write as this method already runs on a worker thread.
                    ffmpegInput.Write(frameBuffer, 0, frameBuffer.Length);

                    frameCount++;
                    worker.ReportProgress(frameCount, settings.TotalFrameCount);

                    hasFrame = enumerator.MoveNext();
                }

                ffmpegInput.Flush();

                // Closing stdin signals EOF.
                // FFmpeg then flushes the encoder and writes the MP4/container trailer.
                ffmpeg.StandardInput.Close();
                ffmpeg.WaitForExit();

                string stderr = GetStandardError(stderrTask);

                if (cancelled)
                {
                    DeleteTemporaryFile(settings.File);
                    return SaveResult.Cancelled;
                }

                if (ffmpeg.ExitCode != 0)
                {
                    log.ErrorFormat("ffmpeg exited with code {0}. Stderr: {1}", ffmpeg.ExitCode, stderr);
                    DeleteTemporaryFile(settings.File);
                    return SaveResult.FFMpegError;
                }

                return SaveResult.Success;
            }
            catch (Win32Exception ex)
            {
                log.ErrorFormat(ex.ToString());
                EnsureProcessTerminated(ffmpeg);
                DeleteTemporaryFile(settings.File);
                return SaveResult.FFMpegError;
            }
            catch (Exception)
            {
                // Terminate before synchronously retrieving stderr.
                // GetStandardError() could block until FFmpeg closes its stderr stream.
                EnsureProcessTerminated(ffmpeg);

                string stderr = GetStandardError(stderrTask);
                log.ErrorFormat(stderr);

                DeleteTemporaryFile(settings.File);
                return SaveResult.FFMpegError;
            }
            finally
            {
                if (processStarted)
                {
                    EnsureProcessTerminated(ffmpeg);
                }
            }
        }

        private static string GetStandardError(Task<string> stderrTask)
        {
            if (stderrTask == null)
            {
                return string.Empty;
            }

            try
            {
                return stderrTask.GetAwaiter().GetResult();
            }
            catch
            {
                return string.Empty;
            }
        }

        private static void EnsureProcessTerminated(Process process)
        {
            if (process == null)
            {
                return;
            }

            try
            {
                // Signal EOF first. This allows ffmpeg to stop naturally.
                process.StandardInput.Close();
            }
            catch
            {
            }

            try
            {
                if (!process.HasExited)
                {
                    process.Kill();
                }
            }
            catch
            {
            }

            try
            {
                process.WaitForExit();
            }
            catch
            {
                // Nothing more can be done.
            }
        }

        private static void DeleteTemporaryFile(string filename)
        {
            log.Debug("Video saving cancelled. Deleting file.");
            if (!File.Exists(filename))
                return;

            try
            {
                File.Delete(filename);
            }
            catch (Exception exp)
            {
                log.Error("Error while deleting file.");
                log.Error(exp.Message);
                log.Error(exp.StackTrace);
            }
        }

    }
}