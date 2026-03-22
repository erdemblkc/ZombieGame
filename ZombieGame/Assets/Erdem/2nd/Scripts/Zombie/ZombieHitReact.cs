using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ZombieHitReact : MonoBehaviour
{
    [Header("Knockback (optional)")]
    public bool enableKnockback = true;
    public float knockbackDistance = 0.35f;
    public float knockbackTime = 0.08f;

    [Header("No Stun / No Freeze")]
    public bool stopAIWhileKnockback = false; // false = AI hiç kapanmaz
    public bool stopAgentWhileKnockback = false; // false = agent hiç durmaz

    NavMeshAgent agent;
    MonoBehaviour followAI;
    Coroutine running;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        followAI = GetComponent<ZombieAI_Follow>();
    }

    public void React(Vector3 attackerPos)
    {
        // Stack olmasýn: knockback çalýþýyorsa yeniden baþlatma
        if (running == null)
            running = StartCoroutine(ReactRoutine(attackerPos));
    }

    IEnumerator ReactRoutine(Vector3 attackerPos)
    {
        bool hadAgent = (agent != null && agent.enabled);

        // Ýstersen knockback süresince AI/agent çok kýsa kesilebilir (default kapalý)
        if (stopAIWhileKnockback && followAI != null) followAI.enabled = false;
        bool prevStopped = false;
        if (hadAgent && stopAgentWhileKnockback)
        {
            prevStopped = agent.isStopped;
            agent.isStopped = true;
        }

        if (enableKnockback)
        {
            Vector3 dir = (transform.position - attackerPos);
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) dir = -transform.forward;
            dir.Normalize();

            float moved = 0f;
            float t = 0f;

            while (t < knockbackTime)
            {
                t += Time.deltaTime;
                float a = Mathf.Clamp01(t / knockbackTime);

                float targetMoved = knockbackDistance * a;
                float step = targetMoved - moved; // bu frame’de ne kadar ilerleyelim
                moved = targetMoved;

                Vector3 delta = dir * step;

                if (hadAgent)
                    agent.Move(delta);
                else
                    transform.position += delta;

                yield return null;
            }
        }

        // Geri aç
        if (hadAgent && stopAgentWhileKnockback) agent.isStopped = prevStopped;
        if (stopAIWhileKnockback && followAI != null) followAI.enabled = true;

        running = null;
    }
}
