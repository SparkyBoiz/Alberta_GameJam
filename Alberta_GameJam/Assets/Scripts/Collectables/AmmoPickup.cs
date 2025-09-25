using UnityEngine;

namespace Game.Collectables
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class AmmoPickup : MonoBehaviour
    {
        public int ammoAmount = 12;
        public float rotateSpeed = 90f;

        private void Awake()
        {
            // Ensure trigger + kinematic rigidbody for reliable trigger events with CharacterController
            var col = GetComponent<Collider>();
            col.isTrigger = true;
            var rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        private void Update()
        {
            transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.World);
        }

        private void OnTriggerEnter(Collider other)
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