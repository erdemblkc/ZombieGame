# Sprint 01 — 2026-03-26 ile 2026-04-09

## Sprint Goal
MVP core loop'u çalışır hale getir: FPS muharebe + oda sistemi + ilk boss (Teddy) oynakabilsin.

## Capacity
- Ekip: 2 kişi (Erdem + Alpin)
- Toplam gün: 14 gün (2 hafta)
- Buffer (%20): 3 gün — beklenmedik bug'lar ve playtesting
- Kullanılabilir: 11 gün

---

## Tasks

### Must Have (Critical Path)

| ID | Görev | Sahip | Est. Gün | Bağımlılık | Kabul Kriteri |
|----|-------|-------|----------|-----------|----------------|
| S01-01 | FPS Controller review — mevcut `PlayerController2` Toy Terror hissine uygun mu? Dodge/dash yeterli mi? | Erdem | 1 | — | Oyuncu sorunsuz hareket ediyor, dash hissettiriyor |
| S01-02 | Oda sistemi — `RoomManager` + sahne geçişi çalışıyor mu? Toy Terror akışına uyarla | Erdem | 2 | S01-01 | HubScene → CombatRoom → RoomSelection geçişi hatasız |
| S01-03 | İlk oyuncak zombie prefab'ı — Koşucu tipi, plastik materyal/ses placeholder | Alpin | 2 | — | Koşucu spawner'da düzgün görünüyor, hasar veriyor |
| S01-04 | Upgrade seçim UI — mevcut `UpgradeSelectionUI` Toy Terror temasına uyarla (renkler, font) | Alpin | 1 | S01-02 | Oda bitince 3 kart çıkıyor, seçim çalışıyor |
| S01-05 | Teddy boss — temel AI + 3 faz geçişi | Erdem | 3 | S01-01, S01-03 | 3 faz HP eşiğinde geçiyor, Faz 3'te müzik değişiyor |
| S01-06 | WaveManager → Toy Terror oda yapısına entegre | Erdem | 1 | S01-02 | Oda temizlenince oda "complete" tetikleniyor |

### Should Have

| ID | Görev | Sahip | Est. Gün | Bağımlılık | Kabul Kriteri |
|----|-------|-------|----------|-----------|----------------|
| S01-07 | Kalay Asker zombie — kalkan mekaniği (önden hasar almaz) | Alpin | 1.5 | S01-03 | Önden atışlar hasar vermiyor, arkadan verıyor |
| S01-08 | Plastik kırılma hit feedback — vurulunca parça efekti (particle) | Alpin | 1 | S01-03 | Her vuruşta görsel + ses feedback var |

### Nice to Have

| ID | Görev | Sahip | Est. Gün | Bağımlılık | Kabul Kriteri |
|----|-------|-------|----------|-----------|----------------|
| S01-09 | Kurmalı zombie — durup sprint mekaniği | Alpin | 1 | S01-03 | Durma → sprint pattern tutarlı ve okunabilir |
| S01-10 | Teddy ölüm anı — sessizlik efekti + üzücü müzik geçişi | Erdem | 0.5 | S01-05 | Boss öldüğünde 2 saniyelik sessizlik, sonra melodi |

---

## Carryover from Previous Sprint
*Sprint 01 — ilk sprint, carryover yok.*

---

## Risks

| Risk | Olasılık | Etki | Önlem |
|------|---------|------|-------|
| Teddy boss AI beklenenden uzun sürer | Orta | Yüksek | S01-05 için 3 gün ayrıldı; faz geçişi önce yapılır, animasyon sonra eklenir |
| Oda sistemi geçişi yeni bug üretir | Orta | Orta | Mevcut `RoomManager` var, sadece uyarlama — sıfırdan yazılmıyor |
| Plastik estetik visual istenen gibi çıkmayabilir | Düşük | Orta | Sprint 01'de placeholder materyal kabul; Sprint 02'de polish |
| 2 kişi için 11 gün sıkışık | Yüksek | Orta | S01-07 ve sonrası Sprint 02'ye taşınabilir |

---

## External Dependencies
- Toy Terror için oyuncak karakter modeli/asset'i — placeholder ile başlanacak, final asset sonra
- Plastik ses efektleri — placeholder Unity sesleri kabul (Sprint 02'de özel ses)

---

## Definition of Done

- [ ] Tüm Must Have görevler tamamlandı
- [ ] Koşucu + Teddy olan bir oda oynanabiliyor (ölüp yeniden başlanabiliyor)
- [ ] Oda bitince upgrade kartları çıkıyor ve seçim çalışıyor
- [ ] Teddy'nin 3 fazı geçiyor, ölüm anı özel
- [ ] S1/S2 bug yok (crash, kayıp state, UI kırık)
- [ ] `design/gdd/game-concept.md` sprint sonunda hâlâ güncel

---

## Sprint 02 Önizleme (2026-04-09 sonrası)

- The Fever boss başlangıcı (Faz 1-2)
- Palyaço + Fırlatıcı zombie tipleri
- Meta-progression ilk versiyonu
- Plastik ses tasarımı
- Oda çeşitlendirme (2-3 şablon varyantı)
