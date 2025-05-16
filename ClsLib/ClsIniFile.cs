using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ClsLib
{
    /// <summary>
    /// Klasse zum Lesen und Schreiben von INI-Dateien.
    /// </summary>
    public class ClsIniFile
    {
        private readonly string _filePath;

        public ClsIniFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Dateipfad darf nicht leer sein.", nameof(filePath));
            _filePath = filePath;
        }

        /// <summary>
        /// Schreibt oder aktualisiert einen INI-Eintrag.
        /// Legt Verzeichnis und Datei an, falls nötig, ohne Validierungsabbruch.
        /// Konvertiert AppName und VarName in Großbuchstaben.
        /// </summary>
        public bool WriteINI(string appName, string varName, string varValue)
        {
            // Namen in Großbuchstaben konvertieren
            var sectionName = string.IsNullOrEmpty(appName)
                ? string.Empty
                : appName.ToUpperInvariant();
            var keyName = varName?.ToUpperInvariant() ?? string.Empty;

            // Verzeichnis sicherstellen
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            // Datei anlegen, falls sie fehlt
            if (!File.Exists(_filePath))
                File.WriteAllText(_filePath, string.Empty);

            // Daten laden, Eintrag schreiben und speichern
            var data = Load();
            if (!data.ContainsKey(sectionName))
                data[sectionName] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            data[sectionName][keyName] = varValue;

            return Save(data);
        }

        /// <summary>
        /// Liest einen INI-Eintrag. Gibt defaultValue zurück, wenn nicht gefunden und defaultValue nicht leer.
        /// Legt bei fehlender Datei und nicht-leerem defaultValue Datei und Verzeichnis an.
        /// Konvertiert AppName und VarName in Großbuchstaben.
        /// </summary>
        public string ReadINI(string appName, string varName, string defaultValue)
        {
            // Namen in Großbuchstaben konvertieren
            var sectionName = string.IsNullOrEmpty(appName)
                ? string.Empty
                : appName.ToUpperInvariant();
            var keyName = varName?.ToUpperInvariant() ?? string.Empty;

            // Datei- und Verzeichnisanlage bei fehlender Datei
            if (!File.Exists(_filePath))
            {
                if (!string.IsNullOrEmpty(defaultValue))
                {
                    WriteINI(sectionName, keyName, defaultValue);
                    return defaultValue;
                }
                return string.Empty;
            }

            // INI-Datei existiert: Wert auslesen
            var data = Load();
            if (data.TryGetValue(sectionName, out var sectionData) && sectionData.TryGetValue(keyName, out var val))
                return val;

            if (!string.IsNullOrEmpty(defaultValue))
            {
                WriteINI(sectionName, keyName, defaultValue);
                return defaultValue;
            }
            return string.Empty;
        }

        /// <summary>
        /// Löscht einen INI-Eintrag.
        /// Konvertiert AppName und VarName in Großbuchstaben.
        /// </summary>
        public bool DeleteINI(string appName, string varName)
        {
            // Namen in Großbuchstaben konvertieren
            var sectionName = string.IsNullOrEmpty(appName)
                ? string.Empty
                : appName.ToUpperInvariant();
            var keyName = varName?.ToUpperInvariant() ?? string.Empty;

            var data = Load();
            if (data.TryGetValue(sectionName, out var sectionData) && sectionData.Remove(keyName))
            {
                if (sectionData.Count == 0)
                    data.Remove(sectionName);
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
    }
}
