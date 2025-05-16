using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ChrisWalkerAI : MonoBehaviour
{
    [SerializeField] private float attackRange;
    [SerializeField] private float eyeHeight;
    [SerializeField] private float hearingRange;
    [SerializeField] private float visionRange;
    [SerializeField] private float viewAngle;
    [SerializeField] private Transform[] patrolPoints;
    private float currentVelocity = 0f;
    private float nextLookTime = 0f;
    private float lookTimer = 0f;
    private AudioSource chrisWalkerAudioSource;
    private bool isLookingAround = false;
    private ChrisWalkerAnimation chrisWalkerAnimation;
    private int currentPointIndex = 0;
    private NavMeshAgent agent;
    private Transform player;

    private void Awake()
    {
        chrisWalkerAnimation = GetComponent<ChrisWalkerAnimation>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        chrisWalkerAudioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        nextLookTime = Random.Range(5f, 10f);
        chrisWalkerAnimation.SetVelocity(0.5f);

        SoundManager.Instance.PlayChrisWalkerVoiceAndChainSound(chrisWalkerAudioSource);
        agent = GetComponent<NavMeshAgent>();
        GoToNextPatrolPoint();
    }

    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        bool isRunning = distanceToPlayer < visionRange;
        float targetVelocity = isRunning ? 1f : 0.5f;

        if (distanceToPlayer < visionRange && CanSeePlayer(player))
        {
            SoundManager.Instance.PlayChrisWalkerChaseSound();
            currentVelocity = Mathf.Lerp(currentVelocity, targetVelocity, Time.deltaTime * 5f);
            chrisWalkerAnimation.SetVelocity(currentVelocity);
            agent.speed = 15f;

            if (!agent.pathPending) agent.SetDestination(player.transform.position);
        }
        else if (distanceToPlayer < attackRange)
        {
            AttackPlayer();
        }
        else if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            chrisWalkerAnimation.SetVelocity(targetVelocity);

            SoundManager.Instance.StopChrisWalkerChaseSound();
            agent.speed = 5f;
            chrisWalkerAnimation.SetVelocity(0.5f);
            GoToNextPatrolPoint();
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

    private IEnumerator PerformLookAround()
    {
        isLookingAround = true;
        lookTimer = 5f;
        nextLookTime = Random.Range(10f, 20f);

        agent.isStopped = true;
        chrisWalkerAnimation.SetVelocity(0f);
        chrisWalkerAnimation.PlayLookAroundAnimation();

        yield return new WaitForSeconds(Random.Range(2f, 4f));

        agent.isStopped = false;
        chrisWalkerAnimation.StopLookAroundAnimation();
        chrisWalkerAnimation.SetVelocity(0.5f);
        isLookingAround = false;
    }

    private bool CanSeePlayer(Transform player)
    {
        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        float shortRange = visionRange * 0.4f;

        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (distanceToPlayer <= visionRange && angleToPlayer <= viewAngle / 2f)
        {
            Ray longRay = new Ray(transform.position + Vector3.up * eyeHeight, directionToPlayer);
            if (Physics.Raycast(longRay, out RaycastHit hit1, distanceToPlayer))
            {
                if (hit1.transform.CompareTag("Player"))
                    return true;
            }
        }

        if (distanceToPlayer <= shortRange)
        {
            Ray shortRay = new Ray(transform.position + Vector3.up * eyeHeight, directionToPlayer);
            if (Physics.Raycast(shortRay, out RaycastHit hit2, distanceToPlayer))
            {
                if (hit2.transform.CompareTag("Player"))
                    return true;
            }
        }

        return false;
    }

    private void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.destination = patrolPoints[currentPointIndex].position;
        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
    }

    private void AttackPlayer()
    {
        Debug.Log("Chris Walker attacks!");
    }
}