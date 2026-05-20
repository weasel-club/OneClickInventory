using UnityEngine;
using VRC.SDKBase;

namespace Goorm.OneClickInventory.runtime
{
    [AddComponentMenu("One-Click Inventory/Inventory Active Parameter")]
    public class InventoryActiveParameter : MonoBehaviour, IEditorOnly
    {
        [SerializeField] private string _parameterName;

        public string ParameterName => _parameterName;
    }
}
