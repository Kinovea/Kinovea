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
using Kinovea.Services;
using System;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Interface for objects that can be mapped to physical values.
    /// Any object that can show its coordinates or length should implement this interface.
    /// </summary>
    public interface IMeasurable
    {
        CalibrationHelper CalibrationHelper { get; set; }

        /// <summary>
        /// Injects the extra data mode right after drawing creation, so that new drawings 
        /// use a similar setting than the last time the option was changed.
        /// This should be ignored for reloaded drawings (undo delete, paste, load from file, etc.).
        /// </summary>
        void InitializeMeasurableData(TrackExtraData trackExtraData);
        
        /// <summary>
        /// An event that the drawing should raise when the extra data was changed.
        /// This will be used to initialize new drawings with a similar setting.
        /// </summary>
        event EventHandler<EventArgs<TrackExtraData>> ShowMeasurableInfoChanged;
    }
}
