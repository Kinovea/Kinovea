using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    public abstract class AbstractMultiDrawingItem
    {
        public Guid Id
        {
            get { return identifier; }
        }

        public abstract int ContentHash { get; }
        
        protected Guid identifier = Guid.NewGuid();
    }
}
