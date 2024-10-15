#region License
/*
Copyright © Joan Charmant 2012.
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
using System;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Source of tracking data (manual or auto).
    /// </summary>
    public enum TrackingSource
    {
        /// <summary>
        /// Point moved manually by the user.
        /// </summary>
        Manual,

        /// <summary>
        /// Point found by a tracking algorithm.
        /// </summary>
        Auto,

        /// <summary>
        /// The tracking algorithm failed but we needed the data 
        /// (for example if the tracked point is part of a bigger drawing 
        /// that has other points that successfully tracked).
        /// In this case the placement reuses old data.
        /// </summary>
        ForcedClosest
    }
}
