using UnityEngine;
using UnityEngine.UI;

public class InfectionUI : MonoBehaviour
{
    [Header("Refs")]
    public InfectionSystem infectionSystem;
    public Slider infectionSlider;

    [Header("Optional")]
    public bool hideWhenZero = false;
    public float smoothSpeed = 12f;

    private float _targetValue;

    void Awake()
    {
        if (infectionSlider != null)
        {
            infectionSlider.minValue = 0f;
            infectionSlider.maxValue = 1f;
        }
    }

    void Start()
    {
        if (infectionSystem == null)
            infectionSystem = FindFirstObjectByType<InfectionSystem>();

        if (infectionSystem != null)
            infectionSystem.OnInfectionChanged += OnInfectionChanged;

        // ilk değer
        if (infectionSystem != null)
            OnInfectionChanged(infectionSystem.infection / infectionSystem.maxInfection);
    }

    void OnDestroy()
    {
        if (infectionSystem != null)
            infectionSystem.OnInfectionChanged -= OnInfectionChanged;
    }

    void Update()
    {
        if (infectionSlider == null) return;

        // yumuşak geçiş
        infectionSlider.value = Mathf.Lerp(infectionSlider.value, _targetValue, smoothSpeed * Time.deltaTime);

        if (hideWhenZero)
        {
            bool shouldShow = infectionSlider.value > 0.001f;
            if (infectionSlider.gameObject.activeSelf != shouldShow)
                infectionSlider.gameObject.SetActive(shouldShow);
        }
    }

    void OnInfectionChanged(float normalized01)
    {
        _targetValue = Mathf.Clamp01(normalized01);
    }
}
