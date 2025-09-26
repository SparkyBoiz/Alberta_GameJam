using UnityEngine;

namespace Game.Core
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile Settings")] 
        public float speed = 20f; 
        public float lifetime = 5f; 
        public float damage = 10f; 
        [Tooltip("If true, this projectile will attempt to damage PlayerHealth components")] 
        public bool damagePlayer = false; 
        [Tooltip("If true, this projectile will attempt to damage Enemy targets")] 
        public bool damageEnemy = false;

        [Header("Impact Settings")] 
        public bool destroyOnHit = true; 
        public GameObject hitVfx;

        private float _despawnAt; 
        private Collider2D _collider2D; 
        private Rigidbody2D _rb2D;
        private Transform _ignoreRoot; 

        private void Awake()
        {
            _collider2D = GetComponent<Collider2D>();
            _collider2D.isTrigger = true;

            _rb2D = GetComponent<Rigidbody2D>();
            _rb2D.isKinematic = false;
            _rb2D.gravityScale = 0f;
            _rb2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rb2D.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        private void OnEnable()
        {
            _despawnAt = Time.time + lifetime;
            // Ensure initial velocity aligns with facing. We use transform.right as the forward direction in 2D.
            _rb2D.linearVelocity = (Vector2)transform.right * speed;
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
            if (_collider2D == null || _ignoreRoot == null) return;

            var ownerColliders = _ignoreRoot.GetComponentsInChildren<Collider2D>(true);
            foreach (var oc in ownerColliders)
            {
                if (oc != null && oc != _collider2D)
                {
                    Physics2D.IgnoreCollision(_collider2D, oc, true);
                }
            }
        }

        private void Update()
        {
            // Keep velocity in sync if someone rotates the projectile after launch
            _rb2D.linearVelocity = (Vector2)transform.right * speed;

            // Lifetime expiry
            if (Time.time >= _despawnAt)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
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
            if (_ignoreRoot != null && _collider2D != null)
            {
                var ownerColliders = _ignoreRoot.GetComponentsInChildren<Collider2D>(true);
                foreach (var oc in ownerColliders)
                {
                    if (oc != null && oc != _collider2D)
                    {
                        Physics2D.IgnoreCollision(_collider2D, oc, false);
                    }
                }
            }
        }
    }
}