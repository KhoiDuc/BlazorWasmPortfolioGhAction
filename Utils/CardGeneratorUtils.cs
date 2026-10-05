namespace BlazorWasmPortfolioGhAction.Utils;

/// <summary>
/// Generate test PANs from account ranges or BIN prefixes, Luhn-valid.
/// ponytail: ceiling 19-digit PAN, IIN/BIN length 6-8; upgrade path = brand-aware length table if needed.
/// </summary>
public static class CardGeneratorUtils
{
    /// <summary>Generate one Luhn-valid PAN from a numeric prefix at target total length.</summary>
    public static string Generate(string prefix, int totalLength, Random rng)
    {
        if (totalLength < prefix.Length + 1)
            throw new ArgumentException("totalLength must exceed prefix length by at least 1 (check digit).", nameof(totalLength));
        if (!prefix.All(char.IsDigit) || prefix.Length == 0)
            throw new ArgumentException("prefix must be non-empty digits.", nameof(prefix));

        Span<char> buf = stackalloc char[totalLength];
        for (var i = 0; i < prefix.Length; i++) buf[i] = prefix[i];
        for (var i = prefix.Length; i < totalLength - 1; i++)
            buf[i] = (char)('0' + rng.Next(10));
        buf[totalLength - 1] = LuhnCheckDigit(buf[..(totalLength - 1)]);
        return new string(buf);
    }

    /// <summary>Generate N PANs from a numeric prefix.</summary>
    public static IEnumerable<string> GenerateMany(string prefix, int totalLength, int count, Random rng)
    {
        for (var i = 0; i < count; i++)
            yield return Generate(prefix, totalLength, rng);
    }

    /// <summary>Generate a PAN whose number falls within [min, max] (inclusive, same digit length).</summary>
    public static string GenerateInRange(string min, string max, Random rng)
    {
        if (min.Length != max.Length || !min.All(char.IsDigit) || !max.All(char.IsDigit))
            throw new ArgumentException("min/max must be same-length digit strings.");
        if (string.CompareOrdinal(min, max) > 0)
            (min, max) = (max, min);

        var len = min.Length;
        // Work on the payload (len-1 digits); check digit computed after.
        var lo = long.Parse(min[..^1]);
        var hi = long.Parse(max[..1]);
        var payload = lo + (long)(rng.NextDouble() * (hi - lo + 1));
        var payloadStr = payload.ToString(new string('0', len - 1));
        return payloadStr + LuhnCheckDigit(payloadStr);
    }

    /// <summary>Compute the Luhn check digit for a payload (no check digit appended).</summary>
    public static char LuhnCheckDigit(ReadOnlySpan<char> digits)
    {
        var sum = 0;
        var parity = digits.Length % 2; // double payload positions whose parity differs from payload length
        for (var i = 0; i < digits.Length; i++)
        {
            var d = digits[i] - 48;
            if (i % 2 != parity)
            {
                d *= 2;
                if (d > 9) d -= 9;
            }
            sum += d;
        }
        var check = (10 - sum % 10) % 10;
        return (char)('0' + check);
    }
}