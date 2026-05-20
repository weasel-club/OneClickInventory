using System.Linq;
using Goorm.OneClickInventory.runtime;
using nadena.dev.modular_avatar.core;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.TestTools;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDKBase;

namespace Goorm.OneClickInventory.Tests
{
    public class GenerationEditModeTests : EditModeTestBase
    {
        [Test]
        public void Encode_ReturnsMostSignificantBitsWithStableNames()
        {
            var encoded = AnimationGenerator.Encode("Inventory", 3, 5);

            Assert.That(encoded.Select(e => e.Item1),
                Is.EqualTo(new[] { "Inventory/Bits/0", "Inventory/Bits/1", "Inventory/Bits/2" }));
            Assert.That(encoded.Select(e => e.Item2), Is.EqualTo(new[] { 1, 0, 1 }));
        }

        [Test]
        public void GenerateControllers_NonUniqueItemCreatesToggleControllerAndLayers()
        {
            var rootInventory = AddInventory(CreateChild("Root", Avatar.transform), "Root");
            var itemInventory = AddInventory(CreateChild("Hat", rootInventory.transform), "Hat");
            SetSerializedValue(itemInventory, "_layerPriority", 23);
            var itemNode = InventoryNode.ResolveRootNodes(Avatar).Single().ChildItems.Single();

            var controllers = AnimationGenerator.GenerateControllers(itemNode.Root);
            var controller = controllers.Keys.Single();

            Assert.That(controllers[controller], Is.EqualTo(23));
            Assert.That(controller.layers.Select(e => e.name),
                Is.EqualTo(new[]
                {
                    itemNode.Key,
                    $"{itemNode.ParameterName}/Encoder",
                    $"{itemNode.ParameterName}/Decoder"
                }));
            Assert.That(controller.parameters.Select(e => e.name),
                Has.Member(itemNode.ParameterName)
                    .And.Member(AnimationGenerator.GetSyncedParameterName(itemNode.ParameterName))
                    .And.Member("IsLocal"));
            Assert.That(controller.layers[0].stateMachine.states.Select(e => e.state.name),
                Has.Member($"Enabled ({itemNode.EscapedName})")
                    .And.Member($"Disabled ({itemNode.EscapedName})"));
        }

        [Test]
        public void GenerateControllers_NonUniqueActiveParameterFollowsItemState()
        {
            var rootInventory = AddInventory(CreateChild("Root", Avatar.transform), "Root");
            var itemObject = CreateChild("Hat", rootInventory.transform);
            AddInventory(itemObject, "Hat");
            AddActiveParameter(itemObject, "HatActive");
            var itemNode = InventoryNode.ResolveRootNodes(Avatar).Single().ChildItems.Single();

            var controller = AnimationGenerator.GenerateControllers(itemNode.Root).Keys.Single();
            var states = controller.layers[0].stateMachine.states;
            var enabledState = states.Single(e => e.state.name.StartsWith("Enabled", System.StringComparison.Ordinal))
                .state;
            var disabledState = states.Single(e => e.state.name.StartsWith("Disabled", System.StringComparison.Ordinal))
                .state;
            var idleState = states.Single(e => e.state.name == "Idle").state;

            Assert.That(GetParameterDriverValue(enabledState, "HatActive"), Is.EqualTo(1f));
            Assert.That(GetParameterDriverValue(disabledState, "HatActive"), Is.EqualTo(0f));
            Assert.That(GetParameterDriverValue(idleState, "HatActive"), Is.EqualTo(0f));
        }

