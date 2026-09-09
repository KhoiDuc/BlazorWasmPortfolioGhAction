using System.Globalization;
using BlazorWasmPortfolioGhAction.Utils.OnlineTools;

namespace BlazorWasmPortfolioGhAction.Utils;

/// <summary>
/// Date/timestamp parsing and conversion. Extracted from <c>Components/OnlineTools/DateConverter.razor</c>.
/// </summary>
public static class DateConvertUtils
{
    public const string InputFormatAuto = "auto";
    public const string InputFormatDateTimeString = "datetime";
    public const string InputFormatUnixSeconds = "unix-seconds";
    public const string InputFormatUnixMilliseconds = "unix-milliseconds";
    public const string InputFormatUnixMicroseconds = "unix-microseconds";
    public const string InputFormatDotNetTicks = "dotnet-ticks";
    public const string InputFormatMongoObjectId = "mongo-objectid";

    public static readonly string[] StandardFormats = ["d", "D", "f", "F", "g", "G", "m", "M", "o", "O", "r", "R", "s", "t", "T", "u", "U", "y", "Y", "l"];

    public static readonly string[] ExactDateFormats =
    [
        "yyyyMMdd",
        "yyyyMMddHHmmss",
        "yyyy-MM-dd",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-dd HH:mm:ss.FFFFFFF",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-ddTHH:mm:ss.FFFFFFFK",
        "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'",
        "ddd, dd MMM yyyy HH':'mm':'ss 'UTC'",
        "r",
        "R",
        "o",
        "O",
        "s",
        "u",
    ];

    public static string NormalizeInputFormat(string? value) => value switch
    {
        InputFormatDateTimeString => InputFormatDateTimeString,
        InputFormatUnixSeconds => InputFormatUnixSeconds,
        InputFormatUnixMilliseconds => InputFormatUnixMilliseconds,
        InputFormatUnixMicroseconds => InputFormatUnixMicroseconds,
        InputFormatDotNetTicks => InputFormatDotNetTicks,
        InputFormatMongoObjectId => InputFormatMongoObjectId,
        _ => InputFormatAuto,
    };

    public static string GetInputFormatDisplayName(string value) => value switch
    {
        InputFormatDateTimeString => "Date/time string",
        InputFormatUnixSeconds => "Unix timestamp (seconds)",
        InputFormatUnixMilliseconds => "Unix timestamp (milliseconds)",
        InputFormatUnixMicroseconds => "Unix timestamp (microseconds)",
        InputFormatDotNetTicks => "Timestamp (.NET ticks)",
        InputFormatMongoObjectId => "MongoDB ObjectId",
        _ => "Auto",
    };

    public static bool TryParseInput(string input, string inputFormat, out DateTimeOffset value, out string source)
    {
        value = default;
        source = string.Empty;

        if (inputFormat == InputFormatAuto || inputFormat == InputFormatMongoObjectId)
        {
            if (TryParseMongoObjectId(input, out value))
            {
                source = "MongoDB ObjectId";
                return true;
            }

            if (inputFormat == InputFormatMongoObjectId)
                return false;
        }

        if (inputFormat == InputFormatAuto || inputFormat == InputFormatDateTimeString)
        {
            if (TryParseDateTimeString(input, out value, out source))
                return true;

            if (inputFormat == InputFormatDateTimeString)
                return false;
        }

        if (inputFormat == InputFormatAuto || inputFormat == InputFormatUnixSeconds)
        {
            if (TryParseAsUnixSeconds(input, out value))
            {
                source = "Unix timestamp (seconds)";
                return true;
            }

            if (inputFormat == InputFormatUnixSeconds)
                return false;
        }

        if (inputFormat == InputFormatAuto || inputFormat == InputFormatUnixMilliseconds)
        {
            if (TryParseAsUnixMilliseconds(input, out value))
            {
                source = "Unix timestamp (milliseconds)";
                return true;
            }

            if (inputFormat == InputFormatUnixMilliseconds)
                return false;
        }

        if (inputFormat == InputFormatAuto || inputFormat == InputFormatUnixMicroseconds)
        {
            if (TryParseAsUnixMicroseconds(input, out value))
            {
                source = "Unix timestamp (microseconds)";
                return true;
            }

            if (inputFormat == InputFormatUnixMicroseconds)
                return false;
        }

        if (inputFormat == InputFormatAuto || inputFormat == InputFormatDotNetTicks)
        {
            if (TryParseAsDotNetTicks(input, out value))
            {
                source = "Timestamp (.NET ticks)";
                return true;
            }

            if (inputFormat == InputFormatDotNetTicks)
                return false;
        }

        if (inputFormat == InputFormatAuto && TryParseNumericTimestamp(input, out value, out source))
            return true;

        return false;
    }

    public static bool TryParseDateTimeString(string input, out DateTimeOffset value, out string source)
    {
        if (DateTimeOffset.TryParse(input, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal, out value))
        {
            source = "Date/time string (current culture)";
            return true;
        }

        if (DateTimeOffset.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal, out value))
        {
            source = "Date/time string (invariant culture)";
            return true;
        }

