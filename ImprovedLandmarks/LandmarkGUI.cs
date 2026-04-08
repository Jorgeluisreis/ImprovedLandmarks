using ImprovedLandmarks.Patches;
using SFS;
using SFS.UI;
using SFS.UI.ModGUI;
using System.Collections;
using Type = SFS.UI.ModGUI.Type;
using UnityEngine;
using UnityEngine.UI;

namespace ImprovedLandmarks
{
    public static class LandmarkGUI
    {

        static GameObject _mainHolder;
        static GameObject _dialogHolder;
        static GameObject _confirmHolder;

        static readonly int MainWindowID = Builder.GetRandomID();
        static readonly int DialogWindowID = Builder.GetRandomID();
        static readonly int ConfirmWindowID = Builder.GetRandomID();

        static RectTransform _mainWinRT;
        static RectTransform _dialogWinRT;
        static RectTransform _confirmWinRT;

        static Vector2? _savedWindowPos;
        static TextInput _nameInput;
        static SFS.UI.ModGUI.Button _toggleButton;

        static bool _expanded = false;
        public static bool PlacementMode { get; private set; }
        public static bool DialogOpen { get; private set; }
        public static CustomLandmark PreviewLandmark { get; private set; }

        public static string CurrentInputText
        {
            get
            {
                if (_nameInput != null && !string.IsNullOrEmpty(_nameInput.Text))
                    return _nameInput.Text;
                return PreviewLandmark?.Name ?? "?";
            }
        }



        static CustomLandmark _editTarget;
        static CustomLandmark _deleteTarget;

        static string _pendingPlanet;
        static float _pendingOffsetX;
        static float _pendingOffsetY;
        static float _pendingNormalAngle;
        static int _pendingPinSize;
        static int _pendingLabelSize;
        static int _pendingLabelSpacing;

        static float _editTargetOriginalAngle;
        static float _editTargetOriginalOffsetX;
        static float _editTargetOriginalOffsetY;
        static int _editTargetOriginalPinSize;
        static int _editTargetOriginalLabelSize;
        static int _editTargetOriginalLabelSpacing;

        static Vector2 _mapRightPerPixel = Vector2.right;
        static Vector2 _mapUpPerPixel = Vector2.up;
        static float _editMoveStep = 15f;



        static void GetCanvasDimensions(out float width, out float height)
        {
            foreach (Canvas c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (!c.isRootCanvas) continue;
                RectTransform rt = c.transform as RectTransform;
                if (rt != null && rt.rect.width > 100f)
                {
                    width = rt.rect.width;
                    height = rt.rect.height;
                    return;
                }
            }
            width = 2844f;
            height = 1600f;
        }

        public static IEnumerator PositionMainWindowCoroutine()
        {
            yield return null;
            ApplyMainWindowPosition();
        }

        public static void ApplyMainWindowPosition()
        {
            if (_mainWinRT == null) return;
            GetCanvasDimensions(out float canvasW, out float canvasH);
            float defaultX = canvasW * 0.5f - 5f - 340f * 0.5f;
            float defaultY = canvasH * 0.5f - 70f;
            Vector2 defaultPos = new Vector2(defaultX, defaultY);

            Vector2 pos;
            if (_savedWindowPos.HasValue)
            {
                pos = _savedWindowPos.Value;
            }
            else
            {
                Vector2 loaded = LandmarkManager.LoadGuiPosition(defaultPos);
                if (Mathf.Abs(loaded.x) <= 2f && Mathf.Abs(loaded.y) <= 2f)
                    pos = new Vector2(loaded.x * canvasW, loaded.y * canvasH);
                else
                    pos = loaded;

                if (!LandmarkManager.ConfigExists)
                    SaveWindowPosition(pos);
                else
                    _savedWindowPos = pos;
            }

            float hw = _mainWinRT.sizeDelta.x * 0.5f;
            pos.x = Mathf.Clamp(pos.x, -canvasW * 0.5f + hw, canvasW * 0.5f - hw);
            pos.y = Mathf.Clamp(pos.y, -canvasH * 0.5f, canvasH * 0.5f);
            _mainWinRT.anchoredPosition = pos;
            _savedWindowPos = pos;
        }

        static void SaveWindowPosition(Vector2 pos)
        {
            _savedWindowPos = pos;
            GetCanvasDimensions(out float canvasW, out float canvasH);
            LandmarkManager.SaveGuiPosition(new Vector2(pos.x / canvasW, pos.y / canvasH));
        }

