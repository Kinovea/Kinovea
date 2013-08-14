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

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// AbstractFrameServer. 
    /// Abstract class that encapsulates all the metadata and configuration for managing frames in a screen.
    /// Concrete implementations will be responsible for holding frames or have access to them,
    /// holding key images and drawings and other meta data, 
    /// and provide a Draw method used by the screens.
    /// 
    /// This is intended to decorrelate the user interface from controller.
    /// </summary>
    public abstract class AbstractFrameServer
    {
        public abstract void Draw(Graphics canvas);
    }
}
