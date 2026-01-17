using System.Collections.Generic;
using RoachRace.Data;
using UnityEngine;

namespace RoachRace.Interaction
{
    /// <summary>
    /// World interactable entrypoint.
    /// 
    /// Scene/prefab setup:
    /// - Put this on a root GameObject for an interactable (door panel, keycard reader, terminal, etc).
    /// - Ensure there's a Collider somewhere under it so items (e.g., KeycardItem) can raycast and find it via GetComponentInParent.
    /// - Attach one or more IInteractionObserver components (usually on the same GameObject) and register them at runtime.
    /// 
    /// Notes:
    /// - CanInteract(team) returns true if ANY registered observer allows interaction.
    /// - OnInteract() calls OnInteract on ALL registered observers.
    /// </summary>
    public class InteractiveItem : MonoBehaviour
    {
        [Tooltip("Optional time-to-interact value (seconds) for UI/progress bars.")]
        [SerializeField] private float interactionDuration = 2.0f;
        public float InteractionDuration => interactionDuration;

        private readonly List<IInteractionObserver> _observers = new();

        public void Register(IInteractionObserver observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }

        public void Unregister(IInteractionObserver observer)
        {
            if (_observers.Contains(observer))
            {
                _observers.Remove(observer);
            }
        }

        public bool CanInteract(Team team)
        {
            // If no observers, cannot interact
            if (_observers.Count == 0) return false;

            // All observers must agree? Or just one?
            // For now, let's say if ANY observer says yes, it's valid.
            // Or usually, the observer IS the logic.
            foreach (var observer in _observers)
            {
                if (observer.CanInteract(team)) return true;
            }
            return false;
        }

        public void OnInteract()
        {
            Debug.Log($"[{nameof(InteractiveItem)}] OnInteract called on {gameObject.name}");
            foreach (var observer in _observers)
            {
                observer.OnInteract();
            }
        }
    }
}
