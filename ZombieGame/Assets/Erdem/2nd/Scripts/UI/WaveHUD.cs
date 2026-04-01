using System.Collections;
using TMPro;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Wave HUD — iki görevi var:
///   1. Köşede kalıcı "Wave 3" yazısı (persistent label).
///   2. Wave başında/bitişinde ekran ortasında beliren ve solan duyuru metni.
///
/// Sahne kurulumu:
///   - PersistentLabel  : Sol/sağ üst köşede küçük bir TextMeshProUGUI.
///   - AnnouncementLabel: Ekran ortasında büyük bir TextMeshProUGUI (başlangıçta gizli).
/// </summary>
public class WaveHUD : MonoBehaviour
{
    [Header("Persistent Label (köşe)")]
    [SerializeField] private TextMeshProUGUI _persistentLabel;

    [Header("Announcement (ekran ortası)")]
    [SerializeField] private TextMeshProUGUI _announcementLabel;

    [SerializeField] private float _announcementShowDuration = 2f;
    [SerializeField] private float _completionShowDuration   = 1.5f;

    [Header("Oda İsimleri (opsiyonel)")]
    [Tooltip("Wave numarasına karşılık gelen oda ismi. Wave 1 → index 0, Wave 2 → index 1 vb.")]
    [SerializeField] private string[] _roomNames;

    // ── API ───────────────────────────────────────────────────────────────

    /// <summary>Günceller köşedeki kalıcı etiketi ve büyük Wave N duyurusunu gösterir.</summary>
    public void ShowWaveAnnouncement(int waveNumber)
    {
        string label = GetLabel(waveNumber);

        if (_persistentLabel != null)
            _persistentLabel.text = label;

        ShowAnnouncement(label.ToUpper(), _announcementShowDuration);
    }

    /// <summary>Wave tamamlandığında kısa duyuru gösterir.</summary>
    public void ShowWaveComplete(int waveNumber)
    {
        ShowAnnouncement($"{GetLabel(waveNumber).ToUpper()}\nTEMİZLENDİ", _completionShowDuration);
    }

    string GetLabel(int waveNumber)
    {
        int idx = waveNumber - 1;
        if (_roomNames != null && idx >= 0 && idx < _roomNames.Length && !string.IsNullOrEmpty(_roomNames[idx]))
            return _roomNames[idx];
        return $"Wave {waveNumber}";
    }

    // ── Internal ──────────────────────────────────────────────────────────

    void Awake()
    {
        if (_announcementLabel != null)
            _announcementLabel.alpha = 0f;
    }

    void ShowAnnouncement(string text, float duration)
    {
        if (_announcementLabel == null) return;

        DOTween.Kill(_announcementLabel);
        _announcementLabel.text  = text;
        _announcementLabel.alpha = 0f;

        _announcementLabel.DOFade(1f, 0.3f)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _announcementLabel.DOFade(0f, 0.5f)
                    .SetDelay(duration)
                    .SetUpdate(true);
            });
    }
}
