using System.Reflection;
using UnityEngine;

namespace RoachRace.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    /// <summary>
    /// Spawns an impact VFX/SFX prefab when this Rigidbody collides with something.
    /// 
    /// Intended setup (for designers):
    /// - Add this component to the MOVING object (the same GameObject that has the Rigidbody).
    /// - Make sure this object also has a Collider (or a Collider on a child) so Unity can generate collision events.
    /// - Do NOT add this to environment/static colliders; it is meant to live on the moving body.
    /// - Create an "Impact Prefab" that contains your ParticleSystems and your sound.
    ///   - For FMOD, the prefab can contain a Studio Event Emitter set to play on Awake/Enable.
    /// 
    /// How it decides to play:
    /// - Uses <see cref="Collision.relativeVelocity"/> magnitude as the "impact speed".
    /// - If impact speed is below <see cref="minimumImpactSpeed"/>, nothing plays.
    /// - <see cref="cooldownSeconds"/> prevents rapid re-triggers from multi-contact bounces.
    /// 
    /// Spawn transform:
    /// - The prefab is spawned at the first contact point.
    /// - Spawn rotation is built so that:
    ///   - Up direction = collision normal
    ///   - Forward direction = this Rigidbody's velocity direction at the time of impact
    /// - If velocity is nearly zero (or parallel to the normal), the script falls back to a stable forward direction.
    /// 
    /// Notes:
    /// - This reacts to collisions (not triggers). For trigger volumes you'd need a separate OnTrigger version.
    /// - Use <see cref="destroySpawnedAfterSeconds"/> to auto-destroy the spawned prefab after a fixed time.
    /// </summary>
    public class CollisionImpactFeedback : MonoBehaviour
    {
        [Header("Threshold")]
        [Tooltip("Minimum impact speed required to play effects. Uses Collision.relativeVelocity magnitude.")]
        [Min(0f)]
        [SerializeField] private float minimumImpactSpeed = 2.0f;

        [Tooltip("Prevents spamming effects when multiple contacts happen rapidly.")]
        [Min(0f)]
        [SerializeField] private float cooldownSeconds = 0.05f;

        [Header("Spawn")]
        [Tooltip("Prefab to spawn on impact (put your particles + sound on this prefab).")]
        [SerializeField] private GameObject impactPrefab;

        [Tooltip("If > 0, destroys the spawned prefab instance after this many seconds.")]
        [Min(0f)]
        [SerializeField] private float destroySpawnedAfterSeconds = 3.0f;

        private Rigidbody _rigidbody;
        private float _lastPlayTime = -999f;

        private static readonly PropertyInfo RigidbodyLinearVelocityProperty =
            typeof(Rigidbody).GetProperty("linearVelocity", BindingFlags.Instance | BindingFlags.Public);

        private static readonly PropertyInfo RigidbodyVelocityProperty =
            typeof(Rigidbody).GetProperty("velocity", BindingFlags.Instance | BindingFlags.Public);

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                Debug.LogError($"[{nameof(CollisionImpactFeedback)}] Rigidbody is missing on '{gameObject.name}'.", gameObject);
                throw new System.NullReferenceException(
                    $"[{nameof(CollisionImpactFeedback)}] Rigidbody is null on GameObject '{gameObject.name}'. This component requires a Rigidbody.");
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!enabled) return;

            if (cooldownSeconds > 0f && Time.time < _lastPlayTime + cooldownSeconds)
            {
                return;
            }

            // Gate by impact speed.
            var impactSpeed = collision.relativeVelocity.magnitude;
            if (impactSpeed < minimumImpactSpeed)
            {
                return;
            }

            // Resolve a contact (or fallback).
            var hasContact = collision.contactCount > 0;
            var contactPoint = hasContact ? collision.GetContact(0).point : transform.position;
            var contactNormal = hasContact ? collision.GetContact(0).normal : Vector3.up;

            var up = contactNormal.sqrMagnitude > 0.0001f ? contactNormal.normalized : Vector3.up;

            // Forward should follow rigidbody velocity direction.
            var velocity = GetRigidbodyVelocity(_rigidbody);
            var forward = velocity.sqrMagnitude > 0.0001f ? velocity.normalized : transform.forward;

            // If forward is nearly parallel to up, choose a stable tangent.
            if (Vector3.Cross(up, forward).sqrMagnitude < 0.0001f)
            {
                forward = Vector3.ProjectOnPlane(transform.forward, up);
                if (forward.sqrMagnitude < 0.0001f)
                {
                    forward = Vector3.Cross(up, Vector3.right);
                    if (forward.sqrMagnitude < 0.0001f)
                    {
                        forward = Vector3.Cross(up, Vector3.forward);
                    }
                }
                forward.Normalize();
            }

            var rotation = Quaternion.LookRotation(forward, up);

            if (impactPrefab != null)
            {
                var instance = Instantiate(impactPrefab, contactPoint, rotation);

                if (destroySpawnedAfterSeconds > 0f)
                {
                    Destroy(instance, destroySpawnedAfterSeconds);
                }
            }

            _lastPlayTime = Time.time;
        }

        private static Vector3 GetRigidbodyVelocity(Rigidbody rb)
        {
            if (rb == null) return Vector3.zero;

            // Newer Unity versions: Rigidbody.linearVelocity
            if (RigidbodyLinearVelocityProperty != null)
            {
                return (Vector3)RigidbodyLinearVelocityProperty.GetValue(rb);
            }

            // Older Unity versions: Rigidbody.velocity
            if (RigidbodyVelocityProperty != null)
            {
                return (Vector3)RigidbodyVelocityProperty.GetValue(rb);
            }

            return Vector3.zero;
        }
    }
}
