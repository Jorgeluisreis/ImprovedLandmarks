using HarmonyLib;
using UnityEngine;

namespace ImprovedLandmarks
{
    public static class WorldRenameHelper
    {
        public static void OrderedRenamePrefix(string __0, string __1)
        {
            LandmarkManager.RenameWorld(__0, __1);
        }

        public static void OrderedRemovePrefix(string __0)
        {
            LandmarkManager.RemoveWorld(__0);
        }
    }
}

