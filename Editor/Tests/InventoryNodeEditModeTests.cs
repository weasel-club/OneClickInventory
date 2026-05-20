using System.Linq;
using Goorm.OneClickInventory.runtime;
using NUnit.Framework;

namespace Goorm.OneClickInventory.Tests
{
    public class InventoryNodeEditModeTests : EditModeTestBase
    {
        [Test]
        public void ResolveRootNodes_FindsInventoriesBelowPlainFolders()
        {
            var folder = CreateChild("Folder", Avatar.transform);
            var rootObject = CreateChild("Wardrobe", folder.transform);
            var childObject = CreateChild("Hat", rootObject.transform);
            var rootInventory = AddInventory(rootObject, "Wardrobe");
            var childInventory = AddInventory(childObject, "Hat");

            var root = InventoryNode.ResolveRootNodes(Avatar).Single();
            var child = root.Children.Single();

            Assert.That(root.Value, Is.EqualTo(rootInventory));
            Assert.That(child.Value, Is.EqualTo(childInventory));
            Assert.That(child.Parent, Is.EqualTo(root));
            Assert.That(child.Root, Is.EqualTo(root));
        }

        [Test]
        public void ResolveRootNodes_DeduplicatesEqualNamesInParameterKeys()
        {
            var rootObject = CreateChild("Root", Avatar.transform);
            AddInventory(rootObject, "Root");
            AddInventory(CreateChild("Item A", rootObject.transform), "Same Item");
            AddInventory(CreateChild("Item B", rootObject.transform), "Same Item");

            var children = InventoryNode.ResolveRootNodes(Avatar).Single().Children.ToArray();

            Assert.That(children[0].Key, Is.EqualTo("OCInv/Root/Same_Item"));
            Assert.That(children[1].Key, Is.EqualTo("OCInv/Root/Same_Item_2"));
        }

        [Test]
        public void UniqueInventory_AssignsDefaultIndexZeroAndEncodesChildParameters()
        {
            var rootObject = CreateChild("Root", Avatar.transform);
            var rootInventory = AddInventory(rootObject, "Root");
            SetSerializedValue(rootInventory, "_isUnique", true);
            SetSerializedValue(rootInventory, "_saved", false);

            var defaultInventory = AddInventory(CreateChild("Default", rootObject.transform), "Default");
            SetSerializedValue(defaultInventory, "_default", true);
            var secondInventory = AddInventory(CreateChild("Second", rootObject.transform), "Second");
            var thirdInventory = AddInventory(CreateChild("Third", rootObject.transform), "Third");

            var root = InventoryNode.ResolveRootNodes(Avatar).Single();
            var children = root.ChildItems.ToArray();

            Assert.That(children.Select(e => e.Index), Is.EqualTo(new[] { 0, 1, 2 }));
            Assert.That(root.ChildrenBits, Is.EqualTo(2));
            Assert.That(root.UsedParameterMemory, Is.EqualTo(2));
            Assert.That(children[0].ParameterName, Is.EqualTo(root.Key));
            Assert.That(children[0].ParameterValue, Is.EqualTo(0));
            Assert.That(children[1].ParameterValue, Is.EqualTo(1));
            Assert.That(children[2].ParameterValue, Is.EqualTo(2));
            Assert.That(children[0].ParameterDefault, Is.EqualTo(0));
            Assert.That(children[1].ParameterBits, Is.EqualTo(2));
        }

        [Test]
        public void NonUniqueInventory_UsesPerItemToggleParametersAndDefaults()
        {
            var rootObject = CreateChild("Root", Avatar.transform);
            AddInventory(rootObject, "Root");
            var itemInventory = AddInventory(CreateChild("Hat", rootObject.transform), "Hat");
            SetSerializedValue(itemInventory, "_default", true);

            var item = InventoryNode.ResolveRootNodes(Avatar).Single().ChildItems.Single();

            Assert.That(item.ParameterName, Is.EqualTo("OCInv/Root/Hat/Toggle"));
            Assert.That(item.ParameterValue, Is.EqualTo(1));
            Assert.That(item.ParameterBits, Is.EqualTo(1));
            Assert.That(item.ParameterDefault, Is.EqualTo(1));
        }

        [Test]
        public void NotItemNode_DoesNotCreateItemButKeepsChildren()
        {
            var rootObject = CreateChild("Root", Avatar.transform);
            AddInventory(rootObject, "Root");
            var groupInventory = AddInventory(CreateChild("Group", rootObject.transform), "Group");
            SetSerializedValue(groupInventory, "_isNotItem", true);
            AddInventory(CreateChild("Nested", groupInventory.transform), "Nested");

            var root = InventoryNode.ResolveRootNodes(Avatar).Single();
            var group = root.Children.Single();
            var nested = group.Children.Single();

            Assert.That(group.IsItem, Is.False);
            Assert.That(group.IsInventory, Is.True);
            Assert.That(nested.IsItem, Is.True);
            Assert.That(nested.ParameterName, Is.EqualTo("OCInv/Root/Group/Nested/Toggle"));
        }

