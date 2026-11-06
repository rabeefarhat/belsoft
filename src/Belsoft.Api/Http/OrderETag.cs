using System.Diagnostics.CodeAnalysis;

namespace Belsoft.Api.Http;

// HTTP representation of Order.RowVersion: a strong ETag such as "AAAAAAAAB9E=".
public static class OrderETag
{
    private const int RowVersionLength = 8;                                            // ①

    public static string From(byte[] rowVersion) =>
        $"\"{Convert.ToBase64String(rowVersion)}\"";                                    // ②

    public static bool TryParse(string header, [NotNullWhen(true)] out byte[]? rowVersion)   // ③
    {
        rowVersion = null;
        string value = header.Trim();

        // Must be one strong ETag in quotes. Weak ("W/...") and lists ("a", "b") are refused.
        if (value.Length < 2 || value[0] != '"' || value[^1] != '"')                    // ④
        {
            return false;
        }

        byte[] buffer = new byte[RowVersionLength];
        if (!Convert.TryFromBase64String(value[1..^1], buffer, out int written)        // ⑤
            || written != RowVersionLength)
        {
            return false;
        }

        rowVersion = buffer;
        return true;
    }
}