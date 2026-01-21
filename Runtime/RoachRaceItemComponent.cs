using UnityEngine;

namespace RoachRace.Interaction
{
    /// <summary>
    /// Base MonoBehaviour implementation of IRoachRaceItem.
    /// 
    /// Setup:
    /// - Derive from this (or RoachRaceItemBase) to implement usable items.
    /// - Place the item as a child GameObject under the player and register it via ItemInstance + PlayerItemRegistry.
    /// 
    /// Notes:
    /// - Inventory selection toggles items via SetVisibility (only the selected item is shown).
    /// - Default SetVisibility uses GameObject.SetActive; this means unselected items will have OnDisable/OnEnable called.
    ///   If your item needs to keep running while "hidden" (eg placement previews), override SetVisibility and
    ///   hide visuals without deactivating the GameObject.
    /// - By itself an item shows up in the inventory only if granted into slots by the server either via initialItems or pickups.
    /// </summary>
    public abstract class RoachRaceItemComponent : MonoBehaviour, IRoachRaceItem
    {
        public abstract Transform UseSource { get; }

        [Tooltip("Optional root GameObject for visual representation of this item. Used by SetVisibility.")]
        public GameObject visualRoot;

        public abstract void InitializeUseContext(int seed, int instigatorId, bool isServer, GameObject instigatorObject);

        public virtual void OnEquipped() { }
        public virtual float OnUnEquipped() => 0f;

        public virtual void UseStart() { }
        public virtual void UseStop() { }

        public virtual void OnAim(bool isAiming) { }

        public virtual void SetVisibility(bool isVisible) { 
            if(visualRoot != null)
                visualRoot.SetActive(isVisible);
        }
    }
}