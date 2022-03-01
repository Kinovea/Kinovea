/*
Copyright © Joan Charmant 2008.
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Describes a generic drawing.
    /// All drawings must implement rendering and manipulation methods.
    /// </summary>
    public abstract class AbstractDrawing
    {
        #region Concrete properties
        public Guid Id
        {
            get { return identifier; }
        }

        /// <summary>
        /// The name of this instance of the drawing.
        /// May be changed by the user.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public virtual bool IsValid
        {
            get { return true; }
        }
        public bool IsCopyPasteable
        {
            get { return (Caps & DrawingCapabilities.CopyPaste) == DrawingCapabilities.CopyPaste; }
        }
        #endregion


        #region Abstract properties
        /// <summary>
        /// Gets or set the fading object for this drawing. 
        /// This is used in opacity calculation for Persistence.
        /// </summary>
        public abstract InfosFading InfosFading
        {
            get;
            set;
        }
        
        /// <summary>
        /// Get the capabilities of this drawing for the generic part of context menu.
        /// </summary>
        public abstract DrawingCapabilities Caps
        {
            get;
        }
        
        /// <summary>
        /// Gets the list of extra context menu specific to this drawing.
        /// </summary>
        public abstract List<ToolStripItem> ContextMenu
        {
            get;
        }
        
        /// <summary>
        /// The friendly name of the drawing tool. Used for undo menu for example.
        /// </summary>
        public abstract string ToolDisplayName
        {
            get;
        }
        
        /// <summary>
        /// Hash of the significant values of the drawing. Used for detection of unsaved changes.
        /// </summary>
        public abstract int ContentHash
        {
            get;
        }
        #endregion

        #region Concrete members
        protected Guid identifier = Guid.NewGuid();
        protected string name;
        #endregion

        #region Abstract methods
        /// <summary>
        /// Draw this drawing on the provided canvas.
        /// </summary>
        /// <param name="canvas">The GDI+ surface on which to draw</param>
        /// <param name="transformer">A helper object providing coordinate systems transformation</param>
        /// <param name="selected">Whether the drawing is currently selected</param>
        /// <param name="currentTimestamp">The current time position in the video</param>
        public abstract void Draw(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer, bool selected, long currentTimestamp);
        
        /// <summary>
        /// Evaluates if a particular point is inside the drawing, on a handler, or completely outside the drawing.
        /// </summary>
        /// <param name="point">The coordinates at original image scale of the point to evaluate</param>
        /// <param name="currentTimestamp">The current time position in the video</param>
        /// <returns>-1 : missed. 0 : The drawing as a whole has been hit. n (with n>0) : The id of a manipulation handle that has been hit</returns>
        public abstract int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer, bool zooming);
        
        /// <summary>
        /// Move the specified handle to its new location.
        /// </summary>
        /// <param name="point">The new location of the handle, in original image scale coordinates</param>
        /// <param name="handleNumber">The handle identifier</param>
        /// <param name="modifiers">Modifiers key pressed while moving the handle</param>
        public abstract void MoveHandle(PointF point, int handleNumber, Keys modifiers);
        
        /// <summary>
        /// Move the drawing as a whole.
        /// </summary>
        /// <param name="dx">Change in x coordinates</param>
        /// <param name="dy">Change in y coordinates</param>
        /// <param name="modifierKeys">Modifiers key pressed while moving the drawing</param>
        /// <param name="zooming">Whether the image is currently zoomed in</param>
        public abstract void MoveDrawing(float dx, float dy, Keys modifierKeys, bool zooming);

        /// <summary>
        /// Should return a standard position for the drawing based on the internal values.
        /// This is used for copy/paste support, to know where the drawing was at the time of copy, 
        /// so we can relocate the paste correctly. 
        /// Drawings keep their internal values in absolute image-space.
        /// The offset between that point and the mouse at copy-time will be computed, 
        /// and that offset will be injected back at paste-time based on the paste point mouse.
        /// </summary>
        /// <returns></returns>
        public abstract PointF GetCopyPoint();
        
        #endregion

        #region Concrete methods
        public static void InvalidateFromMenu(object sender)
        {
            // The screen hook was injected inside menus during AddDrawingCustomMenus in PlayerScreenUserInterface and for capture ViewportController.
            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            if (tsmi == null)
                return;
            
            IDrawingHostView host = tsmi.Tag as IDrawingHostView;
            if (host != null)
                host.InvalidateFromMenu();
        }

        public static void InvalidateFromTextbox(object sender)
        {
            TextBox tb = sender as TextBox;
            if (tb == null)
                return;

            IDrawingHostView host = tb.Tag as IDrawingHostView;
            if (host != null)
                host.InvalidateFromMenu();
        }

        public static long CurrentTimestampFromMenu(object sender)
        {
            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            if (tsmi == null)
                return 0;

            IDrawingHostView host = tsmi.Tag as IDrawingHostView;
            return host != null ? host.CurrentTimestamp : 0;
        }

        public static void UpdateFramesMarkersFromMenu(object sender)
        {
            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            if (tsmi == null)
                return;

            IDrawingHostView host = tsmi.Tag as IDrawingHostView;
            if (host != null)
                host.UpdateFramesMarkers();
        }

        public static void InitializeEndFromMenu(object sender, bool cancelLastPoint)
        {
            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            if (tsmi == null)
                return;

            IDrawingHostView host = tsmi.Tag as IDrawingHostView;
            if (host != null)
                host.InitializeEndFromMenu(cancelLastPoint);
        }

        public void AfterCopy()
        {
            identifier = Guid.NewGuid();

            // A new name will automatically be picked up after insertion.
            Name = "";
        }

        public bool ShouldSerializeCore(SerializationFilter filter)
        {
            return (filter & SerializationFilter.Core) == SerializationFilter.Core;
        }
        public bool ShouldSerializeStyle(SerializationFilter filter)
        {
            return (filter & SerializationFilter.Style) == SerializationFilter.Style;
        }
        public bool ShouldSerializeFading(SerializationFilter filter)
        {
            return (filter & SerializationFilter.Fading) == SerializationFilter.Fading;
        }
        public bool ShouldSerializeKVA(SerializationFilter filter)
        {
            return (filter & SerializationFilter.KVA) == SerializationFilter.KVA;
        }
        #endregion
    }
}
