# Game Concept: Toy Terror

*Created: 2026-03-25*
*Last Updated: 2026-03-26*
*Status: Draft*

---

## Elevator Pitch

> Bir çocuğun ateşli kabus dünyasında geçen, oda-bazlı FPS roguelike. Oyuncak
> silahlarınla zombie oyuncakları temizle, her odadan güçlen ve çocuğun zihnini
> ele geçiren karanlığı kat kat yok et.

---

## Core Identity

| Aspect | Detail |
| ---- | ---- |
| **Genre** | FPS Roguelike |
| **Platform** | PC (Steam) |
| **Target Audience** | Roguelike ve aksiyon oyuncuları, 18-35 yaş |
| **Player Count** | Single-player |
| **Session Length** | 30-60 dakika (bir run) |
| **Monetization** | Premium |
| **Estimated Scope** | Medium (6-12 ay, 2 kişilik ekip) |
| **Comparable Titles** | Hades, Returnal, DOOM Eternal |

---

## Core Fantasy

Sen bir çocuğun en sevdiği oyuncağısın — ama çocuk hasta ve kâbus görüyor.
Zihnindeki korkular diğer oyuncakları zombie'ye dönüştürdü. Tek sağlam kalan
sensin. Oyuncak silahlarınla oda oda ilerliyorsun; her düşmanı yıktığında
çocuğun zihni biraz daha iyileşiyor, kâbus biraz daha zayıflıyor.

Güç fantezisi: Sevimli görünen ama ölümcül hissettiren silahlara hakimsin.
Plastik, köpük, mermer — ama her biri gerçek hasarla vuruyorsun.

---

## Unique Hook

> "Hades gibi bir roguelike, AND ALSO tamamen FPS — ve her silah bir oyuncak."

Oyuncak estetiği (parlak renkler, plastik sesler, Lego-benzeri karakterler)
ile sert aksiyon hissi arasındaki kontrast oyunun kimliği. Silah çeşitliliği
yerine **upgrade çeşitliliği** — her run farklı bir build, farklı bir his.
FPS roguelike formatında bu estetik kombinasyonu yapan başka bir oyun yok.

---

## Player Experience Analysis (MDA Framework)

### Target Aesthetics (What the player FEELS)

| Aesthetic | Priority | How We Deliver It |
| ---- | ---- | ---- |
| **Sensation** (duyusal zevk) | 1 | Silah hissi, geri tepme, vuruş sesi, plastik estetik |
| **Challenge** (ustalık, engel aşma) | 2 | Roguelike zorluk eğrisi, boss mekanikleri |
| **Fantasy** (güç, rol) | 3 | "En sevilen oyuncak olmak" kimliği |
| **Narrative** (hikaye) | 4 | Çocuğun kâbus katmanları, boss'ların temsil ettiği korkular |
| **Discovery** (keşif) | 5 | Yeni upgrade kombinasyonları, gizli oda varyantları |
| **Expression** (ifade) | 6 | Build çeşitliliği, silah tercih özgürlüğü |

### Core Mechanics

1. **FPS Muharebe** — Nişan al, ateş et, dodge et; her silahın farklı his ve ritmi var
2. **Oda Temizleme + Upgrade Seçimi** — Her oda sonrası 3 upgrade seçeneğinden biri
3. **Silah Değiştirme** — Aktif 2 silah slotu; run içinde bulduklarınla yenile
4. **Meta-Progression** — Run dışında kalıcı unlock'lar (yeni silah tipleri, başlangıç bonusları)
5. **Boss Aşamaları** — Her Act sonu boss'u, oyunun öğrendiklerini test eden özel mekanik

---

## Core Loop

### Moment-to-Moment (30 saniye)

Odaya gir → Zombie oyuncaklar spawn ol → Nişan al, ateş et, dodge et →
Oda temizle → Upgrade veya yeni silah kazan → Bir sonraki kapıyı seç

Kritik: Silah hissi burada her şey. Plastik ve köpük görünse de her silahın
geri tepmesi, sesi ve ritmi tatmin edici olmalı.

### Short-Term (5-15 dakika)

3-5 oda temizle → Build şekillenmeye başlar → Act devam eder.
"Bir oda daha" psikolojisi: her oda kapandığında bir sonraki kapı zaten görünüyor.

### Session-Level (30-60 dakika)

Bir Act'ı başlat → Odaları temizle, upgrade topla → Act boss'unu yık ya da
öl → Run biter veya Act 2'ye geç → Sona ulaşırsa çocuk uyandı, kabus bitti.

### Long-Term Progression

- **Run içi**: Upgrade stack'i, silah kombinasyonu, build sinerjisi
- **Run dışı (meta)**: Yeni silah tipleri unlock, başlangıç bonusları, pasif yetenekler
- **Hikaye**: Her boss yenilince çocuğun kâbus hafızasından bir kesit açılır

---

## Enemy Design

### Zombie Tipleri

