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
    ///   (e.g., via NetworkPlayerInventory.initialItems or via pickups).
    /// - If itemComponent is not assigned, Awake will attempt to find it on the same GameObject.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ItemInstance : MonoBehaviour
    {
        [Tooltip("ItemDefinition asset that provides the item id and UI metadata.")]
        [SerializeField] private ItemDefinition definition;

        [Tooltip("Concrete item logic component for this item. If omitted, will be auto-found on this GameObject in Awake.")]
        [SerializeField] private RoachRaceItemComponent itemComponent;

        public ushort ItemId => definition != null ? definition.id : (ushort)0;
        public ItemDefinition Definition => definition;
        public RoachRaceItemComponent ItemComponent => itemComponent;

        private void Awake()
        {
            if (itemComponent == null)
            {
                itemComponent = GetComponent<RoachRaceItemComponent>();
            }

            if (itemComponent == null)
            {
                Debug.LogError($"[{nameof(ItemInstance)}] ItemComponent is not assigned and no {nameof(RoachRaceItemComponent)} was found on '{gameObject.name}'.", gameObject);
                throw new System.NullReferenceException($"[{nameof(ItemInstance)}] Missing RoachRaceItemComponent on '{gameObject.name}'.");
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if(definition == null) return;
            gameObject.name = $"ItemInstance_{definition.displayName}_(id{definition.id})";
        }
#endif
    }
}
