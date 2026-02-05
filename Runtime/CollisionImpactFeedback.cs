using FMODUnity;
using UnityEngine;

namespace RoachRace.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    /// <summary>
    /// Plays impact feedback (particles + FMOD sound) when this Rigidbody collides with something.
    /// 
    /// Intended setup (for designers):
    /// - Add this component to the MOVING object (the same GameObject that has the Rigidbody).
    /// - Make sure this object also has a Collider (or a Collider on a child) so Unity can generate collision events.
    /// - Do NOT add this to environment/static colliders; it is meant to live on the moving body.
    /// 
    /// How it decides to play:
    /// - Uses <see cref="Collision.relativeVelocity"/> magnitude as the "impact speed".
    /// - If impact speed is below <see cref="minimumImpactSpeed"/>, nothing plays.
    /// - <see cref="cooldownSeconds"/> prevents rapid re-triggers from multi-contact bounces.
    /// 
    /// Particle orientation:
    /// - Particles are spawned at the first contact point.
    /// - Spawn rotation is built so that:
    ///   - Up direction = collision normal
    ///   - Forward direction = this Rigidbody's velocity direction at the time of impact
    /// - If velocity is nearly zero (or parallel to the normal), the script falls back to a stable forward direction.
    /// 
    /// FMOD:
    /// - Assign <see cref="impactEvent"/> to play a one-shot sound.
    /// - By default the sound plays at the contact point; disable <see cref="playSoundAtContactPoint"/> to play at this object's position.
    /// 
    /// Notes:
    /// - This reacts to collisions (not triggers). For trigger volumes you'd need a separate OnTrigger version.
    /// - If the particle prefab loops, it will not be auto-destroyed (by design). Use a non-looping prefab for one-shot impacts.
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

        [Header("Particles")]
        [Tooltip("Optional particle prefab to spawn and play on impact.")]
        [SerializeField] private ParticleSystem impactParticlesPrefab;

        [Header("FMOD")]
        [Tooltip("Optional FMOD event to play on impact.")]
        [SerializeField] private EventReference impactEvent;

        [Tooltip("If true, plays sound at the contact point; otherwise at this rigidbody position.")]
        [SerializeField] private bool playSoundAtContactPoint = true;

        private Rigidbody _rigidbody;
        private float _lastPlayTime = -999f;

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
            var velocity = _rigidbody.linearVelocity;
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

            // Particles
            if (impactParticlesPrefab != null)
            {
                var instance = Instantiate(impactParticlesPrefab, contactPoint, rotation);
                instance.Play(true);

                if (!instance.main.loop)
                {
                    Destroy(instance.gameObject, GetAutoDestroySeconds(instance));
                }
            }

            // FMOD
            if (!impactEvent.IsNull)
            {
                var soundPos = playSoundAtContactPoint ? contactPoint : transform.position;
                RuntimeManager.PlayOneShot(impactEvent, soundPos);
            }

            _lastPlayTime = Time.time;
        }

        private static float GetAutoDestroySeconds(ParticleSystem system)
        {
            // Conservative lifetime estimation (duration + max startLifetime), with a small buffer.
            var main = system.main;
            var seconds = main.duration;

            seconds += main.startLifetime.mode switch
            {
                ParticleSystemCurveMode.Constant => main.startLifetime.constant,
                ParticleSystemCurveMode.TwoConstants => main.startLifetime.constantMax,
                ParticleSystemCurveMode.Curve => main.startLifetime.constant,
                ParticleSystemCurveMode.TwoCurves => main.startLifetime.constantMax,
                _ => 0f,
            };

            if (seconds < 0.1f) seconds = 0.1f;
            return seconds + 0.5f;
        }
    }
}
