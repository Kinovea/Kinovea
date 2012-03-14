#region License
/*
Copyright © Joan Charmant 2011. joan.charmant@gmail.com 
Licensed under the MIT license: http://www.opensource.org/licenses/mit-license.php
*/
#endregion
using System;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A base class for drawings that are not attached to any particular keyframe because they are managers for sub items.
    /// TODO: this would be a good candidate for a generic.
    /// </summary>
    public abstract class AbstractMultiDrawing : AbstractDrawing
    {
        public abstract object SelectedItem
        {
            get;
        }
        public abstract int Count
        {
            get;
        }
        
        public abstract void Add(object _item);
        public abstract void Remove(object _item);
        public abstract void Clear();
    }
}
