using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

static class TutorTests
{
    public static void Run()
    {
        using var privateMessage = JsonDocument.Parse("{\"message\":{\"chat\":{\"id\":123,\"type\":\"private\"},\"from\":{\"id\":123,\"is_bot\":false},\"text\":\"ders\"}}");
        Check(Tutor.Allowed(privateMessage.RootElement, 123), "Öğrencinin kendi mesajı kabul edilir");
        Check(!Tutor.Allowed(privateMessage.RootElement, 456), "Başka kullanıcı reddedilir");
        using var group = JsonDocument.Parse("{\"message\":{\"chat\":{\"id\":123,\"type\":\"group\"},\"from\":{\"id\":123,\"is_bot\":false}}}");
        Check(!Tutor.Allowed(group.RootElement, 123), "Grup mesajları reddedilir");
        var key = RandomNumberGenerator.GetBytes(32);
        var plain = Encoding.UTF8.GetBytes("Özel ders cevabı");
        var encrypted = Tutor.Encrypt(plain, key);
        Check(Tutor.Decrypt(encrypted, key).SequenceEqual(plain), "Şifreli geçmiş geri okunur");
        encrypted[^1] ^= 1;
        try { Tutor.Decrypt(encrypted, key); throw new Exception("Bozuk şifreli geçmiş kabul edildi"); }
        catch (CryptographicException) { Console.WriteLine("OK: Değiştirilmiş şifreli geçmiş reddedildi"); }
        Check(Tutor.ExtractReply("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"gizli düşünce\",\"thought\":true},{\"text\":\"Merhaba\"}]}}]}") == "Merhaba", "Yalnızca görünen model cevabı gönderilir");
    }
    static void Check(bool ok, string label) { if (!ok) throw new Exception(label); Console.WriteLine("OK: " + label); }
}
