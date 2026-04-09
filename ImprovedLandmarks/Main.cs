using System.Collections.Generic;
using HarmonyLib;
using ModLoader;
using ModLoader.Helpers;
using SFS;
using SFS.IO;
using SFS.UI;
using SFS.WorldBase;
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
        public override string ModVersion => "1.1.0";
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

            try { patcher.PatchAll(); }
            catch (System.Exception ex) { Debug.LogError($"[ImprovedLandmarks] PatchAll failed: {ex}"); }

            try
            {
                var ordererRename = AccessTools.Method(typeof(SFS.IO.OrderedPathList), "Rename");
                if (ordererRename != null)
                {
                    var prefix = new HarmonyMethod(typeof(WorldRenameHelper), nameof(WorldRenameHelper.OrderedRenamePrefix));
                    patcher.Patch(ordererRename, prefix: prefix);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ImprovedLandmarks] Failed to patch OrderedPathList.Rename: {ex}");
            }

            try
            {
                var ordererRemove = AccessTools.Method(typeof(SFS.IO.OrderedPathList), "Remove");
                if (ordererRemove != null)
                {
                    var prefix = new HarmonyMethod(typeof(WorldRenameHelper), nameof(WorldRenameHelper.OrderedRemovePrefix));
                    patcher.Patch(ordererRemove, prefix: prefix);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ImprovedLandmarks] Failed to patch OrderedPathList.Remove: {ex}");
            }

            Debug.Log($"[ImprovedLandmarks] {ModVersion} loaded successfully.");
        }

        public override void Load()
        {
            try
            {
                LandmarkManager.Load(ModFolder);

                SceneHelper.OnWorldSceneLoaded += () =>
                {
                    LandmarkManager.SetCurrentWorld(GetCurrentWorldName());
                    LandmarkGUI.Build();
                };
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ImprovedLandmarks] Failed to load: {ex}");
            }
        }
        static string GetCurrentWorldName()
        {
            try
            {
                string name = Base.worldBase.paths.worldName;
                return string.IsNullOrEmpty(name) ? "default" : name;
            }
            catch
            {
                return "default";
            }
        }
    }
}
