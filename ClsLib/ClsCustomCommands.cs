using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ClsLib
{
    /// <summary>
    /// Class for delegating and executing specific custom commands,
    /// where the syntax is parsed across multiple nested levels.

    /// </summary>
    public class ClsCustomCommands
    {
        /// <summary>
        /// Executes a custom command using the syntax:
        ///   command parameter file <OPTION>
        ///   command parameter file2 <IP>
        ///   command parameter file3 <TEST>
        ///   command anderes file <OPTION>
        ///   command anderes egal <FDSA>
        ///   anderercommand parameter file <OPTION>
        ///   anderercommand parameter file2 <IP>
        ///   anderercommand anderes file <OPTION>
        ///   anderercommand anderes egal <FDSA>
        /// </summary>
        /// <param name="trimmedInput">The complete, trimmed user input string.</param>
        public void Execute(string trimmedInput)
        {
            if (string.IsNullOrWhiteSpace(trimmedInput))
                ProcessUnknown("Input must not be empty.");

            // Tokens splitten (Quoted strings stay together.)
            var matches = Regex.Matches(trimmedInput, @"(?<=^|\s)""[^""]*""|\S+");
            var tokens = new List<string>(matches.Count);
            foreach (Match m in matches)
                tokens.Add(m.Value.Trim('"'));  // Trim surround quotes

            var sub1 = (tokens.Count > 0) ? tokens[0] : ("");
            switch (sub1)
            {
                case "command":
                    {
                        var sub2 = (tokens.Count > 1) ? tokens[1] : ("");
                        switch (sub2)
                        {
                            case "parameter":
                                {
                                    var sub3 = (tokens.Count > 2) ? tokens[2] : ("");
                                    switch (sub3)
                                    {
                                        case "one":
                                            ProcessUnknown(trimmedInput);
                                            break;
                                        case "file":
                                            ProcessFile(tokens);
                                            break;
                                        case "two":
                                            ProcessUnknown(trimmedInput);
                                            break;
                                        default:
                                            ProcessUnknown(trimmedInput);
                                            break;
                                    }
                                }
                                break;

                            case "option":
                                {
                                    var sub3 = (tokens.Count > 2) ? tokens[2] : ("");
                                    switch (sub3)
                                    {
                                        case "one":
                                            ProcessUnknown(trimmedInput);
                                            break;
                                        case "two":
                                            ProcessUnknown(trimmedInput);
                                            break;
                                        default:
                                            ProcessUnknown(trimmedInput);
                                            break;
                                    }
                                }
                                break;

                            default:
                                ProcessUnknown(trimmedInput);
                                break;
                        }
                    }
                    break;

                default:
                    ProcessUnknown(trimmedInput);
                    break;
            }
        }

        /// <summary>
        /// Prosesses unknown line
        /// </summary>
        private void ProcessUnknown(string line)
        {
            Console.WriteLine("Unknown: " + line);
        }


        /// <summary>
        /// Reads multi-line input and saves it to a file named prefix_parameter.txt.
        /// </summary>
        private void ProcessFile(List<string> tokens)
        {
            if (tokens.Count > 3)
            {
                string prefix = tokens[2];
                string parameter = tokens[3];

                if (string.IsNullOrEmpty(parameter))
                    throw new ArgumentException($"Missing parameter for '{prefix}'.", nameof(parameter));

                Console.WriteLine("Enter content (end with an empty line):");
                var lines = new List<string>();
                string line;
                while (!string.IsNullOrEmpty(line = Console.ReadLine() ?? string.Empty))
                    lines.Add(line);

                var fileName = $"{prefix}_{parameter}.txt";
                File.WriteAllText(fileName, string.Join(Environment.NewLine, lines));
                Console.WriteLine($"Content saved to {fileName}");
            }
            else
            {
                ProcessUnknown(string.Join(" ", tokens));
            }
        }
    }
}


