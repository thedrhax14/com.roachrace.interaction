using UnityEngine;

namespace RoachRace.Interaction
{
    /// <summary>
    /// Optional interface for items which require aim data (camera ray origin/direction)
    /// provided by the owning client.
    /// 
    /// Notes:
    /// - Aim data must be treated as untrusted on the server; use it for convenience but still apply
    ///   server-side validation/rate limiting as needed.
    /// - Items that don't implement this interface can continue to use <see cref="RoachRaceItemComponent.UseSource"/>.
    /// </summary>
    public interface IRoachRaceAimItem
    {
        void SetAim(Vector3 origin, Vector3 direction);
    }
}