        if (DateTimeOffset.TryParseExact(input, ExactDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal, out value))
        {
            source = "Date/time string (exact format)";
            return true;
        }

        source = string.Empty;
        return false;
    }

    public static bool TryParseAsDotNetTicks(string input, out DateTimeOffset value)
    {
        value = default;
        if (!long.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
            return false;

        return TryCreateFromDotNetTicks(number, out value);
    }

    public static bool TryParseAsUnixSeconds(string input, out DateTimeOffset value)
    {
        value = default;
        if (!long.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
            return false;

        return TryCreateFromUnixSeconds(number, out value);
    }

    public static bool TryParseAsUnixMilliseconds(string input, out DateTimeOffset value)
    {
        value = default;
        if (!long.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
            return false;

        return TryCreateFromUnixMilliseconds(number, out value);
    }

    public static bool TryParseAsUnixMicroseconds(string input, out DateTimeOffset value)
    {
        value = default;
        if (!long.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
            return false;

        return TryCreateFromUnixMicroseconds(number, out value);
    }

    public static bool TryParseMongoObjectId(string input, out DateTimeOffset value)
    {
        value = default;
        if (input.Length != 24 || !input.All(IsHexCharacter))
            return false;

        if (!uint.TryParse(input[..8], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var unixSeconds))
            return false;

        try
        {
            value = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsHexCharacter(char c) =>
        (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');

    public static bool TryParseNumericTimestamp(string input, out DateTimeOffset value, out string source)
    {
        value = default;
        source = string.Empty;

        if (!long.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
            return false;

        var digitsCount = input.TrimStart('-', '+').Length;

        if (digitsCount <= 11 && TryCreateFromUnixSeconds(number, out value))
        {
            source = "Unix timestamp (seconds)";
            return true;
        }

        if (digitsCount <= 14 && TryCreateFromUnixMilliseconds(number, out value))
        {
            source = "Unix timestamp (milliseconds)";
            return true;
        }

        if (digitsCount <= 17 && TryCreateFromUnixMicroseconds(number, out value))
        {
            source = "Unix timestamp (microseconds)";
            return true;
        }

        if (TryCreateFromDotNetTicks(number, out value))
        {
            source = "Timestamp (.NET ticks)";
            return true;
        }

        if (TryCreateFromUnixSeconds(number, out value))
        {
            source = "Unix timestamp (seconds)";
            return true;
        }

        if (TryCreateFromUnixMilliseconds(number, out value))
        {
            source = "Unix timestamp (milliseconds)";
            return true;
        }

        if (TryCreateFromUnixMicroseconds(number, out value))
        {
            source = "Unix timestamp (microseconds)";
            return true;
        }

        return false;
    }

    public static bool TryCreateFromDotNetTicks(long ticks, out DateTimeOffset value)
    {
        value = default;
        if (ticks < DateTimeOffset.MinValue.Ticks || ticks > DateTimeOffset.MaxValue.Ticks)
            return false;

        try
        {
            value = new DateTimeOffset(ticks, TimeSpan.Zero);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool TryCreateFromUnixSeconds(long seconds, out DateTimeOffset value)
    {
        value = default;
        try
        {
            value = DateTimeOffset.FromUnixTimeSeconds(seconds);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool TryCreateFromUnixMilliseconds(long milliseconds, out DateTimeOffset value)
    {
        value = default;
        try
        {
            value = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool TryCreateFromUnixMicroseconds(long microseconds, out DateTimeOffset value)
    {
        value = default;
        try
        {
            var milliseconds = microseconds / 1_000;
            var extraMicroseconds = microseconds % 1_000;
            value = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).AddTicks(extraMicroseconds * 10);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static IReadOnlyList<ResultValue> BuildDisplayValues(DateTimeOffset parsed)
    {
        List<ResultValue> result = [];

        result.Add(new ResultValue("UTC", parsed.UtcDateTime.ToString("o", CultureInfo.InvariantCulture)));
        result.Add(new ResultValue("Local", parsed.LocalDateTime.ToString("o", CultureInfo.InvariantCulture)));

        foreach (var format in StandardFormats)
            result.Add(new ResultValue($".NET format '{format}'", FormatUsingStandardFormat(parsed, format)));

        result.Add(new ResultValue("Unix Timestamp", parsed.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)));
        result.Add(new ResultValue("Timestamp (.NET ticks)", parsed.UtcDateTime.Ticks.ToString(CultureInfo.InvariantCulture)));
        result.Add(new ResultValue("Excel date/time", parsed.UtcDateTime.ToOADate().ToString("G17", CultureInfo.InvariantCulture)));

        return result;
    }

    public static string FormatUsingStandardFormat(DateTimeOffset value, string format)
    {
        if (format == "U")
            return value.UtcDateTime.ToString("U", CultureInfo.InvariantCulture);

        if (format == "l")
            return value.UtcDateTime.ToString("r", CultureInfo.InvariantCulture).ToLowerInvariant();

        try
        {
            return value.ToString(format, CultureInfo.InvariantCulture);
        }
        catch (FormatException ex)
        {
            return ex.Message;
        }
    }
}