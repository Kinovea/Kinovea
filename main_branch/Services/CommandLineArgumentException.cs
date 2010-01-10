//+----------------------------------------------------------------------------
//
// File Name: CommandLineArgumentException.cs
// Description: An expection thrown when a command line argument is missing
//              or some parsing error occurs.
// Author: Ferad Zyulkyarov ferad.zyulkyarov[@]bsc.es
// Date: 04.02.2008
// License: LGPL
//
//-----------------------------------------------------------------------------

using System;

namespace Bsc
{
    /// <summary>
    /// An expection thrown when a command line argument is missing or some 
    /// parsing error occurs.
    /// </summary>
    public class CommandLineArgumentException : Exception
    {
        public CommandLineArgumentException()
            : base("CommandLineArgumentException: Unknown")
        {
        }

        public CommandLineArgumentException(string errorMessage)
            : base(errorMessage)
        {
        }
    }
}
