using System.Collections.Generic;
using System.Linq;
using Goorm.OneClickInventory.runtime;
using nadena.dev.modular_avatar.core;

namespace Goorm.OneClickInventory
{
    internal static class InventoryParameterConfigFactory
    {
        public static IEnumerable<ParameterConfig> GetNodeParameterConfigs(InventoryNode node)
        {
            if (!node.IsItem) yield break;

            foreach (var parameterName in node.Value.GetComponents<InventoryActiveParameter>()
                         .Select(e => e.ParameterName)
                         .Where(e => !string.IsNullOrWhiteSpace(e))
                         .Distinct())
            {
                yield return new ParameterConfig
                {
                    nameOrPrefix = parameterName,
                    syncType = ParameterSyncType.Bool,
                    defaultValue = 0,
                    saved = false,
                    localOnly = true
                };
            }

            yield return new ParameterConfig
            {
                nameOrPrefix = node.ParameterName,
                syncType = ParameterSyncType.Int,
                defaultValue = 0,
                saved = false,
                localOnly = true
            };

            yield return new ParameterConfig
            {
                nameOrPrefix = AnimationGenerator.GetSyncedParameterName(node.ParameterName),
                syncType = ParameterSyncType.Bool,
                defaultValue = 0,
                saved = false,
                localOnly = true
            };

            foreach (var (name, defaultValue) in AnimationGenerator.Encode(node.ParameterName, node.ParameterBits,
                         node.ParameterDefault))
            {
                var saved = node.ParentIsUnique ? node.Parent.Value.Saved : node.Value.Saved;

                yield return new ParameterConfig
                {
                    nameOrPrefix = name,
                    syncType = ParameterSyncType.Bool,
                    defaultValue = defaultValue,
                    saved = saved,
                    localOnly = false
                };
            }
        }

        public static IReadOnlyCollection<ParameterConfig> GetParameterConfigs(InventoryNode node)
        {
            var configs = new Dictionary<string, ParameterConfig>();
            AddParameterConfigs(node, configs);
            return configs.Values.ToArray();
        }

        private static void AddParameterConfigs(InventoryNode node, Dictionary<string, ParameterConfig> configs)
        {
            foreach (var config in GetNodeParameterConfigs(node))
            {
                configs[config.nameOrPrefix] = config;
            }

            foreach (var child in node.Children)
            {
                AddParameterConfigs(child, configs);
            }
        }
    }
}
