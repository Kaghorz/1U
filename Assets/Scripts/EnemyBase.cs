using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBase : MonoBehaviour
{
    public EnemyData enemyData;

    protected NavMeshAgent agent;
    protected Animator anim;
    protected Transform playerTransform;

    protected float currentHealth;
    protected float lastAttackTime;
    protected EnemyState currentState = EnemyState.Patrol;
    protected Vector3 lastKnownNoisePosition;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        // Setup initial stats
        currentHealth = enemyData.maxHealth;
        agent.speed = enemyData.moveSpeed;
        agent.stoppingDistance = enemyData.attackRange;

        // In a real scenario, you might find the player by Tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    protected virtual void Start()
    {
        // Register with the manager
        if (MissionManager.Instance != null)
            MissionManager.Instance.RegisterEnemy(this);
    }

    protected virtual void Update()
    {
        if (currentState == EnemyState.Death) return;

        switch (currentState)
        {
            case EnemyState.Patrol:
                HandlePatrol();
                CheckForPlayer();
                break;
            case EnemyState.Investigate:
                HandleInvestigate();
                CheckForPlayer();
                break;
            case EnemyState.Chase:
                HandleChase();
                break;
            case EnemyState.Attack:
                HandleAttack();
                break;
        }

        UpdateAnimations();
    }

    // Existing Raycast Wander Logic
    protected void HandlePatrol()
    {
        transform.position += transform.forward * enemyData.moveSpeed * Time.deltaTime;

        float rayLength = enemyData.raycastDistance;
        Vector3 rayOrigin = transform.position + Vector3.up * 1f;

        if (Physics.Raycast(rayOrigin, transform.forward, rayLength))
        {
            float turnAngle = Random.Range(90f, 180f);
            transform.Rotate(Vector3.up, turnAngle);
        }
    }

    protected void HandleInvestigate()
    {
        agent.SetDestination(lastKnownNoisePosition);

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentState = EnemyState.Patrol;
        }
    }

    protected void HandleChase()
    {
        if (playerTransform == null) return;

        agent.speed = enemyData.runSpeed; // Increase to run speed
        agent.SetDestination(playerTransform.position);

        // Transition to attack if close enough
        if (Vector3.Distance(transform.position, playerTransform.position) <= enemyData.attackRange)
        {
            currentState = EnemyState.Attack;
        }
    }

    protected void HandleAttack()
    {
        if (playerTransform == null) return;

        // Keep facing the player
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        // Check distance to see if we should chase again
        if (Vector3.Distance(transform.position, playerTransform.position) > enemyData.attackRange)
        {
            currentState = EnemyState.Chase;
            return;
        }

        // Trigger the "Attack" parameter from your animator
        if (Time.time >= lastAttackTime + enemyData.attackCooldown)
        {
            anim.SetTrigger("Attack");
            lastAttackTime = Time.time;
        }
    }

    public void TakeDamage(float amount)
    {
        if (currentState == EnemyState.Death) return;

        currentHealth -= amount;

        // If attacked, instantly chase the player
        if (currentState != EnemyState.Chase && currentState != EnemyState.Attack)
        {
            currentState = EnemyState.Chase;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected void Die()
    {
        currentState = EnemyState.Death;
        agent.isStopped = true;

        // Set your animator bool for the Death transition
        anim.SetBool("hasDied", true);

        // If this specific enemy is a Boss, notify the manager
        if (tag == "Boss" && MissionManager.Instance != null)
        {
            MissionManager.Instance.OnBossDefeated();
        }

        // Unregister from the mission manager
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.UnregisterEnemy(this);
        }

        // Optional: Destroy after animation completes or let it stay as a corpse
        Destroy(gameObject, 2f);
    }

    protected void CheckForPlayer()
    {
        // Simple visual aggro check (can be expanded later)
        if (playerTransform != null && Vector3.Distance(transform.position, playerTransform.position) < 10f)
        {
            currentState = EnemyState.Chase;
        }
    }

    public void OnHearNoise(Vector3 sourcePosition)
    {
        if (currentState != EnemyState.Death && currentState != EnemyState.Chase && currentState != EnemyState.Attack)
        {
            lastKnownNoisePosition = sourcePosition;
            currentState = EnemyState.Investigate;
        }
    }

    protected void UpdateAnimations()
    {
        if (anim != null)
        {
            float targetSpeed = 0;

            if (currentState == EnemyState.Patrol)
            {
                targetSpeed = enemyData.moveSpeed;
            }
            else if (currentState == EnemyState.Investigate || currentState == EnemyState.Chase)
            {
                targetSpeed = agent.velocity.magnitude;
            }

            // Smoothly update the "Speed" float parameter
            anim.SetFloat("Speed", targetSpeed);
        }
    }

    // Called from the animation event at the moment of impact in the attack animation
    public void PerformAttackDamage()
    {
        if (playerTransform != null)
        {
            // Calculate the current distance to ensure the player didn't dodge away
            float distance = Vector3.Distance(transform.position, playerTransform.position);

            // A small buffer to the range to make it feel fair for the player
            if (distance <= enemyData.attackRange + 0.5f)
            {
                PlayerStats playerStats = playerTransform.GetComponent<PlayerStats>();
                if (playerStats != null)
                {
                    // Call the TakeDamage method on the player
                    playerStats.TakeDamage(enemyData.attackDamage);

                    Debug.Log($"Enemy {name} dealt {enemyData.attackDamage} damage to the player.");
                }
            }
        }
    }
}