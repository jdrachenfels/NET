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
        public List<string> Execute(string trimmedInput, string textFile = "")
        {
            // Result 
            List<string> result = new List<string>();

            if (string.IsNullOrWhiteSpace(trimmedInput))
                ProcessUnknown(new List<string>(), "Input must not be empty.");

            // Tokens splitten (Quoted strings stay together.)
            var matches = Regex.Matches(trimmedInput, @"(?<=^|\s)""[^""]*""|\S+");
            var tokens = new List<string>(matches.Count);
            foreach (Match m in matches)
                tokens.Add(m.Value.Trim('"'));  // Trim surround quotes

            var sub1 = (tokens.Count > 0) ? tokens[0] : ("");
            switch (sub1)
            {
                case "file":
                    {
                        var sub2 = (tokens.Count > 1) ? tokens[1] : ("");
                        switch (sub2)
                        {
                            case "add":
                                {
                                    result = AddFile(tokens, textFile);
                                    break;
                                }
                            case "delete":
                                {
                                    result = DeleteFile(tokens);
                                    break;
                                }
                            case "show":
                                {
                                    result = ShowFile(tokens);
                                    break;
                                }
                            default:
                                result = ProcessUnknown(tokens, trimmedInput);
                                break;
                        }
                        break;
                    }
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
                                            result = ProcessUnknown(tokens, trimmedInput);
                                            break;
                                        case "two":
                                            result = ProcessUnknown(tokens, trimmedInput);
                                            break;
                                        default:
                                            result = ProcessUnknown(tokens, trimmedInput);
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
                                            result = ProcessUnknown(tokens, trimmedInput);
                                            break;
                                        case "two":
                                            result = ProcessUnknown(tokens, trimmedInput);
                                            break;
                                        default:
                                            result = ProcessUnknown(tokens, trimmedInput);
                                            break;
                                    }
                                }
                                break;

                            default:
                                result = ProcessUnknown(tokens, trimmedInput);
                                break;
                        }
                    }
                    break;

                default:
                    result = ProcessUnknown(tokens, trimmedInput);
                    break;
            }
            return result;
        }

        /// <summary>
        /// Prosesses unknown line
        /// </summary>
        private List<string> ProcessUnknown(List<string> tokens, string line)
        {
            // Result 
            List<string> result = new List<string>();
            result.Add("Unknown: " + line);

            foreach (string m in tokens)
            {
                result.Add(m);
            }

            return result;

        }

        /// <summary>
        /// Reads multi-line input and saves it to a file named file_parameter.txt.
        /// </summary>
        private List<string> AddFile(List<string> tokens, string textFile = "")
        {
            // Result 
            List<string> result = new List<string>();

            if (tokens.Count > 2)
            {
                string parameter = tokens[2];

                if (string.IsNullOrEmpty(parameter))
                    result.Add("Missing parameter for '" + tokens[1] + "'");

                var fileName = $"file_{parameter}.txt";

                if (File.Exists(fileName))
                {
                    result.Add($"File {fileName} already exists.");
                }
                else
                {
                    if (!string.IsNullOrEmpty(textFile))
                    {
                        File.WriteAllText(fileName, textFile);
                        result.Add($"Content saved to {fileName} ({textFile.Length})");
                    }
                    else
                    {
                        Console.WriteLine("Enter content (end with an empty line):");
                        var lines = new List<string>();
                        string line;
                        while (!string.IsNullOrEmpty(line = Console.ReadLine() ?? string.Empty))
                            lines.Add(line);

                        File.WriteAllText(fileName, string.Join(Environment.NewLine, lines));
                        result.Add($"Content saved to {fileName}");
                    }
                }
            }
            else
            {
                result = ProcessUnknown(tokens, string.Join(" ", tokens));
                result.Add($" tokens.count {tokens.Count}");
            }
            return result;
        }

        /// <summary>
        /// Reads multi-line input and saves it to a file named file_parameter.txt.
        /// </summary>
        private List<string> ShowFile(List<string> tokens)
        {
            // Result 
            List<string> result = new List<string>();

            if (tokens.Count > 2)
            {
                string parameter = tokens[2];

                if (string.IsNullOrEmpty(parameter))
                    result.Add("Missing parameter for '" + tokens[1] + "'");

                var fileName = $"file_{parameter}.txt";

                if (File.Exists(fileName))
                {
                    var fileContent = File.ReadAllText(fileName);
                    foreach (string line in fileContent.Split(Environment.NewLine))
                        result.Add(line);
                }
                else
                {
                    result.Add($"File {fileName} does not exist.");
                }
            }
            else
            {
                result = ProcessUnknown(tokens, string.Join(" ", tokens));
            }
            return result;
        }

        /// <summary>
        /// Delete a file named file_parameter.txt.
        /// </summary>
        private List<string> DeleteFile(List<string> tokens)
        {
            // Result 
            List<string> result = new List<string>();

            if (tokens.Count > 2)
            {
                string parameter = tokens[2];

                if (string.IsNullOrEmpty(parameter))
                    result.Add("Missing parameter for '" + tokens[1] + "'");

                var fileName = $"file_{parameter}.txt";

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                    result.Add($"File {fileName} deleted.");
                }
                else
                {
                    result.Add($"File {fileName} does not exist.");
                }
            }
            else
            {
                result = ProcessUnknown(tokens, string.Join(" ", tokens));
            }
            return result;
        }

    }
}