| Tip | Görünüm | Davranış | Counter | Öncelik |
| ---- | ---- | ---- | ---- | ---- |
| **Koşucu** | Standart zombie oyuncak | Düz koşar, yakın saldırı | Normal ateş | MVP |
| **Kalay Asker** | Teneke oyuncak asker, plastik kalkan | Yavaş, önden hasar almaz | Flanklama — arkaya geç | V2 |
| **Kurmalı** | Kurmalı anahtarlı oyuncak | Duruyor → aniden 3x hız sprint | Dururken vur, sprint'te dodge | V2 |
| **Palyaço** | Jack-in-the-box tipi | Yerden fırlar, yakın mesafe ani saldırı | Ses/işaret okuyunca geri çekil | V2 |
| **Fırlatıcı** | Boya tabancalı oyuncak | Uzaktan renkli boya topu atar, yavaşlatır | Kapan, yaklaş ve bitir | V2 |

> Not: V2 tipleri öncelik sırasıyla eklenir. Zaman yetmezse Kalay Asker + Kurmalı ile devam edilir.

### Tasarım Felsefesi

Her zombie tipi farklı bir counter gerektiriyor — oyuncu durduğunda değil,
agresif hareket ettiğinde hayatta kalıyor. DOOM felsefesi: **"Dur ve öl. Saldır ve yaşa."**

---

## Boss Design

### Boss 1 — "Teddy"

> *Çocuğun en sevdiği oyuncak ayısı. Hastalık onu devleştirip bozdu.*

**Tematik anlam:** Güvenin yok olması — en sevilen şeyin tehdit haline gelmesi.
Oyuncunun çocukla duygusal bağını en güçlü kuran an.

**Faz Tasarımı:**

| Faz | HP Eşiği | Davranış |
| ---- | ---- | ---- |
| **Faz 1** | %100-60 | Yavaş, ağır saldırılar. Pattern öğrenilebilir. |
| **Faz 2** | %60-25 | Çılgın koşu, rastgele charge saldırısı, hız artar |
| **Faz 3** | %25-0 | Yerde sürünerek (yaralı), çaresiz ama tehlikeli ani saldırılar |

**Özel:** Faz 3'te müzik değişir — üzücü, kırık bir melodi girer.
Öldürme anı sessiz geçer. Pillar 3 "The Child's Story" burada karşılanır.

---

### Boss 2 — "The Fever"

> *Hastalığın kendisi. Çocuğun zihnini tamamen ele geçirmeye çalışan karanlık.*

**Tematik anlam:** Hastalığın son evresi. Oyunun final boss'u.
Yenilince çocuk uyanır — oyun biter.

**Görünüm:** Sabit formu yok — karanlık bir gölge silueti (basit mesh + particle efektler).
Sürekli değişen yoğunluk ve boyut illüzyonu post-processing ile verilir.

**Faz Tasarımı:**

| Faz | HP Eşiği | Davranış |
| ---- | ---- | ---- |
| **Faz 1** | %100-50 | Odanın köşelerinden görünür, projektil fırlatır, ekran hafif bozulur |
| **Faz 2** | %50-25 | **3'e bölünür** — hangisi gerçek? İkisi sahte (az HP), biri asıl boss |
| **Faz 3** | %25-0 | Yeniden birleşir, oda tamamen karardı, normal zombie spawn eder, müzik bozulur |

**Teknik notlar:**
- Faz 2 bölünme: 3 ayrı prefab, 2'si hızlı ölür, 1'i asıl boss
- Ekran bozulması: URP post-processing + mevcut `RadialBlurFeature` üzerine inşa
- Zombie spawn: Mevcut `WaveManager` mantığı boss'a bağlanır
- Oda karartma: URP Global Volume intensity animasyonu

**Ölüm:** Işık geri döner, renkler normalleşir. Sessizlik. Çocuk uyandı.

---

## Game Pillars

### Pillar 1: Toys With Teeth
Her şey oyuncak görünür ama ölümcül hissettirmelidir. Estetik sevimli,
his sert. Bu kontrast oyunun kimliği ve pazarlama kancası.

*Design test*: "Bu plastik görünüyor mu? Evet. Vururken tatmin edici ses çıkarıyor mu?
Evet. O zaman doğru yoldayız."

### Pillar 2: One More Room
Her oda kısa, her oda tatmin edici. Oyuncu duraksamamalı. Tempo ve ritim
her diğer tasarım kararının önüne geçer.

*Design test*: "Bu mekanik oyunun temposunu kesiyor mu? Uzun animasyon, yükleme,
kesilme var mı? Varsa kısalt ya da kes."

### Pillar 3: The Child's Story
Mekanik ilerleme ile anlatı ilerleme aynı anda olur. Bir boss yenmek sadece
bir engeli aşmak değil, çocuğun bir korkusunu yok etmektir.

*Design test*: "Bu boss'un çocuğun hangi korkusunu temsil ettiğini bir cümlede
anlatabilir miyim? Hayır ise tasarım eksik."

### Anti-Pillars

