using UnityEngine;
using VRC.SDKBase;

namespace Goorm.OneClickInventory.runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("One-Click Inventory/Inventory Config")]
    public class InventoryConfig : MonoBehaviour, IEditorOnly
    {
        [SerializeField] private string _customMenuName;
        public string CustomMenuName => _customMenuName;
        [SerializeField] private Texture2D _customIcon;
        public Texture2D CustomIcon => _customIcon;

        private void Reset()
        {
            _customMenuName = "Inventory";
        }
    }
}