        [Test]
        public void GenerateControllers_UniqueInventoryCreatesDefaultAndChildStates()
        {
            var rootInventory = AddInventory(CreateChild("Root", Avatar.transform), "Root");
            SetSerializedValue(rootInventory, "_isUnique", true);
            SetSerializedValue(rootInventory, "_layerPriority", 7);
            var defaultInventory = AddInventory(CreateChild("Default", rootInventory.transform), "Default");
            SetSerializedValue(defaultInventory, "_default", true);
            AddInventory(CreateChild("Second", rootInventory.transform), "Second");
            var rootNode = InventoryNode.ResolveRootNodes(Avatar).Single();

            var controllers = AnimationGenerator.GenerateControllers(rootNode);
            var controller = controllers.Keys.Single();

            Assert.That(controllers[controller], Is.EqualTo(7));
            Assert.That(controller.layers.Select(e => e.name),
                Is.EqualTo(new[] { rootNode.Key, $"{rootNode.Key}/Encoder", $"{rootNode.Key}/Decoder" }));
            Assert.That(controller.parameters.Select(e => e.name),
                Has.Member(rootNode.Key).And.Member(AnimationGenerator.GetSyncedParameterName(rootNode.Key)));
            Assert.That(controller.layers[0].stateMachine.states.Select(e => e.state.name),
                Has.Member("Default").And.Member("Second"));
        }

        [Test]
        public void GenerateControllers_UniqueActiveParametersResetSiblingItems()
        {
            var rootInventory = AddInventory(CreateChild("Root", Avatar.transform), "Root");
            SetSerializedValue(rootInventory, "_isUnique", true);
            var firstObject = CreateChild("First", rootInventory.transform);
            var firstInventory = AddInventory(firstObject, "First");
            SetSerializedValue(firstInventory, "_default", true);
            AddActiveParameter(firstObject, "FirstActive");
            var secondObject = CreateChild("Second", rootInventory.transform);
            AddInventory(secondObject, "Second");
            AddActiveParameter(secondObject, "SecondActive");
            var rootNode = InventoryNode.ResolveRootNodes(Avatar).Single();

            var controller = AnimationGenerator.GenerateControllers(rootNode).Keys.Single();
            var states = controller.layers[0].stateMachine.states;
            var firstState = states.Single(e => e.state.name == "First").state;
            var secondState = states.Single(e => e.state.name == "Second").state;
            var idleState = states.Single(e => e.state.name == "Idle").state;

            Assert.That(GetParameterDriverValue(firstState, "FirstActive"), Is.EqualTo(1f));
            Assert.That(GetParameterDriverValue(firstState, "SecondActive"), Is.EqualTo(0f));
            Assert.That(GetParameterDriverValue(secondState, "FirstActive"), Is.EqualTo(0f));
            Assert.That(GetParameterDriverValue(secondState, "SecondActive"), Is.EqualTo(1f));
            Assert.That(GetParameterDriverValue(idleState, "FirstActive"), Is.EqualTo(0f));
            Assert.That(GetParameterDriverValue(idleState, "SecondActive"), Is.EqualTo(0f));
        }

