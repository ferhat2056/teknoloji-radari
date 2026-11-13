# Teknoloji Radarım

Android'de Telegram üzerinden kullanılacak kişisel teknoloji takipçisi.

## Tamamlanan ilk adım

- .NET 10 / C# konsol uygulaması Telegram Bot API bağlantısını sınar.
- Bekleyen mesajlarda yalnızca tek bir özel `/start` sohbeti varsa deneme mesajı gönderir.
- Token `.secrets/token`, alıcı `.secrets/chat-id` dosyasında tutulur. Bu klasör Git'e eklenmez.
- Token alternatif olarak `TELEGRAM_BOT_TOKEN` ortam değişkeninden okunabilir.
- Harici NuGet paketi kullanılmaz. Hata çıktılarında token veya sohbet bilgisi yazdırılmaz.

Çalıştırma: `dotnet run`. Bu komut gerçek bir Telegram deneme mesajı gönderir.

## Doğrulama

`dotnet build` başarılı. Canlı bağlantı testinde Telegram mesajı kabul etti ve mesaj sohbet ekranında doğrulandı.

## Sonraki adımlar

1. Resmî haber kaynaklarından güncel içerikleri toplamak ve tekrarları elemek.
2. Türkçe haber sunumu için ücretsiz yöntemi belirlemek. Şu anda çeviri veya yapay zekâ özeti yok.
3. GitHub Actions ücretsiz kullanım koşullarını kontrol edip Mac kapalıyken günlük çalışmayı kurmak. Zamanlama henüz yok; GitHub'a yüklenmedi.
4. Telegram'dan kod düzenleme isteğini ayrıca tasarlamak. Bu uygulama sohbet asistanı veya uzaktan komut çalıştırma aracı değildir.

## Kaynaklar

- https://core.telegram.org/bots/api#getupdates
- https://core.telegram.org/bots/api#sendmessage
- https://docs.github.com/en/actions/reference/workflows-and-actions/events-that-trigger-workflows#schedule
