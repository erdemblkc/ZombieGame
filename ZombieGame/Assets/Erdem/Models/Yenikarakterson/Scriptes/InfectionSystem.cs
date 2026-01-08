using UnityEngine;
using System;

public class InfectionSystem : MonoBehaviour
{
    [Header("Infection")]
    [Range(0f, 100f)] public float infection = 0f;
    public float maxInfection = 100f;

    [Tooltip("100'e kaç saniyede ulaþsýn? (3 dk = 180 sn)")]
    public float timeToMaxSeconds = 180f;

    [Tooltip("Hasar yedikçe eklenecek yüzde")]
    public float increaseOnHit = 10f;

    [Header("Antidote")]
    [Tooltip("Panzehir kullanýnca azalacak yüzde")]
    public float decreaseOnAntidote = 25f;

    public bool isDead = false;

    // UI baðlayýnca kullanacaðýz
    public Action<float> OnInfectionChanged; // 0..1

    void Start()
    {
        infection = 0f;
        BroadcastUI();
    }

    void Update()
    {
        if (isDead) return;

        // Zamanla artýþ: 0->100, timeToMaxSeconds sürede
        float perSecond = maxInfection / timeToMaxSeconds;
        AddInfection(perSecond * Time.deltaTime);
    }

    public void OnPlayerDamaged()
    {
        if (isDead) return;
        AddInfection(increaseOnHit);
    }

    public void UseAntidote()
    {
        if (isDead) return;
        AddInfection(-decreaseOnAntidote);
    }

    public void AddInfection(float amount)
    {
        if (isDead) return;

        infection = Mathf.Clamp(infection + amount, 0f, maxInfection);
        BroadcastUI();

        if (infection >= maxInfection)
        {
            DieFromInfection();
        }
    }

    void DieFromInfection()
    {
        isDead = true;
        Debug.Log("Player died from infection!");

        // Buraya kendi ölüm sistemini baðlayacaðýz:
        // GetComponent<PlayerHealth>()?.Die();
        // veya GameManager.RestartLevel();
    }

    void BroadcastUI()
    {
        OnInfectionChanged?.Invoke(infection / maxInfection);
    }
}
