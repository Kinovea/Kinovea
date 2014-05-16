using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    public class HistoryMementoNull : HistoryMemento
    {
        public override string CommandName
        {
            get { return ""; }
            set { }
        }

        public override HistoryMemento PerformUndo()
        {
            return null;
        }
    }
}
