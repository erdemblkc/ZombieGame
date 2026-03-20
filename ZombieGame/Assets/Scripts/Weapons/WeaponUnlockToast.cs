using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponUnlockToast : MonoBehaviour
{
    public CanvasGroup cg;
    public RectTransform root;
    public TextMeshProUGUI titleTMP;
    public Image gunImage;

    [Header("Timing")]
    public float showTime = 1.5f;
    public float fadeOutTime = 0.35f;

    [Header("Motion")]
    public float risePixels = 40f;      // kaybolurken yukarý çýkma
    public float startScale = 1.05f;    // küçük pop hissi

    Coroutine co;

    void Awake()
    {
        if (cg == null) cg = GetComponent<CanvasGroup>();
        if (root == null) root = GetComponent<RectTransform>();

        // baþlangýç gizli
        cg.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void Show(Sprite weaponSprite, string title = "NEW WEAPON")
    {
        if (co != null) StopCoroutine(co);
        gameObject.SetActive(true);

        if (titleTMP != null) titleTMP.text = title;
        if (gunImage != null)
        {
            gunImage.sprite = weaponSprite;
            gunImage.enabled = (weaponSprite != null);
        }

        co = StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        // anýnda göster (pause yok)
        cg.alpha = 1f;

        Vector2 startPos = root.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0f, risePixels);

        root.localScale = Vector3.one * startScale;

        // çok kýsa pop (0.08 sn)
        float popT = 0f;
        const float popDur = 0.08f;
        while (popT < popDur)
        {
            popT += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(popT / popDur);
            root.localScale = Vector3.Lerp(Vector3.one * startScale, Vector3.one, k);
            yield return null;
        }
        root.localScale = Vector3.one;

        // ekranda kalma
        yield return new WaitForSecondsRealtime(showTime);

        // fade-out + yukarý kay
        float t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeOutTime);

            cg.alpha = 1f - k;
            root.anchoredPosition = Vector2.Lerp(startPos, endPos, k);

            yield return null;
        }

        cg.alpha = 0f;
        root.anchoredPosition = startPos;
        gameObject.SetActive(false);
        co = null;
    }
}
