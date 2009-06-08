/*
Copyright © Joan Charmant 2008.
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

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Resources;
using System.Reflection;
using System.Threading;

using Videa.Services;

namespace Videa.ScreenManager
{
    public class CommandToggleCommonControls : IUndoableCommand
    {
        public string FriendlyName
        {
            get
            {
                ResourceManager rm = new ResourceManager("Videa.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
                return rm.GetString("CommandToggleCommonControls_FriendlyName", Thread.CurrentThread.CurrentUICulture);
            }
        }

        private SplitContainer m_SplitContainer;

        #region constructor
        public CommandToggleCommonControls(SplitContainer _sc)
        {
            m_SplitContainer = _sc;
        }
        #endregion

        public void Execute()
        {
            m_SplitContainer.Panel2Collapsed = !m_SplitContainer.Panel2Collapsed;
        }

        public void Unexecute()
        {
            Execute();
        } 


    }
}
