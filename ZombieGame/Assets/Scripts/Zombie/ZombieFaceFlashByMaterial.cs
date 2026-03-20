using UnityEngine;
using System.Collections.Generic;

public class ZombieFaceFlashByMaterial : MonoBehaviour
{
    [Header("Which material represents the face? (e.g., Material.001)")]
    public Material faceBaseMaterial;

    [Header("Angry material (newface)")]
    public Material angryMaterial;

    struct Slot
    {
        public Renderer r;
        public int index;
    }

    private readonly List<Slot> slots = new List<Slot>();
    private bool angrySet = false;

    void Awake()
    {
        CacheSlots();
    }

    void CacheSlots()
    {
        slots.Clear();

        if (faceBaseMaterial == null) return;

        var renderers = GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == faceBaseMaterial)
                {
                    slots.Add(new Slot { r = r, index = i });
                }
            }
        }
    }

    public void SetAngryPermanent()
    {
        if (angrySet) return;
        if (faceBaseMaterial == null || angryMaterial == null) return;

        if (slots.Count == 0) CacheSlots();

        for (int k = 0; k < slots.Count; k++)
        {
            var s = slots[k];
            if (s.r == null) continue;

            var mats = s.r.sharedMaterials;
            if (s.index < 0 || s.index >= mats.Length) continue;

            mats[s.index] = angryMaterial;
            s.r.sharedMaterials = mats;
        }

        angrySet = true;
    }

    [ContextMenu("TEST SetAngryPermanent")]
    void TEST_Angry() => SetAngryPermanent();
}
