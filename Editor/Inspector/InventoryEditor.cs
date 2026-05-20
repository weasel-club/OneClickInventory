using System;
using System.Collections.Generic;
using System.Linq;
using Goorm.OneClickInventory.runtime;
using nadena.dev.modular_avatar.core;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Goorm.OneClickInventory
{
    [CustomEditor(typeof(Inventory))]
    [CanEditMultipleObjects]
    public class InventoryEditor : Editor
    {
        private static bool _showStructurePreview = true;

        private Inventory Inventory { get; set; }
        private SerializedProperty Name { get; set; }

        private SerializedProperty IsUnique { get; set; }
        private SerializedProperty Default { get; set; }
        private SerializedProperty AdditionalAnimations { get; set; }
        private SerializedProperty AdditionalObjects { get; set; }
        private SerializedProperty ObjectsToDisable { get; set; }
        private SerializedProperty IsNotItem { get; set; }
        private SerializedProperty BlendShapesToChange { get; set; }
        private SerializedProperty MaterialsToReplace { get; set; }
        private SerializedProperty ParameterDriverBindings { get; set; }
        private SerializedProperty LayerPriority { get; set; }
        private SerializedProperty Save { get; set; }
        private SerializedProperty IntegrateMenuInstaller { get; set; }

        private ReorderableList _blendShapesToChangeList;
        private ReorderableList _materialsToReplaceList;
        private void OnEnable()
        {
            Inventory = (Inventory)target;
            Name = serializedObject.FindProperty("_name");
            IsUnique = serializedObject.FindProperty("_isUnique");
            Default = serializedObject.FindProperty("_default");
            AdditionalObjects = serializedObject.FindProperty("_additionalObjects");
            AdditionalAnimations = serializedObject.FindProperty("_additionalAnimations");
            ObjectsToDisable = serializedObject.FindProperty("_objectsToDisable");
            IsNotItem = serializedObject.FindProperty("_isNotItem");
            LayerPriority = serializedObject.FindProperty("_layerPriority");
            Save = serializedObject.FindProperty("_saved");
            IntegrateMenuInstaller = serializedObject.FindProperty("_integrateMenuInstaller");

            BlendShapesToChange = serializedObject.FindProperty("_blendShapesToChange");
            _blendShapesToChangeList =
                new ReorderableList(serializedObject, BlendShapesToChange, true, true, true, true)
                {
                    drawHeaderCallback = rect =>
                    {
                        rect.x += 15;
                        rect.width -= 15;
                        const float gap = 10f;
                        const float valueWidth = 50f - gap / 2;
                        var otherWidth = (rect.width - valueWidth - gap) / 2 - gap / 2;
                        var rendererX = rect.x;
                        var nameX = rendererX + otherWidth + gap;
                        var valueX = nameX + otherWidth + gap;

                        EditorGUI.LabelField(new Rect(rendererX, rect.y, otherWidth, EditorGUIUtility.singleLineHeight),
                            L.Get("rendererColumn"));
                        EditorGUI.LabelField(new Rect(nameX, rect.y, otherWidth, EditorGUIUtility.singleLineHeight),
                            L.Get("blendShapeNameColumn"));
                        EditorGUI.LabelField(new Rect(valueX, rect.y, valueWidth, EditorGUIUtility.singleLineHeight),
                            L.Get("valueColumn"));
                    },
                    drawElementCallback = (rect, index, _, _) =>
                    {
                        var element = _blendShapesToChangeList.serializedProperty.GetArrayElementAtIndex(index);
                        var renderer =
                            element.FindPropertyRelative("renderer").objectReferenceValue as SkinnedMeshRenderer;
                        var blendShapes = renderer != null && renderer.sharedMesh != null
                            ? Enumerable.Range(0, renderer.sharedMesh.blendShapeCount)
                                .Select(i => renderer.sharedMesh.GetBlendShapeName(i)).ToArray()
                            : Array.Empty<string>();
                        const float gap = 10f;
                        const float valueWidth = 50f - gap / 2;
                        var otherWidth = (rect.width - valueWidth - gap) / 2 - gap / 2;
                        var rendererX = rect.x;
                        var nameX = rendererX + otherWidth + gap;
                        var valueX = nameX + otherWidth + gap;

                        EditorGUI.PropertyField(
                            new Rect(rendererX, rect.y, otherWidth, EditorGUIUtility.singleLineHeight),
                            element.FindPropertyRelative("renderer"), GUIContent.none);
                        using var blendShapeProperty = element.FindPropertyRelative("name");
                        var blendShapeIndex =
                            EditorGUI.Popup(new Rect(nameX, rect.y, otherWidth, EditorGUIUtility.singleLineHeight),
                                blendShapes.ToList().IndexOf(blendShapeProperty.stringValue), blendShapes);
                        blendShapeProperty.stringValue = blendShapeIndex >= 0 ? blendShapes[blendShapeIndex] : "";
                        EditorGUI.PropertyField(new Rect(valueX, rect.y, valueWidth, EditorGUIUtility.singleLineHeight),
                            element.FindPropertyRelative("value"), GUIContent.none);
                    }
                };

            MaterialsToReplace = serializedObject.FindProperty("_materialsToReplace");
            _materialsToReplaceList = new ReorderableList(serializedObject, MaterialsToReplace, true, true, true, true)
            {
                drawHeaderCallback = rect =>
                {
                    rect.x += 15;
                    rect.width -= 15;
                    const float gap = 10f;
                    var width = (rect.width - gap * 2) / 3;
                    var rendererX = rect.x;
                    var fromX = rendererX + width + gap;
                    var toX = fromX + width + gap;

                    EditorGUI.LabelField(new Rect(rendererX, rect.y, width, EditorGUIUtility.singleLineHeight),
                        L.Get("rendererColumn"));
                    EditorGUI.LabelField(new Rect(fromX, rect.y, width, EditorGUIUtility.singleLineHeight),
                        L.Get("fromMaterialColumn"));
                    EditorGUI.LabelField(new Rect(toX, rect.y, width, EditorGUIUtility.singleLineHeight),
                        L.Get("toMaterialColumn"));
                },
                drawElementCallback = (rect, index, _, _) =>
                {
                    var element = _materialsToReplaceList.serializedProperty.GetArrayElementAtIndex(index);
                    const float gap = 10f;
                    var width = (rect.width - gap * 2) / 3;
                    var rendererX = rect.x;
                    var fromX = rendererX + width + gap;
                    var toX = fromX + width + gap;

                    var renderer = element.FindPropertyRelative("renderer").objectReferenceValue as Renderer;
                    var materials = renderer != null ? renderer.sharedMaterials.Distinct().ToArray() : new Material[0];

                    EditorGUI.PropertyField(new Rect(rendererX, rect.y, width, EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative("renderer"), GUIContent.none);
                    var from = element.FindPropertyRelative("from");
                    var fromIndex = EditorGUI.Popup(new Rect(fromX, rect.y, width, EditorGUIUtility.singleLineHeight),
                        materials.ToList().IndexOf(from.objectReferenceValue as Material),
                        materials.Select(e => e != null ? e.name : "").ToArray());
                    from.objectReferenceValue = fromIndex >= 0 ? materials[fromIndex] : null;
                    EditorGUI.PropertyField(new Rect(toX, rect.y, width, EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative("to"), GUIContent.none);
                },
            };

            ParameterDriverBindings = serializedObject.FindProperty("_parameterDriverBindings");
        }

        private static GUIContent Content(string key)
        {
            return InventoryEditorUtil.Content(key);
        }

        private static GUIContent Content(string key, string tooltipKey)
        {
            return InventoryEditorUtil.Content(key, tooltipKey);
        }

        public override void OnInspectorGUI()
        {
            InventoryEditorUtil.Banner();

            var avatar = Util.FindAvatar(Inventory.transform.parent);
            if (avatar == null)
            {
                EditorGUILayout.HelpBox(L.Get("noAvatar"), MessageType.Warning);
                return;
            }

            var node = InventoryNode.FindNodeByValue(avatar, Inventory);
            node.Root.Validate();

            serializedObject.Update();

            DrawWarnings(node);
            DrawMenu(node);
            DrawInventorySettings(node);
            DrawItemSettings(node);
            DrawAvatarSection(node);
            EditorGUILayout.Space(12f);
            InventoryEditorUtil.Footer(true);

            serializedObject.ApplyModifiedProperties();
            DisableOtherDefaults(node);
        }

        private void DrawWarnings(InventoryNode node)
        {
            if (string.IsNullOrWhiteSpace(Name.stringValue))
            {
                EditorGUILayout.HelpBox(L.Get("emptyNameWarning"), MessageType.Warning);
            }

            foreach (var warning in GetBlendShapeWarnings())
            {
                EditorGUILayout.HelpBox(warning, MessageType.Warning);
            }

            foreach (var warning in GetMaterialWarnings())
            {
                EditorGUILayout.HelpBox(warning, MessageType.Warning);
            }
        }

        private void DrawMenu(InventoryNode node)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(L.Get("menu"), InventoryEditorUtil.HeaderStyle);
            EditorGUILayout.PropertyField(Name, Content("name"));
            AssetPreview.GetAssetPreview(Inventory.Icon);
            EditorGUILayout.LabelField(Content("customIcon"));
            EditorGUILayout.BeginHorizontal();
            Inventory.Icon = (Texture2D)EditorGUILayout.ObjectField(Inventory.Icon, typeof(Texture2D), false,
                GUILayout.Width(100), GUILayout.Height(100));
            if (GUILayout.Button(Content("generateIcon"), GUILayout.Height(28)))
            {
                Inventory.Icon = IconUtil.Generate(node);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawAvatarSection(InventoryNode node)
        {
            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField(L.Get("avatar"), InventoryEditorUtil.HeaderStyle);
            DrawParameterMemorySummary(node);
            DrawStructurePreview(node);
        }

        private void DrawStructurePreview(InventoryNode selectedNode)
        {
            _showStructurePreview = EditorGUILayout.Foldout(_showStructurePreview, Content("avatarHierarchy"), true);
            if (!_showStructurePreview) return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawInventoryTreeNode(selectedNode.Root, selectedNode, new List<bool>());
            }
        }

        private static void DrawParameterMemorySummary(InventoryNode node)
        {
            EditorGUILayout.LabelField(
                Content("usedParameterMemory"),
                new GUIContent(node.Root.UsedParameterMemory.ToString()));
        }

        private static void DrawInventoryTreeNode(InventoryNode node, InventoryNode selectedNode,
            IReadOnlyList<bool> parentHasNextSiblings)
        {
            const float indentWidth = 14f;
            const float lineCenterOffset = 7f;
            const float iconOffset = 4f;
            var rowHeight = EditorGUIUtility.singleLineHeight + 4f;
            var rect = EditorGUILayout.GetControlRect(false, rowHeight);
            var depth = parentHasNextSiblings.Count;
            var rowRect = new Rect(
                rect.x,
                rect.y + 1f,
                rect.width,
                EditorGUIUtility.singleLineHeight + 2f);

            if (node.Value == selectedNode.Value)
            {
                EditorGUI.DrawRect(rowRect, new Color(0.24f, 0.48f, 0.90f, 0.18f));
            }
            else if (rowRect.Contains(Event.current.mousePosition))
            {
                EditorGUI.DrawRect(rowRect, EditorGUIUtility.isProSkin
                    ? new Color(1f, 1f, 1f, 0.08f)
                    : new Color(0f, 0f, 0f, 0.06f));
            }

            DrawTreeLines(rowRect, parentHasNextSiblings, indentWidth, lineCenterOffset);

            var iconX = rowRect.x + depth * indentWidth + iconOffset;
            var iconRect = new Rect(iconX, rowRect.y + 1f, 16f, 16f);
            var nameRect = new Rect(iconRect.xMax + 2f, rowRect.y, rowRect.xMax - iconRect.xMax - 2f,
                rowRect.height);
            var statusContent = BuildTreeStatusLabel(node);
            var statusWidth = GetTreeStatusStyle().CalcSize(statusContent).x;
            var statusRect = new Rect(nameRect.xMax - statusWidth, rowRect.y, statusWidth, rowRect.height);
            nameRect.width = Mathf.Max(0f, nameRect.width - statusWidth - 6f);

            var icon = EditorGUIUtility.IconContent("GameObject Icon").image;
            if (icon != null)
            {
                GUI.DrawTexture(iconRect, icon);
            }

            EditorGUI.LabelField(nameRect, BuildTreeLabel(node), EditorStyles.label);
            EditorGUI.LabelField(statusRect, statusContent, GetTreeStatusStyle());
            if (Event.current.type == EventType.MouseDown && rowRect.Contains(Event.current.mousePosition))
            {
                Selection.activeGameObject = node.Value.gameObject;
                EditorGUIUtility.PingObject(node.Value.gameObject);
                Event.current.Use();
            }

            var children = node.Children.ToArray();
            for (var i = 0; i < children.Length; i++)
            {
                var childParentHasNextSiblings = parentHasNextSiblings.ToList();
                childParentHasNextSiblings.Add(i < children.Length - 1);
                DrawInventoryTreeNode(children[i], selectedNode, childParentHasNextSiblings);
            }
        }

        private static void DrawTreeLines(Rect rowRect, IReadOnlyList<bool> parentHasNextSiblings, float indentWidth,
            float lineCenterOffset)
        {
            if (parentHasNextSiblings.Count == 0)
            {
                return;
            }

            var lineColor = EditorGUIUtility.isProSkin
                ? new Color(1f, 1f, 1f, 0.18f)
                : new Color(0f, 0f, 0f, 0.16f);
            var centerY = rowRect.y + rowRect.height / 2f;

            for (var i = 0; i < parentHasNextSiblings.Count; i++)
            {
                var x = rowRect.x + i * indentWidth + lineCenterOffset;
                var isCurrentLevel = i == parentHasNextSiblings.Count - 1;
                var hasNextSibling = parentHasNextSiblings[i];

                if (!isCurrentLevel && !hasNextSibling)
                {
                    continue;
                }

                var yMin = rowRect.y - 2f;
                var yMax = rowRect.yMax + 2f;
                if (isCurrentLevel && !hasNextSibling)
                {
                    yMax = centerY;
                }

                EditorGUI.DrawRect(new Rect(x, yMin, 1f, yMax - yMin), lineColor);
            }

            var branchX = rowRect.x + (parentHasNextSiblings.Count - 1) * indentWidth + lineCenterOffset;
            EditorGUI.DrawRect(new Rect(branchX, centerY, indentWidth - 3f, 1f), lineColor);
        }

        private static GUIStyle GetTreeStatusStyle()
        {
            var style = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight
            };
            style.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(1f, 1f, 1f, 0.45f)
                : new Color(0f, 0f, 0f, 0.45f);
            return style;
        }

        private static GUIContent BuildTreeLabel(InventoryNode node)
        {
            var name = string.IsNullOrWhiteSpace(node.Value.Name) ? node.Value.gameObject.name : node.Value.Name;
            return new GUIContent(name);
        }

        private static GUIContent BuildTreeStatusLabel(InventoryNode node)
        {
            var statuses = new List<string>();
            if (node.Value.IsUnique)
            {
                statuses.Add(L.Get("structureUnique"));
            }

            if (node.Value.Default)
            {
                statuses.Add(L.Get("structureDefault"));
            }

            return new GUIContent(string.Join(" · ", statuses));
        }

        private void DrawInventorySettings(InventoryNode node)
        {
            if (!node.IsInventory) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(L.Get("inventory"), InventoryEditorUtil.HeaderStyle);
            EditorGUILayout.PropertyField(IsUnique, Content("isUnique"));

            if (node.Value.IsUnique)
            {
                EditorGUILayout.PropertyField(LayerPriority, Content("layerPriority"));
                EditorGUILayout.PropertyField(Save, Content("saved", "savedInventoryTooltip"));
            }
        }

        private void DrawItemSettings(InventoryNode node)
        {
            if (node.CanBeItem)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(L.Get("item"), InventoryEditorUtil.HeaderStyle);
                EditorGUILayout.PropertyField(IsNotItem, Content("isNotItem"));
            }

            if (!node.IsItem) return;

            EditorGUILayout.PropertyField(Default,
                new GUIContent(node.ParentIsUnique ? L.Get("defaultUnique") : L.Get("default"),
                    L.Get("defaultTooltip")));

            if (!node.ParentIsUnique)
            {
                EditorGUILayout.PropertyField(LayerPriority, Content("layerPriority"));
                EditorGUILayout.PropertyField(Save, Content("saved", "savedItemTooltip"));
            }

            if (Inventory.TryGetComponent<ModularAvatarMenuInstaller>(out _))
            {
                EditorGUILayout.PropertyField(IntegrateMenuInstaller, Content("integrateMenuInstaller"));
            }

            DrawLegacyItemOptions();
            DrawItemActionButtons();
        }

        private void DrawLegacyItemOptions()
        {
            if (!InventoryEditorUtil.ShowLegacyOptions) return;

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(AdditionalObjects, Content("additionalObject"));
            EditorGUILayout.PropertyField(ObjectsToDisable, Content("disableObject"));
            DrawBlendShapeSection();
            DrawMaterialSection();
            EditorGUILayout.PropertyField(ParameterDriverBindings, Content("parameterDrivers"));
            EditorGUILayout.PropertyField(AdditionalAnimations, Content("additionalAnimations"));
            EditorGUILayout.Space();
        }

        private void DrawItemActionButtons()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(Content("itemActions"), InventoryEditorUtil.HeaderStyle);

            const float gap = 2f;
            var rect = EditorGUILayout.GetControlRect(false, 28f);
            var buttonWidth = (rect.width - gap * 2f) / 3f;
            var shapeChangerRect = new Rect(rect.x, rect.y, buttonWidth, rect.height);
            var objectToggleRect = new Rect(shapeChangerRect.xMax + gap, rect.y, buttonWidth, rect.height);
            var activeParameterRect = new Rect(objectToggleRect.xMax + gap, rect.y, buttonWidth, rect.height);
            var iconSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));

            DrawAddComponentButton<ModularAvatarShapeChanger>(shapeChangerRect, "addShapeChangerButton",
                "SkinnedMeshRenderer Icon", "MeshRenderer Icon");
            DrawAddComponentButton<ModularAvatarObjectToggle>(objectToggleRect, "addObjectToggleButton",
                "GameObject Icon", "Prefab Icon");
            DrawAddComponentButton<InventoryActiveParameter>(activeParameterRect, "addActiveParameterButton", true,
                "AnimatorController Icon", "d_AnimatorController Icon");

            EditorGUIUtility.SetIconSize(iconSize);
        }

        private void DrawAddComponentButton<T>(Rect rect, string contentKey, params string[] iconNames)
            where T : Component
        {
            DrawAddComponentButton<T>(rect, contentKey, false, iconNames);
        }

        private void DrawAddComponentButton<T>(Rect rect, string contentKey, bool allowMultiple,
            params string[] iconNames) where T : Component
        {
            var content = Content(contentKey);
            content.image = LoadEditorIcon(iconNames);
            var hasComponent = Inventory.TryGetComponent<T>(out _);

            using (new EditorGUI.DisabledScope(!allowMultiple && hasComponent))
            {
                if (!GUI.Button(rect, content))
                {
                    return;
                }
            }

            Undo.AddComponent<T>(Inventory.gameObject);
            EditorUtility.SetDirty(Inventory.gameObject);
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

        private void DrawBlendShapeSection()
        {
            BlendShapesToChange.isExpanded =
                EditorGUILayout.Foldout(BlendShapesToChange.isExpanded, Content("setBlendShape"), true);
            if (!BlendShapesToChange.isExpanded) return;

            EditorGUILayout.LabelField(L.Get("blendShapeDescription"), InventoryEditorUtil.SmallDescriptionStyle);
            if (BlendShapesToChange.arraySize == 0)
            {
                EditorGUILayout.HelpBox(L.Get("emptyBlendShapeList"), MessageType.Info);
            }

            _blendShapesToChangeList.DoLayoutList();
        }

        private void DrawMaterialSection()
        {
            MaterialsToReplace.isExpanded =
                EditorGUILayout.Foldout(MaterialsToReplace.isExpanded, Content("replaceMaterial"), true);
            if (!MaterialsToReplace.isExpanded) return;

            EditorGUILayout.LabelField(L.Get("replaceMaterialDescription"), InventoryEditorUtil.SmallDescriptionStyle);
            if (MaterialsToReplace.arraySize == 0)
            {
                EditorGUILayout.HelpBox(L.Get("emptyMaterialList"), MessageType.Info);
            }

            _materialsToReplaceList.DoLayoutList();
        }

        private IEnumerable<string> GetBlendShapeWarnings()
        {
            for (var i = 0; i < BlendShapesToChange.arraySize; i++)
            {
                var element = BlendShapesToChange.GetArrayElementAtIndex(i);
                var renderer = element.FindPropertyRelative("renderer").objectReferenceValue as SkinnedMeshRenderer;
                var name = element.FindPropertyRelative("name").stringValue;
                var row = i + 1;

                if (renderer == null)
                {
                    yield return string.Format(L.Get("blendShapeMissingRendererWarning"), row);
                    continue;
                }

                if (renderer.sharedMesh == null)
                {
                    yield return string.Format(L.Get("blendShapeMissingMeshWarning"), row);
                    continue;
                }

                var hasBlendShape = Enumerable.Range(0, renderer.sharedMesh.blendShapeCount)
                    .Any(index => renderer.sharedMesh.GetBlendShapeName(index) == name);
                if (!hasBlendShape)
                {
                    yield return string.Format(L.Get("blendShapeMissingNameWarning"), row);
                }
            }
        }

        private IEnumerable<string> GetMaterialWarnings()
        {
            for (var i = 0; i < MaterialsToReplace.arraySize; i++)
            {
                var element = MaterialsToReplace.GetArrayElementAtIndex(i);
                var renderer = element.FindPropertyRelative("renderer").objectReferenceValue as Renderer;
                var from = element.FindPropertyRelative("from").objectReferenceValue as Material;
                var to = element.FindPropertyRelative("to").objectReferenceValue as Material;
                var row = i + 1;

                if (renderer == null)
                {
                    yield return string.Format(L.Get("materialMissingRendererWarning"), row);
                }

                if (from == null)
                {
                    yield return string.Format(L.Get("materialMissingFromWarning"), row);
                }

                if (to == null)
                {
                    yield return string.Format(L.Get("materialMissingToWarning"), row);
                }
            }
        }

        private void DisableOtherDefaults(InventoryNode node)
        {
            if (!node.ParentIsUnique || !node.Value.Default) return;

            foreach (var e in node.Parent.ChildItems.Where(e => e.Value != Inventory))
            {
                Undo.RecordObject(e.Value, "Unset default inventory item");
                e.Value.Default = false;
                EditorUtility.SetDirty(e.Value);
            }
        }
    }
}
