using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sahnedeki yakındaki zombileri geçici olarak bu transform'a yönlendirir.
/// GhostDoubleEvolution tarafından oluşturulan sahte hedefe eklenir.
/// Decoy yok edildiğinde zombiler orijinal hedeflerine döner.
/// </summary>
public class DecoyDistractor : MonoBehaviour
{
    [Tooltip("Bu yarıçap içindeki zombiler decoy'a çekilir.")]
    public float distractRadius = 12f;

    private readonly List<(ZombieAI_Follow zombie, Transform originalTarget)> _distracted
        = new List<(ZombieAI_Follow, Transform)>();

    void Start()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, distractRadius);
        foreach (Collider col in hits)
        {
            var zombie = col.GetComponentInParent<ZombieAI_Follow>();
            if (zombie == null) continue;

            _distracted.Add((zombie, zombie.target));
            zombie.target = transform;
        }
    }

    void OnDestroy()
    {
        foreach (var entry in _distracted)
        {
            if (entry.zombie != null)
                entry.zombie.target = entry.originalTarget;
        }
        _distracted.Clear();
    }
}
