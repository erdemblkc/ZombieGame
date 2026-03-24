using UnityEngine;

/// <summary>
/// Predator Drop Evolution — requires Grapple + GroundSlam.
/// After a Grapple pull ends, automatically triggers a Ground Slam.
/// Combines the two upgrades for a devastating dive attack.
/// </summary>
public class PredatorDropEvolution : MonoBehaviour, IEvolution
{
    private GrappleUpgrade    _grapple;
    private GroundSlamUpgrade _slam;
    private bool              _wasGrappling;

    public void OnEvolutionEnabled(GameObject player)
    {
        _grapple = player.GetComponent<GrappleUpgrade>();
        _slam    = player.GetComponent<GroundSlamUpgrade>();
        Debug.Log("[PredatorDrop] Evolution active.");
    }

    public void OnEvolutionDisabled()
    {
        _grapple = null;
        _slam    = null;
    }

    void Update()
    {
        if (_grapple == null || _slam == null) return;

        bool grappling = _grapple.IsGrappling;

        // Detect the moment grapple ends (was grappling, now it stopped)
        if (_wasGrappling && !grappling)
            TriggerSlam();

        _wasGrappling = grappling;
    }

    void TriggerSlam()
    {
        var cc = GetComponent<CharacterController>();
        if (cc != null && !cc.isGrounded)
            _slam.TriggerSlam();
    }
}
