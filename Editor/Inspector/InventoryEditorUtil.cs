using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Goorm.OneClickInventory
{
    public abstract class InventoryEditorUtil
    {
        private const string EtcFoldoutKey = "one-click-inventory.etc-foldout";
        private const string ShowLegacyOptionsKey = "one-click-inventory.show-legacy-options";
        private const string DocumentationUrl = "https://goorm.me/docs/one-click-inventory";

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

        public static void Banner()
        {
            EditorGUILayout.Space(2f);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("One Click Inventory", BannerTitleStyle);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawDocumentationLink();
            }

            DrawLanguageSelector();

            EditorGUILayout.Space(4f);
        }

        public static void Footer(bool showLegacyOptionsToggle = false, bool forceShowLegacyOptions = false,
            Action<bool> drawLegacyOptions = null)
        {
            if (!showLegacyOptionsToggle) return;

            EditorGUILayout.Space();
            var showEtc = EditorPrefs.GetBool(EtcFoldoutKey, false);
            var displayedShowEtc = EditorGUILayout.Foldout(showEtc || forceShowLegacyOptions, Content("etc"), true);
            var nextShowEtc = forceShowLegacyOptions || displayedShowEtc;
            if (!forceShowLegacyOptions && nextShowEtc != showEtc)
            {
                EditorPrefs.SetBool(EtcFoldoutKey, nextShowEtc);
            }

            if (!nextShowEtc) return;

            bool showLegacyOptions;
            using (new EditorGUI.DisabledScope(forceShowLegacyOptions))
            {
                showLegacyOptions =
                    EditorGUILayout.Toggle(Content("showLegacyOptions"), forceShowLegacyOptions || ShowLegacyOptions);
            }

            if (forceShowLegacyOptions)
            {
                drawLegacyOptions?.Invoke(true);
                return;
            }

            if (showLegacyOptions != ShowLegacyOptions)
            {
                EditorPrefs.SetBool(ShowLegacyOptionsKey, showLegacyOptions);
            }

            drawLegacyOptions?.Invoke(showLegacyOptions);
        }

        private static GUIStyle BannerTitleStyle => new(EditorStyles.boldLabel) { fontSize = 14 };

        private static void DrawLanguageSelector()
        {
            var selectedLanguage = L.Languages.FindIndex(e => e.Item1 == L.Language);
            if (selectedLanguage < 0) selectedLanguage = 0;

            var nextLanguage = EditorGUILayout.Popup(Content("language"), selectedLanguage,
                L.Languages.Select(e => e.Item2).ToArray());
            if (nextLanguage != selectedLanguage)
            {
                L.Language = L.Languages[nextLanguage].Item1;
            }
        }

        private static void DrawDocumentationLink()
        {
            const float iconSize = 18f;
            const float rowHeight = 22f;
            const float gap = 4f;
            var rect = EditorGUILayout.GetControlRect(false, rowHeight);
            var iconRect = new Rect(rect.x, rect.y + (rowHeight - iconSize) / 2f, iconSize, iconSize);
            var labelRect = new Rect(iconRect.xMax + gap, rect.y, rect.width - iconSize - gap, rowHeight);
            var isHover = rect.Contains(Event.current.mousePosition);
            var icon = LoadEditorIcon("TextAsset Icon", "d_TextAsset Icon", "_Help", "d__Help");

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            if (icon != null)
            {
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
            }

            EditorGUI.LabelField(labelRect, L.Get("documentation"), DocumentationLinkStyle(isHover));

            if (isHover && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                Application.OpenURL(DocumentationUrl);
                Event.current.Use();
            }
        }

        private static GUIStyle DocumentationLinkStyle(bool isHover)
        {
            var style = new GUIStyle(EditorStyles.linkLabel)
            {
                alignment = TextAnchor.MiddleLeft
            };
            if (isHover)
            {
                style.normal.textColor = EditorGUIUtility.isProSkin
                    ? new Color(0.54f, 0.78f, 1f)
                    : new Color(0.08f, 0.32f, 0.72f);
            }

            return style;
        }

        private static Texture LoadEditorIcon(params string[] iconNames)
        {
            foreach (var iconName in iconNames)
            {
                var image = EditorGUIUtility.IconContent(iconName).image;
                if (image != null)
                {
                    return image;
                }
            }

            return null;
        }
    }
}
