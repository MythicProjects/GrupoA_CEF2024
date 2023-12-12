using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum States
{
    Idle,
    Patrol,
    Search,
    Chase,
    Attack
}
public class EnemyController : MonoBehaviour
{
    //Acceso a los scripts del enemigo
    private EnemyLocomotion locomotion;
    private EnemySense enemySense;
    //

    //Flags
    [Header("Flags")]
    public bool isGrounded;
    public bool wallCollision;


    [Header("Enemy States")]
    public States currentState;
    public bool playerDetected;

    [Header("Idle Settings")]
    public float minTime = 0.5f, maxTime = 3f;
    private float idleTime;
    private float idleTimeCount;

    [Header("Patrol Settings")]
    public float patrolRange;
    public float patrolSpeed = 4;
    private Vector3 startPosition;

    [Header("Search Settings")]
    public float searchRange;
    public float searchSpeed = 4;
    private Vector3 searchPosition;


    [Header("Chase Settings")]
    public float chaseSpeed = 6;

    //[Header("Attack Settings")]
    //

    private void Awake()
    {
        locomotion = GetComponent<EnemyLocomotion>();
        enemySense = GetComponent<EnemySense>();

        locomotion.GetLocomotionComponents();
        enemySense.GetSenseComponents();

        SetIdleState();
    }

    private void Start()
    {
        locomotion.SetLocomotionComponents();

        idleTime = Random.Range(minTime, maxTime);
        startPosition = transform.position;
    }

    private void Update()
    {
        float delta = Time.deltaTime;

        enemySense.DetectPlayer(delta);
        EnemyStateMachineBehaviour();
    }

    private void FixedUpdate()
    {
        float delta = Time.fixedDeltaTime;
        locomotion.HandleEnemyRotation(delta);
    }

    //States
    private void EnemyStateMachineBehaviour()
    {
        if (wallCollision)
        {
            SetIdleState();
        }

        if (playerDetected)
        {
            SetChaseState();

        }

        switch (currentState)
        {
            case States.Idle:
                IdleState();
                break;
            case States.Patrol:
                PatrolState();
                break;
            case States.Search:
                SearchState();
                break;
            case States.Chase:
                ChaseState();
                break;
        }
    }
    //Idle---------------------------------------
    private void SetIdleState()
    {
        idleTime = Random.Range(minTime, maxTime);

        currentState = States.Idle;
    }

    private void IdleState()
    {
        idleTimeCount += Time.deltaTime;

        if (idleTimeCount > idleTime)
        {
            SetPatrolState();
            idleTimeCount = 0;
        }
    }
    //Patrol---------------------------------------
    private void SetPatrolState()
    {
        Vector3 target;

        Vector3 randomDirection = new Vector3(Random.Range(0, 10), 0, Random.Range(0, 10));
        randomDirection.Normalize();

        float randomRange = Random.Range(-patrolRange, patrolRange);


        target = startPosition + (randomDirection * randomRange);

        locomotion.SetAgentDestination(target);
        locomotion.SetAgentSpeed(patrolSpeed);

        currentState = States.Patrol;

    }
    private void PatrolState()
    {
        if(locomotion.ArrivesToTarget())
        {
            SetIdleState();

            return;
        }
    }
    //Search---------------------------------------
    private void SetSearchState()
    {
        Vector3 target;
        searchPosition = enemySense.targetToPlayer;

        Vector3 randomDirection = new Vector3(Random.Range(0, 10), 0, Random.Range(0, 10));
        randomDirection.Normalize();

        float randomRange = Random.Range(-searchRange, searchRange);


        target = searchPosition + (randomDirection * randomRange);

        locomotion.SetAgentDestination(target);
        locomotion.SetAgentSpeed(searchSpeed);

        currentState = States.Search;

    }
    private void SearchState()
    {
        if (locomotion.ArrivesToTarget())
        {
            PatrolState();
            return;
        }
    }
    //Chase---------------------------------------
    private void SetChaseState()
    {
        locomotion.SetAgentDestination(enemySense.targetToPlayer);
        locomotion.SetAgentSpeed(chaseSpeed);

        currentState = States.Chase;
    }
    private void ChaseState()
    {
        if (!playerDetected)
        {
            SetPatrolState();

            return;
        }
    }

    //Attack---------------------------------------
    public void Attack()
    {
        //playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 1, 0.2f);

        if (Application.isPlaying)
        {
            Gizmos.DrawSphere(startPosition, patrolRange);
        }
        else
        {
            Gizmos.DrawSphere(transform.position, patrolRange);
        }
    }
}
