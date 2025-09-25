using UnityEngine;

namespace Game.Collectables
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class AmmoPickup : MonoBehaviour
    {
        public int ammoAmount = 12;
        public float rotateSpeed = 90f;

        private void Awake()
        {
            // Ensure trigger + kinematic 2D rigidbody for reliable trigger events
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
            var rb = GetComponent<Rigidbody2D>();
            rb.isKinematic = true;
            rb.gravityScale = 0f;
        }

        private void Update()
        {
            transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime, Space.World);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var weapon = other.GetComponentInParent<Game.Player.PlayerWeapon>();
            if (weapon != null)
            {
                weapon.AddAmmo(ammoAmount);
                Destroy(gameObject);
            }
        }
    }
}