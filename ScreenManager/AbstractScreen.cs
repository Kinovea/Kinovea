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


namespace Kinovea.ScreenManager
{
    public abstract class AbstractScreen : IScreen
    {
    	public delegate void DelegateCloseMe(AbstractScreen _screen);
        public delegate void DelegateSetMeAsActiveScreen(AbstractScreen _screen);        
    	
        public	DelegateCloseMe				CloseMe;
        public DelegateSetMeAsActiveScreen SetMeAsActiveScreen;
        
        public abstract Guid UniqueId
        {
            get;
            set;
        }
        public abstract bool Full
        {
        	get;
        }
        public abstract UserControl UI
        {
        	get;
        }

        public abstract void DisplayAsInactiveScreen();
        public abstract void DisplayAsActiveScreen();
        public abstract void refreshUICulture();
        public abstract void CloseScreen();
        public abstract bool OnKeyPress(Keys _key);
        public abstract void RefreshImage();
    }   
}
