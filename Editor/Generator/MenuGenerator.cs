using System.Linq;
using Goorm.OneClickInventory.runtime;
using nadena.dev.modular_avatar.core;
using nadena.dev.modular_avatar.core.menu;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace Goorm.OneClickInventory
{
    public abstract class MenuGenerator
    {
        private const string GeneratedMenuRootName = "OneClickInventory Generated Menus";

        private static Transform FindDirectChild(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                {
                    return child;
                }
            }

            return null;
        }

        private static ModularAvatarMenuItem AddSubmenu(string name, Texture2D icon, Transform parent)
        {
            var existingSiblingWithName = FindDirectChild(parent, name);
            var menuObject = new GameObject(name);
            menuObject.transform.SetParent(parent, false);
            if (existingSiblingWithName) menuObject.transform.SetSiblingIndex(existingSiblingWithName.GetSiblingIndex());
            var menu = menuObject.AddComponent<ModularAvatarMenuItem>();

            menu.Control = new VRCExpressionsMenu.Control
            {
                name = name,
                icon = icon,
                type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                value = 0,
            };
            menu.MenuSource = SubmenuSource.Children;
            return menu;
        }

        private static ModularAvatarMenuItem AddToggleMenu(
            string name, Texture2D icon, string parameter, int value, Transform parent
        )
        {
            var existingSiblingWithName = FindDirectChild(parent, name);
            var menuObject = new GameObject(name);
            menuObject.transform.SetParent(parent, false);
            if (existingSiblingWithName) menuObject.transform.SetSiblingIndex(existingSiblingWithName.GetSiblingIndex());
            var menu = menuObject.AddComponent<ModularAvatarMenuItem>();

            menu.Control = new VRCExpressionsMenu.Control
            {
                name = name,
                icon = icon,
                type = VRCExpressionsMenu.Control.ControlType.Toggle,
                parameter = new VRCExpressionsMenu.Control.Parameter
                {
                    name = parameter
                },
                value = value
            };
            return menu;
        }

        private static ModularAvatarMenuItem CreateMaMenu(InventoryNode node, Transform parent)
        {
            var menuInstaller = node.IntegratedMenuInstaller;
            ModularAvatarMenuItem menu = null;

            var menuItemsToInstall = node.MenuItemsToInstall.ToArray();

            // If it should be generated as a submenu
            if (node.ShouldBeSubmenu)
            {
                menu = AddSubmenu(node.Value.Name, node.Value.Icon, parent);
                if (node.IsItem)
                    AddToggleMenu(L.Get("enable"), node.Value.Icon, node.ParameterName, node.ParameterValue,
                        menu.transform);
            }
            // Else if it should be generated as a toggle menu
            else if (node.IsItem)
            {
                menu = AddToggleMenu(node.Value.Name, node.Value.Icon, node.ParameterName, node.ParameterValue, parent);
            }

            // Copy menu installer to generated menu object
            if (menu && menuInstaller)
            {
                var newMenuInstaller = menu.gameObject.AddComponent<ModularAvatarMenuInstaller>();
                newMenuInstaller.menuToAppend = menuInstaller.menuToAppend;
                newMenuInstaller.installTargetMenu = menuInstaller.installTargetMenu;

                // Replace all reference to the original installer with the new one
                foreach (var component in node.Avatar.GetComponentsInChildren<MenuSourceComponent>())
                {
                    if (component.GetType().Name == "ModularAvatarMenuInstallTarget")
                    {
                        var installerField = component.GetType().GetField("installer");
                        var targetInstaller = installerField.GetValue(component) as ModularAvatarMenuInstaller;

                        if (targetInstaller != menuInstaller) continue;

                        // Set serialized field "installer" to the new installer
                        installerField.SetValue(component, newMenuInstaller);
                        EditorUtility.SetDirty(component);
                    }
                }

                // Remove original installer
                Object.DestroyImmediate(menuInstaller);
            }

            // Recursively create children
            foreach (var child in node.Children)
            {
                CreateMaMenu(child, menu?.transform ?? parent);
            }

            // Add menus installed by InventoryMenuInstaller
            if (menu)
                foreach (var menuItem in menuItemsToInstall)
                    menuItem.transform.SetParent(menu.transform, false);

            return menu;
        }

        public static void Generate(VRCAvatarDescriptor avatar, InventoryNode[] rootNodes)
        {
            var menuRoot = new GameObject(GeneratedMenuRootName);
            menuRoot.transform.SetParent(avatar.transform, false);

            foreach (var node in rootNodes)
            {
                // Generate menu
                var menuItem = CreateMaMenu(node, menuRoot.transform);
                if (menuItem == null) continue;

                var installer = menuItem.gameObject.AddComponent<ModularAvatarMenuInstaller>();
                installer.menuToAppend = avatar.expressionsMenu;
            }
        }
    }
}
