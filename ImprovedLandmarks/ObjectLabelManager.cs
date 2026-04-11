using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace ImprovedLandmarks
{
    public static class ObjectLabelManager
    {
        private static HashSet<int> labels = new HashSet<int>();
        private static string _currentWorld = "default";

        public static void Init(string modFolder, string worldName)
        {
            _currentWorld = string.IsNullOrEmpty(worldName) ? "default" : worldName;
            ReloadFromLandmarkManager();
        }

        public static void ReloadFromLandmarkManager()
        {
            labels.Clear();
            var list = LandmarkManager.GetLabeledObjects(_currentWorld);
            if (list != null)
                foreach (var id in list)
                    labels.Add(id);
        }

        public static bool IsLabeled(int objectId)
        {
            return labels.Contains(objectId);
        }

        public static void SetLabel(int objectId, bool show)
        {
            if (show)
                labels.Add(objectId);
            else
                labels.Remove(objectId);
            LandmarkManager.SetLabeledObjects(_currentWorld, labels);
        }

        public static void ToggleLabel(int objectId)
        {
            SetLabel(objectId, !IsLabeled(objectId));
        }

        public static void ClearLabels()
        {
            labels.Clear();
            LandmarkManager.SetLabeledObjects(_currentWorld, labels);
        }
            public static void RemoveLabelForObject(int objectId)
            {
                if (labels.Remove(objectId))
                    LandmarkManager.SetLabeledObjects(_currentWorld, labels);
            }
    }
}
