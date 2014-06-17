#region License
/*
Copyright © Joan Charmant 2013.
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
using System.Drawing;
using System.Windows.Forms;
using Kinovea.Services;
using System;

namespace Kinovea.ScreenManager
{
    public interface ICaptureScreenView
    {
        event EventHandler<EventArgs<HotkeyCommand>> DualCommandReceived;

        string CurrentImageFilename { get; }
        string CurrentVideoFilename { get; }

        void DisplayAsActiveScreen(bool active);
        void FullScreen(bool fullScreen);
        void RefreshUICulture();
        
        void AddImageDrawing(string filename, bool svg);
        void AddImageDrawing(Bitmap bmp);
        
        void BeforeClose();
        
        void SetViewport(Viewport viewport);
        void SetCapturedFilesView(CapturedFilesView capturedFilesView);
        void SetToolbarView(Control toolbarView);
        void ShowThumbnails();
        
        void UpdateTitle(string title);
        void UpdateInfo(string info);
        void UpdateGrabbingStatus(bool grabbing);
        void UpdateRecordingStatus(bool recording);
        void UpdateDelayLabel(double delaySeconds, int delayFrames);
        void UpdateDelayMaxAge(double max);
        void UpdateNextImageFilename(string filename, bool editable);
        void UpdateNextVideoFilename(string filename, bool editable);
        
        void Toast(string message, int duration);
    }
}
