using System.Collections.Generic;
using System.Linq;
using Goorm.OneClickInventory.runtime;
using nadena.dev.modular_avatar.core;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Goorm.OneClickInventory
{
    public abstract class Generator
    {
        private static void CreateMaMergeAnimator(InventoryNode node, Dictionary<AnimatorController, int> controllers)
        {
            var mergeAnimatorObject = new GameObject("MergeAnimator");
            mergeAnimatorObject.transform.SetParent(node.Root.Value.transform, false);

            // Add merge animator
            foreach (var entry in controllers)
            {
                var mergeAnimator = mergeAnimatorObject.AddComponent<ModularAvatarMergeAnimator>();
                mergeAnimator.animator = entry.Key;
                mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
                mergeAnimator.deleteAttachedAnimator = true;
                mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
                mergeAnimator.matchAvatarWriteDefaults = false;
                mergeAnimator.layerPriority = entry.Value;
            }
        }

        private static void CreateMaParameters(InventoryNode node)
        {
            var parametersObject = new GameObject("Parameters");
            parametersObject.transform.SetParent(node.Root.Value.transform, false);
            var parameters = parametersObject.AddComponent<ModularAvatarParameters>();
            parameters.parameters = InventoryParameterConfigFactory.GetParameterConfigs(node).ToList();
        }

        public static void Generate(VRCAvatarDescriptor avatar)
        {
            // Resolve root nodes
            var rootNodes = InventoryNode.ResolveRootNodes(avatar).ToArray();

            MenuGenerator.Generate(avatar, rootNodes);
            foreach (var node in rootNodes)
            {
                // Generate animation
                var controllers = AnimationGenerator.GenerateControllers(node);
                CreateMaMergeAnimator(node, controllers);

                // Generate parameters
                CreateMaParameters(node);
            }

            // Remove Inventory components
            var types = new[] { typeof(Inventory), typeof(InventoryMenuInstaller), typeof(InventoryActiveParameter) };
            foreach (var type in types)
            {
                foreach (var component in avatar.GetComponentsInChildren(type, true))
                {
                    Object.DestroyImmediate(component);
                }
            }
        }
    }
}