        public static void Build()
        {
            PlacementMode = false;
            DialogOpen = false;
            PreviewLandmark = null;
            _editTarget = null;
            _deleteTarget = null;
            _nameInput = null;
            _toggleButton = null;

            _mainWinRT = null;
            _dialogWinRT = null;
            _confirmWinRT = null;

            _mainHolder = null;
            _dialogHolder = null;
            _confirmHolder = null;

            RebuildMainWindow();
            GameObject handlerObj = Builder.CreateHolder(Builder.SceneToAttach.CurrentScene, "LM_ClickHandlerHolder");
            handlerObj.AddComponent<MapClickHandler>();
        }

        static void RebuildMainWindow()
        {
            if (_mainHolder != null)
                Object.Destroy(_mainHolder);

            _mainHolder = Builder.CreateHolder(Builder.SceneToAttach.CurrentScene, "LM_MainHolder");

            int count = _expanded ? LandmarkManager.Landmarks.Count : 0;
            bool mapOpen = SFS.World.Maps.Map.manager != null && SFS.World.Maps.Map.manager.mapMode.Value;
            int windowHeight = _expanded ? (165 + count * 65) : 110;
            if (mapOpen) windowHeight += 58;

            Window win = Builder.CreateWindow(_mainHolder.transform, MainWindowID, 340, windowHeight, 20, 20, true, false, 0.95f, "Landmarks");
            _mainWinRT = win.gameObject.GetComponent<RectTransform>();
            win.CreateLayoutGroup(Type.Vertical, spacing: 8);

            if (mapOpen)
            {
                _toggleButton = Builder.CreateButton(win, 310, 50, onClick: OnTogglePlacement) as SFS.UI.ModGUI.Button;
                _toggleButton.Text = PlacementMode ? "Cancel" : "+ Landmark";
            }
            else
            {
                _toggleButton = null;
                if (PlacementMode) PlacementMode = false;
            }

            SFS.UI.ModGUI.Button expandBtn = Builder.CreateButton(win, 310, 50, onClick: OnToggleExpand) as SFS.UI.ModGUI.Button;
            expandBtn.Text = _expanded ? "Hide list" : "Show list";

            if (_expanded)
            {
                for (int i = 0; i < LandmarkManager.Landmarks.Count; i++)
                {
                    CustomLandmark lm = LandmarkManager.Landmarks[i];
                    bool planetExists = Base.planetLoader.planets.ContainsKey(lm.PlanetName);
                    Container row = Builder.CreateContainer(win);
                    row.CreateLayoutGroup(Type.Horizontal, childAlignment: TextAnchor.MiddleCenter, spacing: 5);
                    Builder.CreateLabel(row, planetExists ? 165 : 240, 50, text: lm.Name);
                    if (planetExists)
                    {
                        SFS.UI.ModGUI.Button editBtn = Builder.CreateButton(row, 75, 50, onClick: () => OpenRenameDialog(lm)) as SFS.UI.ModGUI.Button;
                        editBtn.Text = "Edit";
                    }
                    SFS.UI.ModGUI.Button delBtn = Builder.CreateButton(row, 55, 50, onClick: () => DeleteLandmark(lm)) as SFS.UI.ModGUI.Button;
                    delBtn.Text = "Del";
                }
            }

            ApplyMainWindowPosition();
            _mainHolder.AddComponent<WindowPositionTracker>();
        }

        static void OnToggleExpand()
        {
            _expanded = !_expanded;
            RebuildMainWindow();
        }

        static void OnTogglePlacement()
        {
            PlacementMode = !PlacementMode;
            _toggleButton.Text = PlacementMode ? "Cancel" : "+ Landmark";
            MsgDrawer.main.Log(PlacementMode
                ? "Click on the map to place a landmark."
                : "Placement cancelled.");
        }

        public static void CancelPlacement()
        {
            PlacementMode = false;
            if (!DialogOpen)
                PreviewLandmark = null;
        }

        public static void RebuildMainWindowPublic() => RebuildMainWindow();

        public static bool IsMouseOverAnyWindow()
        {
            return IsOverRT(_mainWinRT) || IsOverRT(_dialogWinRT) || IsOverRT(_confirmWinRT);
        }

        static bool IsOverRT(RectTransform rt)
        {
            if (rt == null) return false;
            Canvas canvas = rt.GetComponentInParent<Canvas>();
            Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;
            return RectTransformUtility.RectangleContainsScreenPoint(rt, Input.mousePosition, cam);
        }



