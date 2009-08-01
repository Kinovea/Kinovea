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

using Kinovea.ScreenManager.Languages;
using Kinovea.VideoFiles;
using System.Windows.Forms;

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
		public VideoFile VideoFile
		{
			get { return m_VideoFile; }
			set { m_VideoFile = value; }
		}
		public Metadata Metadata
		{
			get { return m_Metadata; }
			set { m_Metadata = value; }
		}
		public bool Loaded
		{
			get { return m_VideoFile.Loaded;}
		}
		#endregion
		
		#region Members
		private VideoFile m_VideoFile = new VideoFile();
		private Metadata m_Metadata;
		
		// Saving process (globals because the bgWorker is split in several methods)
		private formProgressBar m_FormProgressBar;
		private long m_iSaveStart;
		private long m_iSaveEnd;
        private string m_SaveFile;
        private Metadata m_SaveMetadata;
        private int m_iSaveFramesInterval;
        private bool m_bSaveFlushDrawings;
        private bool m_bSaveKeyframesOnly;
        private DelegateGetOutputBitmap m_SaveDelegateOutputBitmap;
        private SaveResult m_SaveResult;
		#endregion

		#region Constructor
		public FrameServerPlayer()
		{
		}
		#endregion
		
		#region Public
		public LoadResult Load(string _FilePath)
		{
			return m_VideoFile.Load(_FilePath);
		}
		public void Unload()
		{
			// Prepare the FrameServer for a new video by resetting everything.
			m_VideoFile.Unload();
			if(m_Metadata != null) m_Metadata.Reset();
		}
		public void SetupMetadata()
		{
			// Setup Metadata global infos in case we want to flush it to a file (or mux).
			
			if(m_Metadata != null)
			{
				Size imageSize = new Size(m_VideoFile.Infos.iDecodingWidth, m_VideoFile.Infos.iDecodingHeight);
						
				m_Metadata.ImageSize = imageSize;
				m_Metadata.AverageTimeStampsPerFrame = m_VideoFile.Infos.iAverageTimeStampsPerFrame;
				m_Metadata.FirstTimeStamp = m_VideoFile.Infos.iFirstTimeStamp;
				m_Metadata.Plane.SetLocations(imageSize, 1.0, new Point(0,0));
				m_Metadata.Grid.SetLocations(imageSize, 1.0, new Point(0,0));
				
				m_Metadata.CleanupHash();
			}
		}
		public override void Draw(Graphics _canvas)
		{
			// Draw the current image on canvas according to conf.
			// This is called back from screen paint method.
		}
		public void Save(int _iPlaybackFrameInterval, int _iSlowmotionPercentage, Int64 _iSelStart, Int64 _iSelEnd, DelegateGetOutputBitmap _DelegateOutputBitmap)	
		{
			// Let the user select what he wants to save exactly.
			// Note: _iSelStart, _iSelEnd, _Metadata, should ultimately be taken from local members.
			
			formVideoExport fve = new formVideoExport(m_VideoFile.FilePath, m_Metadata, _iSlowmotionPercentage);
            
			if(fve.Spawn() == DialogResult.OK)
            {
            	if(fve.SaveAnalysis)
            	{
            		// Save analysis.
            		m_Metadata.ToXmlFile(fve.Filename);
            	}
            	else
            	{
            		DoSave(fve.Filename, 
    						fve.MuxDrawings ? m_Metadata : null,
    						fve.UseSlowMotion ? _iPlaybackFrameInterval : m_VideoFile.Infos.iFrameInterval,
    						_iSelStart,
    						_iSelEnd,
    						fve.BlendDrawings,
    						false,
    						_DelegateOutputBitmap);
            	}
            }
			
			// Release configuration form.
            fve.Dispose();
		}
		public void SaveDiaporama(Int64 _iSelStart, Int64 _iSelEnd, DelegateGetOutputBitmap _DelegateOutputBitmap)
		{
			// Let the user configure the diaporama export.
			
			formDiapoExport fde = new formDiapoExport(m_VideoFile.FilePath);
			if(fde.ShowDialog() == DialogResult.OK)
			{
				DoSave(fde.Filename, 
				       	null, 
				       	fde.FrameInterval, 
				       	_iSelStart, 
				       	_iSelEnd, 
				       	true, 
				       	true, 
				       	_DelegateOutputBitmap);
			}
			
			// Release configuration form.
			fde.Dispose();
		}
		#endregion
		
		#region Saving processing
		private void DoSave(String _FilePath, Metadata _Metadata, int _iPlaybackFrameInterval, Int64 _iSelStart, Int64 _iSelEnd, bool _bFlushDrawings, bool _bKeyframesOnly, DelegateGetOutputBitmap _DelegateOutputBitmap)
        {
			// Save video.
    		// We use a bgWorker and a Progress Bar.
    		
			// Memorize the parameters, they will be used later in bgWorkerSave_DoWork.
			// Note: _iSelStart, _iSelEnd, _Metadata, should ultimately be taken from the local members.
			m_iSaveStart = _iSelStart;
            m_iSaveEnd = _iSelEnd;
            m_SaveMetadata = _Metadata;
            m_SaveFile = _FilePath;
            m_iSaveFramesInterval = _iPlaybackFrameInterval;
            m_bSaveFlushDrawings = _bFlushDrawings;
            m_bSaveKeyframesOnly = _bKeyframesOnly;
            m_SaveDelegateOutputBitmap = _DelegateOutputBitmap;
            
            // Instanciate and configure the bgWorker.
            BackgroundWorker bgWorkerSave = new BackgroundWorker();
            bgWorkerSave.WorkerReportsProgress = true;
        	bgWorkerSave.DoWork += new DoWorkEventHandler(bgWorkerSave_DoWork);
        	bgWorkerSave.ProgressChanged += new ProgressChangedEventHandler(bgWorkerSave_ProgressChanged);
            bgWorkerSave.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorkerSave_RunWorkerCompleted);
            
            // Attach the bgWorker to the VideoFile object so it can report progress.
            m_VideoFile.BgWorker = bgWorkerSave;
            
            // Create the progress bar and launch the worker.
            m_FormProgressBar = new formProgressBar();
        	bgWorkerSave.RunWorkerAsync();
        	m_FormProgressBar.ShowDialog();
		}
		private void bgWorkerSave_DoWork(object sender, DoWorkEventArgs e)
        {
        	// This is executed in Worker Thread space. (Do not call any UI methods)
        	
        	string metadata = "";
        	if(m_SaveMetadata != null)
        	{
        		metadata = m_SaveMetadata.ToXmlString();
        	}
        	
        	m_SaveResult = m_VideoFile.Save(	m_SaveFile, 
        	                                	m_iSaveFramesInterval, 
        	                                	m_iSaveStart, 
        	                                	m_iSaveEnd, 
        	                                	metadata, 
        	                                	m_bSaveFlushDrawings, 
        	                                	m_bSaveKeyframesOnly, 
        	                                	m_SaveDelegateOutputBitmap);
	        
        	e.Result = 0;
        }
		private void bgWorkerSave_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        	// This method should be called back from the VideoFile when a frame has been processed.
        	// call snippet : m_BackgroundWorker.ReportProgress(iCurrentValue, iMaximum);
        	
        	int iValue = (int)e.ProgressPercentage;
        	int iMaximum = (int)e.UserState;
            
            if (iValue > iMaximum) { iValue = iMaximum; }
        	
            m_FormProgressBar.Update(iValue, iMaximum, true);
        }
		private void bgWorkerSave_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        	m_FormProgressBar.Close();
        	m_FormProgressBar.Dispose();
        	
        	if(m_SaveResult != SaveResult.Success)
            {
            	ReportError(m_SaveResult);
            }
        }
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
		#endregion
	}
}
