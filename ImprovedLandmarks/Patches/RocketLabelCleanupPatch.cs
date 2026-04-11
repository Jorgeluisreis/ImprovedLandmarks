using HarmonyLib;
using SFS.World;
using ImprovedLandmarks;

namespace ImprovedLandmarks.Patches
{
    [HarmonyPatch(typeof(RocketManager), nameof(RocketManager.DestroyRocket))]
    public static class RocketLabelCleanupPatch
    {
        [HarmonyPrefix]
        public static void Prefix(Rocket rocket)
        {
            if (rocket != null && rocket.stats != null)
            {
                ObjectLabelManager.RemoveLabelForObject(rocket.stats.branch);
            }
        }
    }
}
