using UnityEngine;
using static enemyBase;

public class enemyRanged : enemyBase
{
    [Header("Ranged Combat & Rigging")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform headTransform; //assign the child head object here
    [SerializeField] private float preferredDistance = 670f; //target distance to maintain
    [SerializeField] private float distanceTolerance = 1.5f;   //margin buffer
    [SerializeField] private float fireRate = 2.0f;            //interval between shots

    private float nextFireTime = 0f;

    protected override void Start()
    {
        base.Start(); //keep health, target finding, and agent initialization from enemyBase
    }

    protected override void Update()
    {
        if (playerTransform == null) return;

        RunRangedStateMachine();
    }

    void RunRangedStateMachine()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        switch (currentState)
        {
            case EnemyState.Idle:
                if (distanceToPlayer <= detectionRange)
                {
                    currentState = EnemyState.Chasing; //re-purposed as "combat/engagement" state
                }
                break;

            case EnemyState.Chasing: //acting here as engagement/positioning
            case EnemyState.Attacking:
                HandleRangedMovementAndAttack(distanceToPlayer);
                break;
        }
    }

    void HandleRangedMovementAndAttack(float distanceToPlayer)
    {
        //rotate ONLY the head "rig" to face player in all axes
        if (headTransform != null)
        {
            Vector3 directionToPlayer = (playerTransform.position - headTransform.position).normalized;
            if (directionToPlayer != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
                headTransform.rotation = Quaternion.Slerp(headTransform.rotation, lookRotation, Time.deltaTime * 10f);
            }
        }

        //maintain distance margin (strafe/reposition via navmesh on the root body)
        navAgent.isStopped = false;

        if (distanceToPlayer > preferredDistance + distanceTolerance)
        {
            //move closer to player
            navAgent.SetDestination(playerTransform.position);
        }
        else if (distanceToPlayer < preferredDistance - distanceTolerance)
        {
            //back away from player
            Vector3 directionToPlayerBody = (playerTransform.position - transform.position).normalized;
            Vector3 retreatPosition = transform.position - (directionToPlayerBody * 3f);
            navAgent.SetDestination(retreatPosition);
        }
        else
        {
            //distance within margin: hold position
            navAgent.SetDestination(transform.position);
        }

        //firing Logic
        if (Time.time >= nextFireTime && distanceToPlayer <= detectionRange)
        {
            FireProjectile();
            nextFireTime = Time.time + fireRate;
        }

        //lose target if they run away completely
        if (distanceToPlayer > detectionRange * 1.5f)
        {
            currentState = EnemyState.Idle;
        }
    }

    void FireProjectile()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        }
    }
}