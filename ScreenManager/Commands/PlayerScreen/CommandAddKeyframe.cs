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
using System.Reflection;
using System.Resources;
using System.Threading;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    //--------------------------------------------
    // CommandAddPlayerScreen -> devrait être réversible ?
    // Charge le fichier spécifier dans un écran, en créé un si besoin.
    // Si ok, réorganise les écrans pour montrer le nouveau ou décharger un ancien si besoin
    // Affiche le nouvel écran avec la vidéo dedans, prête.
    //--------------------------------------------
    public class CommandAddKeyframe : IUndoableCommand
    {

        public string FriendlyName
        {
        	get { return ScreenManagerLang.ToolTip_AddKeyframe; }
        }

        private PlayerScreenUserInterface m_psui;
        private long m_iFramePosition;
        private Metadata m_Metadata;
        
        #region constructor
        public CommandAddKeyframe(PlayerScreenUserInterface _psui, Metadata _Metadata, long _iFramePosition)
        {
            m_psui = _psui;
            m_iFramePosition = _iFramePosition;
            m_Metadata = _Metadata;
        }
        #endregion

        /// <summary>
        /// Execution de la commande
        /// </summary>
        public void Execute()
        {
            // Add a Keyframe at given position
            m_psui.OnAddKeyframe(m_iFramePosition);
        }
        public void Unexecute()
        {
            // The PlayerScreen used at execute time may not be valid anymore...
            // The MetaData used at execute time may not be valid anymore...
            // (use case : Add KF + Close screen + undo + undo)

            // Delete Keyframe at given position
            int iIndex = GetKeyframeIndex();
            if (iIndex >= 0)
            {
                m_psui.OnRemoveKeyframe(iIndex);
            }
        }
        private int GetKeyframeIndex()
        {
            int iIndex = -1;
            for (int i = 0; i < m_Metadata.Count; i++)
            {
                if (m_Metadata[i].Position == m_iFramePosition)
                {
                    iIndex = i;
                }
            }

            return iIndex;
        }
    }
}