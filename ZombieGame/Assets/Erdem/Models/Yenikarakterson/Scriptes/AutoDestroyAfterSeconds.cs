using UnityEngine;

public class AutoDestroyAfterSeconds : MonoBehaviour
{
    public float life = 1f;
    void Start() => Destroy(gameObject, life);
}
