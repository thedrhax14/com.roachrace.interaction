using UnityEngine;

namespace RoachRace.Interaction
{
    /// <summary>
    /// World/pickup-related behavior for an <see cref="ItemDefinition"/>.<br/>
    ///<br/>
    /// Typical usage:<br/>
    /// - Assign on an <see cref="ItemDefinition"/> to control what prefab is spawned when the item is dropped into the world.<br/>
    /// - Keep world behavior out of the core item identity so the same item can exist without any world representation (eg UI-only / hidden meter assets).<br/>
    ///<br/>
    /// Notes:<br/>
    /// - If <see cref="worldPickupPrefab"/> is null, systems should fall back to a configured default pickup prefab (if any).
    /// </summary>
    [CreateAssetMenu(menuName = "RoachRace/Items/Rules/World Pickup Rules")]
    public sealed class WorldPickupRules : ScriptableObject
    {
        [Tooltip("Optional. Prefab to spawn in the world when this item is dropped. Must contain a FishNet NetworkObject + NetworkItemPickup.")]
        public GameObject worldPickupPrefab;
    }
}
