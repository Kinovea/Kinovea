#region License
/*
Copyright © Joan Charmant 2012.
joan.charmant@gmail.com 
 
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
#endregion
using System;

namespace Kinovea.ScreenManager
{   
    /// <summary>
    /// A command that executes delegates to determine which state the toggle currently is, and to execute the toggle.
    /// This should be part of a more general framework for decoupling the Views from the Presenters.
    /// </summary>
    public class ToggleCommand
    {
        private readonly Action<object> execute;
        private readonly Predicate<object> currentState;
        
        public ToggleCommand(Action<object> execute, Predicate<object> currentState)
        {
            this.execute = execute;
            this.currentState = currentState;
        }
        
        public bool CurrentState(object parameter)
        {
            return currentState(parameter);
        }
        
        public void Execute(object parameter)
        {
            execute(parameter);
        }
    }
}
