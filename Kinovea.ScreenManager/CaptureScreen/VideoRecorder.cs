#region License
/*
Copyright © Joan Charmant 2011.
joan.charmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;

using Kinovea.Video;
using Kinovea.Video.FFMpeg;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// VideoRecorder - Saves images to a file using the producer-consumer paradigm.
    /// 
    /// The class hosts a queue of frames to save, and process them as fast as possible in its own thread.
    /// Once all frames in the queue are processed, it just waits for the producer to add some more or signal stop.
    /// The producer push frames in the queue and set the signal.
    /// The frames pushed have to be deep copies since we don't know when we will be able to handle them.
    /// To signal the end of recording, we push a null frame in the queue.
    /// Ref: http://www.albahari.com/threading/part2.aspx#_Signaling_with_Event_Wait_Handles
    /// </summary>
    public class VideoRecorder : IDisposable
    {
        #region Properties
        public bool Initialized
        {
            get { return initialized; }
        }
        public Bitmap CaptureThumb
        {
            get { return captureThumb; }
        }
        public bool Cancelling
        {
            get { return cancelling; }
        }
        public SaveResult CancelReason
        {
            get { return cancelReason; }
        }
        public bool OverCapacity
        {
            get { return overCapacity; }
        }
        public string Filepath
        {
            get { return filepath;}
        }
        public string Filename 
        {
            get 
            { 
                if(!string.IsNullOrEmpty(filepath))
                    return Path.GetFileNameWithoutExtension(filepath);
                else 
                    return "";
            }
        }
        #endregion
        
        #region Members
        private EventWaitHandle waitHandle = new AutoResetEvent(false);
        private Thread workerThread;
        private readonly object locker = new object();
        private Queue<Bitmap> frameQueue = new Queue<Bitmap>();
        private VideoFileWriter videoFileWriter = new VideoFileWriter();
        private string filepath;
        private string filename;
        private bool initialized;
        private bool cancelling;
        private SaveResult cancelReason = SaveResult.UnknownError;
        private bool captureThumbSet;
        private Bitmap captureThumb;
        private static readonly int capacity = 10;
        private bool overCapacity;
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Constructor
        public VideoRecorder()
        {
            workerThread = new Thread(Work);
        }
        #endregion
        
        #region Public Methods
        public SaveResult Initialize(string filepath, double interval, Size frameSize)
        {
            // Open the recording context and start the recording thread. 
            // The thread will then wait for the first frame to drop in.
            
            // FIXME: The FileWriter will currently only use the original size due to some problems.
            // Most notably, DV video passed into 16:9 (720x405) crashes swscale().
            // TODO: Check if this is due to non even height.
            VideoInfo vi = new VideoInfo { OriginalSize = frameSize};
            SaveResult result = videoFileWriter.OpenSavingContext(filepath, vi, interval, false);
            filename = Path.GetFileName(filepath);
            
            if(result == SaveResult.Success)
            {
                initialized = true;
                captureThumbSet = false;
                this.filepath = filepath;
                workerThread.Start();
            }
            else
            {
                try
                {
                    videoFileWriter.CloseSavingContext(false);    
                }
                catch (Exception exp)
                {
                    // Saving context couldn't be opened properly. Depending on failure we might also fail at trying to close it again.
                    log.Error(exp.Message);
                    log.Error(exp.StackTrace);  
                }
            }
            
            return result;
        }
        public void Dispose()
        {
            Close();
        }
        public void EnqueueFrame(Bitmap frame)
        {
            // Push a frame and signal the consumer thread that some new stuff is ready to be consummed.
            // FIXME: currently there is no mechanism to limit the size of the queue.
            
            //log.DebugFormat("Enqueing a frame.");
            
            if(cancelling || !initialized)
                return;
            
            lock (locker)
            {
                if(frameQueue.Count < capacity)
                    frameQueue.Enqueue(frame);
                else
                    log.DebugFormat("Recording buffer over capacity for {0}. Frame dropped.", filename);
                
                overCapacity = frameQueue.Count == capacity;
            }
            
            waitHandle.Set();
        }
        public void Close()
        {
            if(cancelling)
                return;
            
            if(workerThread.IsAlive)
            {
                EnqueueFrame(null);     // Signal the consumer to exit.
                workerThread.Join();    // Wait for the consumer's thread to finish.
            }
            
            waitHandle.Close();     // Release any OS resources.
            
            if(initialized)
                videoFileWriter.CloseSavingContext(true);
                
            initialized = false;
        }
        #endregion
        
        #region Private Methods
        private void Work()
        {
            Thread.CurrentThread.Name = "Record";
            
            while(!cancelling)
            {
                Bitmap bmp = null;
                bool finished = Dequeue(ref bmp);
                
                if(finished)
                    return;
                
                if (bmp == null)
                {
                    // Nothing to eat. Wait for the producer to push a new frame and go on.
                    waitHandle.WaitOne();
                    continue;
                }
                
                SaveResult res = videoFileWriter.SaveFrame(bmp);
                bool success = AfterSaveFrame(bmp, res);
                
                if(!success)
                    return;
            }
        }
        private bool Dequeue(ref Bitmap bmp)
        {
            // To indicate the end of recording, the producer will push a null bitmap in the queue.
            // - 0 frames means we are waiting for some more data from the producer.
            // - A null frame means the work is finished.
            
            lock (locker)
            {
                if(frameQueue.Count == 0)
                    return false;
                
                bmp = frameQueue.Dequeue();
                if (bmp == null)
                    return true;
            }
            
            return false;
        }
        private void Cancel(Bitmap bmp, SaveResult saveResult)
        {
            // The producer should test for .Cancelling and stop queuing items at this point.
            // We don't try to save the outstanding frames, but the video file should be valid.
            log.Error("Error while saving frame to file.");
            cancelling = true;
            cancelReason = saveResult;
            
            bmp.Dispose();
            ClearOutstandingFrames();
        
            if(!initialized)
                return;
            
            try
            {
                videoFileWriter.CloseSavingContext(true);
            }
            catch (Exception exp)
            {
                log.Error(exp.Message);
                log.Error(exp.StackTrace);  
            }
        }
        private void ClearOutstandingFrames()
        {
            log.DebugFormat("Clear outstanding frames");
            lock (locker)
            {
                while(frameQueue.Count > 0)
                {
                    Bitmap outstanding = frameQueue.Dequeue();
                    if(outstanding != null)
                        outstanding.Dispose();
                }
            }
        }
        private bool AfterSaveFrame(Bitmap bmp, SaveResult saveResult)
        {
            if(saveResult != SaveResult.Success)
            {
                Cancel(bmp, saveResult);
                waitHandle.Close();
                return false;
            }
            
            if(!captureThumbSet)
            {
                // Should be a deep copy ?
                captureThumb = bmp;
                captureThumbSet = true;
            }
            else
            {
                bmp.Dispose();
            }
            
            return true;
        }
        #endregion
    }
}
