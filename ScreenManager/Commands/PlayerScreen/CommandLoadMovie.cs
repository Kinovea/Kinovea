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
        public string FriendlyName
        {
            get
            {
                ResourceManager rm = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
                return rm.GetString("CommandLoadMovie_FriendlyName", Thread.CurrentThread.CurrentUICulture);
            }
        }

        String              m_FilePath;
        PlayerScreen        m_PlayerScreen;

        #region constructor
        public CommandLoadMovie( PlayerScreen _PlayerScreen, String _FilePath)
        {
            m_PlayerScreen = _PlayerScreen;    
            m_FilePath = _FilePath;
        }
        #endregion

        /// <summary>
        /// Execution de la commande
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
            
            LoadResult res = m_PlayerScreen.m_PlayerScreenUI.m_VideoFile.Load(m_FilePath);

            //------------------------------------------------------------------
            // Eviter d'utiliser MessageBoxIcon.Error.
            // (Fait vraiment peur, et devrait être réservé aux crash imminents)
            //------------------------------------------------------------------

            switch (res)
            {
                case LoadResult.Success:
                    {
                        // Chargement a priori OK. 

                        m_PlayerScreen.m_bIsMovieLoaded = true;
                        m_PlayerScreen.m_sFileName = Path.GetFileName(m_FilePath);
                        m_PlayerScreen.FilePath = m_FilePath;

                        // Essayer de charger la première frame et autres initialisations.
                        int iPostLoadProcess = m_PlayerScreen.m_PlayerScreenUI.PostLoadProcess(res, m_FilePath);

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
                                    DisplayErrorMessage(Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_ImageFormatError);
                                    Unload();
                                    break;
                                }
                            case -2:
                                {
                                    // Chargement de la première frame à montré que le fichier était problématique.
                                    Unload();
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
                        //--------------------------------------------
                        // FFMPEG_ERROR_FILE_NOT_OPENED
                        // Exemple : tentative d'ouverture d'une image, fichier introuvable.
                        //--------------------------------------------
                        Unload();
                        DisplayErrorMessage(Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_FileNotOpened);
                        break;
                    }
                case LoadResult.StreamInfoNotFound:
                    {
                        //--------------------------------------------
                        // FFMPEG_ERROR_STREAM_INFO_NOT_FOUND
                        //--------------------------------------------
                        Unload();
                        DisplayErrorMessage(Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_StreamInfoNotFound);
                        break;
                    }
                case LoadResult.VideoStreamNotFound:
                    {
                        //--------------------------------------------
                        // FFMPEG_ERROR_VIDEO_STREAM_NOT_FOUND
                        // Exemple : tentative d'ouverture d'un mp3
                        //--------------------------------------------
                        Unload();
                        DisplayErrorMessage(Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_VideoStreamNotFound);
                        break;
                    }
                case LoadResult.CodecNotFound:
                    {
                        //--------------------------------------------------
                        // FFMPEG_ERROR_CODEC_NOT_FOUND
                        // Exemple : tentative d'ouverture d'un fichier wmv3
                        //--------------------------------------------------
                        Unload();
                        DisplayErrorMessage(Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_CodecNotFound);
                        break;
                    }
                case LoadResult.CodecNotOpened:
                    {
                        //--------------------------------------------
                        // FFMPEG_ERROR_CODEC_NOT_OPENED
                        //--------------------------------------------
                        Unload();
                        DisplayErrorMessage(Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_CodecNotOpened);
                        break;
                    }
                case LoadResult.CodecNotSupported:
                    {
                        //--------------------------------------------
                        // FFMPEG_ERROR_CODEC_NOT_SUPPORTED
                        // Exemple : tentative d'ouverture d'un .dpa
                        //--------------------------------------------
                        Unload();
                        DisplayErrorMessage(Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_CodecNotSupported);
                        break;
                    }
                case LoadResult.Cancelled:
                    {
                        //-----------------------------------------------------------------------------------
                        // FFMPEG_ERROR_LOAD_CANCELLED
                        // Ne devrait jamais passer par là. 
                        // Ne pas lire la valeur de e.Result si e.Cancelled est a true.
                        //-----------------------------------------------------------------------------------
                        Unload();
                        break;
                    }
                case LoadResult.FrameCountError:
                    {
                        //----------------------------------------------------------------------------
                        // FFMPEG_ERROR_FRAMECOUNT_ERROR
                        // On utilise le même message d'erreur que si le codec n'avait pas été trouvé.
                        //----------------------------------------------------------------------------
                        Unload();
                        DisplayErrorMessage(Kinovea.ScreenManager.Languages.ScreenManagerLang.LoadMovie_CodecNotFound);
                        break;
                    }

                default:
                    {
                        //--------------------------------------------
                        // Exemple : crash dans le PlayerServer
                        //--------------------------------------------
                        Unload();
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
        private void Unload()
        {
            m_PlayerScreen.m_bIsMovieLoaded = false;
            m_PlayerScreen.m_PlayerScreenUI.DisableControls();
            m_PlayerScreen.m_PlayerScreenUI.UnloadMovie();
        }
    }
}
