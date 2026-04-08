using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace ImprovedLandmarks
{
    public static class LandmarkManager
    {
        public static List<CustomLandmark> Landmarks { get; } = new List<CustomLandmark>();

        static string _savePath;
        static string _configPath;

        public static void Load(string modFolder)
        {
            _savePath = Path.Combine(modFolder, "landmarks.json");
            _configPath = Path.Combine(modFolder, "Config.ini");

            if (!File.Exists(_savePath))
                return;

            string json = File.ReadAllText(_savePath);
            var loaded = JsonConvert.DeserializeObject<List<CustomLandmark>>(json);
            if (loaded != null)
            {
                Landmarks.Clear();
                Landmarks.AddRange(loaded);
            }
        }

        public static void Save()
        {
            if (_savePath == null)
                return;

            string json = JsonConvert.SerializeObject(Landmarks, Formatting.Indented);
            File.WriteAllText(_savePath, json);
        }

        public static void Add(CustomLandmark landmark)
        {
            Landmarks.Add(landmark);
            Save();
        }

        public static void Remove(CustomLandmark landmark)
        {
            Landmarks.Remove(landmark);
            Save();
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
