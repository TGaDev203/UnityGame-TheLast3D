using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Sound")]
    [SerializeField] protected EnemySoundProfile soundProfile;
    [SerializeField] protected AudioSource enemyAudioSource;
    protected AudioSource audioSource;

    [Header("General Settings")]
    [SerializeField] protected float attackRange;
    [SerializeField] protected float enemyWalkSpeed;
    [SerializeField] protected float enemyRunSpeed;
    [SerializeField] protected float eyeHeight;
    [SerializeField] protected float memoryDuration;
    [SerializeField] protected float pauseActionTime;
    [SerializeField] protected float visionRange;
    [SerializeField] protected float viewAngle;
    [SerializeField] protected Transform[] patrolPoints;

    [Header("AI Runtime State")]
    protected bool isLookingAround = false;
    protected bool isAttacking = false;
    protected float currentVelocity = 0f;
    protected float memoryTimer = 0f;
    protected float nextPauseActionTime = 0f;
    protected float timeSinceLastPauseAction = 0f;
    protected int currentPointIndex = 0;
    protected Vector3 lastSeenPosition;

    [Header("References")]
    [SerializeField] private PauseMenuManager pauseMenuManager;
    protected IEnemyAnimation enemyAnim;
    protected NavMeshAgent agent;
    protected Transform player;

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
        nextPauseActionTime = Random.Range(5f, 10f);
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

            GoToNextPatrolPoint();
        }
        else if (agent.velocity.magnitude > 0.1f && !isLookingAround)
        {
            timeSinceLastPauseAction += Time.deltaTime;
            if (timeSinceLastPauseAction >= nextPauseActionTime)
            {
                StartCoroutine(PerformPauseAction());
            }
        }
    }

    protected virtual void HandleChase()
    {
        var checkpoint = SaveManager.Instance.LoadCheckpoint();
        if (!pauseMenuManager.IsPaused() && (checkpoint == null || !checkpoint.isEnded))
        {
            SoundManager.Instance.PlayChaseSound(enemyAudioSource, soundProfile.chaseSound);
        }

        agent.speed = enemyRunSpeed;

        if (!agent.pathPending && !isAttacking)
        {
            lastSeenPosition = player.position;
            agent.SetDestination(lastSeenPosition);
        }
    }

    protected virtual void GoToNextPatrolPoint()
    {
        var checkpoint = SaveManager.Instance.LoadCheckpoint();
        if (!pauseMenuManager.IsPaused() && (checkpoint == null || !checkpoint.isEnded))
        {
            SoundManager.Instance.PlayVoice(enemyAudioSource, soundProfile.voiceSound);
        }

        if (patrolPoints.Length == 0) return;

        agent.speed = enemyWalkSpeed;
        agent.destination = patrolPoints[currentPointIndex].position;
        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
    }

    protected virtual IEnumerator PerformPauseAction()
    {
        isLookingAround = true;
        timeSinceLastPauseAction = 5f;
        nextPauseActionTime = Random.Range(10f, 20f);

        agent.isStopped = true;

        yield return new WaitForSeconds(pauseActionTime);

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

        DoorController door = other.GetComponentInParent<DoorController>();
        if (door != null && !door.IsOpen)
        {
            StartCoroutine(HandleDoorInteraction(door));
        }
    }

    protected virtual IEnumerator HandleDoorInteraction(DoorController door)
    {
        agent.isStopped = true;

        door.ToggleDoor();
        yield return new WaitForSeconds(1f);
        agent.isStopped = false;
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