using ClsLib;
using NET.Cli;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

class Program
{
    public static class GlobalVars
    {
        public static string AppName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name ?? "APP";
    }

    static async Task Main(string[] args)
    {
        // Parse arguments
        string serverMode = "";
        string socketPath = GlobalVars.AppName + ".sock";
        string serverIP = "localhost";
        int serverPort = 50001;
        bool doAuth = true;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--server":
                    if (i + 1 < args.Length)
                        serverMode = args[++i].ToLowerInvariant();
                    break;
                case "--file":
                    if (i + 1 < args.Length)
                        socketPath = args[++i];
                    break;
                case "--ip":
                    if (i + 1 < args.Length)
                        serverIP = args[++i];
                    break;
                case "--port":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out var p))
                        serverPort = p;
                    break;
                case "--noauth":
                    doAuth = false;
                    break;
                case "--help":
                    Console.WriteLine($"{GlobalVars.AppName} --server socket --file {socketPath}");
                    Console.WriteLine($"{GlobalVars.AppName} --server tcp --ip {serverIP} --port {serverPort}");
                    Console.WriteLine($"{GlobalVars.AppName} --noauth {doAuth}");
                    break;
            }
        }

        if (serverMode == "socket")
        {
            Console.WriteLine("Starting in daemon mode. Waiting for SIGINT/SIGTERM to shut down...");

            // Create and start the server
            var server = new ClsCommandServerSocket(socketPath);
            server.Start();

            // Setup shutdown signal handling
            var shutdown = new ManualResetEventSlim(false);
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; // Prevent immediate termination
                shutdown.Set();
            };
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                shutdown.Set();
            };

            // Wait for signal
            shutdown.Wait();

            // Gracefully stop the server
            await server.StopAsync();
            Console.WriteLine("Server has been stopped.");

        }
        else if (serverMode == "tcp")
        {
            Console.WriteLine("Starting in daemon mode. Waiting for SIGINT/SIGTERM to shut down...");

            // Create and start the server
            var tcpServer = new ClsCommandServerTcp(serverIP, serverPort);
            tcpServer.Start();

            // Setup shutdown signal handling
            var shutdown = new ManualResetEventSlim(false);
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; // Prevent immediate termination
                shutdown.Set();
            };
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                shutdown.Set();
            };

            // Wait for signal
            shutdown.Wait();

            // Gracefully stop the server
            await tcpServer.StopAsync();
            Console.WriteLine("Server has been stopped.");

        }
        else 
        {
            // Auth
            if (doAuth)
            {
                ClsAdminUser AU = new();
                bool isAuthenticated = false;
                int wrongAuthCounter = 0;

                while (isAuthenticated == false)
                {
                    Console.Write("Username: ");
                    AU.Username = Console.ReadLine()!;
                    Console.Write("Password: ");
                    AU.Password = Console.ReadLine()!;
                    isAuthenticated = AU.Auth();
                    if (isAuthenticated == true)
                    {
                        Console.WriteLine("Welcome " + AU.Username + "!");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Wrong username or password!");
                        wrongAuthCounter++;
                        if (wrongAuthCounter == 3)
                        {
                            Console.WriteLine("Too many auth failures!");
                            return;
                        }
                    }
                }
            }

            // Prompt
            var inputHandler = new InputHandler("commands.json");
            while (true)
            {
                // Initial prompt
                var inputRaw = inputHandler.ReadInput("> ");
                var trimmedInput = inputRaw.TrimEnd();

                // Help request: display HelpLines and then re-prompt without '?'
                if (trimmedInput.EndsWith("?"))
                {
                    var cmdPath = trimmedInput.TrimEnd('?').TrimEnd();
                    if (!inputHandler.ShowHelp(cmdPath))
                        Console.WriteLine("No help available for: " + cmdPath);
                    // re-prompt with the same command (without '?') prefilled
                    inputRaw = inputHandler.ReadInput("> ", cmdPath + " ");
                    trimmedInput = inputRaw.TrimEnd();
                }

                // Exit
                if (trimmedInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    break;

                // History commands
                if (trimmedInput.Equals("history show", StringComparison.OrdinalIgnoreCase))
                {
                    inputHandler.ShowHistory();
                    continue;
                }
                if (trimmedInput.Equals("history clear", StringComparison.OrdinalIgnoreCase))
                {
                    inputHandler.ClearHistory();
                    continue;
                }

                if (trimmedInput.Length > 0)
                {

                    List<string> ccResult = new List<string>();
                    // All other commands goes here:
                    ClsCustomCommands CC = new();
                    ccResult = CC.Execute(trimmedInput);

                    // Write output
                    ccResult.ForEach(Console.WriteLine);
                }
            }
        }
    }
}