- **NOT açık dünya veya keşif**: Oda bazlı yapı tempoyu sağlar; serbest gezme Pillar 2'yi yıkar.
- **NOT gerçekçi veya karanlık estetik**: Gritty, kan, gore yok. Oyuncak kontrastı estetiği zayıflatır.
- **NOT çok sayıda eşzamanlı silah**: Aktif 2 slot. Fazlası odağı dağıtır.
- **NOT çok oyunculu mod**: Kapsam dışında; single-player duygusal bağı güçlendirir.

---

## Inspiration and References

| Reference | What We Take From It | What We Do Differently |
| ---- | ---- | ---- |
| **Hades** | Oda bazlı roguelike, upgrade seçim anı, boss tekrarlanabilirliği | FPS perspektif, oyuncak estetiği |
| **Returnal** | FPS + roguelike kombinasyonu, ölüm-yeniden başlama akışı | Çok daha erişilebilir kapsam ve estetik |
| **DOOM Eternal** | Silah hissinin merkeze alınması, agresif oyun ödüllendirilir | Upgrade çeşitliliği silah çeşitliliği yerine |
| **Toy Story / Small Soldiers** | Oyuncak dünyasının ciddiye alınması | Horror tonu, zombie estetik |

**Oyun dışı ilhamlar**: Toy Story 3'ün duygusal tonu, küçük çocukların ateşli hastalık
kâbusları, plastik renk paletiyle noir kontrast.

---

## Technical Considerations

| Consideration | Assessment |
| ---- | ---- |
| **Engine** | Unity (URP) — mevcut kod tabanı, ekip deneyimi |
| **Art Style** | 3D Stylized — plastik materyal, parlak renkler, modüler karakter |
| **Audio** | Yüksek öncelik — plastik kırılma, silah sesi çeşitliliği, dinamik müzik |
| **Key Challenges** | Silah hissi; roguelike oda yönetimi; boss AI faz geçişleri |
| **Networking** | Yok — single-player |

---

## Risks and Open Questions

### Design Risks
- Silah hissi yetersiz kalabilir — oyuncak silahları tatmin edici hissettirmezse döngü çöker
- Upgrade çeşitliliği erken tükenir — 2-3 run sonrası tekrarlayan seçimler sıktırır
- Boss pattern'leri ezber haline gelirse yeniden oynama değeri düşer

### Technical Risks
- FPS controller + roguelike oda sistemi entegrasyonu — beklenmedik bug riski
- The Fever boss'unun "3'e bölünme" mekaniği dikkatli test gerektirir

### Scope Risks
- 2 kişilik ekip, Haziran deadline — MVP + 2 boss sıkı ama yapılabilir
- V2 zombie tipleri hepsi yetişmeyebilir — öncelik sırasıyla eklenir, olmadı eldekiyle devam

### Open Questions (Bekleyen Kararlar)
- **Adrenalin sistemi** (beklemede): Kill = yozlaşma azalır, durmak = artar.
  DOOM felsefesini tam karşılar. Kapsama sığarsa eklenir.
- Oda başına kaç düşman dengeli? → Playtestle belirlenmeli
- Meta-progression derinliği: Hades seviyesi mi, Dead Cells sadeliği mi?

---

## MVP Definition

**Core hypothesis**: "FPS + oyuncak teması + oda bazlı roguelike birlikte eğlenceli mi?"

**MVP için gerekli:**
1. Çalışan FPS controller (hareket, nişan, ateş, dodge)
2. 1 Act — 5-8 oda şablonu, prosedürel sıra
3. 2 silah tipi
4. 10 upgrade seçeneği
5. 2 düşman tipi (Koşucu + 1 V2 tipi)
6. Teddy boss savaşı
7. Basit meta-progression

**MVP dışı:**
- Hikaye/cutscene
- The Fever (Boss 2) — Act 2 ile birlikte gelir
- 3'ten fazla silah tipi
- Shop/Rest/Event özel odaları

### Scope Tiers

| Tier | İçerik | Hedef |
| ---- | ---- | ---- |
| **MVP** | 1 Act, 5-8 oda, 2 silah, Teddy | 4-6 hafta |
| **Haziran Hedefi** | 2 Act, 5 zombie tipi, Teddy + The Fever, 3 silah, 15 upgrade | Haziran 2026 |
| **Alpha** | 2 Act tam, tüm silahlar, ses draft | 6-8 ay |
| **Full Vision** | 3 Act, tüm boss'lar, full ses/müzik | 10-12 ay |

---

## Next Steps

- [ ] `/map-systems` — Sistemleri haritala
- [ ] `/design-system fps-combat` — Silah hissi ve muharebe sistemi GDD
- [ ] `/prototype fps-core` — Silah + oda MVP prototipi
- [ ] `/sprint-plan new` — Haziran hedefli sprint planı

---

## Son Güncelleme
**Tarih:** 2026-03-26
**Güncelleyen:** Erdem + Alpin (Claude Code brainstorm)
**Değişiklik:** INFECTED konseptinden Toy Terror'a pivot. Boss tasarımları (Teddy + The Fever)
ve zombie tipleri eklendi.
