# Teknoloji Radarım

C# / .NET 10 ile yazılmış kişisel Telegram haber takipçisi.

## Kullanım

- `dotnet run -- --self-test`: RSS, Atom, güvenli XML, tarih, tekrar ve Telegram mesaj boyutu testleri. Ağa veya Telegram'a erişmez.
- `dotnet run`: resmî akışlardan son 7 günün haberlerini önizler; mesaj göndermez.
- `dotnet run -- --send`: her kaynaktan en fazla 3, toplam en fazla 12 yeni haber seçip toplu Telegram mesajları gönderir.

Kaynaklar: Microsoft .NET Blog, Python Blog, Hugging Face Blog, GitHub Changelog. Bir kaynak okunamazsa diğerleri devam eder ve Telegram'da uyarı gönderilir; tamamı başarısızsa işlem hata verir.

## Otomatik çalışma

GitHub Actions `news.yml`, her gün 06.15 UTC (Türkiye 09.15) ve elle tetiklendiğinde çalışır. Standart `ubuntu-latest` çalıştırıcısı kullanılır; harici paket veya ücretli API yoktur. GitHub zamanlanmış görevleri geciktirebilir; açık depolarda uzun süre etkinlik olmazsa zamanlamayı durdurabilir.

Gerekli GitHub Actions Secrets: `TELEGRAM_BOT_TOKEN`, `TELEGRAM_CHAT_ID`. Yerelde bunlar `.secrets/token` ve `.secrets/chat-id` dosyalarından da okunur. Gizli dosyalar Git'e eklenmez. Sohbet alıcısı kullanıcı tarafından başlatılmış özel sohbete sabitlenir; başkaları botu başlatarak alıcıyı değiştiremez.

Gönderilen bağlantıların SHA-256 değerleri `state/seen.json` içinde saklanır; kişisel sohbet verisi saklanmaz. Başarılı gönderimden sonra durum kaydedilir. Telegram gönderimi ile GitHub'a durum kaydı tek işlem olmadığından, durum kaydının başarısız olduğu olağan dışı durumlarda tekrar gönderim olabilir. Gönderim hataları otomatik tekrar denenmez.

Durdurmak için GitHub Actions ekranında günlük iş akışını devre dışı bırakın. Yerel `--send` ile bulut görevini aynı anda çalıştırmayın; yerel ve bulut durum dosyaları ayrıdır.

## Sınırlar

Kategori ve arayüz metinleri Türkçe, haber başlıkları kaynak dilindedir. Yapay zekâ özeti ve çeviri henüz yoktur. Anlık önemli haber uyarısı ve proje dosyalarını düzenleme özellikleri henüz yoktur. Bu bot bu Codex sohbetine bağlı değildir; ders yanıtları Gemini tarafından üretilir.

## Doğrulama

Yerel öz testler ve gerçek kaynak önizlemesi başarılıdır. İlk sürümde C# üzerinden gönderilen bağlantı mesajı Telegram sohbetinde doğrulandı. Günlük görev bulutta ayrıca çalıştırılarak doğrulanmalıdır.

## Resmî kaynaklar

- https://core.telegram.org/bots/api#sendmessage
- https://docs.github.com/en/actions/reference/workflows-and-actions/events-that-trigger-workflows#schedule
- https://docs.github.com/en/billing/concepts/product-billing/github-actions
- https://devblogs.microsoft.com/dotnet/feed/
- https://blog.python.org/feeds/posts/default
- https://huggingface.co/blog/feed.xml
- https://github.blog/changelog/feed/

## Telegram C# öğretmeni

`--tutor` modu, yalnızca `TELEGRAM_CHAT_ID` ile sabitlenen özel sohbetin aynı kullanıcıdan gelen metin mesajlarını yanıtlar. Diğer kullanıcılar ve gruplar Gemini'ye gönderilmez. Kullanıcı kodu çalıştırılmaz; yapay zekâya terminal, dosya veya araç erişimi verilmez.

- `/ders`: sıfırdan ilk derse başlar veya ders bağlamına göre devam eder.
- `/yardim`: kullanım bilgisi.
- `/sifirla`: aktif ders bağlamını sıfırlar.
- Normal metin ve kod mesajları doğrudan öğretmene iletilir.

Gizli ayarlar: `GEMINI_API_KEY`, `TUTOR_STATE_KEY` (rastgele 32 baytlık Base64 şifreleme anahtarı) ve mevcut Telegram ayarları. Anahtarlar kaynak koduna veya günlüklere yazılmaz. Sabit model `gemini-3.5-flash-lite`; ücretli modele otomatik geçiş, ücretli araç veya faturalandırma açma yoktur. Google projesi ücretsiz katmanda tutulmalıdır; uygulama projenin faturalandırma durumunu API üzerinden doğrulayamaz. Kota hatasında durur, kullanıcıya günde en fazla bir servis/kota uyarısı verir, 30 dakika sonra yeniden dener. Uygulama sınırı UTC gün başına 20 model isteği ve görev başına 3 yanıttır.

`tutor.yml` her 5 dakikada bir kontrol için ayarlanmıştır. GitHub kuyruk ve zamanlama gecikmeleri nedeniyle bu bir anlık sohbet hizmeti değildir; gecikme 5 dakikayı aşabilir. Haber ve ders görevleri aynı eşzamanlılık kilidini kullanır. Boş kontroller Gemini isteği yapmaz veya yeni durum kaydı oluşturmaz. İlk kurulumda ilk ders gerçek Gemini isteğiyle üretilir.

Son 12 kullanıcı/model mesajı 7 günlük aktif ders bağlamında kullanılır; `state/tutor.enc` içinde AES-256-GCM ile şifrelenir. GitHub dosya geçmişinde eski şifreli sürümler kalır; `/sifirla` bunları veya Google'daki kayıtları silmez. Şifreleme anahtarı kaybolursa eski bağlam okunamaz. Öğrenci metinleri cevap üretmek için Google'a iletilir; ücretsiz katman içerikleri Google ürünlerini geliştirmede kullanılabilir. Anahtar veya kişisel sır paylaşılmamalıdır.

Telegram gönderimiyle GitHub durum kaydı tek bir atomik işlem değildir. GitHub durum kaydı başarısız olursa aynı yanıt tekrar gönderilebilir. Telegram bekleyen mesajları en fazla 24 saat tuttuğundan uzun kesintilerde bazı mesajların tekrar gönderilmesi gerekebilir.

Testler: `dotnet run -- --self-test` hem haber hem öğretmen erişim kontrolü, şifreleme, tahrifat reddi ve yalnızca görünür model metni ayrıştırma testlerini çalıştırır.

Resmî Gemini belgeleri:
- https://aistudio.google.com/docs/models/gemini-3.5-flash-lite
- https://aistudio.google.com/docs/pricing#gemini-3.5-flash-lite
- https://aistudio.google.com/api-reference/generate-content
