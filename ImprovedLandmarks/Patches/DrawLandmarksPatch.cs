using HarmonyLib;
using SFS;
using SFS.Translations;
using SFS.World.Maps;
using SFS.WorldBase;
using UnityEngine;

namespace ImprovedLandmarks.Patches
{
    [HarmonyPatch(typeof(MapManager), "DrawLandmarks")]
    public static class DrawLandmarksPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            foreach (CustomLandmark lm in LandmarkManager.Landmarks)
            {
                if (!Base.planetLoader.planets.TryGetValue(lm.PlanetName, out Planet planet))
                    continue;

                double fadeRadius = planet.data.basics.radius * (2.0 + 5.0);
                float alpha = Mathf.Min(MapDrawer.GetFadeIn(Map.view.view.distance, fadeRadius * 0.5, fadeRadius * 0.4));

                if (alpha <= 0f)
                    continue;

                Color color = new Color(1f, 1f, 1f, alpha);

                Vector2 offset = new Vector2(lm.MapOffsetX, lm.MapOffsetY);
                Vector2 position = (Vector2)planet.mapHolder.position + offset;
                float rad = lm.NormalAngle * Mathf.Deg2Rad;
                Vector2 normal = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));
                MapDrawer.DrawPointWithText(lm.PinSize, color, Field.Text(lm.Name), lm.LabelSize, color, position, normal, 4, lm.LabelSpacing);
            }

            CustomLandmark preview = LandmarkGUI.PreviewLandmark;
            if (preview != null && Base.planetLoader.planets.TryGetValue(preview.PlanetName, out Planet previewPlanet))
            {
                Color previewColor = new Color(1f, 0.65f, 0f, 0.85f);
                Vector2 previewOffset = new Vector2(preview.MapOffsetX, preview.MapOffsetY);
                Vector2 previewPos = (Vector2)previewPlanet.mapHolder.position + previewOffset;
                float previewRad = preview.NormalAngle * Mathf.Deg2Rad;
                Vector2 previewNormal = new Vector2(Mathf.Sin(previewRad), Mathf.Cos(previewRad));
                MapDrawer.DrawPointWithText(preview.PinSize, previewColor, Field.Text(LandmarkGUI.CurrentInputText), preview.LabelSize, previewColor, previewPos, previewNormal, 4, preview.LabelSpacing);
            }
                    foreach (MapRocket mapObject in Object.FindObjectsByType<MapRocket>(FindObjectsSortMode.None))
                    {
                        if (mapObject == null || mapObject.rocket == null) continue;
                        int branch = mapObject.rocket.stats.branch;
                        if (branch < 0 || !ObjectLabelManager.IsLabeled(branch)) continue;

                        string displayName = string.IsNullOrEmpty(mapObject.rocket.rocketName) ? "Object" : mapObject.rocket.rocketName;
                        double zoom = Map.view.view.distance;
                        float spacing = 0.2f * (float)(1000.0 / zoom);
                        Vector2 labelPos = (Vector2)mapObject.Select_MenuPosition + new Vector2(0, spacing);
                        Planet planet = mapObject.rocket.location.planet.Value;
                        double fadeRadius = planet.data.basics.radius * (2.0 + 5.0);
                        float alpha = Mathf.Min(MapDrawer.GetFadeIn(Map.view.view.distance, fadeRadius * 0.5 * 1.5, fadeRadius * 0.4 * 1.5));
                        Color vanillaColor = new Color(1f, 1f, 1f, alpha);
                        MapDrawer.DrawPointWithText(2, vanillaColor, Field.Text(displayName), 40, vanillaColor, labelPos, Vector2.up, 4, 999);
                    }
        }
    }
}
