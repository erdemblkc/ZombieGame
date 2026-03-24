using UnityEngine;

/// <summary>
/// Cannonball Evolution — requires Slide + Armor (UTILITY, not yet implemented).
/// While sliding, deal contact damage to enemies the player passes through.
///
/// NOTE: This evolution will not activate until the Armor upgrade from the Utility
/// branch is implemented and its UpgradeData asset is assigned to the EvolutionRegistry.
/// TODO(upgrade-system): Wire up when ArmorUpgrade is added.
/// </summary>
public class CannonballEvolution : MonoBehaviour, IEvolution
{
    [SerializeField] private float _contactDamage = 25f;
    [SerializeField] private float _hitCooldown   = 0.4f;   // prevent multi-hits per enemy

    private SlideUpgrade _slide;
    private float        _globalHitCooldown;

    public void OnEvolutionEnabled(GameObject player)
    {
        _slide = player.GetComponent<SlideUpgrade>();
        Debug.Log("[Cannonball] Evolution active.");
    }

    public void OnEvolutionDisabled()
    {
        _slide = null;
    }

    void Update()
    {
        if (_slide == null || !_slide.IsSliding) return;
        if (_globalHitCooldown > 0f) { _globalHitCooldown -= Time.deltaTime; return; }

        // Overlap check in front of slide direction
        var cc  = GetComponent<CharacterController>();
        if (cc == null) return;

        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 0.8f, 0.9f);
        foreach (var col in hits)
        {
            if (col.gameObject == gameObject) continue;
            var d = col.GetComponent<IDamageable>();
            if (d == null) continue;
            d.TakeDamage(_contactDamage);
            _globalHitCooldown = _hitCooldown;
            break;  // One hit per frame to avoid triple-hitting the same enemy
        }
    }
}
