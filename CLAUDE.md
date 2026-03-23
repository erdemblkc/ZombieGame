# ZombieGame — Claude Bağlam Dosyası

> Bu dosyayı her session sonunda güncelleyin, commit edin ve push edin.
> Diğer geliştirici pull çekince Claude bu dosyayı otomatik okur.

---

## Proje Özeti

Unity ile yapılan 3D FPS zombi hayatta kalma oyunu.
İki geliştirici: **Erdem** ve **Alpin** — ayrı klasörler (`Assets/Erdem/`, `Assets/Alpin/`).
Aktif scriptler: `Assets/Erdem/2nd/Scripts/` — Legacy klasörü eski/kullanılmayan scriptler, dokunulmaz.

---

## Teknoloji

- Unity (URP)
- DOTween (UI animasyonları)
- TextMeshPro
- NavMesh (zombi AI)
- New Input System

---

## Oyun Akışı

```
Ana Menü (MainMenuScene)
  → Start → GameScene
    → Wave 1: 10 zombi spawn
    → Wave 1 bitti → WaveCompleteManager paneli açılır (zaman durur)
    → Oyuncu upgrade seçer (3 seçenek)
    → Wave 2: 10 zombi, daha güçlü (+35% hız, +60% hasar, +50% HP)
    → Wave 2 bitti → (henüz implemente edilmedi)
  → Oyuncu ölürse → GameOverManager ekranı → Restart
```

---

## Script Listesi

### Core
| Script | Görev |
|--------|-------|
| `GlobalGameState` | Static sınıf — sahne yüklemeleri arası veri tutar (`SavedWave`, `IsWeaponUpgraded`) |
| `IDamageable` | Interface — `TakeDamage(float)` |
| `IInteractable` | Interface — etkileşim sistemi için |

### Player
| Script | Görev |
|--------|-------|
| `PlayerController2` | Hareket (yürü/sprint/dash/zıpla), mouse look, ayak sesi, knockback |
| `PlayerDamageReceiver` | HP sistemi, hasar cooldown, knockback, ölüm kamerası, GameOver tetikleme |
| `InfectionSystem` | Enfeksiyon (0-100), zamanla artar + hasar alınca artar; 100'e ulaşınca ölüm |
| `PlayerInteractor` | E tuşu ile etkileşim (IInteractable) |
| `SprintBlurEffect` | Sprint sırasında radial blur efekti |

### Zombi
| Script | Görev |
|--------|-------|
| `ZombieAI_Follow` | NavMesh ile oyuncuya yaklaş |
| `ZombieHealth1` | HP, hasar al, ölüm animasyonu + çift ses (böğürme + düşme), despawn |
| `ZombieAttackDamageTimed` | Zamanlı hasar verme |
| `ZombieHitFlash` | Vurulunca materyal flaşı |
| `ZombieHitReact` | Vurulunca geri tepme animasyonu |
| `ZombieFaceFlashByMaterial` | Yüz materyal efekti |
| `ZombieHealthBarUI` | World-space HP bar |

### Silahlar
| Script | Görev |
|--------|-------|
| `GunShooter` | Ateşleme, şarjör, yenileme (R), ammo UI, eski/yeni silah geçişi |
| `WeaponStats` | Silah konfigürasyonu (hasar, ateş hızı, mermi sayısı, sesler, muzzle) |
| `Bullet` | Tek namlu mermi |
| `DoubleBullet` | Çift namlu mermi |
| `WeaponPartPickup` | Haritada bulunan silah parçaları (2 adet topla → yükselt) |
| `WeaponUpgradeManager` | Parça sayacı, tamamlanınca GunShooter'a bildir + toast göster |
| `WeaponUnlockToast` | "NEW WEAPON" bildirim animasyonu |

### Spawn & Wave
| Script | Görev |
|--------|-------|
| `ZombieSpawner1` | Wave 1 ve Wave 2 spawn, NavMesh nokta bulma, hayatta kalma takibi |
| `WaveCompleteManager` | Wave bitti paneli (DOTween fade), zaman durdur/başlat |

### UI
| Script | Görev |
|--------|-------|
| `WaveUpgradeChoices` | 3 upgrade butonu: Full Heal / Hasar +%20 / Enfeksiyon -%30 |
| `DamageVignetteUI` | Hasar alınca ekran kenarı kızarma |
| `EnergySliderUI` | Sprint enerjisi slider |
| `HitMakerUI` | Vurma göstergesi |
| `InteractPromptUI` | Etkileşim prompt'u |

### Menü
| Script | Görev |
|--------|-------|
| `MainMenuManager` | Start/Options/Quit/Ses toggle, DOTween animasyonlar |
| `GameOverManager` | Ölüm ekranı fade, Restart butonu |
| `MenuController` | Genel menü yöneticisi |
| `PausePopupController` | Pause menüsü |

### Diğer
| Script | Görev |
|--------|-------|
| `MinimapSystem` | Minimap sistemi |
| `MinimapIcon` | Minimap üzerindeki ikonlar |
| `RadialBlurFeature` | URP renderer feature (sprint blur) |
| `DoorInteract` | Kapı etkileşimi (IInteractable) |

---

## Önemli Mekanikler

### Silah Yükseltme
- Haritada 2 `WeaponPartPickup` var
- Toplantınca `WeaponUpgradeManager.CollectWeaponPart()` çağrılır
- Tamamlanınca `GunShooter.SetNewGunEnabled(true)` → yeni silaha geçiş
- `GlobalGameState.IsWeaponUpgraded = true` ile sahne yeniden yüklenince de kalıcı

### Enfeksiyon Sistemi
- Sürekli artış: `timeToMaxSeconds = 180` (3 dakika)
- Her zombie hasarında ek artış: `addOnHit = 2f`
- `AntidotePickup` ile azaltılabilir
- Wave upgrade: `InfectionSlow` seçeneği ile kazanım yavaşlatılabilir (`gainMultiplier *= 0.7`)

### Wave State Kalıcılığı
- `GlobalGameState.SavedWave` sahne yeniden başladığında doğru wave'den devam etmeyi sağlar

---

## Sahneler
- `MainMenu` (veya benzer isim) — Ana menü
- `GameScene` — Oyun sahnesi (MainMenuManager'da `gameSceneName = "GameScene"`)

---

## Bilinen Eksikler / TODO
- Wave 2 sonrası akış implemente edilmemiş
- Enfeksiyon ölümü şu an sadece `Debug.Log` — GameOver tetiklemiyor
- `InfectionSystem.DieFromInfection()` içinde `GameOverManager` çağrısı yok

---

## Son Güncelleme
**Tarih:** 2026-03-24
**Güncelleyen:** Erdem
**Değişiklik:** İlk CLAUDE.md oluşturuldu
