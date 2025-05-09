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
    private bool isChasing = false;
    private ChrisWalkerAnimation chrisWalkerAnimation;

    private void Awake()
    {
        chrisWalkerAnimation = GetComponent<ChrisWalkerAnimation>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GoToNextPatrolPoint();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (distanceToPlayer < visionRange)
        {
            chrisWalkerAnimation.PlayRunAnimation();
            isChasing = true;
            agent.SetDestination(player.transform.position);
        }
        else if (distanceToPlayer < attackRange)
        {
            isChasing = false;
            chrisWalkerAnimation.StopRunAnimation();
            AttackPlayer();
        }
        else if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextPatrolPoint();
        }
    }

    private void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.destination = patrolPoints[currentPointIndex].position;
        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
    }

    private bool CanSeePlayer()
    {
        chrisWalkerAnimation.PlayRunAnimation();
        RaycastHit hit;
        Vector3 direction = player.transform.position - transform.position;
        if (Physics.Raycast(transform.position, direction, out hit, visionRange))
        {
            return hit.collider.CompareTag("Player");
        }
        return false;
    }

    private void AttackPlayer()
    {
        Debug.Log("Chris Walker attacks!");
    }
}