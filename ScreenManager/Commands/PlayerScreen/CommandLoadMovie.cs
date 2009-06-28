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

using Kinovea.ScreenManager.Languages;
using System;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Kinovea.Services;
using Kinovea.VideoFiles;

namespace Kinovea.ScreenManager
{
    //-------------------------------------------------
    // CommandLoadMovie
    //
    // Objet : Rendre le PlayerScreen opérationnel.
    // - Charger un fichier vidéo dans le PlayerServer
    //--------------------------------------------------

    public class CommandLoadMovie : ICommand
    {
        #region Properties
        public string FriendlyName
        {
            get{ return ScreenManagerLang.CommandLoadMovie_FriendlyName;}
        }
		#endregion
        
        #region Members
        private string m_FilePath;
        private PlayerScreen m_PlayerScreen;
		#endregion
		
        #region constructor
        public CommandLoadMovie( PlayerScreen _PlayerScreen, String _FilePath)
        {
            m_PlayerScreen = _PlayerScreen;    
            m_FilePath = _FilePath;
        }
        #endregion

        /// <summary>
        /// Command execution. 
        /// Load the given file in the given screen.
        /// </summary>
        public void Execute()
        {
            DelegatesPool dp = DelegatesPool.Instance();
            if (dp.StopPlaying != null)
            {
                dp.StopPlaying();
            }
            
			DirectLoad();
        }

        private void DirectLoad()
        {
        	if(m_PlayerScreen.FrameServer.Loaded)
        	{
        		m_PlayerScreen.m_PlayerScreenUI.ResetToEmptyState();
        	}
        	
            LoadResult res = m_PlayerScreen.FrameServer.Load(m_FilePath);

        	switch (res)
            {
                case LoadResult.Success:
                    {
                        // Essayer de charger la première frame et autres initialisations.
                        int iPostLoadProcess = m_PlayerScreen.m_PlayerScreenUI.PostLoadProcess();

                        switch (iPostLoadProcess)
                        {
                            case 0:
                                // Chargment OK.
                                // On est déjà passé en mode analyse si c'était possible.
                                break;
                            case -1:
                                {
                                    // Le chargement de la première frame à complètement échoué.
                                    // Cause la plus probable, taille image non standard.
                                   	m_PlayerScreen.m_PlayerScreenUI.ResetToEmptyState();
                                    DisplayErrorMessage(Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_ImageFormatError);
                                    break;
                                }
                            case -2:
                                {
                                    // Chargement de la première frame à montré que le fichier était problématique.
                                    m_PlayerScreen.m_PlayerScreenUI.ResetToEmptyState();
                                    DisplayErrorMessage(Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_InconsistantMovieError);
                                    break;
                                }
                            default:
                                break;
                        }
                        break;
                    }
                case LoadResult.FileNotOpenned:
                    {
                        DisplayErrorMessage(Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_FileNotOpened);
                        break;
                    }
                case LoadResult.StreamInfoNotFound:
                    {
                        DisplayErrorMessage(Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_StreamInfoNotFound);
                        break;
                    }
                case LoadResult.VideoStreamNotFound:
                    {
                        DisplayErrorMessage(Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_VideoStreamNotFound);
                        break;
                    }
                case LoadResult.CodecNotFound:
                    {
                        DisplayErrorMessage(Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_CodecNotFound);
                        break;
                    }
                case LoadResult.CodecNotOpened:
                    {
                        DisplayErrorMessage(Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_CodecNotOpened);
                        break;
                    }
                case LoadResult.CodecNotSupported:
                    {
                        DisplayErrorMessage(Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_CodecNotSupported);
                        break;
                    }
                case LoadResult.Cancelled:
                    {
                        break;
                    }
                case LoadResult.FrameCountError:
                    {
                        DisplayErrorMessage(Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_CodecNotFound);
                        break;
                    }

                default:
                    {
                        DisplayErrorMessage(Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_UnkownError);
                        break;
                    }
            }

            m_PlayerScreen.UniqueId = System.Guid.NewGuid();
          
        }
        private void DisplayErrorMessage(string error)
        {
        	MessageBox.Show(
        		error,
               	Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_Error,
               	MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation);
        }
    }
}
