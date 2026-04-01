using UnityEngine;

public class ToySoldierBullet : MonoBehaviour
{
    public float speed    = 25f;
    public float damage   = 10f;
    public float lifetime = 4f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponentInParent<PlayerDamageReceiver>();
        if (player != null)
        {
            player.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Duvara çarptıysa yok ol (Player değilse)
        if (!other.isTrigger)
            Destroy(gameObject);
    }
}
