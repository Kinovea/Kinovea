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
    /// 
    /// Refactoring in progress.
    /// These functions should be implemented as events raised by screens.
    /// 
    /// ----
    /// IScreenHandler groups methods that needs to be accessed from screens to 
    /// trigger changes higher in the hierarchy.
    /// It is typically used to inject dependency into Screens,
    /// without passing the complete ScreenManager object, and thus without 
    /// exposing all public methods of ScreenManager.
    /// Previously, these method were injected one by one as delegates.
    /// The methods starting with Player_ or Capture_ are generally used when a screen
    /// needs to have an impact on the other screen.
    /// 
    /// These methods are implemented in ScreenManager.
    /// </summary>
    public interface IScreenHandler
    {
        //void Screen_SetActiveScreen(AbstractScreen _ActiveScreen);
        void Screen_CloseAsked(AbstractScreen _SenderScreen);
        void Screen_UpdateStatusBarAsked(AbstractScreen _SenderScreen);
        
        void Player_SpeedChanged(PlayerScreen _screen, bool _bInitialisation);
        void Player_PauseAsked(PlayerScreen _screen);
        void Player_SelectionChanged(PlayerScreen _screen, bool _bInitialization);
        void Player_ImageChanged(PlayerScreen _screen, Bitmap _image);
        void Player_SendImage(PlayerScreen _screen, Bitmap _image);
        void Player_Reset(PlayerScreen _screen);
        
        //void Capture_FileSaved(CaptureScreen _screen);
        //void Capture_LoadVideo(CaptureScreen _screen, string _filepath);
    }
}