        static void DeleteLandmark(CustomLandmark lm)
        {
            _deleteTarget = lm;
            if (_confirmHolder != null)
                Object.Destroy(_confirmHolder);

            _confirmHolder = Builder.CreateHolder(Builder.SceneToAttach.CurrentScene, "LM_ConfirmHolder");
            DialogOpen = true;

            Window confirm = Builder.CreateWindow(_confirmHolder.transform, ConfirmWindowID, 340, 250, 0, 0, true, true, 0.95f, "Delete Landmark");
            _confirmWinRT = confirm.gameObject.GetComponent<RectTransform>();
            confirm.CreateLayoutGroup(Type.Vertical, spacing: 8);

            Builder.CreateLabel(confirm, 310, 50, text: $"Delete \"{lm.Name}\"?");

            SFS.UI.ModGUI.Button yesBtn = Builder.CreateButton(confirm, 310, 50, onClick: OnConfirmDelete) as SFS.UI.ModGUI.Button;
            yesBtn.Text = "Yes, delete";

            SFS.UI.ModGUI.Button noBtn = Builder.CreateButton(confirm, 310, 50, onClick: OnCancelDelete) as SFS.UI.ModGUI.Button;
            noBtn.Text = "Cancel";
        }

        static void OnConfirmDelete()
        {
            if (_deleteTarget != null)
            {
                LandmarkManager.Remove(_deleteTarget);
                _deleteTarget = null;
            }
            if (_confirmHolder != null)
            {
                Object.Destroy(_confirmHolder);
                _confirmHolder = null;
                _confirmWinRT = null;
            }
            DialogOpen = false;
            RebuildMainWindow();
        }

        static void OnCancelDelete()
        {
            _deleteTarget = null;
            if (_confirmHolder != null)
            {
                Object.Destroy(_confirmHolder);
                _confirmHolder = null;
                _confirmWinRT = null;
            }
            DialogOpen = false;
        }



        public static void ShowNamingDialog(string planetName, float offsetX, float offsetY)
        {
            _pendingPlanet = planetName;
            _pendingOffsetX = offsetX;
            _pendingOffsetY = offsetY;
            _pendingNormalAngle = 90f;
            _pendingPinSize = 15;
            _pendingLabelSize = 40;
            _pendingLabelSpacing = 4;
            _editTarget = null;
            PreviewLandmark = new CustomLandmark
            {
                Name = "?",
                PlanetName = planetName,
                MapOffsetX = offsetX,
                MapOffsetY = offsetY,
                NormalAngle = 90f,
                PinSize = 15,
                LabelSize = 40,
                LabelSpacing = 4
            };
            OpenDialog("New Landmark", "");
        }

        static void OpenRenameDialog(CustomLandmark lm)
        {
            _editTarget = lm;
            _pendingPlanet = lm.PlanetName;
            _editMoveStep = 15f;

            _mapRightPerPixel = MapClickHandler.MapRightPerPixel;
            _mapUpPerPixel = MapClickHandler.MapUpPerPixel;

            _editTargetOriginalAngle = lm.NormalAngle;
            _editTargetOriginalPinSize = lm.PinSize;
            _editTargetOriginalLabelSize = lm.LabelSize;
            _editTargetOriginalLabelSpacing = lm.LabelSpacing;
            _editTargetOriginalOffsetX = lm.MapOffsetX;
            _editTargetOriginalOffsetY = lm.MapOffsetY;

            _pendingNormalAngle = lm.NormalAngle;
            _pendingPinSize = lm.PinSize;
            _pendingLabelSize = lm.LabelSize;
            _pendingLabelSpacing = lm.LabelSpacing;
            _pendingOffsetX = lm.MapOffsetX;
            _pendingOffsetY = lm.MapOffsetY;

            PreviewLandmark = null;
            OpenDialog("Edit Landmark", lm.Name);
        }

