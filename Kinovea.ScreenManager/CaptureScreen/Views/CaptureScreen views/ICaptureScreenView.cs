#region License
/*
Copyright © Joan Charmant 2013.
jcharmant@gmail.com 
 
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
        void ConfigureDisplayControl(DelayCompositeType type);

        void UpdateTitle(string title, Bitmap icon);
        void UpdateInfo(string signal, string bandwidth, string load, string drops);
        void UpdateLoadStatus(float load);
        void UpdateGrabbingStatus(bool grabbing);
        void UpdateRecordingStatus(bool recording);
        void UpdateDelayMax(double delaySeconds, int delayFrames);
        void UpdateNextImageFilename(string filename);
        void UpdateNextVideoFilename(string filename);
        void Toast(string message, int duration);
    }
}
