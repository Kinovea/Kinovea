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
    public class CommandModifyChrono : IUndoableCommand
    {
        public string FriendlyName
        {
            get
            {
                string friendlyName = "";
                switch (modificationType)
                {
                    case ChronoModificationType.TimeStart:
                        friendlyName = ScreenManagerLang.mnuChronoStart;
                        break;
                    case ChronoModificationType.TimeStop:
                        friendlyName = ScreenManagerLang.mnuChronoStop;
                        break;
                    case ChronoModificationType.TimeHide:
                        friendlyName = ScreenManagerLang.mnuChronoHide;
                        break;
                    case ChronoModificationType.Countdown:
                        friendlyName = ScreenManagerLang.mnuChronoCountdown;
                        break;
                    default:
                        break;
                }
                return friendlyName;
            }
        }

        private PlayerScreenUserInterface view;
        private Metadata metadata;
        private DrawingChrono chrono;
        private ChronoModificationType modificationType;
        private long newValue;
        private long memoTimeStart;
        private long memoTimeStop;          
        private long memoTimeInvisible;
        private bool memoCountdown;				
        
        public CommandModifyChrono(PlayerScreenUserInterface view, Metadata metadata, ChronoModificationType modificationType, long newValue)
        {
            // In the special case of Countdown toggle, the new value will be 0 -> false, true otherwise .
            this.view = view;
            this.metadata = metadata;
            //this.chrono = metadata.ExtraDrawings[metadata.SelectedExtraDrawing] as DrawingChrono;
            this.newValue = newValue;
            this.modificationType = modificationType;

            if (chrono == null)
                return;
            
            memoTimeStart = chrono.TimeStart;
            memoTimeStop = chrono.TimeStop;
            memoTimeInvisible = chrono.TimeInvisible;
            memoCountdown = chrono.CountDown;
        }

        public void Execute()
        {
            if (chrono == null)
                return;
            
            switch (modificationType)
            {
                case ChronoModificationType.TimeStart:
                    chrono.Start(newValue);
                    break;
                case ChronoModificationType.TimeStop:
                    chrono.Stop(newValue);
                    break;
                case ChronoModificationType.TimeHide:
                    chrono.Hide(newValue);
                    break;
                case ChronoModificationType.Countdown:
                    chrono.CountDown = (newValue != 0);
                    break;
                default:
                    break;
            }
            
            view.DoInvalidate();
        }
        public void Unexecute()
        {
            if (chrono == null)
                return;
            
            // The 'execute' action might have forced a modification on other values. (e.g. stop before start)
            // We must reinject all the old values.
            chrono.Start(memoTimeStart);
            chrono.Stop(memoTimeStop);
            chrono.Hide(memoTimeInvisible);
            chrono.CountDown = memoCountdown;
            view.DoInvalidate();
        }
    }
}


