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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// FlienameHelper computes the next file name.
	/// </summary>
	public class FilenameHelper
	{
		// The goal of this class is to compute the next file name for snapshot and recording feature on capture screen.
		// For "free text with increment" type of naming (default) :
		// We try to make it look like "it just works" for the user.
		// The compromise :
		// - We try to increment taking both screens into account.
		// - User should always be able to modify the text manually if he wants to.
		// hence: We do not try to update both screens simultaneously with the same number.
		// Each screen tracks his own file name.
		// 
		// When using pattern, both screen will use the same pattern and they will be updated after each save.
		
		#region Members
		private PreferencesManager m_PrefManager = PreferencesManager.Instance();
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
		
		#region Public Methods
		public string InitImage()
		{			
			string next = "";
			if(m_PrefManager.CaptureUsePattern)
			{
				next = ConvertPattern(m_PrefManager.CapturePattern, m_PrefManager.CaptureImageCounter);
			}
			else if(m_PrefManager.CaptureImageFile == "")
			{
				next = PreferencesManager.DefaultCaptureImageFile;
				log.DebugFormat("We never saved a file before, return the default file name : {0}", next);
			}
			else
			{
				next = Next(m_PrefManager.CaptureImageFile);
			}
			
			return next;
		}
		public string InitVideo()
		{
			string next = "";
			if(m_PrefManager.CaptureUsePattern)
			{
				next = ConvertPattern(m_PrefManager.CapturePattern, m_PrefManager.CaptureVideoCounter);
			}
			else if(m_PrefManager.CaptureVideoFile == "")
			{
				next = PreferencesManager.DefaultCaptureVideoFile;
				log.DebugFormat("We never saved a file before, return the default file name : {0}", next);
			}
			else
			{
				next = Next(m_PrefManager.CaptureVideoFile);
			}
			
			return next;
		}
		public string Next(string _current)
		{
			//---------------------------------------------------------------------
			// Increments an existing file name.
			// DO NOT use this function when using naming pattern, always use InitImage/InitVideo.
			// This function is oblivious to image/video.
			// if the existing name has a number in it, we increment this number.
        	// if not, we create a suffix.
        	// We do not care about extension here, it will be appended afterwards.
			//---------------------------------------------------------------------
			
        	string next = "";
        	if(m_PrefManager.CaptureUsePattern)
        	{
        		throw new Exception("Not implemented when using pattern. Use InitImage or InitVideo");
        	}
        	else if(!string.IsNullOrEmpty(_current))
        	{
        		// Find all numbers in the name, if any.
				Regex r = new Regex(@"\d+");
				MatchCollection mc = r.Matches(_current);
	        	
				if(mc.Count > 0)
	        	{
	        		// Number(s) found. Increment the last one.
	        		// TODO: handle leading zeroes in the original (001 -> 002).
	        		Match m = mc[mc.Count - 1];
	        		int number = int.Parse(m.Value);
	        		number++;
	        		
	        		// Replace the number in the original.
	        		next = r.Replace(_current, number.ToString(), 1, m.Index );
	        	}
	        	else
	        	{
	        		// No number found, add suffix.
	        		next = String.Format("{0} - 2", Path.GetFileNameWithoutExtension(_current));
	        	}
	        	
	        	log.DebugFormat("Current file name : {0}, next file name : {1}", _current, next);
        	}
        	
			return next;
		}
		public bool ValidateFilename(string _filename, bool _allowEmpty)
        {
        	// Validate filename chars.
        	bool bIsValid = false;
        	
        	if(_filename.Length == 0 && _allowEmpty)
        	{
        		// special case for when the user is currently typing.
        		bIsValid = true;
        	}
        	else
        	{
				try
				{
				  	new FileInfo(_filename);
				  	bIsValid = true;
				}
				catch (ArgumentException)
				{
					// filename is empty, only white spaces or contains invalid chars.
					log.ErrorFormat("Capture filename has invalid characters. Proposed file was: {0}", _filename);
				}
				catch (NotSupportedException)
				{
					// filename contains a colon in the middle of the string.
					log.ErrorFormat("Capture filename has a colon in the middle. Proposed file was: {0}", _filename);
				}
        	}
			
			return bIsValid;
        }
		public string ConvertPattern(string _input, long _iAutoIncrement)
		{
			// Convert pattern into file name.
			// Codes : %y, %mo, %d, %h, %mi, %s, %i.
			string output = "";
			
			if (!string.IsNullOrEmpty(_input))
            {
                StringBuilder sb = new StringBuilder(_input);
				
                // Date and time.
                DateTime dt = DateTime.Now;
                sb.Replace("%y", String.Format("{0:0000}", dt.Year));
                sb.Replace("%mo", String.Format("{0:00}", dt.Month));
                sb.Replace("%d", String.Format("{0:00}", dt.Day));
                sb.Replace("%h", String.Format("{0:00}", dt.Hour));
                sb.Replace("%mi", String.Format("{0:00}", dt.Minute));
                sb.Replace("%s", String.Format("{0:00}", dt.Second));
               
                // auto-increment
                sb.Replace("%i", String.Format("{0}", _iAutoIncrement));
                
                output = sb.ToString();
            }
			
			return output;
		}
		public void AutoIncrement(bool _image)
		{
			// Autoincrement (only if needed and only the corresponding type).
			if(m_PrefManager.CapturePattern.Contains("%i"))
			{
				if(_image)
				{
					m_PrefManager.CaptureImageCounter++;
				}
				else
				{
					m_PrefManager.CaptureVideoCounter++;
				}
			}
		}
		#endregion
	}
}
