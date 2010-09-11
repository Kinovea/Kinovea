#region License
/*
Copyright © Joan Charmant 2010.
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
using System.Drawing;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// AbstractFrameGrabber.
	/// Class to encapsulate all the functionnality of providing access to the actual grabbing library.
	/// This is intended to decorrelate the capture code from the underlying lib that will 
	/// provide the images and the access to the camera.
	/// </summary>
	public abstract class AbstractFrameGrabber
	{
		public abstract bool IsConnected
		{
			get;
		}
		public abstract bool IsGrabbing
		{
			get;
		}
		public abstract string DeviceName
		{
			get;
		}
		public abstract int FramesInterval
		{
			get;
		}
		public abstract Size FrameSize
		{
			get;
		}	
		
		public abstract void PromptDeviceSelector();
		public abstract void NegociateDevice();
		public abstract void CheckDeviceConnection();
		public abstract void StartGrabbing();
		public abstract void PauseGrabbing();
		public abstract void BeforeClose();
	}
}