public class CommandDefinition
{
    public string Parent { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<string>? HelpLines { get; set; }
}

public class CommandNode
{
    public string Name { get; }
    public string Description { get; set; }
    public List<string> HelpLines { get; set; }
    public List<CommandNode> Children { get; } = new List<CommandNode>();
    public bool IsParameter => Name.StartsWith("<") && Name.EndsWith(">");

    public CommandNode(string name, string description = "", IEnumerable<string>? helpLines = null)
    {
        Name = name;
        Description = description;
        HelpLines = helpLines?.ToList() ?? new List<string>();
    }

    public CommandNode AddChild(CommandNode child)
    {
        Children.Add(child);
        return child;
    }
}

public class InputHandler
{
    private readonly List<string> _history = new List<string>();
    private int _historyIndex;
    private readonly StringBuilder _currentInput = new StringBuilder();
    private int _cursorPosition;
    private readonly List<CommandNode> _rootCommands;

    public InputHandler(string jsonPath)
    {
        var json = File.ReadAllText(jsonPath);
        var defs = JsonSerializer.Deserialize<List<CommandDefinition>>(json)
                   ?? throw new InvalidOperationException("Invalid JSON command definitions");

        // Create nodes by full path
        var nodeByPath = new Dictionary<string, CommandNode>(StringComparer.OrdinalIgnoreCase);
        foreach (var def in defs)
        {
            var fullPath = string.IsNullOrWhiteSpace(def.Parent) || def.Parent.Equals("root", StringComparison.OrdinalIgnoreCase)
                ? def.Name
                : $"{def.Parent} {def.Name}";
            if (!nodeByPath.ContainsKey(fullPath))
                nodeByPath[fullPath] = new CommandNode(def.Name, def.Description, def.HelpLines);
        }

        // Build tree
        _rootCommands = new List<CommandNode>();
        foreach (var def in defs)
        {
            var fullPath = string.IsNullOrWhiteSpace(def.Parent) || def.Parent.Equals("root", StringComparison.OrdinalIgnoreCase)
                ? def.Name
                : $"{def.Parent} {def.Name}";
            var node = nodeByPath[fullPath];
            if (string.IsNullOrWhiteSpace(def.Parent) || def.Parent.Equals("root", StringComparison.OrdinalIgnoreCase))
                _rootCommands.Add(node);
            else if (nodeByPath.TryGetValue(def.Parent, out var parent))
                parent.AddChild(node);
            else
                _rootCommands.Add(node);
        }
    }

