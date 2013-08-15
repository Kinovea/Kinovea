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

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class CommandAddPlayerScreen : IUndoableCommand
    {
        public string FriendlyName
        {
        	get { return ScreenManagerLang.CommandAddPlayerScreen_FriendlyName; }
        }
        
        private ScreenManagerKernel screenManagerKernel;

        public CommandAddPlayerScreen(ScreenManagerKernel screenManagerKernel, bool storeState)
        {
            this.screenManagerKernel = screenManagerKernel;
            if (storeState) 
                screenManagerKernel.StoreCurrentState();
        }

        public void Execute()
        {
            PlayerScreen screen = new PlayerScreen(screenManagerKernel);
            screen.RefreshUICulture();
            screenManagerKernel.AddScreen(screen);
        }
        
        public void Unexecute()
        {
            screenManagerKernel.RecallState();
        }
    }
}