        [Test]
        public void Validate_UniqueInventoryKeepsOnlyFirstDefaultChild()
        {
            var rootObject = CreateChild("Root", Avatar.transform);
            var rootInventory = AddInventory(rootObject, "Root");
            SetSerializedValue(rootInventory, "_isUnique", true);
            var first = AddInventory(CreateChild("First", rootObject.transform), "First");
            var second = AddInventory(CreateChild("Second", rootObject.transform), "Second");
            SetSerializedValue(first, "_default", true);
            SetSerializedValue(second, "_default", true);

            InventoryNode.ResolveRootNodes(Avatar).Single().Validate();

            Assert.That(first.Default, Is.True);
            Assert.That(second.Default, Is.False);
        }

        [Test]
        public void ResolveRootNodes_HandlesMixedDeepInventoryTree()
        {
            var plainRootFolder = CreateChild("Plain Root Folder", Avatar.transform);
            var wardrobeObject = CreateChild("Wardrobe Object", plainRootFolder.transform);
            var wardrobe = AddInventory(wardrobeObject, "Main Wardrobe");
            SetSerializedValue(wardrobe, "_isUnique", true);

            var outfitFolder = CreateChild("Outfit Folder", wardrobeObject.transform);
            var casual = AddInventory(CreateChild("Casual Object", outfitFolder.transform), "Same Outfit");
            SetSerializedValue(casual, "_default", true);
            AddInventory(CreateChild("Formal Object", outfitFolder.transform), "Same Outfit");

            var accessoryGroup = AddInventory(CreateChild("Accessory Group", wardrobeObject.transform), "Accessories");
            SetSerializedValue(accessoryGroup, "_isNotItem", true);
            var hat = AddInventory(CreateChild("Hat Object", accessoryGroup.transform), "Hat");

            var toolsRoot = AddInventory(CreateChild("Tools Root", Avatar.transform), "Tools");
            var toolsFolder = CreateChild("Tools Folder", toolsRoot.transform);
            var wrench = AddInventory(CreateChild("Wrench Object", toolsFolder.transform), "Wrench");

            var roots = InventoryNode.ResolveRootNodes(Avatar).ToArray();
            var wardrobeNode = roots.Single(e => e.Value == wardrobe);
            var toolsNode = roots.Single(e => e.Value == toolsRoot);
            var wardrobeChildren = wardrobeNode.Children.ToArray();
            var accessoryNode = wardrobeChildren.Single(e => e.Value == accessoryGroup);
            var accessoryChild = accessoryNode.Children.Single();
            var toolsChild = toolsNode.Children.Single();

            Assert.That(roots.Select(e => e.Key), Is.EqualTo(new[] { "OCInv/Main_Wardrobe", "OCInv/Tools" }));
            Assert.That(wardrobeChildren.Select(e => e.Key),
                Is.EqualTo(new[]
                {
                    "OCInv/Main_Wardrobe/Same_Outfit",
                    "OCInv/Main_Wardrobe/Same_Outfit_2",
                    "OCInv/Main_Wardrobe/Accessories"
                }));
            Assert.That(wardrobeChildren.Select(e => e.Index), Is.EqualTo(new[] { 0, 1, -1 }));
            Assert.That(wardrobeChildren.Select(e => e.IsItem), Is.EqualTo(new[] { true, true, false }));
            Assert.That(wardrobeNode.ChildrenBits, Is.EqualTo(1));
            Assert.That(wardrobeNode.UsedParameterMemory, Is.EqualTo(2));
            Assert.That(accessoryNode.IsInventory, Is.True);
            Assert.That(accessoryNode.UsedParameterMemory, Is.EqualTo(1));
            Assert.That(accessoryChild.Value, Is.EqualTo(hat));
            Assert.That(accessoryChild.ParameterName, Is.EqualTo("OCInv/Main_Wardrobe/Accessories/Hat/Toggle"));
            Assert.That(toolsChild.Value, Is.EqualTo(wrench));
            Assert.That(toolsChild.ParameterName, Is.EqualTo("OCInv/Tools/Wrench/Toggle"));
        }

        [Test]
        public void FindNodeByValue_FindsDeepNodeInMixedTree()
        {
            var wardrobe = AddInventory(CreateChild("Wardrobe", Avatar.transform), "Wardrobe");
            var group = AddInventory(CreateChild("Group", wardrobe.transform), "Group");
            SetSerializedValue(group, "_isNotItem", true);
            var nestedFolder = CreateChild("Nested Folder", group.transform);
            var target = AddInventory(CreateChild("Target", nestedFolder.transform), "Target Item");

            var found = InventoryNode.FindNodeByValue(Avatar, target);

            Assert.That(found, Is.Not.Null);
            Assert.That(found.Value, Is.EqualTo(target));
            Assert.That(found.Key, Is.EqualTo("OCInv/Wardrobe/Group/Target_Item"));
            Assert.That(found.Parent.Value, Is.EqualTo(group));
        }
    }
}
