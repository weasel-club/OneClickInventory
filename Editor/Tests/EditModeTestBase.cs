using System.Collections.Generic;
using System.IO;
using Goorm.OneClickInventory.runtime;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Object = UnityEngine.Object;

namespace Goorm.OneClickInventory.Tests
{
    public abstract class EditModeTestBase
    {
        protected const string TestRoot = "Assets/OneClickInventoryTestTemp";
        private const string GeneratedPathGuid = "6385f8da0e893d142aaaef7ed709f4bd";

        protected GameObject AvatarObject;
        protected VRCAvatarDescriptor Avatar;

        [SetUp]
        public void SetUp()
        {
            AssetDatabase.DeleteAsset(TestRoot);
            CleanupGeneratedAssets();
            Directory.CreateDirectory(TestRoot);
            AssetDatabase.Refresh();

            AvatarObject = new GameObject("Test Avatar");
            Avatar = AvatarObject.AddComponent<VRCAvatarDescriptor>();
            Avatar.expressionsMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
        }

        [TearDown]
        public void TearDown()
        {
            if (Avatar != null && Avatar.expressionsMenu != null)
            {
                Object.DestroyImmediate(Avatar.expressionsMenu);
            }

            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root != null && root.name.StartsWith("Test Avatar", System.StringComparison.Ordinal))
                {
                    Object.DestroyImmediate(root);
                }
            }

            AssetDatabase.DeleteAsset(TestRoot);
            CleanupGeneratedAssets();
            AssetDatabase.Refresh();
        }

        protected static Inventory AddInventory(GameObject gameObject, string name)
        {
            var inventory = gameObject.AddComponent<Inventory>();
            SetSerializedValue(inventory, "_name", name);
            return inventory;
        }

        protected static GameObject CreateChild(string name, Transform parent)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        protected static Material CreateMaterialAsset(string fileName, Color color)
        {
            var material = new Material(Shader.Find("Unlit/Color")) { color = color };
            AssetDatabase.CreateAsset(material, $"{TestRoot}/{fileName}");
            return material;
        }

        protected static Texture2D CreateTextureAsset(string fileName, Color color)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            for (var y = 0; y < 2; y++)
            {
                for (var x = 0; x < 2; x++)
                {
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            AssetDatabase.CreateAsset(texture, $"{TestRoot}/{fileName}");
            return texture;
        }

        protected static AnimationClip CreateAdditionalClip(string fileName, string path, float value)
        {
            var clip = new AnimationClip();
            clip.SetCurve(path, typeof(GameObject), "m_IsActive",
                new AnimationCurve(new Keyframe(0f, value)));
            AssetDatabase.CreateAsset(clip, $"{TestRoot}/{fileName}");
            return clip;
        }

        protected static Mesh CreateBlendShapeMesh(string blendShapeName)
        {
            var mesh = new Mesh
            {
                vertices = new[]
                {
                    Vector3.zero,
                    Vector3.right,
                    Vector3.up
                },
                triangles = new[] { 0, 1, 2 }
            };

            mesh.AddBlendShapeFrame(blendShapeName, 100f, new[] { Vector3.zero, Vector3.zero, Vector3.zero },
                new[] { Vector3.zero, Vector3.zero, Vector3.zero },
                new[] { Vector3.zero, Vector3.zero, Vector3.zero });
            return mesh;
        }

        protected static void SetSerializedValue(Object target, string propertyName, bool value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).boolValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        protected static void SetSerializedValue(Object target, string propertyName, int value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).intValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        protected static void SetSerializedValue(Object target, string propertyName, string value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).stringValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        protected static void SetSerializedObjectReference(Object target, string propertyName, Object value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        protected static void SetSerializedArray<T>(Object target, string propertyName, IReadOnlyList<T> values)
            where T : Object
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);
            property.arraySize = values.Count;
            for (var i = 0; i < values.Count; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        protected static void SetBlendShapes(Inventory inventory, params SetBlendShapeBinding[] values)
        {
            var serializedObject = new SerializedObject(inventory);
            var property = serializedObject.FindProperty("_blendShapesToChange");
            property.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++)
            {
                var element = property.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("renderer").objectReferenceValue = values[i].renderer;
                element.FindPropertyRelative("name").stringValue = values[i].name;
                element.FindPropertyRelative("value").floatValue = values[i].value;
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        protected static void SetMaterialsToReplace(Inventory inventory, params ReplaceMaterialBinding[] values)
        {
            var serializedObject = new SerializedObject(inventory);
            var property = serializedObject.FindProperty("_materialsToReplace");
            property.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++)
            {
                var element = property.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("renderer").objectReferenceValue = values[i].renderer;
                element.FindPropertyRelative("from").objectReferenceValue = values[i].from;
                element.FindPropertyRelative("to").objectReferenceValue = values[i].to;
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        protected static string GeneratedRoot()
        {
            return AssetDatabase.GUIDToAssetPath(GeneratedPathGuid);
        }

        protected static void CleanupGeneratedAssets()
        {
            var root = GeneratedRoot();
            if (string.IsNullOrEmpty(root)) return;

            AssetDatabase.DeleteAsset($"{root}/Animations");
            AssetDatabase.DeleteAsset($"{root}/Controllers");
            AssetDatabase.DeleteAsset($"{root}/Tests");
        }
    }
}
