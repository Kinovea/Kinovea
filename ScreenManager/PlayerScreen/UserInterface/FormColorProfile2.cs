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

namespace Kinovea.ScreenManager.PlayerScreen.UserInterface
{
	/// <summary>
	/// The dialog lets the user configure the whole color profile.
	/// All the modfications are made on a temporary profile 
	/// which is only comitted when the user submit the form.
	/// </summary>
	public partial class FormColorProfile2 : Form
	{
		
		
		public FormColorProfile2()
		{
			InitializeComponent();
			
			
		}
	}
}
