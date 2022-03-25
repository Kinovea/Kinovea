#region License
/*
Copyright © Joan Charmant 2011.
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
using System.Drawing;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Defines a method to initialize a drawing after it has been created.
    /// This is for drawing that need to set multiple points on the anchor frame
    /// for the drawing to make sense, like "line" or "angle".
    /// Drawings that span multiple frames shouldn't use this.
    /// </summary>
    public interface IInitializable
    {
        /// <summary>
        /// Continues to setup the drawing.
        /// </summary>
        void InitializeMove(PointF point, Keys modifiers);

        /// <summary>
        /// Ends an initialization step.
        /// The drawing should decide to stay in initialization mode or not.
        /// Returns the key of the created point for tracking purposes.
        /// Returns null if this information is irrelevant.
        /// </summary>
        string InitializeCommit(PointF point);

        /// <summary>
        /// Force end the initialization phase.
        /// Returns the key of the cancelled point for tracking purposes.
        /// Returns null if this information is irrelevant.
        /// </summary>
        string InitializeEnd(bool cancelCurrentPoint);

        /// <summary>
        /// Whether the drawing is still in initialization mode.
        /// </summary>
        bool Initializing { get; }
    }
}
