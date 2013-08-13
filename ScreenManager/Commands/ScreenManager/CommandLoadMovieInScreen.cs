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
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Loads a file in a screen, create the screen if needed.
    /// </summary>
    public class CommandLoadMovieInScreen : IUndoableCommand
    {
        public string FriendlyName
        {
            get { return ScreenManagerLang.CommandLoadMovieInScreen_FriendlyName;}
        }

        private String filePath;
        private ScreenManagerKernel manager;
        private int targetScreen;

        #region constructor
        public CommandLoadMovieInScreen(ScreenManagerKernel manager, String filePath, int targetScreen, bool storeState)
        {
            this.manager = manager;
            this.filePath = filePath;
            this.targetScreen = targetScreen;
            if (storeState) 
                manager.StoreCurrentState();
        }
        #endregion

        public void Execute()
        {
            if (targetScreen < 0)
                LoadUnspecified();
            else
                LoadInSpecificTarget(targetScreen);
        }
        
        private void LoadUnspecified()
        {
            if (manager.ScreenCount == 0)
            {
                AddScreen();
                LoadInSpecificTarget(0);
            }
            else if (manager.ScreenCount == 1)
            {
                LoadInSpecificTarget(0);
            }
            else if (manager.ScreenCount == 2)
            {
                int emptyScreen = FindEmptyScreen();

                if (emptyScreen != -1)
                    LoadInSpecificTarget(emptyScreen);
                else
                    LoadInSpecificTarget(1);
            }
        }

        private void AddScreen()
        {
            CommandManager.LaunchCommand(new CommandAddPlayerScreen(manager, false));
        }

        private int FindEmptyScreen()
        {
            AbstractScreen screen0 = manager.GetScreenAt(0);
            if (!screen0.Full)
                return 0;

            AbstractScreen screen1 = manager.GetScreenAt(1);
            if (!screen1.Full)
                return 1;

            return -1;
        }

        private void ShowScreens()
        {
            CommandManager.LaunchCommand(new CommandShowScreens(manager));
        }

        private void LoadInSpecificTarget(int targetScreen)
        {
            AbstractScreen screen = manager.GetScreenAt(targetScreen);

            if (screen is CaptureScreen)
            {
                // loading a video onto a capture screen should not close the capture screen.
                // If there is room to add a second screen, we add a playback screen and load the video there,
                // otherwise, we don't do anything.
                if (manager.ScreenCount == 1)
                {
                    AddScreen();
                    manager.UpdateCaptureBuffers();
                    LoadInSpecificTarget(1);
                }
            }
            else if (screen is PlayerScreen)
            {
                PlayerScreen playerScreen = screen as PlayerScreen;
                bool confirmed = BeforeReplacingContent(targetScreen);
                if (!confirmed)
                    return;

                CommandManager.LaunchCommand(new CommandLoadMovie(playerScreen, filePath));

                if (playerScreen.FrameServer.Loaded)
                {
                    NotificationCenter.RaiseFileOpened(this, filePath);
                    SaveFileToHistory(filePath);
                }
            
                ShowScreens();
                manager.OrganizeCommonControls();
                manager.OrganizeMenus();
                manager.UpdateStatusBar();
            }
        }

        private bool BeforeReplacingContent(int targetScreen)
        {
            // FIXME: duplicated with CommandLoadCameraInScreen. Move to manager ?

            // Check if we are overloading on a non-empty player. Propose to save data.
            // Returns true if the loading can go on.
            
            PlayerScreen player = manager.GetScreenAt(targetScreen) as PlayerScreen;
            if(player == null || !player.FrameServer.Metadata.IsDirty)
                return true;
    
            DialogResult save = ConfirmDirty();
            if (save == DialogResult.No)
            {
                return true;
            }
            else if(save == DialogResult.Cancel)
            {
                manager.CancelLastCommand = true;
                return false;
            }
            else
            {
                // TODO: shouldn't we save the right screen instead of just the active one ?
                manager.SaveData();
                return true;
            }
        }

        private DialogResult ConfirmDirty()
        {
            return MessageBox.Show(
                ScreenManagerLang.InfoBox_MetadataIsDirty_Text.Replace("\\n", "\n"),
                ScreenManagerLang.InfoBox_MetadataIsDirty_Title,
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);
        }

        private void SaveFileToHistory(string filepath)
        {
            PreferencesManager.FileExplorerPreferences.AddRecentFile(filepath);
            PreferencesManager.Save();
        }

        public void Unexecute()
        {
            manager.RecallState();
        }
    }
}
