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
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    public abstract class AbstractScreen
    {
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
        public abstract string FilePath
        {
        	get;
        }
        public abstract bool CapabilityDrawings
        {
        	get;
        }

        public abstract void DisplayAsActiveScreen(bool _bActive);
        public abstract void refreshUICulture();
        public abstract void BeforeClose();
        public abstract bool OnKeyPress(Keys _key);
        public abstract void RefreshImage();
    }   
}
