using UnityEngine;

namespace RoachRace.Interaction
{
    /// <summary>
    /// Convenience base class for most items.<br>
    /// <br>
    /// Provides:<br>
    /// - A configurable UseSource transform (ray origin / muzzle / hand).<br>
    /// - Stored use context (Seed, InstigatorId, IsServer, InstigatorObject) via InitializeUseContext.<br>
    /// - Optional per-instance use cooldown via CanUseNow / TryBeginUseCooldown / UseCooldownSeconds.<br>
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
        [Tooltip("Optional. Minimum seconds between use attempts for this item instance. Set to 0 for no cooldown.")]
        [SerializeField, Min(0f)] private float useCooldownSeconds;
        [Tooltip("Optional. Next item to use after using this item. Can be null.")]
        public RoachRaceItemBase[] nextItemsToUse;

        private float _nextUseTime;

        protected int InstigatorId { get; private set; } = -1;
        protected bool IsServer { get; private set; }
        protected int Seed { get; private set; }
        protected GameObject InstigatorObject { get; private set; }

        public override Transform UseSource => useSource != null ? useSource : transform;

        /// <summary>
        /// Returns whether this item instance can be used right now.<br>
        /// <br>
        /// Typical usage:<br>
        /// - Inventory and gameplay code can query this before calling <see cref="UseStart"/>.<br>
        /// - Returns <c>true</c> when cooldown is disabled or has elapsed.<br>
        /// </summary>
        public bool CanUseNow => useCooldownSeconds <= 0f || Time.time >= _nextUseTime;

        /// <summary>
        /// Attempts to start this item's cooldown window.<br>
        /// <br>
        /// Typical usage:<br>
        /// - Call immediately before <see cref="UseStart"/> to consume the cooldown on the item instance.<br>
        /// - Returns <c>false</c> when the item is still cooling down.<br>
        /// </summary>
        /// <returns><c>true</c> when the cooldown was started; otherwise <c>false</c>.</returns>
        protected bool TryBeginUseCooldown()
        {
            if (!CanUseNow)
                return false;

            _nextUseTime = Time.time + useCooldownSeconds;
            return true;
        }

        /// <summary>
        /// Starts item use while honoring the per-instance cooldown window.<br>
        /// <br>
        /// Typical usage:<br>
        /// - Called by inventory and observer playback code when the item is actually used.<br>
        /// - If the cooldown is still active, this call does nothing.<br>
        /// </summary>
        public override void UseStart()
        {
            if (!TryBeginUseCooldown())
                return;

            base.UseStart();
        }

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