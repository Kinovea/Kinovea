using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.ScreenManager;

namespace Kinovea.Tests.HistoryStackTester
{
    /// <summary>
    /// Basic test of moving through the command history stack.
    /// We hate a simple state object and an operation that modifies the state.
    /// </summary>
    public class HistoryStackSimpleTester
    {
        private HistoryStack stack = new HistoryStack();
        private State state = new State();
        private Random random = new Random();

        public void Test()
        {

            for (int i = 0; i < 10; i++)
            {
                PerformOperation();
            }

            Console.WriteLine("----------------");

            for (int i = 0; i < 50; i++)
            {
                bool backward = random.NextBoolean();

                if (backward)
                {
                    Console.Write("Stepping backward. ");
                    stack.StepBackward();
                }
                else
                {
                    Console.Write("Stepping forward. ");
                    stack.StepForward();
                }

            }
            
            Console.ReadKey();
        }

        private void PerformOperation()
        {
            // Remember current state.
            MementoTest mementoTest = new MementoTest(state);
            
            // Perform operation that changes state.
            int nextState = state.GetState() + random.Next(-10, 11);
            state.SetState(nextState);

            // Push old state to stack.
            stack.PushNewCommand(mementoTest);
        }
    }
}
