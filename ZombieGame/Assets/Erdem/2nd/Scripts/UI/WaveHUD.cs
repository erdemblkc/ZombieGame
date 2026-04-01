using TMPro;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Wave HUD:
///   1. Sol üst köşede kalıcı oda ismi (PersistentLabel) — değişince DOTween flash.
///   2. Wave başında/bitişinde ekran ortasında slide+scale+fade animasyonlu duyuru.
///
/// Inspector:
///   _roomNames[0] = "Living Room"   → Wave 1
///   _roomNames[1] = "Bedroom"       → Wave 2
///   vb.
/// </summary>
public class WaveHUD : MonoBehaviour
{
    [Header("Persistent Label (sol üst köşe)")]
    [SerializeField] private TextMeshProUGUI _persistentLabel;

    [Header("Announcement (ekran ortası)")]
    [SerializeField] private TextMeshProUGUI _announcementLabel;
    [SerializeField] private float _announcementShowDuration = 2f;
    [SerializeField] private float _completionShowDuration   = 1.5f;

    [Header("Oda İsimleri")]
    [Tooltip("Wave 1 → index 0, Wave 2 → index 1, vb.")]
    [SerializeField] private string[] _roomNames;

    [Header("Animasyon Ayarları")]
    [SerializeField] private float _slideInDistance  = 80f;
    [SerializeField] private float _slideOutDistance = 30f;
    [SerializeField] private Color _labelFlashColor  = new Color(1f, 0.85f, 0.1f);

    Vector2 _announcementRestPos;

    // ── Lifecycle ─────────────────────────────────────────────────────────

    void Awake()
    {
        if (_announcementLabel != null)
        {
            _announcementRestPos = _announcementLabel.rectTransform.anchoredPosition;
            _announcementLabel.alpha = 0f;
        }
    }

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>Oda geçişinde doğrudan isim ver. Sol üst label güncellenir + ekran ortası animasyon.</summary>
    public void ShowRoomName(string roomName)
    {
        if (_persistentLabel != null)
        {
            _persistentLabel.text = roomName;
            PlayLabelFlash();
        }
        PlayAnnouncement(roomName.ToUpper(), _announcementShowDuration);
    }

    /// <summary>Wave başında çağrılır. Sol üst label güncellenir + ekran ortası animasyon.</summary>
    public void ShowWaveAnnouncement(int waveNumber)
    {
        string label = GetLabel(waveNumber);

        if (_persistentLabel != null)
        {
            _persistentLabel.text = label;
            PlayLabelFlash();
        }

        PlayAnnouncement(label.ToUpper(), _announcementShowDuration);
    }

    /// <summary>Wave tamamlandığında ekran ortasında kısa duyuru.</summary>
    public void ShowWaveComplete(int waveNumber)
    {
        PlayAnnouncement($"{GetLabel(waveNumber).ToUpper()}\nTEMİZLENDİ", _completionShowDuration);
    }

    // ── Internal ──────────────────────────────────────────────────────────

    string GetLabel(int waveNumber)
    {
        int idx = waveNumber - 1;
        if (_roomNames != null && idx >= 0 && idx < _roomNames.Length &&
            !string.IsNullOrEmpty(_roomNames[idx]))
            return _roomNames[idx];
        return $"Wave {waveNumber}";
    }

    /// <summary>Sol üst label değişince sarı flash + scale punch.</summary>
    void PlayLabelFlash()
    {
        if (_persistentLabel == null) return;

        DOTween.Kill(_persistentLabel.transform);
        DOTween.Kill(_persistentLabel);

        _persistentLabel.transform.localScale = Vector3.one;
        _persistentLabel.color = Color.white;

        _persistentLabel.transform
            .DOPunchScale(Vector3.one * 0.4f, 0.45f, 6, 0.5f)
            .SetUpdate(true);

        _persistentLabel
            .DOColor(_labelFlashColor, 0.12f)
            .SetUpdate(true)
            .SetLoops(4, LoopType.Yoyo)
            .OnComplete(() =>
            {
                if (_persistentLabel != null)
                    _persistentLabel.color = Color.white;
            });
    }

    /// <summary>Ekran ortasına yukarıdan kayarak gelen, bekleyip küçülerek giden metin.</summary>
    void PlayAnnouncement(string text, float holdDuration)
    {
        if (_announcementLabel == null) return;

        // Önceki tweenleri temizle
        DOTween.Kill(_announcementLabel);
        DOTween.Kill(_announcementLabel.transform);

        _announcementLabel.text  = text;
        _announcementLabel.alpha = 0f;
        _announcementLabel.transform.localScale = Vector3.one * 0.75f;
        _announcementLabel.rectTransform.anchoredPosition =
            _announcementRestPos + new Vector2(0f, _slideInDistance);

        var seq = DOTween.Sequence().SetUpdate(true);

        // Giriş: yukarıdan kayma + büyüme + fade in
        seq.Join(_announcementLabel
            .DOFade(1f, 0.35f)
            .SetEase(Ease.OutQuad));

        seq.Join(_announcementLabel.rectTransform
            .DOAnchorPos(_announcementRestPos, 0.4f)
            .SetEase(Ease.OutBack));

        seq.Join(_announcementLabel.transform
            .DOScale(1f, 0.4f)
            .SetEase(Ease.OutBack));

        // Bekle
        seq.AppendInterval(holdDuration);

        // Çıkış: aşağıya kayma + küçülme + fade out
        seq.Append(_announcementLabel
            .DOFade(0f, 0.4f)
            .SetEase(Ease.InQuad));

        seq.Join(_announcementLabel.rectTransform
            .DOAnchorPos(_announcementRestPos - new Vector2(0f, _slideOutDistance), 0.4f)
            .SetEase(Ease.InBack));

        seq.Join(_announcementLabel.transform
            .DOScale(0.8f, 0.4f)
            .SetEase(Ease.InBack));

        seq.OnComplete(() =>
        {
            if (_announcementLabel != null)
                _announcementLabel.rectTransform.anchoredPosition = _announcementRestPos;
        });
    }
}
