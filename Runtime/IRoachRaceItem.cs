using UnityEngine;

namespace RoachRace.Interaction
{
    /// <summary>
    /// Command surface for an equippable/usable item.
    /// 
    /// Intended usage:
    /// - Items live as child GameObjects under a player.
    /// - Inventory/selection systems call SetVisibility / OnEquipped / OnUnEquipped.
    /// - Gameplay systems call InitializeUseContext before calling UseStart/UseStop so the item knows:
    ///   - who is using it (instigatorId / instigatorObject)
    ///   - whether this invocation is server-authoritative (isServer)
    ///   - a deterministic seed (seed) for synced randomness.
    /// 
    /// Networking note:
    /// - Typically, presentation (animations/VFX) can run on client and server,
    ///   but game-affecting logic should be gated behind isServer.
    /// </summary>
    public interface IRoachRaceItem
    {
        Transform UseSource { get; }

        void InitializeUseContext(int seed, int instigatorId, bool isServer, GameObject instigatorObject);

        void OnEquipped();
        float OnUnEquipped();

        void UseStart();
        void UseStop();

        void OnAim(bool isAiming);
        void SetVisibility(bool isVisible);
    }
}