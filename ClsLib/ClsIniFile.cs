using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ClsLib

// Test erfolgreich:

//ClsIniFile INI = new ClsIniFile("Cli.ini");
//string value = "";
//value = INI.ReadINI("BAR", "GETRAENK", "Bier");
//value = INI.ReadINI("BAR", "ESSEN", "Sandwich");
//value = INI.ReadINI("BAR", "ZEITUNG", "Frankfurter Rundschau");
//INI.WriteINI("", "STRASSE", "Hauptstrasse 99 A");
//INI.WriteINI("", "PLZ", "10258");
//INI.WriteINI("", "STADT", "Berlin");
//value = INI.ReadINI("", "STRASSE", "Simmlerstrasse 14");
//Console.WriteLine(value);
//value = INI.ReadINI("BAR", "ZEITUNG", "Pforzheimer Zeitung");
//Console.WriteLine(value);

{
    /// <summary>
    /// Klasse zum Lesen und Schreiben von INI-Dateien.
    /// </summary>
    public class ClsIniFile
    {
        private readonly string _filePath;
        private static readonly Regex ValidNameRegex = new Regex("^[A-Z0-9]+$", RegexOptions.Compiled);
        private static readonly Regex ValidValueRegex = new Regex("^[\x20-\x7E]*$", RegexOptions.Compiled);

        public ClsIniFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Dateipfad darf nicht leer sein.", nameof(filePath));
            _filePath = filePath;
        }

        /// <summary>
        /// Schreibt oder aktualisiert einen INI-Eintrag.
        /// </summary>
        public bool WriteINI(string appName, string varName, string varValue)
        {
            if (!ValidateNames(appName, varName) || !ValidateValue(varValue))
                return false;

            var data = Load();
            var section = string.IsNullOrEmpty(appName) ? string.Empty : appName;
            if (!data.ContainsKey(section))
                data[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            data[section][varName] = varValue;

            return Save(data);
        }

        /// <summary>
        /// Liest einen INI-Eintrag. Gibt DefaultValue zurück, wenn nicht gefunden und DefaultValue nicht leer.
        /// </summary>
        public string ReadINI(string appName, string varName, string defaultValue)
        {
            if (!ValidateNames(appName, varName))
                return defaultValue;

            var data = Load();
            var section = string.IsNullOrEmpty(appName) ? string.Empty : appName;
            if (data.TryGetValue(section, out var sectionData) && sectionData.TryGetValue(varName, out var val))
            {
                return val;
            }
            if (!string.IsNullOrEmpty(defaultValue))
            {
                WriteINI(appName, varName, defaultValue);
                return defaultValue;
            }
            return string.Empty;
        }

        /// <summary>
        /// Löscht einen INI-Eintrag.
        /// </summary>
        public bool DeleteINI(string appName, string varName)
        {
            if (!ValidateNames(appName, varName))
                return false;

            var data = Load();
            var section = string.IsNullOrEmpty(appName) ? string.Empty : appName;
            if (data.TryGetValue(section, out var sectionData) && sectionData.Remove(varName))
            {
                if (sectionData.Count == 0)
                    data.Remove(section);
                return Save(data);
            }
            return false;
        }

        private Dictionary<string, Dictionary<string, string>> Load()
        {
            var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(_filePath))
                return result;

            string currentSection = string.Empty;
            foreach (var rawLine in File.ReadAllLines(_filePath))
            {
                var line = rawLine.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith(";"))
                    continue;
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    currentSection = line.Substring(1, line.Length - 2);
                    if (!result.ContainsKey(currentSection))
                        result[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }
                else
                {
                    var idx = line.IndexOf('=');
                    if (idx <= 0) continue;
                    var name = line.Substring(0, idx);
                    var value = line.Substring(idx + 1);
                    if (!result.ContainsKey(currentSection))
                        result[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    result[currentSection][name] = value;
                }
            }
            return result;
        }

        private bool Save(Dictionary<string, Dictionary<string, string>> data)
        {
            try
            {
                using var writer = new StreamWriter(_filePath, false, Encoding.UTF8);
                // Global entries (section = empty)
                if (data.TryGetValue(string.Empty, out var global))
                {
                    foreach (var kv in global)
                        writer.WriteLine($"{kv.Key}={kv.Value}");
                    writer.WriteLine();
                }
                foreach (var section in data.Keys.Where(k => !string.IsNullOrEmpty(k)))
                {
                    writer.WriteLine($"[{section}]");
                    foreach (var kv in data[section])
                        writer.WriteLine($"{kv.Key}={kv.Value}");
                    writer.WriteLine();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool ValidateNames(string appName, string varName)
        {
            if (!string.IsNullOrEmpty(appName) && !ValidNameRegex.IsMatch(appName))
                return false;
            return ValidNameRegex.IsMatch(varName);
        }

        private bool ValidateValue(string varValue)
        {
            return varValue != null && ValidValueRegex.IsMatch(varValue);
        }
    }
}
