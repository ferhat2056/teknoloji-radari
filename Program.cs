using System.Net.Http.Json;
using System.Text.Json;

try
{
    if (args.Contains("--self-test")) { News.SelfTest(); TutorTests.Run(); return; }
    if (args.Contains("--tutor")) { await Tutor.Run(); return; }
    var send = args.Contains("--send");
    var statePath = Environment.GetEnvironmentVariable("STATE_PATH") ?? "state/seen.json";
    var seen = File.Exists(statePath)
        ? JsonSerializer.Deserialize<HashSet<string>>(await File.ReadAllTextAsync(statePath)) ?? [] : new HashSet<string>();
    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30), MaxResponseContentBufferSize = 8_000_000 };
    client.DefaultRequestHeaders.UserAgent.ParseAdd("TeknolojiRadari/1.0");
    var all = new List<NewsItem>();
    var failures = new List<string>();
    foreach (var source in News.Sources)
    {
        try { all.AddRange(News.Parse(await client.GetStringAsync(source.Url), source)); }
        catch { failures.Add(source.Name); Console.Error.WriteLine($"Kaynak okunamadı: {source.Name}"); }
    }
    if (failures.Count == News.Sources.Length) throw new InvalidOperationException("Hiçbir haber kaynağı okunamadı.");
    var selected = News.Select(all, seen, DateTimeOffset.UtcNow);
    Console.WriteLine($"{all.Count} haber okundu; {selected.Count} yeni haber seçildi.");
    if (!send)
    {
        foreach (var item in selected) Console.WriteLine(News.Format(item) + "\n");
        return;
    }
    var token = (Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN") ?? await File.ReadAllTextAsync(".secrets/token")).Trim();
    var chat = (Environment.GetEnvironmentVariable("TELEGRAM_CHAT_ID") ?? await File.ReadAllTextAsync(".secrets/chat-id")).Trim();
    if (!long.TryParse(chat, out var chatId) || chatId <= 0) throw new InvalidOperationException("Özel sohbet alıcısı geçersiz.");
    foreach (var batch in News.Batches(selected))
    {
        await Send("📰 Teknoloji Radarım — günlük haberler\n\n" + string.Join("\n\n", batch.Select(News.Format))
            + "\n\nBaşlıklar kaynak dilindedir. Bu sürüm otomatik haber listesidir; yapay zekâ özeti içermez.");
        foreach (var item in batch) seen.Add(item.Id);
        Directory.CreateDirectory(Path.GetDirectoryName(statePath)!);
        await File.WriteAllTextAsync(statePath + ".tmp", JsonSerializer.Serialize(seen.Order().ToArray(), new JsonSerializerOptions { WriteIndented = true }));
        File.Move(statePath + ".tmp", statePath, overwrite: true);
        await Task.Delay(1100);
    }
    if (selected.Count == 0) await Send("📰 Teknoloji Radarım\nBugünkü taramada son 7 gün içinde henüz gönderilmemiş haber bulunamadı.");
    if (failures.Count > 0) await Send("⚠️ Bugünkü tarama kısmi tamamlandı. Okunamayan kaynaklar: " + string.Join(", ", failures));
    Console.WriteLine("Telegram gönderimi tamamlandı.");

    async Task Send(string text)
    {
        using var response = await client.PostAsJsonAsync($"https://api.telegram.org/bot{token}/sendMessage", new {
            chat_id = chatId, text, link_preview_options = new { is_disabled = true }
        });
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException($"Telegram gönderimi reddedildi (HTTP {(int)response.StatusCode}).");
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        if (!body.RootElement.GetProperty("ok").GetBoolean()) throw new InvalidOperationException("Telegram gönderimi başarısız.");
    }
}
catch (InvalidOperationException error) { Console.Error.WriteLine(error.Message); Environment.ExitCode = 1; }
catch { Console.Error.WriteLine("İşlem tamamlanamadı. Gizli bilgiler hata çıktısına yazdırılmadı."); Environment.ExitCode = 1; }
