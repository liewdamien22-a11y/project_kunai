using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
//[RequireComponent(typeof(CapsuleCollider))]
public class enemyBase : MonoBehaviour//This is the parent enemy script and also functions as the script for the base melee enemy
{
    public enum EnemyState
    {
        Idle,
        Chasing,
        Attacking
    }

    [Header("State")]
    public EnemyState currentState = EnemyState.Idle;

    [Header("Health")]
    [SerializeField] private float maxHealth = 1f;
    [SerializeField] protected float currentHealth;

    [Header("Movement & Combat")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] protected float detectionRange = 15f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackCooldown = 1.5f;

    protected Transform playerTransform;
    protected NavMeshAgent navAgent;
    protected Animator enemyAnimator;

    private float lastAttackTime = 0f;

    [SerializeField]private GameObject playerObj;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        navAgent = GetComponent<NavMeshAgent>();
        enemyAnimator = GetComponent<Animator>();

        navAgent.speed = moveSpeed;

        // Find the player by tag
        playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.Log($"{gameObject.name}: No GameObject with tag 'Player' found in scene!");
        }
    }

    protected virtual void Update()
    {
        if (playerTransform == null) return;

        RunStateMachine();
    }

    protected void RunStateMachine()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState(distanceToPlayer);
                break;

            case EnemyState.Chasing:
                HandleChasingState(distanceToPlayer);
                break;

            case EnemyState.Attacking:
                HandleAttackingState(distanceToPlayer);
                break;
        }
    }

    protected virtual void HandleIdleState(float distanceToPlayer)
    {
        navAgent.isStopped = true;

        if (distanceToPlayer <= detectionRange)
        {
            currentState = EnemyState.Chasing;
        }
    }

    protected virtual void HandleChasingState(float distanceToPlayer)
    {
        navAgent.isStopped = false;
        navAgent.SetDestination(playerTransform.position);
       

        if (distanceToPlayer <= attackRange)
        {
            currentState = EnemyState.Attacking;

        }
        else if (distanceToPlayer > detectionRange * 1.5f)
        {
            //lose track of player if they get too far
            currentState = EnemyState.Idle;
        }
    }

    protected virtual void HandleAttackingState(float distanceToPlayer)
    {
        navAgent.isStopped = true;

        //if player backs away out of melee range, resume chasing
        if (distanceToPlayer > attackRange * 1.2f)
        {
            currentState = EnemyState.Chasing;
            return;
        }

        //face the player while attacking
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        directionToPlayer.y = 0;
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }

        //execute attack on cooldown
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PerformMeleeAttack();
            lastAttackTime = Time.time;
        }
    }

    protected virtual void PerformMeleeAttack()
    {
        // Trigger attack animation if animator exists(infrastructure for later)
        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger("Attack");
        }

        PlayerStats player_stats = playerObj.GetComponent<PlayerStats>();
        player_stats.TakeDamage(1);
        Debug.Log($"attacked player for {attackDamage} damage");
    }

    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        StartCoroutine(stun(0.5f));

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        // TODO: Spawn death particle / drop loot here if needed
        Destroy(gameObject);
    }
    protected virtual IEnumerator stun(float time)
    {
        yield return new WaitForSeconds(time);
    }
}