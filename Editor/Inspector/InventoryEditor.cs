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
        private static bool _showItems;
        private static readonly AvatarHierarchyFolding AvatarHierarchyFolding = new();

        static InventoryEditor()
        {
            EditorApplication.hierarchyWindowItemOnGUI += DrawIconOnWindowItem;
        }

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

        private static void DrawIconOnWindowItem(int instanceID, Rect rect)
        {
            var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (gameObject == null)
            {
                return;
            }

            if (gameObject.TryGetComponent(out Inventory inventory))
            {
                var size = rect.height;
                var labelRect = new Rect(rect.xMax - size, rect.yMin, size, size);

                GUI.DrawTexture(labelRect,
                    CachedResource.Load<Texture2D>(inventory.Default ? "InventoryActive.png" : "Inventory.png"));
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField(L.Get("inventoryComponentDescription"), InventoryEditorUtil.DescriptionStyle);
            EditorGUILayout.Space();

            var avatar = Util.FindAvatar(Inventory.transform.parent);
            if (avatar == null)
            {
                EditorGUILayout.HelpBox(L.Get("noAvatar"), MessageType.Warning);
                return;
            }

            var node = InventoryNode.FindNodeByValue(avatar, Inventory);
            node.Root.Validate();

            serializedObject.Update();

            DrawStatus(node);
            DrawWarnings(node);
            DrawMenu(node);
            DrawInventorySettings(node);
            DrawItemSettings(node);
            InventoryEditorUtil.Footer(node.Avatar, AvatarHierarchyFolding);

            serializedObject.ApplyModifiedProperties();
            DisableOtherDefaults(node);
        }

        private void DrawStatus(InventoryNode node)
        {
            EditorGUILayout.LabelField(Content("statusSummary"), InventoryEditorUtil.HeaderStyle);
            EditorGUILayout.HelpBox(GetStatusSummary(node), MessageType.Info);
            EditorGUILayout.Space();
        }

        private static string GetStatusSummary(InventoryNode node)
        {
            var parts = new List<string>();
            parts.Add(node.IsRoot ? L.Get("roleRoot") : node.IsItem ? L.Get("roleItem") : L.Get("roleGroup"));
            if (node.IsInventory) parts.Add(L.Get("roleInventory"));
            if (node.Value.Default) parts.Add(L.Get("statusDefaultItem"));
            if (node.IntegratedMenuInstaller != null) parts.Add(L.Get("statusMenuInstallerIntegrated"));
            parts.Add(string.Format(L.Get("statusParameterMemory"), node.UsedParameterMemory));
            return string.Join(" / ", parts);
        }

        private void DrawWarnings(InventoryNode node)
        {
            if (string.IsNullOrWhiteSpace(Name.stringValue))
            {
                EditorGUILayout.HelpBox(L.Get("emptyNameWarning"), MessageType.Warning);
            }

            if (node.IsInventory && node.Value.IsUnique && node.HasChildItems && node.DefaultChild == null)
            {
                EditorGUILayout.HelpBox(L.Get("uniqueNoDefaultWarning"), MessageType.Info);
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

        private void DrawInventorySettings(InventoryNode node)
        {
            if (!node.IsInventory) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(L.Get("inventory"), InventoryEditorUtil.HeaderStyle);
            EditorGUI.BeginDisabledGroup(true);
            _showItems = EditorGUILayout.Foldout(_showItems, Content("items"), true);
            if (_showItems)
            {
                foreach (var child in node.ChildItems)
                {
                    EditorGUILayout.ObjectField(child.Value, typeof(Inventory), false);
                }
            }

            if (node.DefaultChild != null)
            {
                EditorGUILayout.ObjectField(Content("defaultItem"), node.DefaultChild.Value, typeof(Inventory), false);
            }

            EditorGUI.EndDisabledGroup();
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

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(Content("inventory"), node.Parent.Value, typeof(Inventory), false);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(Default,
                new GUIContent(node.ParentIsUnique ? L.Get("defaultUnique") : L.Get("default"),
                    L.Get("defaultTooltip")));
            EditorGUILayout.PropertyField(AdditionalObjects, Content("additionalObject"));
            EditorGUILayout.PropertyField(ObjectsToDisable, Content("disableObject"));

            DrawBlendShapeSection();
            DrawMaterialSection();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(L.Get("advanced"), InventoryEditorUtil.HeaderStyle);
            EditorGUILayout.PropertyField(ParameterDriverBindings, Content("parameterDrivers"));
            EditorGUILayout.PropertyField(AdditionalAnimations, Content("additionalAnimations"));

            if (!node.ParentIsUnique)
            {
                EditorGUILayout.PropertyField(LayerPriority, Content("layerPriority"));
                EditorGUILayout.PropertyField(Save, Content("saved", "savedItemTooltip"));
            }

            if (Inventory.TryGetComponent<ModularAvatarMenuInstaller>(out _))
            {
                EditorGUILayout.PropertyField(IntegrateMenuInstaller, Content("integrateMenuInstaller"));
            }
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
