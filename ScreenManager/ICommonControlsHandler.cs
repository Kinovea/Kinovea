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

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// ICommonControlsHandler.
	/// Groups the methods of ScreenManager that must be reached from the 
	/// CommonControls UI.
	/// This avoid a tunnel of several delegates, without the need to expose all the SM to the UI.
	/// </summary>
	public interface ICommonControlsHandler
	{
		void CommonCtrl_GotoFirst();
		void CommonCtrl_GotoPrev();
		void CommonCtrl_GotoNext();
		void CommonCtrl_GotoLast();
		void CommonCtrl_Play();
		void CommonCtrl_Swap();
		void CommonCtrl_Sync();
		void CommonCtrl_Merge();
		void CommonCtrl_PositionChanged(long _iPosition);
		void CommonCtrl_Snapshot();
		void CommonCtrl_DualVideo();
	}
}
