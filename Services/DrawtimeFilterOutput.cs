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
using System.Drawing;

namespace Videa.Services
{
	
	public delegate void DelegateDraw(Graphics _canvas, Size _newSize);
    
	
	/// <summary>
	/// DrawtimeFilterOutput is a communication object between some filters and the player.
	/// 
	/// Once a filter is configured (and possibly preprocessed) it returns such an object.
	/// This is used for filters that needs back and forth communication with the player.
	/// 
	/// For example Mosaic filter configuration will simply set the input frames for the mosaic.
	/// The player will then ask the filter to draw them at the right canvas size.
	/// 
	/// Other example, the Kinorama filter will need to know which poses 
	/// should be included in the final image.
	/// 
	/// It can also be used for filters that alter the image size.
	/// </summary>
	public class DrawtimeFilterOutput
	{
		#region Delegates
		public DelegateDraw Draw;
		#endregion
		
		#region Propertie/members
		public Size ImageSize;
		#endregion
		
		
	}
}
