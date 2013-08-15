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
        	get { return ScreenManagerLang.CommandDeleteKeyframe_FriendlyName; }
        }

        private PlayerScreenUserInterface view;
        private long framePosition;
        private Metadata metadata;
        private Keyframe keyframe;

        public CommandDeleteKeyframe(PlayerScreenUserInterface view, Metadata metadata, long framePosition)
        {
            this.view = view;
            this.framePosition = framePosition;
            this.metadata = metadata;

            int index = metadata.GetKeyframeIndex(framePosition);
            if (index >= 0)
                keyframe = metadata[index];
        }

        public void Execute()
        {
            int index = metadata.GetKeyframeIndex(framePosition);
            if (index >= 0)
                view.OnRemoveKeyframe(index);
        }

        public void Unexecute()
        {
            view.OnAddKeyframe(framePosition);

            // We can't add them through the CommandAddDrawing scheme, 
            // because it completely messes up the Commands History.

            // Even now, Command History is quite messed up, but the user need to 
            // go back and forth in the undo/redo to notice the problem.

            if (keyframe.Drawings.Count == 0)
                return;
            
            int index = metadata.GetKeyframeIndex(framePosition);
            for (int i = keyframe.Drawings.Count-1; i >= 0; i--)
                metadata[index].Drawings.Insert(0, keyframe.Drawings[i]);
        }
    }
}