using UnityEngine;

namespace Game.Core
{
    [RequireComponent(typeof(Collider))]
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile Settings")] 
        public float speed = 20f; 
        public float lifetime = 5f; 
        public float damage = 10f; 
        [Tooltip("If true, this projectile will attempt to damage PlayerHealth components")] 
        public bool damagePlayer = false; 
        [Tooltip("If true, this projectile will attempt to damage Enemy targets (requires a component exposing TakeDamage(float) named 'EnemyHealth' or an IDamageable interface if added later)")] 
        public bool damageEnemy = false;

        [Header("Impact Settings")] 
        public bool destroyOnHit = true; 
        public GameObject hitVfx;

        private float _despawnAt; 
        private Collider _collider; 
        private Transform _ignoreRoot; 

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true; // Use trigger-based hits for simple projectiles

            // Ensure a Rigidbody exists for trigger events to fire; keep it kinematic
            var rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        private void OnEnable()
        {
            _despawnAt = Time.time + lifetime;
        }

        public void Initialize(float damageAmount, bool hitsPlayer, bool hitsEnemy)
        {
            damage = damageAmount;
            damagePlayer = hitsPlayer;
            damageEnemy = hitsEnemy;
        }

        public void IgnoreOwnerCollisions(Transform ownerRoot)
        {
            _ignoreRoot = ownerRoot;
            if (_collider == null) return;
            if (_ignoreRoot == null) return;

            var ownerColliders = _ignoreRoot.GetComponentsInChildren<Collider>(true);
            foreach (var oc in ownerColliders)
            {
                if (oc != null && oc != _collider)
                {
                    Physics.IgnoreCollision(_collider, oc, true);
                }
            }
        }

        private void Update()
        {
            // Move forward in local space
            transform.position += transform.forward * (speed * Time.deltaTime);

            // Lifetime expiry
            if (Time.time >= _despawnAt)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Ignore collisions with owner
            if (_ignoreRoot != null && other.transform.IsChildOf(_ignoreRoot))
                return;

            bool didDamage = false;

            if (damagePlayer)
            {
                var playerHealth = other.GetComponentInParent<Game.Player.PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                    didDamage = true;
                }
            }

            if (!didDamage && damageEnemy)
            {
                var enemyHealth = other.GetComponentInParent<Game.Enemy.EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);
                    didDamage = true;
                }
            }

            if (didDamage || destroyOnHit)
            {
                if (hitVfx != null)
                {
                    Instantiate(hitVfx, transform.position, Quaternion.identity);
                }
                Destroy(gameObject);
            }
        }

        private void OnDisable()
        {
            // Re-enable any ignored collisions (optional cleanup)
            if (_ignoreRoot != null && _collider != null)
            {
                var ownerColliders = _ignoreRoot.GetComponentsInChildren<Collider>(true);
                foreach (var oc in ownerColliders)
                {
                    if (oc != null && oc != _collider)
                    {
                        Physics.IgnoreCollision(_collider, oc, false);
                    }
                }
            }
        }
    }
}