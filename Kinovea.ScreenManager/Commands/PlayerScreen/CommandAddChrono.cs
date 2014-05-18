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
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager.Deprecated
{
    public class CommandAddChrono : IUndoableCommand
    {
        public string FriendlyName
        {
            get { return ScreenManagerLang.CommandAddChrono_FriendlyName; }
        }
        
        private Action doInvalidate;
        private Action doUndrawn;
        private Metadata metadata;
        private DrawingChrono chrono;

        public CommandAddChrono(Action invalidate, Action undrawn, Metadata metadata)
        {
            this.doInvalidate = invalidate;
        	this.doUndrawn = undrawn;
            this.metadata = metadata;
            //chrono = metadata.ExtraDrawings[metadata.SelectedExtraDrawing] as DrawingChrono;
        }

        public void Execute()
        {
            /*if (chrono == null || metadata.ExtraDrawings.IndexOf(chrono) != -1)
                return;
        		
        	metadata.AddChrono(chrono);
        	doInvalidate();*/
        }

        public void Unexecute()
        {
            /*if (chrono == null)
                return;
            
            metadata.ExtraDrawings.Remove(chrono);
            doUndrawn();
            doInvalidate();*/
        }
    }
}

