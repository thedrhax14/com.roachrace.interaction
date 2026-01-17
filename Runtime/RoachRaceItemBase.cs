using UnityEditor.EditorTools;
using UnityEngine;

namespace RoachRace.Interaction
{
    /// <summary>
    /// Convenience base class for most items.
    /// 
    /// Provides:
    /// - A configurable UseSource transform (ray origin / muzzle / hand).
    /// - Stored use context (Seed, InstigatorId, IsServer, InstigatorObject) via InitializeUseContext.
    /// 
    /// Lifecycle note:
    /// - The current inventory pipeline assumes item GameObjects are present as children under the player prefab
    ///   and are shown/hidden via SetVisibility (rather than being instantiated/destroyed on pickup/consume).
    /// - Any presentation references used by derived items (e.g., an Animator trigger on use) should generally
    ///   reference components that exist within the same instantiated player hierarchy (player/arms/item).
    ///   If you choose a spawned-items lifecycle, bind those references at runtime instead of via Inspector.
    /// 
    /// Alternative lifecycle (spawn/destroy on pickup/consume):
    /// - Inventory stores only data (itemId/count) and instantiates a "view" GameObject only when needed
    ///   (on pickup, on equip/select, or on first use), then destroys/returns it to a pool when consumed/unequipped.
    /// - In that setup, avoid Inspector references that assume a persistent hierarchy; instead bind at runtime
    ///   (e.g., find the instigator's Animator/arms rig in InitializeUseContext/OnEquipped, and assign useSource).
    /// - You would also change how selection works: rather than PlayerItemRegistry scanning pre-existing children,
    ///   selection/equip code would spawn the correct prefab for the selected itemId and keep a reference to it.
    /// 
    /// Prefab setup:
    /// - Assign useSource if your item needs to raycast/spawn from a specific point.
    /// - If unassigned, UseSource defaults to this transform.
    /// </summary>
    public abstract class RoachRaceItemBase : RoachRaceItemComponent
    {
        [Tooltip("Optional. Source transform for aiming/raycasting/spawning. Defaults to this transform if not set.")]
        [SerializeField] protected Transform useSource;
        [Tooltip("Optional. Next item to use after using this item. Can be null.")]
        public RoachRaceItemBase[] nextItemsToUse;

        protected int InstigatorId { get; private set; } = -1;
        protected bool IsServer { get; private set; }
        protected int Seed { get; private set; }
        protected GameObject InstigatorObject { get; private set; }

        public override Transform UseSource => useSource != null ? useSource : transform;

        public override void InitializeUseContext(int seed, int instigatorId, bool isServer, GameObject instigatorObject)
        {
            Seed = seed;
            InstigatorId = instigatorId;
            IsServer = isServer;
            InstigatorObject = instigatorObject;
            OnSeedChanged(seed);
            foreach(var nextItemToUse in nextItemsToUse)
                nextItemToUse.InitializeUseContext(seed, instigatorId, isServer, instigatorObject);
        }

        protected void UseNextItems()
        {
            if (nextItemsToUse == null) return;
            foreach (var nextItemToUse in nextItemsToUse)
            {
                if (nextItemToUse != null) {
                    Debug.Log($"[{nameof(RoachRaceItemBase)}] Triggering next item to use: {nextItemToUse.name}", gameObject);
                    nextItemToUse.UseStart();
                }
            }
        }

        protected virtual void OnSeedChanged(int seed) { }
    }
}