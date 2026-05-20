using Goorm.OneClickInventory.runtime;
using UnityEditor;

namespace Goorm.OneClickInventory
{
    [CustomEditor(typeof(InventoryMenuInstaller))]
    [CanEditMultipleObjects]
    public class InventoryMenuInstallerEditor : Editor
    {
        private SerializedProperty _inventory;

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

            InventoryEditorUtil.Footer();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
