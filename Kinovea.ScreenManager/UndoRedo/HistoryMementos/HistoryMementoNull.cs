using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    public class HistoryMementoNull : HistoryMemento
    {
        public HistoryMementoNull()
        {
        }

        public override HistoryMemento PerformUndo()
        {
            //throw new InvalidOperationException("This operation is not undoable");
            return null;
        }
    }
}
