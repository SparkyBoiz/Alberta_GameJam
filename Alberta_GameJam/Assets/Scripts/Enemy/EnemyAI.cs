using UnityEngine;
using Game.Core;

namespace Game.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyAI : MonoBehaviour
    {
        [Header("References")]
        public Transform target; // the player
        public Transform muzzle; // where bullets spawn (must point right, +X)
        public Projectile projectilePrefab;
        public Transform ownerRoot;

        [Header("Movement")]
        public float moveSpeed = 2.5f;
        public float rotationSpeed = 720f; // deg/sec towards player
        public float stopRange = 0.15f; // minimal distance to avoid jitter when very close

        [Header("Combat")]
        public float attackRange = 8f;
        public float fireRate = 1.5f; // shots per second
        public float projectileDamage = 10f;

        private Rigidbody2D _rb;
        private float _nextShotTime;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        private void FixedUpdate()
        {
            if (target == null)
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 pos = _rb.position;
            Vector2 toTarget = (Vector2)target.position - pos;
            float dist = toTarget.magnitude;
            Vector2 dir = dist > 0.0001f ? toTarget / dist : Vector2.zero;

            // Move toward the player (stop if extremely close to reduce jitter)
            Vector2 desiredVel = dist > stopRange ? dir * moveSpeed : Vector2.zero;
            Vector2 step = desiredVel * Time.fixedDeltaTime;
            _rb.MovePosition(pos + step);

            // Face the player (rotate around Z so +X points toward target)
            if (dir.sqrMagnitude > 0.0001f)
            {
                float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                float newAngle = Mathf.MoveTowardsAngle(_rb.rotation, targetAngle, rotationSpeed * Time.fixedDeltaTime);
                _rb.MoveRotation(newAngle);
            }

            // Shoot if within range
            if (dist <= attackRange && Time.time >= _nextShotTime)
            {
                ShootAt(target.position);
                _nextShotTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);
            }
        }

        private void ShootAt(Vector3 worldPos)
        {
            if (projectilePrefab == null || muzzle == null) return;

            Vector2 from = muzzle.position;
            Vector2 to = worldPos;
            Vector2 dir = (to - from);
            if (dir.sqrMagnitude < 0.0001f) return;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
            var proj = Instantiate(projectilePrefab, muzzle.position, rot);
            proj.Initialize(projectileDamage, hitsPlayer: true, hitsEnemy: false);
            proj.IgnoreOwnerCollisions(ownerRoot != null ? ownerRoot : transform);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}