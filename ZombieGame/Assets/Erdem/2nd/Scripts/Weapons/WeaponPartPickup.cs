using UnityEngine;

public class WeaponPartPickup : MonoBehaviour
{
    public WeaponUpgradeManager manager;
    public string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (manager != null)
            manager.CollectWeaponPart();

        // parçayý yok et
        Destroy(gameObject);
    }
}
