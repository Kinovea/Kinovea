//+----------------------------------------------------------------------------
//
// File Name: CommandLineArgumentParser.cs
// Description: A command line argument parser.
// Author: Ferad Zyulkyarov ferad.zyulkyarov[@]bsc.es
// Date: 04.02.2008
// License: LGPL.
//
//-----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Bsc
{
    public class CommandLineArgumentParser
    {
        public const string Version = "1.0.0";

        /// <summary>
        /// The character used to distinguish which command lines parameters.
        /// </summary>
        private const string PARAM_SEPARATOR = "-";

        /// <summary>
        /// Stores the name=value required parameters.
        /// </summary>
        private static Dictionary<string, string> requiredParameters;

        /// <summary>
        /// Optional parameters.
        /// </summary>
        private static Dictionary<string, string> optionalParameters;

        /// <summary>
        /// Stores the list of the supported switches.
        /// </summary>
        private static Dictionary<string, bool> switches;

        /// <summary>
        /// Store the list of missing required parameters.
        /// </summary>
        private static List<string> missingRequiredParameters;

        /// <summary>
        /// Store the list of missing values of parameters.
        /// </summary>
        private static List<string> missingValue;

        /// <summary>
        /// Contains the raw arguments.
        /// </summary>
        private static List<string> rawArguments;
        
        /// <summary>
        /// Define the required parameters that the user of the program
        /// must provide.
        /// </summary>
        /// <param name="requiredParameterNames">
        /// The list of the required parameters.
        /// </param>
        public static void DefineRequiredParameters(string[] requiredParameterNames)
        {
            CommandLineArgumentParser.requiredParameters = new Dictionary<string, string>();

            foreach (string param in requiredParameterNames)
            {
                string temp = param;
                temp = temp.Trim();
                if (string.IsNullOrEmpty(param))
                {
                    string errorMessage = "Error: The required command line parameter '" + param + "' is empty.";
                    throw new CommandLineArgumentException(errorMessage);
                }

                CommandLineArgumentParser.requiredParameters.Add(param, string.Empty);
            }
        }

        /// <summary>
        /// Define the optional parameters. The parameters must be provided with their
        /// default values in the following format "paramName=paramValue".
        /// </summary>
        /// <param name="optionalParameters">
        /// The list of the optional parameters with their default values.
        /// </param>
        public static void DefineOptionalParameter(Dictionary<string, string> options)
        {
            CommandLineArgumentParser.optionalParameters = new Dictionary<string, string>();

            foreach (var pair in options)
            {
                string key = pair.Key;
                key = key.Trim();

                if (string.IsNullOrEmpty(key))
                {
                    string errorMessage = "Error: an optional command line parameter has no name.";
                    throw new CommandLineArgumentException(errorMessage);
                }

                string value = pair.Value.Trim();
                CommandLineArgumentParser.optionalParameters.Add(key, value);
            }
        }

        /// <summary>
        /// Defines the supported command line switches. Switch is a parameter
        /// without value. When provided it is used to switch on a given feature or
        /// functionality provided by the application. For example a switch for tracing.
        /// </summary>
        /// <param name="switches"></param>
        public static void DefineSwitches(List<string> switches)
        {
            CommandLineArgumentParser.switches = new Dictionary<string, bool>(switches.Count);

            foreach (string switchKey in switches)
            {
                string key = switchKey;
                key = key.Trim();

                if (string.IsNullOrEmpty(key))
                {
                    string errorMessage = "Error: A switch is empty.";
                    throw new CommandLineArgumentException(errorMessage);
                }

                CommandLineArgumentParser.switches.Add(key, false);
            }
        }

        /// <summary>
        /// Parse the command line arguments.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        public static void ParseArguments(string[] args)
        {
        	// All arguments are unknown (raw) until matched.
            rawArguments = new List<string>(args);

            missingRequiredParameters = new List<string>();
            missingValue = new List<string>();

            ParseRequiredParameters();
            ParseOptionalParameters();
            ParseSwitches();

            ThrowIfErrors();
        }

        /// <summary>
        /// Returns the value of the specified parameter.
        /// </summary>
        /// <param name="paramName">The name of the perameter.</param>
        /// <returns>The value of the perameter.</returns>
        public static string GetParamValue(string paramName)
        {
            string paramValue = string.Empty;

            if (requiredParameters != null && requiredParameters.ContainsKey(paramName))
            {
                paramValue = requiredParameters[paramName];
            }
            else if (optionalParameters != null && optionalParameters.ContainsKey(paramName))
            {
                paramValue = optionalParameters[paramName];
            }
            else
            {
                string errorMessage = "Error: The paramter '" + paramName + "' is not supported.";
                throw new CommandLineArgumentException(errorMessage);
            }

            return paramValue;
        }

        public static bool IsSwitchOn(string switchName)
        {
            bool switchValue = false;

            if (switches != null && switches.ContainsKey(switchName))
            {
                switchValue = switches[switchName];
            }
            else
            {
                string errorMessage = "Error: switch '" + switchName + "' not supported.";
                throw new CommandLineArgumentException(errorMessage);
            }

            return switchValue;
        }        

        private static void ParseRequiredParameters()
        {
            if (CommandLineArgumentParser.requiredParameters == null || CommandLineArgumentParser.requiredParameters.Count == 0)
            {
                return;
            }

            List<string> paramNames = new List<string>(CommandLineArgumentParser.requiredParameters.Keys);
            
            foreach (string paramName in paramNames)
            {
                int paramInd = rawArguments.IndexOf(paramName);
                if (paramInd < 0)
                {
                    missingRequiredParameters.Add(paramName);                    
                }
                else
                {
                    if (paramInd + 1 < rawArguments.Count)
                    {
                        //
                        // The argument after the parameter name is expected to be its value.
                        // No check for error is done here.
                        //
                        requiredParameters[paramName] = rawArguments[paramInd + 1];

                        rawArguments.RemoveAt(paramInd);
                        rawArguments.RemoveAt(paramInd);
                    }
                    else
                    {
                        missingValue.Add(paramName);
                        rawArguments.RemoveAt(paramInd);
                    }                    
                }
            }
        }

        private static void ParseOptionalParameters()
        {
            if (CommandLineArgumentParser.optionalParameters == null || CommandLineArgumentParser.optionalParameters.Count == 0)
            {
                return;
            }

            List<string> paramNames = new List<string>(CommandLineArgumentParser.optionalParameters.Keys);

            foreach (string paramName in paramNames)
            {
                int paramInd = rawArguments.IndexOf(paramName);

                if (paramInd >= 0)
                {
                    if (paramInd + 1 < rawArguments.Count)
                    {
                        optionalParameters[paramName] = rawArguments[paramInd + 1];

                        rawArguments.RemoveAt(paramInd);
                        
                        //
                        // After removing the param name, the index of the value
                        // becomes again paramInd.
                        //
                        rawArguments.RemoveAt(paramInd);
                    }
                    else
                    {
                        missingValue.Add(paramName);
                        rawArguments.RemoveAt(paramInd);
                    }
                }
            }
        }

        private static void ParseSwitches()
        {
            if (CommandLineArgumentParser.switches == null || CommandLineArgumentParser.switches.Count == 0)
            {
                return;
            }

            List<string> paramNames = new List<string>(CommandLineArgumentParser.switches.Keys);

            foreach (string paramName in paramNames)
            {
                int paramInd = rawArguments.IndexOf(paramName);

                if (paramInd >= 0)
                {
                    CommandLineArgumentParser.switches[paramName] = true;
                    rawArguments.RemoveAt(paramInd);
                }
            }
        }

        private static void ThrowIfErrors()
        {
            StringBuilder errorMessage = new StringBuilder();

            if (missingRequiredParameters.Count > 0 || missingValue.Count > 0 || rawArguments.Count > 0)
            {
                errorMessage.Append("Error: Processing Command Line Arguments\n");
            }

            if (missingRequiredParameters.Count > 0)
            {                
                errorMessage.Append("Missing Required Parameters\n");
                foreach (string missingParam in missingRequiredParameters)
                {
                    errorMessage.Append("\t" + missingParam + "\n");
                }
            }

            if (missingValue.Count > 0)
            {
                errorMessage.Append("Missing Values\n");
                foreach (string value in missingValue)
                {
                    errorMessage.Append("\t" + value + "\n");
                }
            }
            
            if(rawArguments.Count > 0)
            {                
                errorMessage.Append("Unknown Parameters");
                foreach (string unknown in rawArguments)
                {
                    errorMessage.Append("\t" + unknown + "\n");
                }                
            }

            if (errorMessage.Length > 0)
            {
                throw new CommandLineArgumentException(errorMessage.ToString());
            }
        }
    }
}

