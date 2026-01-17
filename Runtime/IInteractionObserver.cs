using RoachRace.Data;

namespace RoachRace.Interaction
{
    /// <summary>
    /// Interface for objects that observe interactions from an InteractiveItem.
    /// </summary>
    public interface IInteractionObserver
    {
        /// <summary>
        /// Called when a player successfully completes interaction.
        /// </summary>
        void OnInteract();

        /// <summary>
        /// Checks if a specific team can interact with this object.
        /// </summary>
        bool CanInteract(Team team);
    }
}
