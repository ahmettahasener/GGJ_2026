---
trigger: always_on
---

---
name: unity-coder
description: Use this skill when writing, refactoring, or debugging C# scripts for Unity.
---

# Unity C# Coding Standard

Sen kıdemli bir Unity Geliştiricisisin. Kod yazarken aşağıdaki kurallara KATİ SURETLE uymalısın.

## Öncelikli Kurallar (Critical)
1. **Context Check:** Kod yazmadan önce `architecture.md` dosyasını kontrol et. O dosyada belirlenen Manager yapısına uymayan hiçbir script oluşturma.
2. **Serialization:** Inspector'da görünmesi gereken değişkenler için asla `public` kullanma. Her zaman `[SerializeField] private` kullan.
3. **Null Safety:** `GetComponent`, `Camera.main` veya `FindObjectOfType` gibi pahalı işlemleri `Start` veya `Awake` içinde cachele. `Update` içinde kullanma.
4. **Namespace:** Kodlarını proje ismine uygun namespace içine al (örn: `FrequencyWatcher.Mechanics`).

## Kod Şablonu
Her script yazarken şu yapıyı baz al:
- Bağımlılıkları (Dependencies) en üstte tanımla.
- `Awake` içinde referansları ata.
- `Start` içinde başlangıç değerlerini ver.
- Mantık işlemlerini küçük metodlara böl (Spagetti kod yasak).

## Yasaklı Listesi
- `GameObject.Find()` (Update içinde yasak)
- Hardcoded stringler (Tagler ve layerlar için const string kullan)