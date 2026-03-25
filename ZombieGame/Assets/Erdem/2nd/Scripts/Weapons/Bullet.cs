using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 80f;
    public float damage = 25f;
    public float lifeTime = 2f;

    /// <summary>
    /// How many additional enemies this bullet can pierce through after the first hit.
    /// Set by PiercerUpgrade. 0 = normal (destroys on first hit).
    /// </summary>
    [HideInInspector] public int piercingCount = 0;

    /// <summary>
    /// How many times this bullet can bounce off non-damageable surfaces.
    /// Set by RicochetUpgrade. 0 = no bouncing.
    /// </summary>
    [HideInInspector] public int ricochetCount = 0;

    private Rigidbody rb;
    private Collider  _col;
    private int       _hitCount   = 0;
    private Vector3   _lastVelocity;  // pre-collision velocity, sampled each FixedUpdate

    void Awake()
    {
        rb   = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void FixedUpdate()
    {
        // Snapshot velocity BEFORE Unity physics processes the next collision.
        // OnCollisionEnter fires after physics has already altered rb.linearVelocity,
        // so we use this snapshot for correct reflection and pierce-through restoration.
        if (rb != null) _lastVelocity = rb.linearVelocity;
    }

    public void Fire(Vector3 direction)
    {
        rb.linearVelocity = direction.normalized * speed;
        _lastVelocity     = rb.linearVelocity;
    }

    void OnCollisionEnter(Collision collision)
    {
        IDamageable damageable = collision.collider.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            HitmarkerUI.Instance?.OnHit();
            _hitCount++;

            if (_hitCount > piercingCount)
            {
                Destroy(gameObject);
                return;
            }

            // Pierce-through: ignore this collider so physics won't react again,
            // then restore pre-collision velocity so bullet keeps going forward.
            if (_col != null)
                Physics.IgnoreCollision(_col, collision.collider, true);
            rb.linearVelocity = _lastVelocity;
            return;
        }

        // Hit a non-damageable surface (wall/environment)
        if (ricochetCount > 0)
        {
            ricochetCount--;
            // Use _lastVelocity (pre-collision) for correct reflection.
            // rb.linearVelocity at this point is already physics-altered.
            Vector3 reflected = Vector3.Reflect(_lastVelocity, collision.contacts[0].normal);
            rb.linearVelocity = reflected;
            _lastVelocity     = reflected;
            if (_col != null)
                Physics.IgnoreCollision(_col, collision.collider, true);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
