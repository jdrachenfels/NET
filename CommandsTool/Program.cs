using System;
using System.Linq;
using ClsLib;

namespace CommandsTool
{
    class Program
    {
        static void Main(string[] args)
        {
            // Ask for filename once
            Console.Write("Enter commands file name (e.g. commands.json): ");
            var filename = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(filename))
            {
                Console.Error.WriteLine("Filename must not be empty. Exiting.");
                return;
            }

            var store = new ClsCommandStore(filename);

            while (true)
            {
                Console.Write("Action [add, delete, list, exit]: ");
                var action = Console.ReadLine()?.Trim().ToLowerInvariant();
                if (string.IsNullOrEmpty(action))
                    continue;
                if (action == "exit")
                    break;

                string commandString = string.Empty;
                if (action == "add" || action == "delete")
                {
                    Console.Write("Enter command string: ");
                    commandString = Console.ReadLine()?.Trim() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(commandString))
                    {
                        Console.Error.WriteLine("Command string must not be empty.");
                        continue;
                    }
                }

                try
                {
                    switch (action)
                    {
                        case "add":
                            store.AddCommand(commandString);
                            Console.WriteLine("OK");
                            break;
                        case "delete":
                            var removed = store.RemoveCommand(commandString);
                            Console.WriteLine(removed ? "OK" : "Not found");
                            break;
                        case "list":
                            var entries = store.ListEntries();
                            foreach (var entry in entries)
                                Console.WriteLine($"{entry.Parent} {entry.Name}");
                            break;
                        default:
                            Console.Error.WriteLine($"Unknown action: {action}");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error: {ex.Message}");
                }
            }

            Console.WriteLine("Exiting CommandsTool.");
        }
    }
}
