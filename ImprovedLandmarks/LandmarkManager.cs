using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace ImprovedLandmarks
{
    public class WorldData
    {
        public List<CustomLandmark> landmarks = new List<CustomLandmark>();
        public List<int> labeledObjects = new List<int>();
    }

    public static class LandmarkManager
    {
        public static List<int> GetLabeledObjects(string world)
        {
            if (string.IsNullOrEmpty(world)) world = "default";
            if (!_worlds.ContainsKey(world)) return new List<int>();
            return new List<int>(_worlds[world].labeledObjects);
        }

        public static void SetLabeledObjects(string world, IEnumerable<int> objects)
        {
            if (string.IsNullOrEmpty(world)) world = "default";
            if (!_worlds.ContainsKey(world)) _worlds[world] = new WorldData();
            _worlds[world].labeledObjects = new List<int>(objects);
            SaveAll();
        }
        static Dictionary<string, WorldData> _worlds = new Dictionary<string, WorldData>();
        static string _currentWorld = "";

        public static List<CustomLandmark> Landmarks
        {
            get
            {
                if (string.IsNullOrEmpty(_currentWorld))
                    return new List<CustomLandmark>();
                if (!_worlds.ContainsKey(_currentWorld))
                    _worlds[_currentWorld] = new WorldData();
                return _worlds[_currentWorld].landmarks;
            }
        }

        public static string CurrentWorld => _currentWorld;

        static string _savePath;
        static string _configPath;
        static string _objectLabelsPath;

        public static void Load(string modFolder)
        {
            _savePath = Path.Combine(modFolder, "landmarks.json");
            _configPath = Path.Combine(modFolder, "Config.ini");

            if (!File.Exists(_savePath))
                return;

            string json = File.ReadAllText(_savePath);
            bool upgraded = false;
            try
            {
                var loaded = JsonConvert.DeserializeObject<Dictionary<string, WorldData>>(json);
                if (loaded != null)
                {
                    _worlds = loaded;
                    upgraded = true;
                }
            }
            catch { }

            if (!upgraded)
            {
                try
                {
                    var loaded = JsonConvert.DeserializeObject<Dictionary<string, List<CustomLandmark>>>(json);
                    if (loaded != null)
                    {
                        foreach (var kv in loaded)
                        {
                            if (!_worlds.ContainsKey(kv.Key))
                                _worlds[kv.Key] = new WorldData();
                            _worlds[kv.Key].landmarks = kv.Value;
                        }
                        upgraded = true;
                        SaveAll();
                    }
                }
                catch { }
            }

            if (!upgraded)
            {
                try
                {
                    var legacy = JsonConvert.DeserializeObject<List<CustomLandmark>>(json);
                    if (legacy != null && legacy.Count > 0)
                    {
                        _worlds["default"] = new WorldData { landmarks = legacy };
                        SaveAll();
                    }
                }
                catch { }
            }
        }

        public static void SetCurrentWorld(string worldName)
        {
            _currentWorld = string.IsNullOrEmpty(worldName) ? "default" : worldName;
            if (!_worlds.ContainsKey(_currentWorld))
                _worlds[_currentWorld] = new WorldData();
        }

        static void SaveAll()
        {
            if (_savePath == null)
                return;

            var toRemove = new List<string>();
            foreach (var kv in _worlds)
                if (string.IsNullOrEmpty(kv.Key) || (kv.Value.landmarks.Count == 0 && kv.Value.labeledObjects.Count == 0))
                    toRemove.Add(kv.Key);
            foreach (var key in toRemove)
                _worlds.Remove(key);

            string json = JsonConvert.SerializeObject(_worlds, Formatting.Indented);
            File.WriteAllText(_savePath, json);
        }

        public static void Save() => SaveAll();

        public static void RemoveWorld(string worldName)
        {
            if (string.IsNullOrEmpty(worldName))
                return;
            _worlds.Remove(worldName);
            SaveAll();
        }

        public static void RenameWorld(string oldName, string newName)
        {
            if (string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName))
                return;
            if (_worlds.ContainsKey(oldName))
            {
                _worlds[newName] = _worlds[oldName];
                _worlds.Remove(oldName);
            }
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

        public static bool IsObjectLabeled(int branch)
        {
            if (string.IsNullOrEmpty(_currentWorld)) return false;
            if (!_worlds.TryGetValue(_currentWorld, out var data)) return false;
            return data.labeledObjects.Contains(branch);
        }

        public static void ToggleObjectLabel(int branch)
        {
            if (string.IsNullOrEmpty(_currentWorld)) return;
            if (!_worlds.ContainsKey(_currentWorld))
                _worlds[_currentWorld] = new WorldData();
            var list = _worlds[_currentWorld].labeledObjects;
            if (!list.Remove(branch))
                list.Add(branch);
            SaveAll();
        }

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
