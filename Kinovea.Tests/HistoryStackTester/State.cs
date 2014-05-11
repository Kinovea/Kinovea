using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Tests.HistoryStackTester
{
    public class State
    {
        private int state;

        public int GetState()
        {
            return state;
        }
        public void SetState(int value)
        {
            state = value;
            Console.WriteLine("New state: {0}.", value);
        }

        public override string ToString()
        {
            return state.ToString();
        }
    }
}
