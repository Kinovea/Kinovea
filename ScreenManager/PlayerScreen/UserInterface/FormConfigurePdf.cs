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
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// Work in progress...
	/// </summary>
    public partial class formConfigurePdf : Form
    {
        public formConfigurePdf()
        {
            InitializeComponent();
            rtbInfos.Clear();
            rtbInfos.AppendText("Page Layout 2");
            rtbInfos.AppendText("Each page will contain 3 Key Image with full comments text on the right.");
            rtbInfos.AppendText("Use this layout when the comments are important for the reader and you need to compare positions easily");
        }

       
    }
}