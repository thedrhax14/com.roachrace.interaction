using System.Collections.Generic;
using UnityEngine;

namespace RoachRace.Interaction
{
    /// <summary>
    /// Index of a player's available item implementations.
    /// 
    /// Scene/prefab setup:
    /// - Attach to the player GameObject (or a stable child).
    /// - Add ItemInstance components in children (active or inactive) for every usable item.
    /// - Each ItemInstance must reference an ItemDefinition with a non-zero id.
    /// 
    /// Runtime:
    /// - Awake scans children and builds a dictionary from itemId -> ItemInstance.
    /// - Inventory selection calls SetOnlyActive(itemId) so only the selected item is visible.
    /// 
    /// Important:
    /// - This registry only provides implementations for item ids.
    /// - Items appear in the inventory only if the server grants them into slots
    ///   (e.g., NetworkPlayerInventory.initialItems or via pickups).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerItemRegistry : MonoBehaviour
    {
        private readonly Dictionary<ushort, ItemInstance> _itemsById = new();

        private ushort _activeItemId;
        private ItemInstance _activeInstance;

        private void Awake()
        {
            _itemsById.Clear();

            var items = GetComponentsInChildren<ItemInstance>(true);
            for (int i = 0; i < items.Length; i++)
            {
                ItemInstance inst = items[i];
                if (inst == null) continue;
                ushort id = inst.ItemId;
                if (id == 0) continue;

                _itemsById[id] = inst;
            }

            _activeItemId = 0;
            _activeInstance = null;
        }

        private static IRoachRaceItem GetItem(ItemInstance instance)
        {
            if (instance == null) return null;
            return instance.ItemComponent;
        }

        public bool TryGetItem(ushort itemId, out IRoachRaceItem item)
        {
            item = null;
            if (!_itemsById.TryGetValue(itemId, out var inst) || inst == null) return false;
            item = inst.ItemComponent;
            return item != null;
        }

        public bool TryGetItemInstance(ushort itemId, out ItemInstance instance)
        {
            instance = null;
            if (!_itemsById.TryGetValue(itemId, out var inst) || inst == null) return false;
            instance = inst;
            return true;
        }

        public void SetOnlyActive(ushort itemId)
        {
            _itemsById.TryGetValue(itemId, out var nextInstance);

            // No state change: only enforce visibility.
            if (_activeInstance == nextInstance)
            {
                foreach (var kvp in _itemsById)
                {
                    bool shouldBeVisible = kvp.Key == itemId;
                    var inst = kvp.Value;
                    inst?.ItemComponent?.SetVisibility(shouldBeVisible);
                }
                return;
            }

            // Transition: unequip old while still visible.
            var prevItem = GetItem(_activeInstance);
            if (prevItem != null)
            {
                prevItem.Unequip();
                prevItem.OnUnequipped();
            }

            // Apply visibility for all.
            foreach (var kvp in _itemsById)
            {
                bool shouldBeVisible = kvp.Key == itemId;
                var inst = kvp.Value;
                inst?.ItemComponent?.SetVisibility(shouldBeVisible);
            }

            // Equip new after it has been made visible (important for items which SetActive(true/false)).
            var nextItem = GetItem(nextInstance);
            if (nextItem != null)
            {
                nextItem.Equip();
                nextItem.OnEquipped();
            }

            _activeItemId = itemId;
            _activeInstance = nextInstance;
        }

        public void HideAll()
        {
            var prevItem = GetItem(_activeInstance);
            if (prevItem != null)
            {
                prevItem.Unequip();
                prevItem.OnUnequipped();
            }

            foreach (var kvp in _itemsById)
            {
                var inst = kvp.Value;
                inst?.ItemComponent?.SetVisibility(false);
            }

            _activeItemId = 0;
            _activeInstance = null;
        }
    }
}
