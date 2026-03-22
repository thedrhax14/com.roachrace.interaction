using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RoachRace.Interaction
{
    /// <summary>
    /// Defines an item (asset identity + UI presentation) by id.<br/>
    ///<br/>
    /// Asset setup:<br/>
    /// - Create one asset per item via Create &gt; RoachRace &gt; Items &gt; Item Definition.<br/>
    /// - <see cref="id"/> must be unique across your project. 0 is reserved for an empty inventory slot.<br/>
    /// - <see cref="icon"/> is optional.<br/>
    /// - Optional behavior is configured via rule assets (<see cref="inventoryRules"/>, <see cref="worldPickupRules"/>) so that inventory/world semantics do not pollute the core identity.
    /// </summary>
    [CreateAssetMenu(menuName = "RoachRace/Items/Item Definition")]
    public sealed class ItemDefinition : ScriptableObject
    {
        [Tooltip("0 is reserved for empty slots.")]
        public ushort id = 1;

        [Tooltip("Name displayed in UI (if you show names).")]
        public string displayName = "Item";

        [Tooltip("Optional icons. UI can choose which to display based on role.")]
        public Sprite icon;

        [Header("Rules (Optional)")]
        [Tooltip("Optional. Inventory-specific behavior (stacking, consumes-on-use, droppable, etc).")]
        public InventoryItemRules inventoryRules;

        [Tooltip("Optional. World/pickup behavior (eg prefab spawned when dropped).")]
        public WorldPickupRules worldPickupRules;

        /// <summary>
        /// Returns true when this item is configured as stackable in inventory.<br/>
        /// When <see cref="inventoryRules"/> is not assigned, defaults to <c>false</c>.
        /// </summary>
        public bool IsStackable => inventoryRules != null && inventoryRules.stackable;

        /// <summary>
        /// Returns the effective max stack size for this item in inventory ($\ge 1$).<br/>
        /// When <see cref="inventoryRules"/> is not assigned, defaults to 1.
        /// </summary>
        public int MaxStack => inventoryRules != null ? inventoryRules.MaxStackClamped : 1;

        /// <summary>
        /// Returns true when using this item should consume one unit from the selected inventory stack.<br/>
        /// When <see cref="inventoryRules"/> is not assigned, defaults to <c>false</c>.
        /// </summary>
        public bool ConsumesInventoryOnUse => inventoryRules != null && inventoryRules.consumesInventoryOnUse;

        /// <summary>
        /// Returns true if this item may be dropped from inventory.<br/>
        /// When <see cref="inventoryRules"/> is not assigned, defaults to <c>true</c>.
        /// </summary>
        public bool CanDropFromInventory => inventoryRules == null || inventoryRules.canDrop;

        /// <summary>
        /// Returns the world pickup prefab for this item (if configured).<br/>
        /// When <see cref="worldPickupRules"/> is not assigned, returns null.
        /// </summary>
        public GameObject WorldPickupPrefab => worldPickupRules != null ? worldPickupRules.worldPickupPrefab : null;
#if UNITY_EDITOR
        private static bool _isRenamingAsset;

        public void OnValidate()
        {
            var desiredName = $"item_def_{id}_{displayName.Trim().ToLowerInvariant().Replace(" ", "_")}";
            name = desiredName;

            // Renaming the ScriptableObject's `name` changes the instance label, not the .asset file name.
            // This schedules an asset rename so the Project file name stays in sync too.
            if (_isRenamingAsset)
                return;

            var path = AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(path))
                return;

            if (!AssetDatabase.IsMainAsset(this))
                return;

            var currentFileName = System.IO.Path.GetFileNameWithoutExtension(path);
            if (string.Equals(currentFileName, desiredName, System.StringComparison.Ordinal))
                return;

            _isRenamingAsset = true;
            EditorApplication.delayCall += () =>
            {
                try
                {
                    // AssetDatabase.RenameAsset returns an error string; empty means success.
                    AssetDatabase.RenameAsset(path, desiredName);
                }
                finally
                {
                    _isRenamingAsset = false;
                }
            };
        }
#endif
    }
}
