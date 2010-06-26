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
	/// IFrameGrabberContainer.
	/// Defines the interface of the parent of the frame grabber.
	/// These are the methods that are called back from the frame grabber, as notifications.
	/// 
	/// This interface would normally be implemented by FrameServerCapture.
	/// 
	/// Using an interface helps with encapsulation, 
	/// frame grabbers only see what they need from FrameServerCapture.
	/// It avoids passing a collection of delegates to the constructor.
	/// </summary>
	public interface IFrameGrabberContainer
	{
		void Connected();
		void SetImageSize(Size _size);
		void FrameGrabbed();
		void AlertCannotConnect();
	}
}
