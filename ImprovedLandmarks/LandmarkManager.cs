using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace ImprovedLandmarks
{
    public static class LandmarkManager
    {
        static Dictionary<string, List<CustomLandmark>> _allLandmarks = new Dictionary<string, List<CustomLandmark>>();
        static string _currentWorld = "";

        public static List<CustomLandmark> Landmarks
        {
            get
            {
                if (string.IsNullOrEmpty(_currentWorld))
                    return new List<CustomLandmark>();
                if (!_allLandmarks.ContainsKey(_currentWorld))
                    _allLandmarks[_currentWorld] = new List<CustomLandmark>();
                return _allLandmarks[_currentWorld];
            }
        }

        public static string CurrentWorld => _currentWorld;

        static string _savePath;
        static string _configPath;

        public static void Load(string modFolder)
        {
            _savePath = Path.Combine(modFolder, "landmarks.json");
            _configPath = Path.Combine(modFolder, "Config.ini");

            if (!File.Exists(_savePath))
                return;

            string json = File.ReadAllText(_savePath);

            try
            {
                var loaded = JsonConvert.DeserializeObject<Dictionary<string, List<CustomLandmark>>>(json);
                if (loaded != null)
                {
                    _allLandmarks = loaded;
                    return;
                }
            }
            catch { }

            try
            {
                var legacy = JsonConvert.DeserializeObject<List<CustomLandmark>>(json);
                if (legacy != null && legacy.Count > 0)
                {
                    _allLandmarks["default"] = legacy;
                    SaveAll();
                }
            }
            catch { }
        }

        public static void SetCurrentWorld(string worldName)
        {
            _currentWorld = string.IsNullOrEmpty(worldName) ? "default" : worldName;
            if (!_allLandmarks.ContainsKey(_currentWorld))
                _allLandmarks[_currentWorld] = new List<CustomLandmark>();
        }

        static void SaveAll()
        {
            if (_savePath == null)
                return;

            // Remove empty or keyless entries before saving
            var toRemove = new List<string>();
            foreach (var kv in _allLandmarks)
                if (string.IsNullOrEmpty(kv.Key) || kv.Value.Count == 0)
                    toRemove.Add(kv.Key);
            foreach (var key in toRemove)
                _allLandmarks.Remove(key);

            string json = JsonConvert.SerializeObject(_allLandmarks, Formatting.Indented);
            File.WriteAllText(_savePath, json);
        }

        public static void Save() => SaveAll();

        public static void RemoveWorld(string worldName)
        {
            if (string.IsNullOrEmpty(worldName))
                return;
            _allLandmarks.Remove(worldName);
            SaveAll();
        }

        public static void RenameWorld(string oldName, string newName)
        {
            if (string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName))
                return;
            if (!_allLandmarks.ContainsKey(oldName))
                return;
            _allLandmarks[newName] = _allLandmarks[oldName];
            _allLandmarks.Remove(oldName);
            if (_currentWorld == oldName)
                _currentWorld = newName;
            SaveAll();
        }

        public static void Add(CustomLandmark landmark)
        {
            Landmarks.Add(landmark);
            SaveAll();
        }

        public static void Remove(CustomLandmark landmark)
        {
            Landmarks.Remove(landmark);
            SaveAll();
        }

        public static Vector2 LoadGuiPosition(Vector2 defaultPos)
        {
            if (_configPath == null || !File.Exists(_configPath))
                return defaultPos;
            try
            {
                foreach (string line in File.ReadAllLines(_configPath))
                {
                    string trimmed = line.Trim();
                    if (trimmed.StartsWith("PosGUI="))
                    {
                        string[] parts = trimmed.Substring(7).Split(',');
                        if (parts.Length == 2
                            && float.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float x)
                            && float.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float y))
                            return new Vector2(x, y);
                    }
                }
            }
            catch { }
            return defaultPos;
        }

        public static bool ConfigExists => _configPath != null && File.Exists(_configPath);

        public static void SaveGuiPosition(Vector2 pos)
        {
            if (_configPath == null) return;

            string newEntry = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "PosGUI={0},{1}", pos.x, pos.y);

            List<string> lines = new List<string>();
            bool found = false;

            if (File.Exists(_configPath))
            {
                foreach (string line in File.ReadAllLines(_configPath))
                {
                    if (line.TrimStart().StartsWith("PosGUI="))
                    {
                        lines.Add(newEntry);
                        found = true;
                    }
                    else
                    {
                        lines.Add(line);
                    }
                }
            }

            if (!found)
                lines.Add(newEntry);

            File.WriteAllLines(_configPath, lines);
        }
    }
}
