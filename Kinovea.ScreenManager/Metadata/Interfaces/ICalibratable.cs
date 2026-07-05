#region License
/*
Copyright © Joan Charmant 2026.
jcharmant@gmail.com 
 
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
using Kinovea.Services;
using System;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Interface for drawings that can be calibrated.
    /// These should also be IMeasurable and ITrackable.
    /// Examples: Line and Plane.
    /// </summary>
    public interface ICalibratable
    {
        /// <summary>
        /// Called by the trackability manager at the end of the tracking step,
        /// both for object tracking and camera tracking,
        /// after all tracks have been processed and trackable points set.
        /// 
        /// This should be used to update the calibration if needed.
        /// </summary>
        void AfterAllTrackablePointsSet();
    }
}
