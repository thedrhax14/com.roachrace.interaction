using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RoachRace.Interaction
{
    /// <summary>
    /// Defines a gameplay item by id and UI presentation.
    /// 
    /// Asset setup:
    /// - Create one asset per item via Create > RoachRace > Items > Item Definition.
    /// - id must be unique across your project. 0 is reserved for an empty inventory slot.
    /// - Icon is optional
    /// - If stackable is false, maxStack should remain 1.
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

        [Tooltip("If true, inventory can stack multiple copies into one slot.")]
        public bool stackable;

        [Tooltip("If true, using this item consumes one charge from the inventory stack.")]
        public bool consumesInventoryOnUse;

        [Header("Inventory Rules")]
        [Tooltip("If false, this item cannot be dropped from inventory.")]
        public bool canDrop = true;

        [Header("World")]
        [Tooltip("Optional. Prefab to spawn in the world when this item is dropped. Must contain a FishNet NetworkObject + NetworkItemPickup.")]
        public GameObject worldPickupPrefab;

        [Tooltip("Maximum stack size when stackable is true.")]
        [Min(1)] public int maxStack = 1;

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
