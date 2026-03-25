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
    → Wave 1: 8 zombi spawn (WaveManager)
    → Wave 1 bitti → UpgradeSelectionUI açılır (zaman durur, 4 kart)
    → Oyuncu upgrade seçer → UpgradeSlotManager'a eklenir
    → Wave 2-5: Giderek güçlenen zombiler (dahili tanımlar)
    → Wave 6+: Sonsuz mod (her wave +3 zombi, artan stats)
  → Oyuncu ölürse (HP=0 veya Enfeksiyon=100) → GameOverManager ekranı → Restart
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
- **Enfeksiyon ölümü:** `InfectionSystem.DieFromInfection()` → `PlayerDamageReceiver.ForceKill()` → `GameOverManager.ShowDeathScreen()` zinciri çalışır

### Wave State Kalıcılığı
- `GlobalGameState.SavedWave` sahne yeniden başladığında doğru wave'den devam etmeyi sağlar

---

## Sahneler
- `MainMenu` (veya benzer isim) — Ana menü
- `GameScene` — Oyun sahnesi (MainMenuManager'da `gameSceneName = "GameScene"`)

---

## MOVEMENT Upgrade Sistemi
Scriptler: `Assets/Erdem/2nd/Scripts/Upgrades/`

### Core
| Script | Görev |
|--------|-------|
| `IUpgrade` | Interface — tüm upgradeler implement eder |
| `IEvolution` | Interface — evolution behaviour'ları |
| `UpgradeData` | ScriptableObject — upgrade açıklama + `BehaviourTypeName` |
| `MovementUpgradeBase` | Abstract base class — tüm movement upgradeler extend eder |
| `UpgradeSlotManager` | 4 slot yönetimi, AddUpgrade/RemoveUpgrade |
| `EvolutionRegistry` | Kombinasyon eşleştirme ve evolution spawn |
| `PlayerMovementModifiers` | Upgrade → PlayerController2 köprüsü (flag'ler) |
| `GameEvents` | Static event hub — `OnEnemyKilled` (ZombieHealth1.Die() fire eder) |

### Movement Upgradeler
`JetpackUpgrade`, `DoubleDashUpgrade`, `SlideUpgrade`, `WallRunUpgrade`, `GroundSlamUpgrade`, `GrappleUpgrade`, `PhaseStepUpgrade`, `MomentumSurgeUpgrade`

### Evolutions (aktif)
| Evolution | Gerekli Upgradeler |
|-----------|---------------------|
| `ShoulderBashEvolution` | Jetpack + DoubleDash |
| `PredatorDropEvolution` | Grapple + GroundSlam |
| `CannonballEvolution` | Slide + Armor |
| `GhostDoubleEvolution` | PhaseStep + Armor |

`GhostDoubleEvolution`: PhaseStep yapınca orijinal konumda `DecoyDistractor` bileşenli bir sahte obje bırakır.
`DecoyDistractor` (12m yarıçap) yakındaki zombileri geçici olarak sahte hedefe yönlendirir, süre bitince eski hedeflerine dönerler.

### Input Mapping
| Tuş | Upgrade |
|-----|---------|
| E | DoubleDash |
| C (yerde, sprint) | Slide |
| C (havada) | GroundSlam |
| Q | Grapple |
| F | PhaseStep |
| Space (havada, basılı) | Jetpack |

### Kurulum
**Otomatik (önerilen):** Unity Menü → `Tools → ZombieGame → ▶ Setup ALL`
Bu komut: UI hierarchy, WaveManager, Player component'leri ve tüm UpgradeData asset'lerini oluşturur.

Sonrasında elle yapılacaklar:
1. `WaveManager` Inspector → `_wave1EnemyPrefab` ve `_wave2EnemyPrefab` ata
2. `ZombieSpawner1`'i deactivate et (çakışır)

**Manuel adımlar (gerekirse):**
1. Player GameObject'e `PlayerMovementModifiers`, `UpgradeSlotManager`, `EvolutionRegistry` ekle
2. `Tools → ZombieGame → Create All UpgradeData Assets` çalıştır → 18 asset otomatik oluşur
3. `EvolutionRegistry.Entries` dizisini Inspector'dan doldur
4. Test için `UpgradeTestUI` herhangi bir GameObject'e ekle

### PlayerController2 Değişiklikleri
- `PlayerMovementModifiers _upgradeMods` — Awake'de cache edilir
- `public void ResetVerticalVelocity()` — upgradeler gravity override öncesi çağırır
- Gravity/jump/dash/speed artık `_upgradeMods` flag'lerini kontrol eder

---

## WaveManager Sistemi
Scriptler: `Assets/Erdem/2nd/Scripts/WaveSystem/`

| Script | Görev |
|--------|-------|
| `WaveConfig` | ScriptableObject — wave verileri |
| `WaveManager` | Ana wave controller — Singleton, GameEvents.OnEnemyKilled dinler |
| `EnemySpawner` | NavMesh spawn logic |

### Kurulum
`Tools → ZombieGame → Setup WaveManager` ile otomatik oluşturulur.
- `_waveConfigs` boş bırakılırsa dahili 5 wave tanımı devreye girer (Wave 6+ sonsuz mod)
- `_wave1EnemyPrefab` zorunlu — Inspector'dan elle ata
- `ZombieSpawner1` ile çakışır, deactivate et

---

## Gun Upgrade Sistemi
Scriptler: `Assets/Erdem/2nd/Scripts/Upgrades/Gun/`

| Script | Etki |
|--------|------|
| `SpreadShotUpgrade` | Her ateşte koni şeklinde +2 mermi |
| `RapidFireUpgrade` | Ateş hızı +50%, hasar -%15 |
| `HighCaliberUpgrade` | Hasar +80%, ateş hızı -%40 |
| `PiercerUpgrade` | Mermi 3 düşmandan geçer |
| `RicochetUpgrade` | Mermi duvarlara çarpınca 2 kez yansır (reflect velocity) |
| `ArmorUpgrade` | Gelen hasar -%15 |

---

## Ammo Sistemi
- **Sınırsız mermi:** `reserveAmmo` kaldırıldı. Reload her zaman çalışır.
- **HUD:** `currentAmmo / magazineSize ∞` formatında gösterir.

---

## UI

### UpgradeSlotHUD
`Assets/Erdem/2nd/Scripts/UI/UpgradeSlotHUD.cs`
- 4 slot her zaman görünür
- `UpgradeSlotManager.OnSlotsChanged` event'ini dinler

### WaveHUD
`Assets/Erdem/2nd/Scripts/UI/WaveHUD.cs`
- Köşede kalıcı "Wave N" etiketi
- DOTween fade duyurusu

---

## Bilinen Eksikler / TODO

### ⚙️ Inspector Elle Yapılacaklar
- `WaveManager._wave1EnemyPrefab` elle atanmalı
- `EvolutionRegistry.Entries` dizisi Inspector'dan doldurulmalı

---

## 🎮 Roguelike Dönüşümü — Geliştirme Todo

Oyun **Hades tarzı roguelike FPS**'e dönüştürülüyor.
Tasarım belgesi: `design/gdd/game-concept.md`

### Faz 1 — Roguelike İskeleti ✅ TAMAMLANDI
- [x] `GlobalGameState` → `runCurrency`, `metaCurrency`, `currentFloor`, `currentRoomIndex` ekle
- [x] `RunManager.cs` — Singleton, run state yönetimi (floor, oda sırası, currency)
- [x] `RoomManager.cs` — Sahne bazlı oda geçişi, oda tipi yönetimi
- [x] `RoomSelectionUI.cs` — 2-3 kapı önizlemeli oda seçim ekranı (+ `RoomCardUI`)
- [x] `InfectionSystem.cs` → Risk/Ödül mekaniği (34%+ hasar bonus, 67%+ hız bonus + 2x kazanım)
- [x] `WaveManager` → `_maxWavesInRoom` ile oda tamamlanınca `RoomManager.OnCombatRoomComplete()` çağır
- [x] HUB Sahnesi oluştur (Survivor Kampı)
- [x] Sahne geçiş akışı: `HubScene → CombatRoom → RoomSelection → ...` (Build Settings'e sahneler eklenmeli)

### Faz 2 — Meta-Progression
- [ ] `MetaCurrencyManager.cs` — Serum toplama + kalıcı kaydetme
- [ ] `PermanentUpgradeStation.cs` — HUB'da Serum harcama UI
- [ ] `UpgradeRarity.cs` — Common/Uncommon/Rare/Epic rariyet sistemi
- [ ] `UpgradeCardUI` → rariyet renk çubuğu animasyonu

### Faz 3 — İçerik
- [ ] `BossZombie.cs` — 3 fazlı boss (HP eşiğinde faz geçişi)
- [ ] `EliteZombie.cs` — mini-boss, elite oda için
- [ ] Shop odası — run currency ile upgrade satın al
- [ ] Rest odası — HP doldur VEYA Enfeksiyon azalt (seç birini)
- [ ] Event odası — risk/ödül seçimi
- [ ] Laboratuvar kategorisi: yeni upgradeler (zehirli mermi, enfeksiyon kalkanı vb.)

### Faz 4 — Derinlik
- [ ] `ContagionPactManager.cs` — Heat sistemi (6 pakt)
- [ ] NPC diyalog sistemi (Dr. Yıldız, Çavuş Kaya, Çocuk Ali)
- [ ] Bölüm 2: Metro teması + "Sürücü" boss
- [ ] Bölüm 3: Laboratuvar + "Patient Zero" final boss

---

## Roguelike Oda Sistemi

### Sahne Akışı
```
HubScene
  → CombatRoomScene  (WaveManager çalışır)
  → RoomSelectionScene  (2-3 kapı seçimi)
  → CombatRoomScene / ShopScene / RestScene / EliteScene
  → BossScene  (bölüm sonu)
  → HubScene  (run bitti veya ölüm)
```

### Enfeksiyon Risk/Ödül
| Seviye | Bonus | Ek Ceza |
|--------|-------|---------|
| 0–33% | — | — |
| 34–66% | +15% hasar | — |
| 67–99% | +30% hasar, +20% hız | Enfeksiyon 2x hızlı artar |
| 100% | — | Ölüm |

---

## Son Güncelleme
**Tarih:** 2026-03-26
**Güncelleyen:** Erdem + Alpin (Claude Code)
**Değişiklik:**
- Piercer ve Ricochet bug'ları düzeltildi (`_lastVelocity` pre-collision snapshot)
- `DoubleBullet` artık piercingCount/ricochetCount aktarıyor
- `UpgradeSelectionUI.Awake()` bug'ı düzeltildi (SetActive race condition)
- 4 missing script silindi (UpgradeSystem, UI_Manager, UnlockPopup)
- UI Rebuild sistemi eklendi (`♻ Rebuild UI` menüsü)
- Canvas Scaler Scale With Screen Size 1920×1080 ayarlandı
- Roguelike dönüşüm planı hazırlandı (`design/gdd/game-concept.md`)
- **Faz 1 Roguelike İskeleti tamamlandı:**
  - `GlobalGameState` genişletildi (CurrentFloor, RunCurrency, MetaCurrency, RoomType enum)
  - `RunManager.cs` oluşturuldu (DontDestroyOnLoad, run/oda/currency yönetimi)
  - `RoomManager.cs` oluşturuldu (per-scene, elite multiplier, RoomComplete callback)
  - `InfectionSystem` → Risk/Ödül bonusları eklendi (DamageBonusMultiplier, SpeedBonusMultiplier)
  - `GunShooter` → enfeksiyon hasar bonusu entegre edildi
  - `PlayerController2` → enfeksiyon hız bonusu entegre edildi
  - `WaveManager` → `_maxWavesInRoom` field'ı eklendi (0=sonsuz, 1=roguelike)
  - `RoomSelectionUI.cs` + `RoomCardUI` oluşturuldu (DOTween animasyonlu 3 kart)

---

## Claude-Code-Game-Studios — Studio Kuralları

### İş Birliği Protokolü
**Kullanıcı onayı olmadan hiçbir şey yazılmaz.**
Her görev şu sırayı izler: **Soru → Seçenekler → Karar → Taslak → Onay**

- Claude bir dosyaya yazmadan önce **"Bunu [dosya yolu]'na yazayım mı?"** diye sorar
- Çok dosyalı değişiklikler için tüm değişiklik listesi onaylanır
- Kullanıcı talimatı olmadan commit atılmaz

### Kodlama Standartları
- Her zaman önce `CLAUDE.md` oku
- Değişiklik yapmadan önce ilgili scriptleri incele
- Legacy klasörüne (`Assets/Legacy/`) dokunma
- Yeni script eklerken ilgili geliştiricinin klasörünü kullan (`Assets/Erdem/` veya `Assets/Alpin/`)
- Büyük refactor öncesi mutlaka sor

### Versiyon Kontrolü
- Trunk-based development
- Her session sonunda `CLAUDE.md` güncelle, commit et, push et
- Commit mesajı formatı: `[Alan] Kısa açıklama` (ör: `[Wave] WaveManager singleton düzeltildi`)

### Agent Mimarisi
Bu proje 48 koordineli Claude Code subagent ile yönetilebilir.
Her agent belirli bir domain'e sahiptir (UI, AI, Weapon, Wave, vb.).
`/start` komutu ile yeni session başlatılabilir.