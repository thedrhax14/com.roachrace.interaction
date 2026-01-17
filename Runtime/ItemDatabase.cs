using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoachRace.Interaction
{
    /// <summary>
    /// Lookup table for ItemDefinition assets.
    /// 
    /// Asset setup:
    /// - Create via Create > RoachRace > Items > Item Database.
    /// - Populate the items list with all ItemDefinition assets you want available at runtime.
    /// - If multiple definitions share the same id, the last one in the list wins.
    /// 
    /// Runtime:
    /// - Used by UI and inventory logic to resolve an item id to icons/metadata.
    /// </summary>
    [CreateAssetMenu(menuName = "RoachRace/Items/Item Database")]
    public sealed class ItemDatabase : ScriptableObject
    {
        [Tooltip("All ItemDefinition assets available in this build.")]
        [SerializeField] private List<ItemDefinition> items = new();

        private Dictionary<ushort, ItemDefinition> _byId;

        public bool TryGet(ushort id, out ItemDefinition def)
        {
            EnsureIndex();
            return _byId.TryGetValue(id, out def);
        }

        private void EnsureIndex()
        {
            if (_byId != null) return;

            _byId = new Dictionary<ushort, ItemDefinition>(items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                ItemDefinition def = items[i];
                if (def == null) continue;

                _byId[def.id] = def;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Rebuild index when edited.
            _byId = null;
        }
#endif
    }
}