        [Test]
        public void GenerateControllers_CreatesAnimationCurvesAndSkipsOutsideObjects()
        {
            var rootInventory = AddInventory(CreateChild("Root", Avatar.transform), "Root");
            var itemObject = CreateChild("Item", rootInventory.transform);
            var itemInventory = AddInventory(itemObject, "Item");
            var additionalObject = CreateChild("Additional", Avatar.transform);
            var disabledObject = CreateChild("Disabled", Avatar.transform);
            var outsideObject = new GameObject("Outside");
            var additionalClip = CreateAdditionalClip("Extra.anim", "Additional", 0.5f);

            var rendererObject = CreateChild("Renderer", Avatar.transform);
            var blendShapeRenderer = rendererObject.AddComponent<SkinnedMeshRenderer>();
            blendShapeRenderer.sharedMesh = CreateBlendShapeMesh("Smile");
            var materialRenderer = rendererObject.AddComponent<MeshRenderer>();
            var from = CreateMaterialAsset("From.mat", Color.red);
            var to = CreateMaterialAsset("To.mat", Color.blue);
            materialRenderer.sharedMaterials = new[] { from };

            SetSerializedArray(itemInventory, "_additionalObjects", new[] { additionalObject, outsideObject });
            SetSerializedArray(itemInventory, "_objectsToDisable", new[] { disabledObject, outsideObject });
            SetSerializedArray(itemInventory, "_additionalAnimations", new[] { additionalClip });
            SetBlendShapes(itemInventory,
                new SetBlendShapeBinding { renderer = blendShapeRenderer, name = "Smile", value = 75f });
            SetMaterialsToReplace(itemInventory,
                new ReplaceMaterialBinding { renderer = materialRenderer, from = from, to = to });

            var itemNode = InventoryNode.ResolveRootNodes(Avatar).Single().ChildItems.Single();
            var controller = AnimationGenerator.GenerateControllers(itemNode.Root).Keys.Single();
            var enabledClip = (AnimationClip)controller.layers[0].stateMachine.states
                .Single(e => e.state.name.StartsWith("Enabled", System.StringComparison.Ordinal)).state.motion;

            var curveBindings = AnimationUtility.GetCurveBindings(enabledClip);
            var objectBindings = AnimationUtility.GetObjectReferenceCurveBindings(enabledClip);

            Assert.That(curveBindings.Any(e => e.path == "Root/Item" && e.propertyName == "m_IsActive"), Is.True);
            Assert.That(curveBindings.Any(e => e.path == "Additional" && e.propertyName == "m_IsActive"), Is.True);
            Assert.That(curveBindings.Any(e => e.path == "Disabled" && e.propertyName == "m_IsActive"), Is.True);
            Assert.That(curveBindings.Any(e => e.path == "Renderer" && e.propertyName == "blendShape.Smile"), Is.True);
            Assert.That(curveBindings.Any(e => e.path == "Outside"), Is.False);
            Assert.That(objectBindings.Single().path, Is.EqualTo("Renderer"));
            Assert.That(objectBindings.Single().propertyName, Is.EqualTo("m_Materials.Array.data[0]"));

            Object.DestroyImmediate(outsideObject);
        }

        [Test]
        public void GenerateControllers_UnsupportedMaterialRendererLogsWarningAndContinues()
        {
            var rootInventory = AddInventory(CreateChild("Root", Avatar.transform), "Root");
            var itemInventory = AddInventory(CreateChild("Item", rootInventory.transform), "Item");
            var rendererObject = CreateChild("Line", Avatar.transform);
            var lineRenderer = rendererObject.AddComponent<LineRenderer>();
            var from = CreateMaterialAsset("LineFrom.mat", Color.red);
            var to = CreateMaterialAsset("LineTo.mat", Color.blue);
            lineRenderer.sharedMaterials = new[] { from };
            SetMaterialsToReplace(itemInventory,
                new ReplaceMaterialBinding { renderer = lineRenderer, from = from, to = to });

            LogAssert.Expect(LogType.Warning,
                "OneClickInventory skipped material replacement for unsupported renderer type: LineRenderer");

            Assert.DoesNotThrow(() =>
                AnimationGenerator.GenerateControllers(InventoryNode.ResolveRootNodes(Avatar).Single()));
        }

