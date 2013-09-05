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
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    public class CommandLoadMovie : ICommand
    {
        public string FriendlyName
        {
            get { return ScreenManagerLang.CommandLoadMovie_FriendlyName;}
        }
        
        #region Members
        private string path;
        private PlayerScreen player;
        #endregion
        
        public CommandLoadMovie( PlayerScreen player, String path)
        {
            this.player = player;    
            this.path = path;
        }

        public void Execute()
        {
            NotificationCenter.RaiseStopPlayback(this);
            DirectLoad();
        }

        private void DirectLoad()
        {
            if(player.FrameServer.Loaded)
                player.view.ResetToEmptyState();
            
            OpenVideoResult res = player.FrameServer.Load(path);

            switch (res)
            {
                case OpenVideoResult.Success:
                    {
                        // Try to load first frame and other inits.
                        int postLoadResult = player.view.PostLoadProcess();
                        player.AfterLoad();

                        switch (postLoadResult)
                        {
                            case 0:
                                // Loading succeeded.
                                // We already switched to analysis mode if possible.
                                player.view.EnableDisableActions(true);
                                break;
                            case -1:
                                {
                                    // Loading the first frame failed.
                                    player.view.ResetToEmptyState();
                                    DisplayErrorAndDisable(Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_InconsistantMovieError);
                                    break;
                                }
                            case -2:
                                {
                                    // Loading first frame showed that the file is, in the end, not supported.
                                    player.view.ResetToEmptyState();
                                    DisplayErrorAndDisable(Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_InconsistantMovieError);
                                    break;
                                }
                            default:
                                break;
                        }
                        break;
                    }
                case OpenVideoResult.FileNotOpenned:
                    {
                        DisplayErrorAndDisable(Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_FileNotOpened);
                        break;
                    }
                case OpenVideoResult.StreamInfoNotFound:
                    {
                        DisplayErrorAndDisable(Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_StreamInfoNotFound);
                        break;
                    }
                case OpenVideoResult.VideoStreamNotFound:
                    {
                        DisplayErrorAndDisable(Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_VideoStreamNotFound);
                        break;
                    }
                case OpenVideoResult.CodecNotFound:
                    {
                        DisplayErrorAndDisable(Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_CodecNotFound);
                        break;
                    }
                case OpenVideoResult.CodecNotOpened:
                    {
                        DisplayErrorAndDisable(Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_CodecNotOpened);
                        break;
                    }
                case OpenVideoResult.CodecNotSupported:
                case OpenVideoResult.NotSupported:
                    {
                        DisplayErrorAndDisable(Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_CodecNotSupported);
                        break;
                    }
                case OpenVideoResult.Cancelled:
                    {
                        break;
                    }
                default:
                    {
                        DisplayErrorAndDisable(Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_UnkownError);
                        break;
                    }
            }

            player.UniqueId = System.Guid.NewGuid();
          
        }
        private void DisplayErrorAndDisable(string error)
        {
            player.view.EnableDisableActions(false);
            
            MessageBox.Show(
                error,
                Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_Error,
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation);
        }
    }
}
