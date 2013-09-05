#region License
/*
Copyright © Joan Charmant 2009.
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
#endregion

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Back reference to the screen manager for the common controls UI, via a limited interface.
    /// More convenient than bubbling events or injecting commands through the main view.
    /// </summary>
    public interface ICommonControlsManager
    {
        void CommonCtrl_Swap();

        // Dual Players specific
        void CommonCtrl_PlayToggled();
        void CommonCtrl_GotoFirst();
        void CommonCtrl_GotoPrev();
        void CommonCtrl_GotoNext();
        void CommonCtrl_GotoLast();
        void CommonCtrl_Sync();
        void CommonCtrl_Merge();
        void CommonCtrl_PositionChanged(long position);
        void CommonCtrl_DualSave();
        void CommonCtrl_DualSnapshot();

        // Dual Capture specific
        void CommonCtrl_GrabbingChanged(bool grab);
        void CommonCtrl_Snapshot();
        void CommonCtrl_RecordingChanged(bool record);
    }
}
