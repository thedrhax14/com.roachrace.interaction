using UnityEngine;

namespace RoachRace.Interaction
{
    /// <summary>
    /// Binds an ItemDefinition (id + UI metadata) to a concrete in-scene item implementation.
    /// 
    /// Typical setup:
    /// - Put a PlayerItemRegistry on the player root (or an appropriate child).
    /// - Add one child GameObject per usable item and attach:
    ///   - A RoachRaceItemComponent (your item logic)
    ///   - This ItemInstance, referencing the ItemDefinition for the id.
    /// 
    /// Notes:
    /// - The inventory selects items by itemId; PlayerItemRegistry uses ItemInstance to find the matching RoachRaceItemComponent.
    /// - Having an ItemInstance child only means the character *can* use this item implementation.
    ///   For the item to actually appear in the inventory at runtime, it must be granted into slots
    ///   (e.g., via an InventoryLoadout assigned on NetworkPlayerInventory or via pickups).
    /// - If itemComponent is not assigned, Awake will attempt to find it on the same GameObject.
    /// </summary>
    [RequireComponent(typeof(RoachRaceItemComponent))]
    public sealed class ItemInstance : MonoBehaviour
    {
        [Tooltip("ItemDefinition asset that provides the item id and UI metadata.")]
        [SerializeField] private ItemDefinition definition;
        [SerializeField] private RoachRaceItemComponent itemComponent;

        public ushort ItemId => definition != null ? definition.id : (ushort)0;
        public ItemDefinition Definition => definition;
        public RoachRaceItemComponent ItemComponent => itemComponent == null ? GetComponent<RoachRaceItemComponent>() : itemComponent;

        /// <summary>
        /// Item component instance must assign itself here on initialization.
        /// </summary>
        /// <param name="component"></param>
        public void SetItemComponent(RoachRaceItemComponent component)
        {
            itemComponent = component;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if(definition == null) return;
            gameObject.name = $"item_{definition.id}_{definition.displayName.Trim().ToLowerInvariant().Replace(" ", "_")}";
        }
#endif
    }
}
