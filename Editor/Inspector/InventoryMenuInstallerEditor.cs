using Goorm.OneClickInventory.runtime;
using UnityEditor;
using UnityEngine;

namespace Goorm.OneClickInventory
{
    [CustomEditor(typeof(InventoryMenuInstaller))]
    [CanEditMultipleObjects]
    public class InventoryMenuInstallerEditor : Editor
    {
        private SerializedProperty _inventory;
        private readonly AvatarHierarchyFolding _folding = new();

        private void OnEnable()
        {
            _inventory = serializedObject.FindProperty("_inventory");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField(L.Get("menuInstallerDescription"), InventoryEditorUtil.DescriptionStyle);
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(_inventory,
                InventoryEditorUtil.Content("connectedInventory", "menuInstallerInventoryTooltip"));

            var avatar = Util.FindAvatar(
                (serializedObject.targetObject as InventoryMenuInstaller)?.transform
            );

            InventoryEditorUtil.Footer(avatar, _folding);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
