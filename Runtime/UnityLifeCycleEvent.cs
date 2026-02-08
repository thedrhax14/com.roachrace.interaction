using UnityEngine;
using UnityEngine.Events;

namespace RoachRace.Interaction
{
    public class UnityLifeCycleEvent : MonoBehaviour
    {
        public enum LifeCycleHook
        {
            Awake,
            Start,
            OnEnable,
            OnDisable,
            Update,
            FixedUpdate,
            LateUpdate,
            OnDestroy,
            OnApplicationQuit,
        }

        [Header("Trigger")]
        [SerializeField] private LifeCycleHook hook = LifeCycleHook.Start;

        [Header("Event")]
        [SerializeField] private UnityEvent onHook;

        private void Awake() => TryInvoke(LifeCycleHook.Awake);

        private void Start() => TryInvoke(LifeCycleHook.Start);

        private void OnEnable() => TryInvoke(LifeCycleHook.OnEnable);

        private void OnDisable() => TryInvoke(LifeCycleHook.OnDisable);

        private void Update() => TryInvoke(LifeCycleHook.Update);

        private void FixedUpdate() => TryInvoke(LifeCycleHook.FixedUpdate);

        private void LateUpdate() => TryInvoke(LifeCycleHook.LateUpdate);

        private void OnDestroy() => TryInvoke(LifeCycleHook.OnDestroy);

        private void OnApplicationQuit() => TryInvoke(LifeCycleHook.OnApplicationQuit);

        private void TryInvoke(LifeCycleHook invokedHook)
        {
            if (hook != invokedHook)
                return;

            onHook?.Invoke();
        }
    }
}
