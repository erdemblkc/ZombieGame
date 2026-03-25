# INFECTED — Game Concept Document
**Version:** 0.1 | **Date:** 2026-03-26 | **Team:** Erdem + Alpin

---

## Elevator Pitch
Hades'in oda-oda ilerleme ve boon sistemiyle zombie temalı 3D FPS'i birleştiren roguelike.
Her run'da farklı build kur, her ölümde kampa dön, kalıcı güçlen, daha derin ilerle.

---

## Core Fantasy
> "Ben bu salgını durduracağım — ya da bu salgın beni durduracak."

Oyuncu enfeksiyon kaynağına (Patient Zero'ya) ulaşmak için zombi dolu binaları kat kat geçer.
Her ölüm bir öğrenme fırsatı; her run farklı bir build, farklı bir strateji.

---

## Döngüler

### 30 Saniye (Moment-to-Moment)
Oda içinde zombileri öldür → hareket upgrade'leriyle manevra yap → hayatta kal

### 5 Dakika (Oda Döngüsü)
Odayı temizle → upgrade seç (2-3 boon) → bir sonraki odayı seç → tekrar

### 30-90 Dakika (Run Döngüsü)
```
HUB (Survivor Kampı)
  → Run Başlat
    → Bölüm 1 (Hastane): 3-4 oda + Boss
    → Bölüm 2 (Metro): 4-5 oda + Boss
    → Bölüm 3 (Laboratuvar): 5-6 oda + Patient Zero
  → Ölünce → HUB'a dön
  → Serum ile kalıcı upgrade al
  → Tekrar dene
```

### Günler/Haftalar (Meta Döngüsü)
Yeni boon'lar aç → Yeni kombinasyonlar keşfet → Daha yüksek Contagion seviyesi dene

---

## Hades Sistemi Karşılıkları

| Hades | INFECTED Karşılığı | Durum |
|-------|-------------------|-------|
| Boon seçimi | Upgrade kartları (UpgradeSelectionUI) | ✅ VAR |
| Boon kombinasyonları | Evolution sistemi | ✅ VAR |
| Slot sistemi | 4 UpgradeSlot | ✅ VAR |
| Ölüm koşulu | HP=0 veya Enfeksiyon=100 | ✅ VAR |
| Chamber combat | WaveManager odaları | ✅ VAR (refactor gerek) |
| Darkness/Mirror | **Serum + Survivor Günlüğü** | ❌ YAPILACAK |
| Olympian gods | **Upgrade Kaynakları** (4 kategori) | 🔧 GENİŞLETİLECEK |
| Chamber selection | **Oda Seçim UI** | ❌ YAPILACAK |
| Hub (House of Hades) | **Survivor Kampı** sahnesi | ❌ YAPILACAK |
| Boss encounters | **Bölüm Bossu** | ❌ YAPILACAK |
| Pact of Punishment | **Contagion Paktları** | ❌ YAPILACAK |
| NPC ilişkileri | **Hayatta Kalan NPCler** | ❌ YAPILACAK |
| Grasp/Rod of Murex | **Upgrade Rariteleri** | ❌ YAPILACAK |

---

## Upgrade Kaynakları (= Hades'in Tanrıları)

Her kaynak kendi rengi ve temasıyla:

### 🔴 MİLİTER (Kırmızı)
Silah güçlendirme. Hasar, ateş hızı, mermi.
*Mevcut: RapidFire, HighCaliber, SpreadShot, Piercer, Ricochet*

### 🟢 LABORATUVAR (Yeşil)
Enfeksiyonu silaha çevir. Zehirli mermi, yavaşlatma, enfeksiyon kalkanı.
*Mevcut: ArmorUpgrade + yeniler eklenecek*

### 🔵 HAYATTAKİ (Mavi)
Hareket ve parkour. Hız, dash, grapple.
*Mevcut: Jetpack, DoubleDash, Slide, WallRun, GroundSlam, Grapple, PhaseStep, MomentumSurge*

### 🟣 MUTASYON (Mor/Altın)
Evolutions — iki upgrade kombinasyonu.
*Mevcut: ShoulderBash, PredatorDrop, Cannonball, GhostDouble*

---

## Oda Tipleri

| İkon | Tip | İçerik |
|------|-----|--------|
| ⚔️ | Combat | Zombileri öldür → Upgrade seç |
| ⚠️ | Elite | Özel güçlü zombi → İkili upgrade seçimi |
| 💀 | Boss | Bölüm sonu boss → Garanti nadir upgrade |
| 🛒 | Shop | Run currency ile alım |
| 💉 | Rest | HP doldur VEYA Enfeksiyon azalt (seç birini) |
| ❓ | Event | Risk/ödül seçimi ("Bu kutuyu aç: ya iyi ya kötü") |
| 🚪 | Seçim | 2-3 kapı, her biri farklı oda tipi gösterir |

---

## Meta-Progression: Survivor Günlüğü

Run arası **Serum** harcanır (Hades'in Darkness'ı).

| Upgrade | Maliyet | Etki |
|---------|---------|------|
| Başlangıç HP +20 | 50 Serum | Her run'a daha sağlıklı başla |
| 5. Upgrade Slotu | 200 Serum | Ekstra boon taşı |
| Başlangıç Boon | 100 Serum | Run başında 1 upgrade seç |
| Serum Çarpanı +10% | 75 Serum | Her run'dan daha fazla kazan |
| Enfeksiyon Direnci | 80 Serum | Maksimum enfeksiyon +20 |
| Silah Kilidi Aç | 150 Serum | Tüfek / shotgun başlangıç seçimi |

---

## Contagion Paktları (= Hades'in Heat Sistemi)

Her aktif pakt zorluk arttırır, ödülü de artar.

| Pakt | Etki |
|------|------|
| Hızlı Sürü | Zombiler %25 hızlı |
| Kalabalık Enfeksiyon | Her odada +2 zombi |
| Dirençli Ölüler | Zombiler %30 daha fazla HP |
| Yeniden Doğuş | Öldürülen zombiler bir kez canlanır |
| Hızlı Yayılım | Enfeksiyon 2x hızlı artar |
| Seçim Yok | Rest odaları ortadan kalkar |

---

## Bölümler ve Temalar

| Bölüm | Tema | Boss |
|-------|------|------|
| 1 | Hastane (beyaz, steril, dar koridorlar) | "Hemşire" — hızlı elite zombi |
| 2 | Metro (karanlık, geniş, tren vagonu odaları) | "Sürücü" — tank zombi |
| 3 | Laboratuvar (kimyasal tehlike, enfeksiyon gazı) | "Patient Zero" — fazlı final boss |

---

## NPC'ler (Survivor Kampı)

| NPC | Rolü | Hades Karşılığı |
|-----|------|-----------------|
| **Dr. Yıldız** | Bilim insanı — Serum upgradeler satar | Nyx |
| **Çavuş Kaya** | Asker — Silah upgradeleri | Achilles |
| **Çocuk Ali** | Gizemli — Hint verir, hikayeyi iter | Hypnos |
| **Radyo** | Dünya haberleri, lore | Chaos |

Kampı NPC'lerle konuşmak küçük hikaye parçaları açar.
Her başarılı run veya ölümden sonra yeni diyalog.

---

## Upgrade Rariteleri (Hades'in Duo/Legendary gibi)

| Rariyet | Renk | Açıklama |
|---------|------|----------|
| ⬜ Common | Beyaz | Temel upgrade |
| 🟦 Uncommon | Mavi | Biraz daha güçlü versiyon |
| 🟪 Rare | Mor | Özel etki ekler |
| 🟨 Epic | Altın | Nadiren görünür, güçlü sinerji |
| ❤️ Evolution | Kırmızı | İki upgrade kombinasyonu |

---

## Oyun Pilastroları

### 1. AKIŞ
> "Her oda öncekinden farklı hissettirmelidir."
Test: Eğer iki oda aynı hissettiriyorsa, birini değiştir.

### 2. BUILD KİMLİĞİ
> "Oyuncu run'un ortasında 'ben bir hareket build'ı yapıyorum' diyebilmeli."
Test: Upgrade seçimlerinde tutarlı bir yön var mı?

### 3. ANLAMLI ÖLÜM
> "Her ölüm bir şey öğretmeli veya bir şey kazandırmalı."
Test: Oyuncu ölünce sinirli değil, meraklı hissedecek mi?

### 4. KAZANILMIŞ GÜÇ
> "Meta-progression hile gibi değil, birikim gibi hissettirmeli."
Test: Yeni oyuncu ilk run'da eğlenebiliyor mu?

### 5. ZOMBİ ENFEKSİYONU
> "Enfeksiyon sistemi risk/ödül gerilimi yaratmalı, sadece ceza değil."
Test: Yüksek enfeksiyonun avantajı var mı?

---

## Anti-Pilastro (Yapılmayacaklar)

- ❌ Açık dünya / sandbox — odak dağılır
- ❌ Multiplayer — iki geliştirici için kapsam dışı
- ❌ Gerçekçi grafik — Unity URP stylize kalacak
- ❌ Sonsuz upgrade listesi — az upgrade, derin sinerji daha iyi

---

## Geliştirme Yol Haritası

### Faz 1 — Roguelike İskeleti (Önce Bu)
1. `RoomManager` — oda sistemi + kapı geçişi
2. `RunManager` — run state, currency tracking
3. `RoomSelectionUI` — kapı önizlemeli oda seçimi
4. `HUB Scene` — Survivor Kampı sahnesi

### Faz 2 — Meta Loop
5. `MetaCurrency (Serum)` — GlobalGameState genişletme
6. `PermanentUpgradeStation` — HUB'da kalıcı upgrade
7. `UpgradeRarity` — Common/Rare/Epic renk sistemi

### Faz 3 — İçerik
8. `BossZombie` — fazlı boss dövüşü
9. `EliteZombie` — güçlü mini-boss
10. `RoomTypes` — Shop, Rest, Event odaları
11. Yeni upgrade'ler (Laboratuvar kategorisi)

### Faz 4 — Derinlik
12. `ContagionPacts` — Heat sistemi
13. NPC diyalog sistemi
14. Bölüm 2 ve 3 içeriği
15. Enfeksiyon → güç mekaniği

---

## Tasarım Kararları (Onaylandı)

### Oda Sistemi: Sahne Bazlı
Her oda ayrı Unity sahnesi. Geçiş `SceneManager.LoadScene` ile.
```
HubScene → RoomScene → RoomSelectionScene → RoomScene → ...
```

### Enfeksiyon: Risk/Ödül Mekaniği
| Seviye | Bonus | Ceza |
|--------|-------|------|
| 0–33% | Yok | Yok |
| 34–66% | +15% hasar | — |
| 67–99% | +30% hasar, +20% hız | Enfeksiyon 2x hızlı artar |
| 100% | — | Ölüm |

Enfeksiyon artık sadece tehdit değil, aktif güç kaynağı.

---

## Teknik Notlar

**Mevcut koddan yeniden kullanılacaklar:**
- `WaveManager` → `RoomCombatController` olarak refactor
- `UpgradeSelectionUI` → rariyet renkleri eklenerek genişletilir
- `EvolutionRegistry` → yeni evolution çiftleri eklenir
- `GlobalGameState` → `runCurrency`, `metaCurrency`, `currentFloor` eklenir
- `InfectionSystem` → Contagion Paktları ile entegre

**Engine:** Unity URP (mevcut)
**Hedef Platform:** PC

---

## Son Güncelleme
**Tarih:** 2026-03-26
**Güncelleyen:** Erdem + Alpin (Claude Code brainstorm)
