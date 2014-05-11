﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    public static class ScreenRemover
    {
        /// <summary>
        /// Remove the screen at the specified location. 
        /// </summary>
        /// <returns>true if the operation went through, false if it was canceled by the user.</returns>
        public static bool RemoveScreen(ScreenManagerKernel manager, int targetScreen)
        {
            manager.SetAllToInactive();

            if (targetScreen == -1)
            {
                manager.RemoveFirstEmpty();
                return true;
            }

            AbstractScreen screenToRemove = manager.GetScreenAt(targetScreen);

            if (screenToRemove is CaptureScreen)
            {
                manager.RemoveScreen(screenToRemove);
                return true;
            }

            PlayerScreen playerScreen = screenToRemove as PlayerScreen;
            bool confirmed = manager.BeforeReplacingPlayerContent(targetScreen);
            if (!confirmed)
                return false;

            manager.RemoveScreen(screenToRemove);
            return true;
        }
    }
}
