using SFS;
using SFS.World.Maps;
using SFS.WorldBase;
using UnityEngine;

namespace ImprovedLandmarks.Patches
{
    public class MapClickHandler : MonoBehaviour
    {
        void Start()
        {
            StartCoroutine(LandmarkGUI.PositionMainWindowCoroutine());
        }

        public static Camera LastMapCamera { get; private set; }
        public static Vector2 MapRightPerPixel { get; private set; } = Vector2.right;
        public static Vector2 MapUpPerPixel    { get; private set; } = Vector2.up;
        static bool _projectionLogged = false;

        bool _wasMapOpen = false;

        void Update()
        {
            bool mapOpen = Map.manager != null && Map.manager.mapMode.Value;

            if (mapOpen)
            {
                if (LastMapCamera == null)
                {
                    LastMapCamera = Map.view.GetComponentInChildren<Camera>(includeInactive: true);
                    if (LastMapCamera == null)
                    {
                        foreach (Camera c in Camera.allCameras)
                            if (c.isActiveAndEnabled) { LastMapCamera = c; break; }
                    }
                }
                if (LastMapCamera != null && !_projectionLogged)
                {
                    float mapPlaneZ = 0f;
                    bool gotZ = false;
                    foreach (Planet p in Base.planetLoader.planets.Values)
                    {
                        mapPlaneZ = p.mapHolder.position.z;
                        gotZ = true;
                        break;
                    }
                    if (gotZ)
                    {
                        float cx = Screen.width  * 0.5f;
                        float cy = Screen.height * 0.5f;
                        Ray rayO = LastMapCamera.ScreenPointToRay(new Vector2(cx,      cy     ));
                        Ray rayR = LastMapCamera.ScreenPointToRay(new Vector2(cx + 1f, cy     ));
                        Ray rayU = LastMapCamera.ScreenPointToRay(new Vector2(cx,      cy + 1f));
                        if (Mathf.Abs(rayO.direction.z) > 0.0001f)
                        {
                            float tO = (mapPlaneZ - rayO.origin.z) / rayO.direction.z;
                            float tR = (mapPlaneZ - rayR.origin.z) / rayR.direction.z;
                            float tU = (mapPlaneZ - rayU.origin.z) / rayU.direction.z;
                            Vector3 hitO = rayO.origin + rayO.direction * tO;
                            Vector3 hitR = rayR.origin + rayR.direction * tR;
                            Vector3 hitU = rayU.origin + rayU.direction * tU;
                            Vector2 right = new Vector2(hitR.x - hitO.x, hitR.y - hitO.y);
                            Vector2 up    = new Vector2(hitU.x - hitO.x, hitU.y - hitO.y);
                            MapRightPerPixel = right;
                            MapUpPerPixel    = up;
                            _projectionLogged = true;
                        }
                    }
                }
            }
            else
            {
                LastMapCamera = null;
                _projectionLogged = false;
            }

            if (mapOpen != _wasMapOpen)
            {
                _wasMapOpen = mapOpen;
                if (!mapOpen && LandmarkGUI.PlacementMode)
                    LandmarkGUI.CancelPlacement();
                LandmarkGUI.RebuildMainWindowPublic();
            }

            if (!LandmarkGUI.PlacementMode)
                return;

            if (LandmarkGUI.DialogOpen)
                return;

            if (Map.manager == null || !Map.manager.mapMode.Value)
                return;

            if (!Input.GetMouseButtonDown(0))
                return;

            if (LandmarkGUI.IsMouseOverAnyWindow())
                return;

            Camera mapCamera = Map.view.GetComponentInChildren<Camera>(includeInactive: true);
            if (mapCamera == null)
            {
                foreach (Camera cam in Camera.allCameras)
                {
                    if (cam.isActiveAndEnabled)
                    {
                        mapCamera = cam;
                        break;
                    }
                }
            }
            if (mapCamera == null)
                mapCamera = Camera.main;
            if (mapCamera == null)
                return;

            Vector2 mapPos;
            if (mapCamera.orthographic)
            {
                Vector3 worldPos = mapCamera.ScreenToWorldPoint(
                    new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f)
                );
                mapPos = new Vector2(worldPos.x, worldPos.y);
            }
            else
            {
                float mapPlaneZ = 0f;
                foreach (Planet p in Base.planetLoader.planets.Values)
                {
                    mapPlaneZ = p.mapHolder.position.z;
                    break;
                }
                Ray ray = mapCamera.ScreenPointToRay(Input.mousePosition);
                if (Mathf.Abs(ray.direction.z) < 0.0001f)
                    return;
                float t = (mapPlaneZ - ray.origin.z) / ray.direction.z;
                Vector3 hit = ray.origin + ray.direction * t;
                mapPos = new Vector2(hit.x, hit.y);
            }

            Planet closestPlanet = null;
            float closestDist = float.MaxValue;

            foreach (Planet planet in Base.planetLoader.planets.Values)
            {
                float dist = Vector2.Distance(mapPos, (Vector2)planet.mapHolder.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestPlanet = planet;
                }
            }

            if (closestPlanet == null)
                return;

            Vector2 relative = mapPos - (Vector2)closestPlanet.mapHolder.position;
            LandmarkGUI.ShowNamingDialog(closestPlanet.codeName, relative.x, relative.y);
        }
    }
}
