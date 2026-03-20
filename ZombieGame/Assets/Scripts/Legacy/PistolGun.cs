using UnityEngine;
using System.Collections;

public class PistolGun : MonoBehaviour
{
    [Header("Ammo")]
    public int maxAmmo = 7;
    public float reloadTime = 1.2f;

    [Header("Fire")]
    public float fireRate = 0.25f;
    public float damage = 20f;
    public float range = 100f;

    [Header("References")]
    public Camera fpsCam;                 // Gerçekte görüntüyü veren kamera (FPSCamera)
    public Animator characterAnimator;    // Player modelinin Animator'ý
    public string shootTriggerName = "Shoot";

    private int currentAmmo;
    private bool isReloading = false;
    private float nextFireTime = 0f;

    void Start()
    {
        currentAmmo = maxAmmo;

        if (fpsCam == null)
            fpsCam = Camera.main;
    }

    void Update()
    {
        if (isReloading)
            return;

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetKey(KeyCode.Mouse0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        currentAmmo--;

        // Ateþ animasyonu
        if (characterAnimator != null && !string.IsNullOrEmpty(shootTriggerName))
        {
            characterAnimator.SetTrigger(shootTriggerName);
        }

        // === BURASI ÖNEMLÝ ===
        // Ray = ekranýn tam ortasýndan (0.5, 0.5) çýkan ray
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, range))
        {
            // Mermi yolunu görmek için çizgi (istersen sonra silebilirsin)
            Debug.DrawLine(ray.origin, hit.point, Color.red, 0.2f);

            ZombieHealth target = hit.collider.GetComponentInParent<ZombieHealth>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }
        }
        else
        {
            // Boþsa da çizgi çiz (ekrandan ileri doðru)
            Vector3 endPoint = ray.origin + ray.direction * range;
            Debug.DrawLine(ray.origin, endPoint, Color.yellow, 0.2f);
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
    }
}
