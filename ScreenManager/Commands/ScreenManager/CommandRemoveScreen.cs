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

using System.Windows.Forms;
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class CommandRemoveScreen : IUndoableCommand
    {
        public string FriendlyName
        {
            get { return ScreenManagerLang.CommandRemoveScreen_FriendlyName; }
        }

        private ScreenManagerKernel screenManagerKernel;
        private int screenToRemoveIndex;
        private bool storeState;

        public CommandRemoveScreen(ScreenManagerKernel screenManagerKernel, int screenToRemoveIndex, bool storeState)
        {
            this.screenManagerKernel = screenManagerKernel;
            this.screenToRemoveIndex = screenToRemoveIndex;
            this.storeState = storeState;
        }

        public void Execute()
        {
            screenManagerKernel.SetAllToInactive();
            
            // There are two types of closing demands: explicit and implicit.
            // explicit-close ask for a specific screen to be closed.
            // implicit-close just ask for a close, we choose which one here.
            if(screenToRemoveIndex == -1)
            {
                screenManagerKernel.RemoveFirstEmpty(storeState);
                return;
            }
            
            AbstractScreen screenToRemove = screenManagerKernel.GetScreenAt(screenToRemoveIndex);
                
            // Explicit. Make the other one the "active" screen if necessary.
            // For now, we do different actions based on screen type. (fixme?)
            if(screenToRemove is CaptureScreen)
            {
                RemoveScreen(screenToRemove);
                return;
            }

            PlayerScreen playerScreen = screenToRemove as PlayerScreen;
            bool confirmed = BeforeClose(playerScreen);
            if (!confirmed)
                return;

            RemoveScreen(screenToRemove);
        }

        private void RemoveScreen(AbstractScreen screen)
        {
            if (storeState)
                screenManagerKernel.StoreCurrentState();

            screenManagerKernel.RemoveScreen(screen);
        }

        private bool BeforeClose(PlayerScreen screen)
        {
            if (!screen.FrameServer.Metadata.IsDirty)
                return true;

            DialogResult save = ConfirmDirty();
            if (save == DialogResult.No)
            {
                return true;
            }
            else if (save == DialogResult.Cancel)
            {
                screenManagerKernel.CancelLastCommand = true;
                return false;
            }
            else
            {
                screenManagerKernel.SaveData();
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

        public void Unexecute()
        {
            screenManagerKernel.RecallState();
        }
    }
}

