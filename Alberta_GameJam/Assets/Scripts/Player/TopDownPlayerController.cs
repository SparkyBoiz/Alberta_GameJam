using UnityEngine;
using UnityEngine.InputSystem;
using Game.Enemy;

namespace Game.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerWeapon))]
    public class TopDownPlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 6f;
        public float rotationSpeed = 720f; // deg/sec towards target direction

        [Header("Auto Attack")]
        public float autoAttackRange = 8f;
        public float autoAttackCooldown = 0.15f; // seconds between shots

        [Header("Input")]
        public Game.Core.InputSystem_Actions inputActions;

        private Rigidbody2D _rb;
        private Vector2 _moveInput;
        private PlayerWeapon _weapon;

        private float _nextAutoAttackTime = 0f;
        private float _reloadUntilTime = 0f; // > Time.time while reloading

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            _weapon = GetComponent<PlayerWeapon>();

            if (inputActions == null)
            {
                inputActions = new Game.Core.InputSystem_Actions();
            }
        }

        private void OnEnable()
        {
            inputActions.Enable();
            inputActions.Player.Move.performed += OnMove;
            inputActions.Player.Move.canceled += OnMove;
        }

        private void OnDisable()
        {
            inputActions.Player.Move.performed -= OnMove;
            inputActions.Player.Move.canceled -= OnMove;
            inputActions.Disable();
        }

        private void OnMove(InputAction.CallbackContext ctx)
        {
            _moveInput = ctx.ReadValue<Vector2>();
        }

        private void FixedUpdate()
        {
            // 1) Movement on XY plane (2D)
            _rb.linearVelocity = _moveInput * moveSpeed;

            // 2) Acquire target within auto-attack range
            Transform target = FindNearestEnemyInRange();

            // 3) Rotate to face target if any, else face movement direction
            Vector2 faceDir = Vector2.zero;
            if (target != null)
            {
                faceDir = (Vector2)target.position - _rb.position;
            }
            else if (_moveInput.sqrMagnitude > 0.0001f)
            {
                faceDir = _moveInput;
            }

            if (faceDir.sqrMagnitude > 0.0001f)
            {
                float targetAngle = Mathf.Atan2(faceDir.y, faceDir.x) * Mathf.Rad2Deg;
                float newAngle = Mathf.MoveTowardsAngle(_rb.rotation, targetAngle, rotationSpeed * Time.fixedDeltaTime);
                _rb.MoveRotation(newAngle);
            }

            // 4) Handle reload completion
            if (_reloadUntilTime > 0f && Time.time >= _reloadUntilTime)
            {
                CompleteReload();
            }

            // 5) Auto-attack if target in range and not reloading and cooldown ready
            if (target != null && Time.time >= _nextAutoAttackTime && !IsReloading())
            {
                TryAutoShootAt(target.position);
            }
        }

        private Transform FindNearestEnemyInRange()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, autoAttackRange);
            Transform closest = null;
            float closestDist = float.PositiveInfinity;

            foreach (var h in hits)
            {
                if (h == null) continue;
                var eh = h.GetComponentInParent<EnemyHealth>();
                if (eh == null) continue;
                float d = ((Vector2)eh.transform.position - (Vector2)transform.position).sqrMagnitude;
                if (d < closestDist)
                {
                    closestDist = d;
                    closest = eh.transform;
                }
            }
            return closest;
        }

        private void TryAutoShootAt(Vector3 worldPos)
        {
            if (_weapon == null) return;

            // Handle ammo
            if (_weapon.currentAmmo <= 0)
            {
                StartReloadIfPossible();
                return;
            }

            // Ensure we have a projectile and muzzle
            if (_weapon.projectilePrefab == null || _weapon.muzzle == null)
            {
                _nextAutoAttackTime = Time.time + autoAttackCooldown;
                return;
            }

            // Aim muzzle at target
            Vector2 from = _weapon.muzzle.position;
            Vector2 to = worldPos;
            Vector2 dir = (to - from);
            if (dir.sqrMagnitude < 0.0001f)
            {
                _nextAutoAttackTime = Time.time + autoAttackCooldown;
                return;
            }

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            _weapon.muzzle.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // Fire
            var proj = Instantiate(_weapon.projectilePrefab, _weapon.muzzle.position, _weapon.muzzle.rotation);
            proj.Initialize(_weapon.projectileDamage, hitsPlayer: false, hitsEnemy: true);
            proj.IgnoreOwnerCollisions(_weapon.ownerRoot != null ? _weapon.ownerRoot : transform);

            // Ammo bookkeeping
            _weapon.currentAmmo = Mathf.Max(0, _weapon.currentAmmo - 1);
            _weapon.onAmmoChanged?.Invoke(_weapon.currentAmmo, _weapon.reserveAmmo, _weapon.magazineSize);

            // Cooldown (respect weapon fireRate as well)
            float fireInterval = 1f / Mathf.Max(0.01f, _weapon.fireRate);
            _nextAutoAttackTime = Time.time + Mathf.Max(autoAttackCooldown, fireInterval);
        }

        private bool IsReloading()
        {
            return _reloadUntilTime > Time.time;
        }

        private void StartReloadIfPossible()
        {
            if (_weapon == null) return;
            if (_weapon.currentAmmo >= _weapon.magazineSize) return;
            if (_weapon.reserveAmmo <= 0) return;
            if (IsReloading()) return;

            _reloadUntilTime = Time.time + _weapon.reloadTime;
        }

        private void CompleteReload()
        {
            if (_weapon == null) { _reloadUntilTime = 0f; return; }
            int needed = _weapon.magazineSize - _weapon.currentAmmo;
            if (needed <= 0 || _weapon.reserveAmmo <= 0) { _reloadUntilTime = 0f; return; }

            int toLoad = Mathf.Min(needed, _weapon.reserveAmmo);
            _weapon.currentAmmo += toLoad;
            _weapon.reserveAmmo -= toLoad;
            _weapon.onAmmoChanged?.Invoke(_weapon.currentAmmo, _weapon.reserveAmmo, _weapon.magazineSize);
            _reloadUntilTime = 0f;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, autoAttackRange);
        }
    }
}