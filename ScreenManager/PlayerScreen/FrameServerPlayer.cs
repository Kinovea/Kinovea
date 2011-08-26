#region License
/*
Copyright © Joan Charmant 2009.
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
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Kinovea.Base;
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
using Kinovea.Video;
using Kinovea.Video.FFMpeg;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// FrameServerPlayer encapsulate the video file, meta data and everything 
	/// needed to render the frame and access file functions.
	/// PlayerScreenUserInterface is the View, FrameServerPlayer is the Model.
	/// </summary>
	public class FrameServerPlayer : AbstractFrameServer
	{
		#region Properties
		public VideoReader VideoReader
		{
			get { return m_VideoReader; }
			set { m_VideoReader = value; }
		}
		public Metadata Metadata
		{
			get { return m_Metadata; }
			set { m_Metadata = value; }
		}
		public CoordinateSystem CoordinateSystem
		{
			get { return m_Metadata.CoordinateSystem; }
		}
		public bool Loaded
		{
		    get { return m_VideoReader != null && m_VideoReader.Loaded; }
		}
		#endregion
		
		#region Members
		private VideoReader m_VideoReader;
		private Metadata m_Metadata;
		private formProgressBar m_FormProgressBar;
		private BackgroundWorker m_BgWorkerSave = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
        private SaveResult m_SaveResult;
        private bool m_SavingMetada;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion

		#region Constructor
		public FrameServerPlayer()
		{
            m_BgWorkerSave.ProgressChanged += bgWorkerSave_ProgressChanged;
            m_BgWorkerSave.RunWorkerCompleted += bgWorkerSave_RunWorkerCompleted;
            m_BgWorkerSave.DoWork += bgWorkerSave_DoWork;
		}
		#endregion
		
		#region Public
		public OpenVideoResult Load(string _FilePath)
		{
			// Set global settings.
			PreferencesManager pm = PreferencesManager.Instance();
			
			// Instanciate appropriate video reader class depending on extension.
			string extension = Path.GetExtension(_FilePath);
			m_VideoReader = VideoTypeManager.GetVideoReader(extension);
			if(m_VideoReader != null)
			{
			    m_VideoReader.Options = new VideoOptions {
                    ImageAspectRatio = pm.AspectRatio,
                    Deinterlace = pm.DeinterlaceByDefault
			    };
                return m_VideoReader.Open(_FilePath);
			}
			else
			{
                return OpenVideoResult.NotSupported;
			}
		}
		public void Unload()
		{
			// Prepare the FrameServer for a new video by resetting everything.
			if(m_VideoReader != null && m_VideoReader.Loaded)
                m_VideoReader.Close();
			if(m_Metadata != null) m_Metadata.Reset();
		}
		public void SetupMetadata()
		{
			// Setup Metadata global infos in case we want to flush it to a file (or mux).
			
			if(m_Metadata != null && m_VideoReader != null)
			{
				Size imageSize = m_VideoReader.Info.DecodingSize;
						
				m_Metadata.ImageSize = imageSize;
				m_Metadata.AverageTimeStampsPerFrame = m_VideoReader.Info.AverageTimeStampsPerFrame;
				m_Metadata.CalibrationHelper.FramesPerSeconds = m_VideoReader.Info.FramesPerSeconds;
				m_Metadata.FirstTimeStamp = m_VideoReader.Info.FirstTimeStamp;
				
				log.Debug("Setup metadata.");
			}
		}
		public override void Draw(Graphics _canvas)
		{
			// Draw the current image on canvas according to conf.
			// This is called back from screen paint method.
		}
		public void Save(double _fPlaybackFrameInterval, double _fSlowmotionPercentage, ImageRetriever _DelegateOutputBitmap)	
		{
			// Let the user select what he wants to save exactly.
			formVideoExport fve = new formVideoExport(m_VideoReader.FilePath, m_Metadata, _fSlowmotionPercentage);
            if(fve.Spawn() == DialogResult.OK)
            {
            	if(fve.SaveAnalysis)
            	{
            		m_Metadata.ToXmlFile(fve.Filename);
            	}
            	else
            	{
            		DoSave(fve.Filename, 
    						fve.MuxDrawings,
    						fve.UseSlowMotion ? _fPlaybackFrameInterval : m_VideoReader.Info.FrameIntervalMilliseconds,
    						fve.BlendDrawings,
    						false,
    						false,
    						_DelegateOutputBitmap);
            	}
            }
			
			// Release configuration form.
            fve.Dispose();
		}
		public void SaveDiaporama(ImageRetriever _DelegateOutputBitmap, bool _diapo)
		{
			// Let the user configure the diaporama export.
			
			formDiapoExport fde = new formDiapoExport(_diapo);
			if(fde.ShowDialog() == DialogResult.OK)
			{
				DoSave(fde.Filename, 
				       	false, 
				       	fde.FrameInterval,
				       	true, 
				       	fde.PausedVideo ? false : true,
				       	fde.PausedVideo,
				       	_DelegateOutputBitmap);
			}
			fde.Dispose();
		}
		public void AfterSave()
		{
		    if(m_SavingMetada)
		    {
		        Metadata.CleanupHash();
		        m_SavingMetada = false;
		    }
		        
			// Ask the Explorer tree to refresh itself, (but not the thumbnails pane.)
            DelegatesPool dp = DelegatesPool.Instance();
            if (dp.RefreshFileExplorer != null)
            {
                dp.RefreshFileExplorer(false);
            }
		}
		#endregion
		
		#region Saving processing
		private void DoSave(string _FilePath, bool _saveMetadata, double _frameInterval, bool _bFlushDrawings, bool _bKeyframesOnly, bool _bPausedVideo, ImageRetriever _DelegateOutputBitmap)
        {
		    SavingSettings s = new SavingSettings();
		    s.Section = m_VideoReader.WorkingZone;
			s.File = _FilePath;
			s.FrameInterval = _frameInterval;
			s.FlushDrawings = _bFlushDrawings;
			s.KeyframesOnly = _bKeyframesOnly;
			s.PausedVideo = _bPausedVideo;
			s.ImageRetriever = _DelegateOutputBitmap;
			
			m_SavingMetada = _saveMetadata;
        	if(m_SavingMetada)
        	{
        		// If frame duplication is going to occur (saving at less than 8fps)
        		// We have to store this in the xml output to be able to match frames with timestamps later.
        		int iDuplicateFactor = (int)Math.Ceiling(_frameInterval / 125.0);
        		s.Metadata = m_Metadata.ToXmlString(iDuplicateFactor);
        	}

            m_FormProgressBar = new formProgressBar(true);
            m_FormProgressBar.Cancel = Cancel_Asked;
        	m_BgWorkerSave.RunWorkerAsync(s);
        	m_FormProgressBar.ShowDialog();
		}
		
		#region Background worker event handlers
		private void bgWorkerSave_DoWork(object sender, DoWorkEventArgs e)
        {
        	// This is executed in Worker Thread space. (Do not call any UI methods)
        	BackgroundWorker bgWorker = sender as BackgroundWorker;
        	if(!(e.Argument is SavingSettings) || bgWorker == null)
        	{
        	    m_SaveResult = SaveResult.UnknownError;
        	    e.Result = 0;
        	    return;
        	}
        	
        	try
        	{
        	    VideoFileWriter w = new VideoFileWriter();
        	    w.Save((SavingSettings)e.Argument, m_VideoReader, bgWorker);
        	}
        	catch (Exception exp)
			{
        		m_SaveResult = SaveResult.UnknownError;
				log.Error("Unknown error while saving video.");
				log.Error(exp.StackTrace);
			}
        	
        	e.Result = 0;
        }
		private void bgWorkerSave_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        	// This method should be called back from the writer when a frame has been processed.
        	// call snippet : bgWorker.ReportProgress(iCurrentValue, iMaximum);
        	
        	int iValue = (int)e.ProgressPercentage;
        	int iMaximum = (int)e.UserState;
            
            if (iValue > iMaximum) 
                iValue = iMaximum;
        	
            m_FormProgressBar.Update(iValue, iMaximum, true);
        }
		private void bgWorkerSave_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        	m_FormProgressBar.Close();
        	m_FormProgressBar.Dispose();
        	
        	if(m_SaveResult != SaveResult.Success)
            	ReportError(m_SaveResult);
        	else
        		AfterSave();
        }
        #endregion
		
        private void ReportError(SaveResult _err)
        {
        	switch(_err)
        	{
        		case SaveResult.Cancelled:
        			// No error message if the user cancelled herself.
                    break;
                
                case SaveResult.FileHeaderNotWritten:
                case SaveResult.FileNotOpened:
                    DisplayErrorMessage(ScreenManagerLang.Error_SaveMovie_FileError);
                    break;
                
                case SaveResult.EncoderNotFound:
                case SaveResult.EncoderNotOpened:
                case SaveResult.EncoderParametersNotAllocated:
                case SaveResult.EncoderParametersNotSet:
                case SaveResult.InputFrameNotAllocated:
                case SaveResult.MuxerNotFound:
                case SaveResult.MuxerParametersNotAllocated:
                case SaveResult.MuxerParametersNotSet:
                case SaveResult.VideoStreamNotCreated:
                case SaveResult.ReadingError:
                case SaveResult.UnknownError:
                default:
                    DisplayErrorMessage(ScreenManagerLang.Error_SaveMovie_LowLevelError);
                    break;
        	}
        }
		private void DisplayErrorMessage(string _err)
        {
        	MessageBox.Show(
        		_err.Replace("\\n", "\n"),
               	ScreenManagerLang.Error_SaveMovie_Title,
               	MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation);
        }
		private void Cancel_Asked(object sender, EventArgs e)
		{
			// User cancelled from progress form.
			m_BgWorkerSave.CancelAsync();
	        m_FormProgressBar.Dispose();
		}
		#endregion
	}
}
