#region License
/*
Copyright © Joan Charmant 2011. jcharmant@gmail.com 
Licensed under the MIT license: http://www.opensource.org/licenses/mit-license.php
*/
#endregion
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A base class for drawings that are not attached to any particular keyframe because they are managers for sub items.
    /// </summary>
    public abstract class AbstractMultiDrawing : AbstractDrawing
    {
        public abstract AbstractMultiDrawingItem SelectedItem
        {
            get;
        }
        public abstract int Count
        {
            get;
        }
        
        public abstract AbstractMultiDrawingItem GetNewItem(PointF point, long position, double averageTimeStampsPerFrame);
        public abstract AbstractMultiDrawingItem GetItem(Guid id);
        public abstract void Add(AbstractMultiDrawingItem item);
        public abstract void Remove(Guid id);
        public abstract void Clear();
    }
}
