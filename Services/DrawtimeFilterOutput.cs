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
using System.Collections.Generic;
using System.Drawing;

namespace Kinovea.Services
{
	
	public delegate void DelegateDraw(Graphics _canvas, Size _newSize, object _privateData);
	public delegate void DelegateIncreaseZoom(object _privateData);
    public delegate void DelegateDecreaseZoom(object _privateData);
    
	/// <summary>
	/// DrawtimeFilterOutput is a communication object between some filters and the player.
	/// 
	/// Once a filter is configured (and possibly preprocessed) it returns such an object.
	/// This is used for filters that needs back and forth communication with the player.
	/// 
	/// An object of this class needs to be like a self contained filter. 
	/// It can also be used for filters that alter the image size.
	/// </summary>
	public class DrawtimeFilterOutput
	{
		#region Delegates
		public DelegateDraw Draw;
		public DelegateIncreaseZoom IncreaseZoom;
		public DelegateDecreaseZoom DecreaseZoom;
		#endregion

		#region Properties
		public int VideoFilterType 
		{
			get { return m_iVideoFilterType; }
		}				
		public bool Active
		{
			get { return m_bActive; }
		}
		public object PrivateData
		{
			get { return m_PrivateData; }
			set { m_PrivateData = value; }
		}
		#endregion
		
		#region Propertie/members
		private int m_iVideoFilterType;
		private bool m_bActive;
		private object m_PrivateData;
		#endregion
		
		public DrawtimeFilterOutput(int _VideoFilterType, bool _bActive)
		{
			m_iVideoFilterType = _VideoFilterType;
			m_bActive = _bActive;
		}
		
	}
}
