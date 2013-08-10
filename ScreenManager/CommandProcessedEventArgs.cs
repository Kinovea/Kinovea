using System;

namespace Kinovea.ScreenManager
{
    public class CommandProcessedEventArgs : EventArgs
    {
        public readonly int Command;
        public CommandProcessedEventArgs(int command)
        {
            this.Command = command;
        }
    }
}
