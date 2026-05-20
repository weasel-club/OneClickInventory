using System.Collections.Generic;
using System.Linq;
using Goorm.OneClickInventory.runtime;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Goorm.OneClickInventory
{
    public class AvatarHierarchyFolding
    {
        public bool Show;
        public readonly Dictionary<string, bool> NodesShow = new();
    }

    public abstract class InventoryEditorUtil
    {
        public static GUIStyle HeaderStyle => new(EditorStyles.boldLabel);
        public static GUIStyle DescriptionStyle => new(EditorStyles.label) { wordWrap = true };
        public static GUIStyle SmallDescriptionStyle => new(EditorStyles.label) { wordWrap = true, fontSize = 11 };

        public static GUIContent Content(string key)
        {
            return new GUIContent(L.Get(key), L.Get($"{key}Tooltip"));
        }

        public static GUIContent Content(string key, string tooltipKey)
        {
            return new GUIContent(L.Get(key), L.Get(tooltipKey));
        }

        private static void AvatarHierarchy(InventoryNode node, int level, AvatarHierarchyFolding folding)
        {
            if (!node.ShouldBeSubmenu && !node.IsItem) return;
            if (node.IntegratedMenuInstaller) return;

            folding.NodesShow.TryAdd(node.Key, false);
            var menuItemsToInstall = node.MenuItemsToInstall.ToArray();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(level * 10, false);
            if (node.ShouldBeSubmenu)
            {
                folding.NodesShow[node.Key] = EditorGUILayout.BeginFoldoutHeaderGroup(folding.NodesShow[node.Key],
                    GUIContent.none,
                    new GUIStyle(EditorStyles.foldoutHeader)
                    { padding = new RectOffset(0, 0, 0, 0), stretchWidth = false });
            }

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(node.Value, typeof(Inventory), true);
            EditorGUI.EndDisabledGroup();
            if (node.ShouldBeSubmenu)
            {
                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            EditorGUILayout.EndHorizontal();
            if (folding.NodesShow[node.Key])
            {
                foreach (var child in node.Children)
                {
                    AvatarHierarchy(child, level + 1, folding);
                }

                foreach (var menuItem in menuItemsToInstall)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space((level + 1) * 10, false);
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField(menuItem, typeof(Inventory), true);
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        private static void AvatarHierarchy(VRCAvatarDescriptor avatar, AvatarHierarchyFolding folding)
        {
            var rootNodes = InventoryNode.ResolveRootNodes(avatar);
            foreach (var node in rootNodes.Where(node => node.ShouldBeSubmenu))
            {
                AvatarHierarchy(node, 0, folding);
            }
        }

        public static void Footer(VRCAvatarDescriptor avatar, AvatarHierarchyFolding folding)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(L.Get("avatar"), HeaderStyle);

            if (avatar == null)
            {
                EditorGUILayout.HelpBox(L.Get("noAvatar"), MessageType.Warning);
            }
            else
            {
                folding.Show = EditorGUILayout.Foldout(folding.Show, Content("avatarHierarchy"), true);
                if (folding.Show)
                {
                    AvatarHierarchy(avatar, folding);
                }

                var usedParameterMemory = InventoryNode.ResolveRootNodes(avatar).Select(e => e.UsedParameterMemory).Sum();
                EditorGUILayout.LabelField(
                    new GUIContent($"{L.Get("usedParameterMemory")} : {usedParameterMemory}",
                        L.Get("usedParameterMemoryTooltip")));
            }

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
        }
    }
}
