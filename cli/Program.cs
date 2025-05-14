// Version 0.2.1
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

class Program
{
    static void Main()
    {
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
                    Console.WriteLine("Keine Hilfe vorhanden für: " + cmdPath);
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

            // SSL certificate add (multi-line input)
            if (trimmedInput.StartsWith("ssl certificate add ", StringComparison.OrdinalIgnoreCase))
            {
                var ip = trimmedInput.Substring("ssl certificate add ".Length).Trim();
                Console.WriteLine("Zertifikat eingeben (Ende mit leerer Zeile):");
                var certLines = new List<string>();
                string line;
                while (!string.IsNullOrEmpty(line = Console.ReadLine() ?? string.Empty))
                {
                    certLines.Add(line);
                }
                var path = $"cert_{ip}.crt";
                File.WriteAllText(path, string.Join(Environment.NewLine, certLines));
                Console.WriteLine($"Zertifikat gespeichert nach {path}");
                continue;
            }

            // Fallback echo
            Console.WriteLine($"Eingabe: {inputRaw}");
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
            Console.WriteLine("Keine Einträge in der Historie.");
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
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var fullTokens = endsWithSpace ? parts : parts.Take(parts.Length - 1);
        var lastToken = endsWithSpace ? string.Empty : parts.LastOrDefault() ?? string.Empty;

        var candidates = _rootCommands.AsEnumerable();
        CommandNode current = null;
        foreach (var token in fullTokens)
        {
            var match = candidates.FirstOrDefault(n => n.Name.Equals(token, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                current = match;
                candidates = current.Children;
            }
            else
            {
                var param = candidates.FirstOrDefault(n => n.IsParameter);
                if (param != null)
                {
                    current = param;
                    candidates = current.Children;
                }
                else
                {
                    candidates = Enumerable.Empty<CommandNode>();
                    break;
                }
            }
        }

        var suggestions = string.IsNullOrEmpty(lastToken)
            ? candidates
            : candidates.Where(n => n.Name.StartsWith(lastToken, StringComparison.OrdinalIgnoreCase));

        if (suggestions.Count() == 1 && !suggestions.First().IsParameter)
            CompleteToken(suggestions.First(), endsWithSpace, input);
        else if (suggestions.Any())
            ShowSuggestions(endsWithSpace ? input : string.Join(" ", fullTokens) + " ", suggestions);
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
        Console.WriteLine("Vorschläge:");
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
