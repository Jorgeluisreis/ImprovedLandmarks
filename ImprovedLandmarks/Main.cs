using System.Collections.Generic;
using HarmonyLib;
using ModLoader;
using ModLoader.Helpers;
using SFS.IO;
using UITools;
using UnityEngine;

namespace ImprovedLandmarks
{
    public class Main : Mod, IUpdatable
    {
        public override string ModNameID => "improved_landmarks";
        public override string DisplayName => "Improved Landmarks";
        public override string Author => "Jorgeluisreis";
        public override string MinimumGameVersionNecessary => "1.5.9.8";
        public override string ModVersion => "1.0.0";
        public override string Description => "Allows creating custom landmarks anywhere on the map.";
        public override string IconLink => "https://raw.githubusercontent.com/Jorgeluisreis/ImprovedLandmarks/refs/heads/main/Images/Icon.png";

        public override Dictionary<string, string> Dependencies => new Dictionary<string, string>
        {
            { "UITools", "1.1.5" },
        };

        public Dictionary<string, FilePath> UpdatableFiles => new Dictionary<string, FilePath>
        {
            {
                "https://github.com/Jorgeluisreis/ImprovedLandmarks/releases/latest/download/ImprovedLandmarks.dll",
                new FolderPath(ModFolder).ExtendToFile("ImprovedLandmarks.dll")
            },
        };

        static Harmony patcher;

        public override void Early_Load()
        {
            patcher = new Harmony("improved.landmarks.mod");
            patcher.PatchAll();
            Debug.Log($"[ImprovedLandmarks] {ModVersion} loaded successfully.");
        }

        public override void Load()
        {
            try
            {
                LandmarkManager.Load(ModFolder);

                SceneHelper.OnWorldSceneLoaded += () =>
                {
                    LandmarkGUI.Build();
                };
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ImprovedLandmarks] Failed to load: {ex}");
            }
        }
    }
}
