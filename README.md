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

Kategori ve arayüz metinleri Türkçe, haber başlıkları kaynak dilindedir. Yapay zekâ özeti ve çeviri henüz yoktur. Anlık önemli haber uyarısı, Telegram komutlarına cevap ve kod düzenleme özellikleri henüz yoktur. Bu bot bu Codex sohbetine bağlı değildir.

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