        static void OpenDialog(string title, string initialText)
        {
            if (_dialogHolder != null)
                Object.Destroy(_dialogHolder);

            _dialogHolder = Builder.CreateHolder(Builder.SceneToAttach.CurrentScene, "LM_DialogHolder");
            DialogOpen = true;

            Window dialog = Builder.CreateWindow(_dialogHolder.transform, DialogWindowID, 340, (_editTarget != null ? 760 : 570), 0, 0, true, true, 0.95f, title);
            _dialogWinRT = dialog.gameObject.GetComponent<RectTransform>();
            dialog.CreateLayoutGroup(Type.Vertical, spacing: 8);

            _nameInput = Builder.CreateTextInput(dialog, 310, 50, 0, 0, "Landmark name...");
            if (!string.IsNullOrEmpty(initialText))
                _nameInput.Text = initialText;

            Builder.CreateLabel(dialog, 310, 40, text: $"Direction: {GetDirectionArrow(_pendingNormalAngle)} ({(int)_pendingNormalAngle}°)");

            Container rotRow = Builder.CreateContainer(dialog);
            rotRow.CreateLayoutGroup(Type.Horizontal, childAlignment: TextAnchor.MiddleCenter, spacing: 5);
            SFS.UI.ModGUI.Button rotLeftBtn = Builder.CreateButton(rotRow, 145, 50, onClick: OnRotateLeft) as SFS.UI.ModGUI.Button;
            rotLeftBtn.Text = "< Rotate";
            SFS.UI.ModGUI.Button rotRightBtn = Builder.CreateButton(rotRow, 145, 50, onClick: OnRotateRight) as SFS.UI.ModGUI.Button;
            rotRightBtn.Text = "Rotate >";

            Container pinRow = Builder.CreateContainer(dialog);
            pinRow.CreateLayoutGroup(Type.Horizontal, childAlignment: TextAnchor.MiddleCenter, spacing: 5);
            Builder.CreateLabel(pinRow, 140, 50, text: $"Pin: {_pendingPinSize}");
            SFS.UI.ModGUI.Button pinDecBtn = Builder.CreateButton(pinRow, 75, 50, onClick: OnPinDec) as SFS.UI.ModGUI.Button;
            pinDecBtn.Text = "−";
            SFS.UI.ModGUI.Button pinIncBtn = Builder.CreateButton(pinRow, 75, 50, onClick: OnPinInc) as SFS.UI.ModGUI.Button;
            pinIncBtn.Text = "+";

            Container labelSizeRow = Builder.CreateContainer(dialog);
            labelSizeRow.CreateLayoutGroup(Type.Horizontal, childAlignment: TextAnchor.MiddleCenter, spacing: 5);
            Builder.CreateLabel(labelSizeRow, 140, 50, text: $"Text size: {_pendingLabelSize}");
            SFS.UI.ModGUI.Button lblDecBtn = Builder.CreateButton(labelSizeRow, 75, 50, onClick: OnLabelSizeDec) as SFS.UI.ModGUI.Button;
            lblDecBtn.Text = "−";
            SFS.UI.ModGUI.Button lblIncBtn = Builder.CreateButton(labelSizeRow, 75, 50, onClick: OnLabelSizeInc) as SFS.UI.ModGUI.Button;
            lblIncBtn.Text = "+";

            Container spacingRow = Builder.CreateContainer(dialog);
            spacingRow.CreateLayoutGroup(Type.Horizontal, childAlignment: TextAnchor.MiddleCenter, spacing: 5);
            Builder.CreateLabel(spacingRow, 140, 50, text: $"Height: {_pendingLabelSpacing}");
            SFS.UI.ModGUI.Button spDecBtn = Builder.CreateButton(spacingRow, 75, 50, onClick: OnSpacingDec) as SFS.UI.ModGUI.Button;
            spDecBtn.Text = "−";
            SFS.UI.ModGUI.Button spIncBtn = Builder.CreateButton(spacingRow, 75, 50, onClick: OnSpacingInc) as SFS.UI.ModGUI.Button;
            spIncBtn.Text = "+";

            if (_editTarget != null)
            {
                Container stepRow = Builder.CreateContainer(dialog);
                stepRow.CreateLayoutGroup(Type.Horizontal, childAlignment: TextAnchor.MiddleCenter, spacing: 5);
                Builder.CreateLabel(stepRow, 140, 50, text: $"Step: {_editMoveStep:F0}px");
                SFS.UI.ModGUI.Button stDecBtn = Builder.CreateButton(stepRow, 75, 50, onClick: OnStepDec) as SFS.UI.ModGUI.Button;
                stDecBtn.Text = "÷10";
                SFS.UI.ModGUI.Button stIncBtn = Builder.CreateButton(stepRow, 75, 50, onClick: OnStepInc) as SFS.UI.ModGUI.Button;
                stIncBtn.Text = "×10";

                Container xRow = Builder.CreateContainer(dialog);
                xRow.CreateLayoutGroup(Type.Horizontal, childAlignment: TextAnchor.MiddleCenter, spacing: 5);
                Builder.CreateLabel(xRow, 140, 50, text: $"X: {_pendingOffsetX:F0}");
                SFS.UI.ModGUI.Button xDecBtn = Builder.CreateButton(xRow, 75, 50, onClick: OnOffsetXDec) as SFS.UI.ModGUI.Button;
                xDecBtn.Text = "←";
                SFS.UI.ModGUI.Button xIncBtn = Builder.CreateButton(xRow, 75, 50, onClick: OnOffsetXInc) as SFS.UI.ModGUI.Button;
                xIncBtn.Text = "→";

                Container yRow = Builder.CreateContainer(dialog);
                yRow.CreateLayoutGroup(Type.Horizontal, childAlignment: TextAnchor.MiddleCenter, spacing: 5);
                Builder.CreateLabel(yRow, 140, 50, text: $"Y: {_pendingOffsetY:F0}");
                SFS.UI.ModGUI.Button yDecBtn = Builder.CreateButton(yRow, 75, 50, onClick: OnOffsetYDec) as SFS.UI.ModGUI.Button;
                yDecBtn.Text = "↓";
                SFS.UI.ModGUI.Button yIncBtn = Builder.CreateButton(yRow, 75, 50, onClick: OnOffsetYInc) as SFS.UI.ModGUI.Button;
                yIncBtn.Text = "↑";
            }

            SFS.UI.ModGUI.Button confirmBtn = Builder.CreateButton(dialog, 310, 50, onClick: OnDialogConfirm) as SFS.UI.ModGUI.Button;
            confirmBtn.Text = "Confirm";

            SFS.UI.ModGUI.Button cancelBtn = Builder.CreateButton(dialog, 310, 50, onClick: OnDialogCancel) as SFS.UI.ModGUI.Button;
            cancelBtn.Text = "Cancel";
        }

