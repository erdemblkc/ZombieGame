using TMPro;
using UnityEngine;

public class InteractPromptUI : MonoBehaviour
{
    public static InteractPromptUI Instance;

    public TextMeshProUGUI promptText;
    public GameObject root; // text parent (panel vs)

    void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Show(string text)
    {
        if (root != null) root.SetActive(true);
        if (promptText != null) promptText.text = text;
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
    }
}
