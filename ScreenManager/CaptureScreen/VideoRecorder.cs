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
using System.Threading;

using Kinovea.VideoFiles;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// VideoRecorder - Saves images to a file using the producer-consumer paradigm.
	/// 
	/// The class hosts a queue of frames to save, and process them as fast as possible in its own thread.
	/// Once all frames in the queue are processed, it just waits for the producer to add some more or signal stop.
	/// The producer push frames in the queue and set the signal.
	/// The frames pushed have to be deep copies since we don't know when we will be able to handle them.
	/// Ref: http://www.albahari.com/threading/part2.aspx#_Signaling_with_Event_Wait_Handles
	/// </summary>
	public class VideoRecorder : IDisposable
	{
		#region Properties
		public bool Initialized
		{
			get { return m_bInitialized; }
		}
		public Bitmap CaptureThumb
		{
			get { return m_CaptureThumb; }
		}
		public bool Cancelling
		{
			get { return m_bCancelling; }
		}
		public SaveResult CancelReason
		{
			get { return m_CancelReason; }
		}
		public bool Full
		{
			get { return m_FrameQueue.Count > m_iCapacity; }
		}
		#endregion
		
		#region Members
		private EventWaitHandle m_WaitHandle = new AutoResetEvent(false);
  		private Thread m_WorkerThread;
  		private readonly object m_Locker = new object();
  		private Queue<Bitmap> m_FrameQueue = new Queue<Bitmap>();
  		private VideoFileWriter m_VideoFileWriter = new VideoFileWriter();
  		private bool m_bInitialized;
  		private bool m_bCancelling;
  		private SaveResult m_CancelReason = SaveResult.UnknownError;
  		private bool m_bCaptureThumbSet;
		private Bitmap m_CaptureThumb;
		private static readonly int m_iCapacity = 5;
  		
  		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
 		#endregion
		
 		#region Constructor
 		public VideoRecorder()
		{
			m_WorkerThread = new Thread(Work);
		}
  		#endregion
  		
  		#region Public Methods
  		public SaveResult Initialize(string _filepath, double _interval, Size _FrameSize)
  		{
  			// Open the recording context and start the recording thread. 
  			// The thread will then wait for the first frame to drop in.
			
  			// FIXME: The FileWriter will currently only use the original size due to some problems.
			// Most notably, DV video passed into 16:9 (720x405) crashes swscale().
			// TODO: Check if this is due to non even height.
			InfosVideo iv = new InfosVideo();
			iv.iWidth = _FrameSize.Width;
			iv.iHeight = _FrameSize.Height;
			
			SaveResult result = m_VideoFileWriter.OpenSavingContext(_filepath, iv, _interval, false);
			
			if(result == SaveResult.Success)
			{
				m_bInitialized = true;
				m_bCaptureThumbSet = false;
				m_WorkerThread.Start();
			}
			else
			{
				try
				{
					m_VideoFileWriter.CloseSavingContext(false);	
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
  			if(!m_bCancelling)
  			{
  				EnqueueFrame(null);     // Signal the consumer to exit.
				m_WorkerThread.Join();  // Wait for the consumer's thread to finish.
				m_WaitHandle.Close();   // Release any OS resources.
				
				if(m_bInitialized)
					m_VideoFileWriter.CloseSavingContext(true);
  			}
		}
		public void EnqueueFrame(Bitmap _frame)
		{
			if(!m_bCancelling)
  			{
				lock (m_Locker)
				{
					// TODO: prevent overflowing the queue.
					m_FrameQueue.Enqueue(_frame);
				}
				
				m_WaitHandle.Set();
			}
		}
 		#endregion
  		
 		#region Private Methods
 		private void Work()
		{
			Thread.CurrentThread.Name = "Record";
			
			while(!m_bCancelling)
			{
			  	Bitmap bmp = null;
			  	lock (m_Locker)
			  	{
			  		if(m_FrameQueue.Count > 0)
			    	{
			      		bmp = m_FrameQueue.Dequeue();
			      		if (bmp == null)
			      		{
			      			log.Debug("Recording thread finished.");
			      			return;
			      		}
			    	}
			  	}
		    
			  	if (bmp != null)
			  	{
			    	SaveResult res = m_VideoFileWriter.SaveFrame(bmp);
			    	
					if(res != SaveResult.Success)
					{
						// Start cancellation procedure
						// The producer should test for .Cancelling and stop queuing items at this point.
						// We don't try to save the outstanding frames, but the video file should be valid.
						log.Error("Error while saving frame to file.");
						m_bCancelling = true;
						m_CancelReason = res;
						
						bmp.Dispose();
						
						lock (m_Locker)
						{
							while(m_FrameQueue.Count > 0)
							{
								Bitmap outstanding = m_FrameQueue.Dequeue();
								if(outstanding != null)
								{
									outstanding.Dispose();
								}
							}
						
							if(m_bInitialized)
							{
								try
								{
									m_VideoFileWriter.CloseSavingContext(true);
								}
								catch (Exception exp)
								{
									log.Error(exp.Message);
									log.Error(exp.StackTrace);	
								}
							}
						}
						
						m_WaitHandle.Close();
						return;
					}
					else
					{
						if(!m_bCaptureThumbSet)
						{
							m_CaptureThumb = bmp;
							m_bCaptureThumbSet = true;
						}
						else
						{
							bmp.Dispose();
						}
					}
			  	}
			  	else
			  	{
			  		m_WaitHandle.WaitOne();
			  	}
			}
		}
		#endregion
	}
}