        static void CloseDialog()
        {
            if (_dialogHolder != null)
            {
                Object.Destroy(_dialogHolder);
                _dialogHolder = null;
                _dialogWinRT = null;
            }
            DialogOpen = false;
        }

        static void OnDialogConfirm()
        {
            string name = _nameInput?.Text?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MsgDrawer.main.Log("Please enter a name.");
                return;
            }

            if (_editTarget != null)
            {
                _editTarget.Name = name;
                _editTarget.NormalAngle = _pendingNormalAngle;
                _editTarget.PinSize = _pendingPinSize;
                _editTarget.LabelSize = _pendingLabelSize;
                _editTarget.LabelSpacing = _pendingLabelSpacing;
                _editTarget.MapOffsetX = _pendingOffsetX;
                _editTarget.MapOffsetY = _pendingOffsetY;
                LandmarkManager.Save();
            }
            else
            {
                LandmarkManager.Add(new CustomLandmark
                {
                    Name = name,
                    PlanetName = _pendingPlanet,
                    MapOffsetX = _pendingOffsetX,
                    MapOffsetY = _pendingOffsetY,
                    NormalAngle = _pendingNormalAngle,
                    PinSize = _pendingPinSize,
                    LabelSize = _pendingLabelSize,
                    LabelSpacing = _pendingLabelSpacing
                });
            }

            PlacementMode = false;
            PreviewLandmark = null;
            CloseDialog();
            RebuildMainWindow();
        }

        static void OnDialogCancel()
        {
            PlacementMode = false;
            if (_editTarget != null)
            {
                _editTarget.NormalAngle = _editTargetOriginalAngle;
                _editTarget.PinSize = _editTargetOriginalPinSize;
                _editTarget.LabelSize = _editTargetOriginalLabelSize;
                _editTarget.LabelSpacing = _editTargetOriginalLabelSpacing;
                _editTarget.MapOffsetX = _editTargetOriginalOffsetX;
                _editTarget.MapOffsetY = _editTargetOriginalOffsetY;
            }
            PreviewLandmark = null;
            CloseDialog();
            RebuildMainWindow();
        }

        static void ReopenDialog()
        {
            string currentText = _nameInput?.Text ?? "";
            string title = _editTarget != null ? "Edit Landmark" : "New Landmark";
            OpenDialog(title, currentText);
        }



        static void OnRotateLeft()
        {
            _pendingNormalAngle = (_pendingNormalAngle - 15f + 360f) % 360f;
            ApplyPendingToPreview();
            ReopenDialog();
        }