        [Test]
        public void MenuGenerator_CreatesGeneratedMenuRootAndItemToggle()
        {
            var rootInventory = AddInventory(CreateChild("Root", Avatar.transform), "Root");
            var itemInventory = AddInventory(CreateChild("Hat", rootInventory.transform), "Hat");
            var itemNode = InventoryNode.ResolveRootNodes(Avatar).Single().ChildItems.Single();

            MenuGenerator.Generate(Avatar, InventoryNode.ResolveRootNodes(Avatar).ToArray());

            var sourceInventoryObject = Avatar.transform.Find("Root");
            var menuRoot = Avatar.transform.Find("OneClickInventory Generated Menus");
            Assert.That(menuRoot, Is.Not.Null);

            var inventoryMenuObject = menuRoot.Find("Root");
            Assert.That(inventoryMenuObject, Is.Not.Null);

            var rootMenu = inventoryMenuObject.GetComponent<ModularAvatarMenuItem>();
            var installer = inventoryMenuObject.GetComponent<ModularAvatarMenuInstaller>();
            var toggle = inventoryMenuObject.Find("Hat").GetComponent<ModularAvatarMenuItem>();

            Assert.That(sourceInventoryObject.GetComponent<ModularAvatarMenuItem>(), Is.Null);
            Assert.That(menuRoot.GetComponent<ModularAvatarMenuItem>(), Is.Null);
            Assert.That(rootMenu.Control.type, Is.EqualTo(VRCExpressionsMenu.Control.ControlType.SubMenu));
            Assert.That(rootMenu.Control.name, Is.EqualTo(rootInventory.Name));
            Assert.That(installer.menuToAppend, Is.EqualTo(Avatar.expressionsMenu));
            Assert.That(toggle.Control.type, Is.EqualTo(VRCExpressionsMenu.Control.ControlType.Toggle));
            Assert.That(toggle.Control.name, Is.EqualTo(itemInventory.Name));
            Assert.That(toggle.Control.parameter.name, Is.EqualTo(itemNode.ParameterName));
            Assert.That(toggle.Control.value, Is.EqualTo(itemNode.ParameterValue));
        }

        [Test]
        public void MenuGenerator_MovesInventoryMenuInstallerItemsUnderGeneratedMenu()
        {
            var rootInventory = AddInventory(CreateChild("Root", Avatar.transform), "Root");
            var itemInventory = AddInventory(CreateChild("Hat", rootInventory.transform), "Hat");
            var externalMenuObject = CreateChild("External Menu", Avatar.transform);
            externalMenuObject.AddComponent<ModularAvatarMenuItem>().Control = new VRCExpressionsMenu.Control
            {
                name = "External",
                type = VRCExpressionsMenu.Control.ControlType.Button
            };
            var installer = externalMenuObject.AddComponent<InventoryMenuInstaller>();
            SetSerializedObjectReference(installer, "_inventory", itemInventory);

            MenuGenerator.Generate(Avatar, InventoryNode.ResolveRootNodes(Avatar).ToArray());

            var generatedItemMenu = externalMenuObject.transform.parent;

            Assert.That(generatedItemMenu, Is.Not.Null);
            Assert.That(generatedItemMenu.name, Is.EqualTo("Hat"));
            Assert.That(generatedItemMenu.parent.name, Is.EqualTo("Root"));
            Assert.That(generatedItemMenu.parent.parent.name, Is.EqualTo("OneClickInventory Generated Menus"));
            Assert.That(generatedItemMenu.GetComponent<ModularAvatarMenuItem>().Control.name, Is.EqualTo("Hat"));
            Assert.That(externalMenuObject.transform.parent, Is.EqualTo(generatedItemMenu));
        }

        [Test]
        public void Generator_CreatesMergeAnimatorAndParametersThenRemovesInventoryComponents()
        {
            var rootInventory = AddInventory(CreateChild("Root", Avatar.transform), "Root");
            SetSerializedValue(rootInventory, "_isUnique", true);
            SetSerializedValue(rootInventory, "_saved", true);
            var defaultInventory = AddInventory(CreateChild("Default", rootInventory.transform), "Default");
            SetSerializedValue(defaultInventory, "_default", true);
            var secondInventory = AddInventory(CreateChild("Second", rootInventory.transform), "Second");
            SetSerializedValue(secondInventory, "_saved", false);

            Generator.Generate(Avatar);

            var mergeAnimator = Avatar.GetComponentInChildren<ModularAvatarMergeAnimator>(true);
            var parameters = Avatar.GetComponentInChildren<ModularAvatarParameters>(true).parameters;

            Assert.That(mergeAnimator, Is.Not.Null);
            Assert.That(mergeAnimator.layerType, Is.EqualTo(VRCAvatarDescriptor.AnimLayerType.FX));
            Assert.That(mergeAnimator.deleteAttachedAnimator, Is.True);
            Assert.That(mergeAnimator.pathMode, Is.EqualTo(MergeAnimatorPathMode.Absolute));
            Assert.That(parameters.Select(e => e.nameOrPrefix),
                Has.Member("OCInv/Root").And.Member("OCInv/Root/Synced").And.Member("OCInv/Root/Bits/0"));
            Assert.That(parameters.Single(e => e.nameOrPrefix == "OCInv/Root").localOnly, Is.True);
            Assert.That(parameters.Single(e => e.nameOrPrefix == "OCInv/Root/Bits/0").saved, Is.True);
            Assert.That(Avatar.GetComponentInChildren<Inventory>(true), Is.Null);
        }

