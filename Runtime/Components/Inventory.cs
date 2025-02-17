using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;

namespace dog.miruku.inventory.runtime
{
    [Serializable]
    public struct SetBlendShapeBinding
    {
        public SkinnedMeshRenderer renderer;
        public string name;
        public int value;
    }

    [Serializable]
    public struct ReplaceMaterialBinding
    {
        public Renderer renderer;
        public Material from;
        public Material to;
    }

    [Serializable]
    public struct ParameterDriverBinding
    {
        public VRC_AvatarParameterDriver.Parameter parameter;
    }

    [DisallowMultipleComponent]
    [AddComponentMenu("One-Click Inventory/Inventory")]
    public class Inventory : MonoBehaviour, IEditorOnly
    {
        [SerializeField] private bool _installMenuInRoot;

        public bool InstallMenuInRoot => _installMenuInRoot;

        [FormerlySerializedAs("_closetName")] [SerializeField]
        private string _name;
        public string Name => _name;

        [SerializeField] private Texture2D _customIcon;

        public Texture2D Icon
        {
            get => _customIcon;
            set => _customIcon = value;
        }

        // properties as a inventory
        [SerializeField] private bool _isUnique;

        public bool IsUnique => _isUnique;

        // properties as a item
        [SerializeField] private bool _default;

        public bool Default
        {
            get => _default;
            set => _default = value;
        }

        [SerializeField] private List<GameObject> _additionalObjects = new();

        public IEnumerable<GameObject> GameObjects =>
            new[] {gameObject}.Concat(_additionalObjects.Where(e => e != null));

        [SerializeField] private List<AnimationClip> _additionalAnimations = new();

        public IEnumerable<AnimationClip> AdditionalAnimations => _additionalAnimations.Where(e => e != null);

        [SerializeField] private List<GameObject> _objectsToDisable = new();
        public IEnumerable<GameObject> ObjectsToDisable => _objectsToDisable.Where(e => e != null);

        [SerializeField] private List<SetBlendShapeBinding> _blendShapesToChange = new();

        public IEnumerable<SetBlendShapeBinding> BlendShapesToChange =>
            _blendShapesToChange.Where(e => e.renderer != null);

        [SerializeField] private List<ReplaceMaterialBinding> _materialsToReplace = new();

        public IEnumerable<ReplaceMaterialBinding> MaterialsToReplace =>
            _materialsToReplace.Where(e => e.renderer != null && e.from != null && e.to != null);

        [SerializeField] private List<ParameterDriverBinding> _parameterDriverBindings = new();

        public IEnumerable<ParameterDriverBinding> ParameterDriverBindings => _parameterDriverBindings;

        [SerializeField] private int _layerPriority;

        public int LayerPriority => _layerPriority;

        [SerializeField] private bool _isNotItem;

        public bool IsNotItem => _isNotItem;

        [SerializeField] private bool _saved = true;

        public bool Saved => _saved;

        [SerializeField] private bool _integrateMenuInstaller;

        public bool IntegrateMenuInstaller => _integrateMenuInstaller;

        private void Reset()
        {
            _name = gameObject.name;
        }

        private void OnValidate()
        {
            if (!IsValidName(_name))
            {
                Debug.LogError($"[One-Click Inventory] 아이템의 이름에 사용할 수 없는 문자가 포함되어 있습니다: {_name}");
                Debug.LogError($"[One-Click Inventory] 아이템의 이름에 [<>:\"/\\|?*]는 사용할 수 없습니다.");
                _name = "Invalid_Name";
            }
        }

        private bool IsValidName(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && !Regex.IsMatch(input, "[<>:\"/\\|?*]");
        }
    }
}
