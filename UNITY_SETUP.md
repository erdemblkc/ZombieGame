# Unity Kurulum — Adım Adım

---

## HIZLI KURULUM (Önerilen)

Unity'de projeyi derledikten sonra:

### Adım 1 — Sahneyi Hazırla
Unity menü çubuğundan:
```
Tools → ZombieGame → ▶ Setup ALL (UI + WaveManager + Player)
```
Bu komut tek tıkla yapar:
- Canvas altında **WaveHUD** (köşe etiketi + duyuru)
- Canvas altında **UpgradeSelectionPanel** (4 kart upgrade seçim ekranı)
- Canvas altında **UpgradeSlotHUD** (alt HUD, 4 slot)
- Sahnede **WaveManager + EnemySpawner** objesi
- Player'a **GunModifierStack, UpgradeSlotManager, EvolutionRegistry** ekler
- Tüm iç referansları otomatik bağlar

---

### Adım 2 — UpgradeCard Prefabını Oluştur
```
Tools → ZombieGame → Create UpgradeCard Prefab
```
`Assets/Erdem/2nd/Prefabs/UI/UpgradeCard.prefab` oluşturulur ve
UpgradeSelectionUI'ya otomatik bağlanır.

---

### Adım 3 — ZombieSpawner1'i Kapat
- Hiyerarşide `ZombieSpawner1` component'ini seç
- Component checkbox'ını kaldır (deactivate)

> "Deactivate edilsin mi?" diyalog çıkarsa → **Evet**

---

### Adım 4 — Prefab Referanslarını Ata (ZORUNLU)
Hiyerarşide **WaveManager** objesini seç, Inspector'da:

| Alan | Ne Atanacak |
|------|-------------|
| `_wave1EnemyPrefab` | Temel zombi prefabını sürükle |
| `_wave2EnemyPrefab` | Güçlü zombi prefabını sürükle (yoksa wave1 ile aynısı) |

> Bu iki alan boş kalırsa düşman spawn olmaz ve wave hiç bitmez!

---

### Adım 5 — UpgradeData Asset'lerini Oluştur
Project penceresinde `Assets/Erdem/2nd/Data/Upgrades/` klasörü oluştur (yoksa).

Her upgrade için:
```
Sağ tıkla → Create → Upgrades → UpgradeData
```

| Asset Adı | UpgradeName | BehaviourTypeName | Category |
|-----------|-------------|-------------------|----------|
| SpreadShot | Spread Shot | SpreadShotUpgrade | Gun |
| Piercer | Piercer | PiercerUpgrade | Gun |
| RapidFire | Rapid Fire | RapidFireUpgrade | Gun |
| HighCaliber | High Caliber | HighCaliberUpgrade | Gun |
| Armor | Armor | ArmorUpgrade | Utility |
| Slide | Slide | SlideUpgrade | Movement |
| MomentumSurge | Momentum Surge | MomentumSurgeUpgrade | Movement |
| DoubleDash | Double Dash | DoubleDashUpgrade | Movement |
| Jetpack | Jetpack | JetpackUpgrade | Movement |
| Cannonball | Cannonball | CannonballEvolution | Evolution |

**Evolution için:** `Prerequisites[]` alanına gerekli upgrade asset'lerini sürükle.
Cannonball → Slide + Armor.

---

### Adım 6 — Upgrade Pool'u Bağla
Hiyerarşide **UpgradeSelectionPanel** → `UpgradeSelectionUI` component:
- `_allUpgrades` array büyüklüğünü adım 5'teki sayıya eşitle
- Her slota ilgili UpgradeData asset'ini sürükle

---

## 5 Wave Nasıl Çalışır

Adım 4'teki prefablar atandıktan sonra otomatik:

```
Wave 1 →  8 zombi │ Temel
Wave 2 → 12 zombi │ Hız +%20, HP +%25
Wave 3 → 16 zombi │ Hız +%35, Hasar +%30, HP +%50  (güçlü prefab)
Wave 4 → 20 zombi │ Hız +%50, Hasar +%50, HP +%80  (güçlü prefab)
Wave 5 → 25 zombi │ Hız +%70, Hasar +%80, HP +%120 (güçlü prefab)
Wave 6+→ Otomatik skalalar (+3 zombi/wave, istatistikler artar)
```

Kendi WaveConfig ScriptableObject'lerini de kullanabilirsin:
`Create → WaveSystem → WaveConfig` → WaveManager'ın `_waveConfigs[]` dizisine sürükle.

---

## Ammo Sistemi

Artık:
- Mermi **sonsuz** — reload hep çalışır
- HUD'da `24 / 30  ∞` formatı
- Şarjör boyutu, reload süresi değişmedi

---

## Test Kontrol Listesi

- [ ] Oyun başlayınca "WAVE 1" duyurusu çıkıyor
- [ ] 8 zombi spawn oluyor
- [ ] Tüm zombiler ölünce "WAVE 1 TAMAMLANDI" çıkıyor
- [ ] 4 upgrade kartı görünüyor ve tıklanabiliyor
- [ ] Kart seçince Wave 2 başlıyor
- [ ] Wave 5'e kadar sorunsuz geçiş
- [ ] Alt HUD'da 4 slot görünüyor, dolu olanlar isim gösteriyor
- [ ] Tüm slotlar doluysa upgrade ekranı çıkmıyor, 3 sn sonra wave başlıyor
- [ ] Ateş edince mermi bitmiyor, reload çalışıyor
- [ ] HUD'da ∞ işareti görünüyor

---

## Sorun Giderme

| Sorun | Çözüm |
|-------|-------|
| "WaveManager bulunamadı" | `Setup ALL` menüsünü tekrar çalıştır |
| Zombiler spawn olmuyor | `_wave1EnemyPrefab` boş — ata |
| Wave hiç bitmiyor | ZombieSpawner1 hâlâ aktif veya EnemySpawner eksik |
| Upgrade ekranı açılmıyor | `UpgradeSelectionUI._allUpgrades` boş |
| Kart tıklanmıyor | `Create UpgradeCard Prefab` menüsünü çalıştır |
| Player component'leri eksik | `Setup Player Components` menüsünü çalıştır |
