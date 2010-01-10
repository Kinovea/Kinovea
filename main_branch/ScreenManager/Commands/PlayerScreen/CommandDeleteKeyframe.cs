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

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    //--------------------------------------------
    // CommandAddPlayerScreen -> devrait être réversible ?
    // Charge le fichier spécifier dans un écran, en créé un si besoin.
    // Si ok, réorganise les écrans pour montrer le nouveau ou décharger un ancien si besoin
    // Affiche le nouvel écran avec la vidéo dedans, prête.
    //--------------------------------------------
    public class CommandDeleteKeyframe : IUndoableCommand
    {

        public string FriendlyName
        {
            get
            {
                ResourceManager rm = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
                return rm.GetString("CommandDeleteKeyframe_FriendlyName", Thread.CurrentThread.CurrentUICulture);
            }
        }

        private PlayerScreenUserInterface m_psui;
        private long m_iFramePosition;
        private Metadata m_Metadata;
        private Keyframe m_Keyframe;

        #region constructor
        public CommandDeleteKeyframe(PlayerScreenUserInterface _psui, Metadata _Metadata, long _iFramePosition)
        {
            m_psui = _psui;
            m_iFramePosition = _iFramePosition;
            m_Metadata = _Metadata;

            int iIndex = GetKeyframeIndex();
            if (iIndex >= 0)
            {
                m_Keyframe = m_Metadata[iIndex];
            }
        }
        #endregion

        /// <summary>
        /// Execution de la commande
        /// </summary>
        public void Execute()
        {
            // Delete a Keyframe at given position
            int iIndex = GetKeyframeIndex();
            if (iIndex >= 0)
            {
                m_psui.OnRemoveKeyframe(iIndex);
            }
        }
        public void Unexecute()
        {
            // Re Add the Keyframe
            m_psui.OnAddKeyframe(m_iFramePosition);

            // Re Add all drawings on the frame
            // We can't add them through the CommandAddDrawing scheme, 
            // because it completely messes up the Commands History.

            // Even now, Command History is quite messed up, but the user need to 
            // go back and forth in the undo/redo to notice the problem.

            if (m_Keyframe.Drawings.Count > 0)
            {
                int iIndex = GetKeyframeIndex();
                CommandManager cm = CommandManager.Instance();
            
                for (int i = m_Keyframe.Drawings.Count-1; i >= 0; i--)
                {
                    // 1. Add the drawing to the Keyframe
                    m_Metadata[iIndex].Drawings.Insert(0, m_Keyframe.Drawings[i]);

                    // 2. Call the Command
                    //IUndoableCommand cad = new CommandAddDrawing(m_psui, m_Metadata, m_iFramePosition);    
                    //cm.LaunchUndoableCommand(cad);
                }
                
                // We need to block the Redo here.
                // In normal behavior, we should have a "Redo : Delete Keyframe",
                // But here we added other commands, so we'll discard commands that are up in the m_CommandStack.
                // To avoid having a "Redo : Add Drawing" that makes no sense.
                //cm.BlockRedo();
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