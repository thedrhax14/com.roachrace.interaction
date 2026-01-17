using UnityEngine;

namespace RoachRace.Interaction
{
    /// <summary>
    /// Defines a gameplay item by id and UI presentation.
    /// 
    /// Asset setup:
    /// - Create one asset per item via Create > RoachRace > Items > Item Definition.
    /// - id must be unique across your project. 0 is reserved for an empty inventory slot.
    /// - Icons are optional; the UI will fall back to survivorIcon if ghostIcon is missing.
    /// - If stackable is false, maxStack should remain 1.
    /// </summary>
    [CreateAssetMenu(menuName = "RoachRace/Items/Item Definition")]
    public sealed class ItemDefinition : ScriptableObject
    {
        [Tooltip("0 is reserved for empty slots.")]
        public ushort id = 1;

        [Tooltip("Name displayed in UI (if you show names).")]
        public string displayName = "Item";

        [Tooltip("If true, selecting this item should also immediately use it.")]
        public bool useOnSelect;

        [Tooltip("Optional icons. UI can choose which to display based on role.")]
        public Sprite survivorIcon;
        public Sprite ghostIcon;

        [Tooltip("If true, inventory can stack multiple copies into one slot.")]
        public bool stackable;

        [Tooltip("If true, using this item consumes one charge from the inventory stack.")]
        public bool consumesInventoryOnUse;

        [Tooltip("Maximum stack size when stackable is true.")]
        [Min(1)] public int maxStack = 1;
    }
}
