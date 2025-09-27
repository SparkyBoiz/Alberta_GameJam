using UnityEngine;
using Game.Core;

namespace Game.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyAI : MonoBehaviour
    {
    public Transform target;
    public Transform ownerRoot;
    public float moveSpeed = 2.5f;
    public float rotationSpeed = 720f;
    public float stopRange = 0.15f;
    private Rigidbody2D _rb;

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

            Vector2 desiredVel = dist > stopRange ? dir * moveSpeed : Vector2.zero;
            Vector2 step = desiredVel * Time.fixedDeltaTime;
            _rb.MovePosition(pos + step);

            if (dir.sqrMagnitude > 0.0001f)
            {
                float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                float newAngle = Mathf.MoveTowardsAngle(_rb.rotation, targetAngle, rotationSpeed * Time.fixedDeltaTime);
                _rb.MoveRotation(newAngle);
            }

            // Removed projectile attack logic
        }


        private void OnDrawGizmosSelected()
        {
        }
    }
}