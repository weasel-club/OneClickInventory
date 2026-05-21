using System;
using System.Collections.Generic;
using System.IO;
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
        private const string DefaultInventoryIconFolderGuid = "85e63a01589fc6845ae77bd4ca0c8d2d";
        private static bool _showStructurePreview = true;
        private static Texture2D[] _defaultInventoryIcons;

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
        private string _immediateTooltip;

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
            _immediateTooltip = null;
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
            InventoryEditorUtil.Footer(true, HasLegacyItemOptions(), DrawLegacyItemOptions);
            DrawImmediateTooltip();

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

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(Content("generateIcon"), GUILayout.Width(120), GUILayout.Height(28)))
            {
                Inventory.Icon = IconUtil.Generate(node);
            }

            DrawDefaultIconPickerButton();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDefaultIconPickerButton()
        {
            using (new EditorGUI.DisabledScope(GetDefaultInventoryIcons().Length == 0))
            {
                if (!GUILayout.Button(Content("selectDefaultIcon"), GUILayout.Width(140), GUILayout.Height(28))) return;

                var buttonRect = GUILayoutUtility.GetLastRect();
                PopupWindow.Show(buttonRect, new DefaultInventoryIconPopup(GetDefaultInventoryIcons(), icon =>
                {
                    Undo.RecordObject(Inventory, "Select Default Inventory Icon");
                    Inventory.Icon = icon;
                    EditorUtility.SetDirty(Inventory);
                }));
            }
        }

        private static Texture2D[] GetDefaultInventoryIcons()
        {
            if (_defaultInventoryIcons != null) return _defaultInventoryIcons;

            var folderPath = AssetDatabase.GUIDToAssetPath(DefaultInventoryIconFolderGuid);
            if (string.IsNullOrEmpty(folderPath))
            {
                _defaultInventoryIcons = Array.Empty<Texture2D>();
                return _defaultInventoryIcons;
            }

            _defaultInventoryIcons = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(Path.GetFileNameWithoutExtension)
                .Select(AssetDatabase.LoadAssetAtPath<Texture2D>)
                .Where(icon => icon != null)
                .ToArray();
            return _defaultInventoryIcons;
        }

        private class DefaultInventoryIconPopup : PopupWindowContent
        {
            private const float IconButtonSize = 44f;
            private const float IconImageSize = 34f;
            private const float IconGap = 3f;
            private const float Padding = 8f;
            private const float HeaderHeight = 18f;
            private const int Columns = 5;

            private readonly Texture2D[] _icons;
            private readonly Action<Texture2D> _onSelect;

            public DefaultInventoryIconPopup(Texture2D[] icons, Action<Texture2D> onSelect)
            {
                _icons = icons;
                _onSelect = onSelect;
            }

            public override Vector2 GetWindowSize()
            {
                var rows = Mathf.CeilToInt(_icons.Length / (float)Columns);
                return new Vector2(Columns * IconButtonSize + (Columns - 1) * IconGap + Padding * 2f,
                    HeaderHeight + rows * IconButtonSize + Mathf.Max(0, rows - 1) * IconGap + Padding * 2f);
            }

            public override void OnGUI(Rect rect)
            {
                EditorGUI.LabelField(new Rect(Padding, Padding, rect.width - Padding * 2f, HeaderHeight),
                    InventoryEditor.Content("defaultIcons"), EditorStyles.miniBoldLabel);

                var x = Padding;
                var y = Padding + HeaderHeight;
                for (var i = 0; i < _icons.Length; i++)
                {
                    var icon = _icons[i];
                    var column = i % Columns;
                    var row = i / Columns;
                    var buttonRect = new Rect(x + column * (IconButtonSize + IconGap),
                        y + row * (IconButtonSize + IconGap), IconButtonSize, IconButtonSize);

                    DrawIconButton(buttonRect, icon);
                }
            }

            private void DrawIconButton(Rect rect, Texture2D icon)
            {
                if (rect.Contains(Event.current.mousePosition))
                {
                    EditorGUI.DrawRect(rect, new Color(1f, 1f, 1f, 0.08f));
                }

                var imageRect = new Rect(
                    rect.x + (rect.width - IconImageSize) / 2f,
                    rect.y + (rect.height - IconImageSize) / 2f,
                    IconImageSize,
                    IconImageSize);
                GUI.DrawTexture(imageRect, icon, ScaleMode.ScaleToFit, true);
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

                if (!GUI.Button(rect, new GUIContent("", icon.name), GUIStyle.none)) return;

                _onSelect(icon);
                editorWindow.Close();
            }
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
                var roots = InventoryNode.ResolveRootNodes(selectedNode.Avatar);
                foreach (var root in roots)
                {
                    DrawInventoryTreeNode(root, selectedNode, new List<bool>());
                }
            }
        }

        private static void DrawParameterMemorySummary(InventoryNode node)
        {
            var usedParameterMemory = InventoryNode.ResolveRootNodes(node.Avatar).Sum(e => e.UsedParameterMemory);
            EditorGUILayout.LabelField(
                Content("usedParameterMemory"),
                new GUIContent(usedParameterMemory.ToString()));
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

            DrawItemActionButtons();
        }

        private void DrawLegacyItemOptions(bool showLegacyOptions)
        {
            if (!showLegacyOptions && !HasLegacyItemOptions()) return;

            EditorGUILayout.Space();
            DrawLegacyProperty(AdditionalObjects, Content("additionalObject"), showLegacyOptions);
            DrawLegacyProperty(ObjectsToDisable, Content("disableObject"), showLegacyOptions);
            DrawBlendShapeSection(showLegacyOptions);
            DrawMaterialSection(showLegacyOptions);
            DrawLegacyProperty(ParameterDriverBindings, Content("parameterDrivers"), showLegacyOptions);
            DrawLegacyProperty(AdditionalAnimations, Content("additionalAnimations"), showLegacyOptions);
            EditorGUILayout.Space();
        }

        private bool HasLegacyItemOptions()
        {
            return HasArrayElements(AdditionalObjects)
                   || HasArrayElements(ObjectsToDisable)
                   || HasArrayElements(BlendShapesToChange)
                   || HasArrayElements(MaterialsToReplace)
                   || HasArrayElements(ParameterDriverBindings)
                   || HasArrayElements(AdditionalAnimations);
        }

        private static void DrawLegacyProperty(SerializedProperty property, GUIContent content, bool showWhenEmpty)
        {
            if (!showWhenEmpty && !HasArrayElements(property)) return;

            if (showWhenEmpty)
            {
                EditorGUILayout.PropertyField(property, content);
                return;
            }

            property.isExpanded = true;
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(property, content, true);
            }
        }

        private static bool HasArrayElements(SerializedProperty property)
        {
            return property.isArray && property.arraySize > 0;
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
            var tooltip = content.tooltip;
            content.tooltip = null;
            var hasComponent = Inventory.TryGetComponent<T>(out _);
            var clicked = false;

            using (new EditorGUI.DisabledScope(!allowMultiple && hasComponent))
            {
                clicked = GUI.Button(rect, content);
            }

            RegisterImmediateTooltip(rect, tooltip);
            if (!clicked) return;

            Undo.AddComponent<T>(Inventory.gameObject);
            EditorUtility.SetDirty(Inventory.gameObject);
        }

        private void RegisterImmediateTooltip(Rect rect, string tooltip)
        {
            if (string.IsNullOrEmpty(tooltip) || !rect.Contains(Event.current.mousePosition)) return;

            _immediateTooltip = tooltip;
            Repaint();
        }

        private void DrawImmediateTooltip()
        {
            if (string.IsNullOrEmpty(_immediateTooltip) || Event.current.type != EventType.Repaint) return;

            var content = new GUIContent(_immediateTooltip);
            var style = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13,
                normal = { textColor = Color.white },
                wordWrap = true,
                padding = new RectOffset(14, 14, 10, 10)
            };
            const float cursorOffset = 18f;
            const float screenPadding = 8f;
            const float maxWidth = 380f;
            const float minWidth = 220f;
            var width = Mathf.Clamp(style.CalcSize(content).x + style.padding.horizontal, minWidth, maxWidth);
            var height = style.CalcHeight(content, width) + style.padding.vertical;
            var mousePosition = Event.current.mousePosition;
            var x = Mathf.Min(mousePosition.x + cursorOffset, EditorGUIUtility.currentViewWidth - width - screenPadding);
            var y = mousePosition.y + cursorOffset;
            var rect = new Rect(Mathf.Max(screenPadding, x), y, width, height);

            var backgroundColor = EditorGUIUtility.isProSkin
                ? new Color(0.12f, 0.12f, 0.12f, 0.96f)
                : new Color(0.18f, 0.18f, 0.18f, 0.96f);
            EditorGUI.DrawRect(rect, backgroundColor);
            GUI.Label(rect, content, style);
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

        private void DrawBlendShapeSection(bool showWhenEmpty)
        {
            if (!showWhenEmpty && !HasArrayElements(BlendShapesToChange)) return;

            if (!showWhenEmpty)
            {
                BlendShapesToChange.isExpanded = true;
            }

            BlendShapesToChange.isExpanded =
                EditorGUILayout.Foldout(BlendShapesToChange.isExpanded, Content("setBlendShape"), true);
            if (!BlendShapesToChange.isExpanded) return;

            EditorGUILayout.LabelField(L.Get("blendShapeDescription"), InventoryEditorUtil.SmallDescriptionStyle);
            if (BlendShapesToChange.arraySize == 0)
            {
                EditorGUILayout.HelpBox(L.Get("emptyBlendShapeList"), MessageType.Info);
            }

            using (new EditorGUI.DisabledScope(!showWhenEmpty))
            {
                _blendShapesToChangeList.DoLayoutList();
            }
        }

        private void DrawMaterialSection(bool showWhenEmpty)
        {
            if (!showWhenEmpty && !HasArrayElements(MaterialsToReplace)) return;

            if (!showWhenEmpty)
            {
                MaterialsToReplace.isExpanded = true;
            }

            MaterialsToReplace.isExpanded =
                EditorGUILayout.Foldout(MaterialsToReplace.isExpanded, Content("replaceMaterial"), true);
            if (!MaterialsToReplace.isExpanded) return;

            EditorGUILayout.LabelField(L.Get("replaceMaterialDescription"), InventoryEditorUtil.SmallDescriptionStyle);
            if (MaterialsToReplace.arraySize == 0)
            {
                EditorGUILayout.HelpBox(L.Get("emptyMaterialList"), MessageType.Info);
            }

            using (new EditorGUI.DisabledScope(!showWhenEmpty))
            {
                _materialsToReplaceList.DoLayoutList();
            }
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
