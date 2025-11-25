using UnityEngine;

public class GunController : MonoBehaviour
{
    [Header("Silah Ayarlarý")]
    public float damage = 25f;      // Her kurþunun verdiði hasar
    public float range = 100f;      // Menzil
    public float fireRate = 5f;     // Saniyedeki atýþ sayýsý (5 = hýzlý týk týk)

    [Header("Referanslar")]
    public Camera fpsCamera;        // FPS kameramýz

    private float nextTimeToFire = 0f;

    void Update()
    {
        // Sol mouse (Fire1) basýlýysa ve fire rate uygunsa ateþ et
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        if (fpsCamera == null)
        {
            Debug.LogWarning("GunController: FPS Camera atanmadý!");
            return;
        }

        Ray ray = new Ray(fpsCamera.transform.position, fpsCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range))
        {
            // Zombi'yi vurduk mu kontrol et
            ZombieHealth zombie = hit.collider.GetComponent<ZombieHealth>();

            if (zombie != null)
            {
                zombie.TakeDamage(damage);
            }

            // Ýstersen burada debug görebilirsin:
            // Debug.Log("Vurulan obje: " + hit.collider.name);
        }
    }
}
