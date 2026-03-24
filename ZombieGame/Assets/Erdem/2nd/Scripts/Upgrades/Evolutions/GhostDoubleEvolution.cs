using UnityEngine;

/// <summary>
/// Ghost Double Evolution — requires Phase Step + Decoy (UTILITY, not yet implemented).
/// After each Phase Step, leaves a temporary decoy at the original position
/// that distracts enemies briefly.
///
/// NOTE: The Decoy upgrade from the Utility branch is not yet implemented.
/// This evolution currently spawns a minimal visual placeholder instead.
/// TODO(upgrade-system): Replace placeholder with proper Decoy when implemented.
/// </summary>
public class GhostDoubleEvolution : MonoBehaviour, IEvolution
{
    [SerializeField] private float _decoyLifetime = 4f;
    [SerializeField] private Color _decoyColor    = new Color(0.5f, 0.5f, 1f, 0.6f);

    private PhaseStepUpgrade _phaseStep;

    public void OnEvolutionEnabled(GameObject player)
    {
        _phaseStep = player.GetComponent<PhaseStepUpgrade>();
        if (_phaseStep != null)
            _phaseStep.OnPhaseStepPerformed += SpawnDecoy;

        Debug.Log("[GhostDouble] Evolution active.");
    }

    public void OnEvolutionDisabled()
    {
        if (_phaseStep != null)
            _phaseStep.OnPhaseStepPerformed -= SpawnDecoy;

        _phaseStep = null;
    }

    void SpawnDecoy(Vector3 originalPosition)
    {
        // Spawn a primitive sphere placeholder at the original position
        var decoy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        decoy.name = "GhostDecoy";
        decoy.transform.position = originalPosition;

        // Visual: semi-transparent blue
        var rend = decoy.GetComponent<Renderer>();
        if (rend != null)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = _decoyColor;
            rend.material = mat;
        }

        // Remove collider so it doesn't block movement
        var col = decoy.GetComponent<Collider>();
        if (col != null) Destroy(col);

        // Distracts NavMesh agents: TODO(upgrade-system) — set agent destination to decoy
        // when Decoy upgrade is implemented (will have proper AI logic).

        Destroy(decoy, _decoyLifetime);
    }
}
