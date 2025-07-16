using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] protected float visionRange;
    [SerializeField] protected float viewAngle;
    [SerializeField] protected float attackRange;
    [SerializeField] protected float eyeHeight;
    [SerializeField] protected float memoryDuration = 3f;
    [SerializeField] protected Transform[] patrolPoints;
    protected bool isLookingAround = false;
    protected bool isAttacking = false;
    protected float currentVelocity = 0f;
    protected float lookTimer = 0f;
    protected float memoryTimer = 0f;
    protected float nextLookTime = 0f;
    protected AudioSource audioSource;
    protected int currentPointIndex = 0;
    protected IEnemyAnimation enemyAnim;
    protected NavMeshAgent agent;
    protected Transform player;
    protected Vector3 lastSeenPosition;

    protected virtual void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        if (!TryGetComponent<IEnemyAnimation>(out enemyAnim))
        {
            Debug.LogWarning($"{gameObject.name} is missing IEnemyAnimation component!");
        }
    }

    protected virtual void Start()
    {
        GoToNextPatrolPoint();
        nextLookTime = Random.Range(5f, 10f);
    }

    protected virtual void Update()
    {
        if (isAttacking) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool canSee = CanSeePlayer();

        if (canSee)
        {
            lastSeenPosition = player.position;
            memoryTimer = memoryDuration;
        }
        else if (memoryTimer > 0f)
        {
            memoryTimer -= Time.deltaTime;
            canSee = true;
        }

        if (distanceToPlayer < visionRange && canSee)
        {
            HandleChase();
        }
        else if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            HandlePatrol();
        }
        else if (agent.velocity.magnitude > 0.1f && !isLookingAround)
        {
            lookTimer += Time.deltaTime;
            if (lookTimer >= nextLookTime)
            {
                StartCoroutine(PerformLookAround());
            }
        }
    }

    protected virtual void HandleChase()
    {
        agent.speed = 25f;

        if (!agent.pathPending && !isAttacking)
        {
            lastSeenPosition = player.position;
            agent.SetDestination(lastSeenPosition);
        }
    }

    protected virtual void HandlePatrol()
    {
        agent.speed = 5f;
        GoToNextPatrolPoint();
    }

    protected virtual void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        agent.destination = patrolPoints[currentPointIndex].position;
        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
    }

    protected virtual IEnumerator PerformLookAround()
    {
        isLookingAround = true;
        lookTimer = 5f;
        nextLookTime = Random.Range(10f, 20f);

        agent.isStopped = true;

        yield return new WaitForSeconds(Random.Range(2f, 4f));

        agent.isStopped = false;
        isLookingAround = false;
    }

    protected virtual bool CanSeePlayer()
    {
        Vector3 direction = player.position - transform.position;
        float distance = direction.magnitude;
        float angle = Vector3.Angle(transform.forward, direction);

        if (distance <= visionRange && angle <= viewAngle / 2f)
        {
            Ray ray = new Ray(transform.position + Vector3.up * eyeHeight, direction);
            if (Physics.Raycast(ray, out RaycastHit hit, distance))
            {
                if (hit.transform.CompareTag("Player"))
                    return true;
            }
        }

        return distance <= visionRange * 0.4f &&
               Physics.Raycast(transform.position + Vector3.up * eyeHeight, direction, out RaycastHit hit2, distance) &&
               hit2.transform.CompareTag("Player");
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AttackPlayer();
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(WaitForAttackToFinish());
        }
    }
    
    protected virtual void AttackPlayer()
    {
        isAttacking = true;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }

    protected virtual IEnumerator WaitForAttackToFinish()
    {
        yield return new WaitForSeconds(0.8f);
        isAttacking = false;
        agent.isStopped = false;
    }
}