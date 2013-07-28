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
using System;
using System.Windows.Forms;
using Kinovea.Camera;
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Loads a camera in a screen, create the screen if needed.
    /// </summary>
    public class CommandLoadCameraInScreen : IUndoableCommand
    {
        public string FriendlyName
        {
            get { return ""; /*ScreenManagerLang.CommandAddCaptureScreen_FriendlyName;*/}
        }
        
        private ScreenManagerKernel manager;
        private CameraSummary summary;
        private int targetScreen;
        
        public CommandLoadCameraInScreen(ScreenManagerKernel manager, CameraSummary summary, int targetScreen)
        {
            this.manager = manager;
            this.summary = summary;
            this.targetScreen = targetScreen;
        }
        
        public void Execute()
        {
            if (targetScreen < 0)
                LoadUnspecified();
            else
                LoadInSpecificTarget(targetScreen);
        }
        
        public void Unexecute()
        {
        
        }
        
        private void LoadInSpecificTarget(int targetScreen)
        {
            AbstractScreen screen = manager.GetScreenAt(targetScreen);

            if (screen is CaptureScreen)
            {
                CaptureScreen captureScreen = screen as CaptureScreen;
                captureScreen.LoadCamera(summary);
                
                ShowScreens();
                manager.OrganizeCommonControls();
                manager.OrganizeMenus();
                manager.UpdateStatusBar();
            }
            else if (screen is PlayerScreen)
            {
                // Loading a camera onto a video should not close the video.
                // We only load the camera if there is room to create a new capture screen.
                if(manager.ScreenCount == 1)
                {
                    AddScreen();
                    LoadInSpecificTarget(1);
                }
            }
        }
        
        private void LoadUnspecified()
        {
            if(manager.ScreenCount == 0)
            {
                AddScreen();
                LoadInSpecificTarget(0);
            }
            else if(manager.ScreenCount == 1)
            {
                LoadInSpecificTarget(0);
            }
            else if(manager.ScreenCount == 2)
            {
                int emptyScreen = FindEmptyScreen();
                
                if(emptyScreen != -1)
                    LoadInSpecificTarget(emptyScreen);
                else
                    LoadInSpecificTarget(1);
            }
        }
        
        private void AddScreen()
        {
            ICommand caps = new CommandAddCaptureScreen(manager, false);
            CommandManager.LaunchCommand(caps);
        }
        
        private int FindEmptyScreen()
        {
            AbstractScreen screen0 = manager.GetScreenAt(0);
            if(!(screen0 is PlayerScreen) || !screen0.Full)
                return 0;
                
            AbstractScreen screen1 = manager.GetScreenAt(1);
            if(!(screen1 is PlayerScreen) || !screen1.Full)
                return 1;
            
            return -1;
        }
        
        private void ShowScreens()
        {
            CommandManager.LaunchCommand(new CommandShowScreens(manager));
        }
        
        private bool BeforeReplacingContent(int targetScreen)
        {
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
        
        private void ReplaceScreenType(int targetScreen)
        {
            // Replace a player screen with capture screen if needed.
            PlayerScreen player = manager.GetScreenAt(targetScreen) as PlayerScreen;
            if(player == null)
                return;
                
            // TODO: replace.
        }
        
        private DialogResult ConfirmDirty()
        {
            return MessageBox.Show(
                ScreenManagerLang.InfoBox_MetadataIsDirty_Text.Replace("\\n", "\n"),
                ScreenManagerLang.InfoBox_MetadataIsDirty_Title,
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);
        }
    }
}

