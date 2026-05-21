using System.Collections.Generic;
using Goorm.OneClickInventory.runtime;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Goorm.OneClickInventory
{
    [ParameterProviderFor(typeof(Inventory))]
    internal sealed class InventoryParameterProvider : IParameterProvider
    {
        private readonly Inventory _inventory;

        public InventoryParameterProvider(Inventory inventory)
        {
            _inventory = inventory;
        }

        public IEnumerable<ProvidedParameter> GetSuppliedParameters(BuildContext context = null)
        {
            if (_inventory == null) yield break;

            var avatarTransform = nadena.dev.ndmf.runtime.RuntimeUtil.FindAvatarInParents(_inventory.transform);
            if (avatarTransform == null || !avatarTransform.TryGetComponent<VRCAvatarDescriptor>(out var avatar))
            {
                yield break;
            }

            var node = InventoryNode.FindNodeByValue(avatar, _inventory);
            if (node == null) yield break;

            foreach (var config in InventoryParameterConfigFactory.GetNodeParameterConfigs(node))
            {
                yield return ToProvidedParameter(config);
            }
        }

        private ProvidedParameter ToProvidedParameter(ParameterConfig config)
        {
            var animatorOnly = false;
            var parameterType = config.syncType switch
            {
                ParameterSyncType.Bool => AnimatorControllerParameterType.Bool,
                ParameterSyncType.Int => AnimatorControllerParameterType.Int,
                ParameterSyncType.Float => AnimatorControllerParameterType.Float,
                _ => AnimatorControllerParameterType.Float
            };

            if (config.syncType == ParameterSyncType.NotSynced)
            {
                animatorOnly = true;
            }

            return new ProvidedParameter(
                config.nameOrPrefix,
                config.isPrefix ? ParameterNamespace.PhysBonesPrefix : ParameterNamespace.Animator,
                _inventory,
                NDMFPlugin.Instance,
                parameterType)
            {
                IsAnimatorOnly = animatorOnly,
                WantSynced = !config.localOnly && !animatorOnly,
                IsHidden = config.internalParameter,
                DefaultValue = config.defaultValue
            };
        }
    }
}
