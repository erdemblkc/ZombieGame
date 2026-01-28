using UnityEngine;
using System;

public class InfectionSystem : MonoBehaviour
{
    [Header("Infection (0..100)")]
    [Range(0f, 100f)] public float infection = 0f;
    public float maxInfection = 100f;

    [Header("Gain Multiplier (Upgrades)")]
    [Tooltip("1 = normal. 0.7 = %30 daha yavaş artar.")]
    public float gainMultiplier = 1f;

    [Header("Gain Over Time")]
    [Tooltip("0'dan 100'e kaç saniyede ulaşsın? Örn 180 = 3 dakika")]
    public float timeToMaxSeconds = 180f;

    [Header("Gain On Hit")]
    [Tooltip("Hasar yediğinde eklenecek enfeksiyon miktarı (0..100 ölçeğinde). Örn 2 = %2")]
    public float addOnHit = 2f;

    [Header("State")]
    public bool isDead = false;

    public Action<float> OnInfectionChanged; // normalized 0..1

    void Start()
    {
        infection = Mathf.Clamp(infection, 0f, maxInfection);
        BroadcastUI();
    }

    void Update()
    {
        if (isDead) return;

        if (timeToMaxSeconds > 0.001f)
        {
            float perSecond = maxInfection / timeToMaxSeconds; // örn 100/180
            AddInfection(perSecond * Time.deltaTime); // multiplier içeride uygulanıyor
        }
    }

    // Player hasar alınca çağır
    public void OnPlayerDamaged()
    {
        if (isDead) return;
        AddInfection(addOnHit); // multiplier içeride uygulanıyor
    }

    public void AddInfection(float amount)
    {
        if (isDead) return;

        amount *= Mathf.Max(0f, gainMultiplier);

        float prev = infection;
        infection = Mathf.Clamp(infection + amount, 0f, maxInfection);

        if (!Mathf.Approximately(prev, infection))
            BroadcastUI();

        if (infection >= maxInfection)
            DieFromInfection();
    }

    public void ReduceInfection(float amount)
    {
        if (isDead) return;

        float prev = infection;
        infection = Mathf.Clamp(infection - Mathf.Abs(amount), 0f, maxInfection);

        if (!Mathf.Approximately(prev, infection))
            BroadcastUI();
    }

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