    public string ReadInput(string prompt, string prefill = "")
    {
        // display prompt and optional prefill
        Console.Write(prompt);
        _currentInput.Clear();
        if (!string.IsNullOrEmpty(prefill))
        {
            _currentInput.Append(prefill);
            Console.Write(prefill);
            _cursorPosition = _currentInput.Length;
        }
        else
        {
            _cursorPosition = 0;
        }
        _historyIndex = _history.Count;

        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                var result = _currentInput.ToString();
                var trimmed = result.TrimEnd();
                if (!string.IsNullOrWhiteSpace(trimmed))
                    _history.Add(trimmed);
                return result;
            }
            if (key.Key == ConsoleKey.Tab)
                AutoComplete();
            else if (key.Key == ConsoleKey.Backspace && _cursorPosition > 0)
            {
                _currentInput.Remove(--_cursorPosition, 1);
                ReDrawLine(prompt);
            }
            else if (key.Key == ConsoleKey.LeftArrow && _cursorPosition > 0)
                _cursorPosition--;
            else if (key.Key == ConsoleKey.RightArrow && _cursorPosition < _currentInput.Length)
                _cursorPosition++;
            else if (key.Key == ConsoleKey.UpArrow)
                NavigateHistory(-1, prompt);
            else if (key.Key == ConsoleKey.DownArrow)
                NavigateHistory(1, prompt);
            else if (!char.IsControl(key.KeyChar))
            {
                _currentInput.Insert(_cursorPosition++, key.KeyChar);
                ReDrawLine(prompt);
            }
        }
    }

    public void ShowHistory()
    {
        if (_history.Count == 0)
            Console.WriteLine("No entries in history.");
        else
            _history.ForEach(Console.WriteLine);
    }

    public void ClearHistory() => _history.Clear();

    public bool ShowHelp(string commandPath)
    {
        var parts = commandPath.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var candidates = _rootCommands.AsEnumerable();
        CommandNode current = null;
        foreach (var token in parts)
        {
            current = candidates.FirstOrDefault(n => n.Name.Equals(token, StringComparison.OrdinalIgnoreCase));
            if (current == null)
                return false;
            candidates = current.Children;
        }
        if (current != null && current.HelpLines.Any())
        {
            current.HelpLines.ForEach(Console.WriteLine);
            return true;
        }
        return false;
    }

    private void AutoComplete()
    {
        var input = _currentInput.ToString();
        bool endsWithSpace = input.EndsWith(" ");

        // 1. Tokenize: Quoted strings bleiben zusammen
        var allParts = Regex.Matches(input, @"(?<=^|\s)""[^""]*""|\S+")
                            .Cast<Match>()
                            .Select(m => m.Value)
                            .ToArray();

        // 2. Abgeschlossene vs. aktueller Token – auch bei allParts.Length == 0 korrekt
        var completedTokens = (!endsWithSpace && allParts.Length > 0)
            ? allParts.Take(allParts.Length - 1).ToArray()
            : allParts;
        var lastToken = (endsWithSpace || allParts.Length == 0)
            ? string.Empty
            : allParts.Last();

        // 3. Bereits verwendete Optionen herausfiltern
        var usedOptions = completedTokens
            .Select(t => t.Trim('"'))
            .Where(t => t.StartsWith("--"))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // 4. Kandidaten von der Wurzelebene aus ermitteln
        var candidates = _rootCommands.AsEnumerable();
        bool expectingValue = false;

        // 5. Durchlauf abgeschlossener Tokens, skippe Option-Werte
        foreach (var raw in completedTokens)
        {
            var token = raw.Trim('"');
            if (expectingValue)
            {
                expectingValue = false;
                continue;
            }

            var match = candidates
                .FirstOrDefault(n => n.Name.Equals(token, StringComparison.OrdinalIgnoreCase));

            if (match != null)
            {
                if (match.Name.StartsWith("--"))
                    expectingValue = true;      // nächster Token ist Wert
                else
                    candidates = match.Children; // Subcommand-Ebene
            }
            else
            {
                var paramNode = candidates.FirstOrDefault(n => n.IsParameter);
                if (paramNode != null)
                    candidates = paramNode.Children;
                else
                {
                    candidates = Enumerable.Empty<CommandNode>();
                    break;
                }
            }
        }

        // 6. Vorschläge basierend auf dem unvollständigen letzten Token
        var prefix = lastToken.Trim('"');
        var suggestions = string.IsNullOrEmpty(prefix)
            ? candidates
            : candidates.Where(n =>
                n.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

        // 7. Ausgefilterte, bereits genutzte Optionen entfernen
        suggestions = suggestions
            .Where(n => !usedOptions.Contains(n.Name));

        if (suggestions.Count() == 1 && !suggestions.First().IsParameter)
        {
            CompleteToken(suggestions.First(), endsWithSpace, input);
        }
        else if (suggestions.Any())
        {
            var displayPrefix = endsWithSpace
                ? input
                : string.Join(" ", completedTokens) + " ";
            ShowSuggestions(displayPrefix, suggestions);
        }
    }



    private void CompleteToken(CommandNode node, bool endsWithSpace, string input)
    {
        if (!endsWithSpace)
        {
            int idx = input.LastIndexOf(' ');
            if (idx >= 0)
                _currentInput.Remove(idx + 1, _currentInput.Length - (idx + 1));
            else
                _currentInput.Clear();
            _cursorPosition = _currentInput.Length;
        }
        _currentInput.Append(node.Name + " ");
        _cursorPosition = _currentInput.Length;
        ReDrawLine("> ");
    }

    private void ShowSuggestions(string prefix, IEnumerable<CommandNode> nodes)
    {
        Console.WriteLine();
        Console.WriteLine("Suggestions:");
        int maxLen = nodes.Max(n => n.Name.Length);
        foreach (var n in nodes)
            Console.WriteLine($"> {n.Name.PadRight(maxLen + 5)}{n.Description}");
        ReDrawLine("> ");
    }

    private void NavigateHistory(int direction, string prompt)
    {
        _historyIndex = Math.Clamp(_historyIndex + direction, 0, _history.Count);
        _currentInput.Clear();
        if (_historyIndex < _history.Count)
            _currentInput.Append(_history[_historyIndex]);
        _cursorPosition = _currentInput.Length;
        ReDrawLine(prompt);
    }

    private void ReDrawLine(string prompt)
    {
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(prompt + _currentInput);
        Console.SetCursorPosition(prompt.Length + _cursorPosition, Console.CursorTop);
    }
}
