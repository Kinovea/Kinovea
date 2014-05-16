using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.ScreenManager;

namespace Kinovea.Tests.HistoryStackTester
{
    public class MementoTest : HistoryMemento
    {
        public override string CommandName
        {
            get { return "Test"; }
            set { }
        }

        private State state;
        private int value;

        public MementoTest(State state)
        {
            this.state = state;    
            this.value = state.GetState();
        }

        /// <summary>
        /// Builds a command that will be able to undo the undo, then perform the undo.
        /// </summary>
        public override HistoryMemento PerformUndo()
        {
            HistoryMemento command = new MementoTest(state);
            state.SetState(value);
            return command;
        }
    }
}
