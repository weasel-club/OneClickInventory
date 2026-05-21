using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Goorm.OneClickInventory.runtime;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Goorm.OneClickInventory.Tests
{
    public class RuntimeUtilityEditModeTests : EditModeTestBase
    {
        [Test]
        public void InventoryProperties_FilterNullAndIncompleteBindings()
        {
            var inventory = AddInventory(CreateChild("Item", Avatar.transform), "Item");
            var additionalObject = CreateChild("Additional", Avatar.transform);
            var disabledObject = CreateChild("Disabled", Avatar.transform);
            var animation = CreateAdditionalClip("Additional.anim", "Item", 1f);

            var rendererObject = CreateChild("Renderer", Avatar.transform);
            var blendShapeRenderer = rendererObject.AddComponent<SkinnedMeshRenderer>();
            blendShapeRenderer.sharedMesh = CreateBlendShapeMesh("Smile");
            var materialRenderer = rendererObject.AddComponent<MeshRenderer>();
            var from = CreateMaterialAsset("From.mat", Color.red);
            var to = CreateMaterialAsset("To.mat", Color.blue);
            materialRenderer.sharedMaterials = new[] { from };

            SetSerializedArray(inventory, "_additionalObjects", new[] { additionalObject, null });
            SetSerializedArray(inventory, "_objectsToDisable", new[] { disabledObject, null });
            SetSerializedArray(inventory, "_additionalAnimations", new[] { animation, null });
            SetBlendShapes(inventory,
                new SetBlendShapeBinding { renderer = blendShapeRenderer, name = "Smile", value = 50f },
                new SetBlendShapeBinding { renderer = null, name = "Ignored", value = 25f });
            SetMaterialsToReplace(inventory,
                new ReplaceMaterialBinding { renderer = materialRenderer, from = from, to = to },
                new ReplaceMaterialBinding { renderer = materialRenderer, from = null, to = to });

            Assert.That(inventory.GameObjects, Is.EqualTo(new[] { inventory.gameObject, additionalObject }));
            Assert.That(inventory.ObjectsToDisable, Is.EqualTo(new[] { disabledObject }));
            Assert.That(inventory.AdditionalAnimations, Is.EqualTo(new[] { animation }));
            Assert.That(inventory.BlendShapesToChange.Single().name, Is.EqualTo("Smile"));
            Assert.That(inventory.MaterialsToReplace.Single().to, Is.EqualTo(to));
        }

        [Test]
        public void InventoryName_UsesObjectNameWhenSerializedNameStartsEmpty()
        {
            var inventoryObject = CreateChild("Named Object", Avatar.transform);
            var inventory = inventoryObject.AddComponent<Inventory>();

            Assert.That(inventory.Name, Is.EqualTo("Named Object"));
        }

        [Test]
        public void InventoryName_FollowsDuplicatedAutoName()
        {
            var inventoryObject = CreateChild("Hat (1)", Avatar.transform);
            var inventory = inventoryObject.AddComponent<Inventory>();
            SetSerializedValue(inventory, "_name", "Hat");
            SetSerializedValue(inventory, "_lastSyncedObjectName", "Hat");

            typeof(Inventory).GetMethod("OnValidate", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(inventory, null);

            Assert.That(inventory.Name, Is.EqualTo("Hat (1)"));
        }

        [Test]
        public void InventoryName_UsesObjectNameWhenSerializedNameIsEmpty()
        {
            var inventoryObject = CreateChild("Hat", Avatar.transform);
            var inventory = inventoryObject.AddComponent<Inventory>();
            SetSerializedValue(inventory, "_name", "");

            typeof(Inventory).GetMethod("OnValidate", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(inventory, null);

            Assert.That(inventory.Name, Is.EqualTo("Hat"));
        }

        [Test]
        public void InventoryName_MigratesDuplicatedUnityDefaultNames()
        {
            var inventoryObject = CreateChild("GameObject (4)", Avatar.transform);
            var inventory = inventoryObject.AddComponent<Inventory>();
            SetSerializedValue(inventory, "_name", "GameObject (1)");

            Assert.That(inventory.Name, Is.EqualTo("GameObject (4)"));
        }

        [Test]
        public void InventoryName_KeepsCustomName()
        {
            var inventoryObject = CreateChild("Renamed Object", Avatar.transform);
            var inventory = inventoryObject.AddComponent<Inventory>();
            SetSerializedValue(inventory, "_name", "Custom Label");

            Assert.That(inventory.Name, Is.EqualTo("Custom Label"));
        }

        [Test]
        public void UtilMethods_FindAvatarEscapeNamesAndAddComponents()
        {
            var child = CreateChild("Child", Avatar.transform);
            var outside = new GameObject("Outside");

            Assert.That(Util.FindAvatar(child.transform), Is.EqualTo(Avatar));
            Assert.That(Util.IsInAvatar(Avatar, child.transform), Is.True);
            Assert.That(Util.IsInAvatar(Avatar, outside.transform), Is.False);
            Assert.That(Util.EscapeStateMachineName("A.B C/D(E)"), Is.EqualTo("A_B_C_D_E_"));

            var created = Util.GetOrAddComponent<Inventory>(AvatarObject);
            var existing = Util.GetOrAddComponent<Inventory>(AvatarObject);

            Assert.That(existing, Is.EqualTo(created));
            Object.DestroyImmediate(outside);
        }

        [Test]
        public void AssetUtil_CreatesDirectoriesAndReplacesAssets()
        {
            var first = new AnimationClip();
            var second = new AnimationClip();

            AssetUtil.CreateAsset(first, "Tests/Nested/Clip.anim");
            var path = AssetUtil.GetPath("Tests/Nested/Clip.anim");
            AssetUtil.CreateAsset(second, "Tests/Nested/Clip.anim");

            Assert.That(AssetDatabase.LoadAssetAtPath<AnimationClip>(path), Is.EqualTo(second));
            Assert.That(AssetUtil.GetEmptyPath("Tests/Nested/Clip.anim"), Is.EqualTo(path));
            Assert.That(AssetDatabase.LoadAssetAtPath<AnimationClip>(path), Is.Null);
        }

        [Test]
        public void Localization_ReturnsLanguageValueFallbackAndMissingKey()
        {
            var originalLanguage = L.Language;
            try
            {
                L.Language = "ko";

                Assert.That(L.Get("inventory"), Is.EqualTo("인벤토리"));
                Assert.That(L.Get("generateIcon"), Is.EqualTo("아이콘 생성"));
                Assert.That(L.Get("missing-localization-key"), Is.EqualTo("missing-localization-key"));

                L.Language = "ja";

                Assert.That(L.Get("inventory"), Is.EqualTo("インベントリ"));
                Assert.That(L.Get("generateIcon"), Is.EqualTo("アイコンを生成"));
            }
            finally
            {
                L.Language = originalLanguage;
            }
        }

        [Test]
        public void Localization_FilesShareSameKeys()
        {
            var localizationPath = AssetDatabase.GUIDToAssetPath("d9780e86d63caeb4b9287b9a4df854d9");
            var languages = new[] { "en", "ko", "ja", "zh-Hant" };
            var expectedKeys = ReadLocalizationKeys(localizationPath, languages[0]);

            foreach (var language in languages.Skip(1))
            {
                Assert.That(ReadLocalizationKeys(localizationPath, language), Is.EqualTo(expectedKeys), language);
            }
        }

        [Test]
        public void DocumentationUrl_FollowsSupportedLanguageAndFallsBackToEnglish()
        {
            Assert.That(InventoryEditorUtil.DocumentationUrl("ko"),
                Is.EqualTo("https://goorm.me/ko/docs/one-click-inventory"));
            Assert.That(InventoryEditorUtil.DocumentationUrl("en"),
                Is.EqualTo("https://goorm.me/en/docs/one-click-inventory"));
            Assert.That(InventoryEditorUtil.DocumentationUrl("ja"),
                Is.EqualTo("https://goorm.me/ja/docs/one-click-inventory"));
            Assert.That(InventoryEditorUtil.DocumentationUrl("zh-Hant"),
                Is.EqualTo("https://goorm.me/en/docs/one-click-inventory"));
        }

        private static IEnumerable<string> ReadLocalizationKeys(string localizationPath, string language)
        {
            var filename = Path.Combine(localizationPath, $"{language}.json");
            var json = File.ReadAllText(filename);
            return Regex.Matches(json, "^\\s*\"(?<key>[^\"]+)\"\\s*:", RegexOptions.Multiline)
                .Cast<Match>()
                .Select(e => e.Groups["key"].Value)
                .OrderBy(e => e)
                .ToArray();
        }
    }
}
