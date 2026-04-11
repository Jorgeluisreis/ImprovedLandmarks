using HarmonyLib;
using ImprovedLandmarks;
using System.Collections;
using SFS.UI;
using SFS.World;
using SFS.World.Maps;
using SFS.Input;
using UnityEngine;

namespace ImprovedLandmarks.Patches
{
    public static class ObjectLabelInjector
    {
        static IconButton _labelBtn;

        public static void EnsureButtonExists()
        {
            if (_labelBtn != null && _labelBtn.gameObject != null)
                return;
            if (GameSelector.main == null) return;
            IconButton source = GameSelector.main.renameButton;
            if (source == null) return;
            Transform buttonParent = source.transform.parent;
            GameObject newBtnObj = Object.Instantiate(source.gameObject, buttonParent);
            newBtnObj.name = "ShowHideNameButton";
            _labelBtn = newBtnObj.GetComponent<IconButton>();
            _labelBtn.transform.SetSiblingIndex(source.transform.GetSiblingIndex() + 1);
            _labelBtn.button.onClick.Clear();
            _labelBtn.button.onClick += OnClick;
            if (_labelBtn.text != null)
                _labelBtn.text.Text = "";
            _labelBtn.Text = "";
        }

        public static void UpdateButtonState()
        {
            EnsureButtonExists();
            if (_labelBtn == null) return;
            if (GameSelector.main == null) { _labelBtn.Show = false; _labelBtn.gameObject.SetActive(false); return; }

            var sel = GameSelector.main.selected.Value;
            bool show = false;
            string text = "Show Name";
            int objectId = -1;
            if (sel is MapRocket mapObject && mapObject.rocket != null)
            {
                objectId = mapObject.rocket.stats.branch;
                bool isLabeled = objectId >= 0 && ObjectLabelManager.IsLabeled(objectId);
                show = true;
                text = isLabeled ? "Hide Name" : "Show Name";
            }
            _labelBtn.Show = show;
            _labelBtn.gameObject.SetActive(show);

            if (_labelBtn.button != null && _labelBtn.button.gameObject.activeInHierarchy)
                _labelBtn.button.StartCoroutine(DelayedTextUpdate(_labelBtn, text));

            if (_labelBtn.transform.parent is RectTransform parentRect)
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
        }

        private static IEnumerator DelayedTextUpdate(IconButton btn, string text)
        {
            yield return null;
            if (btn != null && btn.text != null)
                btn.text.Text = text;
            if (btn != null)
                btn.Text = text;
        }

        static void OnClick(OnInputEndData data)
        {
            if (GameSelector.main == null) return;
            if (!data.LeftClick) return;
            SelectableObject sel = GameSelector.main.selected.Value;
            if (!(sel is MapRocket mapObject) || mapObject.rocket == null) return;
            int objectId = mapObject.rocket.stats.branch;
            if (objectId < 0) return;
            ObjectLabelManager.ToggleLabel(objectId);
            UpdateButtonState();
        }
    }

    [HarmonyPatch(typeof(GameSelector), "OnSelectedChange")]
    public static class GameSelectorPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            ObjectLabelInjector.UpdateButtonState();
        }
    }
}
