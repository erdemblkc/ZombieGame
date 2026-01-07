using UnityEngine;
using UnityEngine.AI;

public class ZombieAI1 : MonoBehaviour
{
    [Header("Refs")]
    public Transform target;
    public Animator anim;
    public NavMeshAgent agent;

    [Header("Combat")]
    public float attackRange = 1.4f;

    private void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!anim) anim = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj) target = playerObj.transform;
        }
    }

    private void Update()
    {
        if (target == null || agent == null) return;

        // NavMesh üstünde deðilse hiçbir þey yapma (debug deðil, güvenlik)
        if (!agent.isOnNavMesh) return;

        float dist = Vector3.Distance(transform.position, target.position);

        if (dist > attackRange)
        {
            agent.isStopped = false;
            agent.SetDestination(target.position);

            if (anim) anim.SetBool("IsWalking", true);
        }
        else
        {
            agent.isStopped = true;
            if (anim) anim.SetBool("IsWalking", false);
            if (anim) anim.SetTrigger("Attack");
        }
    }
}
