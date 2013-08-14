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
using System.Drawing;
using System.Windows.Forms;

using Kinovea.Services;

namespace Kinovea.Root
{
	/// <summary>
	/// IPreferencePanel.
	/// The interface all Preference panels must implement.
	/// 
	/// Note:
	/// A page must not directly change the Properties of PreferenceManager.
	/// You must create temporary variables for each preference, and then only 
	/// commit them to the PreferenceManager in the CommitChanges() method.
	/// </summary>
	public interface IPreferencePanel
	{
		string Description { get; }
		Bitmap Icon { get; }
		bool Visible { get; set; }
		Point Location { get; set; }
		
		void CommitChanges();
	}
}
