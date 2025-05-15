using UnityEngine;
using UnityEngine.AI;

public class ChrisWalkerAI : MonoBehaviour
{
    public float attackRange;
    private float currentVelocity = 0f;
    public float hearingRange;
    public float visionRange;
    private AudioSource chrisWalkerAudioSource;
    private int currentPointIndex = 0;
    private ChrisWalkerAnimation chrisWalkerAnimation;
    private NavMeshAgent agent;
    public Transform[] patrolPoints;
    private Transform player;
    // float viewDistance = 10f;
    float viewAngle = 120f;
    float eyeHeight = 1.7f;

    private void Awake()
    {
        chrisWalkerAnimation = GetComponent<ChrisWalkerAnimation>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        chrisWalkerAudioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
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