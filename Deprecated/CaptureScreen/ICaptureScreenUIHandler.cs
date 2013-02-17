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
	/// ICaptureScreenUIHandler.
	/// This is the part of the CaptureScreen that the CaptureScreen_UI is allowed to see.
	/// </summary>
	public interface ICaptureScreenUIHandler : IScreenUIHandler
	{
		// This interface is used for propagating requests to the ScreenManager.
		// Note that there are some methods hidden here via IScreenUIHandler inheritance.

		void CaptureScreenUI_FileSaved();
		void CaptureScreenUI_LoadVideo(string _filepath);
	}
}
