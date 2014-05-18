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

namespace Kinovea.ScreenManager.Deprecated
{
    public class CommandDeleteChrono : IUndoableCommand
    {
        public string FriendlyName
        {
            get { return ScreenManagerLang.mnuChronoDelete; }
        }

        private PlayerScreenUserInterface view;
        private Metadata metadata;
        private DrawingChrono chrono;

        #region constructor
        public CommandDeleteChrono(PlayerScreenUserInterface view, Metadata metadata)
        {
            this.view = view;
            this.metadata = metadata;
            //this.chrono = metadata.ExtraDrawings[metadata.SelectedExtraDrawing] as DrawingChrono;
        }
        #endregion

        public void Execute()
        {
            if (chrono == null)
                return;
            
            //metadata.ExtraDrawings.Remove(chrono);
            view.DoInvalidate();
        }
        public void Unexecute()
        {
            if (chrono == null)
                return;
            
            //metadata.ExtraDrawings.Add(chrono);
            view.DoInvalidate();
        }
    }
}


