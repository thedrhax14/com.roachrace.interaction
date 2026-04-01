using UnityEngine;

namespace RoachRace.Interaction
{
    /// <summary>
    /// Declares whether an inventory item should occupy the visible/selectable slot prefix or the hidden suffix.<br/>
    /// Typical usage: choose <see cref="VisibleSlots"/> for items that should appear in the HUD and be selectable via input, and <see cref="HiddenSlots"/> for owned items that gameplay tracks by item id but the player must not select directly.<br/>
    /// Configuration/context: <see cref="NetworkPlayerInventory"/> keeps visible slots in indices 0..9 and stores hidden slots after that prefix.
    /// </summary>
    public enum InventorySlotVisibility
    {
        /// <summary>
        /// Stores the item in the visible/selectable slot prefix.<br/>
        /// Typical usage: weapons, tools, and any item the player should be able to hotkey or select from the HUD.
        /// </summary>
        VisibleSlots = 0,

        /// <summary>
        /// Stores the item in the hidden/non-selectable slot suffix.<br/>
        /// Typical usage: passive ownership items, hidden resources, and other stacks that should not appear in the HUD.
        /// </summary>
        HiddenSlots = 1,
    }

    /// <summary>
    /// Inventory-specific behavior for an <see cref="ItemDefinition"/>.<br/>
    ///<br/>
    /// Typical usage:<br/>
    /// - Create one <see cref="InventoryItemRules"/> asset per item (or share between items with identical behavior).<br/>
    /// - Assign it on the <see cref="ItemDefinition"/> to control stacking, storage visibility, consumption, and drop rules without coupling the core item identity to the inventory system.<br/>
    ///<br/>
    /// Notes:<br/>
    /// - These rules are optional; when missing, runtime code should fall back to safe defaults (visible slots, non-stackable, max stack 1, not consumed on use, droppable).<br/>
    /// </summary>
    [CreateAssetMenu(menuName = "RoachRace/Items/Rules/Inventory Item Rules")]
    public sealed class InventoryItemRules : ScriptableObject
    {
        [Tooltip("Whether this item should be stored in visible/selectable slots or in the hidden suffix of the inventory.")]
        public InventorySlotVisibility slotVisibility = InventorySlotVisibility.VisibleSlots;

        [Tooltip("If true, inventory can stack multiple copies into one slot.")]
        public bool stackable;

        [Tooltip("Maximum stack size when stackable is true.")]
        [Min(1)]
        public int maxStack = 1;

        [Tooltip("If true, using this item consumes one charge from the inventory stack.")]
        public bool consumesInventoryOnUse;

        [Tooltip("If false, this item cannot be dropped from inventory.")]
        public bool canDrop = true;

        /// <summary>
        /// Returns where this item should be stored within the player's inventory.<br/>
        /// This defaults to <see cref="InventorySlotVisibility.VisibleSlots"/> when no override is authored.
        /// </summary>
        public InventorySlotVisibility SlotVisibility => slotVisibility;

        /// <summary>
        /// Returns a safe stack size value ($\ge 1$) for runtime use.<br/>
        /// This protects against invalid authoring values.
        /// </summary>
        public int MaxStackClamped => Mathf.Max(1, maxStack);
    }
}
