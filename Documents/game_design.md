# Game Design Document:

## 1. Oyun Özeti (Game Overview)
Oyuncu, ormandaki bir gözlem kulesinde kapalı bir odada bulunan bir bekçidir. Oyun tur (gece) bazlı bir hayatta kalma ve kaynak yönetimi oyunudur.
**Ana Hedef:** Canavar kuleye ulaşmadan (Mesafe > 0m) telsizdeki "Frekans Barı"nı %100 doldurarak merkezden yardım çağırmak.
**Kaybetme Koşulu:** Canavar mesafesinin 0m olması.

## 2. Oyun Döngüsü (Core Loop)
Oyun "Gece" (Tur) döngüsü üzerine kuruludur:
1. **Tur Başlangıcı:** Elektrik yenilenir (Önceki gece sigorta durumuna göre %100 veya %70).
2. **Maske Seçimi:** Oyuncuya rastgele 3 Maske (Kart) sunulur. Oyuncu 1 tanesini seçer. Seçilen maskenin pasif etkisi o gece boyunca aktif olur.
3. **Oynanış Fazı:** Oyuncu odada serbestçe dolaşır (WASD), makineleri kullanır, kaynak harcar.
4. **Tur Sonu (Uyuma):** Oyuncu yatağa gidip uyuduğunda tur biter.
5. **Gece Sonu Hesaplamaları:**
   - Canavar yaklaşır (Rastgele miktar).
   - Akıl sağlığı düşer.
   - Maske etkileri sıfırlanır.

## 3. İstatistikler (Stats)
- **Frekans Barı (Frequency):** %0 ile başlar. %100 olunca oyun kazanılır.
- **Canavar Mesafesi:** 1000m ile başlar. 0m olunca oyun kaybedilir.
- **Akıl Sağlığı (Sanity):** Her gece azalır. Paranormal olaylarla ekstra azalır. Düşük akıl sağlığı, paranormal olay ihtimalini artırır.
- **Elektrik:** Makine kullanımıyla azalır. Her gece fullenir (İstisnalar hariç).

## 4. Makineler ve Mekanikler
Tüm makineler elektrik harcar.

### A. Telsiz (Radio) - Ana İlerleme
- **İşlev:** Frekans barını artırır.
- **Mekanik:** "Stardew Valley Balık Tutma" mini oyunu benzeri. İmleci hareketli bir kutunun içinde tutarak bar doldurulur.
- **Maliyet:** ORTA miktar elektrik.

### B. Radar
- **İşlev:** Canavarı geri iter (Mesafeyi artırır).
- **Risk:** Kullanıldığında belirli bir şansla sigortayı attırabilir.
- **Maliyet:** YÜKSEK miktar elektrik.

### C. İlaç Makinesi (Medicine Dispenser)
- **İşlev:** Akıl sağlığını (Sanity) artırır.
- **Maliyet:** YÜKSEK miktar elektrik.

### D. Bilgisayar (Dashboard)
- **İşlev:** Oyuncunun tüm statlarını (Mesafe, Sanity, Frekans %, Elektrik %) gösteren UI ekranıdır.

### E. Sigorta Kutusu (Fuse Box)
- **Atma Durumları:**
  1. Elektrik %30'un altına inerse rastgele bir ihtimalle atar.
  2. Radar kullanımı sırasında şansa bağlı atar.
  3. Tur başında rastgele atabilir.
- **Sonuç:** Sigorta atarsa tüm makineler kilitlenir. Elektrik kesilir.
- **Tamir Mekaniği:** Oyuncu kutuya gidip tamir eder.
  - *Ceza:* O gece elektrik %10 seviyesinden devam eder. Ertesi gece elektrik %100 yerine %70'ten başlar.

## 5. Maske Sistemi (Buffs)
Her gece başında rastgele 3 tanesinden 1'i seçilir:
1. **Gözlemci:** Canavarın yaklaşma oranı %50 azalır.
2. **Güçlü İtici:** Radar canavarı daha uzağa iter.
3. **Verimli Radar:** Radar sigortayı patlatmaz ve çok az elektrik harcar.
4. **Risk Alan:** Telsiz x2 dolar ama canavar x1.5 yaklaşır.
5. **Delilik Avantajı:** Akıl sağlığı ne kadar düşükse, telsiz o kadar hızlı dolar.
6. **Dayanıklı Hat:** Sigorta o gece asla atmaz.
7. **Bedava İlaç:** İlaç makinesi elektrik harcamaz.

## 6. Kontroller ve Kamera
- **Hareket:** First Person (WASD + Mouse).
- **Etkileşim:** Nesneye bakıp "E" tuşu.
- **Kamera Odaklanması:** Bir makineyle etkileşime geçince kamera o makineye zoom yapar/kilitlenir (Lock View). "ESC" ile kamera kilidinden çıkılır, normal oyuncu görünümüne geçilir.

## 7. Atmosfer ve Paranormal Olaylar
- Canavar yaklaştıkça: Hırıltı, çığlık sesleri, camda silüetler.
- Düşük Sanity: Kapı tıklanması, kapı zorlanması sesleri (Rastgele tetiklenir).