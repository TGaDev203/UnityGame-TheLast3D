using UnityEngine;
using UnityEngine.AI;

public class ChrisWalkerAI : MonoBehaviour
{
    public Transform[] patrolPoints;
    private int currentPointIndex = 0;
    public float visionRange = 10f;
    public float hearingRange = 8f;
    public float attackRange = 2f;
    private NavMeshAgent agent;
    private Transform player;
    private ChrisWalkerAnimation chrisWalkerAnimation;
    private AudioSource chrisWalkerAudioSource;

    private void Awake()
    {
        chrisWalkerAnimation = GetComponent<ChrisWalkerAnimation>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        chrisWalkerAudioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        SoundManager.Instance.PlayChrisWalkerVoiceAndChainSound(chrisWalkerAudioSource);
        agent = GetComponent<NavMeshAgent>();
        GoToNextPatrolPoint();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (distanceToPlayer < visionRange)
        {
            SoundManager.Instance.PlayChrisWalkerChaseSound();
            chrisWalkerAnimation.PlayRunAnimation();
            agent.speed = 15f;

            if (!agent.pathPending) agent.SetDestination(player.transform.position);
        }
        else if (distanceToPlayer < attackRange)
        {
            AttackPlayer();
        }
        else if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            SoundManager.Instance.StopChrisWalkerChaseSound();
            agent.speed = 5f;
            chrisWalkerAnimation.StopRunAnimation();
            GoToNextPatrolPoint();
        }
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