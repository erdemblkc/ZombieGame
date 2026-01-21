using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ZombieHitReact : MonoBehaviour
{
    [Header("Hit Stun")]
    public float stunTime = 0.18f;

    [Header("Knockback (optional)")]
    public bool enableKnockback = true;
    public float knockbackDistance = 0.35f;
    public float knockbackTime = 0.08f;

    NavMeshAgent agent;
    MonoBehaviour followAI; // ZombieAI_Follow gibi
    Animator anim;
    Coroutine running;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        // Sende AI script adý ZombieAI_Follow
        followAI = GetComponent<ZombieAI_Follow>();
    }

    public void React(Vector3 attackerPos)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(ReactRoutine(attackerPos));
    }

    IEnumerator ReactRoutine(Vector3 attackerPos)
    {
        // AI + agent durdur
        if (followAI != null) followAI.enabled = false;

        bool hadAgent = (agent != null && agent.enabled);
        bool prevUpdatePos = false;

        if (hadAgent)
        {
            agent.isStopped = true;
            prevUpdatePos = agent.updatePosition;
            agent.updatePosition = false;
        }

        // Animasyonu dondur
        if (anim != null) anim.speed = 0f;

        // Opsiyonel knockback
        if (enableKnockback)
        {
            Vector3 dir = (transform.position - attackerPos);
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) dir = -transform.forward;
            dir.Normalize();

            Vector3 start = transform.position;
            Vector3 target = start + dir * knockbackDistance;

            float t = 0f;
            while (t < knockbackTime)
            {
                t += Time.unscaledDeltaTime; // pause vs olmasa da stabil
                float a = Mathf.Clamp01(t / knockbackTime);
                transform.position = Vector3.Lerp(start, target, a);
                yield return null;
            }
        }

        // Stun süresi (anim dururken bekle)
        float timer = 0f;
        while (timer < stunTime)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        // Devam
        if (anim != null) anim.speed = 1f;

        if (hadAgent)
        {
            agent.Warp(transform.position);
            agent.updatePosition = prevUpdatePos;
            agent.isStopped = false;
        }

        if (followAI != null) followAI.enabled = true;

        running = null;
    }
}
