using System.Net.Http.Json;
using System.Text.Json;

// İlk adım: Telegram bağlantısını sınar. Günlük haber zamanlaması henüz yok.
try
{
    var token = (Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN")
        ?? await File.ReadAllTextAsync(".secrets/token")).Trim();
    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    using var updates = await Call("getUpdates", new { timeout = 0, allowed_updates = new[] { "message" } });
    var chats = updates.RootElement.GetProperty("result").EnumerateArray()
        .Where(u => u.TryGetProperty("message", out _))
        .Select(u => u.GetProperty("message"))
        .Where(m => m.GetProperty("chat").GetProperty("type").GetString() == "private"
            && m.TryGetProperty("text", out var text) && text.GetString() == "/start")
        .Select(m => m.GetProperty("chat").GetProperty("id").GetInt64())
        .Distinct().ToArray();
    if (chats.Length != 1)
        throw new InvalidOperationException("Tek bir özel /start sohbeti bulunamadı. Alıcı doğrulanmadan mesaj gönderilmedi.");
    Directory.CreateDirectory(".secrets");
    await File.WriteAllTextAsync(".secrets/chat-id", chats[0].ToString());
    if (!OperatingSystem.IsWindows())
        File.SetUnixFileMode(".secrets/chat-id", UnixFileMode.UserRead | UnixFileMode.UserWrite);
    using var sent = await Call("sendMessage", new {
        chat_id = chats[0],
        text = "✅ Teknoloji Radarım bağlantı testi başarılı!\n\nC# ile yazılan programdan bu mesaj gönderildi. Günlük haber takibi henüz etkin değil; sonraki adımda kuracağız.\n\nBu bot şimdilik bir yapay zekâ sohbeti veya kod editörü değildir."
    });
    Console.WriteLine("Telegram deneme mesajını kabul etti. Günlük zamanlama henüz etkin değil.");

    async Task<JsonDocument> Call(string method, object body)
    {
        // İstek adresi erişim anahtarı içerdiğinden hatalarda adresi yazdırmayız.
        using var response = await client.PostAsJsonAsync($"https://api.telegram.org/bot{token}/{method}", body);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Telegram isteği reddedildi (HTTP {(int)response.StatusCode}).");
        var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        if (!result.RootElement.GetProperty("ok").GetBoolean())
        {
            result.Dispose();
            throw new InvalidOperationException("Telegram işlemi başarısız oldu.");
        }
        return result;
    }
}
catch (InvalidOperationException exception)
{
    Console.Error.WriteLine(exception.Message);
    Environment.ExitCode = 1;
}
catch
{
    Console.Error.WriteLine("Bağlantı veya yerel ayar hatası. Gizli bilgiler güvenlik için yazdırılmadı.");
    Environment.ExitCode = 1;
}
