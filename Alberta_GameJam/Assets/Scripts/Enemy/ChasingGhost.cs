using UnityEngine;
using UnityEngine.AI;
 
public class ChasingGhost : MonoBehaviour
{
    [SerializeField] float moveRadius;
    [SerializeField] float idleDuration = 3f;
    [SerializeField] int maxHealth = 10;
    [SerializeField] float chaseRange = 8f;
    [SerializeField] float chaseSpeed = 4f;
 
    [Header("Visualization")]
    [SerializeField] bool showChaseRange = true;
    [SerializeField] Color chaseRangeColor = Color.red;
    [SerializeField] Transform player;
    [Header("Chase Cooldown")]
    [SerializeField] float chaseCooldown = 2f;
    float chaseCooldownTimer = 0f;
 
    void OnDrawGizmosSelected()
    {
        if (showChaseRange)
        {
            Gizmos.color = chaseRangeColor;
            Gizmos.DrawWireSphere(transform.position, chaseRange);
        }
    }
 
    void OnTriggerEnter2D(Collider2D other)
    {
        if (state == State.Chase && other.CompareTag("Player"))
        {
            // Invert player controls
            var playerController = other.GetComponent<Game.Player.TopDownPlayerController>();
            if (playerController != null)
            {
                playerController.InvertControlsForDuration(3f); // 3 seconds, adjust as needed
            }
            chaseCooldownTimer = chaseCooldown;
            EnterPatrol();
        }
    }
 
    public enum State
    {
        Idle,
        Patrol,
        Chase,
        Trapped,
        Dying
    }
 
    NavMeshAgent agent;
    float idleTimer;
    State state;
    SoundWord soundEffect;
    int health;
 
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        soundEffect = GetComponentInChildren<SoundWord>();
        health = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        EnterIdle();
    }
 
    void Update()
    {
        // Update cooldown timer
        if (chaseCooldownTimer > 0f)
        {
            chaseCooldownTimer -= Time.deltaTime;
        }
 
        if (player != null && state != State.Trapped && state != State.Dying)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            // Only allow chase if cooldown is finished
            if (distanceToPlayer <= chaseRange && chaseCooldownTimer <= 0f)
            {
                if (state != State.Chase)
                {
                    EnterChase();
                }
            }
            // If cooldown is active, force patrol
            else if (chaseCooldownTimer > 0f && state != State.Patrol)
            {
                EnterPatrol();
            }
            // If player leaves range, return to idle
            else if (state == State.Chase && distanceToPlayer > chaseRange)
            {
                EnterIdle();
            }
        }
        switch (state)
        {
            case State.Idle:
                HandleIdle();
                break;
            case State.Patrol:
                HandlePatrol();
                break;
            case State.Chase:
                HandleChase();
                break;
            case State.Trapped:
                HandleTrapped();
                break;
            case State.Dying:
                HandleDying();
                break;
        }
    }
    void EnterChase()
    {
        state = State.Chase;
        if (agent != null)
        {
            agent.speed = chaseSpeed;
        }
    }
 
    void HandleChase()
    {
        if (player == null || agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
            return;
        agent.SetDestination(player.position);
        soundEffect.Spawn(transform.position, Vector3.up, 1f);
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude <= 0.01f)
            {
                // Optionally do something when reaching the player
            }
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
 
    void EnterDying()
    {
        state = State.Dying;
        Destroy(gameObject);
    }
 
    void HandleDying()
    {
 
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
            var randomDirection = UnityEngine.Random.insideUnitSphere * moveRadius;
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
 
    public void TakeDamage(int amount)
    {
        health -= amount;
        health = Mathf.Max(0, health);
        if (health == 0 && state != State.Dying)
        {
            EnterDying();
        }
    }
}