        static void OnRotateRight()
        {
            _pendingNormalAngle = (_pendingNormalAngle + 15f) % 360f;
            ApplyPendingToPreview();
            ReopenDialog();
        }

        static void OnPinDec()
        {
            _pendingPinSize = Mathf.Max(5, _pendingPinSize - 5);
            ApplyPendingToPreview();
            ReopenDialog();
        }

        static void OnPinInc()
        {
            _pendingPinSize = Mathf.Min(50, _pendingPinSize + 5);
            ApplyPendingToPreview();
            ReopenDialog();
        }

        static void OnLabelSizeDec()
        {
            _pendingLabelSize = Mathf.Max(10, _pendingLabelSize - 5);
            ApplyPendingToPreview();
            ReopenDialog();
        }

        static void OnLabelSizeInc()
        {
            _pendingLabelSize = Mathf.Min(80, _pendingLabelSize + 5);
            ApplyPendingToPreview();
            ReopenDialog();
        }

        static void OnSpacingDec()
        {
            _pendingLabelSpacing = Mathf.Max(0, _pendingLabelSpacing - 2);
            ApplyPendingToPreview();
            ReopenDialog();
        }

        static void OnSpacingInc()
        {
            _pendingLabelSpacing = Mathf.Min(40, _pendingLabelSpacing + 2);
            ApplyPendingToPreview();
            ReopenDialog();
        }

        static void OnOffsetXDec() => MovePosition(-1,  0);
        static void OnOffsetXInc() => MovePosition( 1,  0);
        static void OnOffsetYDec() => MovePosition( 0, -1);
        static void OnOffsetYInc() => MovePosition( 0,  1);

        static void OnStepDec()
        {
            _editMoveStep = Mathf.Max(1f, _editMoveStep / 10f);
            ReopenDialog();
        }

        static void OnStepInc()
        {
            _editMoveStep = _editMoveStep * 10f;
            ReopenDialog();
        }

        static void MovePosition(float screenDirX, float screenDirY)
        {
            float dx = (screenDirX * _mapRightPerPixel.x + screenDirY * _mapUpPerPixel.x) * _editMoveStep;
            float dy = (screenDirX * _mapRightPerPixel.y + screenDirY * _mapUpPerPixel.y) * _editMoveStep;
            _pendingOffsetX += dx;
            _pendingOffsetY += dy;
            ApplyPendingToPreview();
            ReopenDialog();
        }

        static void ApplyPendingToPreview()
        {
            if (PreviewLandmark != null)
            {
                PreviewLandmark.NormalAngle = _pendingNormalAngle;
                PreviewLandmark.PinSize = _pendingPinSize;
                PreviewLandmark.LabelSize = _pendingLabelSize;
                PreviewLandmark.LabelSpacing = _pendingLabelSpacing;
            }
            if (_editTarget != null)
            {
                _editTarget.NormalAngle = _pendingNormalAngle;
                _editTarget.PinSize = _pendingPinSize;
                _editTarget.LabelSize = _pendingLabelSize;
                _editTarget.LabelSpacing = _pendingLabelSpacing;
                _editTarget.MapOffsetX = _pendingOffsetX;
                _editTarget.MapOffsetY = _pendingOffsetY;
            }
        }



        static string GetDirectionArrow(float angle)
        {
            float a = ((angle % 360f) + 360f) % 360f;
            if (a < 22.5f  || a >= 337.5f) return "Up";
            if (a < 67.5f)                  return "Up-Right";
            if (a < 112.5f)                 return "Right";
            if (a < 157.5f)                 return "Down-Right";
            if (a < 202.5f)                 return "Down";
            if (a < 247.5f)                 return "Down-Left";
            if (a < 292.5f)                 return "Left";
            return "Up-Left";
        }



        class WindowPositionTracker : MonoBehaviour
        {
            Vector2 _posOnMouseDown;

            void Update()
            {
                if (_mainWinRT == null) return;

                _savedWindowPos = _mainWinRT.anchoredPosition;

                if (Input.GetMouseButtonDown(0))
                    _posOnMouseDown = _mainWinRT.anchoredPosition;

                if (Input.GetMouseButtonUp(0))
                {
                    Vector2 current = _mainWinRT.anchoredPosition;
                    if (current != _posOnMouseDown)
                        SaveWindowPosition(current);
                }
            }
        }

    }
}
