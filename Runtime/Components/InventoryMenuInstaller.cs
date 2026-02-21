using nadena.dev.modular_avatar.core;
using UnityEngine;

namespace Goorm.OneClickInventory.runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("One-Click Inventory/Inventory Menu Installer")]
    public class InventoryMenuInstaller : ModularAvatarMenuInstaller
    {
        [SerializeField] private Inventory _inventory;
        public Inventory Inventory => _inventory;
    }
}