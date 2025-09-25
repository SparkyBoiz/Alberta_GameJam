using UnityEngine;
using UnityEngine.InputSystem;
using Game.Core;

namespace Game.Player
{
    public class PlayerWeapon : MonoBehaviour
    {
        [Header("References")] 
        public Transform muzzle; 
        public Projectile projectilePrefab; 
        public Transform ownerRoot; // root transform used to ignore collisions

        [Header("Shooting")] 
        public float fireRate = 6f; // shots per second
        public float projectileDamage = 15f; 
        public bool automatic = true;

        [Header("Ammo")] 
        public int magazineSize = 12; 
        public int currentAmmo = 12; 
        public int reserveAmmo = 36; 
        public float reloadTime = 1.1f; 

        [Header("Input")] 
        public Game.Core.InputSystem_Actions inputActions;

        private float _nextShotTime = 0f; 
        private bool _isReloading = false; 
        private bool _isFiringHeld = false;

        public System.Action<int, int, int> onAmmoChanged; // mag, reserve, magSize

        private void Awake()
        {
            if (inputActions == null)
                inputActions = new Game.Core.InputSystem_Actions();
        }

        private void OnEnable()
        {
            inputActions.Enable();
            inputActions.Player.Fire.performed += OnFirePerformed;
            inputActions.Player.Fire.canceled += OnFireCanceled;
            inputActions.Player.Reload.performed += OnReloadPerformed;
        }

        private void OnDisable()
        {
            inputActions.Player.Fire.performed -= OnFirePerformed;
            inputActions.Player.Fire.canceled -= OnFireCanceled;
            inputActions.Player.Reload.performed -= OnReloadPerformed;
            inputActions.Disable();
        }

        private void OnFirePerformed(InputAction.CallbackContext ctx)
        {
            _isFiringHeld = true;
            TryShoot();
        }

        private void OnFireCanceled(InputAction.CallbackContext ctx)
        {
            _isFiringHeld = false;
        }

        private void OnReloadPerformed(InputAction.CallbackContext ctx)
        {
            StartCoroutine(StartReload());
        }

        private void Update()
        {
            if (automatic && _isFiringHeld)
            {
                TryShoot();
            }
        }

        private void TryShoot()
        {
            if (_isReloading) return;
            if (Time.time < _nextShotTime) return;

            if (currentAmmo <= 0)
            {
                StartCoroutine(StartReload());
                return;
            }

            if (projectilePrefab == null || muzzle == null) return;

            _nextShotTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);
            currentAmmo--;
            onAmmoChanged?.Invoke(currentAmmo, reserveAmmo, magazineSize);

            var proj = Instantiate(projectilePrefab, muzzle.position, muzzle.rotation);
            proj.Initialize(projectileDamage, hitsPlayer: false, hitsEnemy: true);
            proj.IgnoreOwnerCollisions(ownerRoot != null ? ownerRoot : transform.root);
        }

        private System.Collections.IEnumerator StartReload()
        {
            if (_isReloading) yield break;
            if (currentAmmo == magazineSize) yield break;
            if (reserveAmmo <= 0) yield break;

            _isReloading = true;

            float t = 0f;
            while (t < reloadTime)
            {
                t += Time.deltaTime;
                yield return null;
            }

            int needed = magazineSize - currentAmmo;
            int toLoad = Mathf.Min(needed, reserveAmmo);
            currentAmmo += toLoad;
            reserveAmmo -= toLoad;

            onAmmoChanged?.Invoke(currentAmmo, reserveAmmo, magazineSize);
            _isReloading = false;
        }

        public void AddAmmo(int amount)
        {
            reserveAmmo = Mathf.Max(0, reserveAmmo + amount);
            onAmmoChanged?.Invoke(currentAmmo, reserveAmmo, magazineSize);
        }
    }
}