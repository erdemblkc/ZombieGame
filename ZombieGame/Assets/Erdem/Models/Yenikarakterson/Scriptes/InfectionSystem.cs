using UnityEngine;
using System;

public class InfectionSystem : MonoBehaviour
{
    [Header("Infection")]
    [Range(0f, 100f)] public float infection = 0f;
    public float maxInfection = 100f;

    [Tooltip("Zamanla 100'e kaç saniyede ulaşsın? (Örn 600 = 10 dk)")]
    public float timeToMaxSeconds = 2400f; // ✅ daha yavaş: 10 dakika

    [Tooltip("Hasar yedikçe eklenecek yüzde (sabit kalsın: 10)")]
    public float increaseOnHit = 10f;

    public bool isDead = false;

    public Action<float> OnInfectionChanged; // 0..1

    void Start()
    {
        infection = 0f;
        BroadcastUI();
    }

    void Update()
    {
        if (isDead) return;

        // ✅ Zamanla daha yavaş artış
        float perSecond = (timeToMaxSeconds <= 0.001f) ? 0f : (maxInfection / timeToMaxSeconds);
        AddInfection(perSecond * Time.deltaTime);
    }

    public void OnPlayerDamaged()
    {
        if (isDead) return;
        AddInfection(increaseOnHit); // ✅ sabit +10
    }

    public void AddInfection(float amount)
    {
        if (isDead) return;

        infection = Mathf.Clamp(infection + amount, 0f, maxInfection);
        BroadcastUI();

        if (infection >= maxInfection)
            DieFromInfection();
    }

    // ✅ Küp alınca direkt sıfırla
    public void ResetInfection()
    {
        if (isDead) return;
        infection = 0f;
        BroadcastUI();
    }

    void DieFromInfection()
    {
        isDead = true;
        Debug.Log("Player died from infection!");
    }

    void BroadcastUI()
    {
        OnInfectionChanged?.Invoke(infection / maxInfection);
    }
}
