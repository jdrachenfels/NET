using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ClsLib
{
    /// <summary>
    /// Manages command entries stored in commands.json, each with Parent, Name, Description, and HelpLines.
    /// </summary>
    public class ClsCommandStore
    {
        private readonly string _filePath;
        private readonly List<CommandEntry> _entries;

        public ClsCommandStore(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path must not be empty.", nameof(filePath));

            _filePath = filePath;
            if (!File.Exists(_filePath))
            {
                var dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                _entries = new List<CommandEntry>();
                Save();
            }
            else
            {
                _entries = Load();
            }
        }

        /// <summary>
        /// Adds or updates entries for each token in the command line.
        /// Parent is the full chain of previous tokens or "root" for the first.
        /// </summary>
        public void AddCommand(string commandLine)
        {
            if (string.IsNullOrWhiteSpace(commandLine))
                throw new ArgumentException("Command line must not be empty.", nameof(commandLine));

            var matches = Regex.Matches(commandLine, @"(?<=^|\s)""[^""]*""|\S+");
            var tokens = new List<string>(matches.Count);
            foreach (Match m in matches)
                tokens.Add(m.Value.Trim('"'));

            var parentChain = new List<string>();
            foreach (var tok in tokens)
            {
                var parent = parentChain.Count == 0 ? "root" : string.Join(" ", parentChain);
                if (!_entries.Exists(e => e.Parent == parent && e.Name == tok))
                {
                    _entries.Add(new CommandEntry
                    {
                        Parent = parent,
                        Name = tok,
                        Description = string.Empty,
                        HelpLines = new List<string>()
                    });
                }
                parentChain.Add(tok);
            }
            Save();
        }

        /// <summary>
        /// Removes a specified command segment and all its descendants, but keeps higher-level parents.
        /// </summary>
        public bool RemoveCommand(string commandLine)
        {
            if (string.IsNullOrWhiteSpace(commandLine))
                return false;

            var matches = Regex.Matches(commandLine, @"(?<=^|\s)""[^""]*""|\S+");
            var tokens = new List<string>();
            foreach (Match m in matches)
                tokens.Add(m.Value.Trim('"'));

            if (tokens.Count < 2)
                return false;  // nothing to remove at top level

            // Determine parent chain excluding last token
            var parentChain = new List<string>(tokens);
            var lastToken = parentChain[parentChain.Count - 1];
            parentChain.RemoveAt(parentChain.Count - 1);
            var parent = parentChain.Count == 0 ? "root" : string.Join(" ", parentChain);

            bool removedAny = false;
            // Remove the specified entry
            int before = _entries.Count;
            _entries.RemoveAll(e => e.Parent == parent && e.Name == lastToken);
            if (_entries.Count < before)
                removedAny = true;

            // Remove all descendant entries
            var fullChain = parent + " " + lastToken;
            int beforeDesc = _entries.Count;
            _entries.RemoveAll(e => e.Parent == fullChain || e.Parent.StartsWith(fullChain + " "));
            if (_entries.Count < beforeDesc)
                removedAny = true;

            if (removedAny)
                Save();
            return removedAny;
        }

        /// <summary>
        /// Lists all command entries.
        /// </summary>
        public IReadOnlyList<CommandEntry> ListEntries() => _entries.AsReadOnly();

        private List<CommandEntry> Load()
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<CommandEntry>>(json) ?? new List<CommandEntry>();
        }

        private void Save()
        {
            var opts = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_entries, opts);
            File.WriteAllText(_filePath, json);
        }
    }

    /// <summary>
    /// Represents a command part with hierarchy and documentation.
    /// </summary>
    public class CommandEntry
    {
        [JsonPropertyName("Parent")]
        public string Parent { get; set; } = string.Empty;

        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("Description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("HelpLines")]
        public List<string> HelpLines { get; set; } = new List<string>();
    }
}
