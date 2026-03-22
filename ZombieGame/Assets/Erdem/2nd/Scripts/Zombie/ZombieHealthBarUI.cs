using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ZombieHealthBarUI : MonoBehaviour
{
    public CanvasGroup cg;
    public Image fill;

    public Transform target;
    public Vector3 worldOffset = new Vector3(0, 2.2f, 0);
    public float hideAfter = 0.5f;

    Coroutine co;
    Camera cam;

    void Awake()
    {
        if (cg == null) cg = GetComponent<CanvasGroup>();
        cam = Camera.main;
        SetVisible(false);
    }

    void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position + worldOffset;

        if (cam != null)
            transform.forward = cam.transform.forward;
    }

    public void SetValue(float current, float max)
    {
        if (fill == null) return;
        fill.fillAmount = (max <= 0f) ? 1f : Mathf.Clamp01(current / max);
    }

    public void ShowAndAutoHide()
    {
        SetVisible(true);

        if (co != null) StopCoroutine(co);
        co = StartCoroutine(HideRoutine());
    }

    IEnumerator HideRoutine()
    {
        yield return new WaitForSeconds(hideAfter);
        SetVisible(false);
    }

    void SetVisible(bool v)
    {
        if (cg != null) cg.alpha = v ? 1f : 0f;
    }
}
