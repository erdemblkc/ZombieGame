using System.Collections;
using UnityEngine;

public class ZombieHitFlash : MonoBehaviour
{
    public float flashDuration = 0.08f;
    public Color flashColor = Color.red;

    Renderer[] renderers;
    MaterialPropertyBlock mpb;
    Color[] originalColors;
    string colorProp = "_BaseColor";

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        mpb = new MaterialPropertyBlock();

        // URP: _BaseColor, Standard: _Color
        if (renderers.Length > 0 && renderers[0].sharedMaterial != null)
        {
            var mat = renderers[0].sharedMaterial;
            if (mat.HasProperty("_BaseColor")) colorProp = "_BaseColor";
            else if (mat.HasProperty("_Color")) colorProp = "_Color";
        }

        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            var mat = renderers[i].sharedMaterial;
            if (mat != null && mat.HasProperty(colorProp))
                originalColors[i] = mat.GetColor(colorProp);
            else
                originalColors[i] = Color.white;
        }
    }

    public void Flash()
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].GetPropertyBlock(mpb);
            mpb.SetColor(colorProp, flashColor);
            renderers[i].SetPropertyBlock(mpb);
        }

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].GetPropertyBlock(mpb);
            mpb.SetColor(colorProp, originalColors[i]);
            renderers[i].SetPropertyBlock(mpb);
        }
    }
}