        [Test]
        public void Generator_CreatesLocalBoolParametersForActiveParameters()
        {
            var rootInventory = AddInventory(CreateChild("Root", Avatar.transform), "Root");
            var itemObject = CreateChild("Hat", rootInventory.transform);
            AddInventory(itemObject, "Hat");
            AddActiveParameter(itemObject, "HatActive");

            Generator.Generate(Avatar);

            var parameter = Avatar.GetComponentsInChildren<ModularAvatarParameters>(true)
                .SelectMany(e => e.parameters)
                .Single(e => e.nameOrPrefix == "HatActive");

            Assert.That(parameter.syncType, Is.EqualTo(ParameterSyncType.Bool));
            Assert.That(parameter.defaultValue, Is.EqualTo(0));
            Assert.That(parameter.saved, Is.False);
            Assert.That(parameter.localOnly, Is.True);
            Assert.That(Avatar.GetComponentInChildren<InventoryActiveParameter>(true), Is.Null);
        }

        [Test]
        public void Generator_ParameterDefaultsFollowUniqueAndNonUniqueItems()
        {
            var uniqueInventory = AddInventory(CreateChild("Unique", Avatar.transform), "Unique");
            SetSerializedValue(uniqueInventory, "_isUnique", true);
            var defaultInventory = AddInventory(CreateChild("Default", uniqueInventory.transform), "Default");
            SetSerializedValue(defaultInventory, "_default", true);
            AddInventory(CreateChild("Second", uniqueInventory.transform), "Second");

            var toggleInventory = AddInventory(CreateChild("ToggleRoot", Avatar.transform), "ToggleRoot");
            var toggleItem = AddInventory(CreateChild("ToggleItem", toggleInventory.transform), "ToggleItem");
            SetSerializedValue(toggleItem, "_default", true);
            SetSerializedValue(toggleItem, "_saved", false);

            Generator.Generate(Avatar);

            var parameters = Avatar.GetComponentsInChildren<ModularAvatarParameters>(true)
                .SelectMany(e => e.parameters).ToArray();

            Assert.That(parameters.Single(e => e.nameOrPrefix == "OCInv/Unique/Bits/0").defaultValue,
                Is.EqualTo(0));
            Assert.That(parameters.Single(e => e.nameOrPrefix == "OCInv/Unique/Bits/0").saved,
                Is.True);
            Assert.That(parameters.Single(e => e.nameOrPrefix == "OCInv/ToggleRoot/ToggleItem/Toggle/Bits/0").defaultValue,
                Is.EqualTo(1));
            Assert.That(parameters.Single(e => e.nameOrPrefix == "OCInv/ToggleRoot/ToggleItem/Toggle/Bits/0").saved,
                Is.False);
        }

        private static float? GetParameterDriverValue(AnimatorState state, string parameterName)
        {
            return state.behaviours
                .OfType<VRCAvatarParameterDriver>()
                .SelectMany(e => e.parameters)
                .Where(e => e.name == parameterName)
                .Select(e => (float?)e.value)
                .FirstOrDefault();
        }

        private static InventoryActiveParameter AddActiveParameter(GameObject gameObject, string parameterName)
        {
            var activeParameter = gameObject.AddComponent<InventoryActiveParameter>();
            SetSerializedValue(activeParameter, "_parameterName", parameterName);
            return activeParameter;
        }
    }
}
