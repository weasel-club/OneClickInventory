using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Goorm.OneClickInventory
{
    public abstract class InventoryEditorUtil
    {
        private const string ShowLegacyOptionsKey = "one-click-inventory.show-legacy-options";

        public static GUIStyle HeaderStyle => new(EditorStyles.boldLabel);
        public static GUIStyle DescriptionStyle => new(EditorStyles.label) { wordWrap = true };
        public static GUIStyle SmallDescriptionStyle => new(EditorStyles.label) { wordWrap = true, fontSize = 11 };

        public static bool ShowLegacyOptions => EditorPrefs.GetBool(ShowLegacyOptionsKey, false);

        public static GUIContent Content(string key)
        {
            return new GUIContent(L.Get(key), L.Get($"{key}Tooltip"));
        }

        public static GUIContent Content(string key, string tooltipKey)
        {
            return new GUIContent(L.Get(key), L.Get(tooltipKey));
        }

        public static void Footer(bool showLegacyOptionsToggle = false)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(L.Get("etc"), HeaderStyle);

            var selectedLanguage = L.Languages.FindIndex(e => e.Item1 == L.Language);
            if (selectedLanguage < 0) selectedLanguage = 0;

            var nextLanguage = EditorGUILayout.Popup(Content("language"), selectedLanguage,
                L.Languages.Select(e => e.Item2).ToArray());
            if (nextLanguage != selectedLanguage)
            {
                L.Language = L.Languages[nextLanguage].Item1;
            }

            if (showLegacyOptionsToggle)
            {
                var showLegacyOptions = EditorGUILayout.Toggle(Content("showLegacyOptions"), ShowLegacyOptions);
                if (showLegacyOptions != ShowLegacyOptions)
                {
                    EditorPrefs.SetBool(ShowLegacyOptionsKey, showLegacyOptions);
                }
            }
        }
    }
}
