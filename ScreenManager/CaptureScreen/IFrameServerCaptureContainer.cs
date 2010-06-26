#region License
/*
Copyright © Joan Charmant 2008-2009.
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

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// IFrameServerCaptureContainer.
	/// This is basically the list of methods of the CaptureScreenUserInterface that are relevant for
	/// the frameServerCapture.
	/// frameServerCapture does not need to (and shouldn't) see anything else from its container.
	/// </summary>
	public interface IFrameServerCaptureContainer
	{
		void DoInvalidate();
		void DoInitDecodingSize();
		void DisplayAsGrabbing(bool _bIsGrabbing);
		void DoUpdateCapturedVideos();
	}
}
