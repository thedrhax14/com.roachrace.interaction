using UnityEngine;

namespace RoachRace.Interaction
{
    /// <summary>
    /// Inventory-specific behavior for an <see cref="ItemDefinition"/>.<br/>
    ///<br/>
    /// Typical usage:<br/>
    /// - Create one <see cref="InventoryItemRules"/> asset per item (or share between items with identical behavior).<br/>
    /// - Assign it on the <see cref="ItemDefinition"/> to control stacking/consumption/drop rules without coupling the core item identity to the inventory system.<br/>
    ///<br/>
    /// Notes:<br/>
    /// - These rules are optional; when missing, runtime code should fall back to safe defaults (non-stackable, max stack 1, not consumed on use, droppable).<br/>
    /// </summary>
    [CreateAssetMenu(menuName = "RoachRace/Items/Rules/Inventory Item Rules")]
    public sealed class InventoryItemRules : ScriptableObject
    {
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
        /// Returns a safe stack size value ($\ge 1$) for runtime use.<br/>
        /// This protects against invalid authoring values.
        /// </summary>
        public int MaxStackClamped => Mathf.Max(1, maxStack);
    }
}
