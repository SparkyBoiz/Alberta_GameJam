using UnityEngine;
using UnityEngine.AI;

public class Ghost : MonoBehaviour
{
    [SerializeField] float moveRadius;
    [SerializeField] float idleDuration = 3f;
    public enum State
    {
        Idle,
        Patrol,
        Trapped
    }

    NavMeshAgent agent;
    float idleTimer;
    State state;
    SoundWord soundEffect;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        soundEffect = GetComponentInChildren<SoundWord>();
        EnterIdle();
    }

    void Update()
    {
        switch (state)
        {
            case State.Idle:
                HandleIdle();
                break;
            case State.Patrol:
                HandlePatrol();
                break;
            case State.Trapped:
                HandleTrapped();
                break;
        }
    }

    public void EnterTrapped(float duration)
    {
        if (agent != null)
        {
            agent.isStopped = true;
        }
        state = State.Trapped;
        StartCoroutine(TrappedTimer(duration));
    }

    private System.Collections.IEnumerator TrappedTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (agent != null)
        {
            agent.isStopped = false;
        }
        EnterIdle();
    }

    void HandleTrapped()
    {
        // Do nothing while trapped
    }

    void EnterIdle()
    {
        state = State.Idle;
        idleTimer = idleDuration;
        if (agent != null)
        {
            agent.ResetPath();
        }
    }

    void HandleIdle()
    {
        if (idleTimer > 0f)
        {
            idleTimer -= Time.deltaTime;
        }

        if (idleTimer <= 0f)
        {
            EnterPatrol();
        }
    }

    void EnterPatrol()
    {
        state = State.Patrol;
        FindNextWaypoint();
    }

    void HandlePatrol()
    {
        soundEffect.Spawn(transform.position, Vector3.up, 1f);
        if (agent == null || agent.pathPending)
        {
            return;
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude <= 0.01f)
            {
                EnterIdle();
            }
        }
    }

    void FindNextWaypoint()
    {
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            return;
        }

        var origin = transform.position;
        const int maxAttempts = 10;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var randomDirection = Random.insideUnitSphere * moveRadius;
            var target = origin + randomDirection;
            if (NavMesh.SamplePosition(target, out var navHit, moveRadius, NavMesh.AllAreas))
            {
                var path = new NavMeshPath();
                if (agent.CalculatePath(navHit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    agent.SetDestination(navHit.position);
                    return;
                }
            }
        }
    }
}










