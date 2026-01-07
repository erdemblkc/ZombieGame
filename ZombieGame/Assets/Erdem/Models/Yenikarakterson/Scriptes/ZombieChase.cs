using UnityEngine;
using UnityEngine.AI;

public class ZombieChase : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float speed = 2f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float stopDistance = 1.4f;

    private Transform target;
    private NavMeshAgent agent;
    private float logTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        var p = GameObject.FindGameObjectWithTag(playerTag);
        if (p) target = p.transform;

        if (agent != null)
        {
            agent.speed = speed;
            agent.acceleration = acceleration;
            agent.stoppingDistance = stopDistance;
            agent.isStopped = false;
            agent.updatePosition = true;
            agent.updateRotation = true;
        }
    }

    private void Update()
    {
        if (agent == null || target == null) return;

        // Kovalama
        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(target.position);
        }

        // Teþhis log'u (saniyede 2 kere)
        logTimer += Time.deltaTime;
        if (logTimer >= 0.5f)
        {
            logTimer = 0f;

            string msg =
                $"[ChaseDebug] onNavMesh={agent.isOnNavMesh} " +
                $"isStopped={agent.isStopped} speed={agent.speed:F1} " +
                $"vel={agent.velocity.magnitude:F2} " +
                $"path={agent.pathStatus} pending={agent.pathPending} " +
                $"remaining={agent.remainingDistance:F2} " +
                $"hasPath={agent.hasPath}";

            Debug.Log(msg, this);
        }
    }
}
