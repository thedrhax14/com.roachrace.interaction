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
            foreach (var kvp in _itemsById)
            {
                bool isActive = kvp.Key == itemId;
                var inst = kvp.Value;
                if (inst?.ItemComponent != null)
                    inst.ItemComponent.SetVisibility(isActive);
            }
        }

        public void HideAll()
        {
            foreach (var kvp in _itemsById)
            {
                var inst = kvp.Value;
                if (inst?.ItemComponent != null)
                    inst.ItemComponent.SetVisibility(false);
            }
        }
    }
}
