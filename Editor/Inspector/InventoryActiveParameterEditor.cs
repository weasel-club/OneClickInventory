using Goorm.OneClickInventory.runtime;
using UnityEditor;

namespace Goorm.OneClickInventory
{
    [CustomEditor(typeof(InventoryActiveParameter))]
    [CanEditMultipleObjects]
    public class InventoryActiveParameterEditor : Editor
    {
        private SerializedProperty _parameterName;

        private void OnEnable()
        {
            _parameterName = serializedObject.FindProperty("_parameterName");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_parameterName, InventoryEditorUtil.Content("parameterName"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
