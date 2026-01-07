using UnityEngine;

public class ZombieLifeDebug : MonoBehaviour
{
    private void OnEnable()
    {
        Debug.Log($"[ZombieLifeDebug] ENABLE: {name}", this);
    }

    private void OnDisable()
    {
        Debug.Log($"[ZombieLifeDebug] DISABLE: {name}", this);
    }

    private void OnDestroy()
    {
        Debug.Log($"[ZombieLifeDebug] DESTROY: {name}", this);
    }
}
