using UnityEngine;
using UnityEngine.AI;
using Game.Core;

namespace Game.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAI : MonoBehaviour
    {
        public enum State
        {
            Patrol,
            Chase,
            Attack
        }

        [Header("References")]
        public Transform[] patrolPoints;
        public Transform target; // typically the player
        public Transform muzzle;
        public Projectile projectilePrefab;
        public Transform ownerRoot;

        [Header("AI Settings")]
        public float sightRange = 12f;
        public float attackRange = 8f;
        public float fireRate = 1.5f;
        public float projectileDamage = 10f;
        public float repathInterval = 0.2f;

        private NavMeshAgent _agent;
        private int _currentPatrolIndex = 0;
        private float _nextRepathTime = 0f;
        private float _nextShotTime = 0f;
        private State _state = State.Patrol;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.updateRotation = false; // we will rotate manually on XZ plane
            _agent.updateUpAxis = false;
        }

        private void Start()
        {
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                _currentPatrolIndex = 0;
                _agent.SetDestination(patrolPoints[_currentPatrolIndex].position);
            }
        }

        private void Update()
        {
            UpdateState();
            TickState();
        }

        private void UpdateState()
        {
            float distToTarget = Mathf.Infinity;
            bool canSeeTarget = false;

            if (target != null)
            {
                distToTarget = Vector3.Distance(transform.position, target.position);
                canSeeTarget = distToTarget <= sightRange && HasLineOfSight(target);
            }

            switch (_state)
            {
                case State.Patrol:
                    if (canSeeTarget)
                    {
                        _state = distToTarget <= attackRange ? State.Attack : State.Chase;
                    }
                    break;
                case State.Chase:
                    if (!canSeeTarget)
                    {
                        _state = State.Patrol;
                    }
                    else if (distToTarget <= attackRange)
                    {
                        _state = State.Attack;
                    }
                    break;
                case State.Attack:
                    if (!canSeeTarget)
                    {
                        _state = State.Patrol;
                    }
                    else if (distToTarget > attackRange * 1.15f)
                    {
                        _state = State.Chase;
                    }
                    break;
            }
        }

        private void TickState()
        {
            switch (_state)
            {
                case State.Patrol:
                    DoPatrol();
                    break;
                case State.Chase:
                    DoChase();
                    break;
                case State.Attack:
                    DoAttack();
                    break;
            }
        }

        private void DoPatrol()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                _agent.ResetPath();
                return;
            }

            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
            {
                _currentPatrolIndex = (_currentPatrolIndex + 1) % patrolPoints.Length;
                _agent.SetDestination(patrolPoints[_currentPatrolIndex].position);
            }
        }

        private void DoChase()
        {
            if (target == null) { _state = State.Patrol; return; }
            if (Time.time >= _nextRepathTime)
            {
                _agent.SetDestination(target.position);
                _nextRepathTime = Time.time + repathInterval;
            }
            RotateTowards(target.position);
        }

        private void DoAttack()
        {
            if (target == null) { _state = State.Patrol; return; }
            // Maintain some chasing behaviour while attacking (keep within attack range)
            if (Time.time >= _nextRepathTime)
            {
                _agent.SetDestination(target.position);
                _nextRepathTime = Time.time + repathInterval;
            }

            RotateTowards(target.position);

            if (Time.time >= _nextShotTime)
            {
                ShootAt(target.position);
                _nextShotTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);
            }
        }

        private void ShootAt(Vector3 worldPos)
        {
            if (projectilePrefab == null || muzzle == null) return;

            Vector3 dir = (worldPos - muzzle.position);
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) return;

            Quaternion rot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            var proj = Instantiate(projectilePrefab, muzzle.position, rot);
            proj.Initialize(projectileDamage, hitsPlayer: true, hitsEnemy: false);
            proj.IgnoreOwnerCollisions(ownerRoot != null ? ownerRoot : transform);
        }

        private bool HasLineOfSight(Transform tgt)
        {
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            Vector3 dest = tgt.position + Vector3.up * 0.5f;
            Vector3 dir = (dest - origin);
            float dist = dir.magnitude;
            dir /= dist;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, ~0, QueryTriggerInteraction.Ignore))
            {
                return hit.transform == tgt || hit.transform.IsChildOf(tgt);
            }
            return true;
        }

        private void RotateTowards(Vector3 worldPos)
        {
            Vector3 dir = worldPos - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude <= 0.0001f) return;
            Quaternion rot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 0.2f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, sightRange);
            Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}