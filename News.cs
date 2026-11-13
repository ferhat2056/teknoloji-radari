using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;

record Source(string Name, string Url, string Why);
record NewsItem(string Title, string Url, DateTimeOffset Date, Source Source)
{
    public string Id => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Url)));
}

static class News
{
    public static readonly Source[] Sources = [
        new("C# ve .NET", "https://devblogs.microsoft.com/dotnet/feed/", "Microsoft’un .NET duyuruları, sürümleri ve kullanım örnekleri."),
        new("Python", "https://blog.python.org/feeds/posts/default", "Python ekibinin sürüm ve topluluk duyuruları."),
        new("Yapay zekâ / Hugging Face", "https://huggingface.co/blog/feed.xml", "Açık yapay zekâ modelleri ve geliştirme araçları hakkında yayınlar."),
        new("Geliştirici araçları / GitHub", "https://github.blog/changelog/feed/", "GitHub ve geliştirici araçlarındaki değişiklikler.")
    ];

    public static List<NewsItem> Parse(string xml, Source source)
    {
        using var reader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings {
            DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 8_000_000
        });
        var document = XDocument.Load(reader);
        var result = new List<NewsItem>();
        foreach (var item in document.Descendants().Where(e => e.Name.LocalName is "item" or "entry"))
        {
            string Value(string name) => item.Elements().FirstOrDefault(e => e.Name.LocalName == name)?.Value.Trim() ?? "";
            var title = WebUtility.HtmlDecode(Value("title"));
            var link = item.Elements().FirstOrDefault(e => e.Name.LocalName == "link" && (e.Attribute("rel")?.Value ?? "alternate") == "alternate");
            var url = link?.Attribute("href")?.Value ?? link?.Value ?? "";
            if (title.Length == 0 || url.Length > 1000 || !Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) || uri.Scheme != "https") continue;
            var dateText = new[] { Value("pubDate"), Value("published"), Value("updated") }.FirstOrDefault(s => s.Length > 0);
            if (!DateTimeOffset.TryParse(dateText, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var date)) continue;
            var normalized = new UriBuilder(uri) { Fragment = "" }.Uri.AbsoluteUri;
            result.Add(new(title.Length > 250 ? title[..250] + "…" : title, normalized, date, source));
        }
        return result;
    }

    public static List<NewsItem> Select(IEnumerable<NewsItem> items, ISet<string> seen, DateTimeOffset now)
        => items.Where(i => i.Date >= now.AddDays(-7) && i.Date <= now.AddMinutes(5) && !seen.Contains(i.Id))
            .DistinctBy(i => i.Id).GroupBy(i => i.Source.Name)
            .SelectMany(g => g.OrderByDescending(i => i.Date).Take(3))
            .OrderByDescending(i => i.Date).Take(12).ToList();

    public static string Format(NewsItem item) =>
        $"📌 {item.Source.Name} · {item.Date:dd.MM.yyyy}\n{item.Title}\n{item.Url}";

    public static List<List<NewsItem>> Batches(List<NewsItem> items)
    {
        var batches = new List<List<NewsItem>>();
        var batch = new List<NewsItem>();
        var length = 0;
        foreach (var item in items)
        {
            var size = Format(item).Length + 2;
            if (length + size > 3400 && batch.Count > 0) { batches.Add(batch); batch = []; length = 0; }
            batch.Add(item); length += size;
        }
        if (batch.Count > 0) batches.Add(batch);
        return batches;
    }

    public static void SelfTest()
    {
        var now = DateTimeOffset.Parse("2026-11-14T09:00:00Z");
        var source = Sources[0];
        var rss = "<rss><channel><item><title>A &amp; B</title><link>https://example.com/a#part</link><pubDate>Fri, 13 Nov 2026 10:00:00 GMT</pubDate></item><item><title>Unsafe</title><link>javascript:alert(1)</link><pubDate>Fri, 13 Nov 2026 10:00:00 GMT</pubDate></item></channel></rss>";
        var parsed = Parse(rss, source);
        Check(parsed.Count == 1 && parsed[0].Title == "A & B" && parsed[0].Url == "https://example.com/a", "RSS ve güvenli bağlantı");
        Check(Select([parsed[0], parsed[0], parsed[0] with { Date = now.AddDays(1), Url = "https://example.com/future" }, parsed[0] with { Date = now.AddDays(-8), Url = "https://example.com/old" }], new HashSet<string>(), now).Count == 1, "Tarih ve tekrar filtresi");
        Check(Select(parsed, new HashSet<string> { parsed[0].Id }, now).Count == 0, "Gönderilen haber tekrar gönderilmez");
        var atom = "<feed xmlns='http://www.w3.org/2005/Atom'><entry><title>Atom</title><link href='https://example.com/atom'/><updated>2026-11-13T10:00:00Z</updated></entry></feed>";
        Check(Parse(atom, source).Count == 1, "Atom desteği");
        var longItems = Enumerable.Range(0, 12).Select(i => parsed[0] with { Url = "https://example.com/" + new string('a', 990) + i, Title = new string('b', 250) }).ToList();
        Check(Batches(longItems).SelectMany(b => b).Count() == 12 && Batches(longItems).All(b => string.Join("\n\n", b.Select(Format)).Length <= 3400), "Telegram mesaj sınırı ve toplu gönderim");
        try { Parse("<!DOCTYPE rss [<!ENTITY x SYSTEM 'file:///etc/passwd'>]><rss>&x;</rss>", source); throw new Exception("DTD kabul edildi"); }
        catch (XmlException) { Console.WriteLine("OK: DTD reddedildi"); }
        static void Check(bool value, string name) { if (!value) throw new Exception(name); Console.WriteLine("OK: " + name); }
    }
}
