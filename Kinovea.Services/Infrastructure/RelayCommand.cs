#region License
/*
Copyright © Joan Charmant 2012.
jcharmant@gmail.com 
 
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

namespace Kinovea.Services
{
    /// <summary>
    /// Piece of infrastructure to decouple domain logic from the UI.
    /// </summary>
    public class RelayCommand<T>
    {
        private readonly Action<T> execute;
        private readonly Predicate<T> canExecute;
        
        public RelayCommand(Action<T> execute) : this(execute, null) {}
        
        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if(execute == null)
                throw new ArgumentNullException("execute");
            
            this.execute = execute;
            this.canExecute = canExecute;
        }
        
        public void Execute(T parameter)
        {
            execute(parameter);
        }
        
        public bool CanExecute(T parameter)
        {
            return canExecute == null ? true : canExecute(parameter);
        }
    }
    
    /// <summary>
    /// A relay command that doesn't need any parameter to operate.
    /// </summary>
    public class RelayCommand
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;
        
        public RelayCommand(Action execute) : this(execute, null) {}
        
        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            if(execute == null)
                throw new ArgumentNullException("execute");
            
            this.execute = execute;
            this.canExecute = canExecute;
        }
        
        public void Execute()
        {
            execute();
        }
        
        public bool CanExecute()
        {
            return canExecute == null ? true : canExecute();
        }
    }
}
